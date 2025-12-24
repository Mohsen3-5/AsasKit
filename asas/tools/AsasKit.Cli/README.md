# AsasKit CLI (`asasctl`)

CLI tool for scaffolding and managing **AsasKit** projects.

## 🚀 Features

- **Template Management**: Install and update the `asas-kit` template directly via CLI.
- **Interactive Scaffolding**: Guided setup for your project, database, and Docker configuration.
- **Multi-DB Support**: Out-of-the-box support for PostgreSQL, SQL Server, and SQLite.
- **Auto-Config**: Automatically patches `.env`, `launchSettings.json`, and `appsettings.Development.json` with your database settings.
- **Ready to Run**: Handles initial EF Core migrations and database creation seamlessly.

## 🛠️ Installation

```powershell
dotnet tool install --global AsasKit.Cli
```

## 📖 Basic Usage

### Install the Template
Before scaffolding your first project, install the AsasKit starter template:
```powershell
asasctl template install
```

### Create a New Project
```powershell
asasctl new YourProjectName
```

## 🔗 Repository
[https://github.com/Mohsen3-5/AsasKit](https://github.com/Mohsen3-5/AsasKit)