@echo off
echo Building Advanced Stealth Versions...
echo.

REM Create output directories
if not exist "dist-advanced" mkdir "dist-advanced"
if not exist "dist-advanced\process-hollow" mkdir "dist-advanced\process-hollow"
if not exist "dist-advanced\obfuscated" mkdir "dist-advanced\obfuscated"

echo Building Process Hollowing Version...
cd StealthAssistant-ProcessHollow
dotnet clean
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o ../dist-advanced/process-hollow/
if %ERRORLEVEL% NEQ 0 (
    echo Process Hollowing build failed! Trying without trimming...
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../dist-advanced/process-hollow/
    if %ERRORLEVEL% NEQ 0 (
        echo Process Hollowing build failed completely!
        cd ..
        goto :build_obfuscated
    )
)
cd ..

:build_obfuscated
echo Building Obfuscated Version...
cd StealthAssistant-Obfuscated
dotnet clean
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o ../dist-advanced/obfuscated/
if %ERRORLEVEL% NEQ 0 (
    echo Obfuscated build failed! Trying without trimming...
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../dist-advanced/obfuscated/
    if %ERRORLEVEL% NEQ 0 (
        echo Obfuscated build failed completely!
        cd ..
        goto :copy_env
    )
)
cd ..

:copy_env
REM Copy .env files
if exist ".env" (
    copy ".env" "dist-advanced\process-hollow\.env" >nul 2>&1
    copy ".env" "dist-advanced\obfuscated\.env" >nul 2>&1
)

echo.
echo Advanced Stealth Builds Status:
echo ==============================

if exist "dist-advanced\process-hollow\svchost.exe" (
    echo ✅ Process Hollowing Version: dist-advanced\process-hollow\svchost.exe
    echo    - Injects into legitimate svchost.exe process
    echo    - Uses process hollowing technique
    echo    - Appears as Windows Service Host
) else (
    echo ❌ Process Hollowing Version: BUILD FAILED
)

if exist "dist-advanced\obfuscated\WindowsUpdateService.exe" (
    echo ✅ Obfuscated Version: dist-advanced\obfuscated\WindowsUpdateService.exe
    echo    - Advanced string encryption
    echo    - Dynamic API loading
    echo    - Anti-debugging and anti-sandbox
    echo    - Multiple evasion techniques
) else (
    echo ❌ Obfuscated Version: BUILD FAILED
)

echo.
echo ADVANCED STEALTH FEATURES:
echo ========================
echo 1. Process Hollowing - Runs inside legitimate svchost.exe
echo 2. String Encryption - All strings encrypted
echo 3. Dynamic API Loading - APIs loaded at runtime
echo 4. Anti-Debugging - Multiple debugger detection methods
echo 5. Anti-Sandbox - VM and sandbox detection
echo 6. Memory Obfuscation - Random memory patterns
echo 7. Timing Attacks - Execution timing randomization
echo 8. Legitimate Process Names - Appears as Windows services
echo.

if exist "dist-advanced\process-hollow\svchost.exe" (
    echo 🚀 READY TO TEST:
    echo cd dist-advanced\process-hollow
    echo svchost.exe
    echo.
)

if exist "dist-advanced\obfuscated\WindowsUpdateService.exe" (
    echo 🚀 READY TO TEST:
    echo cd dist-advanced\obfuscated  
    echo WindowsUpdateService.exe
    echo.
)

echo WARNING: These are advanced evasion techniques!
echo Use responsibly and only for legitimate purposes.
echo.
pause