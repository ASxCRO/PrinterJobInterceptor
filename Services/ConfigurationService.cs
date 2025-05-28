using PrinterJobInterceptor.Models;
using PrinterJobInterceptor.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace PrinterJobInterceptor.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly ILoggingService _logger;
    private readonly string _configPath;
    private AppConfiguration _configuration;

    public AppConfiguration Configuration => _configuration;

    public ConfigurationService(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PrinterInterceptor",
            "config.json");
        _configuration = new AppConfiguration();
    }

    public async Task LoadConfigurationAsync()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                _logger.LogInformation("Configuration file not found.");
                return;
            }

            var json = await File.ReadAllTextAsync(_configPath);
            _configuration = JsonSerializer.Deserialize<AppConfiguration>(json);
            _logger.LogInformation("Configuration loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error loading configuration", ex);
            _configuration = new AppConfiguration();
        }
    }

    public async Task SaveConfigurationAsync()
    {
        try
        {
            var directory = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_configPath, json);
            _logger.LogInformation("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error saving configuration", ex);
            throw;
        }
    }

    public async Task ResetToDefaultsAsync()
    {
        _configuration = new AppConfiguration();
        await SaveConfigurationAsync();
        _logger.LogInformation("Configuration reset to defaults");
    }
}