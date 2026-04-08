# Force kill StealthAssistant and rebuild

Write-Host "Stopping StealthAssistant.exe..." -ForegroundColor Yellow
Stop-Process -Name "StealthAssistant" -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

Write-Host "Building new version with Z+C support..." -ForegroundColor Cyan
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful!" -ForegroundColor Green
    Write-Host "New exe ready at: dist\StealthAssistant.exe" -ForegroundColor Green
    Write-Host "`nZ+C hotkey is now available for compiler-aware typing!" -ForegroundColor Cyan
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
}

Write-Host "`nPress any key to exit..."
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
