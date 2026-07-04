# Stealth Assistant - Notification System Implementation

## Overview
Both LOLBin versions now use **small bottom-right notifications** instead of dialog boxes, allowing seamless operation inside target software without interruption.

## Implemented Versions

### 1. HTA Version (mshta.exe)
- **File**: `service.hta`
- **Process**: `mshta.exe` (Microsoft HTML Application Host)
- **Start**: `start.bat` or `mshta.exe service.hta`
- **Features**: 
  - Small balloon notifications in bottom-right corner
  - No dialog boxes or popups
  - Completely hidden window
  - Uses PowerShell for notifications

### 2. VBScript Version (cscript.exe)
- **File**: `wmi-stealth.vbs`
- **Process**: `cscript.exe` (Windows Script Host)
- **Start**: `start-wmi.bat` or `cscript.exe wmi-stealth.vbs`
- **Features**:
  - WMI-based system integration
  - Small balloon notifications
  - Uses PowerShell for notifications
  - Legitimate Windows scripting process

## Notification Types

### Startup Notification
- **HTA**: "Stealth Assistant - Started successfully - Process: mshta.exe"
- **VBScript**: "Stealth Assistant - Started successfully"
- **Duration**: 2-3 seconds
- **Location**: Bottom-right corner

### Status Query (Z+M)
- **Message**: "Stealth Assistant Active - Process: [process_name] - Status: Running"
- **Purpose**: Confirm the service is working inside target software
- **Duration**: 3 seconds

### AI Query (Z+W)
- **Processing**: "AI Processing - Query in progress..."
- **Complete**: "AI Complete - Response copied to clipboard"
- **Purpose**: Show AI query status without interrupting workflow

### Self-Destruct (Z+1)
- **Message**: "Self Destruct - Cleanup initiated..."
- **Purpose**: Confirm cleanup process started
- **Duration**: 2 seconds (before deletion)

## Technical Implementation

### Notification Method
Both versions use PowerShell to create Windows balloon notifications:

```powershell
Add-Type -AssemblyName System.Windows.Forms
$notify = New-Object System.Windows.Forms.NotifyIcon
$notify.Icon = [System.Drawing.SystemIcons]::Information
$notify.Visible = $true
$notify.ShowBalloonTip(3000, 'Title', 'Message', [System.Windows.Forms.ToolTipIcon]::Info)
Start-Sleep -Seconds 4
$notify.Dispose()
```

### Key Features
- **Non-intrusive**: Small notifications that don't steal focus
- **Bottom-right placement**: Standard Windows notification area
- **Auto-dismiss**: Notifications disappear automatically
- **No taskbar presence**: Completely hidden from taskbar
- **Legitimate processes**: Uses Microsoft-signed binaries

## Usage Instructions

### Testing
1. Run `test-notifications.bat` to test both versions
2. Press `Z+M` to see status notification
3. Copy text to clipboard and press `Z+W` for AI query
4. Press `Z+1` for self-destruct (BE CAREFUL - permanently deletes files)

### Production Use
1. Choose either HTA or VBScript version
2. Run the appropriate start script
3. Use inside target software - notifications will appear in bottom-right
4. No dialog boxes will interrupt your workflow

## Stealth Features
- **LOLBin approach**: Uses legitimate Windows binaries
- **Process masquerading**: Appears as system services
- **No external dependencies**: Uses built-in Windows components
- **Minimal footprint**: Small file sizes and memory usage
- **Anti-detection**: Legitimate process names and behavior

## Files Structure
```
StealthAssistant-LOLBin/
├── service.hta              # HTA version with notifications
├── wmi-stealth.vbs         # VBScript version with notifications  
├── start.bat               # Start HTA version
├── start-wmi.bat           # Start VBScript version
├── test-notifications.bat  # Test both versions
├── .env                    # API configuration
└── NOTIFICATION_SYSTEM.md  # This documentation
```

## Security Notes
- Both versions permanently delete themselves with Z+1
- No traces left in recycle bin
- Logs stored in %TEMP%\explorer.log (can be deleted)
- Uses legitimate Windows processes for maximum stealth