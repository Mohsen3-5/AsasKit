import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Link, useLocation } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';

const ScrollToTop = () => {
  const { pathname } = useLocation();

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [pathname]);

  return null;
};
import {
  Shield,
  Lock,
  Users,
  Zap,
  ChevronRight,
  Github,
  BookOpen,
  Layers,
  Code,
  Terminal,
  Server,
  Cpu,
  Globe,
  Star,
  Sun,
  Moon,
  Menu,
  X,
  Plus
} from 'lucide-react';
import Docs from './components/Docs';
import BackgroundEffect from './components/BackgroundEffect';
import QuickStart from './components/QuickStart';

const ThemeToggle = ({ theme, toggleTheme }) => (
  <motion.button
    whileHover={{ scale: 1.1 }}
    whileTap={{ scale: 0.9 }}
    onClick={toggleTheme}
    className="glass-card"
    style={{
      padding: '10px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      cursor: 'pointer',
      color: 'white',
      border: '1px solid var(--glass-border)',
      background: 'var(--glass)',
      borderRadius: '12px'
    }}
  >
    {theme === 'dark' ? <Sun size={20} /> : <Moon size={20} color="var(--text-main)" />}
  </motion.button>
);

const Navbar = ({ theme, toggleTheme }) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <nav className="glass-card" style={{
      margin: '1.5rem 5%',
      padding: '0.8rem 2.5rem',
      position: 'sticky',
      top: '1.5rem',
      zIndex: 1000,
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      boxShadow: '0 15px 35px rgba(0,0,0,0.2)',
      border: '1px solid var(--glass-border)'
    }}>
      <Link to="/" style={{ textDecoration: 'none', display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
        <motion.div
          whileHover={{ rotate: 90, scale: 1.1 }}
          style={{
            width: '46px',
            height: '46px',
            position: 'relative',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center'
          }}
        >
          <svg width="46" height="46" viewBox="0 0 100 100" fill="none" xmlns="http://www.w3.org/2000/svg">
            <defs>
              <linearGradient id="logo-gradient" x1="0%" y1="0%" x2="100%" y2="100%">
                <stop offset="0%" stopColor="var(--primary)" />
                <stop offset="100%" stopColor="var(--accent)" />
              </linearGradient>
            </defs>
            <motion.path
              d="M50 10L90 85H10L50 10Z"
              stroke="url(#logo-gradient)"
              strokeWidth="4"
              initial={{ pathLength: 0, opacity: 0 }}
              animate={{ pathLength: 1, opacity: 1 }}
              transition={{ duration: 2, ease: "easeInOut" }}
            />
            <motion.path
              d="M50 30L75 75H25L50 30Z"
              fill="var(--primary)"
              initial={{ opacity: 0, scale: 0.5 }}
              animate={{ opacity: 0.2, scale: 1 }}
              transition={{ duration: 1, delay: 0.5 }}
            />
            <motion.path
              d="M35 85L50 60L65 85"
              stroke="var(--accent)"
              strokeWidth="6"
              strokeLinecap="round"
              initial={{ pathLength: 0 }}
              animate={{ pathLength: 1 }}
              transition={{ duration: 1.5, delay: 1, repeat: Infinity, repeatType: "reverse" }}
            />
          </svg>
          <div style={{
            position: 'absolute',
            inset: -8,
            background: 'var(--primary)',
            opacity: 0.1,
            filter: 'blur(15px)',
            borderRadius: '50%',
            zIndex: -1
          }} />
        </motion.div>

        <div style={{ display: 'flex', flexDirection: 'column', lineHeight: 0.75 }}>
          <div style={{ display: 'flex', alignItems: 'baseline' }}>
            <span style={{
              color: 'var(--text-main)',
              fontFamily: "'Unbounded', sans-serif",
              fontWeight: 900,
              fontSize: 'clamp(1.2rem, 4vw, 1.6rem)',
              letterSpacing: '-1.5px',
              textTransform: 'uppercase'
            }}>
              ASAS
            </span>
            <span style={{
              background: 'linear-gradient(135deg, var(--primary) 0%, var(--accent) 100%)',
              WebkitBackgroundClip: 'text',
              backgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              fontFamily: "'Unbounded', sans-serif",
              fontWeight: 900,
              fontSize: 'clamp(1.2rem, 4vw, 1.6rem)',
              letterSpacing: '-1.5px',
              textTransform: 'uppercase',
              marginLeft: '4px'
            }}>
              KIT
            </span>
          </div>
          <span className="hide-mobile" style={{
            fontSize: '0.6rem',
            color: 'var(--text-muted)',
            fontWeight: 800,
            letterSpacing: '5px',
            textTransform: 'uppercase',
            marginTop: '4px',
            opacity: 0.8
          }}>
            Foundation
          </span>
        </div>
      </Link>

      {/* Desktop Links */}
      <div className="hide-mobile" style={{ display: 'flex', gap: '2.5rem', alignItems: 'center' }}>
        <Link to="/docs" style={{ color: 'var(--text-muted)', textDecoration: 'none', fontWeight: 600, fontSize: '0.9rem', transition: 'color 0.3s' }} onMouseEnter={(e) => e.target.style.color = 'var(--text-main)'} onMouseLeave={(e) => e.target.style.color = 'var(--text-muted)'}>Documentation</Link>
        <a href="https://github.com/Mohsen3-5/AsasKit" target="_blank" rel="noopener noreferrer" style={{ color: 'var(--text-muted)', textDecoration: 'none', display: 'flex', alignItems: 'center', gap: '0.5rem', fontWeight: 600, fontSize: '0.9rem' }}>
          <Github size={18} />
          <span>GitHub</span>
        </a>
        <ThemeToggle theme={theme} toggleTheme={toggleTheme} />
        <Link to="/docs">
          <button className="glow-btn">Get Started</button>
        </Link>
      </div>

      {/* Mobile Toggle */}
      <div className="show-mobile-flex" style={{ display: 'none', gap: '1rem', alignItems: 'center' }}>
        <ThemeToggle theme={theme} toggleTheme={toggleTheme} />
        <button
          onClick={() => setIsOpen(!isOpen)}
          style={{ background: 'none', border: 'none', color: 'var(--text-main)', cursor: 'pointer' }}
        >
          {isOpen ? <X size={28} /> : <Menu size={28} />}
        </button>
      </div>

      {/* Mobile Drawer */}
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ x: '100%' }}
            animate={{ x: 0 }}
            exit={{ x: '100%' }}
            transition={{ type: 'spring', damping: 25, stiffness: 200 }}
            style={{
              position: 'fixed',
              top: 0,
              right: 0,
              bottom: 0,
              width: '85%',
              maxWidth: '320px',
              background: 'var(--bg-darker)',
              boxShadow: '-10px 0 50px rgba(0,0,0,0.3)',
              zIndex: 2000,
              padding: '4rem 2rem',
              display: 'flex',
              flexDirection: 'column',
              gap: '2rem',
              borderLeft: '1px solid var(--glass-border)'
            }}
          >
            <button
              onClick={() => setIsOpen(false)}
              style={{ position: 'absolute', top: '1.5rem', right: '1.5rem', background: 'none', border: 'none', color: 'var(--text-main)', cursor: 'pointer' }}
            >
              <X size={32} />
            </button>
            <Link to="/docs" onClick={() => setIsOpen(false)} style={{ color: 'var(--text-main)', textDecoration: 'none', fontSize: '1.5rem', fontWeight: 900, fontFamily: "'Unbounded', sans-serif" }}>DOCS</Link>
            <a href="https://github.com/Mohsen3-5/AsasKit" target="_blank" rel="noopener noreferrer" style={{ color: 'var(--text-main)', textDecoration: 'none', fontSize: '1.5rem', fontWeight: 900, fontFamily: "'Unbounded', sans-serif" }}>GITHUB</a>
            <div style={{ marginTop: 'auto' }}>
              <Link to="/docs" onClick={() => setIsOpen(false)}>
                <button className="glow-btn" style={{ width: '100%', padding: '20px' }}>Get Started</button>
              </Link>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </nav>
  );
};

const Particles = () => (
  <div style={{ position: 'absolute', inset: 0, pointerEvents: 'none', overflow: 'hidden' }}>
    {[...Array(10)].map((_, i) => (
      <motion.div
        key={i}
        animate={{
          x: [Math.random() * 100 - 50, Math.random() * 100 - 50],
          y: [Math.random() * 100 - 50, Math.random() * 100 - 50],
          opacity: [0.1, 0.3, 0.1],
        }}
        transition={{
          duration: 15 + Math.random() * 10,
          repeat: Infinity,
          ease: "linear"
        }}
        style={{
          position: 'absolute',
          top: `${Math.random() * 100}%`,
          left: `${Math.random() * 100}%`,
          width: '2px',
          height: '2px',
          background: 'var(--primary)',
          borderRadius: '50%',
          boxShadow: '0 0 10px var(--primary)',
          willChange: 'transform, opacity',
          backfaceVisibility: 'hidden',
          WebkitBackfaceVisibility: 'hidden'
        }}
      />
    ))}
  </div>
);

const Hero = () => (
  <section style={{
    padding: '8rem 0 6rem',
    textAlign: 'center',
    position: 'relative',
    minHeight: '80vh',
    display: 'flex',
    alignItems: 'center',
    overflow: 'hidden'
  }}>
    <Particles />
    <div className="container" style={{ position: 'relative', zIndex: 1 }}>
      <motion.div
        initial={{ opacity: 0, scale: 0.8 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 1, type: "spring" }}
        style={{ marginBottom: '3.5rem' }}
      >
        <span style={{
          background: 'rgba(99, 102, 241, 0.15)',
          color: 'var(--primary)',
          padding: '12px 32px',
          borderRadius: '100px',
          fontSize: '0.85rem',
          fontWeight: 900,
          border: '2px solid rgba(99, 102, 241, 0.3)',
          textTransform: 'uppercase',
          letterSpacing: '4px',
          display: 'inline-flex',
          alignItems: 'center',
          gap: '0.8rem'
        }}>
          <Zap size={16} fill="var(--primary)" /> Architecture For Visionaries
        </span>
      </motion.div>

      <motion.h1
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.8, delay: 0.2 }}
        style={{
          fontSize: 'clamp(3.5rem, 12vw, 8.5rem)',
          color: 'var(--text-main)',
          fontWeight: 900,
          marginBottom: '2.5rem',
          lineHeight: 0.85,
          letterSpacing: '-6px',
          fontFamily: "'Unbounded', sans-serif"
        }}
      >
        MODULARITY <br />
        <span className="primary-gradient-text">REDEFINED.</span>
      </motion.h1>

      <motion.p
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.8, delay: 0.3 }}
        style={{
          fontSize: 'clamp(1rem, 2.5vw, 1.35rem)',
          color: 'var(--text-muted)',
          maxWidth: '850px',
          margin: '0 auto 5rem',
          lineHeight: 1.6,
          fontWeight: 400
        }}
      >
        The definitive open-source foundation for .NET 9. Build sophisticated
        SaaS applications with enterprise-grade security and unmatched performance.
      </motion.p>

      <motion.div
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.8, delay: 0.4 }}
        style={{ display: 'flex', justifyContent: 'center', gap: '1.5rem', flexWrap: 'wrap' }}
      >
        <motion.button
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}
          onClick={() => document.getElementById('install')?.scrollIntoView({ behavior: 'smooth' })}
          className="glow-btn"
          style={{ fontSize: '1.2rem', padding: '24px 56px' }}
        >
          Install Now
        </motion.button>
        <motion.button
          whileHover={{ scale: 1.05, background: 'rgba(255,255,255,0.05)' }}
          whileTap={{ scale: 0.95 }}
          onClick={() => document.getElementById('quick-start')?.scrollIntoView({ behavior: 'smooth' })}
          className="glass-card"
          style={{
            padding: '24px 56px',
            borderRadius: '20px',
            fontWeight: 800,
            fontSize: '1.2rem',
            color: 'var(--text-main)',
            border: '2px solid var(--glass-border)',
            display: 'flex',
            alignItems: 'center',
            gap: '1.2rem'
          }}
        >
          Quick Start <Terminal size={24} />
        </motion.button>
      </motion.div>
    </div>
  </section>
);

const FeatureCard = ({ icon: Icon, title, description, delay }) => (
  <motion.div
    initial={{ opacity: 0, y: 40 }}
    whileInView={{ opacity: 1, y: 0 }}
    viewport={{ once: true, margin: "-100px" }}
    transition={{ duration: 0.7, delay }}
    className="glass-card glowing-border"
    style={{
      padding: '4.5rem 3.5rem',
      textAlign: 'left',
      position: 'relative',
      overflow: 'hidden'
    }}
  >
    <div style={{
      width: '72px',
      height: '72px',
      background: 'rgba(99, 102, 241, 0.1)',
      borderRadius: '24px',
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      color: 'var(--primary)',
      marginBottom: '3rem',
      border: '1px solid rgba(99, 102, 241, 0.2)'
    }}>
      <Icon size={36} />
    </div>
    <h3 style={{
      fontSize: '2.2rem',
      color: 'var(--text-main)',
      fontWeight: 900,
      marginBottom: '1.5rem',
      letterSpacing: '-1.5px',
      fontFamily: "'Unbounded', sans-serif"
    }}>{title}</h3>
    <p style={{ color: 'var(--text-muted)', lineHeight: 1.7, fontSize: '1.15rem' }}>{description}</p>
  </motion.div>
);

const ModuleShowcase = () => (
  <section style={{ padding: '12rem 0' }}>
    <div className="container">
      <motion.div
        initial={{ opacity: 0, x: -50 }}
        whileInView={{ opacity: 1, x: 0 }}
        viewport={{ once: true }}
        style={{ textAlign: 'left', marginBottom: '8rem' }}
      >
        <span style={{ color: 'var(--primary)', fontWeight: 900, textTransform: 'uppercase', letterSpacing: '5px', fontSize: '0.9rem' }}>Ecosystem</span>
        <h2 style={{
          fontSize: 'clamp(3rem, 8vw, 5.5rem)',
          color: 'var(--text-main)',
          fontWeight: 900,
          marginTop: '2rem',
          letterSpacing: '-3px',
          lineHeight: 1,
          fontFamily: "'Unbounded', sans-serif"
        }}>
          Powering The NEXT <br /> <span className="primary-gradient-text">Generation.</span>
        </h2>
      </motion.div>
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(380px, 1fr))',
        gap: '4rem'
      }}>
        <FeatureCard
          delay={0.1}
          icon={Shield}
          title="IDENTITY"
          description="A bulletproof authentication system with MFA, social logins, and global session management out of the box."
        />
        <FeatureCard
          delay={0.2}
          icon={Lock}
          title="RBAC PRO"
          description="Ultra-granular permission control designed to handle complex enterprise hierarchies with effortless ease."
        />
        <FeatureCard
          delay={0.3}
          icon={Globe}
          title="TENANCY"
          description="Schema-level data isolation that scales. Build SaaS applications that global giants can trust."
        />
      </div>
    </div>
  </section>
);

const Installation = () => {
  const [activeStep, setActiveStep] = useState(0);
  const steps = [
    {
      title: "Install AsasKit CLI",
      command: "dotnet tool install --global AsasKit.Cli",
      description: "One command to rule them all. Scaffolding, management, and more."
    }
  ];

  const copyToClipboard = (text) => {
    navigator.clipboard.writeText(text);
    const toast = document.createElement('div');
    toast.innerText = 'Copied to clipboard!';
    toast.style.position = 'fixed';
    toast.style.bottom = '2rem';
    toast.style.right = '2rem';
    toast.style.background = 'var(--primary)';
    toast.style.color = 'white';
    toast.style.padding = '12px 24px';
    toast.style.borderRadius = '12px';
    toast.style.zIndex = '9999';
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 2000);
  };

  return (
    <section id="install" style={{ padding: '8rem 0' }}>
      <div className="container">
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          whileInView={{ opacity: 1, scale: 1 }}
          viewport={{ once: true }}
          className="glass-card glowing-border"
          style={{
            padding: '4rem',
            borderRadius: '40px',
            background: 'linear-gradient(135deg, var(--glass) 0%, rgba(99, 102, 241, 0.05) 100%)',
            textAlign: 'center'
          }}
        >
          <h2 style={{
            fontSize: 'clamp(2.5rem, 6vw, 4rem)',
            fontWeight: 900,
            marginBottom: '1.5rem',
            letterSpacing: '-2px',
            fontFamily: "'Unbounded', sans-serif"
          }}>
            GET READY IN <span className="primary-gradient-text">SECONDS.</span>
          </h2>
          <p style={{ color: 'var(--text-muted)', marginBottom: '4rem', fontSize: '1.2rem', maxWidth: '700px', margin: '0 auto 4rem' }}>
            Install the CLI tool and start building your next masterpiece immediately.
          </p>

          <div style={{ display: 'flex', gap: '2rem', flexWrap: 'wrap', justifyContent: 'center' }}>
            {steps.map((step, idx) => (
              <motion.div
                key={idx}
                whileHover={{ y: -5 }}
                className="glass-card"
                style={{
                  flex: '1',
                  minWidth: '300px',
                  maxWidth: '500px',
                  padding: '2.5rem',
                  textAlign: 'left',
                  background: 'rgba(0,0,0,0.2)',
                  border: '1px solid var(--glass-border)',
                  display: 'flex',
                  flexDirection: 'column',
                  gap: '1.5rem'
                }}
              >
                <div>
                  <h3 style={{ color: 'var(--text-main)', fontSize: '1.8rem', fontWeight: 800 }}>{step.title}</h3>
                </div>
                <p style={{ color: 'var(--text-muted)', fontSize: '1rem' }}>{step.description}</p>
                <div style={{
                  background: '#010409',
                  padding: '1.2rem 1.5rem',
                  borderRadius: '16px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  fontFamily: '"Fira Code", monospace',
                  fontSize: '0.9rem',
                  border: '1px solid var(--glass-border)'
                }}>
                  <code style={{ color: '#e6edf3', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{step.command}</code>
                  <button
                    onClick={() => copyToClipboard(step.command)}
                    style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer', padding: '5px' }}
                    onMouseEnter={(e) => e.target.style.color = 'var(--text-main)'}
                    onMouseLeave={(e) => e.target.style.color = 'var(--text-muted)'}
                  >
                    <Terminal size={18} />
                  </button>
                </div>
              </motion.div>
            ))}
          </div>
        </motion.div>
      </div>
    </section>
  );
};

const LandingPage = () => (
  <>
    <Hero />
    <Installation />
    <QuickStart />
    <ModuleShowcase />
  </>
);

function App() {
  const [theme, setTheme] = useState(() => localStorage.getItem('theme') || 'dark');

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
  }, [theme]);

  const toggleTheme = () => setTheme(prev => prev === 'dark' ? 'light' : 'dark');

  return (
    <Router>
      <ScrollToTop />
      <div style={{ minHeight: '100vh', background: 'var(--bg-darker)', position: 'relative', transition: 'all 0.5s var(--ease-premium)' }}>
        <BackgroundEffect />
        <Navbar theme={theme} toggleTheme={toggleTheme} />
        <AnimatePresence mode="wait">
          <Routes>
            <Route path="/" element={<LandingPage />} />
            <Route path="/docs" element={<Docs />} />
          </Routes>
        </AnimatePresence>

        <footer style={{ padding: '12rem 0 6rem', borderTop: '2px solid var(--glass-border)', marginTop: '10rem', background: 'rgba(0,0,0,0.03)' }}>
          <div className="container">
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '8rem', marginBottom: '8rem' }}>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '2.5rem' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '1.2rem' }}>
                  <svg width="40" height="40" viewBox="0 0 100 100" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M50 10L90 85H10L50 10Z" stroke="var(--primary)" strokeWidth="8" />
                    <path d="M35 85L50 60L65 85" stroke="var(--accent)" strokeWidth="10" strokeLinecap="round" />
                  </svg>
                  <span style={{ fontFamily: "'Unbounded', sans-serif", fontWeight: 900, fontSize: '1.8rem', letterSpacing: '-2px' }}>ASAS KIT</span>
                </div>
                <p style={{ color: 'var(--text-muted)', fontSize: '1.2rem', lineHeight: 1.6, maxWidth: '400px' }}>
                  The architectural foundation for the web's next 10 years. Pure, modular, and uncompromising.
                </p>
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '6rem' }}>
                <div>
                  <h4 style={{ color: 'var(--text-main)', marginBottom: '2.5rem', fontSize: '1.2rem', fontWeight: 900, fontFamily: "'Unbounded', sans-serif" }}>TECH</h4>
                  <ul style={{ listStyle: 'none', padding: 0, color: 'var(--text-muted)', display: 'flex', flexDirection: 'column', gap: '1.5rem', fontSize: '1.1rem' }}>
                    <li><Link to="/docs" style={{ color: 'inherit', textDecoration: 'none' }}>Documentation</Link></li>
                    <li><a href="#" style={{ color: 'inherit', textDecoration: 'none' }}>Core Modules</a></li>
                    <li><a href="#" style={{ color: 'inherit', textDecoration: 'none' }}>CLI Source</a></li>
                  </ul>
                </div>
                <div>
                  <h4 style={{ color: 'var(--text-main)', marginBottom: '2.5rem', fontSize: '1.2rem', fontWeight: 900, fontFamily: "'Unbounded', sans-serif" }}>SOCIAL</h4>
                  <ul style={{ listStyle: 'none', padding: 0, color: 'var(--text-muted)', display: 'flex', flexDirection: 'column', gap: '1.5rem', fontSize: '1.1rem' }}>
                    <li><a href="#" style={{ color: 'inherit', textDecoration: 'none' }}>Discord Guild</a></li>
                    <li><a href="https://github.com/Mohsen3-5/AsasKit" target="_blank" rel="noopener noreferrer" style={{ color: 'inherit', textDecoration: 'none' }}>GitHub Repo</a></li>
                    <li><a href="#" style={{ color: 'inherit', textDecoration: 'none' }}>Twitter / X</a></li>
                  </ul>
                </div>
              </div>
            </div>
            <div style={{ textAlign: 'center', color: 'var(--text-muted)', fontSize: '1rem', borderTop: '2px solid var(--glass-border)', paddingTop: '6rem' }}>
              <p>© {new Date().getFullYear()} ASAS FOUNDATION. CREATED BY MOHSEN ALYOUSEF.</p>
            </div>
          </div>
        </footer>
      </div>
    </Router>
  );
}

export default App;
