Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class Win32 {
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);
}
"@

Write-Host "Key Detection Test"
Write-Host "Press Z+M together and hold for 2 seconds"
Write-Host "Press Ctrl+C to exit"
Write-Host ""

$lastCheck = Get-Date

while ($true) {
    $now = Get-Date
    if (($now - $lastCheck).TotalMilliseconds -gt 200) {
        $zPressed = [Win32]::GetAsyncKeyState(90) -band 0x8000  # Z key
        $mPressed = [Win32]::GetAsyncKeyState(77) -band 0x8000  # M key
        $wPressed = [Win32]::GetAsyncKeyState(87) -band 0x8000  # W key
        $onePressed = [Win32]::GetAsyncKeyState(49) -band 0x8000  # 1 key
        
        if ($zPressed -and $mPressed) {
            Write-Host "$(Get-Date -Format 'HH:mm:ss') - Z+M DETECTED!" -ForegroundColor Green
            
            # Show notification
            Add-Type -AssemblyName System.Windows.Forms
            $notify = New-Object System.Windows.Forms.NotifyIcon
            $notify.Icon = [System.Drawing.SystemIcons]::Information
            $notify.Visible = $true
            $notify.ShowBalloonTip(3000, 'Test Success', 'Z+M combination detected!', [System.Windows.Forms.ToolTipIcon]::Info)
            Start-Sleep -Seconds 1
            $notify.Dispose()
            
            Start-Sleep -Milliseconds 1000  # Prevent multiple triggers
        }
        elseif ($zPressed -and $wPressed) {
            Write-Host "$(Get-Date -Format 'HH:mm:ss') - Z+W DETECTED!" -ForegroundColor Yellow
            Start-Sleep -Milliseconds 1000
        }
        elseif ($zPressed -and $onePressed) {
            Write-Host "$(Get-Date -Format 'HH:mm:ss') - Z+1 DETECTED!" -ForegroundColor Red
            Start-Sleep -Milliseconds 1000
        }
        elseif ($zPressed) {
            Write-Host "$(Get-Date -Format 'HH:mm:ss') - Z pressed (waiting for combo key...)" -ForegroundColor Cyan
        }
        
        $lastCheck = $now
    }
    
    Start-Sleep -Milliseconds 50
}