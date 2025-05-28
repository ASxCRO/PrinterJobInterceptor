using PrinterJobInterceptor.Models;

namespace PrinterJobInterceptor.Services.Interfaces;

public interface IConfigurationService
{
    AppConfiguration Configuration { get; }
    Task LoadConfigurationAsync();
    Task SaveConfigurationAsync();
    Task ResetToDefaultsAsync();
}
