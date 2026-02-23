namespace CommandBot.Clients
{
    public class TelegramHttpClient : ITelegramClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<TelegramHttpClient> _logger;
        private readonly string _botToken;

        public TelegramHttpClient(HttpClient http, ILogger<TelegramHttpClient> logger, Microsoft.Extensions.Options.IOptions<TelegramClientOptions> options)
        {
            _http = http;
            _logger = logger;
            _botToken = options?.Value?.BotToken ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_botToken))
                _logger.LogWarning("Telegram bot token is not configured. TelegramHttpClient will attempt calls but they will likely fail.");
        }

        public async Task SendMessageAsync(long chatId, string text, CancellationToken cancellationToken = default)
        {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new { chat_id = chatId, text };

            var res = await _http.PostAsJsonAsync(url, payload, cancellationToken);
            var body = await res.Content.ReadAsStringAsync(cancellationToken);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError("Telegram sendMessage failed: {Status} {Body}", res.StatusCode, body);
                res.EnsureSuccessStatusCode();
            }

            _logger.LogInformation("Telegram sendMessage success for {ChatId}", chatId);
        }

        public async Task SendPhotoByFileIdAsync(long chatId, string fileId, string? caption, CancellationToken cancellationToken = default)
        {
            var url = $"https://api.telegram.org/bot{_botToken}/sendPhoto";
            var payload = new { chat_id = chatId, photo = fileId, caption };

            var res = await _http.PostAsJsonAsync(url, payload, cancellationToken);
            var body = await res.Content.ReadAsStringAsync(cancellationToken);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError("Telegram sendPhoto failed: {Status} {Body}", res.StatusCode, body);
                res.EnsureSuccessStatusCode();
            }

            _logger.LogInformation("Telegram sendPhoto success for {ChatId}", chatId);
        }

        public async Task<byte[]?> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileId))
            {
                _logger.LogWarning("Cannot download Telegram file: fileId is null or empty");
                return null;
            }

            try
            {
                // 1) Get file path from Telegram
                var getFileUrl = $"https://api.telegram.org/bot{_botToken}/getFile?file_id={fileId}";
                _logger.LogInformation("Downloading Telegram file - FileId: {FileId}", fileId);

                var fileResponse = await _http.GetAsync(getFileUrl, cancellationToken);
                var fileBody = await fileResponse.Content.ReadAsStringAsync(cancellationToken);

                if (!fileResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Telegram getFile failed - FileId: {FileId}, Status: {Status}, Body: {Body}",
                        fileId, fileResponse.StatusCode, fileBody);
                    return null;
                }

                // Parse file path
                using var doc = System.Text.Json.JsonDocument.Parse(fileBody);
                var root = doc.RootElement;
                if (!root.TryGetProperty("result", out var result) ||
                    !result.TryGetProperty("file_path", out var filePathEl))
                {
                    _logger.LogWarning("Telegram getFile response missing file_path - FileId: {FileId}, Body: {Body}",
                        fileId, fileBody);
                    return null;
                }

                var filePath = filePathEl.GetString();
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    _logger.LogWarning("Telegram file_path is empty - FileId: {FileId}", fileId);
                    return null;
                }

                // 2) Download the file
                var downloadUrl = $"https://api.telegram.org/file/bot{_botToken}/{filePath}";
                _logger.LogInformation("Downloading Telegram file from: {Url}", downloadUrl);

                var bytes = await _http.GetByteArrayAsync(downloadUrl, cancellationToken);

                _logger.LogInformation("Successfully downloaded Telegram file - FileId: {FileId}, Size: {Size} bytes",
                    fileId, bytes.Length);

                return bytes;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Telegram file download cancelled - FileId: {FileId}", fileId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading Telegram file - FileId: {FileId}", fileId);
                return null;
            }
        }
    }
}
