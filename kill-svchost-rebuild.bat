@echo off
echo Killing svchost.exe from dist-advanced folder...
taskkill /F /FI "IMAGENAME eq svchost.exe" /FI "WINDOWTITLE eq *" 2>nul
timeout /t 2 /nobreak >nul

echo Rebuilding...
dotnet publish StealthAssistant-ProcessHollow/StealthAssistant-ProcessHollow.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -p:DebugSymbols=false -o dist-advanced/process-hollow

echo.
echo Starting svchost.exe with Ctrl+ hotkeys...
cd dist-advanced\process-hollow
start /B svchost.exe
cd ..\..

echo.
echo ========================================
echo Stealth Assistant Running!
echo ========================================
echo Hotkeys:
echo   Ctrl+M - Status
echo   Ctrl+W - AI Query (clipboard)
echo   Ctrl+1 - Self Destruct
echo ========================================
pause
