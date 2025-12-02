using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class ChatServer
{
    static TcpListener listener;
    static readonly ConcurrentBag<TcpClient> clients = new();
    static readonly ConcurrentDictionary<TcpClient, string> names = new();
    static readonly CancellationTokenSource shutdown = new();

    static async Task Main()
    {
        Console.Title = "Server";

        _ = RunDiscovery(shutdown.Token);

        listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();

        Log($"Chat server started on port 5000 (IP: {GetLocalIP()})");

        while (!shutdown.IsCancellationRequested)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync();
                clients.Add(client);
                Log("Client connected.");
                _ = HandleClient(client);
            }
            catch (Exception ex)
            {
                Log($"Listener error: {ex.Message}");
            }
        }
    }

    static async Task HandleClient(TcpClient client)
    {
        string user = "Unknown";

        try
        {
            var stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0) break;

                string msg = Encoding.UTF8.GetString(buffer, 0, read).Trim();

                if (msg.StartsWith("__username__:"))
                {
                    string newUser = msg[13..].Trim();
                    if (!string.IsNullOrWhiteSpace(newUser))
                    {
                        names[client] = newUser;
                        user = newUser;
                        await BroadcastAsync($"*** {user} joined the chat ***");
                    }
                    continue;
                }

                Log($"{user}: {msg}");
                await BroadcastAsync($"{user}: {msg}", exclude: client);
            }
        }
        catch (Exception ex)
        {
            Log($"Client error ({user}): {ex.Message}");
        }
        finally
        {
            CleanupClient(client, user);
        }
    }

    static async Task BroadcastAsync(string message, TcpClient exclude = null)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        foreach (var client in clients)
        {
            if (client == exclude) continue;

            try
            {
                await client.GetStream().WriteAsync(data);
            }
            catch
            {
                CleanupClient(client);
            }
        }

        Log(message);
    }

    static void CleanupClient(TcpClient client, string username = null)
    {
        if (client == null) return;

        clients.TryTake(out _);
        names.TryRemove(client, out _);

        client.Close();

        if (username != null)
            _ = BroadcastAsync($"*** {username} left the chat ***");

        Log($"Client disconnected: {username}");
    }

    static async Task RunDiscovery(CancellationToken token)
    {
        using var udp = new UdpClient(5001);
        Log("Running discovery service on port 5001...");

        while (!token.IsCancellationRequested)
        {
            try
            {
                var result = await udp.ReceiveAsync(token);
                string msg = Encoding.UTF8.GetString(result.Buffer);

                if (msg == "DISCOVER_CHAT_SERVER")
                {
                    string response = $"CHAT_SERVER|{GetLocalIP()}|5000";
                    byte[] data = Encoding.UTF8.GetBytes(response);
                    await udp.SendAsync(data, data.Length, result.RemoteEndPoint);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Log($"Discovery error: {ex.Message}");
            }
        }
    }

    static string GetLocalIP()
    {
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        return "127.0.0.1";
    }

    static void Log(string msg) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {msg}");
}