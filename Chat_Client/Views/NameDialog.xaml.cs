using System.Windows;

namespace Chat_Client.Views
{
    public partial class NameDialog : Window
    {
        public string UserName { get; private set; } = "";

        public NameDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                UserName = NameTextBox.Text.Trim();
                this.DialogResult = true; 
            }
            else
            {
                MessageBox.Show("Please enter a valid name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}