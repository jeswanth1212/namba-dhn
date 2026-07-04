@echo off
echo ========================================
echo NUCLEAR OPTION: Killing ALL Scripts
echo ========================================
echo.

echo Killing ALL PowerShell processes...
taskkill /F /IM powershell.exe 2>nul

echo Killing ALL cmd.exe processes (except this one)...
for /f "skip=1 tokens=2" %%a in ('tasklist /FI "IMAGENAME eq cmd.exe" /FO TABLE /NH') do (
    if not "%%a"=="%~p0" (
        taskkill /F /PID %%a 2>nul
    )
)

echo Killing ALL script hosts...
taskkill /F /IM wscript.exe 2>nul
taskkill /F /IM cscript.exe 2>nul
taskkill /F /IM mshta.exe 2>nul

echo Killing ALL .NET build processes...
taskkill /F /IM dotnet.exe 2>nul
taskkill /F /IM MSBuild.exe 2>nul

echo.
echo ========================================
echo All Scripts Terminated!
echo ========================================
echo.
echo Now ONLY use the compiled EXE version:
echo   cd dist-advanced\process-hollow
echo   svchost.exe
echo.
echo This version should NOT be detected because:
echo   - It's a compiled .NET EXE (not a script)
echo   - No "script:" prefix
echo   - Legitimate process name
echo.
pause