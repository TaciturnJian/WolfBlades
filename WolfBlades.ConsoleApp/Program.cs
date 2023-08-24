﻿using Fleck;

namespace WolfBlades.ConsoleApp;

public static class Program
{
    public static readonly string ApplicationName = "WolfBlades: UnitMonitor";

    public static readonly string ApplicationVersion = "1.0.0";

    public static string Location { get; private set; } = "127.0.0.1:6363";

    public static ProgramStartInfo ParseProgramArguments(string[] args)
    {
        const string pleaseCallWithFormat = "请使用如下格式在终端调用此程序\n./程序名 <IP地址> <端口号> [-client]";
        const string pleaseProvidePort = $"未指定端口号，{pleaseCallWithFormat}";

        var info = new ProgramStartInfo();

        if (args.Length < 2)
        {
            info.Succeeded = false;
            info.ResultMessage = args.Length < 2 ? pleaseProvidePort : pleaseCallWithFormat;
            return info;
        }

        info.IPAddress = args[0];

        if (!int.TryParse(args[1], out info.Port))
        {
            info.Succeeded = false;
            info.ResultMessage = $"从字符串({args[1]})中解析端口时出现异常";
            return info;
        }

        Location = $"{info.IPAddress}:{info.Port}";
        info.Succeeded = true;

        if (args.Length > 2)
            if (args[2] == "-client")
            {
                info.IsClient = true;
                info.ResultMessage = $"正在尝试开启客户端({Location})";
                return info;
            }

        info.IsClient = false;
        info.ResultMessage = $"正在尝试开启服务器({Location})";

        return info;
    }

    public static void WaitExit()
    {
        try
        {
            WhenProgramExit?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        Console.WriteLine("程序已终止，等待任意输入内容后退出");
        Console.ReadLine();
    }

    public static event Action? WhenProgramExit;

    private static int Main(string[] args)
    {
        var start_info = ParseProgramArguments(args);
        Console.WriteLine(start_info.ResultMessage);
        if (!start_info.Succeeded)
        {
            WaitExit();
            return -1;
        }

        FleckLog.Info($"#{ApplicationName} v{ApplicationVersion}");
        ICanStart some_can_start = start_info.IsClient ? new Client() : new Server();
        some_can_start.Start();
        return 0;
    }

    public struct ProgramStartInfo
    {
        public string IPAddress = "";
        public int Port = 0;

        public bool IsClient = false;

        public string ResultMessage = "";
        public bool Succeeded = false;

        public ProgramStartInfo()
        {
        }
    }
}