# Gym Management System (GMS)

A robust, enterprise-grade Gym Management System built with **Clean Architecture**, **ASP.NET Core 10 MVC**, and **Entity Framework Core**. This project is designed to manage trainees, trainers, receptionists, memberships, and payments with a focus on security, scalability, and background automation.

![Gym Management Banner](https://img.shields.io/badge/Status-Development-orange?style=for-the-badge)
![.NET Version](https://img.shields.io/badge/.NET-10.0-blue?style=for-the-badge&logo=dotnet)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-green?style=for-the-badge)

## 🚀 Key Features

- **🛡️ Granular Authorization**: Bitmask-based permission system for Receptionists, allowing precise control over actions like registration, payments, and membership management.
- **💳 Membership Management**: Flexible plans, automatic suspension of expired accounts, and membership freezing functionality.
- **⏰ Background Automation**: Integrated **Hangfire** for recurring tasks:
  - Daily membership expiry checks and notifications.
  - Automatic suspension of expired memberships.
  - Recalculation of end dates for frozen memberships.
  - Monthly financial report generation.
- **📊 Interactive Dashboard**: Real-time stats for active members, revenue, and expiring memberships.
- **📧 Automated Notifications**: SMTP-based email service for membership reminders and system alerts.
- **🔐 Security & Performance**:
  - ASP.NET Core Identity with custom transaction management.
  - Rate limiting on login and global endpoints.
  - Security headers and anti-forgery token validation.
  - Soft delete and automated auditing (EF Core Interceptors).
- **🏥 System Health**: Built-in health checks for SQL Server, EF Core, and Hangfire.
- **📝 Logging**: Structured logging with **Serilog** and integration with **Seq**.

## 🏗️ Architecture

The project follows the **Clean Architecture** principles to ensure separation of concerns and maintainability:

- **GymManagement.Domain**: Core entities, enums, interfaces, and domain exceptions.
- **GymManagement.Application**: Business logic, CQRS (Commands/Queries) using **MediatR**, DTOs, FluentValidation, and AutoMapper.
- **GymManagement.Infrastructure**: Data persistence (EF Core), Identity configuration, background jobs (Hangfire), and external services (Email).
- **GymManagement (Web)**: Presentation layer using ASP.NET Core MVC, ViewModels, Controllers, and Middleware.

## 🛠️ Tech Stack

- **Backend**: C# 13, .NET 10.0
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Background Jobs**: Hangfire
- **Mediator Pattern**: MediatR
- **Validation**: FluentValidation
- **Logging**: Serilog, Seq
- **Dev-Ops**: Docker, Docker Compose, Makefile

## 🚦 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (if running locally without Docker)

### Quick Start with Docker

1. **Setup Environment**:
   Copy `.env.example` to `.env` and configure your passwords.

   ```bash
   cp .env.example .env
   cp docker-compose.override.yml.example docker-compose.override.yml
   ```

2. **Launch the System**:

   ```bash
   make up
   ```

3. **Access the App**:
   - Web App: [http://localhost:8080](http://localhost:8080)
   - Hangfire Dashboard: [http://localhost:8080/hangfire](http://localhost:8080/hangfire)
   - Seq (Logs): [http://localhost:5341](http://localhost:5341)

### Development Setup (Local)

1. **Update Connection String**: Modify `appsettings.json` in the `GymManagement` project.
2. **Apply Migrations**:
   ```bash
   make migrate
   ```
3. **Run the Application**:
   ```bash
   dotnet run --project GymManagement
   ```

## 📁 Project Structure

```text
├── GymManagement/                # Presentation Layer (MVC)
├── GymManagement.Application/    # Business Logic & CQRS
├── GymManagement.Domain/         # Core Domain Entities
├── GymManagement.Infrastructure/ # Persistence & Services
├── .github/                      # CI/CD Workflows
├── docker-compose.yml            # Container Orchestration
└── Makefile                      # Development Shortcuts
```

## 📈 Monitoring & Health

- **Healthy Check**: `/health/ready`
- **Liveness Check**: `/health/live`
- **Telemetry**: Configured for OpenTelemetry (OTLP) and Seq.

## 📄 Documentation

For more detailed deployment instructions, see [DEPLOYMENT.md](./DEPLOYMENT.md).

---

_Created by [Abdellah Eltrach](https://github.com/abdellaheltrach)_
