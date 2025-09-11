# 🚀 AsasKit

AsasKit is a modular, Domain-Driven-Design (DDD) application framework for building modern .NET solutions.  
It provides a plug-and-play foundation with core building blocks like Identity, Tenancy, Permissions, Messaging, and more.  
Think of it as your **starter kit for enterprise-grade applications**, with a clean architecture and NuGet-based modularity.

---

## ✨ Features

- 🧩 **Modular by Design** – Identity, Tenancy, Permissions, and more, all isolated as reusable modules.
- ⚡ **.NET 9 Ready** – Built with the latest .NET runtime and EF Core 9.
- 🛠 **Clean Architecture** – Separation of concerns between Domain, Application, Infrastructure, and API.
- 🔄 **MediatR Pipeline** – CQRS, domain events, and behaviors (Validation, Unit of Work, Queuing).
- 🗃 **EF Core Integration** – Per-module DbContexts and migrations.
- 🔐 **Identity & Auth** – Multi-tenant user management, JWT authentication, and refresh tokens.
- 🌍 **Multi-Tenancy** – Tenant-aware services, resolution middleware, and tenant-scoped entities.
- 🛡 **Permission System** – Fine-grained permission management via roles, policies, and claims.
- 📦 **NuGet Packaging** – Each module can be packed and reused across projects.
- 📊 **Extensible** – Add custom modules while keeping the host app lean.

---

## 📦 Modules

- **Identity**  
  Authentication & user management with refresh tokens.

- **Tenancy**  
  Multi-tenant awareness with middleware & tenant resolution.

- **Permission**  
  Role/permission management with EF Core persistence.

- **Messaging** *(coming soon)*  
  Event bus and integration for async workflows.

- **Core**  
  Shared utilities, abstractions, and base classes.

---

## ⚙️ Getting Started

1. **Clone the repo**  
   ```bash
   git clone https://github.com/your-org/AsasKit.git
   cd AsasKit


## 🤝 Contributing

- **Contributions are welcome!** 
Feel free to submit pull requests for bug fixes, improvements, or new modules.

## 📜 License

- **This project is licensed under the MIT License.** 
See the LICENSE
 file for more details.
