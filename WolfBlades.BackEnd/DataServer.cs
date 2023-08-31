using Fleck;
using WolfBlades.BackEnd.ItemManager;

namespace WolfBlades.BackEnd;

public class DataServer
{
    public enum ManagerType
    {
        Unknown = 0,
        Comment = Unknown + 1,
        Connection = Comment + 1,
        Document = Connection + 1,
        Task = Document + 1,
        Unit = Task + 1,
        User = Unit + 1
    }

    protected OneLineCommand Commands = new();

    public DataServer()
    {
        Initialize();
    }

    public Dictionary<ManagerType, IItemManager> Managers { get; set; } = new();

    ~DataServer()
    {
        Save();
    }

    /*public static ManagerType ParseManagerType(string selector)
    {
        return selector.ToLower() switch
        {
            "comment" => ManagerType.Comment,
            "connection" => ManagerType.Connection,
            "document" => ManagerType.Document,
            "task" => ManagerType.Task,
            "unit" => ManagerType.Unit,
            "user" => ManagerType.User,
            _ => ManagerType.Unknown
        };
    }*/

    private bool Initialize<T>(ManagerType managerType) where T : IItem, new()
    {
        var name = typeof(T).ToString().Replace("Info", "").Split('.')[^1];
        FleckLog.Info($"正在初始化组件: {name}");
        Managers.Add(managerType, new ItemManager<T> { Name = name });
        return Managers[managerType].ReadDataFromFile($"{name}Info.db");
    }

    private bool SaveData<T>(ManagerType managerType) where T : IItem, new()
    {
        FleckLog.Info($"正在保存组件数据: {typeof(T).ToString().Replace("Info", " ")}");
        var manager = Managers[managerType];
        return manager.SaveDataToFile($"{manager.Name}Info.db");
    }

    public void Save()
    {
        SaveData<CommentInfo>(ManagerType.Comment);
        SaveData<ConnectionInfo>(ManagerType.Connection);
        SaveData<DocumentInfo>(ManagerType.Document);
        SaveData<TaskInfo>(ManagerType.Task);
        SaveData<UnitInfo>(ManagerType.Unit);
        SaveData<UserInfo>(ManagerType.User);
    }

    private void RegisterQueryCommands()
    {
        var query_commands = new OneLineCommand()
            .Register("user", new OneLineCommand()
                .Register("id", HandleUserIDQuery)
                .Register("user_name", (connectionID, arg, output) =>
                {
                    var guard = CommandAuthorityGuard(connectionID, output, 1, out _);
                    if (guard < 0) return guard;

                    var item = Managers[ManagerType.User]
                        .SelectItems(item => item is UserInfo user && user.Name == arg);
                    if (item.Length == 0)
                    {
                        output("-找不到用户");
                        return -1;
                    }

                    output(item[0].ToString());
                    return 0;
                }))
            .Register("unit", new OneLineCommand()
                .Register("id", (connectionID, arg, output) =>
                    {
                        var guard = CommandAuthorityGuard(connectionID, output, 1, out _);

                        FleckLog.Info($"[{connectionID}] 请求使用(id)查询(unit)");
                        return guard < 0 ? guard : HandleIDQuery<UnitInfo>(Managers[ManagerType.Unit], arg, output);
                    }
                )
                .Register("unit_group", (connectionID, arg, output) =>
                    {
                        var guard = CommandAuthorityGuard(connectionID, output, 1, out _);
                        if (guard < 0) return guard;


                        FleckLog.Info($"[{connectionID}] 请求使用(unit_group)查询(unit)");
                        var unit_group = arg.ToID();

                        var items = Managers[ManagerType.Unit]
                            .SelectItems(item => item is UnitInfo obj && obj.UnitGroup == unit_group);

                        output(items.ToJsonString());
                        return 0;
                    }
                )
            )
            .Register("task", new OneLineCommand()
                .Register("id", (connectionID, arg, output) =>
                    {
                        var guard = CommandAuthorityGuard(connectionID, output, 1, out _);

                        FleckLog.Info($"[{connectionID}] 请求使用(id)查询(task)");
                        return guard < 0 ? guard : HandleIDQuery<TaskInfo>(Managers[ManagerType.Task], arg, output);
                    }
                )
                .Register("unit",
                    (connectionID, arg, output) =>
                    {
                        var guard = CommandAuthorityGuard(connectionID, output, 1, out _);
                        if (guard < 0) return guard;

                        FleckLog.Info($"[{connectionID}] 请求使用(unit)查询(task)");
                        var unit_id = arg.ToID();
                        var item = Managers[ManagerType.Task]
                            .SelectItems(item => item is TaskInfo obj && obj.BindUnitID == unit_id);

                        output(item.ToJsonString());
                        return 0;
                    }
                )
            )
            .Register("comment", new OneLineCommand()
                .Register("id", (connectionID, arg, output) =>
                    {
                        var guard = CommandAuthorityGuard(connectionID, output, 1, out _);

                        FleckLog.Info($"[{connectionID}] 请求使用(id)查询(comment)");
                        return guard < 0
                            ? guard
                            : HandleIDQuery<CommentInfo>(Managers[ManagerType.Comment], arg, output);
                    }
                )
                .Register("task",
                    (connectionID, arg, output) =>
                    {
                        var guard = CommandAuthorityGuard(connectionID, output, 1, out _);
                        if (guard < 0) return guard;

                        FleckLog.Info($"[{connectionID}] 请求使用(task)查询(comment)");
                        var task_id = arg.ToID();
                        var item = Managers[ManagerType.Comment]
                            .SelectItems(item => item is CommentInfo obj && obj.BindTaskID == task_id);

                        output(item.ToJsonString());
                        return 0;
                    }
                )
            )
            .Register("document", new OneLineCommand()
                .Register("id", (connectionID, arg, output) =>
                {
                    var guard = CommandAuthorityGuard(connectionID, output, 1, out _);

                    FleckLog.Info($"[{connectionID}] 请求使用(id)查询(document)");
                    return guard < 0 ? guard : HandleIDQuery<DocumentInfo>(Managers[ManagerType.Document], arg, output);
                })
                .Register("task", (connectionID, arg, output) =>
                    {
                        var guard = CommandAuthorityGuard(connectionID, output, 1, out _);
                        if (guard < 0) return guard;

                        FleckLog.Info($"[{connectionID}] 请求使用(task)查询(document)");
                        var task_id = arg.ToID();
                        var item = Managers[ManagerType.Comment]
                            .SelectItems(item => item is DocumentInfo obj && obj.RelatedTasks.Contains(task_id));

                        output(item.ToJsonString());
                        return 0;
                    }
                )
            );

        Commands.Register("query", query_commands);
    }

    private void RegisterAppendRemoveUpdateCommands()
    {
        var append_commands = new OneLineCommand()
            .Register("unit",
                (connectionID, arg, output) =>
                    AppendCommandTemplate<UnitInfo>(connectionID, arg, output, 2, Managers[ManagerType.Unit]))
            .Register("user",
                (connectionID, arg, output) =>
                    AppendCommandTemplate<UserInfo>(connectionID, arg, output, 2, Managers[ManagerType.User]))
            .Register("task", AppendTask)
            .Register("document",
                (connectionID, arg, output) =>
                    AppendCommandTemplate<DocumentInfo>(connectionID, arg, output, 1, Managers[ManagerType.Document]))
            .Register("comment",
                (connectionID, arg, output) =>
                    AppendCommandTemplate<CommentInfo>(connectionID, arg, output, 1, Managers[ManagerType.Comment]));

        Commands.Register("append", append_commands);

        var remove_commands = new OneLineCommand()
            .Register("unit",
                (connectionID, arg, output) =>
                    RemoveCommandTemplate<UnitInfo>(connectionID, arg, output, 2, Managers[ManagerType.Unit]))
            .Register("user",
                (connectionID, arg, output) =>
                    RemoveCommandTemplate<UserInfo>(connectionID, arg, output, 2, Managers[ManagerType.User]))
            .Register("task",
                (connectionID, arg, output) =>
                    RemoveCommandTemplate<TaskInfo>(connectionID, arg, output, 2, Managers[ManagerType.Task]))
            .Register("document",
                (connectionID, arg, output) =>
                    RemoveCommandTemplate<DocumentInfo>(connectionID, arg, output, 2, Managers[ManagerType.Document]))
            .Register("comment",
                (connectionID, arg, output) =>
                    RemoveCommandTemplate<CommentInfo>(connectionID, arg, output, 2, Managers[ManagerType.Comment]));

        Commands.Register("remove", remove_commands);

        var update_commands = new OneLineCommand()
            .Register("unit",
                (connectionID, arg, output) =>
                    UpdateCommandTemplate<UnitInfo>(connectionID, arg, output, 2, Managers[ManagerType.Unit]))
            .Register("user",
                (connectionID, arg, output) =>
                    UpdateCommandTemplate<UserInfo>(connectionID, arg, output, 2, Managers[ManagerType.User]))
            .Register("task",
                (connectionID, arg, output) =>
                    UpdateCommandTemplate<TaskInfo>(connectionID, arg, output, 1, Managers[ManagerType.Task]))
            .Register("document",
                (connectionID, arg, output) =>
                    UpdateCommandTemplate<DocumentInfo>(connectionID, arg, output, 1, Managers[ManagerType.Document]))
            .Register("comment",
                (connectionID, arg, output) =>
                    UpdateCommandTemplate<CommentInfo>(connectionID, arg, output, 1, Managers[ManagerType.Comment]));

        Commands.Register("update", update_commands);
    }

    private int AppendCommandTemplate<T>(int connectionID, string arg, Action<string> output, int authority,
        IItemManager manager) where T : IItem, new()
    {
        var guard = CommandAuthorityGuard(connectionID, output, authority, out _);
        if (guard < 0) return guard;

        FleckLog.Info($"[{connectionID}] 请求添加({typeof(T).ToString().Replace("Info", "").ToLower()})");

        var item = new T();
        item.ReadFrom(arg);
        var id = manager.AppendItem(item);
        if (id < 0)
        {
            output("-添加失败，系统内部错误");
            return -1;
        }

        output(id.ToString());
        return 0;
    }

    private int AppendTask(int connectionID, string arg, Action<string> output)
    {
        var guard = CommandAuthorityGuard(connectionID, output, 1, out _);
        if (guard < 0) return guard;

        FleckLog.Info($"[{connectionID}] 请求添加(task)");

        var task_info = new TaskInfo();
        task_info.ReadFrom(arg);
        var task_id = Managers[ManagerType.Task].AppendItem(task_info);
        if (task_id < 0)
        {
            output("-添加失败，系统内部错误");
            return -1;
        }
        
        var document_info = new DocumentInfo();
        document_info.RelatedTasks.Add(task_id);
        var document_id = Managers[ManagerType.Document].AppendItem(document_info);

        if (document_id < 0)
        {
            output("-添加失败，系统内部错误");
            return -1;
        }

        task_info.DocumentID = document_id;
        Managers[ManagerType.Task].UpdateItem(task_id, task_info);

        output(task_id.ToString());
        return 0;
    }

    private int RemoveCommandTemplate<T>(int connectionID, string arg, Action<string> output, int authority,
        IItemManager manager) where T : IItem, new()
    {
        var guard = CommandAuthorityGuard(connectionID, output, authority, out _);
        if (guard < 0) return guard;

        FleckLog.Info($"[{connectionID}] 请求移除({typeof(T).ToString().Replace("Info", "").ToLower()})");

        var item_id = arg.ToID();
        if (!manager.RemoveItem(item_id))
        {
            output("-无法移除目标");
            return -1;
        }

        output(item_id.ToString());
        return 0;
    }

    private int UpdateCommandTemplate<T>(int connectionID, string arg, Action<string> output, int authority,
        IItemManager manager) where T : IItem, new()
    {
        var guard = CommandAuthorityGuard(connectionID, output, authority, out _);
        if (guard < 0) return guard;

        FleckLog.Info($"[{connectionID}] 请求更新({typeof(T).ToString().Replace("Info", "").ToLower()})");

        var space_index = arg.IndexOf(' ');
        var flag = (space_index == -1 ? arg : arg[..space_index]).ToLower();
        var content_left = arg[(space_index + 1)..];

        var item_id = flag.ToID();
        var item = new T();
        item.ReadFrom(content_left);

        if (manager.UpdateItem(item_id, item))
        {
            output(item.ToJsonString());
            return 0;
        }

        output("-更新失败");
        return -1;
    }

    private void RegisterCommands()
    {
        FleckLog.Info("数据服务器正在加载命令");
        Commands
            .Register("login", HandleLogin)
            .Register("echo", (connectionID, arg, output) =>
            {
                var guard = CommandAuthorityGuard(connectionID, output, 1, out _);
                if (guard < 0) return guard;

                FleckLog.Info($"[{connectionID}] 请求(echo)");

                output(arg);
                return 0;
            });

        RegisterQueryCommands();
        RegisterAppendRemoveUpdateCommands();
    }

    private void Initialize()
    {
        FleckLog.Info("正在尝试初始化数据服务器");
        if (!Initialize<CommentInfo>(ManagerType.Comment) ||
            !Initialize<ConnectionInfo>(ManagerType.Connection) ||
            !Initialize<DocumentInfo>(ManagerType.Document) ||
            !Initialize<TaskInfo>(ManagerType.Task) ||
            !Initialize<UnitInfo>(ManagerType.Unit) ||
            !Initialize<UserInfo>(ManagerType.User))
            throw new Exception("无法完成初始化，停止启动数据服务器"); //TODO 解析完整数据

        RegisterCommands();
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        Managers[ManagerType.Task].WhenAppendItem += (sender, arg) =>
        {
            if (arg.Item is not TaskInfo obj || obj.BindUnitID == -1)
                return;

            if (Managers[ManagerType.Unit].GetItem(obj.BindUnitID) is not UnitInfo unit ||
                !unit.InProgressTasks.Contains(obj.ID))
            {
                return;
            }

            unit.InProgressTasks.Add(obj.ID);
        };

        Managers[ManagerType.Task].WhenRemoveItem += (sender, arg) =>
        {
            if (arg.Item is not TaskInfo obj
                || obj.BindUnitID == -1
                || Managers[ManagerType.Unit].GetItem(obj.BindUnitID) is not UnitInfo unit
                || !unit.InProgressTasks.Contains(obj.ID))
                return;

            unit.InProgressTasks.Remove(obj.ID);
        };

        Managers[ManagerType.Task].WhenUpdateItem += (sender, arg) =>
        {
            if (arg.Item is not TaskInfo obj
                || obj.BindUnitID == -1
                || Managers[ManagerType.Unit].GetItem(obj.BindUnitID) is not UnitInfo unit
                || !unit.InProgressTasks.Contains(obj.ID))
                return;

            unit.InProgressTasks.Add(obj.ID);
        };




    }

    private int CommandAuthorityGuard(int connectionID, Action<string> output, int authority,
        out ConnectionInfo connectionInfo)
    {
        connectionInfo = new ConnectionInfo();
        if (Managers[ManagerType.Connection].GetItem(connectionID) is not ConnectionInfo connection_info ||
            connection_info.ID == -1)
        {
            output("-服务器拒绝了来自异常连接的指令");
            return -1;
        }

        connectionInfo.ReadFrom(connection_info);
        if (connection_info.UserAuthority >= authority) return 0;

        output("-服务器拒绝了指令：用户权限不足");
        return -2;
    }

    private int HandleLogin(int connectionID, string arg, Action<string> output)
    {
        var guard = CommandAuthorityGuard(connectionID, output, 0, out var connection_info);
        if (guard < 0) return guard;

        FleckLog.Info($"[{connectionID}] 请求(login)");

        var args = arg.Split(' ');

        connection_info.LoginUserID = -1;
        connection_info.UserAuthority = 0;

        if (args.Length < 2)
        {
            output("-错误的登录指令，正确格式：!login <name> <loginValue>");
            return -2;
        }

        var user_name = args[0];
        var login_value = args[1];

        UserInfo? target_user = null;

        var user_info_list = Managers[ManagerType.User].SelectItems(item =>
        {
            if (item is not UserInfo user || user.Name != user_name) return false;
            target_user = user;
            return true;
        });

        if (user_info_list.Length == 0 || target_user is null)
        {
            output("-用户不存在");
            return -1;
        }

        if (user_info_list.Length > 1)
        {
            output("-系统内部错误");
            return -1;
        }

        bool succeeded;
        if (login_value == target_user.Password)
        {
            IgnoreSeSc.GenerateTokenForUser(target_user);
            succeeded = true;
        }
        else
        {
            if (login_value == target_user.Token &&
                DateTime.Now > target_user.TokenGeneratedTime.ConvertToDateTime() + target_user.TokenTimeBeforeExpire)
                succeeded = true;
            else
                succeeded = false;
        }

        if (succeeded)
        {
            var user = new UserInfo();
            user.ReadFrom(target_user);
            user.Password = "Blocked";
            user.TokenGeneratedTime = TinyConverterExtension.ConvertToString(DateTime.MinValue);
            user.TokenTimeBeforeExpire = TimeSpan.Zero;

            connection_info.LoginUserID = user.ID;
            connection_info.UserAuthority = user.Authority;
            Managers[ManagerType.Connection].UpdateItem(connectionID, connection_info);

            output(user.ToJsonString());
            return user.ID;
        }

        output("-2"); //TODO 登录失败
        return -1;
    }

    private int HandleUserIDQuery(int connectionID, string arg, Action<string> output)
    {
        var guard = CommandAuthorityGuard(connectionID, output, 1, out var connection_info);
        if (guard < 0) return guard;

        FleckLog.Info($"[{connectionID}] 请求查询(user)");

        var id = arg.ToID();
        if (id == -1)
        {
            output("-无法解析id");
            return -1;
        }

        if (Managers[ManagerType.User].GetItem(id) is not UserInfo target_user || target_user.ID == -1)
        {
            output("-用户不存在");
            return -2;
        }

        var user = new UserInfo();
        user.ReadFrom(target_user);
        if (connection_info.UserAuthority < 2)
        {
            user.Password = "Blocked";
            user.TokenGeneratedTime = TinyConverterExtension.ConvertToString(DateTime.MinValue);
            user.TokenTimeBeforeExpire = TimeSpan.Zero;
        }

        output(user.ToJsonString());
        return 0;
    }

    private int HandleIDQuery<T>(IItemManager manager, string arg, Action<string> output) where T : class, IItem
    {
        var id = arg.ToID();
        if (id == -1)
        {
            output("-无法解析id");
            return -1;
        }

        if (manager.GetItem(id) is not T item || item.ID == -1)
        {
            output("-目标不存在");
            return -2;
        }

        output(item.ToJsonString());
        return 0;
    }

    public int HandleCommand(int connectionID, string command, Action<string> output)
    {
        FleckLog.Info($"[{connectionID}]> {command}");

        if (command.Length == 0) return -1;

        var value = command[1..];
        var new_prefix = command[0] switch
        {
            '!' => "",
            '?' => "query ",
            '+' => "append ",
            '-' => "remove ",
            '*' => "update ",
            '#' => "echo ",
            _ => null
        };

        if (new_prefix != null) return Commands.Execute(connectionID, $"{new_prefix}{value}", output);

        output("-未知的前缀");
        return -1;
    }
}