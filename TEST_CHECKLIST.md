# StealthAssistant - Testing Checklist

## Pre-Test Setup
- [ ] Copy dist/StealthAssistant.exe to test location
- [ ] Edit .env file with valid Gemini API key
- [ ] Close any running instances
- [ ] Run as Administrator (recommended)

## Hotkey Tests

### Z+M - Status Check
- [ ] Press Z then M (within 1 second)
- [ ] Expected: Popup shows "Running" in bottom-right
- [ ] Popup disappears after 2 seconds
- [ ] ✅ PASS / ❌ FAIL

### Z+W - Extract & Query
- [ ] Open any application with text
- [ ] Display text on screen
- [ ] Press Z then W
- [ ] Expected: Text extracted, AI response in clipboard
- [ ] Popup shows summary
- [ ] ✅ PASS / ❌ FAIL

### Z+J - Java Code
- [ ] Copy requirement to clipboard (e.g., "create quicksort")
- [ ] Press Z then J
- [ ] Expected: Java code in clipboard
- [ ] Mouse flickers briefly
- [ ] Code includes all imports
- [ ] ✅ PASS / ❌ FAIL

### Z+P - Python Code
- [ ] Copy requirement to clipboard
- [ ] Press Z then P
- [ ] Expected: Python code in clipboard
- [ ] Mouse flickers briefly
- [ ] Code includes all imports
- [ ] ✅ PASS / ❌ FAIL

### Z+C - C++ Code
- [ ] Copy requirement to clipboard
- [ ] Press Z then C
- [ ] Expected: C++ code in clipboard
- [ ] Mouse flickers briefly
- [ ] Code includes all includes
- [ ] ✅ PASS / ❌ FAIL

### Z+E - Clipboard Viewer
- [ ] Copy text to clipboard
- [ ] Press Z then E
- [ ] Expected: Floating window shows clipboard content
- [ ] Window is scrollable
- [ ] Click anywhere to close
- [ ] ✅ PASS / ❌ FAIL

### Z+V - Fast Auto-Type
- [ ] Copy text to clipboard
- [ ] Click in target application
- [ ] Press Z then V
- [ ] Expected: Text types at 10,000 chars/sec
- [ ] Popup shows character count
- [ ] ✅ PASS / ❌ FAIL

### Z+B - Compiler Auto-Type
- [ ] Copy indented code to clipboard
- [ ] Click in IDE/editor
- [ ] Press Z then B
- [ ] Expected: Code types with stripped indentation
- [ ] IDE auto-formats properly
- [ ] ✅ PASS / ❌ FAIL

### ` (Backtick) - Pause/Resume
- [ ] Start auto-typing (Z+V or Z+B)
- [ ] Press ` (backtick) during typing
- [ ] Expected: Typing pauses
- [ ] Press ` again
- [ ] Expected: Typing resumes
- [ ] ✅ PASS / ❌ FAIL

## Stealth Tests

### Process Obfuscation
- [ ] Open Task Manager
- [ ] Find process
- [ ] Expected: Shows as "Windows Service Host" or similar
- [ ] Not obviously named "StealthAssistant"
- [ ] ✅ PASS / ❌ FAIL

### Screen Capture Protection
- [ ] Start screen sharing (Teams/Zoom/OBS)
- [ ] Trigger any hotkey that shows popup
- [ ] Expected: Popup NOT visible in screen share
- [ ] ✅ PASS / ❌ FAIL

- [ ] Open clipboard viewer (Z+E)
- [ ] Expected: Viewer NOT visible in screen share
- [ ] ✅ PASS / ❌ FAIL

### Memory Usage
- [ ] Check Task Manager
- [ ] Expected: ~50MB RAM usage
- [ ] CPU <1% when idle
- [ ] ✅ PASS / ❌ FAIL

### Alt+Tab Hiding
- [ ] Press Alt+Tab
- [ ] Expected: StealthAssistant NOT in window list
- [ ] ✅ PASS / ❌ FAIL

## Functional Tests

### Conversation History
- [ ] Use Z+W to ask question
- [ ] Use Z+W again with follow-up question
- [ ] Expected: AI remembers context
- [ ] ✅ PASS / ❌ FAIL

### Text Extraction Methods
- [ ] Test Z+W on different applications:
  - [ ] Notepad
  - [ ] Browser
  - [ ] PDF viewer
  - [ ] Protected application
- [ ] Expected: At least one method succeeds
- [ ] ✅ PASS / ❌ FAIL

### Paste Detection Bypass
- [ ] Find application that blocks paste (Ctrl+V)
- [ ] Copy text to clipboard
- [ ] Use Z+V to auto-type
- [ ] Expected: Text enters successfully
- [ ] ✅ PASS / ❌ FAIL

## Error Handling Tests

### Empty Clipboard
- [ ] Clear clipboard
- [ ] Press Z+V
- [ ] Expected: Popup shows "Clipboard is empty!"
- [ ] ✅ PASS / ❌ FAIL

### Invalid API Key
- [ ] Edit .env with invalid key
- [ ] Restart application
- [ ] Try Z+W
- [ ] Expected: Error popup or no response
- [ ] ✅ PASS / ❌ FAIL

### No Internet
- [ ] Disconnect internet
- [ ] Try Z+W
- [ ] Expected: Error popup or timeout
- [ ] ✅ PASS / ❌ FAIL

## Performance Tests

### Large Text Auto-Type
- [ ] Copy 10,000+ characters
- [ ] Use Z+V
- [ ] Expected: Types smoothly at 10,000 chars/sec
- [ ] Can pause/resume with `
- [ ] ✅ PASS / ❌ FAIL

### Multiple Hotkeys Rapidly
- [ ] Press Z+M, Z+M, Z+M quickly
- [ ] Expected: Multiple popups appear
- [ ] No crashes or hangs
- [ ] ✅ PASS / ❌ FAIL

### Long Running
- [ ] Leave application running for 1+ hour
- [ ] Test hotkeys periodically
- [ ] Expected: Still responsive
- [ ] Memory usage stable
- [ ] ✅ PASS / ❌ FAIL

## Edge Cases

### Hotkey Timing
- [ ] Press Z, wait 2 seconds, press M
- [ ] Expected: Nothing happens (timeout)
- [ ] ✅ PASS / ❌ FAIL

### Multiple Instances
- [ ] Try to run second instance
- [ ] Expected: Second instance exits immediately
- [ ] Only one instance runs
- [ ] ✅ PASS / ❌ FAIL

### Special Characters
- [ ] Copy text with emojis, unicode, special chars
- [ ] Use Z+V to auto-type
- [ ] Expected: All characters type correctly
- [ ] ✅ PASS / ❌ FAIL

## Final Verification

### Documentation
- [ ] README.md is accurate
- [ ] dist/README.txt is accurate
- [ ] All hotkeys documented
- [ ] ✅ PASS / ❌ FAIL

### Build Quality
- [ ] No crashes during testing
- [ ] No error popups (except expected)
- [ ] Smooth performance
- [ ] ✅ PASS / ❌ FAIL

### Stealth Quality
- [ ] Process name obfuscated
- [ ] Popups hidden from screen share
- [ ] No taskbar presence
- [ ] Hidden from Alt+Tab
- [ ] ✅ PASS / ❌ FAIL

---

## Test Results Summary

**Total Tests:** 40+
**Passed:** ___
**Failed:** ___
**Skipped:** ___

**Overall Status:** ✅ PASS / ❌ FAIL

**Tester:** _______________
**Date:** _______________
**Version:** 1.0 (Final Clean Build)

---

## Notes

(Add any observations, issues, or comments here)
