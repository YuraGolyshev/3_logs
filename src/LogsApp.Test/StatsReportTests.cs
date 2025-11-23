using LogsApp;
using Xunit;
using System.IO;
using System.Linq;

namespace LogsApp.Test;

public class StatsReportTests
{
    [Fact]
    public void JsonTest()
    {
        var stat = new StatisticsBuilder().Build(new[]
        {
            new LogEntry{BodyBytesSent=50, Status=200, Resource="/abc", TimeLocal=System.DateTime.UtcNow, Protocol="HTTP/2.0"}
        }, new() { "file1.log" });
        string outfile = "test_stats.json";
        if (File.Exists(outfile))
        {
            File.Delete(outfile);
        }

        new JsonFormatter().Write(stat, outfile);
        var output = File.ReadAllText(outfile);
        Assert.Contains("totalRequestsCount", output);
        Assert.Contains("abc", output);
        File.Delete(outfile);
    }
    [Fact]
    public void MarkdownTest()
    {
        var stat = new StatisticsBuilder().Build(new[] { new LogEntry { BodyBytesSent = 51, Status = 200, Resource = "/m", TimeLocal = System.DateTime.UtcNow, Protocol = "HTTP/1.1" } }, new() { "f.md" });
        string outfile = "test_stats.md";
        if (File.Exists(outfile))
        {
            File.Delete(outfile);
        }

        new MarkdownFormatter().Write(stat, outfile);
        var output = File.ReadAllText(outfile);
        Assert.Contains("#### Общая информация", output);
        Assert.Contains("f.md", output);
        File.Delete(outfile);
    }
    [Fact]
    public void AdocTest()
    {
        var stat = new StatisticsBuilder().Build(new[] { new LogEntry { BodyBytesSent = 52, Status = 404, Resource = "/ad", TimeLocal = System.DateTime.UtcNow, Protocol = "QUIC" } }, new() { "a.ad" });
        string outfile = "test_stats.ad";
        if (File.Exists(outfile))
        {
            File.Delete(outfile);
        }

        new AdocFormatter().Write(stat, outfile);
        var output = File.ReadAllText(outfile);
        Assert.Contains("Общая информация", output);
        Assert.Contains("QUIC", output);
        File.Delete(outfile);
    }
}