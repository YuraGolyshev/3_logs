using System;
using System.Collections.Generic;
using System.IO;

namespace LogsApp;

public class LogFileProcessor
{
    /// <summary>
    /// Читает логи из списка файлов, строчно, фильтрует по датам, отдаёт валидные LogEntry.
    /// </summary>
    public IEnumerable<LogEntry> ReadLogEntries(IEnumerable<string> files, DateTime? from, DateTime? to)
    {
        foreach (var file in files)
        {
            if (file.StartsWith("http://") || file.StartsWith("https://"))
            {
                // TODO: реализовать загрузку по URL
                Console.Error.WriteLine($"[INFO] Пропущен файл по URL (не реализовано): {file}");
                continue;
            }
            if (!File.Exists(file))
            {
                Console.Error.WriteLine($"[WARN] Файл не найден: {file}");
                continue;
            }
            int total = 0, valid = 0;
            using var sr = new StreamReader(file);
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                total++;
                var entry = LogParser.ParseAndWarn(line);
                if (entry == null)
                {
                    continue;
                }

                if (from != null && entry.TimeLocal.Date < from.Value.Date)
                {
                    continue;
                }

                if (to != null && entry.TimeLocal.Date > to.Value.Date)
                {
                    continue;
                }

                valid++;
                yield return entry;
            }
            Console.Error.WriteLine($"[INFO] Завершено чтение файла {file}: {total} строк, валидных {valid}");
        }
    }
}
