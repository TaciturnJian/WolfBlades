using System.Text;
using Fleck;
using Newtonsoft.Json;
using WolfBlades.BackEnd;
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
    public delegate void ConnectionCommandDelegate(ref ConnectionInfo info, string arg, CanSend send);

    protected Dictionary<string, ConnectionCommandDelegate> AddCommands;

    protected Dictionary<string, ConnectionCommandDelegate> Commands;

    protected Dictionary<string, ConnectionCommandDelegate> QueryCommands;

    protected Dictionary<string, ConnectionCommandDelegate> RemoveCommands;

    protected Dictionary<string, ConnectionCommandDelegate> UpdateCommands;

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
        QueryCommands = new Dictionary<string, ConnectionCommandDelegate>();
        UpdateCommands = new Dictionary<string, ConnectionCommandDelegate>();
        AddCommands = new Dictionary<string, ConnectionCommandDelegate>();
        RemoveCommands = new Dictionary<string, ConnectionCommandDelegate>();
        RegisterCommands();

        ReadDataFromFiles();

        FleckLog.Info($"Initializing: {nameof(WebSocketServer).Replace("WebSocket", "")}");
        WebSocketServer = new WebSocketServer(Location);
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

    ~Server()
    {
        WebSocketServer.Dispose();
    }

    private static int HandleParseID(string arg, CanSend send)
    {
        if (int.TryParse(arg, out var id)) return id;

        send("-2");
        return -2;
    }

    private void RegisterCommands()
    {
        Commands.Add("echo", HandleEcho);
        Commands.Add("query", HandleQuery);
        Commands.Add("login", HandleLogin);
        Commands.Add("update", HandleUpdate);
        Commands.Add("append", HandleAppend);
        Commands.Add("remove", HandleRemove);

        QueryCommands.Add("unit", HandleQueryUnit);
        QueryCommands.Add("user", HandleQueryUser);
        QueryCommands.Add("task", HandleQueryTask);
        QueryCommands.Add("comment", HandleQueryComment);
        QueryCommands.Add("document", HandleQueryDocument);
        QueryCommands.Add("connection", HandleQueryConnection);

        UpdateCommands.Add("unit",
            (ref ConnectionInfo _, string arg, CanSend send) => HandleUpdateTemplate(UnitInfoManager, arg, send));
        UpdateCommands.Add("task",
            (ref ConnectionInfo _, string arg, CanSend send) => HandleUpdateTemplate(TaskInfoManager, arg, send));
        UpdateCommands.Add("comment",
            (ref ConnectionInfo _, string arg, CanSend send) => HandleUpdateTemplate(CommentInfoManager, arg, send));
        UpdateCommands.Add("document",
            (ref ConnectionInfo _, string arg, CanSend send) => HandleUpdateTemplate(DocumentInfoManager, arg, send));

        AddCommands.Add("unit",
            (ref ConnectionInfo _, string arg, CanSend send) => HandleAppendTemplate(UnitInfoManager, arg, send));
        AddCommands.Add("task",
            (ref ConnectionInfo _, string arg, CanSend send) => HandleAppendTemplate(TaskInfoManager, arg, send));
        AddCommands.Add("comment",
            (ref ConnectionInfo _, string arg, CanSend send) => HandleAppendTemplate(CommentInfoManager, arg, send));
        AddCommands.Add("document",
            (ref ConnectionInfo _, string arg, CanSend send) => HandleAppendTemplate(DocumentInfoManager, arg, send));

        RemoveCommands.Add("task",
            (ref ConnectionInfo _, string arg, CanSend send) => HandleRemoveTemplate(TaskInfoManager, arg, send));
        RemoveCommands.Add("comment",
            (ref ConnectionInfo _, string arg, CanSend send) => HandleRemoveTemplate(CommentInfoManager, arg, send));
        RemoveCommands.Add("document",
            (ref ConnectionInfo _, string arg, CanSend send) => HandleRemoveTemplate(DocumentInfoManager, arg, send));
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

    private void HandleCommand(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 请求(command)");

        if (arg.Length == 0)
        {
            send("-未提供命令");
            return;
        }

        var space_index = arg.IndexOf(' ');
        var flag = space_index == -1 ? arg : arg[..space_index];

        if (!Commands.ContainsKey(flag))
        {
            send($"-未知命令({flag})");
            return;
        }

        Commands[flag](ref info, arg[(space_index + 1)..], send);
    }

    private static void HandleEcho(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 请求(echo)");

        if (info.UserAuthority < 1)
        {
            send("-1");
            return;
        }

        send(arg);
    }

    private void HandleLogin(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 请求(login)");

        var args = arg.Split(' ');

        info.LoginUserID = -1;
        info.UserAuthority = 0;

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

    #region Update

    private void HandleUpdate(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 请求(update)");

        if (info.UserAuthority < 2)
        {
            send("-1");
            return;
        }

        if (arg.Length == 0)
        {
            send("-未提供更新目标");
            return;
        }

        var space_index = arg.IndexOf(' ');
        var target = space_index == -1 ? arg : arg[..space_index];

        if (!UpdateCommands.ContainsKey(target))
        {
            send($"-未知更新目标({target})");
            return;
        }

        UpdateCommands[target](ref info, arg[(space_index + 1)..], send);
    }

    private static void HandleUpdateTemplate<TStorage, TQuery>(IDataManager<TStorage, TQuery> manager, string arg,
        CanSend send)
        where TStorage : class, IDataStorage<TQuery>, new()
        where TQuery : struct
    {
        var space_index = arg.IndexOf(' ');
        var id_string = space_index == -1 ? arg : arg[..space_index];
        var value = arg[(space_index + 1)..];
        var id = HandleParseID(id_string, send);
        var query_info = JsonConvert.DeserializeObject<TQuery>(value);
        if (!manager.UpdateItem(id, query_info))
        {
            id = -1;
        }
        send(id.ToString());
    }

    #endregion

    #region Connection

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
            FleckLog.Info($"建立新连接 {connection_info.IPAddress}:{connection_info.Port}");

        var send = new CanSend(message => FleckLog.Info($"{connection_info.LogPrefix}{message}"));
        send += message => connection.Send(message);

        connection_info.ID = ConnectionInfoManager.AddItem(connection_info);
        connection_info.LogPrefix = $"[{connection_info.ID}]< ";
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

        info = new ConnectionInfo();

        if (info.ID < 0) return;

        if (!ConnectionInfoManager.RemoveItem(info.ID))
            FleckLog.Error($"[{info.ID}] 无法从连接管理器移除此连接的信息");

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

        switch (command.First())
        {
            case '!':
                HandleCommand(ref info, command[1..], send);
                return;

            case '?':
                HandleQuery(ref info, command[1..], send);
                return;

            case '#':
                HandleEcho(ref info, command[1..], send);
                return;

            case '*':
                HandleUpdate(ref info, command[1..], send);
                return;

            case '+':
                HandleAppend(ref info, command[1..], send);
                return;

            case '-':
                HandleRemove(ref info, command[1..], send);
                return;

            default:
                send("-未知的前缀");
                return;
        }
    }

    #endregion

    #region Console

    private void HandleConsoleInput()
    {
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

                case ConsoleKey.Tab:
                case ConsoleKey.Clear:
                case ConsoleKey.Pause:
                case ConsoleKey.Escape:
                case ConsoleKey.Spacebar:
                case ConsoleKey.PageUp:
                case ConsoleKey.PageDown:
                case ConsoleKey.End:
                case ConsoleKey.Home:
                case ConsoleKey.LeftArrow:
                case ConsoleKey.UpArrow:
                case ConsoleKey.RightArrow:
                case ConsoleKey.DownArrow:
                case ConsoleKey.Select:
                case ConsoleKey.Print:
                case ConsoleKey.Execute:
                case ConsoleKey.PrintScreen:
                case ConsoleKey.Insert:
                case ConsoleKey.Delete:
                case ConsoleKey.Help:
                case ConsoleKey.D0:
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                case ConsoleKey.D5:
                case ConsoleKey.D6:
                case ConsoleKey.D7:
                case ConsoleKey.D8:
                case ConsoleKey.D9:
                case ConsoleKey.A:
                case ConsoleKey.B:
                case ConsoleKey.C:
                case ConsoleKey.D:
                case ConsoleKey.E:
                case ConsoleKey.F:
                case ConsoleKey.G:
                case ConsoleKey.H:
                case ConsoleKey.I:
                case ConsoleKey.J:
                case ConsoleKey.K:
                case ConsoleKey.L:
                case ConsoleKey.M:
                case ConsoleKey.N:
                case ConsoleKey.O:
                case ConsoleKey.P:
                case ConsoleKey.Q:
                case ConsoleKey.R:
                case ConsoleKey.S:
                case ConsoleKey.T:
                case ConsoleKey.U:
                case ConsoleKey.V:
                case ConsoleKey.W:
                case ConsoleKey.X:
                case ConsoleKey.Y:
                case ConsoleKey.Z:
                case ConsoleKey.LeftWindows:
                case ConsoleKey.RightWindows:
                case ConsoleKey.Applications:
                case ConsoleKey.Sleep:
                case ConsoleKey.NumPad0:
                case ConsoleKey.NumPad1:
                case ConsoleKey.NumPad2:
                case ConsoleKey.NumPad3:
                case ConsoleKey.NumPad4:
                case ConsoleKey.NumPad5:
                case ConsoleKey.NumPad6:
                case ConsoleKey.NumPad7:
                case ConsoleKey.NumPad8:
                case ConsoleKey.NumPad9:
                case ConsoleKey.Multiply:
                case ConsoleKey.Add:
                case ConsoleKey.Separator:
                case ConsoleKey.Subtract:
                case ConsoleKey.Decimal:
                case ConsoleKey.Divide:
                case ConsoleKey.F1:
                case ConsoleKey.F2:
                case ConsoleKey.F3:
                case ConsoleKey.F4:
                case ConsoleKey.F5:
                case ConsoleKey.F6:
                case ConsoleKey.F7:
                case ConsoleKey.F8:
                case ConsoleKey.F9:
                case ConsoleKey.F10:
                case ConsoleKey.F11:
                case ConsoleKey.F12:
                case ConsoleKey.F13:
                case ConsoleKey.F14:
                case ConsoleKey.F15:
                case ConsoleKey.F16:
                case ConsoleKey.F17:
                case ConsoleKey.F18:
                case ConsoleKey.F19:
                case ConsoleKey.F20:
                case ConsoleKey.F21:
                case ConsoleKey.F22:
                case ConsoleKey.F23:
                case ConsoleKey.F24:
                case ConsoleKey.BrowserBack:
                case ConsoleKey.BrowserForward:
                case ConsoleKey.BrowserRefresh:
                case ConsoleKey.BrowserStop:
                case ConsoleKey.BrowserSearch:
                case ConsoleKey.BrowserFavorites:
                case ConsoleKey.BrowserHome:
                case ConsoleKey.VolumeMute:
                case ConsoleKey.VolumeDown:
                case ConsoleKey.VolumeUp:
                case ConsoleKey.MediaNext:
                case ConsoleKey.MediaPrevious:
                case ConsoleKey.MediaStop:
                case ConsoleKey.MediaPlay:
                case ConsoleKey.LaunchMail:
                case ConsoleKey.LaunchMediaSelect:
                case ConsoleKey.LaunchApp1:
                case ConsoleKey.LaunchApp2:
                case ConsoleKey.Oem1:
                case ConsoleKey.OemPlus:
                case ConsoleKey.OemComma:
                case ConsoleKey.OemMinus:
                case ConsoleKey.OemPeriod:
                case ConsoleKey.Oem2:
                case ConsoleKey.Oem3:
                case ConsoleKey.Oem4:
                case ConsoleKey.Oem5:
                case ConsoleKey.Oem6:
                case ConsoleKey.Oem7:
                case ConsoleKey.Oem8:
                case ConsoleKey.Oem102:
                case ConsoleKey.Process:
                case ConsoleKey.Packet:
                case ConsoleKey.Attention:
                case ConsoleKey.CrSel:
                case ConsoleKey.ExSel:
                case ConsoleKey.EraseEndOfFile:
                case ConsoleKey.Play:
                case ConsoleKey.Zoom:
                case ConsoleKey.NoName:
                case ConsoleKey.Pa1:
                case ConsoleKey.OemClear:
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
        FleckLog.Info($"服务器控制台输入命令: \"{command}\"");

        var space_index = command.IndexOf(' ');
        var flag = (space_index == -1 ? command : command[..space_index]).ToLower();
        if (flag == "quit")
        {
            IsRunning = false;
            return;
        }

        if (flag == "save") SaveDataToFiles();
    }

    #endregion

    #region Query

    private void HandleQuery(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 请求(query)");

        if (info.UserAuthority < 1)
        {
            send("-1");
            return;
        }

        if (arg.Length == 0)
        {
            send("-未提供查询对象");
            return;
        }

        var space_index = arg.IndexOf(' ');
        var target = space_index == -1 ? arg : arg[..space_index];

        if (!QueryCommands.ContainsKey(target))
        {
            send($"-未知查询对象({target})");
            return;
        }

        QueryCommands[target](ref info, arg[(space_index + 1)..], send);
    }

    private void HandleQueryUnit(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 查询(unit)");

        if (info.UserAuthority < 1)
        {
            send("-1");
            return;
        }

        var space_index = arg.IndexOf(' ');
        var selector = space_index == -1 ? arg : arg[..space_index];
        var value = arg[(space_index + 1)..];
        switch (selector)
        {
            case "id":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var unit_info = new UnitInfo();
                if (!UnitInfoManager.QueryInfoByID(id, ref unit_info))
                {
                    send("-3");
                    return;
                }

                send(JsonConvert.SerializeObject(unit_info));
                return;
            }
            case "name":
            {
                var ids = UnitInfoManager.QueryIDListBySelector(unit => unit.Name == value);
                if (ids.Length == 0)
                {
                    send("-2");
                    return;
                }

                send(JsonConvert.SerializeObject(ids));
                return;
            }
            case "unit_group":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var ids = UnitInfoManager.QueryIDListBySelector(unit => unit.UnitGroup == id);
                if (ids.Length == 0)
                {
                    send("-2");
                    return;
                }

                send(JsonConvert.SerializeObject(ids));
                return;
            }
        }
    }

    private void HandleQueryUser(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 查询(user)");

        if (info.UserAuthority < 2)
        {
            send("-1");
            return;
        }

        var space_index = arg.IndexOf(' ');
        var selector = space_index == -1 ? arg : arg[..space_index];
        var value = arg[(space_index + 1)..];
        switch (selector)
        {
            case "id":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var user_info = new UserInfo();
                if (!UserInfoManager.QueryInfoByID(id, ref user_info))
                {
                    send("-3");
                    return;
                }

                send(JsonConvert.SerializeObject(user_info));
                return;
            }
            case "name":
            {
                var ids = UserInfoManager.QueryIDListBySelector(user => user.Name == value);
                if (ids.Length == 0)
                {
                    send("-2");
                    return;
                }

                send(JsonConvert.SerializeObject(ids));
                return;
            }
            case "tech_group":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var ids = UserInfoManager.QueryIDListBySelector(user => user.TechGroup.Contains(id));
                if (ids.Length == 0)
                {
                    send("-2");
                    return;
                }

                send(JsonConvert.SerializeObject(ids));
                return;
            }
            case "unit_group":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var ids = UserInfoManager.QueryIDListBySelector(user => user.UnitGroup.Contains(id));
                if (ids.Length == 0)
                {
                    send("-2");
                    return;
                }

                send(JsonConvert.SerializeObject(ids));
                return;
            }
        }
    }

    private void HandleQueryTask(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 查询(task)");

        if (info.UserAuthority < 1)
        {
            send("-1");
            return;
        }

        var space_index = arg.IndexOf(' ');
        var selector = space_index == -1 ? arg : arg[..space_index];
        var value = arg[(space_index + 1)..];
        switch (selector)
        {
            case "id":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var task_info = new TaskInfo();
                if (!TaskInfoManager.QueryInfoByID(id, ref task_info))
                {
                    send("-3");
                    return;
                }

                send(JsonConvert.SerializeObject(task_info));
                return;
            }
            case "unit_id":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var ids = TaskInfoManager.QueryIDListBySelector(task => task.BindUnitID == id);
                if (ids.Length == 0)
                {
                    send("-2");
                    return;
                }

                send(JsonConvert.SerializeObject(ids));
                return;
            }
        }
    }

    private void HandleQueryComment(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 查询(comment)");

        if (info.UserAuthority < 1)
        {
            send("-1");
            return;
        }

        var space_index = arg.IndexOf(' ');
        var selector = space_index == -1 ? arg : arg[..space_index];
        var value = arg[(space_index + 1)..];
        switch (selector)
        {
            case "id":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var comment_info = new CommentInfo();
                if (!CommentInfoManager.QueryInfoByID(id, ref comment_info))
                {
                    send("-3");
                    return;
                }

                send(JsonConvert.SerializeObject(comment_info));
                return;
            }
            case "task_id":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var ids = CommentInfoManager.QueryIDListBySelector(comment => comment.BindTaskID == id);
                if (ids.Length == 0)
                {
                    send("-2");
                    return;
                }

                send(JsonConvert.SerializeObject(ids));
                return;
            }
        }
    }

    private void HandleQueryDocument(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 查询(document)");

        if (info.UserAuthority < 1)
        {
            send("-1");
            return;
        }

        var space_index = arg.IndexOf(' ');
        var selector = space_index == -1 ? arg : arg[..space_index];
        var value = arg[(space_index + 1)..];
        switch (selector)
        {
            case "id":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var document_info = new DocumentInfo();
                if (!DocumentInfoManager.QueryInfoByID(id, ref document_info))
                {
                    send("-3");
                    return;
                }

                send(JsonConvert.SerializeObject(document_info));
                return;
            }
            case "task_id":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var ids = DocumentInfoManager.QueryIDListBySelector(document => document.RelatedTasks.Contains(id));
                if (ids.Length == 0)
                {
                    send("-2");
                    return;
                }

                send(JsonConvert.SerializeObject(ids));
                return;
            }
            default:
                send("-未知的目标选择器");
                return;
        }
    }

    private void HandleQueryConnection(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 查询(connection)");

        if (info.UserAuthority < 2) send("-1");

        var space_index = arg.IndexOf(' ');
        var selector = space_index == -1 ? arg : arg[..space_index];
        var value = arg[(space_index + 1)..];
        switch (selector)
        {
            case "id":
            {
                var id = HandleParseID(value, send);
                if (id < 0) return;

                var document_info = new DocumentInfo();
                if (!DocumentInfoManager.QueryInfoByID(id, ref document_info))
                {
                    send("-3");
                    return;
                }

                send(JsonConvert.SerializeObject(document_info));
                return;
            }
        }
    }

    #endregion

    #region Append

    private void HandleAppend(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 请求(append)");

        if (info.UserAuthority < 1)
        {
            send("-1");
            return;
        }

        if (arg.Length == 0)
        {
            send("-未提供添加目标");
            return;
        }

        var space_index = arg.IndexOf(' ');
        var target = space_index == -1 ? arg : arg[..space_index];

        if (!AddCommands.ContainsKey(target))
        {
            send($"-未知添加目标({target})");
            return;
        }

        AddCommands[target](ref info, arg[(space_index + 1)..], send);
    }

    private static void HandleAppendTemplate<TStorage, TQuery>(IDataManager<TStorage, TQuery> manager, string arg,
        CanSend send)
        where TStorage : class, IDataStorage<TQuery>, new()
        where TQuery : struct
    {
        var query_info = JsonConvert.DeserializeObject<TQuery>(arg);
        var id = manager.AddItem(query_info);
        send(id.ToString());
    }

    #endregion

    #region Remove

    private void HandleRemove(ref ConnectionInfo info, string arg, CanSend send)
    {
        FleckLog.Info($"[{info.ID}] 请求(remove)");

        if (info.UserAuthority < 2)
        {
            send("-1");
            return;
        }

        if (arg.Length == 0)
        {
            send("-未提供移除目标");
            return;
        }

        var space_index = arg.IndexOf(' ');
        var target = space_index == -1 ? arg : arg[..space_index];

        if (!RemoveCommands.ContainsKey(target))
        {
            send($"-未知移除目标({target})");
            return;
        }

        RemoveCommands[target](ref info, arg[(space_index + 1)..], send);
    }

    private static void HandleRemoveTemplate<TStorage, TQuery>(IDataManager<TStorage, TQuery> manager, string arg,
        CanSend send)
        where TStorage : class, IDataStorage<TQuery>, new()
        where TQuery : struct
    {
        var id = HandleParseID(arg, send);
        if (id < 0)
        {
            send(id.ToString());
            return;
        }

        send(manager.RemoveItem(id) ? id.ToString() : "-2");
    }

    #endregion
}