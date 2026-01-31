using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using VRCX.App.Services;

namespace VRCX.App;

public partial class App : Application
{
    internal static IServiceProvider? ServiceProvider { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        InitTrayIconService();
        DataContext = this;
    }
}