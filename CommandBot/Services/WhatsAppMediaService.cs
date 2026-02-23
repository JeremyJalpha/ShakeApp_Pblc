using CbTsSa_Shared.DBModels;
using CommandBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CommandBot.Services
{
    public class WhatsAppMediaService
    {
        private readonly ILogger<WhatsAppMediaService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpFactory;

        private const string GraphApiVersion = "v17.0";

        public WhatsAppMediaService(ILogger<WhatsAppMediaService> logger, IConfiguration configuration, IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpFactory = httpFactory;
        }

        /// <summary>
        /// Uploads a header sample for template creation using the Graph "resumable upload" flow.
        /// This typically returns an h_... handle which is more reliable for template submission than a /media id.
        /// </summary>
        public async Task<string?> UploadTemplateHeaderHandleAsync(
            CampaignImage campaignImage,
            CommandContext context,
            CancellationToken cancellationToken = default)
        {
            if (campaignImage == null) return null;

            // inbound handle can be a phone-number media id or previous handle; we need something we can GET metadata for.
            var inboundHandle = !string.IsNullOrWhiteSpace(campaignImage.BusinessMediaHandle)
                ? campaignImage.BusinessMediaHandle
                : campaignImage.MediaHandle;

            if (string.IsNullOrWhiteSpace(inboundHandle))
            {
                _logger.LogWarning("No inbound media handle available for template-header upload (CampaignImageId {Id}).", campaignImage.CampaignImageId);
                return null;
            }

            var accessToken = _configuration["WhatsAppBusiness:AccessToken"];
            var appId = _configuration["WhatsAppBusiness:AppId"];

            _logger.LogInformation(
                "TemplateHeaderUpload start. CampaignImageId={CampaignImageId}, InboundHandle={InboundHandle}, InboundIsH={InboundIsH}, AppIdConfigured={HasAppId}",
                campaignImage.CampaignImageId,
                PreviewHandle(inboundHandle),
                inboundHandle.StartsWith("h_", StringComparison.OrdinalIgnoreCase),
                !string.IsNullOrWhiteSpace(appId));

            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(appId))
            {
                _logger.LogWarning(
                    "TemplateHeaderUpload cannot proceed due to missing config. HasAccessToken={HasToken}, HasAppId={HasAppId}.",
                    !string.IsNullOrWhiteSpace(accessToken),
                    !string.IsNullOrWhiteSpace(appId));
                return null;
            }

            try
            {
                var client = _httpFactory.CreateClient("WhatsAppApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // 1) Metadata (download URL)
                var metaUrl = $"https://graph.facebook.com/{GraphApiVersion}/{inboundHandle}?fields=mime_type,url";
                _logger.LogInformation("TemplateHeaderUpload metadata GET: {Url}", metaUrl);

                using var metaResp = await client.GetAsync(metaUrl, cancellationToken);
                var metaBody = await metaResp.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogInformation(
                    "TemplateHeaderUpload metadata response. Status={Status} Body={Body}",
                    metaResp.StatusCode,
                    metaBody);

                if (!metaResp.IsSuccessStatusCode)
                    return null;

                using var metaDoc = JsonDocument.Parse(metaBody);
                var root = metaDoc.RootElement;
                var mime = root.TryGetProperty("mime_type", out var m) ? m.GetString() : null;
                var downloadUrl = root.TryGetProperty("url", out var u) ? u.GetString() : null;

                _logger.LogInformation(
                    "TemplateHeaderUpload metadata parsed. MimeType={MimeType}, HasDownloadUrl={HasUrl}",
                    mime ?? "<null>",
                    !string.IsNullOrWhiteSpace(downloadUrl));

                if (string.IsNullOrWhiteSpace(downloadUrl))
                    return null;

                // 2) Download bytes
                byte[] bytes;
                try
                {
                    bytes = await client.GetByteArrayAsync(downloadUrl, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "TemplateHeaderUpload failed downloading bytes. Url={Url}", downloadUrl);
                    return null;
                }

                _logger.LogInformation(
                    "TemplateHeaderUpload downloaded bytes. Length={Length} MimeType={MimeType}",
                    bytes.Length,
                    mime ?? "<null>");

                if (bytes.Length == 0)
                    return null;

                // 3) Create session (/uploads)
                var createSessionUrl = $"https://graph.facebook.com/{GraphApiVersion}/{appId}/uploads";
                _logger.LogInformation(
                    "TemplateHeaderUpload create session POST: {Url} file_length={Len} file_type={MimeType}",
                    createSessionUrl,
                    bytes.Length,
                    mime ?? "<null>");

                using var createForm = new MultipartFormDataContent();
                createForm.Add(new StringContent(bytes.Length.ToString()), "file_length");
                if (!string.IsNullOrWhiteSpace(mime))
                    createForm.Add(new StringContent(mime), "file_type");

                using var createResp = await client.PostAsync(createSessionUrl, createForm, cancellationToken);
                var createBody = await createResp.Content.ReadAsStringAsync(cancellationToken);

                // This is the key blob you asked for—log it at INFO so it shows in normal runs.
                _logger.LogInformation(
                    "TemplateHeaderUpload create session response. Status={Status} Body={Body}",
                    createResp.StatusCode,
                    createBody);

                if (!createResp.IsSuccessStatusCode)
                    return null;

                using var createDoc = JsonDocument.Parse(createBody);
                var createRoot = createDoc.RootElement;

                var handle = createRoot.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
                var uploadUrl = createRoot.TryGetProperty("upload_url", out var uuEl) ? uuEl.GetString() : null;

                _logger.LogInformation(
                    "TemplateHeaderUpload session parsed. Handle={HandlePreview}, HasUploadUrl={HasUploadUrl}",
                    PreviewHandle(handle),
                    !string.IsNullOrWhiteSpace(uploadUrl));

                if (string.IsNullOrWhiteSpace(handle))
                    return null;

                // 4) Upload bytes
                var uploadEndpoint = !string.IsNullOrWhiteSpace(uploadUrl)
                    ? uploadUrl
                    : $"https://graph.facebook.com/{GraphApiVersion}/{handle}";

                _logger.LogInformation(
                    "TemplateHeaderUpload upload POST: {Url} ContentLength={Len} MimeType={MimeType}",
                    uploadEndpoint,
                    bytes.Length,
                    mime ?? "<null>");

                using var bin = new ByteArrayContent(bytes);
                bin.Headers.ContentType = new MediaTypeHeaderValue(!string.IsNullOrWhiteSpace(mime) ? mime : "application/octet-stream");
                bin.Headers.ContentLength = bytes.Length;
                bin.Headers.TryAddWithoutValidation("file_offset", "0");

                using var uploadResp = await client.PostAsync(uploadEndpoint, bin, cancellationToken);
                var uploadBody = await uploadResp.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogInformation(
                    "TemplateHeaderUpload upload response. Status={Status} Body={Body}",
                    uploadResp.StatusCode,
                    uploadBody);

                if (!uploadResp.IsSuccessStatusCode)
                    return null;

                // Parse returned handle (defensive)
                var finalHandle = handle;
                try
                {
                    if (!string.IsNullOrWhiteSpace(uploadBody))
                    {
                        using var upDoc = JsonDocument.Parse(uploadBody);
                        var upRoot = upDoc.RootElement;
                        if (upRoot.TryGetProperty("id", out var upIdEl))
                            finalHandle = upIdEl.GetString() ?? finalHandle;
                        else if (upRoot.TryGetProperty("h", out var hEl))
                            finalHandle = hEl.GetString() ?? finalHandle;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "TemplateHeaderUpload couldn't parse upload response JSON; using session handle.");
                }

                _logger.LogInformation(
                    "TemplateHeaderUpload success. FinalHandle={HandlePreview}",
                    PreviewHandle(finalHandle));

                // Persist when possible
                if (context?.AppDbContext != null)
                {
                    try
                    {
                        campaignImage.BusinessMediaHandle = finalHandle;
                        campaignImage.UploadedDateTime = DateTime.UtcNow;
                        await context.AppDbContext.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation(
                            "TemplateHeaderUpload persisted BusinessMediaHandle for CampaignImageId {Id}.",
                            campaignImage.CampaignImageId);
                    }
                    catch (DbUpdateConcurrencyException dbEx)
                    {
                        _logger.LogWarning(dbEx, "TemplateHeaderUpload concurrency error persisting handle for CampaignImageId {Id}.", campaignImage.CampaignImageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "TemplateHeaderUpload failed persisting handle for CampaignImageId {Id}.", campaignImage.CampaignImageId);
                    }
                }

                return finalHandle;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("TemplateHeaderUpload cancelled for CampaignImageId {Id}.", campaignImage.CampaignImageId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TemplateHeaderUpload exception for CampaignImageId {Id}.", campaignImage.CampaignImageId);
                return null;
            }
        }

        private static string PreviewHandle(string? handle)
        {
            if (string.IsNullOrWhiteSpace(handle)) return "<null>";
            return handle.Length > 10 ? $"...{handle[^6..]}" : handle;
        }

        /// <summary>
        /// Re-uploads the supplied campaign image to the configured business phone_number.
        /// Returns the new business-owned media id if successful; otherwise null.
        /// The method validates propagation before returning.
        /// Supports CancellationToken and includes persistence retry on DB SaveChanges.
        /// </summary>
        public async Task<string?> ReuploadAsBusinessAsync(CampaignImage campaignImage, CommandContext context, CancellationToken cancellationToken = default)
        {
            if (campaignImage == null) return null;

            var inboundHandle = !string.IsNullOrWhiteSpace(campaignImage.BusinessMediaHandle)
                ? campaignImage.BusinessMediaHandle
                : campaignImage.MediaHandle;

            if (string.IsNullOrWhiteSpace(inboundHandle))
            {
                _logger.LogWarning("No inbound media handle available to re-upload for CampaignImageId {Id}", campaignImage.CampaignImageId);
                return null;
            }

            var accessToken = _configuration["WhatsAppBusiness:AccessToken"];
            var phoneNumberId = _configuration["WhatsAppBusiness:WhatsAppBusinessPhoneNumberId"];
            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(phoneNumberId))
            {
                _logger.LogInformation("WhatsApp re-upload credentials missing; skipping re-upload for CampaignImageId {Id}", campaignImage.CampaignImageId);
                return null;
            }

            try
            {
                // Use named client from factory for requests (configured in Program.cs)
                var client = _httpFactory.CreateClient("WhatsAppApi");
                // Ensure Authorization header per-request in case factory config changes
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // 1) Get metadata for inbound media to retrieve a download URL
                var metaUrl = $"https://graph.facebook.com/v17.0/{inboundHandle}?fields=mime_type,url";
                using var metaResp = await client.GetAsync(metaUrl, cancellationToken);
                var metaBody = await metaResp.Content.ReadAsStringAsync(cancellationToken);
                if (!metaResp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch media metadata for {MediaId}: {Status} {Body}", inboundHandle, metaResp.StatusCode, metaBody);
                    return null;
                }

                using var metaDoc = JsonDocument.Parse(metaBody);
                var root = metaDoc.RootElement;
                var mime = root.TryGetProperty("mime_type", out var m) ? m.GetString() : null;
                var downloadUrl = root.TryGetProperty("url", out var u) ? u.GetString() : null;

                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    _logger.LogWarning("Media metadata for {MediaId} did not include download url", inboundHandle);
                    return null;
                }

                // 2) Download bytes (use same client with Authorization)
                byte[] imageBytes;
                try
                {
                    imageBytes = await client.GetByteArrayAsync(downloadUrl, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to download media bytes for {MediaId} from {Url}", inboundHandle, downloadUrl);
                    return null;
                }

                // 3) Upload to business phone-number media endpoint
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent("whatsapp"), "messaging_product");

                var fileContent = new ByteArrayContent(imageBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(!string.IsNullOrWhiteSpace(mime) ? mime : "application/octet-stream");
                content.Add(fileContent, "file", "upload.jpg");

                var postClient = _httpFactory.CreateClient("WhatsAppApi");
                postClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var uploadUrl = $"https://graph.facebook.com/v17.0/{phoneNumberId}/media";
                _logger.LogInformation("Uploading media to business endpoint {UploadUrl} for CampaignImageId {Id}", uploadUrl, campaignImage.CampaignImageId);
                using var uploadResp = await postClient.PostAsync(uploadUrl, content, cancellationToken);
                var uploadBody = await uploadResp.Content.ReadAsStringAsync(cancellationToken);

                if (!uploadResp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Business media upload failed: {Status} {Body}", uploadResp.StatusCode, uploadBody);
                    return null;
                }

                using var uploadDoc = JsonDocument.Parse(uploadBody);
                if (!uploadDoc.RootElement.TryGetProperty("id", out var idEl))
                {
                    _logger.LogWarning("Upload response did not contain new media id. Body: {Body}", uploadBody);
                    return null;
                }

                var newMediaId = idEl.GetString();
                if (string.IsNullOrWhiteSpace(newMediaId))
                {
                    _logger.LogWarning("New media id is empty after upload. Response: {Body}", uploadBody);
                    return null;
                }

                // 4) Validate propagation with retries and increasing delay
                var validated = false;
                var maxAttempts = 8;
                for (int attempt = 0; attempt < maxAttempts && !cancellationToken.IsCancellationRequested; attempt++)
                {
                    var checkDelayMs = 500 * (attempt + 1); // 0.5s, 1s, 1.5s, ...
                    _logger.LogInformation("Validating uploaded business media id {NewMediaId}, attempt {Attempt}/{Max}", newMediaId, attempt + 1, maxAttempts);

                    var checkUrl = $"https://graph.facebook.com/v17.0/{newMediaId}?fields=id,messaging_product,mime_type,url";
                    using var checkRes = await postClient.GetAsync(checkUrl, cancellationToken);
                    var checkBody = await checkRes.Content.ReadAsStringAsync(cancellationToken);

                    if (checkRes.IsSuccessStatusCode)
                    {
                        using var checkDoc = JsonDocument.Parse(checkBody);
                        var rootCheck = checkDoc.RootElement;
                        var hasMp = rootCheck.TryGetProperty("messaging_product", out var mp) && string.Equals(mp.GetString(), "whatsapp", StringComparison.OrdinalIgnoreCase);
                        var hasUrl = rootCheck.TryGetProperty("url", out var _);
                        if (hasMp && hasUrl)
                        {
                            validated = true;
                            break;
                        }

                        _logger.LogInformation("Uploaded media not yet propagated: {Body}", checkBody);
                    }
                    else
                    {
                        _logger.LogInformation("Validation request returned {Status}. Body: {Body}", checkRes.StatusCode, checkBody);
                    }

                    await Task.Delay(checkDelayMs, cancellationToken);
                }

                if (!validated)
                {
                    // Observability: emit admin log/metric for repeated failures
                    _logger.LogWarning("Uploaded business media id {NewMediaId} did not validate after {Attempts} attempts for CampaignImageId {Id}.", newMediaId, maxAttempts, campaignImage.CampaignImageId);
                    return null;
                }

                // 5) Persist business media handle if DB available on context with concurrency-safe retry
                if (context?.AppDbContext != null)
                {
                    int persistAttempts = 0;
                    const int maxPersistAttempts = 3;
                    while (persistAttempts < maxPersistAttempts)
                    {
                        try
                        {
                            campaignImage.BusinessMediaHandle = newMediaId;
                            campaignImage.UploadedDateTime = DateTime.UtcNow;
                            await context.AppDbContext.SaveChangesAsync(cancellationToken);
                            break;
                        }
                        catch (DbUpdateConcurrencyException dbEx)
                        {
                            persistAttempts++;
                            _logger.LogWarning(dbEx, "Concurrency exception while persisting BusinessMediaHandle (attempt {Attempt}/{Max}).", persistAttempts, maxPersistAttempts);
                            if (persistAttempts >= maxPersistAttempts) throw;
                            await Task.Delay(200 * persistAttempts, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to persist BusinessMediaHandle {NewMediaId} to DB for CampaignImageId {Id} (attempt {Attempt}/{Max})", newMediaId, campaignImage.CampaignImageId, persistAttempts + 1, maxPersistAttempts);
                            // don't bubble - we still return newMediaId even if DB save fails
                            break;
                        }
                    }
                }

                return newMediaId;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Reupload operation cancelled for CampaignImageId {Id}", campaignImage.CampaignImageId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception while re-uploading campaign image {Id}", campaignImage.CampaignImageId);
                return null;
            }
        }

        /// <summary>
        /// Downloads media from WhatsApp using a media handle.
        /// Returns the raw image bytes if successful; otherwise null.
        /// </summary>
        public async Task<byte[]?> DownloadMediaAsync(string mediaHandle, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(mediaHandle))
            {
                _logger.LogWarning("Cannot download media: mediaHandle is null or empty");
                return null;
            }

            var accessToken = _configuration["WhatsAppBusiness:AccessToken"];
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogWarning("Cannot download media: WhatsApp access token is not configured");
                return null;
            }

            try
            {
                var client = _httpFactory.CreateClient("WhatsAppApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // 1) Get metadata for media to retrieve download URL
                var metaUrl = $"https://graph.facebook.com/{GraphApiVersion}/{mediaHandle}?fields=mime_type,url";
                _logger.LogInformation("Downloading WhatsApp media - Handle: {Handle}, MetadataUrl: {Url}",
                    PreviewHandle(mediaHandle), metaUrl);

                using var metaResp = await client.GetAsync(metaUrl, cancellationToken);
                var metaBody = await metaResp.Content.ReadAsStringAsync(cancellationToken);

                if (!metaResp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch media metadata - Handle: {Handle}, Status: {Status}, Body: {Body}",
                        PreviewHandle(mediaHandle), metaResp.StatusCode, metaBody);
                    return null;
                }

                using var metaDoc = JsonDocument.Parse(metaBody);
                var root = metaDoc.RootElement;
                var mime = root.TryGetProperty("mime_type", out var m) ? m.GetString() : null;
                var downloadUrl = root.TryGetProperty("url", out var u) ? u.GetString() : null;

                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    _logger.LogWarning("Media metadata did not contain download URL - Handle: {Handle}", PreviewHandle(mediaHandle));
                    return null;
                }

                // 2) Download the actual media bytes
                _logger.LogInformation("Downloading media bytes - Handle: {Handle}, MimeType: {MimeType}",
                    PreviewHandle(mediaHandle), mime ?? "<null>");

                var bytes = await client.GetByteArrayAsync(downloadUrl, cancellationToken);

                _logger.LogInformation("Successfully downloaded media - Handle: {Handle}, Size: {Size} bytes",
                    PreviewHandle(mediaHandle), bytes.Length);

                return bytes;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Media download cancelled - Handle: {Handle}", PreviewHandle(mediaHandle));
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading media - Handle: {Handle}", PreviewHandle(mediaHandle));
                return null;
            }
        }
    }
}