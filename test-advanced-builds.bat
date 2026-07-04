@echo off
echo Testing Advanced Stealth Builds...
echo.

REM Check if builds exist
if not exist "dist-advanced\process-hollow\svchost.exe" (
    echo ❌ Process Hollowing version not found!
    echo Run build-advanced-stealth.bat first
    goto :check_obfuscated
)

echo ✅ Process Hollowing version found: dist-advanced\process-hollow\svchost.exe
echo    Size: 
dir "dist-advanced\process-hollow\svchost.exe" | find "svchost.exe"

:check_obfuscated
if not exist "dist-advanced\obfuscated\WindowsUpdateService.exe" (
    echo ❌ Obfuscated version not found!
    echo Run build-advanced-stealth.bat first
    goto :end
)

echo ✅ Obfuscated version found: dist-advanced\obfuscated\WindowsUpdateService.exe
echo    Size:
dir "dist-advanced\obfuscated\WindowsUpdateService.exe" | find "WindowsUpdateService.exe"

echo.
echo 🚀 TESTING INSTRUCTIONS:
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
echo 3. Test Shortcuts:
echo    Z+M - Status notification
echo    Z+W - AI query (clipboard content)
echo    Z+1 - Self-destruct (⚠️ DELETES FILES!)
echo.
echo 4. Check Process Names:
echo    Open Task Manager and verify process names
echo    Process Hollow: Should show as "svchost.exe"
echo    Obfuscated: Should show as "WindowsUpdateService.exe"
echo.

:end
echo Press any key to continue...
pause >nul