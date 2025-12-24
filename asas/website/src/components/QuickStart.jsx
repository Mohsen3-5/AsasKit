import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { Terminal, Copy, Check, Database, Server, Cpu } from 'lucide-react';

const QuickStart = () => {
    const [appName, setAppName] = useState('MyAwesomeApp');
    const [provider, setProvider] = useState('Sqlite');
    const [copied, setCopied] = useState(false);

    const providers = [
        { id: 'Sqlite', name: 'SQLite', icon: Cpu, color: '#fca311' },
        { id: 'Postgres', name: 'PostgreSQL', icon: Database, color: '#336791' },
        { id: 'SqlServer', name: 'SQL Server', icon: Server, color: '#CC2927' },
    ];

    const command = `asasctl new ${appName || 'MyApp'} --db ${provider}`;

    const copyToClipboard = () => {
        navigator.clipboard.writeText(command);
        setCopied(true);
        setTimeout(() => setCopied(false), 2000);
    };

    return (
        <section id="quick-start" style={{ padding: '8rem 0' }}>
            <div className="container">
                <motion.div
                    initial={{ opacity: 0, y: 50 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    className="glass-card glowing-border"
                    style={{
                        padding: '5rem',
                        borderRadius: '40px',
                        background: 'linear-gradient(135deg, var(--glass) 0%, rgba(99, 102, 241, 0.03) 100%)',
                        textAlign: 'center',
                        maxWidth: '1000px',
                        margin: '0 auto'
                    }}
                >
                    <motion.div
                        initial={{ scale: 0.9 }}
                        whileInView={{ scale: 1 }}
                        style={{ marginBottom: '3rem' }}
                    >
                        <h2 style={{
                            fontSize: 'clamp(2.5rem, 6vw, 4.5rem)',
                            fontWeight: 900,
                            marginBottom: '1.5rem',
                            letterSpacing: '-3px',
                            lineHeight: 1,
                            fontFamily: "'Unbounded', sans-serif"
                        }}>
                            READY TO <span className="primary-gradient-text">LAUNCH?</span>
                        </h2>
                        <p style={{ color: 'var(--text-muted)', fontSize: '1.2rem', maxWidth: '600px', margin: '0 auto' }}>
                            Configure and generate your scaffolding command in seconds.
                        </p>
                    </motion.div>

                    <div style={{
                        display: 'grid',
                        gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
                        gap: '3rem',
                        marginBottom: '4rem',
                        textAlign: 'left'
                    }}>
                        {/* App Name Input */}
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                            <label style={{ color: 'var(--text-main)', fontWeight: 800, fontSize: '0.9rem', textTransform: 'uppercase', letterSpacing: '2px' }}>
                                Application Name
                            </label>
                            <input
                                type="text"
                                value={appName}
                                onChange={(e) => setAppName(e.target.value)}
                                placeholder="Enter app name..."
                                style={{
                                    background: 'rgba(255, 255, 255, 0.03)',
                                    border: '2px solid var(--glass-border)',
                                    borderRadius: '16px',
                                    padding: '18px 24px',
                                    color: 'var(--text-main)',
                                    fontSize: '1.1rem',
                                    outline: 'none',
                                    transition: 'all 0.3s var(--ease-premium)',
                                    width: '100%',
                                    fontFamily: 'inherit'
                                }}
                                onFocus={(e) => e.target.style.borderColor = 'var(--primary)'}
                                onBlur={(e) => e.target.style.borderColor = 'var(--glass-border)'}
                            />
                        </div>

                        {/* Provider Selection */}
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                            <label style={{ color: 'var(--text-main)', fontWeight: 800, fontSize: '0.9rem', textTransform: 'uppercase', letterSpacing: '2px' }}>
                                Database Provider
                            </label>
                            <div style={{ display: 'flex', gap: '1rem' }}>
                                {providers.map((p) => (
                                    <motion.button
                                        key={p.id}
                                        whileHover={{ scale: 1.05 }}
                                        whileTap={{ scale: 0.95 }}
                                        onClick={() => setProvider(p.id)}
                                        style={{
                                            flex: 1,
                                            padding: '16px',
                                            borderRadius: '16px',
                                            border: `2px solid ${provider === p.id ? 'var(--primary)' : 'var(--glass-border)'}`,
                                            background: provider === p.id ? 'rgba(99, 102, 241, 0.1)' : 'transparent',
                                            color: provider === p.id ? 'var(--text-main)' : 'var(--text-muted)',
                                            cursor: 'pointer',
                                            display: 'flex',
                                            flexDirection: 'column',
                                            alignItems: 'center',
                                            gap: '0.6rem',
                                            transition: 'all 0.3s var(--ease-premium)'
                                        }}
                                    >
                                        <p.icon size={24} color={provider === p.id ? 'var(--primary)' : 'var(--text-muted)'} />
                                        <span style={{ fontSize: '0.8rem', fontWeight: 800 }}>{p.name}</span>
                                    </motion.button>
                                ))}
                            </div>
                        </div>
                    </div>

                    {/* Generated Command Area */}
                    <div style={{
                        background: '#010409',
                        padding: '2.5rem',
                        borderRadius: '24px',
                        border: '2px solid var(--glass-border)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        gap: '2rem',
                        fontFamily: '"Fira Code", monospace',
                        fontSize: '1.1rem',
                        boxShadow: '0 20px 40px rgba(0,0,0,0.4)',
                        position: 'relative',
                        overflow: 'hidden'
                    }}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', overflowX: 'auto', flex: 1 }}>
                            <Terminal size={24} color="var(--primary)" />
                            <div style={{ display: 'flex', gap: '0.8rem', whiteSpace: 'nowrap' }}>
                                <span style={{ color: 'var(--primary)' }}>asasctl</span>
                                <span style={{ color: '#ff7b72' }}>new</span>
                                <span style={{ color: '#a5d6ff' }}>{appName || 'MyApp'}</span>
                                <span style={{ color: '#ffa657' }}>--db</span>
                                <span style={{ color: '#79c0ff' }}>{provider}</span>
                            </div>
                        </div>
                        <motion.button
                            whileHover={{ scale: 1.1 }}
                            whileTap={{ scale: 0.9 }}
                            onClick={copyToClipboard}
                            style={{
                                background: copied ? '#238636' : 'rgba(255,255,255,0.05)',
                                border: '1px solid var(--glass-border)',
                                borderRadius: '12px',
                                color: 'white',
                                padding: '12px',
                                cursor: 'pointer',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center'
                            }}
                        >
                            {copied ? <Check size={20} /> : <Copy size={20} />}
                        </motion.button>
                    </div>
                </motion.div>
            </div>
        </section>
    );
};

export default QuickStart;
