using PrinterJobInterceptor.Models;

namespace PrinterJobInterceptor.Services.Interfaces;

public interface IDocumentAnalysisService
{
    Task<DocumentAnalysis> AnalyzeJobAsync(PrintJob job);
} 