namespace WolfBlades.BackEnd.Users;

public class UserStorageInfo : IDataStorage<UserInfo>
{
    public int Authority = -1;
    public string DisplayName = "user_display_name(or nickname)";
    public string Name = "user_name_for_login";
    public string Password = "user_password_for_login";
    public int[] TechGroup = Array.Empty<int>();
    public TimeSpan TokenTimeBeforeExpire = TimeSpan.FromDays(1);
    public string TokenValue = "token";
    public DateTime TokenGeneratedTime = DateTime.Now;
    public int[] UnitGroup = Array.Empty<int>();

    public void ReadFrom(UserInfo data)
    {
        Name = data.Name;
        DisplayName = data.DisplayName;
        TechGroup = data.TechGroup.ToList().ToArray();
        UnitGroup = data.UnitGroup.ToList().ToArray();
        Authority = data.Authority;
        TokenValue = data.Token;
    }

    public void WriteTo(ref UserInfo data)
    {
        data.Name = Name;
        data.DisplayName = DisplayName;
        data.TechGroup = TechGroup.ToList().ToArray();
        data.UnitGroup = UnitGroup.ToList().ToArray();
        data.Authority = Authority;
        data.Token = TokenValue;
    }
}