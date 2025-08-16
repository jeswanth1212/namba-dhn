# 🥷 StealthAssistant

A powerful Windows background application that provides AI-powered assistance through global hotkeys while remaining completely undetected by monitoring software and proctoring applications.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-6.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)

## ✨ Features

### 🎯 **AI-Powered Assistance**
- **Gemini 2.5 Pro Integration**: Latest Google AI model for superior responses
- **Context Maintenance**: Maintains conversation history across interactions
- **Unlimited Responses**: No token limits - get complete, full-length answers
- **Smart Content Detection**: Automatically handles different content types

### ⌨️ **Global Hotkeys**
- **Z+M**: Show application status
- **Z+S**: Normal AI queries with popup responses
- **Z+Q**: Multiple choice questions with cursor movement feedback
- **Z+J**: Java code generation with clean output
- **Z+A**: Smart clipboard text combination

### 🥷 **Stealth Capabilities**
- **Complete Invisibility**: Runs hidden from proctoring applications
- **Process Obfuscation**: Appears as "Windows Service Host"
- **Memory Optimization**: Minimal system footprint
- **Anti-Detection**: Advanced techniques to avoid monitoring software
- **No Taskbar Presence**: Completely background operation

### 📋 **Smart Clipboard Management**
- **Intelligent History**: Tracks clipboard usage from hotkey interactions
- **Auto-Copy Responses**: All AI responses automatically copied to clipboard
- **Text Combination**: Merge multiple clipboard entries with Z+A
- **Java Code Extraction**: Clean, comment-free code generation

### 🎮 **Interactive Feedback**
- **MCQ Cursor Movement**: Visual feedback for multiple choice answers
  - A = Up ⬆️
  - B = Right ➡️  
  - C = Down ⬇️
  - D = Left ⬅️
- **Mouse Notifications**: Subtle flicker for code generation completion
- **Popup Responses**: Bottom-right notifications for queries

## 🚀 Quick Start

### Prerequisites
- Windows 10/11
- Internet connection
- Gemini API key from [Google AI Studio](https://aistudio.google.com/app/apikey)

### Installation

#### Option 1: Download Pre-built Executable (Recommended)
1. **Download from Releases**
   - Visit [GitHub Releases](https://github.com/jeswanth1212/namba-dhn/releases)
   - Download `StealthAssistant.exe` or the complete package
   - No build required - ready to run!

#### Option 2: Build from Source
1. **Clone the Repository**
   ```bash
   git clone https://github.com/jeswanth1212/namba-dhn.git
   cd namba-dhn/StealthAssistant
   ```

2. **Build the Application**
   ```powershell
   # Using PowerShell
   .\build.ps1
   
   # Or using Command Prompt
   build.bat
   ```

3. **Configure API Key**
   
   Create a `.env` file in the `dist` folder:
   ```
   GEMINI_API_KEY=your_gemini_api_key_here
   ```

4. **Run the Application**
   ```powershell
   cd dist
   .\StealthAssistant.exe
   ```

### First Time Setup

1. **Get Your API Key**
   - Visit [Google AI Studio](https://aistudio.google.com/app/apikey)
   - Create a new API key
   - Copy the key

2. **Test the Application**
   - Press **Z+M** to verify it's running
   - You should see a "Running" popup in the bottom-right corner

## 📖 Usage Guide

### Hotkey Commands

| Hotkey | Function | Description |
|--------|----------|-------------|
| **Z+M** | Status Check | Shows "Running" confirmation popup |
| **Z+S** | Normal Query | General AI assistance with popup response |
| **Z+Q** | MCQ Processing | Multiple choice questions with cursor movement |
| **Z+J** | Java Code | Clean Java code generation to clipboard |
| **Z+A** | Text Combination | Combines last 2 clipboard entries |

### Step-by-Step Usage

#### For General Questions (Z+S):
1. Copy your question to clipboard (Ctrl+C)
2. Press **Z** then **S** (within 1 second)
3. AI response appears in popup and copies to clipboard

#### For Multiple Choice Questions (Z+Q):
1. Copy the MCQ with options to clipboard
2. Press **Z** then **Q**
3. No popup appears, but answer copies to clipboard
4. Cursor moves to indicate the correct answer

#### For Java Code (Z+J):
1. Copy your coding request to clipboard
2. Press **Z** then **J**  
3. Clean Java code (no comments) copies to clipboard
4. Mouse flickers to confirm completion

#### For Text Combination (Z+A):
1. Use other hotkeys to build clipboard history
2. Press **Z** then **A**
3. Last 2 clipboard entries combine into one
4. Combined text available in clipboard for manual pasting

### Example Workflows

**Research Helper:**
```
1. Copy: "Explain quantum computing principles"
2. Z+S → Get detailed explanation in popup + clipboard
3. Copy: "What are practical applications?"  
4. Z+S → Get applications info
5. Z+A → Combine both responses for comprehensive notes
```

**Coding Assistant:**
```
1. Copy: "create java method to sort array using quicksort"
2. Z+J → Get clean quicksort implementation
3. Copy: "add error handling to this method"
4. Z+J → Get enhanced version with error handling
```

**MCQ Solver:**
```
1. Copy: "What is 2+2? A) 3 B) 4 C) 5 D) 6"
2. Z+Q → Answer copies to clipboard, cursor moves right (B is correct)
```

## 🛠️ Technical Details

### Architecture
- **Framework**: .NET 6.0 Windows Forms
- **Language**: C# with advanced Windows API integration
- **AI Model**: Google Gemini 2.5 Pro
- **Global Hooks**: Low-level keyboard hook for Z+key sequences
- **Stealth Tech**: Process obfuscation and memory optimization

### Security Features
- **Local Processing**: API key stored locally in .env file
- **No Data Logging**: Conversation history exists only in memory
- **Encrypted Communication**: HTTPS API calls to Google
- **Process Hiding**: Advanced anti-detection techniques

### System Requirements
- **OS**: Windows 10 1809+ or Windows 11
- **Runtime**: Self-contained (no .NET installation required)
- **Memory**: ~50MB typical usage
- **Network**: Internet connection for AI API calls
- **Permissions**: Administrator rights recommended for global hotkeys

## 🔧 Configuration

### Environment Variables (.env file)
```env
# Required: Your Gemini API key
GEMINI_API_KEY=your_actual_api_key_here

# Optional: Application settings (defaults shown)
APP_NAME=Windows Service Host
STEALTH_MODE=true
```

### Customization Options

**Response Length**: Unlimited by default (no token limits)

**Hotkey Timing**: 1-second window for Z+key sequences

**Popup Duration**: 2 seconds for response notifications

**Memory Management**: Automatic optimization every 5 minutes

## 🚫 Troubleshooting

### Common Issues

**Hotkeys Not Working:**
- Ensure application is running (test with Z+M)
- Run as Administrator for global hotkey access
- Check that no other applications are intercepting Z key

**No AI Responses:**
- Verify `.env` file contains correct API key
- Check internet connection
- Confirm API key has remaining quota

**Application Not Starting:**
- Check Windows version compatibility
- Verify all files are in the same directory
- Run as Administrator

**Detection by Monitoring Software:**
- Ensure you're using the latest version
- Run from a non-suspicious directory name
- Consider running during safe periods

### Debug Mode

For development and troubleshooting, compile in Debug mode:
```powershell
dotnet build -c Debug
```

### Logs and Diagnostics

Application uses silent error handling for stealth operation. For debugging:
- Check Windows Event Viewer for application errors
- Monitor Task Manager for process behavior
- Use Process Monitor for file/registry access

## 📁 Project Structure

```
StealthAssistant/
├── 📄 Program.cs              # Application entry point
├── 📄 StealthAssistantCore.cs # Main application logic
├── 📄 ZKeySequenceWindow.cs   # Global hotkey detection
├── 📄 ResponsePopup.cs        # UI notification system
├── 📄 app.manifest           # Windows application manifest
├── 📄 StealthAssistant.csproj # Project configuration
├── 📄 build.ps1              # PowerShell build script
├── 📄 build.bat              # Batch build script
├── 📄 env-template.txt       # Environment template
├── 📄 .gitignore             # Git ignore rules
├── 📄 README.md              # This file
└── 📁 dist/                  # Distribution folder
    ├── 📄 StealthAssistant.exe # Standalone executable
    └── 📄 .env               # API configuration
```

## 🤝 Contributing

We welcome contributions! Here's how you can help:

### Development Setup
1. Clone the repository
2. Open in Visual Studio 2022 or VS Code
3. Ensure .NET 6.0 SDK is installed
4. Build and test your changes

### Pull Request Process
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Guidelines
- Follow C# naming conventions
- Add comments for complex logic
- Test all hotkey combinations
- Ensure stealth functionality remains intact
- Update documentation for new features

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ⚠️ Disclaimer

**Educational Purpose**: This software is created for educational and productivity purposes.

**Responsible Use**: Users are responsible for complying with their institution's policies and local laws.

**No Warranty**: This software is provided "as is" without warranties of any kind.

**Privacy**: No user data is collected or transmitted except for AI API calls to Google.

## 🔗 Links

- **GitHub Repository**: [https://github.com/jeswanth1212/namba-dhn](https://github.com/jeswanth1212/namba-dhn)
- **Google AI Studio**: [https://aistudio.google.com/app/apikey](https://aistudio.google.com/app/apikey)
- **Issues & Support**: [GitHub Issues](https://github.com/jeswanth1212/namba-dhn/issues)

## 🌟 Features Roadmap

- [ ] Plugin system for custom AI models
- [ ] Customizable hotkey combinations  
- [ ] Advanced text processing options
- [ ] Multi-language code generation
- [ ] Encrypted local storage
- [ ] Multiple AI provider support

---

**Made with ❤️ for productivity and learning**

*Remember: Use responsibly and in accordance with your institution's academic integrity policies.*