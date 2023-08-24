using System.Net.WebSockets;
using System.Text;
using Fleck;

namespace WolfBlades.ConsoleApp;

public class Client : ICanStart
{
    public void Start()
    {
        using var client = new ClientWebSocket();
        var attempt = 0;
        for (; attempt < 10; attempt++)
        {
            FleckLog.Info($"Trying to connect {Program.Location}. Attempt: {attempt}");
            if (!client.ConnectAsync(new Uri($"ws://{Program.Location}"), CancellationToken.None).IsFaulted) break;
        }

        if (attempt == 10)
        {
            FleckLog.Error($"Cannot connect {Program.Location}");
            Environment.Exit(-1);
        }

        FleckLog.Info($"Connected to {Program.Location}");
        while (true)
        {
            Console.Write(">> ");
            var command = Console.ReadLine();

            if (command == null) continue;

            if (command == "quit") break;

            client.SendAsync(Encoding.UTF8.GetBytes(command), WebSocketMessageType.Text, true, CancellationToken.None);
            FleckLog.Info($"Client> {command}");

            Thread.Sleep(500);

            var receive_buffer = new ArraySegment<byte>(new byte[1024 * 10]);
            client.ReceiveAsync(receive_buffer, CancellationToken.None);
            FleckLog.Info($"Server> {Encoding.UTF8.GetString(receive_buffer)}");
        }

        client.CloseAsync(WebSocketCloseStatus.Empty, "Normal close", CancellationToken.None);
    }
}