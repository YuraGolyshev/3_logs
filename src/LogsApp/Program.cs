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
            // Чтение и обработка логов (простая индексация для отладки)
            var processor = new LogFileProcessor();
            int count = 0;
            foreach (var entry in processor.ReadLogEntries(cliArgs.Paths!, cliArgs.From, cliArgs.To))
            {
                Console.WriteLine($"LOG: {entry.TimeLocal:O} {entry.RemoteAddr} {entry.Resource} {entry.Status} {entry.BodyBytesSent}");
                count++;
            }
            Console.WriteLine($"Итого валидных логов считано: {count}");
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
