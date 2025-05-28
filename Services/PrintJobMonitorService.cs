using System.Collections.Concurrent;
using System.Drawing.Printing;
using PrinterJobInterceptor.Helpers;
using PrinterJobInterceptor.Models;
using PrinterJobInterceptor.Services.Interfaces;

namespace PrinterJobInterceptor.Services;
public class PrintJobMonitorService : IPrintJobMonitorService, IDisposable
{
    private readonly ILoggingService _logger;
    private readonly IPrintJobGroupingService _groupingService;
    private readonly IDocumentAnalysisService _analysisService;
    private readonly ConcurrentDictionary<string, PrintJob> _activeJobs;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task _monitoringTask;
    private bool _isDisposed;

    public PrintJobMonitorService(
        ILoggingService logger,
        IPrintJobGroupingService groupingService,
        IDocumentAnalysisService analysisService)
    {
        _logger = logger;
        _groupingService = groupingService;
        _analysisService = analysisService;
        _activeJobs = new ConcurrentDictionary<string, PrintJob>();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public bool IsMonitoring => _monitoringTask != null && !_monitoringTask.IsCompleted;

    public event EventHandler<PrintJob> PrintJobCreated;
    public event EventHandler<PrintJob> PrintJobModified;
    public event EventHandler<PrintJob> PrintJobDeleted;

    public async Task StartMonitoringAsync()
    {
        if (IsMonitoring)
        {
            _logger.LogWarning("Print job monitoring is already running");
            return;
        }

        _monitoringTask = Task.Run(async () =>
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await MonitorPrintJobsAsync();
                    await Task.Delay(1000, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, no need to log
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in print job monitoring", ex);
            }
        }, _cancellationTokenSource.Token);

        _logger.LogInformation("Print job monitoring started");
    }

    public void StopMonitoring()
    {
        if (!IsMonitoring)
        {
            _logger.LogWarning("Print job monitoring is not running");
            return;
        }

        _cancellationTokenSource.Cancel();
        _monitoringTask?.Wait();
        _monitoringTask = null;
        _logger.LogInformation("Print job monitoring stopped");
    }

    private async Task MonitorPrintJobsAsync()
    {
        try
        {
            // Get all printers
            var printers = System.Drawing.Printing.PrinterSettings.InstalledPrinters.Cast<string>();

            foreach (var printer in printers)
            {
                // Get all jobs for the printer
                var jobIds = GetJobIds(printer);
                var currentJobs = new HashSet<string>();

                foreach (var jobId in jobIds)
                {
                    var jobKey = $"{printer}_{jobId}";
                    currentJobs.Add(jobKey);

                    var jobInfo = WinspoolHelper.GetJobInfo(printer, jobId);
                    if (!jobInfo.HasValue) continue;

                    var job = new PrintJob
                    {
                        JobId = jobId.ToString(),
                        DocumentName = jobInfo.Value.pDocument,
                        Owner = jobInfo.Value.pUserName,
                        PrinterName = printer,
                        TotalPages = (int)jobInfo.Value.TotalPages,
                        PagesPrinted = (int)jobInfo.Value.PagesPrinted,
                        Status = MapStatus(jobInfo.Value.Status),
                        SubmissionTime = new DateTime(
                            jobInfo.Value.Submitted.wYear,
                            jobInfo.Value.Submitted.wMonth,
                            jobInfo.Value.Submitted.wDay,
                            jobInfo.Value.Submitted.wHour,
                            jobInfo.Value.Submitted.wMinute,
                            jobInfo.Value.Submitted.wSecond,
                            jobInfo.Value.Submitted.wMilliseconds),
                        MachineName = jobInfo.Value.pMachineName,
                        DataType = jobInfo.Value.pDatatype,
                        PrintProcessor = jobInfo.Value.pPrintProcessor,
                        DriverName = jobInfo.Value.pDriverName,
                        Priority = (int)jobInfo.Value.Priority,
                        Size = jobInfo.Value.Size
                    };

                    if (_activeJobs.TryGetValue(jobKey, out var existingJob))
                    {
                        if (HasJobChanged(existingJob, job))
                        {
                            _activeJobs[jobKey] = job;
                            PrintJobModified?.Invoke(this, job);
                        }
                    }
                    else
                    {
                        _activeJobs[jobKey] = job;
                        PrintJobCreated?.Invoke(this, job);
                        await _groupingService.ProcessNewJobAsync(job);

                        // Analyze the new job
                        var analysis = await _analysisService.AnalyzeJobAsync(job);
                        _logger.LogInformation($"Document analysis: {analysis}");
                    }
                }

                // Check for deleted jobs
                var deletedJobs = _activeJobs.Keys.Where(k => k.StartsWith(printer + "_") && !currentJobs.Contains(k));
                foreach (var jobKey in deletedJobs)
                {
                    if (_activeJobs.TryRemove(jobKey, out var job))
                    {
                        PrintJobDeleted?.Invoke(this, job);
                    }
                }
            }

            _groupingService.CleanupOldGroups();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error monitoring print jobs", ex);
        }
    }

    private IEnumerable<uint> GetJobIds(string printerName)
    {
        try
        {
            return WinspoolHelper.GetJobIds(printerName);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to get job IDs for printer {printerName}", ex);
            return Array.Empty<uint>();
        }
    }

    private bool HasJobChanged(PrintJob existing, PrintJob current)
    {
        return existing.Status != current.Status ||
               existing.PagesPrinted != current.PagesPrinted ||
               existing.TotalPages != current.TotalPages ||
               existing.Priority != current.Priority;
    }

    private PrintJobStatus MapStatus(uint status)
    {
        if ((status & 0x00000100) != 0) return PrintJobStatus.Paused;
        if ((status & 0x00000200) != 0) return PrintJobStatus.Error;
        if ((status & 0x00000400) != 0) return PrintJobStatus.Deleted;
        if ((status & 0x00001000) != 0) return PrintJobStatus.Completed;
        return PrintJobStatus.Printing;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            StopMonitoring();
            _cancellationTokenSource.Dispose();
        }

        _isDisposed = true;
    }
}