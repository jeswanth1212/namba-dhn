@echo off
cd /d "%~dp0"
echo Starting Working Stealth Assistant (VBScript version)
echo.
echo This version uses the EXACT same key detection method
echo that worked in the PowerShell test.
echo.
echo Test shortcuts:
echo Z+M - Status notification
echo Z+W - AI query  
echo Z+1 - Self-destruct
echo.
echo Starting service...
cscript.exe //nologo service-working.vbs