using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Chat_Client.Converters;
using Chat_Client.Viewmodel;
using Chat_Client.Services; 
namespace Chat_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel mainViewModel = new MainViewModel();
        public MainWindow()
        {
            DataContext = mainViewModel;
            InitializeComponent();
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true; // Prevent newline

                if (DataContext is Viewmodel.MainViewModel vm)
                {
                    if (vm.SendCommand.CanExecute(null))
                        vm.SendCommand.Execute(null);
                }
            }
        }
    }
}