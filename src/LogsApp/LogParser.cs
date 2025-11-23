using System.Text.RegularExpressions;

namespace LogsApp;

public static class LogParser
{
    private static readonly Regex pattern = new Regex(
        // Используем raw string и именованные группы для удобства разбора
        @"^(?<ip>\S+) - (?<user>\S+) \[(?<datetime>[^\]]+)\] ""(?<method>\S+) (?<resource>\S+)(?: (?<protocol>\S+))?"" (?<status>\d{3}) (?<bytes>\d+) ""(?<referer>[^""]*)"" ""(?<agent>[^""]*)""$",
        RegexOptions.Compiled);

    public static LogEntry? ParseAndWarn(string line)
    {
        var match = pattern.Match(line);
        if (!match.Success)
        {
            Console.Error.WriteLine($"[WARN] Некорректная строка лога: {line}");
            return null;
        }
        try
        {
            var log = new LogEntry
            {
                RemoteAddr = match.Groups["ip"].Value,
                RemoteUser = match.Groups["user"].Value,
                TimeLocal = ParseNginxDate(match.Groups["datetime"].Value),
                Resource = match.Groups["resource"].Value,
                Protocol = match.Groups["protocol"].Success ? match.Groups["protocol"].Value : "",
                Status = int.Parse(match.Groups["status"].Value),
                BodyBytesSent = int.Parse(match.Groups["bytes"].Value),
                Referer = match.Groups["referer"].Value,
                UserAgent = match.Groups["agent"].Value
            };
            return log;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[WARN] Ошибка разбора строки: {line} | {ex.Message}");
            return null;
        }
    }

    private static DateTime ParseNginxDate(string s)
    {
        var formats = new[]
        {
            "dd/MMM/yyyy:HH:mm:ss zzz",
            "d/MMM/yyyy:HH:mm:ss zzz"
        };
        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(s, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
        }
        throw new FormatException($"Не удалось распарсить дату: {s}");
    }
}
