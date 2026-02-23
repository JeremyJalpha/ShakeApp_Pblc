namespace CommandBot.Clients
{
    public interface ITelegramClient
    {
        Task SendMessageAsync(long chatId, string text, CancellationToken cancellationToken = default);
        Task SendPhotoByFileIdAsync(long chatId, string fileId, string? caption, CancellationToken cancellationToken = default);
        Task<byte[]?> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default);
    }
}
