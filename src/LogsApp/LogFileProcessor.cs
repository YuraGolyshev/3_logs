using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogsApp;

public class LogFileProcessor
{
    /// <summary>
    /// Читает логи из списка файлов, строчно, фильтрует по датам, отдаёт валидные LogEntry.
    /// </summary>
    public IEnumerable<LogEntry> ReadLogEntries(IEnumerable<string> files, DateTime? from, DateTime? to)
    {
        var expandedFiles = ExpandFilePatterns(files);
        foreach (var file in expandedFiles)
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

    private IEnumerable<string> ExpandFilePatterns(IEnumerable<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            if (pattern.Contains('*') || pattern.Contains('?'))
            {
                string directory;
                string fileNamePattern;

                if (pattern.Contains("**"))
                {
                    var lastStarIndex = pattern.LastIndexOf("**");
                    var afterStars = pattern.Substring(lastStarIndex + 2);
                    var pathBeforeStars = pattern.Substring(0, lastStarIndex);

                    if (string.IsNullOrEmpty(pathBeforeStars))
                    {
                        directory = Directory.GetCurrentDirectory();
                    }
                    else
                    {
                        directory = Path.IsPathRooted(pathBeforeStars) ? pathBeforeStars : Path.GetFullPath(pathBeforeStars);
                    }

                    fileNamePattern = afterStars.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    if (string.IsNullOrEmpty(fileNamePattern))
                    {
                        fileNamePattern = "*";
                    }
                }
                else
                {
                    directory = Path.GetDirectoryName(pattern);
                    if (string.IsNullOrEmpty(directory))
                    {
                        directory = Directory.GetCurrentDirectory();
                    }
                    else if (!Path.IsPathRooted(directory))
                    {
                        directory = Path.GetFullPath(directory);
                    }

                    fileNamePattern = Path.GetFileName(pattern);
                }

                if (Directory.Exists(directory))
                {
                    var searchOption = pattern.Contains("**") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    var foundFiles = Directory.GetFiles(directory, fileNamePattern, searchOption);
                    foreach (var file in foundFiles)
                    {
                        var ext = Path.GetExtension(file).ToLower();
                        if (ext == ".log" || ext == ".txt")
                        {
                            yield return file;
                        }
                    }
                }
            }
            else
            {
                yield return pattern;
            }
        }
    }
}
