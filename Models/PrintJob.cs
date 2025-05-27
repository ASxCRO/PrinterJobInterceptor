using PrinterJobInterceptor.Helpers;

namespace PrinterJobInterceptor.Models;
public class PrintJob
{
    public string JobId { get; set; }
    public string DocumentName { get; set; }
    public string Owner { get; set; }
    public string PrinterName { get; set; }
    public int TotalPages { get; set; }
    public int TotalBytes { get; set; }
    public int PagesPrinted { get; set; }
    public PrintJobStatus Status { get; set; }
    public DateTime SubmissionTime { get; set; }
    public string MachineName { get; set; }
    public string DataType { get; set; }
    public string PrintProcessor { get; set; }
    public string DriverName { get; set; }
    public int Priority { get; set; }
    public long Size { get; set; }


    public async Task<bool> PauseAsync()
    {
        try
        {
            return await Task.Run(() => WinspoolHelper.PauseJob(PrinterName, uint.Parse(JobId)));
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ResumeAsync()
    {
        try
        {
            return await Task.Run(() => WinspoolHelper.ResumeJob(PrinterName, uint.Parse(JobId)));
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> CancelAsync()
    {
        try
        {
            return await Task.Run(() => WinspoolHelper.CancelJob(PrinterName, uint.Parse(JobId)));
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task RefreshAsync()
    {
        try
        {
            var jobInfo = await Task.Run(() => WinspoolHelper.GetJobInfo(PrinterName, uint.Parse(JobId)));
            if (jobInfo.HasValue)
            {
                var info = jobInfo.Value;
                DocumentName = info.pDocument;
                Owner = info.pUserName;
                TotalPages = (int)info.TotalPages;
                PagesPrinted = (int)info.PagesPrinted;
                Status = MapStatus(info.Status);
                Priority = (int)info.Priority;
                Size = info.Size;
                DataType = info.pDatatype;
                PrintProcessor = info.pPrintProcessor;
                DriverName = info.pDriverName;
            }
        }
        catch (Exception)
        {
            // TODO : Handle exceptions appropriately, maybe log them
        }
    }

    private PrintJobStatus MapStatus(uint status)
    {
        if ((status & 0x00000100) != 0) return PrintJobStatus.Paused;
        if ((status & 0x00000200) != 0) return PrintJobStatus.Error;
        if ((status & 0x00000400) != 0) return PrintJobStatus.Deleted;
        if ((status & 0x00001000) != 0) return PrintJobStatus.Completed;
        return PrintJobStatus.Printing;
    }
}
