using Avalonia.Controls;
using Avalonia.Input;
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

    protected override void OnLoaded()
    {
        base.OnLoaded();
        
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
