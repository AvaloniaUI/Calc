using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Calc.ViewModels;

namespace Calc.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        Focusable = true;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Focus();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        if (DataContext is MainWindowViewModel vm)
        {
            vm.ProcessText(e.Text);
        }
    }
}
