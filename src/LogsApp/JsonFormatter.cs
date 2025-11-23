using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogsApp;

public class JsonFormatter
{
    public void Write(StatisticsResult stats, string outputPath)
    {
        if (!outputPath.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentsCliException("Для json-отчёта разрешено только расширение .json!");
        }

        if (File.Exists(outputPath))
        {
            throw new ArgumentsCliException("Файл уже существует (выберите другой --output или удалите файл вручную)");
        }

        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            throw new ArgumentsCliException($"Директория не существует или недоступна для записи: {dir}");
        }

        var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var json = JsonSerializer.Serialize(stats, opts);
        File.WriteAllText(outputPath, json);
    }
}
