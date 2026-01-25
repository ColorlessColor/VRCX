using VRCX.Core.Services;

namespace VRCX.Core.WebViewInterop;

public class VRCXStorage(AppStorageService appStorageService)
{
    public void Save() => appStorageService.Save();
    public void Clear() => appStorageService.Clear();
    public bool Remove(string key) => appStorageService.Remove(key);
    public string Get(string key) => appStorageService.Get(key);
    public void Set(string key, string value) => appStorageService.Set(key, value);
    public string GetAll() => appStorageService.GetAll();
}