import React, { useState } from 'react';
import { motion } from 'framer-motion';
import {
    ChevronRight,
    Terminal,
    Package,
    ShieldCheck,
    Key,
    Database,
    Search,
    ExternalLink,
    Code2,
    Code
} from 'lucide-react';

const SidebarItem = ({ title, active, onClick, subItems, activeSubItem, onSubItemClick, isExpanded, level = 0 }) => {
    const [expanded, setExpanded] = React.useState(isExpanded || false);

    // Sync with isExpanded prop changes
    React.useEffect(() => {
        setExpanded(isExpanded || false);
    }, [isExpanded]);

    const hasSubItems = subItems && subItems.length > 0;

    return (
        <div>
            <motion.div
                onClick={() => {
                    if (hasSubItems) {
                        setExpanded(!expanded);
                    }
                    // Top-level items (level 0) should always change section
                    // Nested items with sub-items should only expand/collapse
                    if (onClick && (level === 0 || !hasSubItems)) {
                        onClick();
                    }
                }}
                whileHover={{ x: 5 }}
                style={{
                    padding: '0.8rem 1.2rem',
                    paddingLeft: `${1.2 + level * 1}rem`,
                    borderRadius: '12px',
                    cursor: 'pointer',
                    color: active ? 'var(--active-tab-text)' : 'var(--text-muted)',
                    background: active ? 'var(--active-tab-bg)' : 'transparent',
                    fontWeight: active ? 700 : 500,
                    transition: 'all 0.3s var(--ease-premium)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    border: active ? '1px solid rgba(99, 102, 241, 0.2)' : '1px solid transparent',
                    fontSize: level > 0 ? '0.85rem' : '0.95rem',
                    boxShadow: active ? '0 4px 12px rgba(99, 102, 241, 0.1)' : 'none'
                }}
            >
                {title}
                <ChevronRight size={16} style={{
                    opacity: hasSubItems ? 1 : (active ? 1 : 0),
                    transform: hasSubItems ? (expanded ? 'rotate(90deg)' : 'rotate(0)') : (active ? 'translateX(0)' : 'translateX(-10px)'),
                    transition: 'all 0.4s var(--ease-premium)',
                    color: 'inherit'
                }} />
            </motion.div>

            {hasSubItems && expanded && (
                <motion.div
                    initial={{ opacity: 0, height: 0 }}
                    animate={{ opacity: 1, height: 'auto' }}
                    exit={{ opacity: 0, height: 0 }}
                    transition={{ duration: 0.3 }}
                    style={{ marginTop: '0.5rem' }}
                >
                    {subItems.map((subItem, index) => {
                        // Check if this sub-item itself has sub-items (nested structure)
                        const hasNestedSubItems = subItem.subItems && subItem.subItems.length > 0;

                        if (hasNestedSubItems) {
                            // Recursive call for nested sub-items
                            return (
                                <SidebarItem
                                    key={index}
                                    title={subItem.title}
                                    active={activeSubItem === subItem.id}
                                    onClick={undefined} // Don't navigate for items with nested sub-items
                                    subItems={subItem.subItems}
                                    activeSubItem={activeSubItem}
                                    onSubItemClick={onSubItemClick}
                                    isExpanded={true}
                                    level={level + 1}
                                />
                            );
                        } else {
                            // Regular sub-item without further nesting
                            return (
                                <motion.div
                                    key={index}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        if (onSubItemClick) {
                                            onSubItemClick(subItem.id);
                                        }
                                    }}
                                    whileHover={{ x: 3 }}
                                    style={{
                                        padding: '0.6rem 1rem',
                                        paddingLeft: `${1 + (level + 1) * 1}rem`,
                                        borderRadius: '8px',
                                        cursor: 'pointer',
                                        color: activeSubItem === subItem.id ? 'var(--primary)' : 'var(--text-muted)',
                                        background: activeSubItem === subItem.id ? 'rgba(99, 102, 241, 0.1)' : 'transparent',
                                        fontWeight: activeSubItem === subItem.id ? 600 : 400,
                                        fontSize: '0.8rem',
                                        marginBottom: '0.3rem',
                                        transition: 'all 0.2s var(--ease-premium)',
                                        borderLeft: activeSubItem === subItem.id ? '2px solid var(--primary)' : '2px solid transparent',
                                        paddingLeft: `${0.8 + (level + 1) * 1}rem`
                                    }}
                                >
                                    {subItem.title}
                                </motion.div>
                            );
                        }
                    })}
                </motion.div>
            )}
        </div>
    );
};


const CodeBlock = ({ code, language = 'csharp' }) => (
    <div style={{
        background: 'var(--code-bg)',
        padding: '1.8rem',
        borderRadius: '20px',
        border: '1px solid var(--glass-border)',
        fontFamily: '"Fira Code", monospace',
        fontSize: '0.85rem',
        color: 'var(--text-main)',
        margin: '2rem 0',
        overflowX: 'auto',
        position: 'relative',
        boxShadow: '0 10px 30px rgba(0,0,0,0.05)'
    }}>
        <div style={{ display: 'flex', gap: '0.6rem', marginBottom: '1.5rem', alignItems: 'center' }}>
            <div style={{ display: 'flex', gap: '0.4rem' }}>
                <div style={{ width: '10px', height: '10px', borderRadius: '50%', background: '#ff5f56' }} />
                <div style={{ width: '10px', height: '10px', borderRadius: '50%', background: '#ffbd2e' }} />
                <div style={{ width: '10px', height: '10px', borderRadius: '50%', background: '#27c93f' }} />
            </div>
            <span style={{ marginLeft: 'auto', color: 'var(--text-muted)', fontSize: '0.7rem', textTransform: 'uppercase', letterSpacing: '1px', fontWeight: 700 }}>{language}</span>
        </div>
        <pre style={{ margin: 0, lineHeight: 1.7 }}>
            <code dangerouslySetInnerHTML={{
                __html: code
                    .replace(/("[^"]*")/g, '<span class="syn-str">$1</span>')
                    .replace(/\b(public|private|protected|internal|class|interface|record|sealed|readonly|async|await|Task|dynamic|Guid|string|bool|namespace|using|new|return|await|void|int|IActionResult|Ok)\b/g, '<span class="syn-kw">$1</span>')
                    .replace(/\b(RegisterAsync|LoginAsync|ExternalAuthAsync|ForgotPasswordAsync|ResetPasswordAsync|ChangePasswordAsync|LogoutAsync|RegisterDeviceAsync|CreateAsync|Create|Process|ProcessAsync|Login|Register|Exploit|Navigate)\b/g, '<span class="syn-fn">$1</span>')
                    .replace(/\/\/.*/g, '<span class="syn-cmt">$&</span>')
                    .replace(/\[([^\]]+)\]/g, '<span class="syn-attr">[$1]</span>')
            }} />
        </pre>
    </div>
);


const Docs = () => {
    const [activeSection, setActiveSection] = useState('Overview');
    const [activeSubItem, setActiveSubItem] = useState(null);

    // Navigation structure with sub-items
    const navigationStructure = [
        { id: 'Overview', title: 'Overview', subItems: [] },
        {
            id: 'Authentication',
            title: 'Authentication',
            subItems: [
                { id: 'auth-architecture', title: 'Architecture' },
                {
                    id: 'auth-features',
                    title: 'Core Features',
                    subItems: [
                        { id: 'auth-jwt', title: 'JWT & Refresh Tokens' },
                        { id: 'auth-social', title: 'Social Authentication' },
                        { id: 'auth-device', title: 'Device Management' }
                    ]
                },
                { id: 'auth-api', title: 'API Reference' },
                { id: 'auth-benefits', title: 'Benefits' },
                { id: 'auth-quickstart', title: 'Quick Start' }
            ]
        },
        {
            id: 'Authorization',
            title: 'Authorization',
            subItems: [
                { id: 'perm-architecture', title: 'Architecture' },
                {
                    id: 'perm-features',
                    title: 'Core Features',
                    subItems: [
                        { id: 'perm-attribute', title: 'Permission Attribute' },
                        { id: 'perm-caching', title: 'High-Performance Caching' },
                        { id: 'perm-rbac', title: 'Role & User Overrides' },
                        { id: 'perm-providers', title: 'Permission Providers' }
                    ]
                },
                { id: 'perm-api', title: 'API Reference' },
                { id: 'perm-benefits', title: 'Benefits' },
                { id: 'perm-quickstart', title: 'Quick Start' }
            ]
        },
        {
            id: 'Multi-Tenancy',
            title: 'Multi-Tenancy',
            subItems: [
                { id: 'tenant-architecture', title: 'Architecture' },
                {
                    id: 'tenant-features',
                    title: 'Core Features',
                    subItems: [
                        { id: 'tenant-resolution', title: 'Resolution Strategies' },
                        { id: 'tenant-ef', title: 'EF Core Filtering' },
                        { id: 'tenant-service', title: 'ICurrentTenant Service' }
                    ]
                },
                { id: 'tenant-benefits', title: 'Benefits' },
                { id: 'tenant-quickstart', title: 'Quick Start' }
            ]
        },
        { id: 'CLI & Templates', title: 'CLI & Templates', subItems: [] }
    ];

    // Scroll spy: Update active section based on scroll position
    React.useEffect(() => {
        if (activeSection !== 'Authentication' && activeSection !== 'Authorization' && activeSection !== 'Multi-Tenancy') return;


        let sectionIds = [];
        if (activeSection === 'Authentication') {
            sectionIds = [
                'auth-architecture',
                'auth-features',
                'auth-jwt',
                'auth-social',
                'auth-device',
                'auth-api',
                'auth-benefits',
                'auth-quickstart'
            ];
        } else if (activeSection === 'Authorization') {
            sectionIds = [
                'perm-architecture',
                'perm-features',
                'perm-attribute',
                'perm-caching',
                'perm-rbac',
                'perm-providers',
                'perm-api',
                'perm-benefits',
                'perm-quickstart'
            ];
        } else {
            // Multi-Tenancy
            sectionIds = [
                'tenant-architecture',
                'tenant-features',
                'tenant-resolution',
                'tenant-ef',
                'tenant-service',
                'tenant-benefits',
                'tenant-quickstart'
            ];
        }


        const observerOptions = {
            root: null,
            rootMargin: '-20% 0px -70% 0px', // Trigger when section is in the middle of viewport
            threshold: 0
        };

        const observerCallback = (entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    setActiveSubItem(entry.target.id);
                }
            });
        };

        const observer = new IntersectionObserver(observerCallback, observerOptions);

        // Observe all sections
        sectionIds.forEach(id => {
            const element = document.getElementById(id);
            if (element) {
                observer.observe(element);
            }
        });

        // Cleanup
        return () => {
            sectionIds.forEach(id => {
                const element = document.getElementById(id);
                if (element) {
                    observer.unobserve(element);
                }
            });
        };
    }, [activeSection]);

    const scrollToSection = (sectionId) => {
        const element = document.getElementById(sectionId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
            setActiveSubItem(sectionId);
        }
    };


    const sections = {
        'Overview': (
            <div className="animate-fade-in">
                <h1 className="primary-gradient-text" style={{ fontSize: '3rem', marginBottom: '1rem' }}>Framework Overview</h1>
                <p style={{ color: 'var(--text-muted)', fontSize: '1.2rem', lineHeight: 1.6, marginBottom: '2rem' }}>
                    Asas Kit is a modular development foundation designed for high-end SaaS applications. It provides a standardized architecture for Identity, Multi-Tenancy, and Granular Authorizations.
                </p>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem', marginTop: '3rem' }}>
                    <div className="glass-card" style={{ padding: '1.5rem' }}>
                        <h3 style={{ marginBottom: '0.5rem', color: 'var(--primary)' }}>Core Modules</h3>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>6+ pre-built modules for Identity, Tenancy, Permissions, Media, and more.</p>
                    </div>
                    <div className="glass-card" style={{ padding: '1.5rem' }}>
                        <h3 style={{ marginBottom: '0.5rem', color: 'white' }}>Enterprise Ready</h3>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>Built on .NET 9 with Clean Architecture and DDD principles.</p>
                    </div>
                </div>
            </div>
        ),
        'Authentication': (
            <div className="animate-fade-in">
                <h1 className="primary-gradient-text" style={{ fontSize: '3rem', marginBottom: '0.5rem' }}>Asas.Identity Module</h1>
                <p style={{ color: 'var(--text-muted)', fontSize: '1.3rem', marginBottom: '3rem', lineHeight: 1.7 }}>
                    Enterprise-grade authentication and user management system built on ASP.NET Core Identity. From user registration to multi-device session control, every feature is designed for production-ready SaaS applications.
                </p>

                <div className="glass-card" style={{ padding: '2rem', marginBottom: '3rem', background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.05) 0%, rgba(168, 85, 247, 0.05) 100%)' }}>
                    <h3 style={{ marginBottom: '1rem' }}>🎯 What You Get</h3>
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Complete user lifecycle management</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> JWT with automatic refresh</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Social login (Google, Facebook, Apple)</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Email verification with codes</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Password reset flows</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Device tracking & push notifications</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Multi-device logout</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Token rotation for security</div>
                    </div>
                </div>

                <h2 id="auth-architecture" style={{ fontSize: '2rem', marginTop: '4rem', marginBottom: '1.5rem', scrollMarginTop: '120px' }}>🏗️ Architecture Overview</h2>
                <p style={{ color: 'var(--text-muted)', marginBottom: '2rem', lineHeight: 1.7 }}>
                    Clean Architecture with strict separation of concerns. Each layer has a specific responsibility, making the system maintainable and testable.
                </p>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '1.5rem', marginBottom: '4rem' }}>
                    <div className="glass-card glowing-border" style={{ padding: '2rem', textAlign: 'center' }}>
                        <div style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>📦</div>
                        <h4 style={{ color: 'var(--primary)', marginBottom: '0.8rem', fontSize: '1.1rem' }}>Domain Layer</h4>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.6 }}>
                            Core entities: AsasUser, AsasRole, RefreshToken, EmailConfirmationCode, UserDevice. Pure business logic with no dependencies.
                        </p>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem', textAlign: 'center' }}>
                        <div style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>⚙️</div>
                        <h4 style={{ color: 'var(--primary)', marginBottom: '0.8rem', fontSize: '1.1rem' }}>Application Layer</h4>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.6 }}>
                            Services: AuthService, TokenService, EmailConfirmationService, UserDeviceService. Orchestrates business workflows.
                        </p>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem', textAlign: 'center' }}>
                        <div style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>🔌</div>
                        <h4 style={{ color: 'var(--primary)', marginBottom: '0.8rem', fontSize: '1.1rem' }}>Infrastructure Layer</h4>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.6 }}>
                            ASP.NET Identity integration, JWT generation, external OAuth providers (Google, Facebook, Apple).
                        </p>
                    </div>
                </div>

                <h2 id="auth-features" style={{ fontSize: '2rem', marginTop: '4rem', marginBottom: '1.5rem', scrollMarginTop: '120px' }}>✨ Core Features</h2>

                <div id="auth-jwt" style={{ marginBottom: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ color: 'var(--primary)', marginBottom: '1rem' }}>🔐 JWT Authentication with Refresh Tokens</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Secure token-based authentication using industry-standard JWT. Access tokens are short-lived (15-60 minutes), while refresh tokens provide long-term access with automatic rotation for enhanced security.
                    </p>
                    <div className="glass-card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
                        <h4 style={{ fontSize: '0.9rem', marginBottom: '0.8rem', color: 'var(--text-main)' }}>🔄 Token Rotation</h4>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', lineHeight: 1.6 }}>
                            Each refresh generates a new token pair and invalidates the old refresh token. Stores metadata including IP, user agent, device info, and tracks token chains for security auditing.
                        </p>
                    </div>
                    <CodeBlock code={`public class RefreshToken
{
    public string TokenHash { get; set; }          // Hashed token (never store raw)
    public DateTime ExpiresAtUtc { get; set; }     // Token expiration
    public string? CreatedByIp { get; set; }       // IP tracking
    public string? UserAgent { get; set; }         // Device tracking
    
    // Token Rotation
    public DateTime? RevokedAtUtc { get; set; }    // When revoked
    public string? ReplacedByTokenHash { get; set; } // New token hash
    
    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}`} />
                </div>


                <div id="auth-social" style={{ marginBottom: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ color: 'var(--primary)', marginBottom: '1rem' }}>🌐 Social Authentication</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Seamless integration with major OAuth providers. Automatically creates user accounts, syncs profile data, and returns user information including name and profile image.
                    </p>
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '1rem', marginBottom: '1.5rem' }}>
                        <div className="glass-card" style={{ padding: '1.2rem', textAlign: 'center' }}>
                            <div style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>🔵</div>
                            <strong>Google</strong>
                            <p style={{ color: 'var(--text-muted)', fontSize: '0.8rem', marginTop: '0.3rem' }}>JWT validation with audience check</p>
                        </div>
                        <div className="glass-card" style={{ padding: '1.2rem', textAlign: 'center' }}>
                            <div style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>📘</div>
                            <strong>Facebook</strong>
                            <p style={{ color: 'var(--text-muted)', fontSize: '0.8rem', marginTop: '0.3rem' }}>Graph API + Limited Login</p>
                        </div>
                        <div className="glass-card" style={{ padding: '1.2rem', textAlign: 'center' }}>
                            <div style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>🍎</div>
                            <strong>Apple</strong>
                            <p style={{ color: 'var(--text-muted)', fontSize: '0.8rem', marginTop: '0.3rem' }}>Sign in with Apple JWT</p>
                        </div>
                    </div>
                    <CodeBlock code={`// External login returns enriched user data
public sealed record ExternalAuthResult(
    string Token,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    bool EmailConfirmed,
    string Name,              // User's full name from provider
    string? ProfileImageUrl   // Profile picture URL
);

// Usage
var result = await authService.ExternalAuthAsync(new ExternalAuthRequest
{
    Provider = "google",      // "google", "facebook", or "apple"
    IdToken = "<id_token>"
});`} />
                </div>

                <div style={{ marginBottom: '3rem' }}>
                    <h3 style={{ color: 'var(--primary)', marginBottom: '1rem' }}>📧 Email Confirmation</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Uses secure numeric codes instead of long URL tokens for better UX. Codes expire after a configurable time period and are stored hashed in the database.
                    </p>
                </div>

                <div id="auth-device" style={{ marginBottom: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ color: 'var(--primary)', marginBottom: '1rem' }}>📱 Device Management</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Track user devices for push notifications and session management. Register device tokens (FCM/APNS) and manage multi-device logout scenarios.
                    </p>
                    <CodeBlock code={`// Register device for push notifications
await authService.RegisterDeviceAsync(new RegisterDeviceRequest
{
    UserId = user.Id,
    DeviceToken = "fcm_token_here",
    DeviceType = "Android"  // or "iOS", "Web"
});

// Logout from specific device
await authService.LogoutAsync(new LogoutRequest
{
    UserId = user.Id,
    DeviceToken = "fcm_token",
    AllDevices = false
});

// Logout from all devices (global logout)
await authService.LogoutAsync(new LogoutRequest
{
    UserId = user.Id,
    AllDevices = true
});`} />
                </div>

                <h2 id="auth-api" style={{ fontSize: '2rem', marginTop: '4rem', marginBottom: '1.5rem', scrollMarginTop: '120px' }}>📚 API Reference</h2>

                <h3 style={{ marginBottom: '1rem' }}>IAuthService Interface</h3>
                <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem' }}>
                    Complete authentication service covering all user lifecycle operations.
                </p>
                <CodeBlock code={`public interface IAuthService
{
    // User Registration
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
    
    // Login & Authentication
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<ExternalAuthResult> ExternalAuthAsync(ExternalAuthRequest request);
    
    // Password Management
    Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(ChangePasswordRequest request);
    
    // Session & Device Management
    Task LogoutAsync(LogoutRequest r);
    Task RegisterDeviceAsync(RegisterDeviceRequest r);
}`} />

                <h3 style={{ marginTop: '2.5rem', marginBottom: '1rem' }}>Authentication Response</h3>
                <CodeBlock code={`public sealed record AuthResult(
    string Token,              // JWT access token
    string RefreshToken,       // Refresh token for renewal
    DateTime ExpiresAtUtc,     // Token expiration timestamp
    bool EmailConfirmed        // Email verification status
);`} />

                <h2 id="auth-benefits" style={{ fontSize: '2rem', marginTop: '4rem', marginBottom: '1.5rem', scrollMarginTop: '120px' }}>💡 Why Choose Asas.Identity?</h2>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem', marginBottom: '3rem' }}>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>🔒 Security First</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Refresh tokens stored as hashes, never plaintext</li>
                            <li>Automatic token rotation prevents replay attacks</li>
                            <li>IP and device tracking for security audits</li>
                            <li>Built on battle-tested ASP.NET Core Identity</li>
                        </ul>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>🚀 Production Ready</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Complete flows for registration, login, password reset</li>
                            <li>Email confirmation with customizable templates</li>
                            <li>Multi-tenant support out of the box</li>
                            <li>Role and permission integration</li>
                        </ul>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>🌐 Multi-Platform</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Social login for web and mobile apps</li>
                            <li>Device token management for push notifications</li>
                            <li>Cross-platform session control</li>
                            <li>OAuth 2.0 and OpenID Connect support</li>
                        </ul>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>🔧 Highly Extensible</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Clean Architecture for easy customization</li>
                            <li>Interface-based design for testability</li>
                            <li>Add custom OAuth providers easily</li>
                            <li>Override any service with your implementation</li>
                        </ul>
                    </div>
                </div>

                <div id="auth-quickstart" className="glass-card" style={{ padding: '2.5rem', background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.1) 0%, rgba(168, 85, 247, 0.1) 100%)', marginTop: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ marginBottom: '1rem' }}>🎓 Quick Start</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        The Identity module is pre-configured in Asas Kit templates. Simply inject <code style={{ background: 'var(--code-bg)', padding: '0.2rem 0.5rem', borderRadius: '4px', color: 'var(--primary)' }}>IAuthService</code> and start using it!
                    </p>
                    <CodeBlock code={`[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    
    public AuthController(IAuthService auth) => _auth = auth;
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        return Ok(result);
    }
}`} />
                </div>
            </div>
        ),
        'Authorization': (
            <div className="animate-fade-in">
                <h1 className="primary-gradient-text" style={{ fontSize: '3rem', marginBottom: '0.5rem' }}>Asas.Permission Module</h1>
                <p style={{ color: 'var(--text-muted)', fontSize: '1.3rem', marginBottom: '3rem', lineHeight: 1.7 }}>
                    Granular, high-performance permission system built on ASP.NET Core Authorization. From role-based permissions to user-specific overrides, everything is designed for enterprise SaaS applications with caching for blazing fast checks.
                </p>

                <div className="glass-card" style={{ padding: '2rem', marginBottom: '3rem', background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.05) 0%, rgba(168, 85, 247, 0.05) 100%)' }}>
                    <h3 style={{ marginBottom: '1rem' }}>🎯 What You Get</h3>
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Policy-based authorization</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Simple [RequiresPermission] attribute</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Role-based permissions</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> User-specific overrides</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Distributed cache integration</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Permission definition providers</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Multi-tenant aware</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Automatic policy generation</div>
                    </div>
                </div>

                <h2 id="perm-architecture" style={{ fontSize: '2rem', marginTop: '4rem', marginBottom: '1.5rem', scrollMarginTop: '120px' }}>🏗️ Architecture Overview</h2>
                <p style={{ color: 'var(--text-muted)', marginBottom: '2rem', lineHeight: 1.7 }}>
                    Built on ASP.NET Core's policy-based authorization with clean architecture. The system automatically integrates with authentication and tenancy modules for seamless permission checking.
                </p>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '1.5rem', marginBottom: '4rem' }}>
                    <div className="glass-card glowing-border" style={{ padding: '2rem', textAlign: 'center' }}>
                        <div style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>📦</div>
                        <h4 style={{ color: 'var(--primary)', marginBottom: '0.8rem', fontSize: '1.1rem' }}>Domain Layer</h4>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.6 }}>
                            Entities: AsasPermission, RolePermission, UserPermissionOverride. Pure permission models with no dependencies.
                        </p>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem', textAlign: 'center' }}>
                        <div style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>⚙️</div>
                        <h4 style={{ color: 'var(--primary)', marginBottom: '0.8rem', fontSize: '1.1rem' }}>Application Layer</h4>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.6 }}>
                            Services: PermissionChecker, PermissionHandler, PolicyProvider, PermissionDefinitionStore.
                        </p>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem', textAlign: 'center' }}>
                        <div style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>🔌</div>
                        <h4 style={{ color: 'var(--primary)', marginBottom: '0.8rem', fontSize: '1.1rem' }}>ASP.NET Integration</h4>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.6 }}>
                            Custom policy provider, authorization handlers, and automatic policy discovery for permissions.
                        </p>
                    </div>
                </div>

                <h2 id="perm-features" style={{ fontSize: '2rem', marginTop: '4rem', marginBottom: '1.5rem', scrollMarginTop: '120px' }}>✨ Core Features</h2>

                <div id="perm-attribute" style={{ marginBottom: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ color: 'var(--primary)', marginBottom: '1rem' }}>🔐 Simple Permission Attribute</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Use the <code style={{ background: 'var(--code-bg)', padding: '0.2rem 0.5rem', borderRadius: '4px', color: 'var(--primary)' }}>[RequiresPermission]</code> attribute at the controller or action level. The system automatically integrates with ASP.NET Core's authorization pipeline.
                    </p>
                    <CodeBlock code={`[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    // Single permission check
    [RequiresPermission("Permissions.Users.Create")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        return Ok(await _userService.CreateAsync(dto));
    }

    // Multiple permissions (all must be granted)
    [RequiresPermission("Permissions.Users.View")]
    [RequiresPermission("Permissions.Users.Sensitive")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        return Ok(await _userService.GetAsync(id));
    }
}`} />
                </div>

                <div id="perm-caching" style={{ marginBottom: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ color: 'var(--primary)', marginBottom: '1rem' }}>⚡ High-Performance Caching</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Permission checks are cached in distributed cache (Redis) for 10 minutes. The system:
                    </p>
                    <ul style={{ color: 'var(--text-muted)', lineHeight: 2, marginBottom: '1.5rem', paddingLeft: '1.5rem' }}>
                        <li>Builds an effective permission map combining all user roles</li>
                        <li>Applies user-specific overrides on top</li>
                        <li>Caches the final result per user per tenant</li>
                        <li>Returns cached results on subsequent requests (microsecond latency)</li>
                    </ul>
                    <CodeBlock code={`// Behind the scenes when you use [RequiresPermission]
public async Task<bool> IsGrantedAsync(Guid userId, string permission, int? tenantId)
{
    var cacheKey = $"perm:{tenantId}:{userId}";
    
    // Try cache first
    var cachedMap = await _cache.GetAsync(cacheKey);
    if (cachedMap != null) 
        return cachedMap[permission];
    
    // Build effective permission map
    var roleIds = await GetUserRoleIds(userId, tenantId);
    var rolePerms = await GetRolePermissions(roleIds, tenantId);
    var userOverrides = await GetUserOverrides(userId, tenantId);
    
    // Combine and cache
    var map = CombinePermissions(rolePerms, userOverrides);
    await _cache.SetAsync(cacheKey, map, TimeSpan.FromMinutes(10));
    
    return map[permission];
}`} />
                </div>


                <div id="perm-rbac" style={{ marginBottom: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ color: 'var(--primary)', marginBottom: '1rem' }}>👥 Role-Based + User Overrides</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Permissions can be granted to roles (standard RBAC) or overridden per-user for exceptional cases. User overrides always take precedence over role permissions.
                    </p>
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem', marginBottom: '1.5rem' }}>
                        <div className="glass-card" style={{ padding: '1.5rem' }}>
                            <h4 style={{ fontSize: '1rem', marginBottom: '0.8rem', color: 'var(--text-main)' }}>Role Permissions</h4>
                            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', lineHeight: 1.6 }}>
                                Assign permissions to roles (Admin, Manager, User). All users in that role inherit the permissions.
                            </p>
                        </div>
                        <div className="glass-card" style={{ padding: '1.5rem' }}>
                            <h4 style={{ fontSize: '1rem', marginBottom: '0.8rem', color: 'var(--text-main)' }}>User Overrides</h4>
                            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', lineHeight: 1.6 }}>
                                Grant or revoke specific permissions for individual users, overriding their role permissions.
                            </p>
                        </div>
                    </div>
                </div>


                <div id="perm-providers" style={{ marginBottom: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ color: 'var(--primary)', marginBottom: '1rem' }}>📋 Permission Definition Providers</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Define your application's permissions in code using providers. The system discovers and registers them automatically at startup.
                    </p>
                    <CodeBlock code={`public class MyAppPermissionProvider : IPermissionDefinitionProvider
{
    public void Define(PermissionDefinitionContext ctx)
    {
        // Define permission groups
        ctx.Add(
            "Permissions.Products.View",
            "View Products",
            "Can view product listings",
            "Product Management");

        ctx.Add(
            "Permissions.Products.Create",
            "Create Products",
            "Can create new products",
            "Product Management");

        ctx.Add(
            "Permissions.Orders.Process",
            "Process Orders",
            "Can process and fulfill orders",
            "Order Management");
    }
}`} />
                </div>

                <h2 id="perm-api" style={{ fontSize: '2rem', marginTop: '4rem', marginBottom: '1.5rem', scrollMarginTop: '120px' }}>📚 API Reference</h2>

                <h3 style={{ marginBottom: '1rem' }}>RequiresPermissionAttribute</h3>
                <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem' }}>
                    Declarative authorization attribute for controllers and actions.
                </p>
                <CodeBlock code={`[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequiresPermissionAttribute : AuthorizeAttribute
{
    public RequiresPermissionAttribute(string permission)
    {
        Policy = $"perm:{permission}";
    }
}`} />

                <h3 style={{ marginTop: '2.5rem', marginBottom: '1rem' }}>IPermissionChecker Interface</h3>
                <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem' }}>
                    Programmatic permission checking for complex scenarios.
                </p>
                <CodeBlock code={`public interface IPermissionChecker
{
    Task<bool> IsGrantedAsync(
        Guid userId, 
        string permission, 
        int? tenantId, 
        CancellationToken ct = default);
}

// Usage
var canCreate = await _permissionChecker.IsGrantedAsync(
    userId, 
    "Permissions.Products.Create", 
    tenantId);`} />

                <h2 id="perm-benefits" style={{ fontSize: '2rem', marginTop: '4rem', marginBottom: '1.5rem', scrollMarginTop: '120px' }}>💡 Why Choose Asas.Permission?</h2>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem', marginBottom: '3rem' }}>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>⚡ Blazing Fast</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Distributed cache (Redis) for microsecond checks</li>
                            <li>Single query builds entire permission map</li>
                            <li>10-minute cache reduces DB load dramatically</li>
                            <li>Automatic cache invalidation on permission changes</li>
                        </ul>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>🎯 Simple Yet Powerful</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Just use [RequiresPermission] attribute</li>
                            <li>Works with existing ASP.NET patterns</li>
                            <li>Supports controller-level and action-level authorization</li>
                            <li>No learning curve if you know ASP.NET</li>
                        </ul>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>🏢 Enterprise Ready</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Role-based permissions for standard access control</li>
                            <li>User overrides for exceptional cases</li>
                            <li>Multi-tenant isolation built-in</li>
                            <li>Permission definition providers for code-based setup</li>
                        </ul>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>🔧 Flexible Integration</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Works with Identity module seamlessly</li>
                            <li>Tenant-aware permission isolation</li>
                            <li>Define permissions in code or database</li>
                            <li>Easy to extend with custom providers</li>
                        </ul>
                    </div>
                </div>

                <div id="perm-quickstart" className="glass-card" style={{ padding: '2.5rem', background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.1) 0%, rgba(168, 85, 247, 0.1) 100%)', marginTop: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ marginBottom: '1rem' }}>🎓 Quick Start</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        The Permission module is pre-configured in Asas Kit templates. Just use the <code style={{ background: 'var(--code-bg)', padding: '0.2rem 0.5rem', borderRadius: '4px', color: 'var(--primary)' }}>[RequiresPermission]</code> attribute!
                    </p>
                    <CodeBlock code={`[ApiController]
[Route("api/products")]
[RequiresPermission("Permissions.Products.View")] // Applied to entire controller
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        // Only users with Products.View can access
        return Ok(await _products.GetAllAsync());
    }

    [RequiresPermission("Permissions.Products.Create")] // Action-level override
    [HttpPost]
    public async Task<IActionResult> Create(ProductDto dto)
    {
        // Requires both Products.View AND Products.Create
        return Ok(await _products.CreateAsync(dto));
    }
}`} />
                </div>
            </div>
        ),
        'Multi-Tenancy': (
            <div className="animate-fade-in">
                <h1 className="primary-gradient-text" style={{ fontSize: '3rem', marginBottom: '0.5rem' }}>Asas.Tenancy Module</h1>
                <p style={{ color: 'var(--text-muted)', fontSize: '1.3rem', marginBottom: '3rem', lineHeight: 1.7 }}>
                    Production-ready multi-tenancy built into EF Core with automatic tenant isolation. From subdomain-based resolution to database-level filtering, everything works seamlessly out of the box.
                </p>

                <div className="glass-card" style={{ padding: '2rem', marginBottom: '3rem', background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.05) 0%, rgba(168, 85, 247, 0.05) 100%)' }}>
                    <h3 style={{ marginBottom: '1rem' }}>🎯 What You Get</h3>
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Automatic tenant resolution</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> EF Core query filtering</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Subdomain-based tenants</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Header/Route/Claims resolvers</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Automatic TenantId assignment</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> ICurrentTenant service</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Zero configuration needed</div>
                        <div><strong style={{ color: 'var(--primary)' }}>✓</strong> Multi-database support</div>
                    </div>
                </div>

                <h2 id="tenant-architecture" style={{ fontSize: '2rem', marginTop: '4rem', marginBottom: '1.5rem', scrollMarginTop: '120px' }}>🏗️ Architecture Overview</h2>
                <p style={{ color: 'var(--text-muted)', marginBottom: '2rem', lineHeight: 1.7 }}>
                    Built on EF Core with clean separation of concerns. Tenants are resolved at the middleware level and automatically applied to all database queries.
                </p>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '1.5rem', marginBottom: '4rem' }}>
                    <div className="glass-card glowing-border" style={{ padding: '2rem', textAlign: 'center' }}>
                        <div style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>🌐</div>
                        <h4 style={{ color: 'var(--primary)', marginBottom: '0.8rem', fontSize: '1.1rem' }}>Resolution Layer</h4>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.6 }}>
                            Middleware resolves tenant from subdomain, headers, routes, or claims. Stores tenant context in HttpContext.
                        </p>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem', textAlign: 'center' }}>
                        <div style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>🗄️</div>
                        <h4 style={{ color: 'var(--primary)', marginBottom: '0.8rem', fontSize: '1.1rem' }}>EF Core Integration</h4>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.6 }}>
                            BaseAsasDbContext applies global query filters and auto-assigns TenantId on SaveChanges.
                        </p>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem', textAlign: 'center' }}>
                        <div style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>🔌</div>
                        <h4 style={{ color: 'var(--primary)', marginBottom: '0.8rem', fontSize: '1.1rem' }}>Service Layer</h4>
                        <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.6 }}>
                            ICurrentTenant provides tenant context anywhere in your application via dependency injection.
                        </p>
                    </div>
                </div>

                <h2 id="tenant-features" style={{ fontSize: '2rem', marginTop: '4rem', marginBottom: '1.5rem', scrollMarginTop: '120px' }}>✨ Core Features</h2>

                <div id="tenant-resolution" style={{ marginBottom: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ color: 'var(--primary)', marginBottom: '1rem' }}>🌍 Multiple Resolution Strategies</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Asas supports multiple ways to identify tenants. You can use one or combine multiple strategies in a composite resolver.
                    </p>
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem', marginBottom: '1.5rem' }}>
                        <div className="glass-card" style={{ padding: '1.5rem' }}>
                            <h4 style={{ fontSize: '1rem', marginBottom: '0.8rem', color: 'var(--text-main)' }}>📌 Subdomain</h4>
                            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', lineHeight: 1.6 }}>
                                <code style={{ background: 'var(--code-bg)', padding: '0.2rem 0.5rem', borderRadius: '4px' }}>tenant1.example.com</code><br />Extracts tenant from the subdomain portion of the hostname.
                            </p>
                        </div>
                        <div className="glass-card" style={{ padding: '1.5rem' }}>
                            <h4 style={{ fontSize: '1rem', marginBottom: '0.8rem', color: 'var(--text-main)' }}>📋 Header</h4>
                            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', lineHeight: 1.6 }}>
                                <code style={{ background: 'var(--code-bg)', padding: '0.2rem 0.5rem', borderRadius: '4px' }}>X-Tenant-Id: tenant1</code><br />Reads tenant from a custom HTTP header.
                            </p>
                        </div>
                        <div className="glass-card" style={{ padding: '1.5rem' }}>
                            <h4 style={{ fontSize: '1rem', marginBottom: '0.8rem', color: 'var(--text-main)' }}>🔗 Route</h4>
                            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', lineHeight: 1.6 }}>
                                <code style={{ background: 'var(--code-bg)', padding: '0.2rem 0.5rem', borderRadius: '4px' }}>/tenant1/api/products</code><br />Extracts tenant from URL route parameter.
                            </p>
                        </div>
                        <div className="glass-card" style={{ padding: '1.5rem' }}>
                            <h4 style={{ fontSize: '1rem', marginBottom: '0.8rem', color: 'var(--text-main)' }}>🎫 Claims</h4>
                            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', lineHeight: 1.6 }}>
                                Reads tenant from JWT claims after authentication. Perfect for mobile apps.
                            </p>
                        </div>
                    </div>
                </div>

                <div id="tenant-ef" style={{ marginBottom: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ color: 'var(--primary)', marginBottom: '1rem' }}>🔐 Automatic EF Core Filtering</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Inherit from <code style={{ background: 'var(--code-bg)', padding: '0.2rem 0.5rem', borderRadius: '4px', color: 'var(--primary)' }}>BaseAsasDbContext</code> and tenancy "just works". All queries are automatically filtered by the current tenant.
                    </p>
                    <CodeBlock code={`public class ApplicationDbContext : BaseAsasDbContext<ApplicationDbContext>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, 
        ICurrentTenant tenant) 
        : base(options, tenant)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
}

// In your service - queries are AUTOMATICALLY filtered by tenant
public class ProductService
{
    private readonly ApplicationDbContext _db;
    
    public async Task<List<Product>> GetAllAsync()
    {
        // Only returns products for the CURRENT tenant
        // No need to add .Where(p => p.TenantId == ...)
        return await _db.Products.ToListAsync();
    }
}`} />
                </div>

                <div id="tenant-service" style={{ marginBottom: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ color: 'var(--primary)', marginBottom: '1rem' }}>🔧 ICurrentTenant Service</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Access the current tenant context anywhere in your application. Perfect for multi-tenant business logic.
                    </p>
                    <CodeBlock code={`public interface ICurrentTenant
{
    int? Id { get; }      // Current tenant ID (null if no tenant)
    bool IsSet { get; }   // True if tenant context exists
}

// Usage in your services
public class OrderService
{
    private readonly ICurrentTenant _tenant;
    private readonly IEmailService _email;
    
    public async Task ProcessOrderAsync(Order order)
    {
        if (!_tenant.IsSet)
            throw new Exception("Tenant context required");
            
        // Use tenant-specific configuration
        var config = await GetTenantConfig(_tenant.Id.Value);
        
        // Send tenant-branded email
        await _email.SendAsync(
            to: order.Email,
            subject: $"Order from {config.CompanyName}",
            template: config.EmailTemplate);
    }
}`} />
                </div>

                <h2 id="tenant-benefits" style={{ fontSize: '2rem', marginTop: '4rem', marginBottom: '1.5rem', scrollMarginTop: '120px' }}>💡 Why Choose Asas.Tenancy?</h2>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem', marginBottom: '3rem' }}>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>⚡ Zero Configuration</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Inherit from BaseAsasDbContext and you're done</li>
                            <li>Automatic query filtering - no manual Where clauses</li>
                            <li>TenantId auto-assigned on SaveChanges</li>
                            <li>Middleware handles tenant resolution</li>
                        </ul>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>🔒 Complete Isolation</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Database-level tenant filtering on all queries</li>
                            <li>Impossible to accidentally leak data between tenants</li>
                            <li>Global query filters applied automatically</li>
                            <li>Tenant context validated on every request</li>
                        </ul>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>🎯 Flexible Strategies</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Choose from 4+ resolution strategies</li>
                            <li>Subdomain for web apps (tenant.app.com)</li>
                            <li>Claims for mobile apps via JWT</li>
                            <li>Composite resolver for multiple strategies</li>
                        </ul>
                    </div>
                    <div className="glass-card glowing-border" style={{ padding: '2rem' }}>
                        <h4 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>🚀 Production Ready</h4>
                        <ul style={{ color: 'var(--text-muted)', lineHeight: 1.8, fontSize: '0.9rem', paddingLeft: '1.2rem' }}>
                            <li>Battle-tested in real SaaS applications</li>
                            <li>Integrates seamlessly with Identity & Permission modules</li>
                            <li>Support for single-tenant and multi-tenant modes</li>
                            <li>Clean architecture with minimal dependencies</li>
                        </ul>
                    </div>
                </div>

                <div id="tenant-quickstart" className="glass-card" style={{ padding: '2.5rem', background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.1) 0%, rgba(168, 85, 247, 0.1) 100%)', marginTop: '3rem', scrollMarginTop: '120px' }}>
                    <h3 style={{ marginBottom: '1rem' }}>🎓 Quick Start</h3>
                    <p style={{ color: 'var(--text-muted)', marginBottom: '1.5rem', lineHeight: 1.7 }}>
                        Just inherit from <code style={{ background: 'var(--code-bg)', padding: '0.2rem 0.5rem', borderRadius: '4px', color: 'var(--primary)' }}>BaseAsasDbContext</code> and inject <code style={{ background: 'var(--code-bg)', padding: '0.2rem 0.5rem', borderRadius: '4px', color: 'var(--primary)' }}>ICurrentTenant</code> - tenancy is automatic!
                    </p>
                    <CodeBlock code={`// 1. Your DbContext inherits from BaseAsasDbContext
public class AppDbContext : BaseAsasDbContext<AppDbContext>
{
    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenant tenant) 
        : base(options, tenant) { }
        
    public DbSet<Product> Products { get; set; }
}

// 2. Use ICurrentTenant in your services
public class ProductService
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;
    
    public ProductService(AppDbContext db, ICurrentTenant tenant)
    {
        _db = db;
        _tenant = tenant;
    }
    
    public async Task<List<Product>> GetProducts()
    {
        // Automatically filtered by current tenant!
        return await _db.Products.ToListAsync();
    }
    
    public async Task CreateProduct(Product product)
    {
        // TenantId automatically assigned on save!
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
    }
}`} />
                </div>
            </div>
        ),
        'CLI & Templates': (
            <div className="animate-fade-in">
                <h1 className="primary-gradient-text" style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>CLI & Templates</h1>
                <p style={{ color: 'var(--text-muted)', fontSize: '1.1rem', marginBottom: '2rem' }}>
                    Speed up your development with out-of-the-box templates.
                </p>
                <CodeBlock language="bash" code={`# Install the templates
dotnet new install Asas.Templates

# Create a new project
dotnet new asas-web -n MyCoolApp`} />
            </div>
        )
    };

    return (
        <div className="container" style={{ display: 'grid', gridTemplateColumns: '280px 1fr', gap: '5rem', padding: '6rem 2rem' }}>
            <aside style={{ position: 'sticky', top: '8rem', alignSelf: 'start' }}>
                <div style={{ position: 'relative', marginBottom: '2.5rem' }}>
                    <Search size={18} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
                    <input
                        placeholder="Search documentation..."
                        style={{
                            width: '100%',
                            background: 'var(--bg-dark)',
                            border: '1px solid var(--glass-border)',
                            borderRadius: '12px',
                            padding: '12px 12px 12px 42px',
                            color: 'var(--text-main)',
                            outline: 'none',
                            fontSize: '0.9rem'
                        }}
                    />
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.6rem' }}>
                    <span style={{ color: 'var(--text-muted)', fontSize: '0.75rem', fontWeight: 700, textTransform: 'uppercase', letterSpacing: '1px', marginBottom: '0.5rem', marginLeft: '0.5rem' }}>Documentation</span>
                    {navigationStructure.map(item => (
                        <SidebarItem
                            key={item.id}
                            title={item.title}
                            active={activeSection === item.id}
                            onClick={() => {
                                // Reset sub-item FIRST to prevent scroll-to-section
                                if (item.id !== 'Authentication') {
                                    setActiveSubItem(null);
                                }
                                // Instant scroll to top (no smooth to avoid visual jump)
                                window.scrollTo(0, 0);
                                // Then change the section
                                setActiveSection(item.id);
                            }}
                            subItems={item.subItems}
                            activeSubItem={activeSubItem}
                            onSubItemClick={scrollToSection}
                            isExpanded={activeSection === item.id}
                        />
                    ))}
                </div>
                <div style={{ marginTop: '4rem', padding: '1.5rem', borderRadius: '16px', background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.1) 0%, rgba(168, 85, 247, 0.1) 100%)', border: '1px solid var(--glass-border)' }}>
                    <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', marginBottom: '1rem' }}>Need help with implementation?</p>
                    <a href="#" style={{ color: 'var(--text-main)', textDecoration: 'none', fontSize: '0.9rem', fontWeight: 600, display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                        Join Discord <ExternalLink size={14} />
                    </a>
                </div>
            </aside>

            <motion.main
                key={activeSection}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4, ease: "easeOut" }}
                style={{ minHeight: '800px' }}
            >
                {sections[activeSection]}
            </motion.main>
        </div>
    );
};

export default Docs;
