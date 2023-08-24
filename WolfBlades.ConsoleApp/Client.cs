using System.Net.WebSockets;
using System.Text;
using Fleck;

namespace WolfBlades.ConsoleApp;

public class Client : ICanStart
{
    public void Start()
    {
        var client = new ClientWebSocket();
        var attempt = 0;
        for (; attempt < 10; attempt++)
        {
            FleckLog.Info($"尝试连接到服务器({Program.Location}). 尝试次数: {attempt}");
            if (!client.ConnectAsync(new Uri($"ws://{Program.Location}"), CancellationToken.None).IsFaulted) break;
        }

        if (attempt == 10)
        {
            FleckLog.Error($"无法连接到服务器({Program.Location})");
            Environment.Exit(-1);
        }

        FleckLog.Info($"成功连接到服务器({Program.Location})，输入quit以退出");
        while (true)
        {
            Console.Write(">> ");
            var command = Console.ReadLine();

            if (command == null) continue;

            if (command == "quit") break;

            client.SendAsync(Encoding.UTF8.GetBytes(command), WebSocketMessageType.Text, true, CancellationToken.None);
            FleckLog.Info($"客户端> {command}");

            Thread.Sleep(100);

            var receive_buffer = new ArraySegment<byte>(new byte[1024 * 10]);
            client.ReceiveAsync(receive_buffer, CancellationToken.None);
            FleckLog.Info($"服务器> {Encoding.UTF8.GetString(receive_buffer)}");
        }

        client.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
        client.Dispose();
    }
}