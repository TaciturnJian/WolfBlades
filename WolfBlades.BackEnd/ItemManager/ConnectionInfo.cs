namespace WolfBlades.BackEnd.ItemManager;

/// <summary>
///     连接的信息
/// </summary>
public class ConnectionInfo : IItem
{
    /// <summary>
    ///     连接的IP地址
    /// </summary>
    public string IPAddress { get; set; } = string.Empty;

    /// <summary>
    ///     登录的用户的ID
    /// </summary>
    public int LoginUserID { get; set; } = -1;

    /// <summary>
    ///     连接的端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    ///     用户的权限
    /// </summary>
    public int UserAuthority { get; set; }

    public int ID { get; set; }

    public void ReadFrom(IItem item)
    {
        if (item is not ConnectionInfo obj) return;

        IPAddress = obj.IPAddress;
        Port = obj.Port;
        LoginUserID = obj.LoginUserID;
        UserAuthority = obj.UserAuthority;
        ID = obj.ID;
    }

    public void ReadFrom(string content)
    {
        this.EasyReadFrom(content);
    }
}