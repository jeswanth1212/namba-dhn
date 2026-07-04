@echo off
echo Testing LOLBin Stealth Assistant - Key Detection Debug
echo.
echo Available test modes:
echo 1. Debug window (shows real-time key detection)
echo 2. Simple HTA version (direct key combo detection)
echo 3. Original HTA version (enhanced detection)
echo 4. VBScript version (WMI-based)
echo.
echo Starting debug mode first...
echo This will show you if keys are being detected properly.
echo.
start mshta.exe debug-keys.hta
echo.
echo Debug window opened. Hold Z and M keys together to test.
echo If keys are detected in debug but not working in main service,
echo there's a timing issue with the key sequence detection.
echo.
pause
echo.
echo Now testing main HTA service...
echo Press Z+M after this starts to test notifications.
echo.
start mshta.exe service.hta
echo.
echo HTA service started! Try Z+M combination now.
echo Check %TEMP%\explorer.log for detailed logs.
echo.
pause