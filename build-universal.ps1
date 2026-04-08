Write-Host "Building Universal Windows Executable..." -ForegroundColor Green
Write-Host "Target: All Windows 10/11 versions (1809+)" -ForegroundColor Yellow

# Clean previous builds
if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
if (Test-Path "dist") { Remove-Item -Recurse -Force "dist" }

# Build the application
Write-Host "`nRestoring packages..." -ForegroundColor Yellow
dotnet restore

Write-Host "Building application..." -ForegroundColor Yellow
dotnet build -c Release

if ($LASTEXITCODE -eq 0) {
    # Create universal single-file executable
    Write-Host "`nCreating universal single-file executable..." -ForegroundColor Yellow
    Write-Host "This will work on all Windows 10/11 devices (1607+)" -ForegroundColor Cyan
    
    dotnet publish -c Release -r win-x64 --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishReadyToRun=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -p:PublishTrimmed=false `
        -o "dist"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ Build complete!" -ForegroundColor Green
        Write-Host "Executable location: dist\StealthAssistant.exe" -ForegroundColor Cyan
        Write-Host "`n📋 Compatibility Information:" -ForegroundColor Yellow
        Write-Host "  • Windows 10 version 1809 (October 2018 Update) or later" -ForegroundColor White
        Write-Host "  • Windows 11 (all versions)" -ForegroundColor White
        Write-Host "  • Self-contained (no .NET installation required)" -ForegroundColor White
        Write-Host "  • Single-file executable (portable)" -ForegroundColor White
        Write-Host "`n⚠️  IMPORTANT: Create a .env file in the dist folder with your Gemini API key:" -ForegroundColor Red
        Write-Host "GEMINI_API_KEY=your_actual_api_key_here" -ForegroundColor Cyan
        
        # Create template .env file
        $envContent = "GEMINI_API_KEY=your_actual_api_key_here"
        $envContent | Out-File -FilePath "dist\.env" -Encoding UTF8
        Write-Host "`n✅ Template .env file created in dist folder." -ForegroundColor Green
        Write-Host "   Please update with your actual API key." -ForegroundColor Yellow
    } else {
        Write-Host "`n❌ Publish failed!" -ForegroundColor Red
    }
} else {
    Write-Host "`n❌ Build failed!" -ForegroundColor Red
}

Write-Host "`nPress any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

