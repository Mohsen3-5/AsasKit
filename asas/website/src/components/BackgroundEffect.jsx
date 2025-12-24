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
                scale: [1, 1.2, 1],
                opacity: [0.08, 0.12, 0.08],
                x: [0, 50, 0],
                y: [0, -30, 0]
            }}
            transition={{ duration: 15, repeat: Infinity, ease: "easeInOut" }}
            style={{
                position: 'absolute',
                width: '60vw',
                height: '60vw',
                borderRadius: '50%',
                background: 'radial-gradient(circle, rgba(99, 102, 241, 1) 0%, rgba(99, 102, 241, 0) 70%)',
                top: '-15%',
                right: '-10%',
                filter: 'blur(80px)'
            }}
        />

        <motion.div
            animate={{
                scale: [1, 1.3, 1],
                opacity: [0.04, 0.07, 0.04],
                x: [0, -40, 0],
                y: [0, 20, 0]
            }}
            transition={{ duration: 20, repeat: Infinity, ease: "easeInOut", delay: 2 }}
            style={{
                position: 'absolute',
                width: '50vw',
                height: '50vw',
                borderRadius: '50%',
                background: 'radial-gradient(circle, rgba(244, 63, 94, 1) 0%, rgba(244, 63, 94, 0) 70%)',
                bottom: '5%',
                left: '-10%',
                filter: 'blur(100px)'
            }}
        />

        <motion.div
            animate={{
                scale: [1, 1.1, 1],
                opacity: [0.03, 0.05, 0.03]
            }}
            transition={{ duration: 18, repeat: Infinity, ease: "easeInOut", delay: 1 }}
            style={{
                position: 'absolute',
                width: '40vw',
                height: '40vw',
                borderRadius: '50%',
                background: 'radial-gradient(circle, rgba(168, 85, 247, 1) 0%, rgba(168, 85, 247, 0) 70%)',
                top: '30%',
                left: '20%',
                filter: 'blur(120px)'
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
