Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building Portable StealthAssistant" -ForegroundColor Green
Write-Host "Target: Maximum Windows Compatibility" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Cyan

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
if (Test-Path "dist") { Remove-Item -Recurse -Force "dist" }

# Restore packages
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n❌ Package restore failed!" -ForegroundColor Red
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# Build Release configuration
Write-Host "`nBuilding Release configuration..." -ForegroundColor Yellow
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n❌ Build failed!" -ForegroundColor Red
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# Publish self-contained single-file executable
Write-Host "`nPublishing self-contained executable..." -ForegroundColor Yellow
Write-Host "This may take a few minutes..." -ForegroundColor Gray

dotnet publish -c Release -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:PublishTrimmed=false `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o "dist"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n❌ Publish failed!" -ForegroundColor Red
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# Create .env template file
Write-Host "`nCreating .env template file..." -ForegroundColor Yellow
$envTemplate = "GEMINI_API_KEY=your_actual_gemini_api_key_here"
$envTemplate | Out-File -FilePath "dist\.env" -Encoding UTF8 -NoNewline

# Create README file
Write-Host "Creating README file..." -ForegroundColor Yellow

$readmeContent = @"
========================================
StealthAssistant - Quick Start
========================================

ACTIVE HOTKEYS (8 total):
--------------------------
Z+M = Status check
Z+W = Extract text and get AI response (auto-detects MCQ)
Z+J = Extract text and generate Java code
Z+Q = Extract text and generate Python code
Z+P = Extract text and generate C++ code
Z+E = Toggle clipboard viewer
Z+V = Auto-type from clipboard (10,000 chars/sec)
` (backtick) = Pause/resume auto-typing

SETUP:
------
1. Edit .env file with your Gemini API key
2. Get key from: https://aistudio.google.com/app/apikey
3. Run StealthAssistant.exe
4. Press Z+M to verify (shows "Running")

FEATURES:
---------
✅ Bypasses screenshot blocking (uses PrintWindow + Accessibility API)
✅ Bypasses paste detection (types at hardware level)
✅ Auto-detects MCQ questions
✅ Preserves code formatting (indentation, newlines)
✅ Works on locked/restricted apps

SYSTEM REQUIREMENTS:
--------------------
✅ Windows 10 version 1809+ or Windows 11
✅ 64-bit Windows
✅ No .NET installation needed

========================================
"@

$readmeContent | Out-File -FilePath "dist\README.txt" -Encoding UTF8

# Display success message
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "✅ BUILD SUCCESSFUL!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "📦 Distribution Package:" -ForegroundColor Yellow
Write-Host "  • dist\ folder contains StealthAssistant.exe + .env" -ForegroundColor White

Write-Host "`n🎯 Active Hotkeys:" -ForegroundColor Yellow
Write-Host "  Z+M = Status" -ForegroundColor White
Write-Host "  Z+W = Extract and Query (MCQ auto-detect)" -ForegroundColor White
Write-Host "  Z+J = Java code" -ForegroundColor White
Write-Host "  Z+Q = Python code" -ForegroundColor White
Write-Host "  Z+P = C++ code" -ForegroundColor White
Write-Host "  Z+E = Clipboard viewer" -ForegroundColor White
Write-Host "  Z+V = Auto-type" -ForegroundColor White
Write-Host "  `` = Pause/resume" -ForegroundColor White

Write-Host "`n⚠️  IMPORTANT:" -ForegroundColor Red
Write-Host "  1. Edit dist\.env with your API key" -ForegroundColor Yellow
Write-Host "  2. Get key from: https://aistudio.google.com/app/apikey" -ForegroundColor Cyan

Write-Host "`n🧪 To Test:" -ForegroundColor Yellow
Write-Host "  1. cd dist" -ForegroundColor White
Write-Host "  2. .\StealthAssistant.exe" -ForegroundColor White
Write-Host "  3. Press Z+M (should show 'Running')" -ForegroundColor White

Write-Host "`n========================================`n" -ForegroundColor Cyan
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')

