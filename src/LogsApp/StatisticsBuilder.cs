using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LogsApp;

public class StatisticsResult
{
    public List<string> Files { get; set; } = new();
    public int TotalRequestsCount { get; set; }
    public ResponseSizeInBytesStat ResponseSizeInBytes { get; set; } = new();
    public List<ResourceStat> Resources { get; set; } = new();
    public List<ResponseCodeStat> ResponseCodes { get; set; } = new();
    public List<RequestsPerDateStat> RequestsPerDate { get; set; } = new();
    public List<string> UniqueProtocols { get; set; } = new();
}

public class ResponseSizeInBytesStat { public double Average { get; set; } public int Max { get; set; } public int P95 { get; set; } }
public class ResourceStat { public string Resource { get; set; } = ""; public int TotalRequestsCount { get; set; } }
public class ResponseCodeStat { public int Code { get; set; } public int TotalResponsesCount { get; set; } }
public class RequestsPerDateStat { public string Date { get; set; } = ""; public string Weekday { get; set; } = ""; public int TotalRequestsCount { get; set; } public double TotalRequestsPercentage { get; set; } }

public class StatisticsBuilder
{
    public StatisticsResult Build(IEnumerable<LogEntry> logEntries, List<string> files)
    {
        var list = logEntries.ToList();
        var stats = new StatisticsResult
        {
            Files = files.Select(f => Path.GetFileName(f)).ToList(),
            TotalRequestsCount = list.Count
        };

        // Response size (byte) aggregate
        if (list.Count > 0)
        {
            var sizes = list.Select(x => x.BodyBytesSent).OrderBy(x => x).ToList();
            stats.ResponseSizeInBytes.Average = Math.Round(sizes.Average(), 2);
            stats.ResponseSizeInBytes.Max = sizes.Max();

            // Расчет P95 с линейной интерполяцией
            double p95Position = 0.95 * (sizes.Count - 1);
            int lowerIndex = (int)Math.Floor(p95Position);
            int upperIndex = (int)Math.Ceiling(p95Position);
            double fraction = p95Position - lowerIndex;

            if (upperIndex >= sizes.Count)
            {
                stats.ResponseSizeInBytes.P95 = sizes[lowerIndex];
            }
            else
            {
                stats.ResponseSizeInBytes.P95 = (int)Math.Round(sizes[lowerIndex] + fraction * (sizes[upperIndex] - sizes[lowerIndex]));
            }
        }

        // Top 10 ресурсов
        stats.Resources = list.GroupBy(x => x.Resource)
            .Select(g => new ResourceStat { Resource = g.Key, TotalRequestsCount = g.Count() })
            .OrderByDescending(r => r.TotalRequestsCount)
            .Take(10)
            .ToList();

        // Частота кодов ответа
        stats.ResponseCodes = list.GroupBy(x => x.Status)
            .Select(g => new ResponseCodeStat { Code = g.Key, TotalResponsesCount = g.Count() })
            .OrderByDescending(g => g.TotalResponsesCount)
            .ToList();

        // Распределение по датам
        if (list.Count > 0)
        {
            var byDate = list.GroupBy(l => l.TimeLocal.Date)
                .Select(g => new RequestsPerDateStat
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Weekday = CultureInfo.InvariantCulture.DateTimeFormat.GetDayName(g.Key.DayOfWeek),
                    TotalRequestsCount = g.Count(),
                    TotalRequestsPercentage = Math.Round((double)g.Count() * 100.0 / list.Count, 2)
                })
                .OrderBy(d => d.Date)
                .ToList();
            stats.RequestsPerDate = byDate;
        }

        // Уникальные протоколы 
        var allProtocols = list
            .Select(l => l.Protocol)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList();

        // Определяем правильный порядок протоколов
        var protocolOrder = new List<string> { "HTTP/1.1", "HTTP/1.0", "HTTP/2.1", "grpc" };
        stats.UniqueProtocols = protocolOrder
            .Where(p => allProtocols.Contains(p))
            .Concat(allProtocols.Where(p => !protocolOrder.Contains(p)))
            .ToList();

        return stats;
    }
}