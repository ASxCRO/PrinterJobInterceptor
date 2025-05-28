using Moq;
using PrinterJobInterceptor.Models;
using PrinterJobInterceptor.Services.Interfaces;
using PrinterJobInterceptor.ViewModels;
using Xunit;

namespace PrinterJobInterceptor.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly Mock<IPrintJobMonitorService> _monitorServiceMock;
    private readonly Mock<IPrintJobGroupingService> _groupingServiceMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<IDispatcherService> _dispatcherMock;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _dispatcherMock = new Mock<IDispatcherService>();
        _monitorServiceMock = new Mock<IPrintJobMonitorService>();
        _groupingServiceMock = new Mock<IPrintJobGroupingService>();
        _loggerMock = new Mock<ILoggingService>();
        _viewModel = new MainViewModel(
            _monitorServiceMock.Object,
            _groupingServiceMock.Object,
            _loggerMock.Object,
            _dispatcherMock.Object);
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Assert
        Assert.False(_viewModel.IsMonitoring);
        Assert.Equal(60, _viewModel.GroupTimeoutMinutes);
        Assert.Empty(_viewModel.EnabledPrinters);
        Assert.Empty(_viewModel.PrintJobs);
        Assert.Null(_viewModel.SelectedJob);
    }

    [Fact]
    public void StartMonitoring_StartsMonitorService()
    {
        // Act
         _viewModel.StartMonitoringCommand.Execute(null);

        // Assert
        _monitorServiceMock.Verify(x => x.StartMonitoringAsync(), Times.Once);
        Assert.True(_viewModel.IsMonitoring);
    }

    [Fact]
    public void StopMonitoring_StopsMonitorService()
    {
        // Arrange
        _viewModel.StartMonitoringCommand.Execute(null);

        // Act
        _viewModel.StopMonitoringCommand.Execute(null);

        // Assert
        _monitorServiceMock.Verify(x => x.StopMonitoring(), Times.Once);
        Assert.False(_viewModel.IsMonitoring);
    }

    [Fact]
    public void OnPrintJobCreated_AddsJobToCollection()
    {
        // Arrange
        var job = CreateTestJob();

        // Act
        _monitorServiceMock.Raise(x => x.PrintJobCreated += null, this, job);

        // Assert
        Assert.Single(_viewModel.PrintJobs);
        Assert.Equal(job.DocumentName, _viewModel.PrintJobs[0].DocumentName);
    }

    [Fact]
    public void OnPrintJobModified_UpdatesExistingJob()
    {
        // Arrange
        var job = CreateTestJob();
        _monitorServiceMock.Raise(x => x.PrintJobCreated += null, this, job);
        job.PagesPrinted = 5;

        // Act
        _monitorServiceMock.Raise(x => x.PrintJobModified += null, this, job);

        // Assert
        Assert.Single(_viewModel.PrintJobs);
        Assert.Equal(5, _viewModel.PrintJobs[0].PagesPrinted);
    }

    [Fact]
    public void OnPrintJobDeleted_RemovesJobFromCollection()
    {
        // Arrange
        var job = CreateTestJob();
        _monitorServiceMock.Raise(x => x.PrintJobCreated += null, this, job);

        // Act
        _monitorServiceMock.Raise(x => x.PrintJobDeleted += null, this, job);

        // Assert
        Assert.Empty(_viewModel.PrintJobs);
    }

    [Fact]
    public void PauseJob_WhenJobSelected_LogsAction()
    {
        // Arrange
        var job = CreateTestJob();
        _monitorServiceMock.Raise(x => x.PrintJobCreated += null, this, job);
        _viewModel.SelectedJob = _viewModel.PrintJobs[0];

        // Act
        _viewModel.PauseJobCommand.Execute(null);

        // Assert
        _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Pausing print job"))), Times.Once);
    }

    [Fact]
    public void ResumeJob_WhenJobSelected_LogsAction()
    {
        // Arrange
        var job = CreateTestJob();
        _monitorServiceMock.Raise(x => x.PrintJobCreated += null, this, job);
        _viewModel.SelectedJob = _viewModel.PrintJobs[0];

        // Act
        _viewModel.ResumeJobCommand.Execute(null);

        // Assert
        _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Resuming print job"))), Times.Once);
    }

    private static PrintJob CreateTestJob()
    {
        return new PrintJob
        {
            JobId = Guid.NewGuid().ToString(),
            DocumentName = "test.pdf",
            Owner = "TestUser",
            PrinterName = "TestPrinter",
            TotalPages = 10,
            PagesPrinted = 0,
            Status = PrintJobStatus.Printing,
            SubmissionTime = DateTime.Now
        };
    }
} 