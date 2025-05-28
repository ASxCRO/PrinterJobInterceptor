namespace PrinterJobInterceptor;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrinterJobInterceptor.Models;
using PrinterJobInterceptor.Services;
using PrinterJobInterceptor.Services.Interfaces;
using PrinterJobInterceptor.ViewModels;
using System;
using System.IO;
using System.Windows;

public partial class App : Application
{
    private readonly IHost _host;
    public static IConfiguration Configuration { get; private set; }
    public static AppConfiguration AppConfig { get; private set; }

    public App()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables();

        Configuration = builder.Build();
        AppConfig = Configuration.GetSection("PrinterJobInterceptor").Get<AppConfiguration>();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register services
                services.AddSingleton<IDispatcherService, DispatcherService>();
                services.AddSingleton<ILoggingService, SerilogLoggingService>();
                services.AddSingleton<IPrintJobMonitorService, PrintJobMonitorService>();
                services.AddSingleton<IPrintJobGroupingService, PrintJobGroupingService>();
                services.AddSingleton<IDocumentAnalysisService, DocumentAnalysisService>();

                // Register ViewModels
                services.AddTransient<MainViewModel>();

                // Register configuration
                services.AddSingleton(Configuration);
                services.AddSingleton(AppConfig);
            })
            .Build();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = new MainWindow
        {
            DataContext = _host.Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host.Dispose();
        base.OnExit(e);
    }
}
