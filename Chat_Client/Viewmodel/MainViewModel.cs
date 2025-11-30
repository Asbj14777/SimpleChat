using Chat_Client.Models;
using Chat_Client.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace Chat_Client.Viewmodel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ChatClientService _clientService = new();

        public ObservableCollection<Message> Messages { get; } = new();

        private string inputMessage;
        public string InputMessage
        {
            get => inputMessage;
            set { inputMessage = value; OnPropertyChanged(); }
        }

        private string status;
        public string Status
        {
            get => status;
            set { status = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }

        public MainViewModel()
        {
            SendCommand = new RelayCommand(async _ => await SendMessage());

            _clientService.MessageReceived += OnMessageReceived;
            _clientService.StatusChanged += s => Status = s;

   
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
                IsIncoming = false
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
                    IsIncoming = true
                });
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
