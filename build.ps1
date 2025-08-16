Write-Host "Building Stealth Assistant..." -ForegroundColor Green

# Clean previous builds
if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
if (Test-Path "dist") { Remove-Item -Recurse -Force "dist" }

# Build the application
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore

Write-Host "Building application..." -ForegroundColor Yellow
dotnet build -c Release

if ($LASTEXITCODE -eq 0) {
    # Create single-file executable
    Write-Host "Creating single-file executable..." -ForegroundColor Yellow
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o "dist"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nBuild complete! Executable location: dist\StealthAssistant.exe" -ForegroundColor Green
        Write-Host "`nIMPORTANT: Create a .env file in the dist folder with your Gemini API key:" -ForegroundColor Red
        Write-Host "GEMINI_API_KEY=your_actual_api_key_here" -ForegroundColor Cyan
        
        # Create template .env file
        $envContent = "GEMINI_API_KEY=your_actual_api_key_here"
        $envContent | Out-File -FilePath "dist\.env" -Encoding UTF8
        Write-Host "`nTemplate .env file created in dist folder. Please update with your actual API key." -ForegroundColor Yellow
    } else {
        Write-Host "Publish failed!" -ForegroundColor Red
    }
} else {
    Write-Host "Build failed!" -ForegroundColor Red
}

Write-Host "`nPress any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

