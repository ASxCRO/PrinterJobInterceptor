using PrinterJobInterceptor.Models;

namespace PrinterJobInterceptor.Services.Interfaces;
public interface IPrintJobMonitorService : IDisposable
{
    bool IsMonitoring { get; }
    event EventHandler<PrintJob> PrintJobCreated;
    event EventHandler<PrintJob> PrintJobModified;
    event EventHandler<PrintJob> PrintJobDeleted;
    Task StartMonitoringAsync();
    void StopMonitoring();
}