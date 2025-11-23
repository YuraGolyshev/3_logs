// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Text.RegularExpressions;
// TODO: подключение логгера log4net

namespace LogsApp;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            Arguments cliArgs = Arguments.Parse(args);
            // LOG: Успешно считали и валидировали аргументы CLI
            Console.WriteLine($"Путь(и) к лог-файлам: {string.Join(", ", cliArgs.Paths)}");
            Console.WriteLine($"Формат вывода: {cliArgs.Format}");
            Console.WriteLine($"Выходной файл: {cliArgs.OutputPath}");
            if(cliArgs.From != null) Console.WriteLine($"Дата от: {cliArgs.From:O}");
            if(cliArgs.To != null) Console.WriteLine($"Дата до: {cliArgs.To:O}");
            // ... Основная логика будет добавлена на следующих этапах ...
            return 0;
        }
        catch(ArgumentsCliException aex)
        {
            Console.Error.WriteLine($"[ERR] {aex.Message}");
            return 2;
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"[FATAL] {ex.Message}\n{ex.StackTrace}");
            return 1;
        }
    }
}

// Класс для хранения CLI-аргументов и логики парсинга
internal class Arguments
{
    public List<string>? Paths { get; set; }
    public string? Format { get; set; } // json, markdown, adoc
    public string? OutputPath { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public static Arguments Parse(string[] args)
    {
        // TODO: заменить ручной парсер на CommandLineParser/кратность поддерживаемых опций
        if (args.Length == 0)
            throw new ArgumentsCliException("Не переданы параметры запуска. Пример использования: --path file.log --output report.json --format json");

        var cli = new Arguments();

        for(int i=0;i<args.Length;i++)
        {
            switch(args[i])
            {
                case "--path":
                case "-p":
                    if (i+1 >= args.Length)
                        throw new ArgumentsCliException("После --path не указан путь к файлу");
                    cli.Paths = args[++i].Split(';').ToList();
                    break;
                case "--format":
                case "-f":
                    if (i+1 >= args.Length)
                        throw new ArgumentsCliException("После --format не передан формат (json/markdown/adoc)");
                    cli.Format = args[++i].ToLower();
                    if (!(cli.Format == "json" || cli.Format == "markdown" || cli.Format == "adoc"))
                        throw new ArgumentsCliException($"Неподдерживаемый формат: {cli.Format}");
                    break;
                case "--output":
                case "-o":
                    if (i+1 >= args.Length)
                        throw new ArgumentsCliException("После --output не указан путь выходного файла");
                    cli.OutputPath = args[++i];
                    break;
                case "--from":
                    if (i+1 >= args.Length)
                        throw new ArgumentsCliException("После --from не указана дата");
                    cli.From = ParseIso8601Date(args[++i], "from");
                    break;
                case "--to":
                    if (i+1 >= args.Length)
                        throw new ArgumentsCliException("После --to не указана дата");
                    cli.To = ParseIso8601Date(args[++i], "to");
                    break;
                default:
                    throw new ArgumentsCliException($"Неподдерживаемый параметр: {args[i]}");
            }
        }
        // Валидация после парса
        if (cli.Paths == null || cli.Paths.Count == 0)
            throw new ArgumentsCliException("Не передан параметр --path, он обязателен");
        if (string.IsNullOrWhiteSpace(cli.OutputPath))
            throw new ArgumentsCliException("Не передан параметр --output, он обязателен");
        if (string.IsNullOrWhiteSpace(cli.Format))
            throw new ArgumentsCliException("Не передан параметр --format, он обязателен");
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

internal class ArgumentsCliException : Exception
{
    public ArgumentsCliException(string msg): base(msg) {}
}
