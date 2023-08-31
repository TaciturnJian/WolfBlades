using Newtonsoft.Json;

namespace WolfBlades.BackEnd.ItemManager;

public static class TinyConverterExtension
{
    public static string ToJsonString<T>(this T source, bool indented = false)
    {
        return JsonConvert.SerializeObject(source, indented ? Formatting.Indented : Formatting.None);
    }

    public static void EasyReadFrom<T>(this T item, string content) where T : IItem
    {
        var obj = JsonConvert.DeserializeObject<T>(content);
        if (obj != null) item.ReadFrom(obj);
    }

    public static int ToID(this string? text)
    {
        if (text is null) return -1;

        if (int.TryParse(text, out var id)) return id;

        return -1;
    }

    public static DateTime ConvertToDateTime(this string? message)
    {
        if (message is null) return DateTime.MinValue;

        try
        {
            return DateTime.ParseExact(message.Replace(' ', 'T'), "s", null);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    public static string ConvertToString(this DateTime? date)
    {
        return (date ?? DateTime.MinValue).ToString("s").Replace('T', ' ');
    }

    public static int[] CopyIntArray(this int[]? target)
    {
        return target is not null &&
               target.Length > 0
            ? target.ToArray()
            : Array.Empty<int>();
    }
}