# VITpyq - AI-Powered Stealth Assistant

> A powerful Windows desktop assistant with dual AI model support, screenshot OCR, and intelligent hotkeys. Built for productivity, exams, and coding assistance.

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![.NET](https://img.shields.io/badge/.NET-6.0-purple)
![Size](https://img.shields.io/badge/size-170MB-orange)

---

## 🌟 Features

### Dual AI Model Support
- **OpenRouter (Owl Alpha)**: Fast, reliable, general-purpose AI
- **Gemini 3.1 Flash Lite**: Ultra-low latency, cost-efficient Google AI
- **Toggle on-the-fly**: Switch between models with Z+L hotkey

### Core Capabilities
- 📸 **Screenshot + OCR**: Automatically extracts text from screen
- 🤖 **AI Summarization**: Bullet-point summaries for quick understanding
- 💻 **Code Generation**: Java, Python, C++ from screenshots
- 🔄 **Conversation History**: Maintains context across queries
- 🛠️ **Compiler Error Fixing**: Auto-detects errors and provides fixes
- ⚡ **High-Speed Auto-Type**: 10,000 chars/sec to bypass paste detection
- 👁️ **Clipboard Viewer**: Floating window for clipboard management
- 🎨 **Theme Support**: Dark/Light theme toggle
- 💣 **Self-Destruct**: Permanently deletes exe + .env on Z+1

---

## ⌨️ Hotkey Reference

| Hotkey | Function | Description |
|--------|----------|-------------|
| **Z+M** | Status Check | Shows current model and all shortcuts |
| **Z+W** | AI Query | Screenshot → OCR → AI summary (bullet points) |
| **Z+J** | Java Code | Generate Java code from screenshot |
| **Z+P** | Python Code | Generate Python code from screenshot |
| **Z+C** | C++ Code | Generate C++ code from screenshot |
| **Z+E** | Clipboard Viewer | Toggle floating clipboard viewer |
| **Z+B** | Compiler Auto-Type | Auto-type clipboard at 10,000 chars/sec with stripped indentation |
| **Z+R** | Reset History | Clear conversation history |
| **Z+T** | Toggle Theme | Switch between dark/light theme |
| **Z+L** | Toggle Model | Switch between OpenRouter and Gemini |
| **Z+1** | Self-Destruct | **Permanently delete exe + .env (no recovery)** |

> **Note**: Press Z first, then the second key within 1 second.

---

## 🎯 Use Cases

### For Students
- 📝 **MCQ Assistance**: Screenshot question → Get answer in popup
- 📚 **Study Summaries**: Summarize textbook pages in bullet points
- 🧮 **Problem Solving**: Screenshot math/coding problems → Get solutions

### For Developers
- 🐛 **Bug Fixing**: Screenshot compiler errors → Get fixes automatically
- 💡 **Code Generation**: Describe problem in text → Get code in any language
- 📋 **Clipboard Management**: View and manage multiple clipboard entries
- ⚡ **Fast Code Input**: Auto-type code at 10,000 chars/sec to bypass IDE restrictions

### For Professionals
- 📊 **Document Summarization**: Extract key points from reports
- 🔍 **Quick Research**: Get concise answers with references
- 📝 **Content Creation**: Generate text based on context

---

## 🚀 Getting Started

### For End Users

1. **Download VITpyq**
   - Visit the landing page (deployed on Vercel)
   - Sign up with username, password, and both API keys
   - Download VITpyq.zip (~170MB)

2. **Get API Keys**
   - **Gemini**: https://aistudio.google.com/app/api-keys
   - **OpenRouter**: https://openrouter.ai/workspaces/default/keys

3. **Setup**
   - Extract VITpyq.zip to any folder
   - Folder contents:
     - `VITpyq.exe` (170MB)
     - `.env` (your API keys)

4. **Run**
   - Double-click `VITpyq.exe`
   - No console window appears (runs in background)
   - Press **Z+M** to verify it's running

5. **Start Using**
   - Press **Z+W** to query AI about anything on screen
   - Press **Z+L** to switch AI models
   - Press **Z+T** to toggle theme
   - Full response copied to clipboard automatically

---

## 🛠️ For Developers - Build from Source

### Prerequisites

Before you start, install these tools:

1. **Windows 10/11** (64-bit)
2. **.NET 6.0 SDK** - Download from: https://dotnet.microsoft.com/download/dotnet/6.0
3. **Git** - Download from: https://git-scm.com/downloads
4. **PowerShell** (comes with Windows) or **Git Bash**
5. **Text Editor** (VS Code recommended): https://code.visualstudio.com/

### Step-by-Step Installation Guide

#### Step 1: Install .NET 6.0 SDK

1. Visit https://dotnet.microsoft.com/download/dotnet/6.0
2. Download "SDK x64" for Windows
3. Run the installer (e.g., `dotnet-sdk-6.0.xxx-win-x64.exe`)
4. Click through the installation wizard
5. Verify installation:
   ```bash
   dotnet --version
   # Should output: 6.0.xxx
   ```

#### Step 2: Clone the Repository

Open PowerShell or Git Bash and run:

```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/StealthAssistant.git

# Navigate to the project directory
cd StealthAssistant
```

If you don't have Git, you can download the repository as a ZIP file from GitHub and extract it.

#### Step 3: Explore the Project Structure

```bash
# List files
dir  # PowerShell
ls   # Bash

# You should see:
# - StealthAssistant-ProcessHollow/  (main source code)
# - landing-page/                    (web interface)
# - build.ps1                        (build script)
# - README.md                        (this file)
```

### 🏗️ Build the EXE - Three Methods

---

### Method 1: Quick Build (Easiest - Recommended for Beginners)

#### Using PowerShell:

```powershell
# Open PowerShell in the project directory
# Right-click in folder → "Open PowerShell window here"

# Run the build script
.\build.ps1

# Wait 20-30 seconds...
# Done! Your exe is at: dist-advanced/process-hollow/svchost.exe
```

#### Using Command Prompt (CMD):

```bash
# Open CMD in the project directory
# Type "cmd" in the folder address bar

# Run the build script
build.bat

# Wait 20-30 seconds...
# Done! Your exe is at: dist-advanced\process-hollow\svchost.exe
```

**That's it!** Your `svchost.exe` (170MB) is ready.

---

### Method 2: Manual Build with Visual Studio (For VS Users)

1. **Open in Visual Studio**
   - Open Visual Studio 2022
   - Click "Open a project or solution"
   - Navigate to: `StealthAssistant-ProcessHollow/StealthAssistant-ProcessHollow.csproj`
   - Click Open

2. **Configure Build**
   - Click **Build** → **Configuration Manager**
   - Set "Active solution configuration" to **Release**
   - Set "Active solution platform" to **x64**
   - Click Close

3. **Publish as Single File**
   - Right-click the project in Solution Explorer
   - Click **Publish**
   - Choose **Folder** as target
   - Click **Advanced** → **Deployment mode**: Self-contained
   - Check **Produce single file**
   - Set **Target runtime**: win-x64
   - Click **Publish**

4. **Find Your EXE**
   - Go to: `StealthAssistant-ProcessHollow/bin/Release/net6.0-windows10.0.17763.0/win-x64/publish/`
   - Your exe is: `svchost.exe`

---

### Method 3: Command Line Build (Most Control)

#### For PowerShell:

```powershell
# Navigate to project root
cd C:\path\to\StealthAssistant

# Clean previous builds (optional)
Remove-Item -Recurse -Force dist-advanced/process-hollow -ErrorAction SilentlyContinue

# Build the exe
dotnet publish StealthAssistant-ProcessHollow/StealthAssistant-ProcessHollow.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:AssemblyName=svchost `
  -o dist-advanced/process-hollow

# Check the output
Get-Item dist-advanced/process-hollow/svchost.exe | Select-Object Name, Length

# Should show: svchost.exe, ~179,000,000 bytes (170MB)
```

#### For Bash/Git Bash:

```bash
# Navigate to project root
cd /c/path/to/StealthAssistant

# Clean previous builds (optional)
rm -rf dist-advanced/process-hollow

# Build the exe
dotnet publish StealthAssistant-ProcessHollow/StealthAssistant-ProcessHollow.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:AssemblyName=svchost \
  -o dist-advanced/process-hollow

# Check the output
ls -lh dist-advanced/process-hollow/svchost.exe

# Should show: svchost.exe, ~170M
```

---

### 🔧 Build Options Explained

| Option | Value | Purpose |
|--------|-------|---------|
| `-c Release` | Release | Optimized build (smaller, faster) |
| `-r win-x64` | win-x64 | Target Windows 64-bit |
| `--self-contained true` | true | Include .NET runtime (no installation required) |
| `-p:PublishSingleFile=true` | true | Bundle everything into one exe |
| `-p:IncludeNativeLibrariesForSelfExtract=true` | true | Embed Windows SDK DLLs |
| `-p:AssemblyName=svchost` | svchost | Output filename |
| `-o dist-advanced/process-hollow` | folder | Output directory |

---

### 📦 After Building - Setup Your EXE

#### Step 1: Get API Keys

**OpenRouter API Key:**
1. Go to https://openrouter.ai/workspaces/default/keys
2. Sign up or log in
3. Click "Create Key"
4. Copy your key (starts with `sk-or-v1-...`)

**Gemini API Key:**
1. Go to https://aistudio.google.com/app/api-keys
2. Sign in with Google
3. Click "Create API Key"
4. Copy your key (starts with `AIza...`)

#### Step 2: Create .env File

In the same folder as `svchost.exe`, create a file named `.env`:

**Using Notepad:**
1. Open Notepad
2. Copy this template:
   ```env
   # OpenRouter API Key (for Owl Alpha model)
   OPENROUTER_API_KEY=sk-or-v1-paste_your_key_here
   
   # Gemini API Key (for Gemini 3.1 Flash Lite model)
   GEMINI_API_KEY=AIza_paste_your_key_here
   ```
3. Replace `paste_your_key_here` with your actual keys
4. Save as `.env` (with the dot, no .txt extension)
5. Save location: `dist-advanced/process-hollow/.env`

**Using PowerShell:**
```powershell
# Navigate to output directory
cd dist-advanced/process-hollow

# Create .env file
@"
# OpenRouter API Key (for Owl Alpha model)
OPENROUTER_API_KEY=sk-or-v1-your_key_here

# Gemini API Key (for Gemini 3.1 Flash Lite model)
GEMINI_API_KEY=AIza_your_key_here
"@ | Out-File -FilePath .env -Encoding UTF8

# Edit the file to add your actual keys
notepad .env
```

**Using Command Prompt:**
```bash
cd dist-advanced\process-hollow

echo # OpenRouter API Key (for Owl Alpha model) > .env
echo OPENROUTER_API_KEY=sk-or-v1-your_key_here >> .env
echo. >> .env
echo # Gemini API Key (for Gemini 3.1 Flash Lite model) >> .env
echo GEMINI_API_KEY=AIza_your_key_here >> .env

# Edit the file to add your actual keys
notepad .env
```

#### Step 3: Verify Your Setup

Your folder should look like this:

```
dist-advanced/process-hollow/
├── svchost.exe       (170MB)
└── .env              (your API keys)
```

**Check file sizes:**
```powershell
# PowerShell
Get-ChildItem dist-advanced/process-hollow | Select-Object Name, Length

# Should show:
# svchost.exe    ~179,000,000 bytes
# .env           ~200 bytes
```

---

### 🚀 Run Your EXE

#### Method 1: Double-Click
1. Open `dist-advanced/process-hollow/` in File Explorer
2. Double-click `svchost.exe`
3. No window appears (it runs in background)

#### Method 2: Command Line
```powershell
# PowerShell
cd dist-advanced/process-hollow
.\svchost.exe

# Bash
cd dist-advanced/process-hollow
./svchost.exe
```

#### Method 3: With Logging (Debug Mode)
```powershell
# PowerShell - See output messages
cd dist-advanced/process-hollow
Start-Process -FilePath ".\svchost.exe" -Wait -NoNewWindow

# Stop the process
Get-Process | Where-Object { $_.Name -eq 'svchost' -and $_.Path -like '*dist-advanced*' } | Stop-Process
```

---

### ✅ Testing Your Build

#### Test 1: Check if Running
```powershell
# Press Z then M (within 1 second)
# You should see a popup: "Active (OpenRouter)"
```

#### Test 2: AI Query
```powershell
# 1. Open any text document or webpage
# 2. Press Z then W
# 3. Wait 2-3 seconds
# 4. Check clipboard (Ctrl+V) - should have AI response
```

#### Test 3: Model Toggle
```powershell
# Press Z then L
# Popup should say: "Model: Gemini 3.1"
# Press Z then L again
# Popup should say: "Model: OpenRouter"
```

#### Test 4: Theme Toggle
```powershell
# Press Z then T
# Popup should say: "Theme: Light" or "Theme: Dark"
```

#### Test 5: Clipboard Viewer
```powershell
# Copy some text (Ctrl+C)
# Press Z then E
# A window should appear showing your clipboard content
# Press Z then E again to close
```

---

### 🐛 Common Build Errors and Fixes

#### Error: "dotnet: command not found"
**Cause**: .NET SDK not installed or not in PATH

**Fix**:
1. Install .NET 6.0 SDK: https://dotnet.microsoft.com/download/dotnet/6.0
2. Restart your terminal
3. Verify: `dotnet --version`

#### Error: "Could not find project file"
**Cause**: Wrong directory

**Fix**:
```bash
# Make sure you're in the project root
cd C:\path\to\StealthAssistant

# Check if the project file exists
dir StealthAssistant-ProcessHollow\StealthAssistant-ProcessHollow.csproj
```

#### Error: "The process cannot access the file"
**Cause**: The exe is currently running

**Fix**:
```powershell
# Kill the running process
Get-Process | Where-Object { $_.Name -eq 'svchost' -and $_.Path -like '*dist-advanced*' } | Stop-Process -Force

# Wait 2 seconds, then rebuild
```

#### Error: "TargetFramework 'net6.0-windows10.0.17763.0' not found"
**Cause**: Windows SDK not installed

**Fix**:
1. Open Visual Studio Installer
2. Modify your VS installation
3. Under "Individual components", search for "Windows 10 SDK (10.0.17763.0)"
4. Check the box and install

#### Error: Build succeeds but exe won't run
**Cause**: Missing .env file or invalid API keys

**Fix**:
1. Ensure `.env` file exists in same folder as exe
2. Check API keys are valid (no extra spaces, quotes, or newlines)
3. Test API keys manually:
   ```powershell
   # Test Gemini key
   curl "https://generativelanguage.googleapis.com/v1beta/models?key=YOUR_GEMINI_KEY"
   
   # Test OpenRouter key
   curl -H "Authorization: Bearer YOUR_OPENROUTER_KEY" https://openrouter.ai/api/v1/models
   ```

---

### 📤 Distributing Your EXE

#### Option 1: Direct Share (Simple)
1. Copy the entire folder:
   ```
   dist-advanced/process-hollow/
   ├── svchost.exe
   └── .env (with your keys)
   ```
2. Zip it: Right-click → "Send to" → "Compressed (zipped) folder"
3. Share the zip file

**⚠️ Warning**: This includes YOUR API keys. Only share with trusted people.

#### Option 2: Let Users Add Their Own Keys (Recommended)
1. Copy only `svchost.exe`
2. Create a template `.env`:
   ```env
   OPENROUTER_API_KEY=YOUR_KEY_HERE
   GEMINI_API_KEY=YOUR_KEY_HERE
   ```
3. Zip both files
4. Include instructions for users to:
   - Get their own API keys
   - Replace `YOUR_KEY_HERE` with actual keys

#### Option 3: Upload to Firebase (For Landing Page)
```bash
# See FIREBASE_STORAGE_UPLOAD_INSTRUCTIONS.md for full guide

# Quick version:
1. Go to https://console.firebase.google.com
2. Select your project
3. Storage → Upload file
4. Upload svchost.exe as VITpyq.exe
```

---

### 🔄 Rebuilding After Code Changes

If you modify the source code:

```bash
# 1. Save your changes in VS Code or your editor

# 2. Rebuild
dotnet publish StealthAssistant-ProcessHollow/StealthAssistant-ProcessHollow.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:AssemblyName=svchost \
  -o dist-advanced/process-hollow

# 3. Test the new exe
cd dist-advanced/process-hollow
.\svchost.exe
```

**Pro Tip**: Create a batch file for quick rebuilds:

`quick-rebuild.bat`:
```batch
@echo off
echo Stopping existing process...
taskkill /F /IM svchost.exe 2>nul
timeout /t 2 >nul

echo Building...
dotnet publish StealthAssistant-ProcessHollow/StealthAssistant-ProcessHollow.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:AssemblyName=svchost -o dist-advanced/process-hollow

echo Done!
echo Starting exe...
start "" "dist-advanced\process-hollow\svchost.exe"
pause
```

Then just double-click `quick-rebuild.bat` whenever you make changes.

---

## 📁 Project Structure

```
StealthAssistant/
├── StealthAssistant-ProcessHollow/          # Main application source
│   ├── SimpleStealthCore.cs                 # Core logic (AI, OCR, hotkeys)
│   ├── ZKeySequenceWindow.cs                # Keyboard hook implementation
│   ├── ResponsePopup.cs                     # Fixed-size popup notifications
│   ├── StealthClipboardViewer.cs            # Clipboard viewer window
│   └── StealthAssistant-ProcessHollow.csproj
│
├── landing-page/                            # Next.js landing page
│   ├── app/
│   │   └── components/
│   │       ├── AuthDialog.tsx               # Login/Signup with dual API keys
│   │       ├── HeroSection.tsx              # Hero with download button
│   │       └── FeaturesSection.tsx          # Feature showcase
│   ├── lib/
│   │   ├── firebase.ts                      # Firebase config + Storage
│   │   └── firebaseService.ts               # Auth + Firestore operations
│   └── public/                              # Static assets
│
├── dist-advanced/
│   └── process-hollow/
│       ├── svchost.exe                      # Built executable (170MB)
│       └── .env                             # API keys configuration
│
├── build.ps1                                # PowerShell build script
├── build.bat                                # Batch build script
├── README.md                                # This file
├── DEPLOYMENT_SUMMARY.md                    # Deployment guide
└── FIREBASE_STORAGE_UPLOAD_INSTRUCTIONS.md  # Firebase setup
```

---

## 🔧 Configuration

### Environment Variables

The `.env` file must be in the same directory as `VITpyq.exe`:

```env
# Required: OpenRouter API Key
OPENROUTER_API_KEY=sk-or-v1-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

# Required: Gemini API Key  
GEMINI_API_KEY=AIzaSyxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

### Customization

#### Change Hotkeys

Edit `StealthAssistant-ProcessHollow/ZKeySequenceWindow.cs`:

```csharp
private int GetHotkeyId(int vkCode)
{
    return vkCode switch
    {
        VK_M => 1,  // Z+M - Status
        VK_W => 2,  // Z+W - AI Query
        // Add your custom hotkeys here
        _ => 0
    };
}
```

#### Change AI Models

Edit `StealthAssistant-ProcessHollow/SimpleStealthCore.cs`:

```csharp
// OpenRouter model
model = "openrouter/owl-alpha"

// Gemini model
model = "gemini-3.1-flash-lite-preview"
```

#### Change Popup Size

Edit `StealthAssistant-ProcessHollow/ResponsePopup.cs`:

```csharp
int width = 300;  // Current: 300px
int height = 80;  // Current: 80px
```

---

## 🎨 Theme Customization

### Dark Theme (Default)
- Background: `Color.FromArgb(45, 45, 48)`
- Foreground: `Color.White`

### Light Theme
- Background: `Color.FromArgb(240, 240, 240)`
- Foreground: `Color.Black`

Toggle at runtime with **Z+T**

---

## 📊 Technical Details

### Technologies Used

- **Language**: C# (.NET 6.0)
- **UI Framework**: Windows Forms
- **OCR**: Windows.Media.Ocr (Windows SDK)
- **HTTP Client**: HttpClient (.NET)
- **JSON**: Newtonsoft.Json
- **Environment**: DotNetEnv
- **Keyboard Hook**: Win32 API (SetWindowsHookEx)

### Dependencies

```xml
<ItemGroup>
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  <PackageReference Include="DotNetEnv" Version="3.1.1" />
</ItemGroup>
```

### System Requirements

- **OS**: Windows 10 (version 1809) or later
- **Architecture**: x64
- **RAM**: 100MB minimum
- **.NET Runtime**: Bundled (no installation required)
- **Internet**: Required for AI API calls

### File Size Breakdown

- **.NET Runtime**: ~60MB
- **Windows SDK (OCR)**: ~80MB
- **Application Code**: ~5MB
- **Dependencies**: ~25MB
- **Total**: 170MB

---

## 🔒 Security & Privacy

### Data Handling
- ✅ **No telemetry**: Zero data collection
- ✅ **Local processing**: Screenshots processed locally
- ✅ **API only**: Only AI queries sent to cloud
- ✅ **No logging**: No files written to disk
- ✅ **Memory-only history**: Cleared on exit or Z+R

### Stealth Features
- ❌ No taskbar icon
- ❌ No system tray icon
- ❌ No visible window (runs in background)
- ❌ Excluded from screen capture
- ✅ Self-destruct option (Z+1)

### API Security
- API keys stored in `.env` file (local only)
- Keys encrypted in transit (HTTPS)
- No keys stored in cloud (except Firebase for download)

---

## 🐛 Troubleshooting

### Common Issues

#### "Screenshot capture failed"
- **Cause**: High DPI scaling or multi-monitor setup
- **Fix**: Try again, or restart application

#### "OCR failed - no text found"
- **Cause**: Text not clear or non-English
- **Fix**: Ensure text is visible, high contrast, and in English

#### "API Error"
- **Cause**: Invalid API key or no internet
- **Fix**: 
  1. Check `.env` file exists in same folder as exe
  2. Verify API keys are valid
  3. Test internet connection
  4. Try switching models with Z+L

#### "Hotkey not working"
- **Cause**: Another app using the same hotkey
- **Fix**: Close conflicting apps or change hotkey in source code

#### "DLL files appearing"
- **Cause**: You're using the 72MB optimized version
- **Fix**: Use the 170MB version (no DLL extraction)

### Debug Mode

To see console output:

```bash
# Edit StealthAssistant-ProcessHollow.csproj
<OutputType>Exe</OutputType>  <!-- Change from WinExe -->
```

Then rebuild.

---

## 📈 Performance

### Benchmarks

| Operation | Time | Notes |
|-----------|------|-------|
| Screenshot Capture | ~50ms | Full screen, 1920x1080 |
| OCR Processing | ~200ms | English text, medium density |
| AI Query (OpenRouter) | ~1-3s | Depends on prompt length |
| AI Query (Gemini) | ~500ms-2s | Faster for short prompts |
| Auto-Type (10K chars) | ~1s | 10,000 chars/sec |
| Startup Time | ~500ms | Cold start |

### Resource Usage

- **RAM**: 50-100MB (idle)
- **CPU**: <1% (idle), 5-10% (active query)
- **Disk**: 0 writes (memory-only)
- **Network**: Only during AI queries

---

## 🌐 Landing Page Deployment

The landing page is a Next.js app deployed on Vercel.

### Local Development

```bash
cd landing-page
npm install
npm run dev
```

Visit http://localhost:3000

### Deploy to Vercel

```bash
npm run build
vercel --prod
```

### Firebase Setup

1. Upload VITpyq.exe to Firebase Storage
2. Set public read access
3. Update environment variables

See `DEPLOYMENT_SUMMARY.md` for full instructions.

---

## 📝 License

MIT License - see LICENSE file

---

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

---

## 💬 Support

- **Issues**: Open a GitHub issue
- **Discussions**: Use GitHub Discussions
- **Email**: your-email@example.com

---

## 📚 Additional Documentation

- [Deployment Guide](DEPLOYMENT_SUMMARY.md)
- [Firebase Setup](FIREBASE_STORAGE_UPLOAD_INSTRUCTIONS.md)
- [Model Toggle Implementation](MODEL_TOGGLE_IMPLEMENTATION.md)
- [Final Deployment Guide](FINAL_DEPLOYMENT_GUIDE.md)

---

## 🙏 Acknowledgments

- **OpenRouter**: For providing access to multiple AI models
- **Google AI**: For Gemini API and OCR capabilities
- **Microsoft**: For .NET and Windows SDK
- **Firebase**: For authentication and file hosting

---

## ⚠️ Disclaimer

This tool is for educational and productivity purposes. Users are responsible for:
- Obtaining valid API keys
- Complying with API terms of service
- Using the tool ethically and legally
- Not using in environments where such tools are prohibited

---

## 🎯 Roadmap

- [ ] Add more AI models (Claude, GPT-4, etc.)
- [ ] Support for multiple languages in OCR
- [ ] Custom hotkey configuration UI
- [ ] Plugin system for extensions
- [ ] Mobile companion app
- [ ] Voice command support

---

## 📊 Stats

- **Total Lines of Code**: ~3,000
- **Development Time**: ~2 weeks
- **Languages**: C#, TypeScript, Markdown
- **Files**: 50+
- **Dependencies**: 10+ packages

---

Made with ❤️ for productivity enthusiasts and exam warriors.

**Star ⭐ this repo if you find it useful!**
