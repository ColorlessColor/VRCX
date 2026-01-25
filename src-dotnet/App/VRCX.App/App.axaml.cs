using Avalonia;
using Avalonia.Markup.Xaml;

namespace VRCX.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}