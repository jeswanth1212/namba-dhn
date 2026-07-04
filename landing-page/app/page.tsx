import HeroSection from './components/HeroSection';
import FeaturesSection from './components/FeaturesSection';

export default function Home() {
  return (
    <div className="relative bg-black text-white">
      {/* Hero Section with Spline 3D */}
      <HeroSection />
      
      {/* Features Section */}
      <FeaturesSection />
    </div>
  );
}
