using System.Text;
using Fleck;
using Newtonsoft.Json;
using WolfBlades.BackEnd.Comments;
using WolfBlades.BackEnd.Connections;
using WolfBlades.BackEnd.Documents;
using WolfBlades.BackEnd.Tasks;
using WolfBlades.BackEnd.Units;
using WolfBlades.BackEnd.Users;
using Timer = System.Timers.Timer;

namespace WolfBlades.ConsoleApp;

public class Server : ICanStart
{
    public delegate void ConnectionCommandDelegate(ref ConnectionInfo info, string[] args, CanSend send);

    protected Dictionary<string, ConnectionCommandDelegate> Commands;

    public Server()
    {
        FleckLog.Info($"Initializing: {nameof(CommentInfoManager).Replace("Info", " ")}");
        CommentInfoManager = new CommentInfoManager();

        FleckLog.Info($"Initializing: {nameof(ConnectionInfoManager).Replace("Info", " ")}");
        ConnectionInfoManager = new ConnectionInfoManager();

        FleckLog.Info($"Initializing: {nameof(DocumentInfoManager).Replace("Info", " ")}");
        DocumentInfoManager = new DocumentInfoManager();

        FleckLog.Info($"Initializing: {nameof(TaskInfoManager).Replace("Info", " ")}");
        TaskInfoManager = new TaskInfoManager();

        FleckLog.Info($"Initializing: {nameof(UnitInfoManager).Replace("Info", " ")}");
        UnitInfoManager = new UnitInfoManager();

        FleckLog.Info($"Initializing: {nameof(UserInfoManager).Replace("Info", " ")}");
        UserInfoManager = new UserInfoManager();

        FleckLog.Info($"Initializing: {nameof(Commands)}");
        Commands = new Dictionary<string, ConnectionCommandDelegate>();
        RegisterCommands();

        ReadDataFromFiles();

        FleckLog.Info($"Initializing: {nameof(WebSocketServer).Replace("WebSocket", "")}");
        WebSocketServer = new WebSocketServer(Location);
    }

    ~Server()
    {
        WebSocketServer.Dispose();
    }

    public bool IsRunning { get; private set; }

    protected CommentInfoManager CommentInfoManager { get; private init; }
    protected ConnectionInfoManager ConnectionInfoManager { get; private init; }
    protected DocumentInfoManager DocumentInfoManager { get; private init; }
    protected TaskInfoManager TaskInfoManager { get; private init; }
    protected UnitInfoManager UnitInfoManager { get; private init; }
    protected UserInfoManager UserInfoManager { get; private init; }
    protected WebSocketServer WebSocketServer { get; private init; }

    public static string Location => $"ws://{Program.Location}";

    public void Start()
    {
        if (
            CommentInfoManager is null ||
            ConnectionInfoManager is null ||
            DocumentInfoManager is null ||
            TaskInfoManager is null ||
            UnitInfoManager is null ||
            UserInfoManager is null ||
            WebSocketServer is null
        )
            throw new InvalidOperationException("服务器无法运行: 服务器未初始化");

        IsRunning = true;
        WebSocketServer.Start(HandleNewConnection);
        HandleConsoleInput();
        IsRunning = false;
    }

    private void HandleNewConnection(IWebSocketConnection connection)
    {
        var connection_info = new ConnectionInfo
        {
            IPAddress = connection.ConnectionInfo.ClientIpAddress,
            Port = connection.ConnectionInfo.ClientPort,
            LoginUserID = -1,
            UserAuthority = -1,
            LogPrefix = "< "
        };

        connection.OnOpen = () =>
            FleckLog.Info($"New connection from {connection_info.IPAddress}:{connection_info.Port}");

        var send = new CanSend(message => FleckLog.Info($"{connection_info.LogPrefix}{message}"));
        send += message => connection.Send(message);

        connection_info.ID = ConnectionInfoManager.AddItem(connection_info);
        connection_info.LogPrefix = $"[{connection_info.ID}]< ";
        connection.OnClose = () => HandleConnectionClose(ref connection_info);
        if (connection_info.ID == -1)
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
        FleckLog.Info($"Closing connection from{info.IPAddress}:{info.Port}");

        info = new ConnectionInfo();

        if (info.ID == -1) return;

        if (!ConnectionInfoManager.RemoveItem(info.ID))
            FleckLog.Error($"[{info.ID}] Cannot remove this connection from connection manager");

        info = new ConnectionInfo();
    }

    private void HandleConnectionMessage(ref ConnectionInfo info, string command, CanSend send)
    {
        if (command.Length == 0)
        {
            const string zeroMessage = "-消息的长度为0";
            send(zeroMessage);
            return;
        }

        FleckLog.Info($"[{info.ID}]> {command}");

        var words = command[1..].Split(' ');
        var prefix = words[0].First();
        words[0] = words[0].ToLower();
        switch (prefix)
        {
            case '!':
                HandleCommand(ref info, words, send);
                return;

            case '?':
                HandleQuery(ref info, words, send);
                return;

            case '#':
                HandleEcho(ref info, words, send);
                return;

            case '+':
                HandleUpdate(ref info, words, send);
                return;

            default:
                send("-未知的前缀");
                return;
        }
    }

    private void RegisterCommands()
    {
        Commands.Add("echo", HandleEcho);
        Commands.Add("query", HandleQuery);
        Commands.Add("login", HandleLogin);
        Commands.Add("update", HandleUpdate);
    }

    private void ReadDataFromFiles()
    {
        FleckLog.Info("Initializing: Reading data from files");

        CommentInfoManager.ReadDataFromFile($"{nameof(CommentInfoManager).Replace("Manager", ".db")}");

        DocumentInfoManager.ReadDataFromFile($"{nameof(DocumentInfoManager).Replace("Manager", ".db")}");

        TaskInfoManager.ReadDataFromFile($"{nameof(TaskInfoManager).Replace("Manager", ".db")}");

        UnitInfoManager.ReadDataFromFile($"{nameof(UnitInfoManager).Replace("Manager", ".db")}");

        UserInfoManager.ReadDataFromFile($"{nameof(UserInfoManager).Replace("Manager", ".db")}");
    }

    private void SaveDataToFiles()
    {
        FleckLog.Info("Saving data to files");

        CommentInfoManager.SaveDataToFile($"{nameof(CommentInfoManager).Replace("Manager", ".db")}");

        DocumentInfoManager.SaveDataToFile($"{nameof(DocumentInfoManager).Replace("Manager", ".db")}");

        TaskInfoManager.SaveDataToFile($"{nameof(TaskInfoManager).Replace("Manager", ".db")}");

        UnitInfoManager.SaveDataToFile($"{nameof(UnitInfoManager).Replace("Manager", ".db")}");

        UserInfoManager.SaveDataToFile($"{nameof(UserInfoManager).Replace("Manager", ".db")}");
    }

    private void HandleCommand(ref ConnectionInfo info, string[] args, CanSend send)
    {
        if (!Commands.ContainsKey(args[0]))
        {
            send($"-未知命令({args[0]})");
            return;
        }

        Commands[args[0]](ref info, args[1..], send);
    }

    private static void HandleEcho(ref ConnectionInfo info, string[] args, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] Request(echo)");
        send(string.Concat(args));
    }

    private void HandleQuery(ref ConnectionInfo info, string[] args, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] Request(query)");
    }

    private void HandleUpdate(ref ConnectionInfo info, string[] args, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] Request(update)");
    }

    private void HandleLogin(ref ConnectionInfo info, string[] args, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] Request(login)");

        if (args.Length < 2)
        {
            send("-错误的登录指令，正确格式：!login <name> <loginValue>");
            return;
        }

        var login_result = UserInfoManager.LoginUser(args[0], args[1]);
        if (login_result < 0)
        {
            send(login_result.ToString());
            return;
        }

        var user_info = new UserInfo();
        if (!UserInfoManager.QueryInfoByID(login_result, ref user_info))
        {
            send("-3");
            return;
        }

        info.LoginUserID = login_result;
        info.UserAuthority = user_info.Authority;
        send(JsonConvert.SerializeObject(user_info));
    }

    private void HandleConsoleInput()
    {
        var input_buffer = new StringBuilder();
        using var output_timer = new Timer();

        output_timer.AutoReset = false;
        output_timer.Interval = 100;
        output_timer.Elapsed += (sender, args) =>
        {
            if (input_buffer.Length > 0)
            {
                FleckLog.Info($">> {input_buffer}");
            }
        };
        output_timer.Start();

        Console.CancelKeyPress += (sender, args) =>
        {
            IsRunning = false;
            args.Cancel = true;
            FleckLog.Info("强制关闭服务器，等待服务端退出");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine("按任意键退出");
        };

        while (IsRunning)
        {
            output_timer.Start();
            var input = Console.ReadKey(true);
            output_timer.Stop();
            if ((input.Modifiers & ConsoleModifiers.Control) != 0)
            {
                if (input.Key == ConsoleKey.C)
                {
                    IsRunning = false;
                }
            }

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

        SaveDataToFiles();
        WebSocketServer.Dispose();
    }

    private void HandleConsoleCommand(string command)
    {
        var words = command.Split(' ');
        var flag = words[0].ToLower();
        if (flag == "quit")
        {
            IsRunning = false;
        }
    }
}