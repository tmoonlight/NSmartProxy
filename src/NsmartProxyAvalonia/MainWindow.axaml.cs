using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NSmartProxy.Client;

namespace NsmartProxyAvalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.FindControl<Button>("btnTest").Click += testshao_click;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void testshao_click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            //≤‚ ‘
            var test = new Router();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
