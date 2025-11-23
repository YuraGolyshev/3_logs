using LogsApp;
using Xunit;

namespace LogsApp.Test;

public class LogFileParsingTests
{
    [Fact]
    public void ParsesValidLogEntry()
    {
        string valid = "93.180.71.3 - - [17/May/2015:08:05:32 +0000] \"GET /downloads/product_1 HTTP/1.1\" 304 0 \"-\" \"Debian APT-HTTP/1.3 (0.8.16~exp12ubuntu10.21)\"";
        var entry = LogParser.ParseAndWarn(valid);
        Assert.NotNull(entry);
        Assert.Equal("93.180.71.3", entry.RemoteAddr);
        Assert.Equal(304, entry.Status);
        Assert.Equal("/downloads/product_1", entry.Resource);
    }

    [Fact]
    public void IgnoresBrokenLine()
    {
        string invalid = "this is not nginx log line!";
        var entry = LogParser.ParseAndWarn(invalid);
        Assert.Null(entry);
    }
}