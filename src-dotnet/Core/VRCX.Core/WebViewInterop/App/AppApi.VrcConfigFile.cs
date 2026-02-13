namespace VRCX.Core.WebViewInterop.App
{
    public partial class AppApi
    {
        public string ReadConfigFile()
        {
            var path = GetVRChatAppDataLocation();
            var configFile = Path.Join(path, "config.json");
            if (!Directory.Exists(path) || !File.Exists(configFile))
            {
                return string.Empty;
            }

            var json = File.ReadAllText(configFile);
            return json;
        }

        public void WriteConfigFile(string json)
        {
            var path = GetVRChatAppDataLocation();
            Directory.CreateDirectory(path);
            var configFile = Path.Join(path, "config.json");
            File.WriteAllText(configFile, json);
        }
    }
}