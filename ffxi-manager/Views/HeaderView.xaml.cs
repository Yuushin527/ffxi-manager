using System.Windows;
using System.Windows.Controls;
using FFXIManager.Infrastructure;
using FFXIManager.ViewModels;
using FFXIManager.Views.ClaudeLogin;

namespace FFXIManager.Views
{
    public partial class HeaderView : UserControl
    {
        public HeaderView()
        {
            InitializeComponent();
            DataContext = new HeaderViewModel(
                ServiceLocator.UiDispatcher,
                ServiceLocator.SettingsService,
                ServiceLocator.ExternalApplicationService,
                ServiceLocator.PlayOnlineMonitorService);
        }

        private void ClaudeLoginButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ClaudeLoginDialog
            {
                Owner = Window.GetWindow(this)
            };
            dialog.ShowDialog();
        }
    }
}
