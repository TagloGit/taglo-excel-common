using Xunit;

namespace Taglo.Excel.Common.Tests;

public class LoggerTests : IDisposable
{
    private readonly string _testDir;

    public LoggerTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"LoggerTests_{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        Logger.Reset();
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    [Fact]
    public void Initialize_CreatesLogDirectory()
    {
        var appName = $"TestApp_{Guid.NewGuid():N}";
        Logger.Initialize(appName);

        Assert.NotNull(Logger.LogFilePath);
        Assert.Contains(appName, Logger.LogFilePath);
        Assert.True(Directory.Exists(Path.GetDirectoryName(Logger.LogFilePath)));
    }

    [Fact]
    public void Info_WritesToLogFile()
    {
        var appName = $"TestApp_{Guid.NewGuid():N}";
        Logger.Initialize(appName);

        Logger.Info("Test message");

        Assert.True(File.Exists(Logger.LogFilePath));
        var content = File.ReadAllText(Logger.LogFilePath!);
        Assert.Contains("[INFO] Test message", content);
    }

    [Fact]
    public void Error_WritesExceptionToLogFile()
    {
        var appName = $"TestApp_{Guid.NewGuid():N}";
        Logger.Initialize(appName);

        var ex = new InvalidOperationException("Something broke");
        Logger.Error("TestSource", ex);

        var content = File.ReadAllText(Logger.LogFilePath!);
        Assert.Contains("[ERROR] TestSource:", content);
        Assert.Contains("Something broke", content);
    }

    [Fact]
    public void Initialize_TruncatesOversizedLogFile()
    {
        var appName = $"TestApp_{Guid.NewGuid():N}";

        // First initialize to create the directory and file
        Logger.Initialize(appName);
        var logPath = Logger.LogFilePath!;

        // Write more than 1 MB to the log file
        var bigContent = new string('X', 1_100_000);
        File.WriteAllText(logPath, bigContent);
        Assert.True(new FileInfo(logPath).Length > 1_048_576);

        // Re-initialize should truncate
        Logger.Reset();
        Logger.Initialize(appName);

        Assert.True(File.Exists(logPath));
        Assert.Equal(0, new FileInfo(logPath).Length);
    }

    [Fact]
    public void Write_BeforeInitialize_DoesNotThrow()
    {
        // Should silently do nothing when not initialized
        Logger.Reset();
        Logger.Info("This should not throw");
        Logger.Error("Source", new Exception("test"));
    }
}
