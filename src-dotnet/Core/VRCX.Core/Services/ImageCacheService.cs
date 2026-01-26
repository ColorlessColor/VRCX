using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;

namespace VRCX.Core.Services;

public sealed class ImageCacheService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly string _cacheLocation;
    private readonly HttpClient _httpClient;

    private readonly List<string> _imageHosts =
    [
        "api.vrchat.cloud",
        "files.vrchat.cloud",
        "d348imysud55la.cloudfront.net",
        "assets.vrchat.com"
    ];

    public ImageCacheService(WebApiService webApiService)
    {
        _cacheLocation = Path.Join(Program.AppDataDirectory, "ImageCache");
        var httpClientHandler = new HttpClientHandler();
        if (webApiService.ProxySet)
            httpClientHandler.Proxy = webApiService.Proxy;

        _httpClient = new HttpClient(httpClientHandler);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", Program.Version);
    }

    public void PopulateImageHosts(List<string> hosts)
    {
        foreach (var host in hosts)
        {
            if (string.IsNullOrEmpty(host))
                continue;

            var uri = new Uri(host);
            if (string.IsNullOrEmpty(uri.Host))
                continue;

            if (!_imageHosts.Contains(uri.Host))
                _imageHosts.Add(uri.Host);
        }
    }

    private async Task<Stream> FetchImage(string url)
    {
        var uri = new Uri(url);
        if (!_imageHosts.Contains(uri.Host))
            throw new ArgumentException("Invalid image host", url);
        
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<string> GetImage(string url, string fileId, string version)
    {
        var directoryLocation = Path.Join(_cacheLocation, fileId);
        var fileLocation = Path.Join(directoryLocation, $"{version}.png");

        if (File.Exists(fileLocation) && new FileInfo(fileLocation).Length > 0)
        {
            Directory.SetLastWriteTimeUtc(directoryLocation, DateTime.UtcNow);
            return fileLocation;
        }

        if (Directory.Exists(directoryLocation))
            Directory.Delete(directoryLocation, true);
        Directory.CreateDirectory(directoryLocation);

        try
        {
            await using var stream = await FetchImage(url);
            await using var fileStream =
                new FileStream(fileLocation, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.CopyToAsync(fileStream);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch image");
            return string.Empty;
        }

        var cacheSize = Directory.GetDirectories(_cacheLocation).Length;
        if (cacheSize > 1100)
            CleanImageCache();

        return fileLocation;
    }

    public async Task SaveImageToFile(string url, string path)
    {
        await using var stream = await FetchImage(url);
        await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream);
    }

    private void CleanImageCache()
    {
        var dirInfo = new DirectoryInfo(_cacheLocation);
        var folders = dirInfo.GetDirectories().OrderByDescending(p => p.LastWriteTime).Skip(1000);
        foreach (var folder in folders)
        {
            folder.Delete(true);
        }
    }
}