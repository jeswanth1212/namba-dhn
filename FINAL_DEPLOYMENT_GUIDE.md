# Final Deployment Guide - Stealth Assistant

## ✅ Complete Feature List

### Global Hotkeys (11 Total)

| Hotkey | Function | Description |
|--------|----------|-------------|
| **Z+M** | Status Check | Shows current model and shortcuts |
| **Z+W** | AI Query | Screenshot → OCR → AI summary (bullet points) |
| **Z+J** | Java Code | Generate Java code from screenshot |
| **Z+P** | Python Code | Generate Python code from screenshot |
| **Z+C** | C++ Code | Generate C++ code from screenshot |
| **Z+E** | Clipboard Viewer | Toggle floating clipboard viewer |
| **Z+B** | Compiler Auto-Type | Auto-type clipboard at 10,000 chars/sec |
| **Z+R** | Reset History | Clear conversation history |
| **Z+T** | Toggle Theme | Switch between dark/light theme |
| **Z+L** | Toggle Model | Switch between OpenRouter and Gemini |
| **Z+1** | Self Destruct | **Permanently delete exe + .env (no recycle bin)** |

---

## 🎯 Key Features Implemented

### 1. Fixed-Size Popup Dialog
- **Size**: 300x80 pixels (fixed)
- **Content**: Shows first 100 characters only
- **Position**: Bottom-right corner
- **Auto-close**: 3 seconds

### 2. AI Summarization (Z+W)
- **Default Prompt**: "Provide a concise summary in bullet points with only the most important information"
- **Applies to**: All Z+W queries (except compiler errors)
- **Code Generation**: No summarization - full code provided (Z+J, Z+P, Z+C)

### 3. Dual Model Support
- **OpenRouter (Owl Alpha)**: Default, fast and reliable
- **Gemini 3.1 Flash Lite**: Cost-efficient, low latency
- **Toggle**: Z+L switches between models
- **Status**: Z+M shows current active model

### 4. Self-Destruct (Z+1)
- **Deletes**: Both `svchost.exe` and `.env` file
- **Method**: Permanent deletion (bypasses recycle bin)
- **Process**: Creates temporary batch file that deletes files after exit
- **Security**: Batch file also deletes itself

### 5. Theme System (Z+T)
- **Dark Theme**: Default (dark gray background, white text)
- **Light Theme**: Light gray background, black text
- **Applies to**: Both popup and clipboard viewer

---

## 📦 Files Required for Deployment

### **ONLY 2 FILES NEEDED:**

1. **`svchost.exe`** (Main executable)
   - Location: `dist-advanced/process-hollow/svchost.exe`
   - Size: ~70-80 MB (self-contained, includes .NET runtime)
   - Platform: Windows x64

2. **`.env`** (Configuration file)
   - Location: `dist-advanced/process-hollow/.env`
   - Size: ~200 bytes
   - Content:
     ```env
     # OpenRouter API Key (for Owl Alpha model)
     OPENROUTER_API_KEY=your_openrouter_api_key_here
     
     # Gemini API Key (for Gemini 3.1 Flash Lite model)
     GEMINI_API_KEY=your_gemini_api_key_here
     ```

---

## 🌍 Cross-Device Compatibility

### ✅ **YES - These 2 files are enough to run on ANY Windows device!**

#### Requirements:
- **Operating System**: Windows 10/11 (x64)
- **No Installation Needed**: Self-contained executable
- **No .NET Runtime Required**: Bundled inside exe
- **No Dependencies**: Everything is included

#### How to Share:
1. Copy `svchost.exe` and `.env` to a folder
2. Zip the folder or share both files
3. Recipient extracts to any folder
4. Double-click `svchost.exe` to run
5. Works immediately - no setup required!

#### What's Included in the EXE:
- ✅ .NET 6.0 Runtime
- ✅ All required libraries (Newtonsoft.Json, DotNetEnv)
- ✅ Windows SDK components
- ✅ OCR engine (Windows.Media.Ocr)
- ✅ All application code

#### What's NOT Included (and why it's fine):
- ❌ API Keys (stored in `.env` - user can change them)
- ❌ Windows OS components (already on target system)

---

## 🚀 Quick Start Guide

### For First-Time Users:

1. **Extract Files**
   ```
   MyFolder/
   ├── svchost.exe
   └── .env
   ```

2. **Run Application**
   - Double-click `svchost.exe`
   - No console window appears (runs silently)

3. **Test Status**
   - Press **Z** then **M** (within 1 second)
   - Should see popup: "Active (OpenRouter)"

4. **Try AI Query**
   - Open any text/image on screen
   - Press **Z** then **W**
   - Wait 2-3 seconds
   - Check clipboard for summarized response

5. **Self-Destruct**
   - Press **Z** then **1**
   - Both exe and .env are permanently deleted
   - No trace left on system

---

## 🔒 Security Features

### Stealth Mode:
- No taskbar icon
- No system tray icon
- Runs as background process
- Window excluded from screen capture
- Process name: `svchost.exe` (mimics Windows service)

### Self-Destruct:
- Permanent file deletion (no recovery)
- Deletes both exe and configuration
- Automatic cleanup of temporary files
- No recycle bin traces

### Privacy:
- Conversation history stored in memory only
- No logs written to disk
- API keys in separate `.env` file (easy to change)
- Z+R clears all conversation history

---

## 📊 Popup Messages Reference

| Action | Popup Message | Duration |
|--------|---------------|----------|
| Z+M | "Active (OpenRouter)" | 3 sec |
| Z+W | First 100 chars of response | 3 sec |
| Z+J | "JAVA code copied" | 3 sec |
| Z+P | "PYTHON code copied" | 3 sec |
| Z+C | "C++ code copied" | 3 sec |
| Z+B | "Auto-typing X chars" | 3 sec |
| Z+R | "History cleared!" | 3 sec |
| Z+T | "Theme: Dark" or "Theme: Light" | 3 sec |
| Z+L | "Model: OpenRouter" or "Model: Gemini 3.1" | 3 sec |
| Z+1 | "Self-destruct..." | 3 sec |
| Error | "Error: [first 80 chars]..." | 3 sec |

---

## 🎓 Advanced Usage

### Compiler Error Fixing:
1. Generate code with Z+J/Z+P/Z+C
2. Paste in IDE and compile
3. If error occurs, screenshot the error
4. Press Z+W
5. AI automatically detects error and provides fix

### Prompt Extraction:
1. Write on screen: `prompt(your question here)`
2. Press Z+W
3. AI extracts text in parentheses as main prompt
4. Uses rest of screen as context

### MCQ Quick Answers:
1. Screenshot MCQ question
2. Press Z+W
3. First 100 chars show in popup (often includes answer)
4. Full response in clipboard

---

## ⚠️ Important Notes

### API Keys:
- Both keys must be valid for full functionality
- OpenRouter key: Required for OpenRouter model
- Gemini key: Required for Gemini model
- If one key is invalid, that model won't work (other model still works)

### Model Selection:
- Starts with OpenRouter by default
- Model selection resets on restart
- Conversation history persists across model switches
- Recommend pressing Z+R after Z+L for clean context

### Self-Destruct:
- **IRREVERSIBLE** - files cannot be recovered
- Use only when you want to completely remove the application
- Deletes both exe and .env in same folder
- Does NOT delete files in other folders

---

## 📝 Troubleshooting

### "Screenshot capture failed"
- Check screen resolution (very high res may fail)
- Try again after a few seconds

### "OCR failed - no text found"
- Ensure text is visible and clear on screen
- Try with higher contrast text
- Check if text is in English (OCR engine is English-only)

### "API Error"
- Check internet connection
- Verify API keys in `.env` file
- Try switching models with Z+L

### Hotkey not working
- Ensure Z is pressed first, then second key within 1 second
- Check if another application is blocking global hotkeys
- Try restarting the application

---

## 🎉 Summary

**YES - Just 2 files (`svchost.exe` + `.env`) are enough to run on any Windows 10/11 x64 device!**

- ✅ No installation required
- ✅ No .NET runtime needed
- ✅ No dependencies to install
- ✅ Works immediately after extraction
- ✅ Fully portable
- ✅ Self-contained executable

**Share these 2 files and it will work on any compatible Windows machine!**
