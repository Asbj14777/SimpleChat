using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat_Client.Services
{
    public class ChatClientService
    {
        private TcpClient? client;
        private NetworkStream? stream;
        private string userName = "You"; 

        public event Action<string>? MessageReceived;
        public event Action<string>? StatusChanged;

        public void SetUserName(string name)
        {
            userName = name;
        }

        public async Task<bool> ConnectAsync()
        {
            string? serverIp = await DiscoverServer();

            if (serverIp == null)
            {
                StatusChanged?.Invoke("No chat server found on LAN.");
                return false;
            }

            StatusChanged?.Invoke($"Found server at {serverIp}, connecting...");

            client = new TcpClient(serverIp, 5000);
            stream = client.GetStream();

            StatusChanged?.Invoke("Connected.");


            await SendMessage($"__username__:{userName}");

    
            _ = Task.Run(() => ReceiveLoop());
            return true;
        }

        public async Task SendMessage(string text)
        {
            if (stream == null) return;

            byte[] data = Encoding.UTF8.GetBytes(text);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int read = await stream!.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, read);
                    MessageReceived?.Invoke(msg);
                }
            }
            catch
            {
                StatusChanged?.Invoke("Disconnected from server.");
            }
        }

        private async Task<string?> DiscoverServer()
        {
            using UdpClient udp = new UdpClient();
            udp.EnableBroadcast = true;

            byte[] request = Encoding.UTF8.GetBytes("DISCOVER_CHAT_SERVER");
            var endpoint = new IPEndPoint(IPAddress.Broadcast, 5001);

            await udp.SendAsync(request, request.Length, endpoint);

            var receiveTask = udp.ReceiveAsync();
            var timeout = Task.Delay(2000);

            var completed = await Task.WhenAny(receiveTask, timeout);

            if (completed == timeout)
                return null;

            string msg = Encoding.UTF8.GetString(receiveTask.Result.Buffer);

            if (msg.StartsWith("CHAT_SERVER"))
            {
                var parts = msg.Split('|');
                return parts[1];
            }

            return null;
        }
    }
}
