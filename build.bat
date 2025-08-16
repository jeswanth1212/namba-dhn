@echo off
echo Building Stealth Assistant...

REM Clean previous builds
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"

REM Build the application
dotnet restore
dotnet build -c Release

REM Create single-file executable
echo Creating single-file executable...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o "dist"

echo.
echo Build complete! Executable location: dist\StealthAssistant.exe
echo.
echo IMPORTANT: Create a .env file in the dist folder with your Gemini API key:
echo GEMINI_API_KEY=your_actual_api_key_here
echo.
pause

