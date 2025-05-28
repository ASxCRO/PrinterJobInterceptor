# Printer Job Interceptor

A Windows application that monitors and manages print jobs across multiple printers, with the ability to group related print jobs and control their execution.

## Features

- Real-time monitoring of print jobs across multiple printers
- Print job grouping based on document analysis
- Manual control of print jobs (pause/resume)
- Configurable group timeout settings
- Printer-specific monitoring controls
- Detailed job status and progress tracking
- Modern WPF user interface

## Requirements

- Windows 10 or later
- .NET 8.0 Runtime
- Administrative privileges (for printer management)

## Installation

1. Download the latest release from the Releases page
2. Extract the ZIP file to your desired location
3. Run `PrinterJobInterceptor.exe` as administrator

## Configuration

The application can be configured through the `appsettings.json` file:

```json
{
	"PrinterJobInterceptor": {
		"GroupTimeoutMinutes": 5,
		"EnabledPrinters": ["Printer1", "Printer2"],
		"Logging": {
			"LogLevel": "Information",
			"LogFilePath": "logs/app.log"
		}
	}
}
```

## Usage

1. Launch the application
2. Click "Start Monitoring" to begin tracking print jobs
3. Use the configuration panel to:
   - Set group timeout duration
   - Enable/disable specific printers
   - Monitor job status and progress
4. Use the job controls to:
   - Pause selected jobs
   - Resume paused jobs
   - View detailed job information

## Test Scenarios

### Basic Functionality

1. Start the application
2. Verify printer detection
3. Send a test print job
4. Confirm job appears in the monitoring list
5. Verify job status updates in real-time

### Job Grouping

1. Send multiple related print jobs
2. Verify automatic grouping based on document analysis
3. Test group timeout functionality
4. Verify group status updates

### Job Control

1. Select a print job
2. Pause the job
3. Verify job status changes
4. Resume the job
5. Confirm job continues printing

### Error Handling

1. Test with invalid printer configuration
2. Verify error messages display correctly
3. Test application recovery after errors

### Performance

1. Monitor multiple printers simultaneously
2. Test with high volume of print jobs
3. Verify UI responsiveness
4. Check memory usage

## Development

### Building from Source

1. Clone the repository
2. Open the solution in Visual Studio 2022
3. Restore NuGet packages
4. Build the solution

### Project Structure

- `Models/` - Data models and entities
- `Services/` - Core business logic and printer management
- `ViewModels/` - MVVM view models
- `Converters/` - XAML value converters
- `Helpers/` - Utility classes and helpers

### Dependencies

- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- Serilog
- System.Drawing.Common
- System.ServiceProcess.ServiceController

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Tagged v1.0.0