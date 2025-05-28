using System.ComponentModel;
using System.Runtime.CompilerServices;
using PrinterJobInterceptor.Models;

namespace PrinterJobInterceptor.ViewModels;

public class PrintJobViewModel : INotifyPropertyChanged
{
    private string _jobId;
    private string _documentName;
    private string _owner;
    private string _printerName;
    private int _totalPages;
    private int _pagesPrinted;
    private PrintJobStatus _status;
    private DateTime _submissionTime;
    private string _groupName;
    private bool _isSelected;

    public string JobId
    {
        get => _jobId;
        set => SetProperty(ref _jobId, value);
    }

    public string DocumentName
    {
        get => _documentName;
        set => SetProperty(ref _documentName, value);
    }

    public string Owner
    {
        get => _owner;
        set => SetProperty(ref _owner, value);
    }

    public string PrinterName
    {
        get => _printerName;
        set => SetProperty(ref _printerName, value);
    }

    public int TotalPages
    {
        get => _totalPages;
        set => SetProperty(ref _totalPages, value);
    }

    public int PagesPrinted
    {
        get => _pagesPrinted;
        set => SetProperty(ref _pagesPrinted, value);
    }

    public PrintJobStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public DateTime SubmissionTime
    {
        get => _submissionTime;
        set => SetProperty(ref _submissionTime, value);
    }

    public string GroupName
    {
        get => _groupName;
        set => SetProperty(ref _groupName, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string StatusText => Status.ToString();
    public string ProgressText => $"{PagesPrinted} / {TotalPages} pages";
    public bool CanPause => Status == PrintJobStatus.Printing;
    public bool CanResume => Status == PrintJobStatus.Paused;

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public static PrintJobViewModel FromPrintJob(PrintJob job)
    {
        return new PrintJobViewModel
        {
            JobId = job.JobId,
            DocumentName = job.DocumentName,
            Owner = job.Owner,
            PrinterName = job.PrinterName,
            TotalPages = job.TotalPages,
            PagesPrinted = job.PagesPrinted,
            Status = job.Status,
            SubmissionTime = job.SubmissionTime
        };
    }
} 