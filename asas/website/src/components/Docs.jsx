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

const SidebarItem = ({ title, active, onClick }) => (
    <motion.div
        onClick={onClick}
        whileHover={{ x: 5 }}
        style={{
            padding: '0.8rem 1.2rem',
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
            fontSize: '0.95rem',
            boxShadow: active ? '0 4px 12px rgba(99, 102, 241, 0.1)' : 'none'
        }}
    >
        {title}
        <ChevronRight size={16} style={{
            opacity: active ? 1 : 0,
            transform: active ? 'translateX(0)' : 'translateX(-10px)',
            transition: 'all 0.4s var(--ease-premium)',
            color: 'inherit'
        }} />
    </motion.div>
);

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
                <h1 className="primary-gradient-text" style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>Asas.Identity</h1>
                <p style={{ color: 'var(--text-muted)', fontSize: '1.1rem', marginBottom: '2rem' }}>
                    The Identity module handles user lifecycle, authentication, and device management.
                </p>
                <h2>IAuthService Interface</h2>
                <CodeBlock code={`public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<ExternalAuthResult> ExternalAuthAsync(ExternalAuthRequest request);
    Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(ChangePasswordRequest request);
    Task LogoutAsync(LogoutRequest r);
    Task RegisterDeviceAsync(RegisterDeviceRequest r);
}`} />
                <h3 style={{ marginTop: '2rem' }}>Authentication Result</h3>
                <CodeBlock code={`public sealed record AuthResult(
    string Token, 
    string RefreshToken, 
    DateTime ExpiresAtUtc, 
    bool EmailConfirmed
);`} />
            </div>
        ),
        'Authorization': (
            <div className="animate-fade-in">
                <h1 className="primary-gradient-text" style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>Asas.Permission</h1>
                <p style={{ color: 'var(--text-muted)', fontSize: '1.1rem', marginBottom: '2rem' }}>
                    Granular permission management that integrates deeply with ASP.NET Core Authorization policies.
                </p>
                <h2>The [RequiresPermission] Attribute</h2>
                <p style={{ color: 'var(--text-muted)', margin: '1rem 0' }}>
                    Use the attribute at the class or method level to secure your API endpoints.
                </p>
                <CodeBlock code={`[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    [RequiresPermission("Permissions.Users.Create")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        return Ok(await _userService.CreateAsync(dto));
    }
}`} />
            </div>
        ),
        'Multi-Tenancy': (
            <div className="animate-fade-in">
                <h1 className="primary-gradient-text" style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>Asas.Tenancy</h1>
                <p style={{ color: 'var(--text-muted)', fontSize: '1.1rem', marginBottom: '2rem' }}>
                    Build SaaS applications with first-class multi-tenancy support.
                </p>
                <h2>Accessing Current Tenant</h2>
                <CodeBlock code={`public interface ICurrentTenant
{
    int? Id { get; }
    bool IsSet { get; }
}`} />
                <h3 style={{ marginTop: '2rem' }}>Usage Example</h3>
                <CodeBlock code={`public class ProductService
{
    private readonly ICurrentTenant _currentTenant;
    
    public ProductService(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public void Process()
    {
        var tenantId = _currentTenant.Id;
        // Business logic filtered by tenantId
    }
}`} />
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
                    {Object.keys(sections).map(section => (
                        <SidebarItem
                            key={section}
                            title={section}
                            active={activeSection === section}
                            onClick={() => setActiveSection(section)}
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
