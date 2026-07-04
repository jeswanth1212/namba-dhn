@echo off
echo Testing Key Detection Debug
echo.
echo This will open a debug window that shows key detection in real-time
echo Hold Z+M keys and see if they are detected
echo.
echo Starting debug window...
start mshta.exe debug-keys.hta
echo.
echo Debug window opened. Hold Z and M keys to test detection.
echo Close the debug window when done.
pause