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

    static async Task Main()
    {
        _ = startDiscoveryServer();

        listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Chat server started on port 5000...");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            lock (lockObj) clients.Add(client);

            Console.WriteLine("Client connected.");
            _ = clientHandler(client);
        }
    }

    static async Task clientHandler(TcpClient client)
    {
        byte[] buffer = new byte[1024];
        var stream = client.GetStream();

        try
        {
            while (true)
            {
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0) break;

                string msg = Encoding.UTF8.GetString(buffer, 0, read);
                Console.WriteLine("Received: " + msg);

                broadcastMessage(msg, client);
            }
        }
        catch { }
        finally
        {
            lock (lockObj)
                clients.Remove(client);

            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }

    static void broadcastMessage(string message, TcpClient sender)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        lock (lockObj)
        {
            foreach (var c in clients)
            {
                if (c == sender) 
                    continue;

                try
                {
                    c.GetStream().Write(data, 0, data.Length);
                }
                catch { }
            }
        }
    }

    static async Task startDiscoveryServer()
    {
        UdpClient udp = new UdpClient(5001);
        Console.WriteLine("Discovery service running on port 5001...");

        while (true)
        {
            var result = await udp.ReceiveAsync();
            string request = Encoding.UTF8.GetString(result.Buffer);

            if (request == "DISCOVER_CHAT_SERVER")
            {
                string response = "CHAT_SERVER|" + getLocalIPAddress() + "|5000";
                byte[] data = Encoding.UTF8.GetBytes(response);

                await udp.SendAsync(data, data.Length, result.RemoteEndPoint);
            }
        }
    }

    static string getLocalIPAddress()
    {
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();

        return "127.0.0.1";
    }
}
