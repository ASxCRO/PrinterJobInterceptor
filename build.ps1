# Build script for Printer Job Interceptor

# Configuration
$version = "1.0.0"
$configuration = "Release"
$outputDir = ".\dist"
$publishDir = "$outputDir\publish"
$releaseDir = "$outputDir\release"

# Clean previous builds
Write-Host "Cleaning previous builds..."
if (Test-Path $outputDir) {
    Remove-Item -Path $outputDir -Recurse -Force
}

# Create output directories
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null

# Restore NuGet packages
Write-Host "Restoring NuGet packages..."
dotnet restore

# Build the solution
Write-Host "Building solution..."
dotnet build -c $configuration

# Run tests
Write-Host "Running tests..."
dotnet test -c $configuration

# Publish the application
Write-Host "Publishing application..."
dotnet publish -c $configuration -o $publishDir --self-contained true -r win-x64

# Copy additional files
Write-Host "Copying additional files..."
Copy-Item "README.md" -Destination $publishDir
Copy-Item "TEST_PLAN.md" -Destination $publishDir
Copy-Item "LICENSE" -Destination $publishDir

# Create release package
Write-Host "Creating release package..."
$zipFile = "$releaseDir\PrinterJobInterceptor-v$version.zip"
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipFile

# Create installer
Write-Host "Creating installer..."
$installerDir = "$releaseDir\installer"
New-Item -ItemType Directory -Path $installerDir -Force | Out-Null

# Create installer script
$installerScript = @"
@echo off
echo Installing Printer Job Interceptor v$version...

REM Check for administrative privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo This installer requires administrative privileges.
    echo Please run as administrator.
    pause
    exit /b 1
)

REM Create installation directory
set INSTALL_DIR=%ProgramFiles%\PrinterJobInterceptor
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

REM Copy files
xcopy /E /I /Y "%~dp0\*" "%INSTALL_DIR%"

REM Create desktop shortcut
set SHORTCUT="%USERPROFILE%\Desktop\Printer Job Interceptor.lnk"
powershell -Command "`$WshShell = New-Object -ComObject WScript.Shell; `$Shortcut = `$WshShell.CreateShortcut(%SHORTCUT%); `$Shortcut.TargetPath = '%INSTALL_DIR%\PrinterJobInterceptor.exe'; `$Shortcut.Save()"

echo Installation complete!
pause
"@

$installerScript | Out-File -FilePath "$installerDir\install.bat" -Encoding ASCII

# Copy files to installer directory
Copy-Item "$publishDir\*" -Destination $installerDir -Recurse

# Create installer package
$installerZip = "$releaseDir\PrinterJobInterceptor-Setup-v$version.zip"
Compress-Archive -Path "$installerDir\*" -DestinationPath $installerZip

Write-Host "Build completed successfully!"
Write-Host "Release package: $zipFile"
Write-Host "Installer package: $installerZip" 