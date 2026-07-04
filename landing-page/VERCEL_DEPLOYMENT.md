# Vercel Deployment Guide

Complete step-by-step guide to deploy StealthAssistant landing page on Vercel.

---

## Prerequisites

✅ GitHub account  
✅ Vercel account (sign up at [vercel.com](https://vercel.com))  
✅ Landing page code pushed to GitHub

---

## Step 1: Push to GitHub

### 1.1 Initialize Git (if not already done)

```bash
cd landing-page
git init
```

### 1.2 Add Remote Repository

```bash
git remote add origin https://github.com/jeswanth1212/namba-dhn-landing-page.git
```

### 1.3 Stage All Files

```bash
git add .
```

### 1.4 Commit

```bash
git commit -m "Initial commit: StealthAssistant landing page"
```

### 1.5 Push to GitHub

```bash
git branch -M main
git push -u origin main
```

---

## Step 2: Deploy to Vercel

### 2.1 Go to Vercel Dashboard

1. Open [vercel.com](https://vercel.com)
2. Click **"Sign Up"** or **"Log In"**
3. Sign in with GitHub

### 2.2 Import Project

1. Click **"Add New..."** button (top right)
2. Select **"Project"**
3. Click **"Import Git Repository"**

### 2.3 Select Repository

1. Find **"namba-dhn-landing-page"** in the list
2. Click **"Import"**

### 2.4 Configure Project

**Framework Preset**: Next.js (auto-detected)

**Root Directory**: `./` (leave as default)

**Build Command**: 
```bash
npm run build
```

**Output Directory**: 
```
.next
```

**Install Command**:
```bash
npm install
```

### 2.5 Environment Variables (Optional)

If you need any environment variables, add them here. For this project, Firebase config is already in the code, so **skip this step**.

### 2.6 Deploy

1. Click **"Deploy"**
2. Wait for build to complete (2-3 minutes)
3. Once done, you'll see: **"Congratulations! Your project has been deployed."**

---

## Step 3: Access Your Site

### Your URLs

**Production URL**: `https://namba-dhn-landing-page.vercel.app`  
**Custom Domain** (optional): You can add your own domain later

### Test the Site

1. Click the production URL
2. Test features:
   - ✅ Hero section loads with Spline 3D
   - ✅ Scroll effects work
   - ✅ Click "DOWNLOAD" → Auth dialog opens
   - ✅ Login/Signup works
   - ✅ Download button downloads exe

---

## Step 4: Automatic Deployments

Vercel automatically deploys when you push to GitHub:

1. Make changes to your code
2. Commit and push:
   ```bash
   git add .
   git commit -m "Update: description of changes"
   git push
   ```
3. Vercel automatically builds and deploys
4. Check deployment status in Vercel dashboard

---

## Step 5: Custom Domain (Optional)

### Add Your Own Domain

1. Go to Vercel Dashboard
2. Select your project
3. Click **"Settings"** → **"Domains"**
4. Click **"Add"**
5. Enter your domain (e.g., `stealthassistant.com`)
6. Follow DNS configuration instructions
7. Wait for DNS propagation (5-60 minutes)

---

## Troubleshooting

### Build Fails

**Error**: `Module not found`
- **Fix**: Make sure all dependencies are in `package.json`
- Run `npm install` locally to verify

**Error**: `Firebase not configured`
- **Fix**: Check `lib/firebase.ts` has correct config

### Exe Download Not Working

**Error**: 404 on `/StealthAssistant.exe`
- **Fix**: Ensure `public/StealthAssistant.exe` exists
- Check file size (should be ~70-80MB)
- Vercel has 100MB file limit (exe is within limit)

### Spline Not Loading

**Error**: Spline scene doesn't appear
- **Fix**: Check `public/assets/rotating_interactive_hero_section.spline` exists
- Verify Spline URL in `SplineViewer.tsx`

### Firebase Errors

**Error**: `Permission denied` on Firestore
- **Fix**: Update Firestore security rules in Firebase Console:
  ```javascript
  rules_version = '2';
  service cloud.firestore {
    match /databases/{database}/documents {
      match /users/{userId} {
        allow read, write: if true;
      }
      match /nameSuggestions/{suggestionId} {
        allow read, write: if true;
      }
    }
  }
  ```

---

## Performance Optimization

### Vercel Automatically Handles:
- ✅ CDN distribution (global edge network)
- ✅ Image optimization
- ✅ Code splitting
- ✅ Compression (gzip/brotli)
- ✅ Caching

### Manual Optimizations:
1. **Compress Spline file** (if too large)
2. **Optimize images** in `public/` folder
3. **Enable ISR** (Incremental Static Regeneration) if needed

---

## Monitoring

### View Analytics

1. Go to Vercel Dashboard
2. Select your project
3. Click **"Analytics"** tab
4. View:
   - Page views
   - Unique visitors
   - Performance metrics
   - Error rates

### View Logs

1. Go to Vercel Dashboard
2. Select your project
3. Click **"Deployments"**
4. Click on a deployment
5. View build logs and runtime logs

---

## Updating the Exe

When you update `StealthAssistant.exe`:

1. Copy new exe to `landing-page/public/`:
   ```bash
   cp dist/StealthAssistant.exe landing-page/public/
   ```

2. Commit and push:
   ```bash
   cd landing-page
   git add public/StealthAssistant.exe
   git commit -m "Update: StealthAssistant.exe v2.0"
   git push
   ```

3. Vercel automatically redeploys with new exe

---

## Security Notes

### Firebase Security
- ✅ API keys are stored in Firebase (not in code)
- ✅ Firestore rules control access
- ✅ HTTPS enforced by Vercel

### Exe Security
- ✅ Exe served over HTTPS
- ✅ Users download directly from Vercel CDN
- ✅ No third-party hosting

---

## Cost

### Vercel Pricing
- **Hobby Plan**: FREE
  - Unlimited deployments
  - 100GB bandwidth/month
  - Automatic HTTPS
  - Global CDN

- **Pro Plan**: $20/month (if you need more)
  - 1TB bandwidth/month
  - Advanced analytics
  - Team collaboration

**For this project**: Hobby plan is sufficient

---

## Support

### Vercel Support
- Documentation: [vercel.com/docs](https://vercel.com/docs)
- Community: [github.com/vercel/vercel/discussions](https://github.com/vercel/vercel/discussions)

### Next.js Support
- Documentation: [nextjs.org/docs](https://nextjs.org/docs)
- Community: [github.com/vercel/next.js/discussions](https://github.com/vercel/next.js/discussions)

---

## Quick Reference

### Useful Commands

```bash
# Local development
npm run dev

# Build locally
npm run build

# Start production server locally
npm start

# Push to GitHub (triggers Vercel deploy)
git push

# View Vercel logs
vercel logs

# Pull environment variables from Vercel
vercel env pull
```

### Useful Links

- **Vercel Dashboard**: [vercel.com/dashboard](https://vercel.com/dashboard)
- **GitHub Repo**: [github.com/jeswanth1212/namba-dhn-landing-page](https://github.com/jeswanth1212/namba-dhn-landing-page)
- **Firebase Console**: [console.firebase.google.com](https://console.firebase.google.com)

---

## Checklist

Before deploying, ensure:

- [ ] All code committed to GitHub
- [ ] `public/StealthAssistant.exe` exists
- [ ] Firebase config is correct
- [ ] Spline file is in `public/assets/`
- [ ] `package.json` has all dependencies
- [ ] `.gitignore` excludes `node_modules/` and `.next/`
- [ ] README.md is complete

After deploying, verify:

- [ ] Site loads at Vercel URL
- [ ] Hero section displays correctly
- [ ] Scroll effects work
- [ ] Auth dialog opens
- [ ] Login/Signup works
- [ ] Download button downloads exe
- [ ] Name suggestion saves to Firebase

---

## Done! 🎉

Your StealthAssistant landing page is now live on Vercel!

**Production URL**: `https://namba-dhn-landing-page.vercel.app`

Share this URL with users to download StealthAssistant!
