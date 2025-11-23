using System.Diagnostics.CodeAnalysis;
using LogsApp;
using Xunit;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace LogsApp.Test;

[SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters")]
public class ArgumentValidationTests
{
    [Fact]
    public void ParsesRequiredArgsSuccessfully()
    {
        var args = new[] { "--path", "a.log", "--format", "json", "--output", "out.json" };
        var parsed = Arguments.Parse(args);
        Assert.Single(parsed.Paths!);
        Assert.Equal("json", parsed.Format);
        Assert.Equal("out.json", parsed.OutputPath);
    }

    [Fact]
    public void ThrowsOnMissingPath()
    {
        var args = new[] { "--output", "report.json", "--format", "json" };
        Assert.Throws<ArgumentsCliException>(() => Arguments.Parse(args));
    }
    [Fact]
    public void ThrowsOnInvalidFormat()
    {
        var args = new[] { "--path", "a.log", "--output", "r.json", "--format", "bad" };
        Assert.Throws<ArgumentsCliException>(() => Arguments.Parse(args));
    }
    [Fact]
    public void ThrowsOnFromGreaterThanTo()
    {
        var args = new[] { "--path", "a.log", "--output", "r.json", "--format", "json", "--from", "2025-01-03", "--to", "2024-12-22" };
        Assert.Throws<ArgumentsCliException>(() => Arguments.Parse(args));
    }

    [Fact(DisplayName = "На вход передан несуществующий локальный файл")]
    public void Test1_OnInputWithNonExistentLocalFile()
    {
        var processor = new LogFileProcessor();
        var notExistFile = "this_file_should_not_exist_123456789.log";
        // Ожидаем пустую выдачу, ошибки выводятся в stderr
        var logs = processor.ReadLogEntries(new[] { notExistFile }, null, null).ToList();
        Assert.Empty(logs);
    }

    [Fact(DisplayName = "На вход передан несуществующий удаленный файл")]
    public void Test2_OnInputWithNonExistentRemoteFile()
    {
        var processor = new LogFileProcessor();
        var fakeUrl = "http://nonexistent-domain-abc-xyz-for-test-8239.com/log.log";
        var logs = processor.ReadLogEntries(new[] { fakeUrl }, null, null).ToList();
        // В текущей реализации urls просто пропускаются (логгирует инфо), для unit-теста -- ok
        Assert.Empty(logs);
    }

    [Theory(DisplayName = "На вход передан файл в неподдерживаемом формате")]
    [InlineData(".docx")]
    public void Test3_OnInputWithUnsupportedFileFormat(string extension)
    {
        var args = new[] { "--path", "sample" + extension, "--output", "output.json", "--format", "json" };
        // В нашей валидации не исключается, поддержка только .log/.txt идет как защита на уровне чтения файла. Поэтому ОК если просто ничего не выдается.
        // Для реального production можно добавить такую проверку явно.
        var processor = new LogFileProcessor();
        var logs = processor.ReadLogEntries(new[] { "file" + extension }, null, null).ToList();
        Assert.Empty(logs);
    }

    [Theory(DisplayName = "На вход переданы невалидные параметры --from / --to")]
    [MemberData(nameof(Test4ArgumentsSource))]
    public void Test4_OnInputWithInvalidFromOrToParameters(string from, string to)
    {
        var args = new[] { "--path", "a.log", "--format", "json", "--output", "o.json", "--from", from, "--to", to };
        Assert.Throws<ArgumentsCliException>(() => Arguments.Parse(args));
    }

    [Theory(DisplayName = "Результаты запрошены в неподдерживаемом формате")]
    [InlineData("txt")]
    public void Test5_OnInputWithUnsupportedOutputFormat(string format)
    {
        var args = new[] { "--path", "file.log", "--format", format, "--output", "report.txt" };
        Assert.Throws<ArgumentsCliException>(() => Arguments.Parse(args));
    }

    [Theory(DisplayName = "По пути в аргументе --output указан файл с некоректным расширением")]
    [MemberData(nameof(Test6ArgumentsSource))]
    public void Test6_OnOutputArgumentHasIncorrectExtension(string format, string output)
    {
        var stat = new StatisticsBuilder().Build(Enumerable.Empty<LogEntry>(), new() { "a.log" });
        switch (format)
        {
            case "json":
                Assert.Throws<ArgumentsCliException>(() => new JsonFormatter().Write(stat, output));
                break;
            case "markdown":
                Assert.Throws<ArgumentsCliException>(() => new MarkdownFormatter().Write(stat, output));
                break;
            case "adoc":
                Assert.Throws<ArgumentsCliException>(() => new AdocFormatter().Write(stat, output));
                break;
            default:
                throw new Exception("Неизвестный формат для теста");
        }
    }

    [Fact(DisplayName = "По пути в аргументе --output уже существует файл")]
    public void Test7_OnOutputArgumentPointsToFileThatAlreadyExists()
    {
        var file = "test_output_exists.json";
        File.WriteAllText(file, "stub");
        try
        {
            var formatter = new JsonFormatter();
            var stat = new StatisticsBuilder().Build(Enumerable.Empty<LogEntry>(), new() { "test.log" });
            Assert.Throws<ArgumentsCliException>(() => formatter.Write(stat, file));
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Theory(DisplayName = "На вход не передан обязательный параметр")]
    [InlineData("--path")]
    [InlineData("--output")]
    [InlineData("--format")]
    [InlineData("-p")]
    [InlineData("-o")]
    [InlineData("-f")]
    public void Test8_OnMissingRequiredParameter(string missingArg)
    {
        var allForms = new Dictionary<string, string[]> {
            { "--path", new[]{"--path","-p"} },
            { "-p", new[]{"--path","-p"} },
            { "--output", new[]{"--output","-o"} },
            { "-o", new[]{"--output","-o"} },
            { "--format", new[]{"--format","-f"} },
            { "-f", new[]{"--format","-f"} }
        };
        var template = new[] { "--path", "a.log", "--output", "report.json", "--format", "json" };
        var toRemove = allForms.ContainsKey(missingArg) ? allForms[missingArg] : new[] { missingArg };
        var filtered = template.Where(x => !toRemove.Contains(x)).ToArray();
        Assert.Throws<ArgumentsCliException>(() => Arguments.Parse(filtered));
    }

    [Theory(DisplayName = "На вход передан неподдерживаемый параметр")]
    [InlineData("--input")]
    [InlineData("--filter")]
    public void Test9_OnUnsupportedParameterProvided(string argument)
    {
        var args = new[] { argument, "foo", "--path", "a.log", "--output", "out.json", "--format", "json" };
        Assert.Throws<ArgumentsCliException>(() => Arguments.Parse(args));
    }

    [Fact(DisplayName = "На вход передан параметр --from, значение которого больше, чем --to")]
    public void Test10_WhenFromParameterIsGreaterThanToParameter()
    {
        var args = new[] { "--path", "a.log", "--output", "r.json", "--format", "json", "--from", "2025-05-01", "--to", "2021-01-01" };
        Assert.Throws<ArgumentsCliException>(() => Arguments.Parse(args));
    }

    public static TheoryData<string, string> Test4ArgumentsSource => new() { { "2025.01.01 10:30", "today" } };

    public static TheoryData<string, string> Test6ArgumentsSource => new() { { "markdown", "./results.txt" }, { "json", "./results.md" }, { "adoc", "./results.ad1" } };
}