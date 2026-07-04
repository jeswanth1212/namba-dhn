' Windows Management Instrumentation Stealth Service
' Uses WMI (legitimate Windows service) to avoid detection

Dim objWMI, objShell, objFSO, objHTTP
Dim zPressed, lastZPress, geminiApiKey, logPath

Set objWMI = GetObject("winmgmts:")
Set objShell = CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objHTTP = CreateObject("MSXML2.XMLHTTP")

zPressed = False
lastZPress = Now()
logPath = objShell.ExpandEnvironmentStrings("%TEMP%") & "\explorer.log"

' Load environment variables
Sub LoadEnvironment()
    On Error Resume Next
    Dim envFile, fileContent, line, parts
    envFile = objFSO.GetParentFolderName(WScript.ScriptFullName) & "\.env"
    
    If objFSO.FileExists(envFile) Then
        Set fileContent = objFSO.OpenTextFile(envFile, 1)
        Do While Not fileContent.AtEndOfStream
            line = Trim(fileContent.ReadLine())
            If Len(line) > 0 And Left(line, 1) <> "#" And InStr(line, "=") > 0 Then
                parts = Split(line, "=", 2)
                If UBound(parts) = 1 Then
                    objShell.Environment("Process")(Trim(parts(0))) = Trim(parts(1))
                End If
            End If
        Loop
        fileContent.Close
    End If
    
    geminiApiKey = objShell.Environment("Process")("GEMINI_API_KEY")
End Sub

' Logging function
Sub WriteLog(message)
    On Error Resume Next
    Dim logFile, logEntry
    logEntry = FormatDateTime(Now(), 0) & " " & FormatDateTime(Now(), 3) & " - " & message & vbCrLf
    
    Set logFile = objFSO.OpenTextFile(logPath, 8, True)
    logFile.Write logEntry
    logFile.Close
End Sub

' Clipboard functions using WMI
Function GetClipboardText()
    On Error Resume Next
    ' Use PowerShell through WMI to get clipboard
    Dim objProcess, strCommand, objExecObject, strOutput
    strCommand = "powershell -Command ""Get-Clipboard"""
    
    Set objProcess = objWMI.Get("Win32_Process")
    objProcess.Create strCommand, , , intProcessID
    
    ' Wait and get result (simplified)
    WScript.Sleep 500
    GetClipboardText = ""
End Function

Sub SetClipboardText(text)
    On Error Resume Next
    ' Use PowerShell through WMI to set clipboard
    Dim objProcess, strCommand
    strCommand = "powershell -Command ""Set-Clipboard -Value '" & Replace(text, "'", "''") & "'"""
    
    Set objProcess = objWMI.Get("Win32_Process")
    objProcess.Create strCommand, , , intProcessID
End Sub

' AI Integration
Function SendToGemini(inputText)
    On Error Resume Next
    If Len(geminiApiKey) = 0 Then
        SendToGemini = "WMI AI service unavailable - configuration incomplete"
        Exit Function
    End If
    
    Dim requestBody, response
    requestBody = "{""contents"":[{""parts"":[{""text"":""" & Replace(Replace(inputText, """", "\"""), vbCrLf, "\n") & """}]}]}"
    
    objHTTP.Open "POST", "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key=" & geminiApiKey, False
    objHTTP.setRequestHeader "Content-Type", "application/json"
    objHTTP.send requestBody
    
    If objHTTP.status = 200 Then
        response = objHTTP.responseText
        ' Simple JSON parsing
        Dim startPos, endPos, textContent
        startPos = InStr(response, """text"":""") + 8
        endPos = InStr(startPos, response, """")
        If startPos > 8 And endPos > startPos Then
            textContent = Mid(response, startPos, endPos - startPos)
            textContent = Replace(Replace(textContent, "\n", vbCrLf), "\""", """")
            SendToGemini = textContent
        Else
            SendToGemini = "WMI AI parsing error"
        End If
    Else
        SendToGemini = "WMI AI service error: " & objHTTP.status
    End If
End Function

' Key monitoring using WMI
Function IsKeyPressed(keyCode)
    On Error Resume Next
    ' Use WMI to check key state through PowerShell
    Dim objProcess, strCommand
    strCommand = "powershell -Command ""Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class Win32 { [DllImport(\""user32.dll\"")] public static extern short GetAsyncKeyState(int vKey); }'; [Win32]::GetAsyncKeyState(" & keyCode & ") -band 0x8000"""
    
    Set objProcess = objWMI.Get("Win32_Process")
    objProcess.Create strCommand, , , intProcessID
    
    ' Simplified - in real implementation would capture output
    IsKeyPressed = False
End Function

' Hotkey handlers
Sub HandleStatusQuery()
    WriteLog "WMI status query detected - showing notification"
    ' Show small notification in bottom-right corner
    ShowNotification "Stealth Assistant Active", "Process: cscript.exe" & vbCrLf & "Status: Running", 3000
    WriteLog "Status notification shown successfully"
End Sub

Sub HandleAIQuery()
    On Error Resume Next
    Dim clipboardText, response
    clipboardText = GetClipboardText()
    
    If Len(clipboardText) = 0 Then
        WriteLog "WMI AI query - no clipboard content"
        ShowNotification "AI Query", "No clipboard content", 2000
        Exit Sub
    End If
    
    WriteLog "WMI AI processing user query..."
    ShowNotification "AI Processing", "Query in progress...", 2000
    
    response = SendToGemini(clipboardText)
    SetClipboardText response
    WriteLog "WMI AI query processed successfully"
    
    ShowNotification "AI Complete", "Response copied to clipboard", 3000
End Sub

Sub HandleSelfDestruct()
    On Error Resume Next
    WriteLog "WMI cleanup sequence detected"
    ShowNotification "Self Destruct", "Cleanup initiated...", 2000
    
    Dim scriptPath, envPath, objProcess, strCommand
    scriptPath = WScript.ScriptFullName
    envPath = objFSO.GetParentFolderName(scriptPath) & "\.env"
    
    ' Use WMI to create cleanup process
    strCommand = "cmd /c timeout /t 3 && del /f /q """ & scriptPath & """ && del /f /q """ & envPath & """"
    
    Set objProcess = objWMI.Get("Win32_Process")
    objProcess.Create strCommand, , , intProcessID
    
    WScript.Quit
End Sub

' Show notification in bottom-right corner
Sub ShowNotification(title, message, duration)
    On Error Resume Next
    ' Use PowerShell to show Windows 10 toast notification
    Dim objProcess, strCommand
    strCommand = "powershell -WindowStyle Hidden -Command """ & _
                 "Add-Type -AssemblyName System.Windows.Forms; " & _
                 "$notify = New-Object System.Windows.Forms.NotifyIcon; " & _
                 "$notify.Icon = [System.Drawing.SystemIcons]::Information; " & _
                 "$notify.Visible = $true; " & _
                 "$notify.ShowBalloonTip(" & duration & ", '" & title & "', '" & Replace(message, vbCrLf, " - ") & "', [System.Windows.Forms.ToolTipIcon]::Info); " & _
                 "Start-Sleep -Seconds " & (duration/1000 + 1) & "; " & _
                 "$notify.Dispose()"""
    
    Set objProcess = objWMI.Get("Win32_Process")
    objProcess.Create strCommand, , , intProcessID
End Sub

' Main execution loop
Sub MainLoop()
    On Error Resume Next
    Dim loopCount
    loopCount = 0
    
    Do
        ' Simplified key checking (in real implementation would be more sophisticated)
        ' For demonstration purposes, we'll just log periodic activity
        
        loopCount = loopCount + 1
        If loopCount Mod 6000 = 0 Then ' Every ~10 minutes
            WriteLog "WMI service heartbeat - running normally"
        End If
        
        ' Simulate WMI queries (what legitimate WMI services do)
        If loopCount Mod 3000 = 0 Then ' Every ~5 minutes
            Dim colProcesses
            Set colProcesses = objWMI.ExecQuery("SELECT * FROM Win32_Process WHERE Name='explorer.exe'")
            WriteLog "WMI maintenance query completed"
        End If
        
        WScript.Sleep 100 ' 100ms delay
    Loop
End Sub

' Initialize and start service
Sub Main()
    On Error Resume Next
    
    ' Load configuration
    LoadEnvironment
    
    ' Log startup
    WriteLog "Windows Management Service started (WMI/cscript.exe implementation with notifications)"
    WriteLog "Service process: cscript.exe or wscript.exe (legitimate Windows scripting host)"
    WriteLog "Using WMI for system integration"
    WriteLog "Notification mode: ENABLED for testing"
    
    ' Show startup notification instead of dialog
    ShowNotification "Stealth Assistant", "Started successfully", 2000
    
    ' Start main service loop
    WriteLog "Entering WMI service loop with notification support"
    MainLoop
End Sub

' Start the service
Main