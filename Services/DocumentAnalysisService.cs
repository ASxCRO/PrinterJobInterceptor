using System.Text.RegularExpressions;
using PrinterJobInterceptor.Helpers;
using PrinterJobInterceptor.Models;
using PrinterJobInterceptor.Services.Interfaces;

namespace PrinterJobInterceptor.Services;

public class DocumentAnalysisService : IDocumentAnalysisService
{
    private readonly ILoggingService _logger;
    private static readonly Regex _fileExtensionRegex = new(@"\.([^.]+)$", RegexOptions.Compiled);
    private static readonly Dictionary<string, DocumentType> _extensionToType = new()
    {
        { "txt", DocumentType.Text },
        { "rtf", DocumentType.Text },
        { "doc", DocumentType.OfficeDocument },
        { "docx", DocumentType.OfficeDocument },
        { "xls", DocumentType.OfficeDocument },
        { "xlsx", DocumentType.OfficeDocument },
        { "ppt", DocumentType.OfficeDocument },
        { "pptx", DocumentType.OfficeDocument },
        { "pdf", DocumentType.PDF },
        { "jpg", DocumentType.Image },
        { "jpeg", DocumentType.Image },
        { "png", DocumentType.Image },
        { "gif", DocumentType.Image },
        { "bmp", DocumentType.Image },
        { "tiff", DocumentType.Image },
        { "tif", DocumentType.Image }
    };

    public DocumentAnalysisService(ILoggingService logger)
    {
        _logger = logger;
    }

    public async Task<DocumentAnalysis> AnalyzeJobAsync(PrintJob job)
    {
        try
        {
            var analysis = new DocumentAnalysis
            {
                DocumentName = job.DocumentName,
                PageCount = job.TotalPages,
                FileSize = job.Size,
                PrinterName = job.PrinterName,
                PrintProcessor = job.PrintProcessor,
                DriverName = job.DriverName
            };

            // Extract file extension and determine document type
            var extension = ExtractFileExtension(job.DocumentName);
            analysis.FileExtension = extension;
            analysis.Type = DetermineDocumentType(extension, job);

            // Extract additional metadata from print processor and driver
            ExtractPrintSettings(analysis, job);

            // For PDF and Office documents, try to extract more metadata
            if (analysis.Type == DocumentType.PDF || analysis.Type == DocumentType.OfficeDocument)
            {
                await ExtractDocumentMetadataAsync(analysis, job);
            }

            _logger.LogInformation($"Analyzed document: {analysis}");
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error analyzing job {job.JobId}", ex);
            return new DocumentAnalysis
            {
                DocumentName = job.DocumentName,
                Type = DocumentType.Unknown,
                PageCount = job.TotalPages,
                FileSize = job.Size
            };
        }
    }

    private string ExtractFileExtension(string fileName)
    {
        var match = _fileExtensionRegex.Match(fileName);
        return match.Success ? match.Groups[1].Value.ToLowerInvariant() : string.Empty;
    }

    private DocumentType DetermineDocumentType(string extension, PrintJob job)
    {
        // First try to determine from file extension
        if (_extensionToType.TryGetValue(extension, out var type))
        {
            return type;
        }

        // If no extension match, try to determine from print processor and data type
        if (job.PrintProcessor?.Contains("PDF", StringComparison.OrdinalIgnoreCase) == true)
        {
            return DocumentType.PDF;
        }

        if (job.DataType?.Contains("EMF", StringComparison.OrdinalIgnoreCase) == true)
        {
            return DocumentType.Mixed;
        }

        if (job.DataType?.Contains("RAW", StringComparison.OrdinalIgnoreCase) == true)
        {
            return DocumentType.Text;
        }

        return DocumentType.Unknown;
    }

    private void ExtractPrintSettings(DocumentAnalysis analysis, PrintJob job)
    {
        // Extract print settings from job data type and print processor
        if (job.DataType != null)
        {
            analysis.Metadata["DataType"] = job.DataType;
        }

        if (job.PrintProcessor != null)
        {
            analysis.Metadata["PrintProcessor"] = job.PrintProcessor;
        }

        // Try to determine color/duplex from print processor name
        if (job.PrintProcessor != null)
        {
            analysis.IsColor = job.PrintProcessor.Contains("Color", StringComparison.OrdinalIgnoreCase);
            analysis.IsDuplex = job.PrintProcessor.Contains("Duplex", StringComparison.OrdinalIgnoreCase);
        }

        // Default to standard paper size if not specified
        analysis.PaperSize = "A4";
        analysis.Orientation = "Portrait";
        analysis.Copies = 1;
    }

    private async Task ExtractDocumentMetadataAsync(DocumentAnalysis analysis, PrintJob job)
    {
        try
        {
            if (analysis.Type == DocumentType.PDF)
            {
                // TODO: Implement PDF metadata extraction
                
            }
            else if (analysis.Type == DocumentType.OfficeDocument)
            {
                // TODO: Implement Office document metadata extraction
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to extract additional metadata for {job.DocumentName}");
        }
    }
} 