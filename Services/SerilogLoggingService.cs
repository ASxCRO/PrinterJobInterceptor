using PrinterJobInterceptor.Services.Interfaces;
using Serilog;

namespace PrinterJobInterceptor.Services;

public class SerilogLoggingService : ILoggingService
{
    private readonly ILogger _logger;

    public SerilogLoggingService()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/printer-interceptor-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Logger = _logger;
    }

    public void LogInformation(string message)
    {
        _logger.Information(message);
    }

    public void LogWarning(string message)
    {
        _logger.Warning(message);
    }

    public void LogError(string message, Exception exception = null)
    {
        if (exception != null)
            _logger.Error(exception, message);
        else
            _logger.Error(message);
    }

    public void LogDebug(string message)
    {
        _logger.Debug(message);
    }

    public void LogTrace(string message)
    {
        _logger.Verbose(message);
    }
}
