﻿namespace WolfBlades.BackEnd;

public static class NoExceptionConverterExtension
{
    public static DateTime ConvertToDateTime(this string message)
    {
        try
        {
            return DateTime.ParseExact(message.Replace(' ', 'T'), "s", null);
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

    public static int[] CopyIntArray(this int[]? target)
    {
        return target is not null &&
               target.Length > 0
            ? target.ToArray()
            : Array.Empty<int>();
    }
}