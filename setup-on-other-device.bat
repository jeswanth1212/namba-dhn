@echo off
echo ========================================
echo StealthAssistant Setup Script
echo ========================================
echo.

REM Check if .env file exists
if exist ".env" (
    echo .env file already exists.
    echo.
    choice /C YN /M "Do you want to edit it"
    if errorlevel 2 goto :end
    if errorlevel 1 goto :edit
) else (
    echo Creating .env file...
    goto :create
)

:create
echo.
echo Please enter your Gemini API key.
echo You can get it from: https://aistudio.google.com/app/apikey
echo.
set /p API_KEY="Enter your API key: "

if "%API_KEY%"=="" (
    echo Error: API key cannot be empty!
    pause
    exit /b 1
)

echo GEMINI_API_KEY=%API_KEY% > .env
echo.
echo .env file created successfully!
echo.
goto :end

:edit
echo.
echo Opening .env file in Notepad...
timeout /t 2 >nul
notepad .env
goto :end

:end
echo.
echo Setup complete!
echo.
echo To run StealthAssistant:
echo   1. Double-click StealthAssistant.exe
echo   2. Press Z+M to verify it's working
echo.
pause



