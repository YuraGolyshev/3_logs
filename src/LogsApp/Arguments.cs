using System.Globalization;

namespace LogsApp;

public class Arguments
{
    public List<string>? Paths { get; set; }
    public string? Format { get; set; } // json, markdown, adoc
    public string? OutputPath { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public static Arguments Parse(string[] args)
    {
        if (args.Length == 0)
            throw new ArgumentsCliException("Не переданы параметры запуска. Пример использования: --path file.log --output report.json --format json");
        var cli = new Arguments();
        for(int i=0;i<args.Length;i++)
        {
            switch(args[i])
            {
                case "--path":
                case "-p":
                    if (i+1 >= args.Length || args[i+1].StartsWith("-"))
                        throw new ArgumentsCliException("После --path/-p не указан путь к файлу");
                    cli.Paths = args[++i].Split(';').ToList();
                    break;
                case "--format":
                case "-f":
                    if (i+1 >= args.Length || args[i+1].StartsWith("-"))
                        throw new ArgumentsCliException("После --format/-f не передан формат (json/markdown/adoc)");
                    cli.Format = args[++i].ToLower();
                    if (!(cli.Format == "json" || cli.Format == "markdown" || cli.Format == "adoc"))
                        throw new ArgumentsCliException($"Неподдерживаемый формат: {cli.Format}");
                    break;
                case "--output":
                case "-o":
                    if (i+1 >= args.Length || args[i+1].StartsWith("-"))
                        throw new ArgumentsCliException("После --output/-o не указан путь выходного файла");
                    cli.OutputPath = args[++i];
                    break;
                case "--from":
                    if (i+1 >= args.Length || args[i+1].StartsWith("-"))
                        throw new ArgumentsCliException("После --from не указана дата");
                    cli.From = ParseIso8601Date(args[++i], "from");
                    break;
                case "--to":
                    if (i+1 >= args.Length || args[i+1].StartsWith("-"))
                        throw new ArgumentsCliException("После --to не указана дата");
                    cli.To = ParseIso8601Date(args[++i], "to");
                    break;
                default:
                    throw new ArgumentsCliException($"Неподдерживаемый параметр: {args[i]}");
            }
        }
        if (cli.Paths == null || cli.Paths.Count == 0)
            throw new ArgumentsCliException("Не передан параметр --path/-p, он обязателен");
        if (string.IsNullOrWhiteSpace(cli.OutputPath))
            throw new ArgumentsCliException("Не передан параметр --output/-o, он обязателен");
        if (string.IsNullOrWhiteSpace(cli.Format))
            throw new ArgumentsCliException("Не передан параметр --format/-f, он обязателен");
        if (cli.From != null && cli.To != null && cli.From >= cli.To)
            throw new ArgumentsCliException("from >= to: дата начала больше или равна дате конца");
        return cli;
    }
    static DateTime ParseIso8601Date(string value, string field)
    {
        if (!DateTime.TryParse(value, null, DateTimeStyles.AdjustToUniversal, out var dt))
            throw new ArgumentsCliException($"Параметр --{field} не является валидной датой ISO8601: {value}");
        return dt;
    }
}

public class ArgumentsCliException : Exception
{
    public ArgumentsCliException(string msg): base(msg) {}
}
