# VITpyq Deployment Summary

## ✅ What's Been Implemented

### 1. **Dual Model Support (OpenRouter + Gemini 3.1 Flash Lite)**
- Z+L hotkey toggles between models
- Both API keys stored in Firebase
- .env file generated with both keys on download

### 2. **Updated Landing Page**
- Signup now asks for both API keys
- Links to generate keys:
  - Gemini: https://aistudio.google.com/app/api-keys
  - OpenRouter: https://openrouter.ai/workspaces/default/keys
- Download fetches from Firebase Storage (not Vercel public folder)

### 3. **Firebase Integration**
- **Firestore**: Stores user credentials + both API keys
- **Storage**: Hosts the 170MB VITpyq.exe file
- Backward compatible with old users (only Gemini key)

### 4. **Optimized Features**
- Fixed-size popup (300x80px)
- AI summarization with bullet points
- Self-destruct permanently deletes exe + .env
- All 11 hotkeys working

---

## 📦 File Size Issue & Solution

### Problem
- **VITpyq.exe**: 170MB (too large for Vercel's 100MB limit)
- Cannot reduce below 170MB without DLL extraction issues

### Solution
**Host exe on Firebase Storage** instead of Vercel public folder

#### Why Firebase Storage?
- ✅ Free tier: 1GB storage, 10GB/month bandwidth
- ✅ Already using Firebase for auth
- ✅ Easy integration
- ✅ Fast CDN delivery
- ✅ ~58 free downloads/month

---

## 🚀 Deployment Steps

### Step 1: Upload VITpyq.exe to Firebase Storage

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Select project: `namba-dhn`
3. Click "Storage" → "Upload file"
4. Upload: `dist-advanced/process-hollow/svchost.exe`
5. Rename to: `VITpyq.exe`
6. Set public read access (see FIREBASE_STORAGE_UPLOAD_INSTRUCTIONS.md)

### Step 2: Deploy Landing Page to Vercel

```bash
cd landing-page
npm install
npm run build
vercel --prod
```

### Step 3: Test the Flow

1. Visit your Vercel URL
2. Click "DOWNLOAD"
3. Sign up with both API keys
4. Download should fetch from Firebase Storage
5. Extract VITpyq.zip
6. Run VITpyq.exe
7. Test Z+L to toggle models

---

## 📊 Current File Structure

```
StealthAssistant/
├── dist-advanced/
│   └── process-hollow/
│       ├── svchost.exe (170MB) ← Upload this to Firebase as VITpyq.exe
│       └── .env (template)
│
├── landing-page/
│   ├── app/
│   │   └── components/
│   │       └── AuthDialog.tsx (updated for dual keys + Firebase Storage)
│   ├── lib/
│   │   ├── firebase.ts (added Storage)
│   │   └── firebaseService.ts (updated for dual keys)
│   └── public/
│       └── VITpyq.exe (DELETE THIS - not used anymore)
│
└── StealthAssistant-ProcessHollow/
    └── SimpleStealthCore.cs (dual model support)
```

---

## 🔑 Environment Variables

### Firebase (already configured)
```
Project ID: namba-dhn
Storage Bucket: namba-dhn.firebasestorage.app
```

### User .env (generated on download)
```env
# OpenRouter API Key (for Owl Alpha model)
OPENROUTER_API_KEY=user_key_here

# Gemini API Key (for Gemini 3.1 Flash Lite model)
GEMINI_API_KEY=user_key_here
```

---

## 🎯 Features Summary

| Feature | Status | Details |
|---------|--------|---------|
| Dual Model Support | ✅ | OpenRouter + Gemini 3.1 Flash Lite |
| Model Toggle (Z+L) | ✅ | Switch between models at runtime |
| Firebase Auth | ✅ | Login/Signup with username + password |
| Dual API Keys | ✅ | Both keys stored in Firestore |
| Firebase Storage | ✅ | Hosts 170MB exe file |
| Download System | ✅ | Fetches from Firebase, bundles with .env |
| Fixed Popup Size | ✅ | 300x80px, shows first 100 chars |
| AI Summarization | ✅ | Bullet points for Z+W queries |
| Self-Destruct | ✅ | Permanently deletes exe + .env |
| All Hotkeys | ✅ | 11 hotkeys working |

---

## 💰 Cost Analysis

### Firebase Free Tier
- **Storage**: 1GB (VITpyq.exe uses 170MB = 17%)
- **Bandwidth**: 10GB/month
  - Each download = 170MB
  - **Free downloads**: ~58/month
  - After that: $0.12/GB

### Vercel Free Tier
- **Bandwidth**: 100GB/month (landing page only, no exe)
- **Builds**: 6,000 minutes/month
- **Deployments**: Unlimited

### Total Cost
- **0-58 downloads/month**: $0 (completely free)
- **100 downloads/month**: ~$7/month (Firebase bandwidth)
- **500 downloads/month**: ~$35/month (Firebase bandwidth)

---

## 🔄 Alternative: GitHub Releases (Unlimited Free)

If you need more than 58 downloads/month:

1. Create GitHub repo
2. Upload exe as release asset
3. Update AuthDialog.tsx to fetch from GitHub
4. **Cost**: $0 forever, unlimited downloads

See FIREBASE_STORAGE_UPLOAD_INSTRUCTIONS.md for details.

---

## ✅ Final Checklist

Before going live:

- [ ] Upload VITpyq.exe to Firebase Storage
- [ ] Set Firebase Storage rules for public read
- [ ] Test download from Firebase Console
- [ ] Delete `landing-page/public/VITpyq.exe` (not needed)
- [ ] Deploy landing page to Vercel
- [ ] Test full signup → download → run flow
- [ ] Test Z+L model toggle
- [ ] Test both API keys work
- [ ] Monitor Firebase Storage usage

---

## 📝 User Instructions

When users download:

1. **Download**: VITpyq.zip (~170MB)
2. **Extract**: Unzip to any folder
3. **Contents**:
   - VITpyq.exe (170MB)
   - .env (with their API keys)
4. **Run**: Double-click VITpyq.exe
5. **Test**: Press Z+M to verify it's running
6. **Toggle Model**: Press Z+L to switch between OpenRouter and Gemini

---

## 🐛 Troubleshooting

### "Download failed"
- Check Firebase Storage rules are set to public read
- Verify VITpyq.exe exists in Firebase Storage
- Check browser console for errors

### "API Error"
- Verify both API keys are valid
- Test keys manually in browser
- Check internet connection

### "Exe won't run"
- Ensure .env is in same folder as VITpyq.exe
- Check Windows Defender didn't block it
- Run as administrator if needed

---

## 🎉 You're Ready to Deploy!

Follow the steps in order:
1. Upload to Firebase Storage
2. Deploy to Vercel
3. Test the flow
4. Share with users!
