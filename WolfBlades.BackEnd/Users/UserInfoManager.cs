using System.Text;

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
        user.TokenGeneratedTime = DateTime.Now;
        user.TokenTimeBeforeExpire = TimeSpan.FromDays(1);

        var code_nam = user.Name.GetHashCode();
        var code_tim = user.TokenGeneratedTime.ConvertToString().GetHashCode();
        var code_xor = code_nam ^ code_tim;
        var code_add = code_nam + code_tim;
        var code_sub = code_nam - code_tim;
        var code_otp = (code_nam * code_tim) ^ (code_add * code_sub);

        int[] codes = { code_nam, code_add, code_sub, code_xor, code_otp, code_tim };
        var code_ext = new StringBuilder();

        foreach (var code in codes)
        foreach (var @byte in BitConverter.GetBytes(code + code_tim))
        {
            var hash = @byte.GetHashCode();
            var xor = hash ^ code;
            var word = Math.Abs((hash + code) % 3) switch
            {
                0 => (char)(Math.Abs(xor % 10) + '0'),
                1 => (char)(Math.Abs(xor % 26) + 'a'),
                _ => (char)(Math.Abs(xor % 26) + 'A')
            };
            code_ext.Append(word);
        }

        user.TokenValue = code_ext.ToString(); //TODO Generate token
    }
}
