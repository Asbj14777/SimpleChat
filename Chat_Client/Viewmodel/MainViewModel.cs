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
        private readonly ChatClientService _chatService;


        public ObservableCollection<Message> Messages { get; set; } = new();


        private string status = "";
        public string Status
        {
            get => status;
            set { status = value; OnPropertyChanged(); }
        }

        private string inputMessage = "";
        public string InputMessage
        {
            get => inputMessage;
            set { inputMessage = value; OnPropertyChanged(); }
        }

        private string userName = "You";
        public string UserName
        {
            get => userName;
            set { userName = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }

        public MainViewModel()
        {
            _chatService = new ChatClientService();

         
            _chatService.MessageReceived += OnMessageReceived;
            _chatService.StatusChanged += s => Status = s;

            SendCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => !string.IsNullOrWhiteSpace(InputMessage));
        }


        public async Task ConnectAsync()
        {
            _chatService.SetUserName(UserName);
            bool connected = await _chatService.ConnectAsync();
            if (!connected)
                Status = "Failed to connect to server.";
        }

        private void OnMessageReceived(string msg)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                bool isSystem = msg.StartsWith("***") && msg.EndsWith("***");

                string sender = "";
                string text = msg;
                bool isIncoming = false;

                if (isSystem)
                {
 
                    text = msg.Trim('*').Trim();
                    sender = "";
                    isIncoming = true;
                }
                else
                {
         
                    int index = msg.IndexOf(":");
                    if (index > 0)
                    {
                        sender = msg.Substring(0, index).Trim();
                        text = msg.Substring(index + 1).Trim(); 
                    }
                    else
                    {
                        sender = "";
                        text = msg.Trim();
                    }

                    isIncoming = sender != UserName;
                }

            
                Messages.Add(new Message
                {
                    Text = text,
                    Sender = sender,        
                    IsIncoming = isIncoming,
                    Type = isSystem ? MessageType.System : MessageType.Chat,
                    Timestamp = DateTime.Now
                });
            });
        }



        private async Task SendMessageAsync()
        {
            string text = InputMessage.Trim();
            if (string.IsNullOrEmpty(text)) return;

            await _chatService.SendMessage(text);

            Messages.Add(new Message
            {
                Text = text,
                IsIncoming = false,
                Timestamp = DateTime.Now,
                Type = MessageType.Chat,
                Sender = UserName
            });

            InputMessage = "";
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion
    }
}
