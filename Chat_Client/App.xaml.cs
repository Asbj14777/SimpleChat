using Chat_Client.Viewmodel;
using Chat_Client.Views;
using System.Windows;

namespace Chat_Client
{
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var nameDialog = new NameDialog();
            bool? result = nameDialog.ShowDialog();

            if (result != true)
            {
                Shutdown();
                return;
            }

            var mainWindow = new MainWindow();
            var vm = (MainViewModel)mainWindow.DataContext;
            vm.UserName = nameDialog.UserName;

            Current.MainWindow = mainWindow;
            mainWindow.Show();

            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            await vm.ConnectAsync();
        }
    }
}
