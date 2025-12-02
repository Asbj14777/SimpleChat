using Chat_Client.Models;
using Chat_Client.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Chat_Client.Viewmodel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        readonly ChatClientService _chatService = new();
        public ObservableCollection<Message> Messages { get; } = new();
    

        string status = "";
        public string Status { get => status; set => Set(ref status, value); }

        string input = "";
        public string InputMessage { get => input; set => Set(ref input, value); }

        string user = "You";
        public string UserName { get => user; set => Set(ref user, value); }

        public ICommand SendCommand { get; }

        public MainViewModel()
        {
            _chatService.MessageReceived += HandleIncoming;
            _chatService.StatusChanged += _status => Status = _status;

            SendCommand = new RelayCommand(
                async _ => await SendAsync(),
                _ => !string.IsNullOrWhiteSpace(InputMessage)
            );
        }

        public async Task ConnectAsync()
        {
            _chatService.SetUserName(UserName);
            if (!await _chatService.ConnectAsync())
                Status = "Failed to connect to server.";
        }

        void HandleIncoming(string msg)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                bool system = msg.StartsWith("***") && msg.EndsWith("***");
                string sender = "", text = msg;

                if (system)
                {
                    text = msg.Trim('*').Trim();
                }
                else
                {
                    int index = msg.IndexOf(':');
                    if (index > 0)
                    {
                        sender = msg[..index].Trim();
                        text = msg[(index + 1)..].Trim();
                    }
                }

                Messages.Add(new Message
                {
                    Sender = sender,
                    Text = text,
                    Type = system ? MessageType.System : MessageType.Chat,
                    IsIncoming = sender != UserName,
                    Timestamp = DateTime.Now
                });
            });
        }

        async Task SendAsync()
        {
            string text = InputMessage.Trim();
            if (text == "") return;

            await _chatService.SendMessageAsync(text);

            Messages.Add(new Message
            {
                Sender = UserName,
                Text = text,
                Timestamp = DateTime.Now,
                Type = MessageType.Chat,
                IsIncoming = false
            });

            InputMessage = "";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void Set<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}