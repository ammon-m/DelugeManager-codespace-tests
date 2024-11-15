// based on this StackOverflow answer:
// https://stackoverflow.com/a/46497896

namespace DelugeManager;

public static class HttpClientExtensions
{
    public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<DownloadProgressInfo> progress = null, CancellationToken cancellationToken = default)
    {
        // Get the http headers first to examine the content length
        using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var contentLength = response.Content.Headers.ContentLength;

        using var download = await response.Content.ReadAsStreamAsync(cancellationToken);

        // Ignore progress reporting when no progress reporter was 
        // passed or when the content length is unknown
        if (progress == null || !contentLength.HasValue)
        {
            await download.CopyToAsync(destination, cancellationToken);
            return;
        }

        // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
        var relativeProgress = new Progress<long>(totalBytes => progress.Report(new() {
            TotalBytes = totalBytes,
            Length = contentLength.Value
        }));

        // Use extension method to report progress while downloading
        await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
        progress.Report(new() {
            TotalBytes = contentLength.Value,
            Length = contentLength.Value
        });
    }
}

public static class StreamExtensions
{
    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(bufferSize);
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));

        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }
}
