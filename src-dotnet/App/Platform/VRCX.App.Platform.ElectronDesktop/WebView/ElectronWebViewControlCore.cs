using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using ElectronNET.API;
using ElectronNET.API.Entities;
using VRCX.App.WebView;

namespace VRCX.App.Platform.ElectronDesktop.WebView;

public sealed class ElectronWebViewControlCore : NativeControlHost
{
    private BrowserWindow? _browserWindow;
    private const string WebViewMessageChannel = "webview-message";

    private readonly TaskCompletionSource<IntPtr> _nativeHandleTcs = new();

    public EventHandler<PlatformWebViewMessageEventArgs>? OnMessageReceived { get; set; }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetParent(IntPtr child, IntPtr newParent);

    internal async Task InitializeAsync()
    {
        _browserWindow = await Electron.WindowManager.CreateWindowAsync(
            new BrowserWindowOptions
            {
                Show = false,
                Frame = false,
                WebPreferences = new WebPreferences
                {
                    Preload = Path.GetFullPath("preload.js"),
                    ContextIsolation = true
                }
            });

        await Electron.IpcMain.On(WebViewMessageChannel, arg =>
        {
            var message = arg.ToString() ?? "";
            OnMessageReceived?.Invoke(this, new PlatformWebViewMessageEventArgs(message));
        });

        var handle = await _nativeHandleTcs.Task;
        _browserWindow.Show();

        var windowHandle = IntPtr.Parse(await _browserWindow.GetNativeWindowHandle(), NumberStyles.HexNumber);
        ConvertToChildWindow(windowHandle);
        SetParent(windowHandle, handle);
    }
    
    void ConvertToChildWindow(IntPtr hwnd)
    {
        // convert the "normal" window of Electron to a child one
        var style = GetWindowLong(hwnd, GWL_STYLE);
        style |= WS_CHILD;
        SetWindowLong(hwnd, GWL_STYLE, style);
    
        var styleEx = GetWindowLong(hwnd, GWL_EXSTYLE);
        styleEx |= WS_EX_LAYERED;
        SetWindowLong(hwnd, GWL_EXSTYLE, styleEx);
    }
    
    const int GWL_STYLE = -16;
    const int GWL_EXSTYLE = -20;
    const uint WS_CHILD = 0x40000000;
    const uint WS_EX_APPWINDOW = 0x00040000;
    const uint WS_EX_LAYERED = 0x00080000;
    
    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    public static extern uint SetWindowLong(IntPtr hwnd, int index, uint newLong);
    
    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    public static extern uint GetWindowLong(IntPtr hwnd, int index);

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var handle = base.CreateNativeControlCore(parent);
        _nativeHandleTcs.TrySetResult(handle.Handle);
        return handle;
    }

    internal void Navigate(string url)
    {
        _browserWindow?.LoadURL(url);
    }

    internal void ExecuteScript(string script)
    {
        _ = _browserWindow?.WebContents.ExecuteJavaScriptAsync<object>(script, true);
    }

    internal void OpenDevTools()
    {
        _browserWindow?.WebContents.OpenDevTools();
    }

    internal void PostMessage(string message)
    {
        Electron.IpcMain.Send(_browserWindow, WebViewMessageChannel, message);
    }

    internal void OnBoundsChanged(System.Drawing.Rectangle rectangle)
    {
        _browserWindow?.SetSize(rectangle.Width, rectangle.Height);
        _browserWindow?.SetPosition(0, 0);
    }
}