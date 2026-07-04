'use client';

import Spline from '@splinetool/react-spline';
import { useRef } from 'react';

export default function SplineViewer() {
  const splineRef = useRef<any>(null);

  const onLoad = (spline: any) => {
    splineRef.current = spline;
    
    // Try to access and zoom out the camera
    try {
      const camera = spline.findObjectByName('Camera');
      if (camera) {
        // Move camera back to zoom out
        camera.position.z += 1000; // Move camera away from scene
      }
      
      // Alternative: Try to access the scene's camera directly
      if (spline._scene && spline._scene.camera) {
        spline._scene.camera.position.z += 1000;
      }
    } catch (error) {
      console.log('Camera adjustment failed:', error);
    }
  };

  return (
    <div 
      className="w-full h-full"
      onWheel={(e) => {
        // Prevent Spline from capturing scroll events
        e.stopPropagation();
      }}
      style={{ pointerEvents: 'none' }}
    >
      <Spline
        scene="/assets/rotating_interactive_hero_section.spline"
        onLoad={onLoad}
        className="w-full h-full"
        style={{ pointerEvents: 'none' }}
      />
    </div>
  );
}
