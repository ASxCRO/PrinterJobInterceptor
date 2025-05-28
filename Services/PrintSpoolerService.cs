using System.ServiceProcess;
using PrinterJobInterceptor.Services.Interfaces;

namespace PrinterJobInterceptor.Services;
public class PrintSpoolerService : IPrintSpoolerService
{
    private const string SPOOLER_SERVICE_NAME = "Spooler";
    private readonly ServiceController _spoolerService;
    private readonly ILoggingService _logger;

    public PrintSpoolerService(ILoggingService logger)
    {
        _logger = logger;
        _spoolerService = new ServiceController(SPOOLER_SERVICE_NAME);
    }

    public bool IsSpoolerRunning => _spoolerService.Status == ServiceControllerStatus.Running;

    public event EventHandler SpoolerStarted;
    public event EventHandler SpoolerStopped;

    public async Task<bool> RestartSpoolerAsync()
    {
        try
        {
            await StopSpoolerAsync();
            await Task.Delay(1000); // Wait for service to fully stop
            return await StartSpoolerAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to restart print spooler", ex);
            return false;
        }
    }

    public async Task<bool> StartSpoolerAsync()
    {
        try
        {
            if (IsSpoolerRunning) return true;

            _spoolerService.Start();
            await WaitForSpoolerAsync(TimeSpan.FromSeconds(30));
            
            if (IsSpoolerRunning)
            {
                SpoolerStarted?.Invoke(this, EventArgs.Empty);
                _logger.LogInformation("Print spooler started successfully");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to start print spooler", ex);
            return false;
        }
    }

    public async Task<bool> StopSpoolerAsync()
    {
        try
        {
            if (!IsSpoolerRunning) return true;

            _spoolerService.Stop();
            await WaitForSpoolerAsync(TimeSpan.FromSeconds(30));
            
            if (!IsSpoolerRunning)
            {
                SpoolerStopped?.Invoke(this, EventArgs.Empty);
                _logger.LogInformation("Print spooler stopped successfully");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to stop print spooler", ex);
            return false;
        }
    }

    public async Task WaitForSpoolerAsync(TimeSpan timeout)
    {
        var startTime = DateTime.Now;
        while (DateTime.Now - startTime < timeout)
        {
            _spoolerService.Refresh();
            if (_spoolerService.Status == ServiceControllerStatus.Running)
            {
                return;
            }
            await Task.Delay(100);
        }
        throw new System.TimeoutException("Timeout waiting for print spooler to start");
    }
}
