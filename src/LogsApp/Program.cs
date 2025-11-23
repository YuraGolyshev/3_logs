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
            var processor = new LogFileProcessor();
            var logEntries = processor.ReadLogEntries(cliArgs.Paths!, cliArgs.From, cliArgs.To).ToList();
            var statsBuilder = new StatisticsBuilder();
            var stats = statsBuilder.Build(logEntries, cliArgs.Paths!);

            switch (cliArgs.Format)
            {
                case "json":
                    new JsonFormatter().Write(stats, cliArgs.OutputPath!);
                    break;
                case "markdown":
                    new MarkdownFormatter().Write(stats, cliArgs.OutputPath!);
                    break;
                case "adoc":
                    new AdocFormatter().Write(stats, cliArgs.OutputPath!);
                    break;
                default:
                    throw new ArgumentsCliException($"Неподдерживаемый формат: {cliArgs.Format}");
            }
            Console.WriteLine($"[OK] Анализ завершён, результат в файле: {cliArgs.OutputPath}");
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
