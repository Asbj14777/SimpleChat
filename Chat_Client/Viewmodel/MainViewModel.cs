using Chat_Client.Models;
using Chat_Client.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Chat_Client.Viewmodel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ChatClientService _clientService = new();
        public string UserName { get; set; } = "You";
        public ObservableCollection<Message> Messages { get; } = new();

        private string inputMessage = "";
        public string InputMessage
        {
            get => inputMessage;
            set { inputMessage = value; OnPropertyChanged(); }
        }

        private string status = "";
        public string Status
        {
            get => status;
            set { status = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }

        public MainViewModel()
        {

            SendCommand = new RelayCommand(async _ => await SendMessage());

            // Hook service events
            _clientService.MessageReceived += OnMessageReceived;
            _clientService.StatusChanged += s => Status = s;

            // Auto-connect
            _ = ConnectAsync();
        }

        private async Task ConnectAsync()
        {
            await _clientService.ConnectAsync();
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(InputMessage))
                return;

            await _clientService.SendMessage(InputMessage);

            Messages.Add(new Message
            {
                Text = InputMessage,
                Sender = UserName, 
                IsIncoming = false,
                Timestamp = DateTime.Now,
                Type = MessageType.Chat
            });

            InputMessage = "";
        }

        private void OnMessageReceived(string msg)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(new Message
                {
                    Text = msg,
                    Sender = "Other",
                    IsIncoming = true,
                    Timestamp = DateTime.Now,
                    Type = MessageType.Chat
                });
            });
        }

        public void AddSystemMessage(string text)
        {
            Messages.Add(new Message
            {
                Text = text,
                Type = MessageType.System,
                IsIncoming = true,
                Timestamp = DateTime.Now
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
