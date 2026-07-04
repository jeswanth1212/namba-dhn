# StealthAssistant Landing Page

Landing page for StealthAssistant - AI-powered assistant with military-grade stealth technology.

## Features

- 🎨 Modern Next.js 15 with App Router
- 🎭 3D Spline hero section with scroll effects
- 🔥 Firebase authentication
- 📱 Responsive design
- ⚡ Optimized for Vercel deployment

## Tech Stack

- **Framework**: Next.js 15.1.6
- **UI**: React 19, Tailwind CSS 4
- **3D**: Spline (@splinetool/react-spline)
- **Animation**: Framer Motion
- **Backend**: Firebase Firestore
- **Font**: Michroma (Google Fonts)

## Getting Started

### Prerequisites

- Node.js 18+ 
- npm or pnpm

### Installation

```bash
# Install dependencies
npm install
# or
pnpm install
```

### Development

```bash
# Run development server
npm run dev
# or
pnpm dev
```

Open [http://localhost:3000](http://localhost:3000) to view the site.

### Build

```bash
# Create production build
npm run build
# or
pnpm build
```

## Firebase Configuration

The app uses Firebase Firestore for authentication. Firebase config is in:
- `lib/firebase.ts` - Firebase initialization
- `lib/firebaseService.ts` - Authentication functions

### Firestore Collections

**users**
```
users/{username}
  - password: string
  - apiKey: string (Gemini API key)
  - createdAt: timestamp
```

**nameSuggestions**
```
nameSuggestions/{id}
  - username: string
  - description: string
  - suggestedNames: array
  - createdAt: timestamp
```

## Deployment

### Vercel (Recommended)

1. Push to GitHub
2. Import project in Vercel
3. Deploy automatically

See [VERCEL_DEPLOYMENT.md](./VERCEL_DEPLOYMENT.md) for detailed steps.

## Project Structure

```
landing-page/
├── app/
│   ├── components/
│   │   ├── AuthDialog.tsx       # Login/Signup dialog
│   │   ├── FeaturesSection.tsx  # Features display
│   │   ├── HeroSection.tsx      # Hero with scroll effects
│   │   └── SplineViewer.tsx     # 3D Spline viewer
│   ├── layout.tsx               # Root layout
│   ├── page.tsx                 # Home page
│   └── globals.css              # Global styles
├── components/ui/
│   └── kinetic-text.tsx         # Animated text component
├── lib/
│   ├── firebase.ts              # Firebase config
│   ├── firebaseService.ts       # Auth functions
│   └── utils.ts                 # Utility functions
├── public/
│   ├── assets/                  # Spline files
│   └── StealthAssistant.exe     # Downloadable exe
└── package.json
```

## Features

### Hero Section
- 3D Spline animation
- Scroll-driven shrinking effect
- White glow border
- Clickable "DOWNLOAD" text

### Features Section
- Stealth features list
- Global hotkeys table
- Name suggestion input
- Firebase integration

### Auth Dialog
- Login/Signup modes
- Firebase authentication
- Error handling
- Download trigger

## License

MIT
