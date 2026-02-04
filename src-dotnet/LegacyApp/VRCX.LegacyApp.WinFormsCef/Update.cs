using System.Diagnostics;
using NLog;
using VRCX.LegacyApp.WinFormsCef.CoreGlue.LegacySingleton;

namespace VRCX.LegacyApp.WinFormsCef
{
    public static class Update
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient HttpClient;

        static Update()
        {
            var httpClientHandler = new HttpClientHandler();
            if (WebApi.Instance.ProxySet)
                httpClientHandler.Proxy = WebApi.Instance.Proxy;

            HttpClient = new HttpClient(httpClientHandler);
            HttpClient.DefaultRequestHeaders.Add("User-Agent", Program.Version);
        }

        public static async Task DownloadInstallRedist()
        {
            try
            {
                var filePath = await DownloadFile("https://aka.ms/vs/17/release/vc_redist.x64.exe");
                var installRedist = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = filePath,
                        Arguments = "/install /quiet /norestart"
                    }
                };
                installRedist.Start();
                await installRedist.WaitForExitAsync();
            }
            catch (Exception e)
            {
                var message = $"Failed to download and install the Visual C++ Redistributable: {e.Message}";
                Logger.Info(message);
                MessageBox.Show(message, "Update failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task<string> DownloadFile(string fileUrl, CancellationToken cancellationToken = default)
        {
            var response = await HttpClient.GetAsync(fileUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to download the file. Status code: {response.StatusCode}");

            var fileName = GetFileNameFromContentDisposition(response);
            var tempPath = Path.Join(Path.GetTempPath(), "VRCX");
            Directory.CreateDirectory(tempPath);
            var filePath = Path.Join(tempPath, fileName);
            await using var fileStream = File.Create(filePath);
            await response.Content.CopyToAsync(fileStream, cancellationToken);
            return filePath;
        }

        private static string GetFileNameFromContentDisposition(HttpResponseMessage response)
        {
            string contentDisposition = response.Content.Headers.ContentDisposition?.ToString();
            if (contentDisposition != null)
            {
                int startIndex = contentDisposition.IndexOf("filename=", StringComparison.OrdinalIgnoreCase);
                if (startIndex >= 0)
                {
                    startIndex += "filename=".Length;
                    int endIndex = contentDisposition.IndexOf(";", startIndex, StringComparison.Ordinal);
                    if (endIndex == -1)
                    {
                        endIndex = contentDisposition.Length;
                    }

                    string fileName = contentDisposition.Substring(startIndex, endIndex - startIndex).Trim(' ', '"');
                    return fileName;
                }
            }

            throw new Exception("Unable to extract file name from content-disposition header.");
        }
    }
}