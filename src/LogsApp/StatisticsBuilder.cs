using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LogsApp;

internal class StatisticsResult
{
    public List<string> Files { get; set; } = new();
    public int TotalRequestsCount { get; set; }
    public ResponseSizeInBytesStat ResponseSizeInBytes { get; set; } = new();
    public List<ResourceStat> Resources { get; set; } = new();
    public List<ResponseCodeStat> ResponseCodes { get; set; } = new();
    public List<RequestsPerDateStat> RequestsPerDate { get; set; } = new();
    public List<string> UniqueProtocols { get; set; } = new();
}

internal class ResponseSizeInBytesStat { public int Average { get; set; } public int Max { get; set; } public int P95 { get; set; } }
internal class ResourceStat { public string Resource { get; set; } = ""; public int TotalRequestsCount { get; set; } }
internal class ResponseCodeStat { public int Code { get; set; } public int TotalResponsesCount { get; set; } }
internal class RequestsPerDateStat { public string Date { get; set; } = ""; public string Weekday { get; set; } = ""; public int TotalRequestsCount { get; set; } public double TotalRequestsPercentage { get; set; } }

internal class StatisticsBuilder
{
    public StatisticsResult Build(IEnumerable<LogEntry> logEntries, List<string> files)
    {
        var list = logEntries.ToList();
        var stats = new StatisticsResult
        {
            Files = new List<string>(files),
            TotalRequestsCount = list.Count
        };
        // Response size (byte) aggregate
        if (list.Count > 0)
        {
            var sizes = list.Select(x => x.BodyBytesSent).OrderBy(x => x).ToList();
            stats.ResponseSizeInBytes.Average = (int)Math.Round(sizes.Average());
            stats.ResponseSizeInBytes.Max = sizes.Max();
            int p95Index = (int)Math.Ceiling(sizes.Count * 0.95) - 1;
            p95Index = Math.Clamp(p95Index, 0, sizes.Count - 1);
            stats.ResponseSizeInBytes.P95 = sizes[p95Index];
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
                .Select(g => new RequestsPerDateStat {
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
        stats.UniqueProtocols = list.Select(l => l.Protocol).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
        return stats;
    }
}
