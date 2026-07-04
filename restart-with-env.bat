@echo off
echo Restarting Stealth Assistant with API key...
echo.

REM Kill current svchost.exe
echo Stopping current svchost.exe...
taskkill /F /IM svchost.exe 2>nul

REM Wait a moment
timeout /t 2 /nobreak >nul

REM Start with environment variable loaded
echo Starting svchost.exe with API key...
cd dist-advanced\process-hollow
call start.bat

echo.
echo Done! Stealth Assistant restarted with API key loaded.
echo Try Z+W now with text in clipboard.
echo.