
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class ChatServer
{
    static TcpListener listener;
    static readonly List<TcpClient> clients = new();
    static readonly object lockObj = new();
    static readonly Dictionary<TcpClient, string> clientNames = new();
    static async Task Main()
    {
        Console.Title = "Server"; 
        _ = StartDiscoveryServer();

        listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Chat server started on port 5000...");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            lock (lockObj) clients.Add(client);

            Console.WriteLine("Client connected.");
            _ = HandleClient(client);
        }

    }

    static async Task HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        byte[] buffer = new byte[1024];

        string username = "Unknown";

        try
        {
            while (true)
            {
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0) break; 

                string msg = Encoding.UTF8.GetString(buffer, 0, read);

                if (msg.StartsWith("__username__:"))
                {
                    username = msg.Substring(13);
                    lock (lockObj) clientNames[client] = username;

                    BroadcastSystemMessage($"*** {username} joined the chat ***");
                    continue;
                }
                Console.WriteLine($"{username}: {msg}");
                BroadcastMessage($"{username}: {msg}", client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client error: {ex.Message}");
        }
        finally
        {
            lock (lockObj)
            {
                clients.Remove(client);
                clientNames.Remove(client);
            }

            BroadcastSystemMessage($"*** {username} left the chat ***");

            client.Close();
            Console.WriteLine($"Client {username} disconnected.");
        }
    }

    static void BroadcastMessage(string message, TcpClient sender)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        lock (lockObj)
        {
            foreach (var c in clients)
            {
                if (c == sender) continue;
                try
                {
                    c.GetStream().Write(data, 0, data.Length);
                }
                catch { }
            }
        }
    }

    static void BroadcastSystemMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        lock (lockObj)
        {
            foreach (var c in clients)
            {
                try
                {
                    c.GetStream().Write(data, 0, data.Length);
                }
                catch { }
            }
        }

        Console.WriteLine(message);
    }

    static async Task StartDiscoveryServer()
    {
        UdpClient udp = new UdpClient(5001);
        Console.WriteLine("Discovery service running on port 5001...");

        while (true)
        {
            try
            {
                var result = await udp.ReceiveAsync();
                string request = Encoding.UTF8.GetString(result.Buffer);

                if (request == "DISCOVER_CHAT_SERVER")
                {
                    string response = "CHAT_SERVER|" + GetLocalIPAddress() + "|5000";
                    byte[] data = Encoding.UTF8.GetBytes(response);

                    await udp.SendAsync(data, data.Length, result.RemoteEndPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Discovery error: {ex.Message}");
            }
        }
    }

    static string GetLocalIPAddress()
    {
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();

        return "127.0.0.1";
    }
}
