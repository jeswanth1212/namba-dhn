'use client';

import { useRef, useState } from 'react';
import { motion, useScroll, useTransform } from 'framer-motion';
import SplineViewer from './SplineViewer';
import { KineticText } from '@/components/ui/kinetic-text';
import AuthDialog from './AuthDialog';

export default function HeroSection() {
  const sectionRef = useRef<HTMLDivElement>(null);
  const [isAuthDialogOpen, setIsAuthDialogOpen] = useState(false);

  // Scroll animation
  const { scrollYProgress } = useScroll({
    target: sectionRef,
    offset: ['start start', 'end start'],
  });

  // Scale effect
  const scale = useTransform(scrollYProgress, [0, 0.5], [1, 0.85]);

  // Rounded corners effect
  const radius = useTransform(scrollYProgress, [0, 0.5], ['0px', '40px']);

  // Glow values
  const glowOpacity = useTransform(scrollYProgress, [0, 0.5], [0, 0.8]);
  const glowSize = useTransform(scrollYProgress, [0, 0.5], [0, 60]);

  // Box shadow transform
  const boxShadow = useTransform(
    [glowOpacity, glowSize],
    (values) => {
      const opacity = values[0] as number;
      const size = values[1] as number;

      return `0 0 ${size}px ${size / 2}px rgba(255, 255, 255, ${opacity})`;
    }
  );

  // Border transform
  const border = useTransform(
    glowOpacity,
    (opacity) => `2px solid rgba(255, 255, 255, ${opacity})`
  );

  return (
    <>
      <section ref={sectionRef} className="relative h-[200vh]">
        <motion.div
          style={{ scale }}
          className="sticky top-0 h-screen flex items-center justify-center"
        >
          <motion.div
            className="relative w-full h-full overflow-hidden"
            style={{
              borderRadius: radius,
              boxShadow: boxShadow as any,
              border: border as any,
            }}
          >
            {/* Content */}
            <div className="absolute inset-0 flex items-center justify-center">

              {/* Spline background */}
              <div
                className="absolute inset-0"
                style={{
                  transform: 'translateX(0.1%)',
                }}
              >
                <SplineViewer />
              </div>

              {/* Download button */}
              <div className="relative z-20">
                <button
                  onClick={() => setIsAuthDialogOpen(true)}
                  className="cursor-pointer hover:scale-105 transition-transform"
                >
                  <KineticText
                    text="DOWNLOAD"
                    className="text-lg md:text-xl lg:text-2xl font-bold text-white tracking-wider"
                    style={{ fontFamily: 'var(--font-michroma)' }}
                  />
                </button>
              </div>
            </div>
          </motion.div>
        </motion.div>
      </section>

      {/* Auth Dialog */}
      <AuthDialog
        isOpen={isAuthDialogOpen}
        onClose={() => setIsAuthDialogOpen(false)}
      />
    </>
  );
}