using LogsApp;

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
            
            if (logEntries.Count == 0)
            {
                Console.Error.WriteLine("[ERR] Не удалось обработать ни одной записи лога. Проверьте пути к файлам и формат данных.");
                return 2;
            }
            
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
