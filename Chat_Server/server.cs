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
    static readonly Dictionary<TcpClient, string> names = new();
    static readonly object lockObj = new();

    static async Task Main()
    {
        Console.Title = "Server";
        _ = RunDiscovery();

        listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Chat server started on port 5000");

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
        var buf = new byte[1024];
        string user = "Unknown";

        try
        {
            while (true)
            {
                int read = await stream.ReadAsync(buf, 0, buf.Length);
                if (read == 0) break;

                string msg = Encoding.UTF8.GetString(buf, 0, read);

                if (msg.StartsWith("__username__:"))
                {
                    user = msg[13..];
                    lock (lockObj) names[client] = user;
                    Broadcast($"*** {user} joined the chat ***");
                    continue;
                }

                Console.WriteLine($"{user}: {msg}");
                Broadcast($"{user}: {msg}", client);
            }
        }
        catch { }
        finally
        {
            lock (lockObj)
            {
                clients.Remove(client);
                names.Remove(client);
            }
            Broadcast($"*** {user} left the chat ***");
            client.Close();
        }
    }

    static void Broadcast(string message, TcpClient exclude = null)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        lock (lockObj)
        {
            foreach (var c in clients)
            {
                if (c == exclude) continue;
                try { c.GetStream().Write(data, 0, data.Length); } catch { }
            }
        }
        Console.WriteLine(exclude == null ? message : "");
    }

    static async Task RunDiscovery()
    {
        using var udp = new UdpClient(5001);
        Console.WriteLine("Discovery service on port 5001");

        while (true)
        {
            var result = await udp.ReceiveAsync();
            if (Encoding.UTF8.GetString(result.Buffer) == "DISCOVER_CHAT_SERVER")
            {
                string response = $"CHAT_SERVER|{GetLocalIP()}|5000";
                byte[] data = Encoding.UTF8.GetBytes(response);
                await udp.SendAsync(data, data.Length, result.RemoteEndPoint);
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
}