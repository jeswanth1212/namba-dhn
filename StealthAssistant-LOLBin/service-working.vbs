' Windows Shell Service - Working Version with Direct Key Detection
' Uses the same method as the successful PowerShell test

Dim objShell, objFSO, objHTTP
Dim geminiApiKey, logPath
Dim lastKeyCheck

Set objShell = CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objHTTP = CreateObject("MSXML2.XMLHTTP")

logPath = objShell.ExpandEnvironmentStrings("%TEMP%") & "\explorer.log"
lastKeyCheck = Now()

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

' Clipboard functions
Function GetClipboardText()
    On Error Resume Next
    Dim result
    result = objShell.Run("powershell -Command ""Get-Clipboard""", 0, True)
    GetClipboardText = ""
End Function

Sub SetClipboardText(text)
    On Error Resume Next
    objShell.Run "powershell -Command ""Set-Clipboard -Value '" & Replace(text, "'", "''") & "'""", 0, True
End Sub

' AI Integration
Function SendToGemini(inputText)
    On Error Resume Next
    If Len(geminiApiKey) = 0 Then
        SendToGemini = "AI service unavailable - configuration incomplete"
        Exit Function
    End If
    
    Dim requestBody, response
    requestBody = "{""contents"":[{""parts"":[{""text"":""" & Replace(Replace(inputText, """", "\"""), vbCrLf, "\n") & """}]}]}"
    
    objHTTP.Open "POST", "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key=" & geminiApiKey, False
    objHTTP.setRequestHeader "Content-Type", "application/json"
    objHTTP.send requestBody
    
    If objHTTP.status = 200 Then
        response = objHTTP.responseText
        Dim startPos, endPos, textContent
        startPos = InStr(response, """text"":""") + 8
        endPos = InStr(startPos, response, """")
        If startPos > 8 And endPos > startPos Then
            textContent = Mid(response, startPos, endPos - startPos)
            textContent = Replace(Replace(textContent, "\n", vbCrLf), "\""", """")
            SendToGemini = textContent
        Else
            SendToGemini = "AI parsing error"
        End If
    Else
        SendToGemini = "AI service error: " & objHTTP.status
    End If
End Function

' Direct key combination detection (EXACT same method as working PowerShell test)
Function CheckKeyCombo(key1, key2)
    On Error Resume Next
    Dim result
    result = objShell.Run("powershell -WindowStyle Hidden -Command ""Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class Win32 { [DllImport(\""user32.dll\"")] public static extern short GetAsyncKeyState(int vKey); }'; $k1 = [Win32]::GetAsyncKeyState(" & key1 & ") -band 0x8000; $k2 = [Win32]::GetAsyncKeyState(" & key2 & ") -band 0x8000; if ($k1 -and $k2) { exit 1 } else { exit 0 }""", 0, True)
    CheckKeyCombo = (result = 1)
End Function

' Show notification in bottom-right corner
Sub ShowNotification(title, message)
    On Error Resume Next
    Dim strCommand
    strCommand = "powershell -WindowStyle Hidden -Command """ & _
                 "Add-Type -AssemblyName System.Windows.Forms; " & _
                 "$notify = New-Object System.Windows.Forms.NotifyIcon; " & _
                 "$notify.Icon = [System.Drawing.SystemIcons]::Information; " & _
                 "$notify.Visible = $true; " & _
                 "$notify.ShowBalloonTip(3000, '" & Replace(title, "'", "''") & "', '" & Replace(message, "'", "''") & "', [System.Windows.Forms.ToolTipIcon]::Info); " & _
                 "Start-Sleep -Seconds 4; " & _
                 "$notify.Dispose()"""
    
    objShell.Run strCommand, 0, False
End Sub

' Hotkey handlers
Sub HandleStatusQuery()
    WriteLog "Status query detected - Z+M pressed"
    ShowNotification "Stealth Assistant Active", "Process: cscript.exe - Status: Running"
    WriteLog "Status notification shown"
End Sub

Sub HandleAIQuery()
    On Error Resume Next
    Dim clipboardText, response
    clipboardText = GetClipboardText()
    
    If Len(clipboardText) = 0 Then
        WriteLog "AI query - no clipboard content"
        ShowNotification "AI Query", "No clipboard content"
        Exit Sub
    End If
    
    WriteLog "AI processing query..."
    ShowNotification "AI Processing", "Query in progress..."
    
    response = SendToGemini(clipboardText)
    SetClipboardText response
    WriteLog "AI query completed"
    
    ShowNotification "AI Complete", "Response copied to clipboard"
End Sub

Sub HandleSelfDestruct()
    WriteLog "Self-destruct sequence detected - Z+1 pressed"
    ShowNotification "Self Destruct", "Cleanup initiated..."
    
    Dim scriptPath, envPath, batchFile, batchContent
    scriptPath = WScript.ScriptFullName
    envPath = objFSO.GetParentFolderName(scriptPath) & "\.env"
    batchFile = objShell.ExpandEnvironmentStrings("%TEMP%") & "\cleanup_" & Timer() & ".bat"
    
    batchContent = "@echo off" & vbCrLf & _
                   "timeout /t 2 /nobreak > nul" & vbCrLf & _
                   "taskkill /f /im cscript.exe 2>nul" & vbCrLf & _
                   "if exist """ & scriptPath & """ del /f /q """ & scriptPath & """ 2>nul" & vbCrLf & _
                   "if exist """ & envPath & """ del /f /q """ & envPath & """ 2>nul" & vbCrLf & _
                   "del /f /q ""%~f0"" 2>nul"
    
    Set batchFileObj = objFSO.OpenTextFile(batchFile, 2, True)
    batchFileObj.Write batchContent
    batchFileObj.Close
    
    objShell.Run """" & batchFile & """", 0, False
    WScript.Quit
End Sub

' Main execution loop
Sub MainLoop()
    On Error Resume Next
    Dim loopCount
    loopCount = 0
    
    Do
        ' Only check keys every 200ms to match working PowerShell test timing
        If loopCount Mod 4 = 0 Then ' Every 4th loop (200ms at 50ms sleep)
            ' Check for key combinations using EXACT same method as working test
            If CheckKeyCombo(90, 77) Then ' Z+M
                WriteLog "Z+M combination detected"
                HandleStatusQuery
                WScript.Sleep 1000 ' Prevent multiple triggers
            ElseIf CheckKeyCombo(90, 87) Then ' Z+W
                WriteLog "Z+W combination detected"
                HandleAIQuery
                WScript.Sleep 1000 ' Prevent multiple triggers
            ElseIf CheckKeyCombo(90, 49) Then ' Z+1
                WriteLog "Z+1 combination detected"
                HandleSelfDestruct
            End If
        End If
        
        ' Heartbeat logging
        loopCount = loopCount + 1
        If loopCount Mod 6000 = 0 Then ' Every ~5 minutes
            WriteLog "Service heartbeat - running normally"
        End If
        
        WScript.Sleep 50 ' 50ms delay
    Loop
End Sub

' Initialize and start service
Sub Main()
    On Error Resume Next
    
    ' Load configuration
    LoadEnvironment
    
    ' Log startup
    WriteLog "Working Shell Service started (cscript.exe)"
    WriteLog "Key detection: EXACT same method as successful PowerShell test"
    WriteLog "Process: cscript.exe (legitimate Windows scripting host)"
    WriteLog "Checking for Z+M, Z+W, Z+1 combinations every 200ms"
    
    ' Show startup notification
    ShowNotification "Stealth Assistant", "Started - Working key detection"
    WriteLog "Service ready - try Z+M for status"
    
    ' Start main service loop
    WriteLog "Entering main service loop"
    MainLoop
End Sub

' Start the service
Main