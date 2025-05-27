namespace PrinterJobInterceptor;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrinterJobInterceptor.Services;
using PrinterJobInterceptor.Services.Interfaces;
using System.Windows;

public partial class App : Application
{
    public static IHost AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<MainWindow>();
                services.AddSingleton<ILoggingService, SerilogLoggingService>();
                services.AddSingleton<IConfigurationService, ConfigurationService>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost.StartAsync();

        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        base.OnExit(e);
    }
}
