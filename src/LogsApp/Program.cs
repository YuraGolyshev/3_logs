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
            // TODO: форматирование и запись результата в stats
            Console.WriteLine($"Сбор статистики завершён. Всего обработано: {stats.TotalRequestsCount} записей.");
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
