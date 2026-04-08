@echo off
echo ========================================
echo Building Portable StealthAssistant
echo Target: Maximum Windows Compatibility
echo ========================================
echo.

echo Cleaning previous builds...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"
if exist "dist" rmdir /s /q "dist"
if exist "dist-clean" rmdir /s /q "dist-clean"

echo.
echo Restoring NuGet packages...
dotnet restore
if errorlevel 1 goto error

echo.
echo Building Release configuration...
dotnet build -c Release
if errorlevel 1 goto error

echo.
echo Publishing self-contained executable...
echo This may take a few minutes...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=false -p:DebugType=embedded -o "dist"
if errorlevel 1 goto error

echo.
echo Creating clean distribution...
mkdir "dist-clean"
copy "dist\StealthAssistant.exe" "dist-clean\" >nul

echo.
echo Creating .env template files...
echo GEMINI_API_KEY=your_actual_gemini_api_key_here> "dist\.env"
echo GEMINI_API_KEY=your_actual_gemini_api_key_here> "dist-clean\.env"

echo.
echo ========================================
echo BUILD SUCCESSFUL!
echo ========================================
echo.
echo Distribution Packages Created:
echo   - dist\ (full package with debug symbols)
echo   - dist-clean\ (clean package, smaller size)
echo.
echo Compatibility:
echo   - Windows 10 version 1809+ (October 2018)
echo   - Windows 11 (all versions)
echo   - Self-contained (no .NET required)
echo   - Single-file executable (portable)
echo.
echo IMPORTANT - Before Running:
echo   1. Edit dist\.env or dist-clean\.env
echo   2. Replace with your real Gemini API key
echo   3. Get key from: https://aistudio.google.com/app/apikey
echo.
echo To Share With Others:
echo   - Share the entire dist-clean\ folder
echo   - Tell them to edit .env with their own API key
echo   - Works on any Windows 10 1809+ or Windows 11 PC
echo.
echo To Test:
echo   1. cd dist
echo   2. StealthAssistant.exe
echo   3. Press Z+M (should show 'Running' popup)
echo.
echo ========================================
echo.
pause
exit /b 0

:error
echo.
echo ========================================
echo BUILD FAILED!
echo ========================================
echo.
pause
exit /b 1
