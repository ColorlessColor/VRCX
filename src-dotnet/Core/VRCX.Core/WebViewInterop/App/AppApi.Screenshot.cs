using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using VRCX.Core.ScreenshotManagement.ImageProcessing;
using VRCX.Core.ScreenshotManagement.Services;

namespace VRCX.Core.WebViewInterop.App;

public partial class AppApi
{
    [GeneratedRegex(@"\\Prints\\|\\Stickers\\|\\Emoji\\", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ScreenshotRegex();

    public string? GetExtraScreenshotData(string path, bool carouselCache)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var metadata = new JsonObject();

        if (!File.Exists(path) || !path.EndsWith(".png"))
            return null;

        var files = Directory.GetFiles(Path.GetDirectoryName(path), "*.png");

        // Add previous/next file paths to metadata so the screenshot viewer carousel can request metadata for next/previous images in directory
        if (carouselCache)
        {
            var index = Array.IndexOf(files, path);
            if (index > 0)
            {
                metadata.Add("previousFilePath", files[index - 1]);
            }

            if (index < files.Length - 1)
            {
                metadata.Add("nextFilePath", files[index + 1]);
            }
        }

        using var png = new PNGFile(path, false);
        metadata.Add("fileResolution", PNGHelper.ReadResolution(png));

        var creationDate = File.GetCreationTime(path);
        metadata.Add("creationDate", creationDate.ToString("yyyy-MM-dd HH:mm:ss"));

        var fileSizeBytes = new FileInfo(path).Length;
        metadata.Add("fileSizeBytes", fileSizeBytes.ToString());
        metadata.Add("fileName", fileName);
        metadata.Add("filePath", path);
        metadata.Add("fileSize", $"{(fileSizeBytes / 1024f / 1024f).ToString("0.00")} MB");

        return metadata.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public string? GetScreenshotMetadata(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;


        var metadata = screenshotMetadataService.GetScreenshotMetadata(path);

        if (metadata == null)
        {
            var obj = new JsonObject
            {
                { "sourceFile", path },
                { "error", "Screenshot contains no metadata." }
            };

            return obj.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        if (metadata.Error != null)
        {
            var obj = new JsonObject
            {
                { "sourceFile", path },
                { "error", metadata.Error }
            };

            return obj.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        return JsonSerializer.Serialize(metadata, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public string FindScreenshotsBySearch(string searchQuery, int searchType = 0)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var searchPath = GetVRChatPhotosLocation();
        var screenshots = screenshotMetadataService.FindScreenshots(searchQuery, searchPath,
            (ScreenshotMetadataService.ScreenshotSearchType)searchType);

        var json = new JsonArray();

        foreach (var screenshot in screenshots)
        {
            json.Add(screenshot.SourceFile);
        }

        stopwatch.Stop();

        Logger.Information("FindScreenshotsBySearch took {OperationDurationInMilliseconds}ms to complete",
            stopwatch.ElapsedMilliseconds);

        return json.ToString();
    }

    public string? GetLastScreenshot()
    {
        // Get the last screenshot taken by VRChat
        var path = GetVRChatPhotosLocation();
        if (!Directory.Exists(path))
            return null;

        // exclude folder names that contain "Prints", "Stickers" or "Emoji"
        var imageFiles = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories)
            .Where(x => !ScreenshotRegex().IsMatch(x));
        var lastScreenshot = imageFiles.OrderByDescending(Directory.GetCreationTime).FirstOrDefault();

        return lastScreenshot;
    }

    public bool DeleteScreenshotMetadata(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path) || !path.EndsWith(".png"))
            return false;

        try
        {
            screenshotMetadataService.DeleteTextMetadata(path, true);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to delete screenshot metadata for {0}", path);
            return false;
        }
    }

    public void DeleteAllScreenshotMetadata()
    {
        var path = GetVRChatPhotosLocation();
        if (!Directory.Exists(path))
            return;

        var imageFiles = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
        foreach (var file in imageFiles)
        {
            try
            {
                screenshotMetadataService.DeleteTextMetadata(file, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to delete screenshot metadata for {0}", file);
            }
        }
    }
}