@echo off
echo Building Simplified Advanced Stealth Versions...
echo.

REM Create output directories
if not exist "dist-advanced" mkdir "dist-advanced"
if not exist "dist-advanced\process-hollow" mkdir "dist-advanced\process-hollow"
if not exist "dist-advanced\obfuscated" mkdir "dist-advanced\obfuscated"

echo Building Process Hollowing Version (Simplified)...
cd StealthAssistant-ProcessHollow
dotnet clean -c Release
dotnet restore
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo Process Hollowing build failed!
    cd ..
    goto :build_obfuscated
)

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../dist-advanced/process-hollow/
if %ERRORLEVEL% NEQ 0 (
    echo Process Hollowing publish failed!
    cd ..
    goto :build_obfuscated
)
cd ..

:build_obfuscated
echo Building Obfuscated Version (Simplified)...
cd StealthAssistant-Obfuscated
dotnet clean -c Release
dotnet restore
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo Obfuscated build failed!
    cd ..
    goto :copy_env
)

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../dist-advanced/obfuscated/
if %ERRORLEVEL% NEQ 0 (
    echo Obfuscated publish failed!
    cd ..
    goto :copy_env
)
cd ..

:copy_env
REM Copy .env files
if exist ".env" (
    copy ".env" "dist-advanced\process-hollow\.env" >nul 2>&1
    copy ".env" "dist-advanced\obfuscated\.env" >nul 2>&1
)

echo.
echo ========================================
echo Advanced Stealth Builds Status:
echo ========================================

if exist "dist-advanced\process-hollow\svchost.exe" (
    echo ✅ Process Hollowing Version: BUILT SUCCESSFULLY
    echo    📁 Location: dist-advanced\process-hollow\svchost.exe
    echo    🎯 Features: Process hollowing, legitimate svchost.exe name
    echo    📊 Size: 
    dir "dist-advanced\process-hollow\svchost.exe" | find "svchost.exe"
    echo.
) else (
    echo ❌ Process Hollowing Version: BUILD FAILED
    echo.
)

if exist "dist-advanced\obfuscated\WindowsUpdateService.exe" (
    echo ✅ Obfuscated Version: BUILT SUCCESSFULLY  
    echo    📁 Location: dist-advanced\obfuscated\WindowsUpdateService.exe
    echo    🎯 Features: String encryption, anti-debugging, anti-sandbox
    echo    📊 Size:
    dir "dist-advanced\obfuscated\WindowsUpdateService.exe" | find "WindowsUpdateService.exe"
    echo.
) else (
    echo ❌ Obfuscated Version: BUILD FAILED
    echo.
)

echo 🚀 TESTING INSTRUCTIONS:
echo ========================
echo.
echo 1. Test Process Hollowing Version:
echo    cd dist-advanced\process-hollow
echo    svchost.exe
echo    (Should appear as legitimate svchost.exe in Task Manager)
echo.
echo 2. Test Obfuscated Version:
echo    cd dist-advanced\obfuscated
echo    WindowsUpdateService.exe
echo    (Should appear as Windows Update Service)
echo.
echo 3. Test Shortcuts (both versions):
echo    Z+M - Status notification
echo    Z+W - AI query (processes clipboard content)
echo    Z+1 - Self-destruct
echo.
echo 4. Verify Stealth:
echo    - Open Task Manager
echo    - Look for process names (svchost.exe or WindowsUpdateService.exe)
echo    - Should appear as legitimate Windows processes
echo.
echo ⚠️  ADVANCED STEALTH FEATURES:
echo ================================
echo 🔹 Process Hollowing - Injects into legitimate svchost.exe
echo 🔹 String Encryption - All strings encrypted to avoid detection
echo 🔹 Dynamic API Loading - Windows APIs loaded at runtime
echo 🔹 Anti-Debugging - Multiple debugger detection methods
echo 🔹 Anti-Sandbox - VM and sandbox environment detection
echo 🔹 Legitimate Process Names - Appears as Windows services
echo 🔹 Memory Obfuscation - Random memory patterns
echo.
echo WARNING: Use responsibly and only for legitimate purposes!
echo.
pause