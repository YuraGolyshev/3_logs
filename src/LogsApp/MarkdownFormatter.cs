using System.IO;
using System.Text;

namespace LogsApp;

public class MarkdownFormatter
{
    public void Write(StatisticsResult stats, string outputPath)
    {
        if (!outputPath.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase))
            throw new ArgumentsCliException("Для markdown-отчёта разрешено только расширение .md!");
        if (File.Exists(outputPath))
            throw new ArgumentsCliException("Файл уже существует (выберите другой --output или удалите файл вручную)");
        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            throw new ArgumentsCliException($"Директория {dir} не существует или недоступна для записи");
        var sb = new StringBuilder();
        sb.AppendLine("#### Общая информация\n");
        sb.AppendLine("|        Метрика        |     Значение |");
        sb.AppendLine("|:---------------------:|-------------:|");
        sb.AppendLine($"|       Файл(-ы)        | {string.Join(", ", stats.Files)} |");
        sb.AppendLine($"|    Количество запросов |   {stats.TotalRequestsCount} |");
        if (stats.TotalRequestsCount > 0)
        {
            sb.AppendLine($"| Средний размер ответа  |     {stats.ResponseSizeInBytes.Average}b |");
            sb.AppendLine($"| Максимальный размер    |     {stats.ResponseSizeInBytes.Max}b |");
            sb.AppendLine($"|  95p размера ответа    |     {stats.ResponseSizeInBytes.P95}b |");
        }
        sb.AppendLine("\n#### Запрашиваемые ресурсы\n");
        sb.AppendLine("|     Ресурс      | Количество |");
        sb.AppendLine("|:---------------:|-----------:|");
        foreach (var res in stats.Resources) sb.AppendLine($"| {res.Resource}  |  {res.TotalRequestsCount} |");
        sb.AppendLine("\n#### Коды ответа\n");
        sb.AppendLine("| Код | Количество |");
        sb.AppendLine("|:---:|-----------:|");
        foreach (var c in stats.ResponseCodes) sb.AppendLine($"| {c.Code} | {c.TotalResponsesCount} |");
        if (stats.RequestsPerDate.Count > 0)
        {
            sb.AppendLine("\n#### Распределение по датам\n");
            sb.AppendLine("|    Дата    |   День   | Количество | Проценты |");
            sb.AppendLine("|:----------:|:--------:|-----------:|----------:|");
            foreach(var d in stats.RequestsPerDate) sb.AppendLine($"| {d.Date} | {d.Weekday} | {d.TotalRequestsCount} | {d.TotalRequestsPercentage}% |");
        }
        if(stats.UniqueProtocols.Count > 0)
        {
            sb.AppendLine("\n**Уникальные протоколы:** " + string.Join(", ", stats.UniqueProtocols));
        }
        File.WriteAllText(outputPath, sb.ToString());
    }
}
