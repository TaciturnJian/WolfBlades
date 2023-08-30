namespace WolfBlades.BackEnd.Users;

public struct UserInfo
{
    public string Name = "user_name_for_login";
    public string DisplayName = "user_display_name(or nickname)";
    public int[] TechGroup;
    public int[] UnitGroup;
    public int Authority = -1;
    public string Token = "token";

    public UserInfo()
    {
        TechGroup = Array.Empty<int>();
        UnitGroup = Array.Empty<int>();
    }
}