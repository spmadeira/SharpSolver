using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SharpSolverUI.Avalonia.ViewModels;

namespace SharpSolverUI.Avalonia.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContextChanged += (sender, args) =>
            {
                var context = DataContext as MainWindowViewModel;
                context.MainWindow = this;
                context.TextBox = this.FindControl<TextBox>("TextBox");
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
