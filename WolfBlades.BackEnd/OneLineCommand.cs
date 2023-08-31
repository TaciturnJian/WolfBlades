namespace WolfBlades.BackEnd;

public class OneLineCommand
{
    public Dictionary<string, OneLineCommand>? ChildCommands;

    public Func<int, string, Action<string>, int>? SelfCommand;

    public OneLineCommand()
    {
    }

    public OneLineCommand(Func<int, string, Action<string>, int> selfCommand)
    {
        SelfCommand = selfCommand;
    }

    public int Execute(int id, string arg, Action<string> output)
    {
        if (ChildCommands is null) return SelfCommand?.Invoke(id, arg, output) ?? -1;

        var space_index = arg.IndexOf(' ');
        var flag = (space_index == -1 ? arg : arg[..space_index]).ToLower();
        var content_left = arg[(space_index + 1)..];
        foreach (var (key, command) in ChildCommands)
            if (key == flag)
                return command.Execute(id, content_left, output);

        output("-未知命令");
        return -1;
    }

    public OneLineCommand Register(string flag, OneLineCommand command)
    {
        ChildCommands ??= new Dictionary<string, OneLineCommand>();
        ChildCommands.Add(flag.ToLower(), command);
        return this;
    }

    public OneLineCommand Register(string flag, Func<int, string, Action<string>, int> command)
    {
        Register(flag, new OneLineCommand(command));
        return this;
    }

    public static implicit operator Func<int, string, Action<string>, int>(OneLineCommand command)
    {
        return command.Execute;
    }
}