using Chat_Client.Views;
using System.Windows;

namespace Chat_Client
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var nameDialog = new NameDialog();
            bool? result = nameDialog.ShowDialog();

            if (result != true)
            {
                MessageBox.Show("failed"); 
                Shutdown();
                return;
            }

            var mainWindow = new MainWindow();

            if (mainWindow.DataContext is Viewmodel.MainViewModel vm)
                vm.UserName = nameDialog.UserName;

            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
