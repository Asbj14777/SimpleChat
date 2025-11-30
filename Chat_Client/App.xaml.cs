using Chat_Client.Views;
using System.Windows;

namespace Chat_Client
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Show NameDialog first
            var nameDialog = new NameDialog();
            bool? result = nameDialog.ShowDialog();

            if (result != true)
            {
                Shutdown(); // Exit app if user cancels
                return;
            }

            // Open MainWindow and pass username
            var mainWindow = new MainWindow();

            if (mainWindow.DataContext is Viewmodel.MainViewModel vm)
            {
                vm.UserName = nameDialog.UserName;
            }

            mainWindow.Show();
        }
    }
}
