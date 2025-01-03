namespace Core.Extensions;

public static class StringExtensions
{
    public static TEnum? GetEnumValueOrNull<TEnum>(this string input) where TEnum : struct
    {
        if (!string.IsNullOrEmpty(input) && Enum.TryParse(input, true, out TEnum parsedValue))
        {
            return parsedValue;
        }
        return null;
    }
}