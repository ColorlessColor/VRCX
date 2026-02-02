using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using VRCX.App.ViewModels;

namespace VRCX.App.Views;

public partial class BootstrapWindow : Window
{
    public BootstrapWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is not BootstrapWindowViewModel bootstrapWindowViewModel) return;

        bootstrapWindowViewModel.RequestClose += OnRequestClose;
        _ = bootstrapWindowViewModel.BootstrapAsync();
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        Close();
    }

    private bool _isWindowDragInEffect;
    private Point _cursorPositionAtWindowDragStart = new(0, 0);

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isWindowDragInEffect = true;
        _cursorPositionAtWindowDragStart = e.GetPosition(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e) =>
        _isWindowDragInEffect = false;

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isWindowDragInEffect)
        {
            var currentCursorPosition = e.GetPosition(this);
            var cursorPositionDelta = currentCursorPosition - _cursorPositionAtWindowDragStart;

            Position = this.PointToScreen(cursorPositionDelta);
        }
    }
}