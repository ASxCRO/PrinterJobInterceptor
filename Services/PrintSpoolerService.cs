using PrinterJobInterceptor.Services.Interfaces;

namespace PrinterJobInterceptor.Services;
public class PrintSpoolerService : IPrintSpoolerService
{
    public bool IsSpoolerRunning => throw new NotImplementedException();

    public event EventHandler SpoolerStarted;
    public event EventHandler SpoolerStopped;

    public Task<bool> RestartSpoolerAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> StartSpoolerAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> StopSpoolerAsync()
    {
        throw new NotImplementedException();
    }

    public Task WaitForSpoolerAsync(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }
}
