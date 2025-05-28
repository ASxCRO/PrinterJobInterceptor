using Moq;
using PrinterJobInterceptor.Models;
using PrinterJobInterceptor.Services;
using PrinterJobInterceptor.Services.Interfaces;
using Xunit;

namespace PrinterJobInterceptor.Tests.Services;

public class DocumentAnalysisServiceTests
{
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly DocumentAnalysisService _service;

    public DocumentAnalysisServiceTests()
    {
        _loggerMock = new Mock<ILoggingService>();
        _service = new DocumentAnalysisService(_loggerMock.Object);
    }

    [Theory]
    [InlineData("test.pdf", DocumentType.PDF)]
    [InlineData("test.docx", DocumentType.OfficeDocument)]
    [InlineData("test.txt", DocumentType.Text)]
    [InlineData("test.jpg", DocumentType.Image)]
    [InlineData("test.png", DocumentType.Image)]
    [InlineData("test.unknown", DocumentType.Unknown)]
    public async Task AnalyzeJobAsync_WithDifferentExtensions_ReturnsCorrectType(string fileName, DocumentType expectedType)
    {
        // Arrange
        var job = CreateTestJob(fileName);

        // Act
        var result = await _service.AnalyzeJobAsync(job);

        // Assert
        Assert.Equal(expectedType, result.Type);
        Assert.Equal(Path.GetExtension(fileName).TrimStart('.'), result.FileExtension);
    }

    [Fact]
    public async Task AnalyzeJobAsync_WithPDFProcessor_ReturnsPDFType()
    {
        // Arrange
        var job = CreateTestJob("test.unknown");
        job.PrintProcessor = "PDF";

        // Act
        var result = await _service.AnalyzeJobAsync(job);

        // Assert
        Assert.Equal(DocumentType.PDF, result.Type);
    }

    [Fact]
    public async Task AnalyzeJobAsync_WithEMFDataType_ReturnsMixedType()
    {
        // Arrange
        var job = CreateTestJob("test.unknown");
        job.DataType = "EMF";

        // Act
        var result = await _service.AnalyzeJobAsync(job);

        // Assert
        Assert.Equal(DocumentType.Mixed, result.Type);
    }

    [Fact]
    public async Task AnalyzeJobAsync_WithRAWDataType_ReturnsTextType()
    {
        // Arrange
        var job = CreateTestJob("test.unknown");
        job.DataType = "RAW";

        // Act
        var result = await _service.AnalyzeJobAsync(job);

        // Assert
        Assert.Equal(DocumentType.Text, result.Type);
    }

    [Fact]
    public async Task AnalyzeJobAsync_ExtractsPrintSettings()
    {
        // Arrange
        var job = CreateTestJob("test.pdf");
        job.PrintProcessor = "ColorDuplex";

        // Act
        var result = await _service.AnalyzeJobAsync(job);

        // Assert
        Assert.True(result.IsColor);
        Assert.True(result.IsDuplex);
    }

    [Fact]
    public async Task AnalyzeJobAsync_HandlesNullValues()
    {
        // Arrange
        var job = CreateTestJob(null);

        // Act
        var result = await _service.AnalyzeJobAsync(job);

        // Assert
        Assert.Equal(DocumentType.Unknown, result.Type);
        Assert.Empty(result.FileExtension);
    }

    [Fact]
    public async Task AnalyzeJobAsync_LogsErrors()
    {
        // Arrange
        var job = CreateTestJob("test.pdf");
        _loggerMock.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()))
            .Verifiable();

        // Act
        var result = await _service.AnalyzeJobAsync(job);

        // Assert
        _loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
    }

    private static PrintJob CreateTestJob(string fileName)
    {
        return new PrintJob
        {
            JobId = Guid.NewGuid().ToString(),
            DocumentName = fileName,
            Owner = "TestUser",
            PrinterName = "TestPrinter",
            TotalPages = 1,
            PagesPrinted = 0,
            Status = PrintJobStatus.Printing,
            SubmissionTime = DateTime.Now,
            PrintProcessor = "WinPrint",
            DataType = "NT_PRINT_1",
            DriverName = "TestDriver"
        };
    }
} 