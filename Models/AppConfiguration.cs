using System.Collections.Generic;

namespace PrinterJobInterceptor.Models;

public class AppConfiguration
{
    public MonitoringSettings Monitoring { get; set; } = new();
    public PrinterSettings Printers { get; set; } = new();
    public DocumentAnalysisSettings DocumentAnalysis { get; set; } = new();
    public StorageSettings Storage { get; set; } = new();
}

public class MonitoringSettings
{
    public bool Enabled { get; set; } = true;
    public int PollingIntervalSeconds { get; set; } = 5;
    public int GroupTimeoutMinutes { get; set; } = 60;
}

public class PrinterSettings
{
    public List<string> EnabledPrinters { get; set; } = new();
    public List<string> ExcludedPrinters { get; set; } = new();
}

public class DocumentAnalysisSettings
{
    public int MaxFileSizeMB { get; set; } = 100;
    public List<string> SupportedExtensions { get; set; } = new();
}

public class StorageSettings
{
    public string LogDirectory { get; set; } = "Logs";
    public int JobHistoryRetentionDays { get; set; } = 30;
}
