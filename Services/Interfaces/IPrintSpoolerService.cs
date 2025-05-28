namespace PrinterJobInterceptor.Services.Interfaces;
public interface IPrintSpoolerService
{
    bool IsSpoolerRunning { get; }
    event EventHandler SpoolerStarted;
    event EventHandler SpoolerStopped;
    Task<bool> StartSpoolerAsync();
    Task<bool> StopSpoolerAsync();
    Task<bool> RestartSpoolerAsync();
    Task WaitForSpoolerAsync(TimeSpan timeout);
}
