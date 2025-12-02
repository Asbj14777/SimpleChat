using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chat_Client.Services
{
    public class ChatClientService
    {
        private TcpClient? client;
        private NetworkStream? stream;
        private string userName = "You";
        private CancellationTokenSource? cancelToken;

        public event Action<string>? MessageReceived;
        public event Action<string>? StatusChanged;

        public void SetUserName(string name) => userName = name;

        public async Task<bool> ConnectAsync()
        {
            string? ip = await DiscoverServer();
            if (ip == null)
            {
                StatusChanged?.Invoke("No chat server found.");
                return false;
            }

            try
            {
                StatusChanged?.Invoke($"Found server at {ip}, connecting...");

                client = new TcpClient();
                await client.ConnectAsync(ip, 5000);

                stream = client.GetStream();
                cancelToken = new CancellationTokenSource();

                StatusChanged?.Invoke("Connected.");

                await SendMessageAsync($"__username__:{userName}");

                _ = Task.Run(() => ReceiveLoop(cancelToken.Token));

                return true;
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Connection failed: {ex.Message}");
                Disconnect();
                return false;
            }
        }

        public async Task SendMessageAsync(string text)
        {
            if (stream == null || !client!.Connected) 
                return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(text);
                await stream.WriteAsync(data, 0, data.Length);
            }
            catch
            {
                StatusChanged?.Invoke("Failed to send message.");
                Disconnect();
            }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            byte[] buffer = new byte[1024];
            var decoder = Encoding.UTF8.GetDecoder();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int read = await stream!.ReadAsync(buffer, 0, buffer.Length, token);
                    if (read == 0) break;

                    char[] chars = new char[decoder.GetCharCount(buffer, 0, read)];
                    decoder.GetChars(buffer, 0, read, chars, 0);

                    string message = new string(chars);
                    MessageReceived?.Invoke(message);
                }
            }
            catch
            {
                StatusChanged?.Invoke("Lost connection to server.");
            }
            finally
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            try
            {
                StatusChanged?.Invoke("Disconnected.");
                cancelToken?.Cancel();
                stream?.Close();
                client?.Close();
            }
            catch { }

            stream = null;
            client = null;
            cancelToken = null;
        }

        private async Task<string?> DiscoverServer()
        {
            using var udp = new UdpClient() { 
                EnableBroadcast = true 
            };

            byte[] req = Encoding.UTF8.GetBytes("DISCOVER_CHAT_SERVER");
            await udp.SendAsync(req, req.Length, new IPEndPoint(IPAddress.Broadcast, 5001));

            var resultTask = udp.ReceiveAsync();
            var completed = await Task.WhenAny(resultTask, Task.Delay(2000));

            if (completed != resultTask)
                return null;

            string msg = Encoding.UTF8.GetString(resultTask.Result.Buffer).Trim();

            var parts = msg.Split('|');
            if (parts.Length != 3 || parts[0] != "CHAT_SERVER")
                return null;

            string ip = parts[1];

            if (!IPAddress.TryParse(ip, out _))
                return null;

            return ip;
        }
    }
}
