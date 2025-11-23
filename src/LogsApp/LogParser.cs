using System.Text.RegularExpressions;

namespace LogsApp;

internal static class LogParser
{
    private static readonly Regex pattern = new Regex(
        "^(?<ip>\\S+) - (?<user>\\S+) \\[([^\]]+)\\] \"(?<method>\\S+) (?<resource>\\S+)(?: (?<protocol>\\S+))?\" (?<status>\\d{3}) (?<bytes>\\d+) \"([^\"]*)\" \"([^\"]*)\"$",
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
                TimeLocal = ParseNginxDate(match.Groups[3].Value),
                Resource = match.Groups["resource"].Value,
                Protocol = match.Groups["protocol"].Success ? match.Groups["protocol"].Value : "",
                Status = int.Parse(match.Groups["status"].Value),
                BodyBytesSent = int.Parse(match.Groups["bytes"].Value),
                Referer = match.Groups[6].Value,
                UserAgent = match.Groups[7].Value
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
        return DateTime.ParseExact(s, "dd/MMM/yyyy:HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture);
    }
}
