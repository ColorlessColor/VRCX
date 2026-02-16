using System.Text.Json;
using SixLabors.ImageSharp;
using VRCX.Core.ScreenshotManagement.ImageProcessing;
using VRCX.Core.Utils;
using Image = SixLabors.ImageSharp.Image;

namespace VRCX.Core.WebViewInterop.App
{
    public partial class AppApi
    {
        public void PopulateImageHosts(string json)
        {
            var hosts = JsonSerializer.Deserialize<List<string>>(json);
            imageCacheService.PopulateImageHosts(hosts);
        }

        public async Task<string> GetImage(string url, string fileId, string version)
        {
            return await imageCacheService.GetImage(url, fileId, version);
        }

        public string ResizeImageToFitLimits(string base64data)
        {
            return Convert.ToBase64String(ResizeImageToFitLimits(Convert.FromBase64String(base64data), false));
        }

        public byte[] ResizeImageToFitLimits(byte[] imageData, bool matchingDimensions, int maxWidth = 2000,
            int maxHeight = 2000, long maxSize = 10_000_000) =>
            ImageUtils.ResizeImageToFitLimits(imageData, matchingDimensions, maxWidth, maxHeight, maxSize);

        public byte[] ResizePrintImage(byte[] imageData) => ImageUtils.ResizePrintImage(imageData);

        public async Task CropAllPrints(string ugcFolderPath)
        {
            var folder = Path.Join(GetUGCPhotoLocation(ugcFolderPath), "Prints");

            if (!Directory.Exists(folder))
            {
                return;
            }

            var files = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                await CropPrintImage(file);
            }
        }

        public async Task<bool> CropPrintImage(string path)
        {
            var tempPath = path + ".temp";
            var bytes = await File.ReadAllBytesAsync(path);
            var ms = new MemoryStream(bytes);
            var print = await Image.LoadAsync(ms);
            // validation step to ensure image is actually a print
            if (!CropPrint(ref print))
                return false;

            await print.SaveAsPngAsync(tempPath);

            var oldPngFile = new PNGFile(path, false);
            var newPngFile = new PNGFile(tempPath, true);

            // Copy all iTXt chunks to new file
            var textChunks = oldPngFile.GetChunksOfType(PNGChunkTypeFilter.iTXt);

            for (var i = 0; i < textChunks.Count; i++)
            {
                newPngFile.WriteChunk(textChunks[i]);
            }

            oldPngFile.Dispose();
            newPngFile.Dispose();

            // check if file is in use and we have permission to write
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    await using (File.Open(path, FileMode.Append, FileAccess.Write, FileShare.None))
                    {
                        break;
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    await Task.Delay(1000);
                }
            }

            try
            {
                File.Move(tempPath, path, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to replace cropped print image");
                return false;
            }

            return true;
        }

        public bool CropPrint(ref Image image) => ImageUtils.CropPrint(image);

        public async Task<string?> SavePrintToFile(string url, string ugcFolderPath, string monthFolder, string fileName)
        {
            var folder = Path.Join(GetUGCPhotoLocation(ugcFolderPath), "Prints", MakeValidFileName(monthFolder));
            Directory.CreateDirectory(folder);
            var filePath = Path.Join(folder, MakeValidFileName(fileName));
            if (File.Exists(filePath))
                return null;

            try
            {
                await imageCacheService.SaveImageToFile(url, filePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save print to file");
                return null;
            }

            return filePath;
        }

        public async Task<string?> SaveStickerToFile(string url, string ugcFolderPath, string monthFolder,
            string fileName)
        {
            var folder = Path.Join(GetUGCPhotoLocation(ugcFolderPath), "Stickers", MakeValidFileName(monthFolder));
            Directory.CreateDirectory(folder);
            var filePath = Path.Join(folder, MakeValidFileName(fileName));
            if (File.Exists(filePath))
                return null;

            try
            {
                await imageCacheService.SaveImageToFile(url, filePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save print to file");
                return null;
            }

            return filePath;
        }

        public async Task<string?> SaveEmojiToFile(string url, string ugcFolderPath, string monthFolder, string fileName)
        {
            var folder = Path.Join(GetUGCPhotoLocation(ugcFolderPath), "Emoji", MakeValidFileName(monthFolder));
            Directory.CreateDirectory(folder);
            var filePath = Path.Join(folder, MakeValidFileName(fileName));
            if (File.Exists(filePath))
                return null;

            try
            {
                await imageCacheService.SaveImageToFile(url, filePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save print to file");
                return null;
            }

            return filePath;
        }
    }
}