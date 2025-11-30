using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chat_Client.Services
{
    public class ChatClientService
    {
        TcpClient? client;
        NetworkStream? stream;
        string userName = "You";

        public event Action<string>? MessageReceived;
        public event Action<string>? StatusChanged;

        public void SetUserName(string name) => userName = name;

        public async Task<bool> ConnectAsync()
        {
            string? ip = await DiscoverServer();
            if (ip == null)
            {
                StatusChanged?.Invoke("No chat server found on LAN.");
                return false;
            }

            StatusChanged?.Invoke($"Found server at {ip}, connecting...");
            client = new TcpClient(ip, 5000);
            stream = client.GetStream();
            StatusChanged?.Invoke("Connected.");

            await SendMessage($"__username__:{userName}");
            _ = Task.Run(ReceiveLoop);
            return true;
        }

        public async Task SendMessage(string text)
        {
            if (stream == null) return;
            byte[] data = Encoding.UTF8.GetBytes(text);
            await stream.WriteAsync(data);
        }

        async Task ReceiveLoop()
        {
            var buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int read = await stream!.ReadAsync(buffer);
                    if (read == 0) break;

                    MessageReceived?.Invoke(
                        Encoding.UTF8.GetString(buffer, 0, read)
                    );
                }
            }
            catch
            {
                StatusChanged?.Invoke("Disconnected from server.");
            }
        }

        async Task<string?> DiscoverServer()
        {
            using var udp = new UdpClient() { EnableBroadcast = true };

            byte[] req = Encoding.UTF8.GetBytes("DISCOVER_CHAT_SERVER");
            await udp.SendAsync(req, req.Length, new IPEndPoint(IPAddress.Broadcast, 5001));

            var resultTask = udp.ReceiveAsync();
            if (await Task.WhenAny(resultTask, Task.Delay(2000)) != resultTask)
                return null;

            string msg = Encoding.UTF8.GetString(resultTask.Result.Buffer);
            return msg.StartsWith("CHAT_SERVER") ? msg.Split('|')[1] : null;
        }
    }
}