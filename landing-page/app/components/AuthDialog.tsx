'use client';

import { useState } from 'react';
import { KineticText } from '@/components/ui/kinetic-text';
import { loginUser, signupUser } from '@/lib/firebaseService';

interface AuthDialogProps {
  isOpen: boolean;
  onClose: () => void;
}

export default function AuthDialog({ isOpen, onClose }: AuthDialogProps) {
  const [mode, setMode] = useState<'initial' | 'login' | 'signup' | 'download'>('initial');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [geminiApiKey, setGeminiApiKey] = useState('');
  const [openRouterApiKey, setOpenRouterApiKey] = useState('');
  const [userGeminiApiKey, setUserGeminiApiKey] = useState(''); // Gemini API key fetched from Firebase
  const [userOpenRouterApiKey, setUserOpenRouterApiKey] = useState(''); // OpenRouter API key fetched from Firebase
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [downloadLoading, setDownloadLoading] = useState(false);
  const [downloadProgress, setDownloadProgress] = useState('');

  const handleLogin = async () => {
    if (!username || !password) {
      setError('Please fill in all fields');
      return;
    }

    setLoading(true);
    setError('');
    
    const result = await loginUser(username, password);
    
    setLoading(false);
    
    if (result.success) {
      setUserGeminiApiKey(result.geminiApiKey ?? '');
      setUserOpenRouterApiKey(result.openRouterApiKey ?? '');
      setMode('download');
    } else {
      setError(result.message);
    }
  };

  const handleSignup = async () => {
    if (!username || !password || !geminiApiKey || !openRouterApiKey) {
      setError('Please fill in all fields');
      return;
    }

    setLoading(true);
    setError('');
    
    const result = await signupUser(username, password, geminiApiKey, openRouterApiKey);
    
    setLoading(false);
    
    if (result.success) {
      setUserGeminiApiKey(geminiApiKey); // Use the keys they just registered with
      setUserOpenRouterApiKey(openRouterApiKey);
      setMode('download');
    } else {
      setError(result.message);
    }
  };

  const handleDownload = async () => {
    if (!userGeminiApiKey || !userOpenRouterApiKey) {
      setError('No API keys found. Please log in again.');
      return;
    }

    setDownloadLoading(true);
    setDownloadProgress('Fetching application from Firebase...');
    setError('');

    try {
      // 1. Fetch the exe from Firebase Storage
      const { storage } = await import('@/lib/firebase');
      const { ref, getDownloadURL, getBlob } = await import('firebase/storage');
      
      const exeRef = ref(storage, 'VITpyq.exe');
      
      setDownloadProgress('Downloading application...');
      
      // Get the blob directly from Firebase Storage
      const exeBlob = await getBlob(exeRef);

      setDownloadProgress('Building your package...');

      // 2. Create the .env file content with both API keys
      const envContent = `# OpenRouter API Key (for Owl Alpha model)
OPENROUTER_API_KEY=${userOpenRouterApiKey}

# Gemini API Key (for Gemini 3.1 Flash Lite model)
GEMINI_API_KEY=${userGeminiApiKey}`;

      // 3. Use JSZip to bundle both into VITpyq.zip
      const JSZip = (await import('jszip')).default;
      const zip = new JSZip();

      // Put both files directly in the zip root (user unzips to get a folder)
      const folder = zip.folder('VITpyq');
      if (!folder) throw new Error('Failed to create zip folder');
      folder.file('VITpyq.exe', exeBlob);
      folder.file('.env', envContent);

      setDownloadProgress('Compressing...');

      const zipBlob = await zip.generateAsync(
        { type: 'blob', compression: 'DEFLATE', compressionOptions: { level: 1 } },
        (meta) => {
          setDownloadProgress(`Compressing... ${Math.round(meta.percent)}%`);
        }
      );

      setDownloadProgress('Starting download...');

      // 4. Trigger the download
      const url = URL.createObjectURL(zipBlob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'VITpyq.zip';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);

      onClose();
    } catch (err) {
      console.error('Download error:', err);
      setError('Download failed. Please try again or contact support.');
    } finally {
      setDownloadLoading(false);
      setDownloadProgress('');
    }
  };

  const resetDialog = () => {
    setMode('initial');
    setUsername('');
    setPassword('');
    setGeminiApiKey('');
    setOpenRouterApiKey('');
    setUserGeminiApiKey('');
    setUserOpenRouterApiKey('');
    setError('');
    setDownloadProgress('');
  };

  const handleClose = () => {
    resetDialog();
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div 
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      onClick={handleClose}
    >
      {/* Backdrop blur */}
      <div className="absolute inset-0 bg-black/70 backdrop-blur-md" />
      
      {/* Dialog */}
      <div 
        className="relative w-full max-w-md p-8 rounded-[40px] border-2 border-white/80 bg-black"
        style={{
          boxShadow: '0 0 60px 30px rgba(255, 255, 255, 0.2)',
        }}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Close button */}
        <button
          onClick={handleClose}
          className="absolute top-4 right-4 text-white hover:text-gray-400 transition-colors"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-6 h-6">
            <path strokeLinecap="round" strokeLinejoin="round" d="M6 18 18 6M6 6l12 12" />
          </svg>
        </button>

        {/* Initial Mode - Login or Signup */}
        {mode === 'initial' && (
          <div className="space-y-6">
            <KineticText
              text="GET STARTED"
              className="text-2xl md:text-3xl font-bold text-white text-center tracking-wider mb-8"
              style={{ fontFamily: 'var(--font-michroma)' }}
            />
            
            <button
              onClick={() => setMode('login')}
              className="w-full py-4 rounded-full border-2 border-white/80 text-white font-bold tracking-wider hover:bg-white hover:text-black transition-all"
              style={{ 
                fontFamily: 'var(--font-michroma)',
                boxShadow: '0 0 20px 10px rgba(255, 255, 255, 0.1)',
              }}
            >
              LOG IN
            </button>
            
            <button
              onClick={() => setMode('signup')}
              className="w-full py-4 rounded-full border-2 border-white/80 text-white font-bold tracking-wider hover:bg-white hover:text-black transition-all"
              style={{ 
                fontFamily: 'var(--font-michroma)',
                boxShadow: '0 0 20px 10px rgba(255, 255, 255, 0.1)',
              }}
            >
              SIGN UP
            </button>
          </div>
        )}

        {/* Login Mode */}
        {mode === 'login' && (
          <div className="space-y-6">
            <KineticText
              text="LOG IN"
              className="text-2xl md:text-3xl font-bold text-white text-center tracking-wider mb-8"
              style={{ fontFamily: 'var(--font-michroma)' }}
            />
            
            {error && (
              <div className="p-3 rounded-lg bg-red-500/20 border border-red-500/50">
                <p className="text-red-400 text-sm text-center" style={{ fontFamily: 'var(--font-michroma)' }}>
                  {error}
                </p>
              </div>
            )}
            
            <div>
              <label className="block text-white text-sm mb-2" style={{ fontFamily: 'var(--font-michroma)' }}>
                USERNAME
              </label>
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleLogin()}
                className="w-full bg-transparent border-b-2 border-white/50 text-white px-4 py-3 focus:outline-none focus:border-white transition-colors"
                style={{ fontFamily: 'var(--font-michroma)' }}
                disabled={loading}
              />
            </div>
            
            <div>
              <label className="block text-white text-sm mb-2" style={{ fontFamily: 'var(--font-michroma)' }}>
                PASSWORD
              </label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleLogin()}
                className="w-full bg-transparent border-b-2 border-white/50 text-white px-4 py-3 focus:outline-none focus:border-white transition-colors"
                style={{ fontFamily: 'var(--font-michroma)' }}
                disabled={loading}
              />
            </div>
            
            <button
              onClick={handleLogin}
              disabled={loading}
              className="w-full py-4 rounded-full border-2 border-white/80 text-white font-bold tracking-wider hover:bg-white hover:text-black transition-all disabled:opacity-50 disabled:cursor-not-allowed"
              style={{ 
                fontFamily: 'var(--font-michroma)',
                boxShadow: '0 0 20px 10px rgba(255, 255, 255, 0.1)',
              }}
            >
              {loading ? 'LOGGING IN...' : 'SUBMIT'}
            </button>
            
            <button
              onClick={() => { setMode('initial'); setError(''); }}
              disabled={loading}
              className="w-full text-gray-400 hover:text-white transition-colors text-sm disabled:opacity-50"
              style={{ fontFamily: 'var(--font-michroma)' }}
            >
              BACK
            </button>
          </div>
        )}

        {/* Signup Mode */}
        {mode === 'signup' && (
          <div className="space-y-6">
            <KineticText
              text="SIGN UP"
              className="text-2xl md:text-3xl font-bold text-white text-center tracking-wider mb-8"
              style={{ fontFamily: 'var(--font-michroma)' }}
            />
            
            {error && (
              <div className="p-3 rounded-lg bg-red-500/20 border border-red-500/50">
                <p className="text-red-400 text-sm text-center" style={{ fontFamily: 'var(--font-michroma)' }}>
                  {error}
                </p>
              </div>
            )}
            
            <div>
              <label className="block text-white text-sm mb-2" style={{ fontFamily: 'var(--font-michroma)' }}>
                USERNAME
              </label>
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className="w-full bg-transparent border-b-2 border-white/50 text-white px-4 py-3 focus:outline-none focus:border-white transition-colors"
                style={{ fontFamily: 'var(--font-michroma)' }}
                disabled={loading}
              />
            </div>
            
            <div>
              <label className="block text-white text-sm mb-2" style={{ fontFamily: 'var(--font-michroma)' }}>
                PASSWORD
              </label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full bg-transparent border-b-2 border-white/50 text-white px-4 py-3 focus:outline-none focus:border-white transition-colors"
                style={{ fontFamily: 'var(--font-michroma)' }}
                disabled={loading}
              />
            </div>
            
            <div>
              <label className="block text-white text-sm mb-2" style={{ fontFamily: 'var(--font-michroma)' }}>
                GEMINI API KEY
              </label>
              <input
                type="text"
                value={geminiApiKey}
                onChange={(e) => setGeminiApiKey(e.target.value)}
                className="w-full bg-transparent border-b-2 border-white/50 text-white px-4 py-3 focus:outline-none focus:border-white transition-colors text-sm"
                style={{ fontFamily: 'var(--font-michroma)' }}
                disabled={loading}
              />
              <a 
                href="https://aistudio.google.com/app/api-keys" 
                target="_blank" 
                rel="noopener noreferrer"
                className="text-xs text-gray-400 hover:text-white transition-colors mt-2 inline-block"
              >
                Generate Gemini key from Google AI Studio →
              </a>
            </div>
            
            <div>
              <label className="block text-white text-sm mb-2" style={{ fontFamily: 'var(--font-michroma)' }}>
                OPENROUTER API KEY
              </label>
              <input
                type="text"
                value={openRouterApiKey}
                onChange={(e) => setOpenRouterApiKey(e.target.value)}
                className="w-full bg-transparent border-b-2 border-white/50 text-white px-4 py-3 focus:outline-none focus:border-white transition-colors text-sm"
                style={{ fontFamily: 'var(--font-michroma)' }}
                disabled={loading}
              />
              <a 
                href="https://openrouter.ai/workspaces/default/keys" 
                target="_blank" 
                rel="noopener noreferrer"
                className="text-xs text-gray-400 hover:text-white transition-colors mt-2 inline-block"
              >
                Generate OpenRouter key →
              </a>
            </div>
            
            <button
              onClick={handleSignup}
              disabled={loading}
              className="w-full py-4 rounded-full border-2 border-white/80 text-white font-bold tracking-wider hover:bg-white hover:text-black transition-all disabled:opacity-50 disabled:cursor-not-allowed"
              style={{ 
                fontFamily: 'var(--font-michroma)',
                boxShadow: '0 0 20px 10px rgba(255, 255, 255, 0.1)',
              }}
            >
              {loading ? 'CREATING ACCOUNT...' : 'CREATE ACCOUNT'}
            </button>
            
            <button
              onClick={() => { setMode('initial'); setError(''); }}
              disabled={loading}
              className="w-full text-gray-400 hover:text-white transition-colors text-sm disabled:opacity-50"
              style={{ fontFamily: 'var(--font-michroma)' }}
            >
              BACK
            </button>
          </div>
        )}

        {/* Download Mode */}
        {mode === 'download' && (
          <div className="space-y-6">
            <KineticText
              text="READY TO DOWNLOAD"
              className="text-2xl md:text-3xl font-bold text-white text-center tracking-wider mb-8"
              style={{ fontFamily: 'var(--font-michroma)' }}
            />

            {error && (
              <div className="p-3 rounded-lg bg-red-500/20 border border-red-500/50">
                <p className="text-red-400 text-sm text-center" style={{ fontFamily: 'var(--font-michroma)' }}>
                  {error}
                </p>
              </div>
            )}
            
            <p className="text-gray-400 text-center text-sm" style={{ fontFamily: 'var(--font-michroma)' }}>
              Downloads <span className="text-white font-bold">VITpyq.zip</span> — unzip it anywhere, then run <span className="text-white font-bold">VITpyq.exe</span>.
            </p>

            {/* Progress indicator */}
            {downloadLoading && downloadProgress && (
              <div className="space-y-2">
                <p className="text-white/60 text-xs text-center" style={{ fontFamily: 'var(--font-michroma)' }}>
                  {downloadProgress}
                </p>
                <div className="w-full h-0.5 bg-white/10 rounded-full overflow-hidden">
                  <div className="h-full bg-white/60 rounded-full animate-pulse w-3/4" />
                </div>
              </div>
            )}
            
            <button
              onClick={handleDownload}
              disabled={downloadLoading}
              className="w-full py-4 rounded-full border-2 border-white/80 text-white font-bold tracking-wider hover:bg-white hover:text-black transition-all disabled:opacity-50 disabled:cursor-not-allowed"
              style={{ 
                fontFamily: 'var(--font-michroma)',
                boxShadow: '0 0 20px 10px rgba(255, 255, 255, 0.1)',
              }}
            >
              {downloadLoading ? downloadProgress || 'PREPARING...' : 'DOWNLOAD NOW'}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
