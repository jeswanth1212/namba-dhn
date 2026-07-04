'use client';

import { useState } from 'react';
import { KineticText } from '@/components/ui/kinetic-text';
import { saveNameSuggestion } from '@/lib/firebaseService';

export default function FeaturesSection() {
  const [showNameInput, setShowNameInput] = useState(false);
  const [nameInput, setNameInput] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async () => {
    if (nameInput.trim()) {
      setLoading(true);

      // Save to Firebase (username can be 'anonymous' or you can track logged-in users)
      const result = await saveNameSuggestion('anonymous', nameInput, []);

      setLoading(false);

      if (result.success) {
        alert(`Name suggestion saved: ${nameInput}`);
        setNameInput('');
        setShowNameInput(false);
      } else {
        alert('Failed to save suggestion. Please try again.');
      }
    }
  };

  return (
    <section className="relative bg-black py-16 px-8">
      <div className="max-w-7xl mx-auto">
        <div className="grid lg:grid-cols-2 gap-6">
          {/* Left - Two boxes stacked */}
          <div className="flex flex-col gap-6 h-full">
            {/* Top - Stealth Features */}
            <div
              className="relative p-8 rounded-[40px] border-2 border-white/80 bg-black flex-1"
              style={{
                boxShadow: '0 0 60px 30px rgba(255, 255, 255, 0.15)',
              }}
            >
              <KineticText
                text="STEALTH FEATURES"
                className="text-2xl md:text-3xl font-bold text-white mb-6 tracking-wider"
                style={{ fontFamily: 'var(--font-michroma)' }}
              />

              <ul className="space-y-3">
                <li className="flex items-start">
                  <span className="text-white mr-3 text-sm">•</span>
                  <div>
                    <h3
                      className="text-sm md:text-base font-bold text-white mb-1 tracking-wide"
                      style={{ fontFamily: 'var(--font-michroma)' }}
                    >
                      PROCESS OBFUSCATION
                    </h3>
                    <p className="text-gray-400 text-xs">
                      Disguised as Windows Service Host
                    </p>
                  </div>
                </li>

                <li className="flex items-start">
                  <span className="text-white mr-3 text-sm">•</span>
                  <div>
                    <h3
                      className="text-sm md:text-base font-bold text-white mb-1 tracking-wide"
                      style={{ fontFamily: 'var(--font-michroma)' }}
                    >
                      SCREEN CAPTURE PROTECTION
                    </h3>
                    <p className="text-gray-400 text-xs">
                      Hidden from screen sharing
                    </p>
                  </div>
                </li>

                <li className="flex items-start">
                  <span className="text-white mr-3 text-sm">•</span>
                  <div>
                    <h3
                      className="text-sm md:text-base font-bold text-white mb-1 tracking-wide"
                      style={{ fontFamily: 'var(--font-michroma)' }}
                    >
                      MINIMAL FOOTPRINT
                    </h3>
                    <p className="text-gray-400 text-xs">
                      Lightweight and efficient
                    </p>
                  </div>
                </li>

                <li className="flex items-start">
                  <span className="text-white mr-3 text-sm">•</span>
                  <div>
                    <h3
                      className="text-sm md:text-base font-bold text-white mb-1 tracking-wide"
                      style={{ fontFamily: 'var(--font-michroma)' }}
                    >
                      ANTI-DETECTION
                    </h3>
                    <p className="text-gray-400 text-xs">
                      Advanced evasion techniques
                    </p>
                  </div>
                </li>

                <li className="flex items-start">
                  <span className="text-white mr-3 text-sm">•</span>
                  <div>
                    <h3
                      className="text-sm md:text-base font-bold text-white mb-1 tracking-wide"
                      style={{ fontFamily: 'var(--font-michroma)' }}
                    >
                      HARDWARE-LEVEL SIMULATION
                    </h3>
                    <p className="text-gray-400 text-xs">
                      Native keyboard simulation
                    </p>
                  </div>
                </li>
              </ul>
            </div>

            {/* Bottom - Name Suggestion */}
            <div
              className="relative p-8 rounded-[40px] border-2 border-white/80 bg-black flex items-center justify-center flex-1"
              style={{
                boxShadow: '0 0 60px 30px rgba(255, 255, 255, 0.15)',
              }}
            >
              {!showNameInput ? (
                <button
                  onClick={() => setShowNameInput(true)}
                  className="w-full"
                >
                  <KineticText
                    text="SUGGEST NAMES FOR SOFTWARE"
                    className="text-base md:text-lg font-bold text-white tracking-wider hover:text-gray-300 transition-colors"
                    style={{ fontFamily: 'var(--font-michroma)' }}
                  />
                </button>
              ) : (
                <div className="w-full flex items-center gap-4">
                  <input
                    type="text"
                    value={nameInput}
                    onChange={(e) => setNameInput(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && !loading && handleSubmit()}
                    placeholder="Describe your software..."
                    className="flex-1 bg-transparent border-b-2 border-white/50 text-white text-sm md:text-base px-4 py-2 focus:outline-none focus:border-white transition-colors disabled:opacity-50"
                    style={{ fontFamily: 'var(--font-michroma)' }}
                    autoFocus
                    disabled={loading}
                  />
                  <button
                    onClick={handleSubmit}
                    disabled={loading}
                    className="w-10 h-10 flex items-center justify-center rounded-full border-2 border-white/80 hover:bg-white hover:text-black transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                    style={{
                      boxShadow: '0 0 20px 10px rgba(255, 255, 255, 0.1)',
                    }}
                  >
                    {loading ? (
                      <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                    ) : (
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                        strokeWidth={2.5}
                        stroke="currentColor"
                        className="w-5 h-5"
                      >
                        <path strokeLinecap="round" strokeLinejoin="round" d="M13.5 4.5 21 12m0 0-7.5 7.5M21 12H3" />
                      </svg>
                    )}
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* Right - Global Hotkeys */}
          <div
            className="relative p-8 rounded-[40px] border-2 border-white/80 bg-black"
            style={{
              boxShadow: '0 0 60px 30px rgba(255, 255, 255, 0.15)',
            }}
          >
            <KineticText
              text="GLOBAL HOTKEYS"
              className="text-2xl md:text-3xl font-bold text-white mb-6 tracking-wider"
              style={{ fontFamily: 'var(--font-michroma)' }}
            />

            <div className="space-y-2">
              {[
                { key: 'Z+M', desc: 'Status Check' },
                { key: 'Z+W', desc: 'Extract & Query' },
                { key: 'Z+J', desc: 'Java Code' },
                { key: 'Z+P', desc: 'Python Code' },
                { key: 'Z+C', desc: 'C++ Code' },
                { key: 'Z+E', desc: 'Clipboard Viewer' },
                { key: 'Z+T', desc: 'Toggle Themes' },
                { key: 'Z+L', desc: 'Toggle AI Models' },
                { key: 'Z+R', desc: 'Reset History' },
                { key: 'Z+1', desc: 'Self Destruct' },
              ].map((hotkey, index) => (
                <div
                  key={index}
                  className="flex items-center justify-between py-2 border-b border-white/10"
                >
                  <span
                    className="text-sm md:text-base font-bold text-white tracking-wider"
                    style={{ fontFamily: 'var(--font-michroma)' }}
                  >
                    {hotkey.key}
                  </span>
                  <span className="text-gray-400 text-xs text-right">
                    {hotkey.desc}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
