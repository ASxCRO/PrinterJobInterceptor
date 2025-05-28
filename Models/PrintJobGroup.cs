namespace PrinterJobInterceptor.Models;

public class PrintJobGroup
{
    public string GroupId { get; set; }
    public string DocumentName { get; set; }
    public string Owner { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime LastModifiedTime { get; set; }
    public List<PrintJob> Jobs { get; set; }

    public int TotalPages => Jobs?.Sum(j => j.TotalPages) ?? 0;
    public int PagesPrinted => Jobs?.Sum(j => j.PagesPrinted) ?? 0;
    public PrintJobStatus Status => Jobs?.Any(j => j.Status == PrintJobStatus.Error) == true 
        ? PrintJobStatus.Error 
        : Jobs?.All(j => j.Status == PrintJobStatus.Completed) == true 
            ? PrintJobStatus.Completed 
            : PrintJobStatus.Printing;

    public bool IsComplete => Status == PrintJobStatus.Completed;
    public bool IsError => Status == PrintJobStatus.Error;
    public bool IsPrinting => Status == PrintJobStatus.Printing;

    public void AddJob(PrintJob job)
    {
        Jobs.Add(job);
        if (CreatedTime == default || job.SubmissionTime < CreatedTime)
        {
            CreatedTime = job.SubmissionTime;
        }
        if (LastModifiedTime == default || job.SubmissionTime > LastModifiedTime)
        {
            LastModifiedTime = job.SubmissionTime;
        }
    }

    public async Task PauseAllAsync()
    {
        foreach (var job in Jobs)
        {
            await job.PauseAsync();
        }
    }

    public async Task ResumeAllAsync()
    {
        foreach (var job in Jobs)
        {
            await job.ResumeAsync();
        }
    }

    public async Task CancelAllAsync()
    {
        foreach (var job in Jobs)
        {
            await job.CancelAsync();
        }
    }
} 