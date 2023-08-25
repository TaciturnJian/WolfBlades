namespace WolfBlades.BackEnd.Users;

public class UserInfoManager : DataManager<UserStorageInfo, UserInfo>
{
    public int LoginUser(string userName, string loginValue)
    {
        foreach (var (id, user) in Database)
        {
            if (user.Name != userName) continue;

            if (user.TokenValue == loginValue)
                return user.TokenGeneratedTime + user.TokenTimeBeforeExpire >= DateTime.Now ? id : -2;

            if (user.Password != loginValue) return -2;

            GenerateTokenForUser(user);
            return id;
        }

        return -1;
    }

    private static void GenerateTokenForUser(UserStorageInfo user)
    {
        user.TokenValue = "token";                              //TODO Generate token
        user.TokenGeneratedTime = DateTime.Now;
        user.TokenTimeBeforeExpire = TimeSpan.FromDays(1);
    }
}