# Contributing to StealthAssistant

Thank you for your interest in contributing to StealthAssistant! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites
- Windows 10/11 development environment
- Visual Studio 2022 or VS Code with C# extension
- .NET 6.0 SDK or later
- Git for version control

### Development Setup
1. **Fork and Clone**
   ```bash
   git clone https://github.com/your-username/namba-dhn.git
   cd namba-dhn
   ```

2. **Open in IDE**
   - Open `StealthAssistant.sln` in Visual Studio 2022
   - Or open the folder in VS Code

3. **Configure Environment**
   - Create a `.env` file in the `dist` folder
   - Add your Gemini API key for testing

4. **Build and Test**
   ```powershell
   .\build.ps1
   ```

## 📝 Code Guidelines

### C# Coding Standards
- Follow Microsoft C# naming conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and under 50 lines when possible
- Use async/await for I/O operations

### Project Structure
- **Program.cs**: Application entry point
- **StealthAssistantCore.cs**: Main application logic
- **ZKeySequenceWindow.cs**: Global hotkey handling
- **ResponsePopup.cs**: UI notifications
- Keep related functionality together

### Stealth Requirements
- Maintain process obfuscation techniques
- Preserve memory optimization features
- Ensure anti-detection capabilities remain intact
- Test with various monitoring software

### API Integration
- Handle all API errors gracefully
- Implement proper timeout mechanisms
- Respect rate limiting
- Maintain conversation context

## 🧪 Testing

### Manual Testing Checklist
- [ ] All hotkeys work correctly (Z+M, Z+S, Z+Q, Z+J, Z+A)
- [ ] Application runs without visible windows
- [ ] API responses are properly formatted
- [ ] Clipboard operations work as expected
- [ ] Mouse notifications function correctly
- [ ] MCQ cursor movement works for all options
- [ ] Memory usage remains stable over time

### Stealth Testing
- [ ] Process appears as "Windows Service Host"
- [ ] No taskbar presence
- [ ] Minimal resource footprint
- [ ] Hidden from Alt+Tab
- [ ] Undetectable by common monitoring tools

### Edge Cases
- [ ] Invalid API keys
- [ ] Network connectivity issues
- [ ] Large response handling
- [ ] Rapid hotkey sequences
- [ ] Empty clipboard scenarios

## 🐛 Bug Reports

When reporting bugs, please include:

### System Information
- Windows version
- .NET runtime version
- Application version
- Hardware specifications

### Bug Details
- Steps to reproduce
- Expected behavior
- Actual behavior
- Error messages (if any)
- Screenshots/videos if applicable

### Example Bug Report
```markdown
**Bug**: Z+J hotkey not working for complex algorithms

**Environment**: 
- Windows 11 Pro 22H2
- StealthAssistant v1.0.0

**Steps to Reproduce**:
1. Copy complex sorting algorithm request
2. Press Z+J
3. No response in clipboard

**Expected**: Clean Java code in clipboard
**Actual**: Nothing happens
**Error**: None visible
```

## ✨ Feature Requests

### Proposal Format
When suggesting new features:

1. **Use Case**: Describe the problem this solves
2. **Proposed Solution**: Detailed implementation approach
3. **Alternatives**: Other solutions considered
4. **Compatibility**: Impact on existing functionality
5. **Priority**: Low/Medium/High with justification

### Example Feature Request
```markdown
**Feature**: Custom hotkey configuration

**Use Case**: Users want to customize Z+key combinations to their preference

**Proposed Solution**: 
- Add configuration file for hotkey mapping
- UI for hotkey selection
- Conflict detection system

**Alternatives**: 
- Command-line configuration
- Registry-based settings

**Compatibility**: Backward compatible with defaults
**Priority**: Medium - Quality of life improvement
```

## 🔧 Development Workflow

### Branch Naming
- `feature/short-description` for new features
- `bugfix/issue-description` for bug fixes
- `docs/update-description` for documentation
- `refactor/component-name` for code refactoring

### Commit Messages
Follow conventional commits format:
```
type(scope): short description

Detailed explanation if needed

Fixes #123
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

### Pull Request Process

1. **Create Branch**
   ```bash
   git checkout -b feature/my-awesome-feature
   ```

2. **Make Changes**
   - Follow coding standards
   - Add tests where applicable
   - Update documentation

3. **Test Thoroughly**
   - Manual testing
   - Stealth functionality verification
   - Performance impact assessment

4. **Submit PR**
   - Clear title and description
   - Reference related issues
   - Include testing evidence
   - Request appropriate reviewers

### Review Criteria
- Code quality and standards compliance
- Functionality correctness
- Stealth features preservation
- Performance impact
- Documentation completeness
- Test coverage

## 🔒 Security Considerations

### Sensitive Information
- Never commit API keys or credentials
- Use environment variables for configuration
- Sanitize user inputs
- Validate all external data

### Stealth Features
- Maintain process obfuscation
- Preserve anti-detection mechanisms
- Minimize system footprint
- Avoid suspicious behaviors

### API Security
- Use HTTPS for all communications
- Implement proper error handling
- Respect rate limits
- Handle authentication securely

## 📚 Resources

### Documentation
- [.NET 6.0 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Windows Forms Guide](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)
- [Gemini API Documentation](https://ai.google.dev/docs)

### Tools
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)
- [Git for Windows](https://git-scm.com/download/win)

### Community
- GitHub Issues for bug reports
- GitHub Discussions for questions
- Pull Requests for contributions

## 🎯 Contribution Areas

We especially welcome contributions in:

### Core Features
- Hotkey system improvements
- AI integration enhancements
- Clipboard management optimization
- Response processing refinements

### Stealth Technology
- Anti-detection improvements
- Memory optimization
- Process obfuscation enhancements
- Performance optimizations

### User Experience
- Error handling improvements
- Configuration options
- Documentation updates
- Accessibility features

### Platform Support
- Windows version compatibility
- Hardware optimization
- Network resilience
- Resource management

## 📄 License

By contributing to StealthAssistant, you agree that your contributions will be licensed under the MIT License.

## 🙏 Recognition

Contributors will be recognized in:
- README.md contributors section
- Release notes for significant contributions
- Special thanks for major features

Thank you for helping make StealthAssistant better! 🚀
