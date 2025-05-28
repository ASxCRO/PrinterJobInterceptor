namespace PrinterJobInterceptor.Models;

public enum DocumentType
{
    Unknown,
    Text,
    Image,
    Mixed,
    PDF,
    OfficeDocument
}

public class DocumentAnalysis
{
    public string DocumentName { get; set; }
    public DocumentType Type { get; set; }
    public int PageCount { get; set; }
    public long FileSize { get; set; }
    public string FileExtension { get; set; }
    public Dictionary<string, string> Metadata { get; } = new();
    public bool IsColor { get; set; }
    public bool IsDuplex { get; set; }
    public string PaperSize { get; set; }
    public string Orientation { get; set; }
    public int Copies { get; set; }
    public string PrinterName { get; set; }
    public string PrintProcessor { get; set; }
    public string DriverName { get; set; }
    public DateTime AnalysisTime { get; } = DateTime.Now;

    public override string ToString()
    {
        return $"{DocumentName} ({Type}, {PageCount} pages, {FileSize:N0} bytes)";
    }
} 