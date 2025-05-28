using PrinterJobInterceptor.Models;

namespace PrinterJobInterceptor.Services.Interfaces;

public interface IPrintJobGroupingService
{
    TimeSpan GroupTimeout { get; }
    IReadOnlyList<PrintJobGroup> ActiveGroups { get; }
    Task<PrintJobGroup> ProcessNewJobAsync(PrintJob job);
    void UpdateGroupTimeout(TimeSpan timeout);
    void CleanupOldGroups();
} 