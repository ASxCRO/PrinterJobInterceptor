using PrinterJobInterceptor.Models;
using PrinterJobInterceptor.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PrinterJobInterceptor.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IPrintJobMonitorService _monitorService;
    private readonly IPrintJobGroupingService _groupingService;
    private readonly ILoggingService _logger;
    private readonly IDispatcherService _dispatcherService;
    private bool _isMonitoring;
    private int _groupTimeoutMinutes = 60;
    private ObservableCollection<string> _enabledPrinters;
    private ObservableCollection<PrintJobViewModel> _printJobs;
    private PrintJobViewModel _selectedJob;
    private string _statusMessage;
    private string _errorMessage;
    private bool _isBusy;
    private bool _hasError;

    public MainViewModel(
        IPrintJobMonitorService monitorService,
        IPrintJobGroupingService groupingService,
        ILoggingService logger,
        IDispatcherService dispatcherService)
    {
        _monitorService = monitorService;
        _groupingService = groupingService;
        _logger = logger;
        _dispatcherService = dispatcherService;
        _enabledPrinters = new ObservableCollection<string>();
        _printJobs = new ObservableCollection<PrintJobViewModel>();
        _statusMessage = "Ready";

        StartMonitoringCommand = new RelayCommand(StartMonitoring, () => !IsMonitoring && !IsBusy);
        StopMonitoringCommand = new RelayCommand(StopMonitoring, () => IsMonitoring && !IsBusy);
        PauseJobCommand = new RelayCommand(PauseSelectedJob, () => SelectedJob?.CanPause == true && !IsBusy);
        ResumeJobCommand = new RelayCommand(ResumeSelectedJob, () => SelectedJob?.CanResume == true && !IsBusy);

        SubscribeToEvents();
    }

    public bool IsMonitoring
    {
        get => _isMonitoring;
        private set => SetProperty(ref _isMonitoring, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public bool HasError
    {
        get => _hasError;
        private set => SetProperty(ref _hasError, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                HasError = !string.IsNullOrEmpty(value);
            }
        }
    }

    public int GroupTimeoutMinutes
    {
        get => _groupTimeoutMinutes;
        set
        {
            if (SetProperty(ref _groupTimeoutMinutes, value))
            {
                _groupingService.UpdateGroupTimeout(TimeSpan.FromMinutes(value));
            }
        }
    }

    public ObservableCollection<string> EnabledPrinters
    {
        get => _enabledPrinters;
        private set => SetProperty(ref _enabledPrinters, value);
    }

    public ObservableCollection<PrintJobViewModel> PrintJobs
    {
        get => _printJobs;
        private set => SetProperty(ref _printJobs, value);
    }

    public PrintJobViewModel SelectedJob
    {
        get => _selectedJob;
        set => SetProperty(ref _selectedJob, value);
    }

    public ICommand StartMonitoringCommand { get; }
    public ICommand StopMonitoringCommand { get; }
    public ICommand PauseJobCommand { get; }
    public ICommand ResumeJobCommand { get; }

    private void SubscribeToEvents()
    {
        _monitorService.PrintJobCreated += OnPrintJobCreated;
        _monitorService.PrintJobModified += OnPrintJobModified;
        _monitorService.PrintJobDeleted += OnPrintJobDeleted;
    }

    private void OnPrintJobCreated(object sender, PrintJob job)
    {
        _dispatcherService.Invoke(() =>
        {
            var viewModel = PrintJobViewModel.FromPrintJob(job);
            PrintJobs.Add(viewModel);
            _logger.LogInformation($"New print job created: {job.DocumentName}");
        });
    }

    private void OnPrintJobModified(object sender, PrintJob job)
    {
        _dispatcherService.Invoke(() =>
        {
            var viewModel = PrintJobs.FirstOrDefault(j => j.JobId == job.JobId);
            if (viewModel != null)
            {
                viewModel.DocumentName = job.DocumentName;
                viewModel.PagesPrinted = job.PagesPrinted;
                viewModel.Status = job.Status;
                _logger.LogInformation($"Print job modified: {job.DocumentName}");
            }
        });
    }

    private void OnPrintJobDeleted(object sender, PrintJob job)
    {
        _dispatcherService.Invoke(() =>
        {
            var viewModel = PrintJobs.FirstOrDefault(j => j.JobId == job.JobId);
            if (viewModel != null)
            {
                PrintJobs.Remove(viewModel);
                _logger.LogInformation($"Print job deleted: {job.DocumentName}");
            }
        });
    }

    private async void StartMonitoring()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Starting print job monitoring...";
            ErrorMessage = null;

            await _monitorService.StartMonitoringAsync();
            IsMonitoring = true;
            StatusMessage = "Monitoring active";
            _logger.LogInformation("Print job monitoring started");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to start monitoring: {ex.Message}";
            _logger.LogError("Failed to start monitoring", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void StopMonitoring()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Stopping print job monitoring...";
            ErrorMessage = null;

            _monitorService.StopMonitoring();
            IsMonitoring = false;
            StatusMessage = "Monitoring stopped";
            _logger.LogInformation("Print job monitoring stopped");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to stop monitoring: {ex.Message}";
            _logger.LogError("Failed to stop monitoring", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void PauseSelectedJob()
    {
        if (SelectedJob == null) return;

        try
        {
            IsBusy = true;
            StatusMessage = $"Pausing print job: {SelectedJob.DocumentName}";
            ErrorMessage = null;

            // TODO: Implement pause functionality
            await Task.Delay(100); // Simulated operation
            _logger.LogInformation($"Pausing print job: {SelectedJob.DocumentName}");
            
            StatusMessage = "Job paused successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to pause print job: {ex.Message}";
            _logger.LogError($"Failed to pause print job: {SelectedJob.DocumentName}", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ResumeSelectedJob()
    {
        if (SelectedJob == null) return;

        try
        {
            IsBusy = true;
            StatusMessage = $"Resuming print job: {SelectedJob.DocumentName}";
            ErrorMessage = null;

            // TODO: Implement resume functionality
            await Task.Delay(100); // Simulated operation
            _logger.LogInformation($"Resuming print job: {SelectedJob.DocumentName}");
            
            StatusMessage = "Job resumed successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to resume print job: {ex.Message}";
            _logger.LogError($"Failed to resume print job: {SelectedJob.DocumentName}", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

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
}
