using System.Text;
using Fleck;
using WolfBlades.BackEnd;
using WolfBlades.BackEnd.ItemManager;
using Timer = System.Timers.Timer;

namespace WolfBlades.ConsoleApp;

public class Server : ICanStart
{
    protected DataServer DataServer;

    protected Timer SaveTimer = new();

    public Server()
    {
        FleckLog.Info("正在初始化数据服务器: DataServer");
        DataServer = new DataServer();

        SaveTimer.AutoReset = true;
        SaveTimer.Interval = 1000 * 60 * 60 * 2;
        SaveTimer.Elapsed += SaveTimer_Elapsed;

        FleckLog.Info($"正在初始化: {nameof(WebSocketServer).Replace("WebSocket", "")}");
        WebSocketServer = new WebSocketServer(Location);

        SaveTimer.Start();
    }

    private void SaveTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        DataServer.Save();
    }

    public bool IsRunning { get; private set; }

    protected WebSocketServer WebSocketServer { get; private init; }

    public static string Location => $"ws://{Program.Location}";

    public void Start()
    {
        if (WebSocketServer is null)
            throw new InvalidOperationException("服务器无法运行: 服务器未初始化");

        IsRunning = true;
        WebSocketServer.Start(HandleNewConnection);
        HandleConsoleInput();
        IsRunning = false;
    }

    ~Server()
    {
        SaveTimer.Stop();
        SaveTimer.Elapsed -= SaveTimer_Elapsed;
        SaveTimer.Dispose();

        DataServer.Save();
        WebSocketServer.Dispose();
    }

    #region Connection

    private void HandleNewConnection(IWebSocketConnection connection)
    {
        var connection_info = new ConnectionInfo
        {
            ID = -1,
            IPAddress = connection.ConnectionInfo.ClientIpAddress,
            Port = connection.ConnectionInfo.ClientPort,
            LoginUserID = -1,
            UserAuthority = 0
        };

        var id = DataServer.Managers[DataServer.ManagerType.Connection].AppendItem(connection_info);
        connection_info.ID = id;

        if (id < 0)
        {
            connection.Close(-1);
            return;
        }

        connection.OnOpen = () =>
            FleckLog.Info($"[{id}] 建立新连接 {connection_info.IPAddress}:{connection_info.Port}");

        var send = new Action<string>(message => FleckLog.Info($"[{id}]< {message}"));
        send += message => connection.Send(message);
        connection.OnClose = () => HandleConnectionClose(ref connection_info);
        if (connection_info.ID < 0)
        {
            FleckLog.Warn("连接管理器拒绝管理此连接，尝试关闭连接");
            const string connectionRefused = "-服务器了拒绝来自客户端的连接";
            send(connectionRefused);
            connection.Close(-1);
            return;
        }

        connection.OnMessage = message => { HandleConnectionMessage(ref connection_info, message, send); };
    }

    private void HandleConnectionClose(ref ConnectionInfo info)
    {
        FleckLog.Info($"与 {info.IPAddress}:{info.Port} 断开连接");

        if (info.ID < 0) return;

        if (!DataServer.Managers[DataServer.ManagerType.Connection].RemoveItem(info.ID))
            FleckLog.Error($"[{info.ID}] 无法从连接管理器移除此连接的信息");

        info = new ConnectionInfo();
    }

    private void HandleConnectionMessage(ref ConnectionInfo info, string command, Action<string> send)
    {
        DataServer.HandleCommand(info.ID, command, send);
    }

    #endregion

    #region Console

    private void HandleConsoleInput()
    {
        if (!Environment.UserInteractive)
        {
            while (IsRunning)
            {
            }
            return;
        }

        var input_buffer = new StringBuilder();
        using var output_timer = new Timer();

        output_timer.AutoReset = false;
        output_timer.Interval = 150;
        output_timer.Elapsed += (_, _) =>
        {
            if (input_buffer.Length > 0) FleckLog.Info($">> {input_buffer}");
        };
        output_timer.Start();

        Console.CancelKeyPress += (_, args) =>
        {
            IsRunning = false;
            args.Cancel = true;
            FleckLog.Info("强制关闭服务器，等待服务端退出");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine("再次输入任意内容后退出");
        };

        while (IsRunning)
        {
            output_timer.Start();
            var input = Console.ReadKey(true);
            output_timer.Stop();
            if ((input.Modifiers & ConsoleModifiers.Control) != 0)
                if (input.Key == ConsoleKey.C)
                    IsRunning = false;

            switch (input.Key)
            {
                case ConsoleKey.Backspace:
                    if (input_buffer.Length == 0) continue;
                    input_buffer.Remove(input_buffer.Length - 1, 1);
                    continue;

                case ConsoleKey.Enter:
                    var command = input_buffer.ToString();
                    HandleConsoleCommand(command);
                    input_buffer.Clear();
                    continue;
                default:
                    input_buffer.Append(input.KeyChar);
                    break;
            }
        }

        WebSocketServer.Dispose();
    }

    private void HandleConsoleCommand(string command)
    {
        FleckLog.Info($"服务器控制台输入命令: \"{command}\"");

        var space_index = command.IndexOf(' ');
        var flag = (space_index == -1 ? command : command[..space_index]).ToLower();
        switch (flag)
        {
            case "quit":
                IsRunning = false;
                return;
            case "save":
                DataServer.Save();
                break;
        }
    }

    #endregion
}
