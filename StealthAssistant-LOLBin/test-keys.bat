@echo off
echo Testing Key Detection with PowerShell
echo.
echo This will test if Z+M, Z+W, Z+1 combinations are detected properly.
echo Hold the keys together for at least 1 second.
echo.
echo Starting PowerShell key detection test...
echo Press Ctrl+C in the PowerShell window to stop.
echo.
powershell -ExecutionPolicy Bypass -File test-keys.ps1
pause