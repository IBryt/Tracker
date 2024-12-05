namespace Core.Utils;

public static class Constants
{
    public readonly static int ProcessCheckInterval = 1000;
    public readonly static string Notepad = "notepad";
    public readonly static string Telegram = "Telegram";
    public readonly static List<string> ProcessNames = new() { Notepad, Telegram };
}
