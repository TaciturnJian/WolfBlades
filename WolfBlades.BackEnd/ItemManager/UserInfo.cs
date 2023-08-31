namespace WolfBlades.BackEnd.ItemManager;

/// <summary>
///     系统中储存的用户信息
/// </summary>
public class UserInfo : IItem
{
    /// <summary>
    ///     用户登录时所需的用户密码
    /// </summary>
    public string Password = "user_password_for_login";

    /// <summary>
    ///     用户口令生成的时间
    /// </summary>
    public DateTime TokenGeneratedTime = DateTime.Now;

    /// <summary>
    ///     用户登录口令的持续时间
    /// </summary>
    public TimeSpan TokenTimeBeforeExpire = TimeSpan.FromDays(1);

    /// <summary>
    ///     用户名，用于用户登录
    /// </summary>
    public string Name { get; set; } = "user_name_for_login";

    /// <summary>
    ///     用户显示名称
    /// </summary>
    public string DisplayName { get; set; } = "user_display_name(or nickname)";

    /// <summary>
    ///     用户所在的技术组
    /// </summary>
    public List<int> TechGroup { get; set; } = new();

    /// <summary>
    ///     用户所在的单位组
    /// </summary>
    public List<int> UnitGroup { get; set; } = new();

    /// <summary>
    ///     用户权限
    /// </summary>
    public int Authority { get; set; } = -1;

    /// <summary>
    ///     用户登录口令，用于快捷登录
    /// </summary>
    public string Token { get; set; } = "token_value";

    public int ID { get; set; } = -1;

    public void ReadFrom(IItem item)
    {
        if (item is not UserInfo obj) return;

        Password = obj.Password;
        TokenGeneratedTime = obj.TokenGeneratedTime;
        TokenTimeBeforeExpire = obj.TokenTimeBeforeExpire;
        Name = obj.Name;
        DisplayName = obj.DisplayName;
        TechGroup = new List<int>(obj.TechGroup);
        UnitGroup = new List<int>(obj.UnitGroup);
        Authority = obj.Authority;
        Token = obj.Token;
        ID = obj.ID;
    }

    public void ReadFrom(string content)
    {
        this.EasyReadFrom(content);
    }
}