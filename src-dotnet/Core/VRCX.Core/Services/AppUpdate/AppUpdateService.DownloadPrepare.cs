using System.Buffers;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace VRCX.Core.Services.AppUpdate;

public sealed partial class AppUpdateService
{
    public double DownloadProgress { get; private set; }

    public bool IsUpdateDownloading { get; private set; }
    private CancellationTokenSource? _downloadCancellationTokenSource;

    public async ValueTask DownloadAndPrepareUpdateAsync(
        string targetVersion,
        string fileUrl,
        string hashString,
        long downloadSize
    )
    {
        if (IsUpdateDownloading)
            throw new InvalidOperationException("An update is already being downloaded.");

        _downloadCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _downloadCancellationTokenSource.Token;
        IsUpdateDownloading = true;

        try
        {
            var pathToInstaller = await DownloadUpdateAsyncCore(fileUrl, hashString, downloadSize, cancellationToken);
            await PrepareUpdateInstallationAsyncCore(targetVersion, pathToInstaller);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to download and prepare update");
            throw;
        }
        finally
        {
            IsUpdateDownloading = false;
        }
    }

    private async ValueTask<string> DownloadUpdateAsyncCore(
        string fileUrl,
        string hashString,
        long downloadSize,
        CancellationToken cancellationToken)
    {
        DownloadProgress = 0;
        cancellationToken.ThrowIfCancellationRequested();

        using var httpHandler = new SocketsHttpHandler();
        if (webApiService.ProxySet)
            httpHandler.Proxy = webApiService.Proxy;

        using var httpClient = new HttpClient(httpHandler);
        httpClient.DefaultRequestHeaders.Add("User-Agent", AppBuildInfoService.Version);

        _logger.Info("Starting download of update from {FileUrl}", fileUrl);
        var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength is { } contentLength && contentLength != downloadSize)
        {
            throw new InvalidOperationException(
                $"Download size mismatch. Expected: {downloadSize}, Actual: {contentLength}");
        }

        var tempFilePath =
            Path.Combine(Path.GetTempPath(), "vrcx_update_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ".tmp");
        try
        {
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var tempFileStream =
                File.Create(tempFilePath, 8192, FileOptions.Asynchronous | FileOptions.SequentialScan);

            using var buffer = MemoryPool<byte>.Shared.Rent(8 * 1024 * 1024); // at least 8MiB buffer
            _logger.Debug("Buffer size for download: {BufferSize} bytes ({BufferSizeInMiB} MiB)"
                , buffer.Memory.Length,
                buffer.Memory.Length / (1024 * 1024)
            );

            var totalBytesRead = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bytesRead = await contentStream.ReadAsync(buffer.Memory, cancellationToken);
                if (bytesRead == 0)
                    break;

                await tempFileStream.WriteAsync(buffer.Memory[..bytesRead], cancellationToken);

                totalBytesRead += bytesRead;
                DownloadProgress = Math.Round((double)totalBytesRead / downloadSize * 100, 2);
            }

            _logger.Info("Download completed. Verifying file integrity...");
            await VerifyFileHashAsync(tempFileStream, hashString, cancellationToken);
            _logger.Info("File integrity verified.");

            return tempFilePath;
        }
        catch
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
            throw;
        }
        finally
        {
            DownloadProgress = 0;
        }
    }

    private static async ValueTask VerifyFileHashAsync(
        FileStream fileStream,
        string hashString,
        CancellationToken cancellationToken
    )
    {
        fileStream.Seek(0, SeekOrigin.Begin);

        using var sha256 = SHA256.Create();
        var computedHash = await sha256.ComputeHashAsync(fileStream, cancellationToken);
        var computedHashString = Convert.ToHexStringLower(computedHash);

        if (!string.Equals(computedHashString, hashString, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"File hash mismatch. Expected: {hashString}, Actual: {computedHashString}");
    }

    public async ValueTask CancelUpdateDownloadAsync()
    {
        if (!IsUpdateDownloading)
        {
            _logger.Info("No update download in progress to cancel.");
            return;
        }

        if (_downloadCancellationTokenSource is null)
            throw new InvalidOperationException("Download cancellation token source is null.");

        _logger.Info("Cancelling update download...");
        await _downloadCancellationTokenSource.CancelAsync();
    }

    private async ValueTask PrepareUpdateInstallationAsyncCore(string targetVersion, string pathToInstaller)
    {
        await updateInstallationService.PrepareUpdateInstallationAsync(pathToInstaller);
        await SaveUpdateStatus(targetVersion);
    }
}