# Model Toggle Implementation - Z+L

## Overview
Implemented dual AI model support with Z+L hotkey to toggle between OpenRouter and Gemini 3.1 Flash Lite.

## Features Implemented

### 1. Dual Model Support
- **OpenRouter (Owl Alpha)**: Default model, fast and reliable
- **Gemini 3.1 Flash Lite**: Google's most cost-efficient model, optimized for low latency

### 2. Z+L Hotkey
- Press **Z+L** to toggle between models
- Shows popup notification with current model name
- Model selection persists until toggled again

### 3. Model Integration
Both models work with all features:
- **Z+W**: AI Query with conversation history
- **Z+J**: Java code generation
- **Z+P**: Python code generation
- **Z+C**: C++ code generation

## Environment Configuration

### .env File Structure
```env
# OpenRouter API Key (for Owl Alpha model)
OPENROUTER_API_KEY=your_openrouter_api_key_here

# Gemini API Key (for Gemini 3.1 Flash Lite model)
GEMINI_API_KEY=your_gemini_api_key_here
```

## Files Required to Run

To share the application, you need **2 files** in the same directory:

1. **svchost.exe** - Main executable
   - Location: `dist-advanced/process-hollow/svchost.exe`

2. **.env** - Configuration file with API keys
   - Location: `dist-advanced/process-hollow/.env`
   - Contains both OpenRouter and Gemini API keys

## Complete Hotkey List

| Hotkey | Function | Description |
|--------|----------|-------------|
| **Z+M** | Status Check | Shows current model and all shortcuts |
| **Z+W** | AI Query | Screenshot → OCR → AI response with history |
| **Z+J** | Java Code | Generate Java code from screenshot |
| **Z+P** | Python Code | Generate Python code from screenshot |
| **Z+C** | C++ Code | Generate C++ code from screenshot |
| **Z+E** | Clipboard Viewer | Toggle floating clipboard viewer |
| **Z+B** | Compiler Auto-Type | Auto-type clipboard at 10,000 chars/sec |
| **Z+R** | Reset History | Clear conversation history |
| **Z+T** | Toggle Theme | Switch between dark/light theme |
| **Z+L** | Toggle Model | Switch between OpenRouter and Gemini |
| **Z+1** | Self Destruct | Exit application |

## Technical Implementation

### Model Selection Logic
```csharp
private bool _useOpenRouter = true; // Starts with OpenRouter

// Toggle with Z+L
private void HandleToggleModel()
{
    _useOpenRouter = !_useOpenRouter;
    var modelName = _useOpenRouter ? "OpenRouter (Owl Alpha)" : "Gemini 3.1 Flash Lite";
    // Show notification popup
}
```

### API Integration
- **OpenRouter**: Uses `/v1/chat/completions` endpoint with `openrouter/owl-alpha` model
- **Gemini**: Uses `/v1beta/models/gemini-3.1-flash-lite-preview:generateContent` endpoint

### Conversation History
Both models maintain conversation history:
- History persists across queries until Z+R is pressed
- OpenRouter uses `role: "user"/"assistant"` format
- Gemini uses `role: "user"/"model"` format (automatically converted)

## Model Comparison

### OpenRouter (Owl Alpha)
- **Pros**: Fast, reliable, good for general queries
- **Use Case**: General AI assistance, quick responses
- **Cost**: Moderate

### Gemini 3.1 Flash Lite
- **Pros**: Ultra-low latency, cost-efficient, high-volume optimized
- **Use Case**: High-frequency queries, cost-sensitive operations
- **Cost**: Lowest cost option

## Usage Instructions

1. **Start the application**: Run `svchost.exe` (ensure `.env` is in same folder)
2. **Check status**: Press **Z+M** to see current model
3. **Toggle model**: Press **Z+L** to switch between OpenRouter and Gemini
4. **Use AI features**: All shortcuts (Z+W, Z+J, Z+P, Z+C) work with selected model
5. **Reset history**: Press **Z+R** to clear conversation history when switching contexts

## Notes

- Model selection is **runtime-only** (resets to OpenRouter on restart)
- Both API keys must be valid in `.env` file
- Conversation history is **shared** between models (not cleared on toggle)
- If you want to start fresh with new model, press **Z+R** after **Z+L**
