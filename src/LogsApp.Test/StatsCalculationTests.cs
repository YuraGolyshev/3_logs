using LogsApp;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogsApp.Test;

public class StatsCalculationTests
{
    [Fact]
    public void CalculatesStatsCorrectly()
    {
        var list = new List<LogEntry>
        {
            new LogEntry { BodyBytesSent = 100, Status = 200, Resource="/a", TimeLocal=DateTime.Parse("2022-01-01T12:00:00Z"), Protocol="HTTP/1.1" },
            new LogEntry { BodyBytesSent = 200, Status = 200, Resource="/a", TimeLocal=DateTime.Parse("2022-01-01T12:05:00Z"), Protocol="HTTP/1.1" },
            new LogEntry { BodyBytesSent = 700, Status = 404, Resource="/b", TimeLocal=DateTime.Parse("2022-01-02T10:00:00Z"), Protocol="grpc" }
        };
        var stats = new StatisticsBuilder().Build(list, new List<string> { "123.log" });
        Assert.Equal(3, stats.TotalRequestsCount);
        Assert.Equal(333, stats.ResponseSizeInBytes.Average);
        Assert.Equal(700, stats.ResponseSizeInBytes.Max);
        Assert.Equal(700, stats.ResponseSizeInBytes.P95); // ceil(95%*3)=2.85=>2,indexed from 0=>2
        Assert.Equal(2, stats.Resources.Count);
        Assert.Equal("/a", stats.Resources[0].Resource);
        Assert.Single(stats.UniqueProtocols.Where(p => p == "grpc"));
        Assert.Contains(stats.ResponseCodes, x=>x.Code==404);
    }
}