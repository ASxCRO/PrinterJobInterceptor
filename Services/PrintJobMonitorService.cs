using PrinterJobInterceptor.Models;
using PrinterJobInterceptor.Services.Interfaces;

namespace PrinterJobInterceptor.Services;
public class PrintJobMonitorService : IPrintJobMonitorService
{
    public bool IsMonitoring => throw new NotImplementedException();

    public event EventHandler<PrintJob> PrintJobCreated;
    public event EventHandler<PrintJob> PrintJobModified;
    public event EventHandler<PrintJob> PrintJobDeleted;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task StartMonitoringAsync()
    {
        throw new NotImplementedException();
    }

    public void StopMonitoring()
    {
        throw new NotImplementedException();
    }
}