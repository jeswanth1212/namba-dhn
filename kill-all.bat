@echo off
echo ========================================
echo Killing ALL Stealth Assistant Processes
echo ========================================
echo.

echo Stopping .NET EXE versions...
taskkill /F /IM svchost.exe 2>nul
taskkill /F /IM WindowsUpdateService.exe 2>nul
taskkill /F /IM explorer.exe 2>nul
taskkill /F /IM dotnet.exe 2>nul

echo Stopping LOLBin script versions...
taskkill /F /IM wscript.exe 2>nul
taskkill /F /IM cscript.exe 2>nul
taskkill /F /IM mshta.exe 2>nul

echo Stopping PowerShell versions...
taskkill /F /IM powershell.exe /FI "WINDOWTITLE eq *Shell*" 2>nul

echo Stopping batch script versions...
taskkill /F /IM cmd.exe /FI "WINDOWTITLE eq *stealth*" 2>nul
taskkill /F /IM cmd.exe /FI "WINDOWTITLE eq *service*" 2>nul

echo.
echo ========================================
echo All Stealth Assistant Processes Killed!
echo ========================================
echo.
echo Terminated:
echo ✓ svchost.exe (Process Hollowing)
echo ✓ WindowsUpdateService.exe (Obfuscated)
echo ✓ explorer.exe (Hollow version)
echo ✓ wscript.exe (VBScript LOLBin)
echo ✓ cscript.exe (VBScript LOLBin)
echo ✓ mshta.exe (HTA LOLBin)
echo ✓ powershell.exe (PowerShell versions)
echo ✓ cmd.exe (Batch script versions)
echo.
echo Note: Some legitimate Windows processes may restart automatically.
echo.
pause