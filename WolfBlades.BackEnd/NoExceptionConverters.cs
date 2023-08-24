namespace WolfBlades.BackEnd;

public static class NoExceptionConverters
{
    public static DateTime ConvertToDateTime(this string message)
    {
        try
        {
            return DateTime.ParseExact(message, "s", null);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    public static string ConvertToString(this DateTime date)
    {
        return date.ToString("s").Replace('T', ' ');
    }
}