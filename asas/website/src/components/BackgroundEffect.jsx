import React from 'react';
import { motion } from 'framer-motion';

const BackgroundEffect = () => (
    <div style={{
        position: 'fixed',
        top: 0,
        left: 0,
        width: '100vw',
        height: '100vh',
        zIndex: -2,
        overflow: 'hidden',
        backgroundColor: 'var(--bg-darker)'
    }}>
        {/* Animated Blobs */}
        <motion.div
            animate={{
                scale: [1, 1.1, 1],
                opacity: [0.06, 0.1, 0.06],
                x: [0, 30, 0],
                y: [0, -20, 0]
            }}
            transition={{ duration: 20, repeat: Infinity, ease: "easeInOut" }}
            style={{
                position: 'absolute',
                width: '60vw',
                height: '60vw',
                borderRadius: '50%',
                background: 'radial-gradient(circle, rgba(99, 102, 241, 0.8) 0%, rgba(99, 102, 241, 0) 70%)',
                top: '-15%',
                right: '-10%',
                filter: 'blur(80px)',
                willChange: 'transform, opacity',
                pointerEvents: 'none'
            }}
        />

        <motion.div
            animate={{
                scale: [1, 1.2, 1],
                opacity: [0.03, 0.05, 0.03],
                x: [0, -30, 0],
                y: [0, 15, 0]
            }}
            transition={{ duration: 25, repeat: Infinity, ease: "easeInOut", delay: 2 }}
            style={{
                position: 'absolute',
                width: '50vw',
                height: '50vw',
                borderRadius: '50%',
                background: 'radial-gradient(circle, rgba(244, 63, 94, 0.8) 0%, rgba(244, 63, 94, 0) 70%)',
                bottom: '10%',
                left: '-10%',
                filter: 'blur(100px)',
                willChange: 'transform, opacity',
                pointerEvents: 'none'
            }}
        />

        <motion.div
            animate={{
                scale: [1, 1.05, 1],
                opacity: [0.02, 0.04, 0.02]
            }}
            transition={{ duration: 22, repeat: Infinity, ease: "easeInOut", delay: 1 }}
            style={{
                position: 'absolute',
                width: '40vw',
                height: '40vw',
                borderRadius: '50%',
                background: 'radial-gradient(circle, rgba(168, 85, 247, 0.8) 0%, rgba(168, 85, 247, 0) 70%)',
                top: '30%',
                left: '20%',
                filter: 'blur(120px)',
                willChange: 'transform, opacity',
                pointerEvents: 'none'
            }}
        />

        {/* Noise/Grain Overlay (Optional but premium) */}
        <div style={{
            position: 'absolute',
            inset: 0,
            opacity: 0.02,
            pointerEvents: 'none',
            backgroundImage: 'url("https://grainy-gradients.vercel.app/noise.svg")',
        }} />
    </div>
);

export default BackgroundEffect;
