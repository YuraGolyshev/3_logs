using System.IO;
using System.Text;

namespace LogsApp;

public class AdocFormatter
{
    public void Write(StatisticsResult stats, string outputPath)
    {
        if (!outputPath.EndsWith(".ad", System.StringComparison.OrdinalIgnoreCase) && !outputPath.EndsWith(".adoc", System.StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentsCliException("Для adoc-отчёта разрешено только расширение .ad или .adoc!");
        }

        if (File.Exists(outputPath))
        {
            throw new ArgumentsCliException("Файл уже существует (выберите другой --output или удалите файл вручную)");
        }

        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            throw new ArgumentsCliException($"Директория {dir} не существует или недоступна для записи");
        }

        var sb = new StringBuilder();
        sb.AppendLine("== Общая информация\n");
        sb.AppendLine("|===");
        sb.AppendLine("|     Метрика        | Значение");
        sb.AppendLine($"| Файл(-ы)           | {string.Join(", ", stats.Files)}");
        sb.AppendLine($"| Количество запросов| {stats.TotalRequestsCount}");
        if (stats.TotalRequestsCount > 0)
        {
            sb.AppendLine($"| Средний размер     | {stats.ResponseSizeInBytes.Average}b");
            sb.AppendLine($"| Максимальный размер| {stats.ResponseSizeInBytes.Max}b");
            sb.AppendLine($"| 95p размер         | {stats.ResponseSizeInBytes.P95}b");
        }
        sb.AppendLine("|===\n");
        sb.AppendLine("== Топ-10 ресурсов\n");
        sb.AppendLine("|===");
        sb.AppendLine("| Ресурс | Количество");
        foreach (var res in stats.Resources)
        {
            sb.AppendLine($"| {res.Resource} | {res.TotalRequestsCount}");
        }

        sb.AppendLine("|===\n");
        sb.AppendLine("== Коды ответа\n");
        sb.AppendLine("|===");
        sb.AppendLine("| Код | Количество");
        foreach (var c in stats.ResponseCodes)
        {
            sb.AppendLine($"| {c.Code} | {c.TotalResponsesCount}");
        }

        sb.AppendLine("|===\n");
        if (stats.RequestsPerDate.Count > 0)
        {
            sb.AppendLine("== По датам\n");
            sb.AppendLine("|===");
            sb.AppendLine("|    Дата    |   День   | Количество | Проценты");
            foreach (var d in stats.RequestsPerDate)
            {
                sb.AppendLine($"| {d.Date} | {d.Weekday} | {d.TotalRequestsCount} | {d.TotalRequestsPercentage}%");
            }

            sb.AppendLine("|===\n");
        }
        if(stats.UniqueProtocols.Count > 0)
        {
            sb.AppendLine("**Уникальные протоколы**: " + string.Join(", ", stats.UniqueProtocols));
        }
        File.WriteAllText(outputPath, sb.ToString());
    }
}
