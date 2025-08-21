# KaopizAuth - Tech Stack & Architecture Guide

## 🏗️ Architecture Overview

### Clean Architecture + CQRS Pattern

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│  ┌─────────────────┐    ┌─────────────────────────────┐ │
│  │   React Web     │    │     ASP.NET Core API       │ │
│  │   TypeScript    │    │     Controllers            │ │
│  └─────────────────┘    └─────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐ │
│  │   Commands  │  │   Queries   │  │   Handlers      │ │
│  │   (CQRS)    │  │   (CQRS)    │  │   (MediatR)     │ │
│  └─────────────┘  └─────────────┘  └─────────────────┘ │
│  ┌─────────────────────────────────────────────────────┐ │
│  │           Interfaces & DTOs                         │ │
│  └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐ │
│  │ Entity      │  │   Redis     │  │   External      │ │
│  │ Framework   │  │   Cache     │  │   Services      │ │
│  └─────────────┘  └─────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────┐
│                     Domain Layer                        │
│  ┌─────────────────────────────────────────────────────┐ │
│  │     Entities, Value Objects, Domain Services       │ │
│  │              Business Rules                         │ │
│  └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

## 🛠️ Technology Stack

### Backend (.NET)

| Technology                | Version | Purpose                        | Documentation                                                                                  |
| ------------------------- | ------- | ------------------------------ | ---------------------------------------------------------------------------------------------- |
| **.NET**                  | 8.0 LTS | Runtime Framework              | [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/)                                     |
| **ASP.NET Core**          | 8.0     | Web API Framework              | [ASP.NET Core Docs](https://docs.microsoft.com/en-us/aspnet/core/)                             |
| **Entity Framework Core** | 8.0     | ORM                            | [EF Core Docs](https://docs.microsoft.com/en-us/ef/core/)                                      |
| **ASP.NET Core Identity** | 8.0     | Authentication & Authorization | [Identity Docs](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity) |
| **MediatR**               | 12.x    | CQRS Implementation            | [MediatR GitHub](https://github.com/jbogard/MediatR)                                           |
| **FluentValidation**      | 11.x    | Input Validation               | [FluentValidation Docs](https://docs.fluentvalidation.net/)                                    |
| **AutoMapper**            | 12.x    | Object Mapping                 | [AutoMapper Docs](https://docs.automapper.org/)                                                |
| **JWT Bearer**            | 8.0     | JWT Token Authentication       | [JWT Docs](https://jwt.io/)                                                                    |
| **Serilog**               | 3.x     | Structured Logging             | [Serilog Docs](https://serilog.net/)                                                           |
| **Redis**                 | 7.x     | Caching & Session Store        | [Redis Docs](https://redis.io/docs/)                                                           |
| **Swagger/OpenAPI**       | 6.x     | API Documentation              | [Swashbuckle Docs](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)                  |

### Frontend (React)

| Technology                | Version | Purpose                  | Documentation                                                                     |
| ------------------------- | ------- | ------------------------ | --------------------------------------------------------------------------------- |
| **React**                 | 18.x    | UI Framework             | [React Docs](https://react.dev/)                                                  |
| **TypeScript**            | 5.x     | Type Safety              | [TypeScript Docs](https://www.typescriptlang.org/)                                |
| **Vite**                  | 5.x     | Build Tool               | [Vite Docs](https://vitejs.dev/)                                                  |
| **React DOM**             | 18.x    | React DOM Rendering      | [React DOM Docs](https://react.dev/reference/react-dom)                           |
| **Native Fetch API**      | -       | HTTP Requests            | [MDN Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API)       |
| **CSS Modules**           | -       | Component-scoped Styling | [CSS Modules Docs](https://github.com/css-modules/css-modules)                    |
| **CSS Custom Properties** | -       | CSS Variables            | [MDN CSS Custom Properties](https://developer.mozilla.org/en-US/docs/Web/CSS/--*) |

### Database

| Technology     | Version | Purpose              | Documentation                                                       |
| -------------- | ------- | -------------------- | ------------------------------------------------------------------- |
| **PostgreSQL** | 15.x    | Primary Database     | [PostgreSQL Docs](https://www.postgresql.org/docs/)                 |
| **SQL Server** | 2022    | Alternative Database | [SQL Server Docs](https://docs.microsoft.com/en-us/sql/sql-server/) |

### DevOps & Tools

| Technology         | Version | Purpose                    | Documentation                                             |
| ------------------ | ------- | -------------------------- | --------------------------------------------------------- |
| **Docker**         | 24.x    | Containerization           | [Docker Docs](https://docs.docker.com/)                   |
| **Docker Compose** | 2.x     | Multi-container Management | [Compose Docs](https://docs.docker.com/compose/)          |
| **GitHub Actions** | -       | CI/CD Pipeline             | [GitHub Actions Docs](https://docs.github.com/en/actions) |
| **SonarCloud**     | -       | Code Quality Analysis      | [SonarCloud Docs](https://sonarcloud.io/documentation)    |

### Testing

| Technology                | Version | Purpose                 | Documentation                                                                         |
| ------------------------- | ------- | ----------------------- | ------------------------------------------------------------------------------------- |
| **xUnit**                 | 2.x     | Unit Testing Framework  | [xUnit Docs](https://xunit.net/)                                                      |
| **FluentAssertions**      | 6.x     | Assertion Library       | [FluentAssertions Docs](https://fluentassertions.com/)                                |
| **Testcontainers**        | 3.x     | Integration Testing     | [Testcontainers Docs](https://dotnet.testcontainers.org/)                             |
| **Bogus**                 | 34.x    | Test Data Generation    | [Bogus Docs](https://github.com/bchavez/Bogus)                                        |
| **WebApplicationFactory** | 8.0     | API Integration Testing | [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests) |
| **Playwright**            | 1.x     | End-to-End Testing      | [Playwright Docs](https://playwright.dev/)                                            |

## 🏛️ Design Patterns & Principles

### Architecture Patterns

- **Clean Architecture**: Dependency inversion, separation of concerns
- **CQRS (Command Query Responsibility Segregation)**: Separate read and write operations
- **Repository Pattern**: Data access abstraction
- **Unit of Work Pattern**: Transaction management

### Design Patterns

- **Strategy Pattern**: User type authentication strategies
- **Factory Pattern**: Authentication provider creation
- **Decorator Pattern**: Middleware pipeline
- **Observer Pattern**: Event-driven notifications
- **Builder Pattern**: Complex object construction
- **Specification Pattern**: Business rule encapsulation

### Frontend Architecture & Approach

#### Pure React Implementation Strategy

- **No External Form Libraries**: Custom form handling with React hooks (useState, useReducer)
- **No Component Libraries**: Build all UI components from scratch using semantic HTML + CSS
- **No State Management Libraries**: Use React Context API + useReducer for global state
- **No HTTP Libraries**: Use native Fetch API with custom hooks
- **Vanilla CSS with CSS Modules**: Component-scoped styling without external CSS frameworks

#### Custom Components Implementation

```typescript
// Custom Form Hook
interface UseFormOptions<T> {
  initialValues: T;
  validate?: (values: T) => Record<string, string>;
  onSubmit: (values: T) => void | Promise<void>;
}

export function useForm<T>({
  initialValues,
  validate,
  onSubmit,
}: UseFormOptions<T>) {
  const [values, setValues] = useState<T>(initialValues);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Custom form logic implementation
  return { values, errors, isSubmitting, handleChange, handleSubmit };
}

// Custom Button Component
interface ButtonProps {
  type?: "button" | "submit" | "reset";
  variant?: "primary" | "secondary" | "danger";
  size?: "small" | "medium" | "large";
  disabled?: boolean;
  loading?: boolean;
  children: React.ReactNode;
  onClick?: () => void;
}

export const Button: React.FC<ButtonProps> = ({
  type = "button",
  variant = "primary",
  // ... other props
}) => {
  return (
    <button
      type={type}
      className={`button button--${variant} button--${size}`}
      disabled={disabled || loading}
      onClick={onClick}
    >
      {loading ? <LoadingSpinner /> : children}
    </button>
  );
};
```

### SOLID Principles

- **S**ingle Responsibility Principle
- **O**pen/Closed Principle
- **L**iskov Substitution Principle
- **I**nterface Segregation Principle
- **D**ependency Inversion Principle

### DDD (Domain Driven Design)

- **Entities**: Core business objects with identity
- **Value Objects**: Immutable objects without identity
- **Aggregates**: Consistency boundaries
- **Domain Services**: Business logic that doesn't fit in entities
- **Repository Interfaces**: Data access contracts

## 📁 Project Structure

```
KaopizAuth/
├── src/
│   ├── KaopizAuth.Domain/                 # Core business logic
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   └── Events/
│   │
│   ├── KaopizAuth.Application/            # Use cases and business rules
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   ├── Models/
│   │   │   └── Extensions/
│   │   ├── Commands/
│   │   │   ├── Auth/
│   │   │   └── Users/
│   │   ├── Queries/
│   │   │   ├── Auth/
│   │   │   └── Users/
│   │   ├── Handlers/
│   │   ├── Validators/
│   │   ├── Mappings/
│   │   └── Services/
│   │
│   ├── KaopizAuth.Infrastructure/         # External concerns
│   │   ├── Data/
│   │   │   ├── Configurations/
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   ├── Services/
│   │   │   ├── Authentication/
│   │   │   ├── Email/
│   │   │   └── Caching/
│   │   ├── External/
│   │   └── Extensions/
│   │
│   ├── KaopizAuth.WebAPI/                # API presentation layer
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Filters/
│   │   ├── Extensions/
│   │   ├── Models/
│   │   │   ├── Requests/
│   │   │   └── Responses/
│   │   └── Configuration/
│   │
│   └── KaopizAuth.WebApp/                # React frontend
│       ├── public/
│       ├── src/
│       │   ├── components/
│       │   │   ├── common/
│       │   │   ├── auth/
│       │   │   └── layout/
│       │   ├── pages/
│       │   ├── hooks/
│       │   ├── services/
│       │   ├── store/
│       │   ├── types/
│       │   ├── utils/
│       │   └── styles/
│       ├── package.json
│       └── vite.config.ts
│
├── tests/
│   ├── KaopizAuth.UnitTests/
│   ├── KaopizAuth.IntegrationTests/
│   ├── KaopizAuth.ArchitectureTests/
│   └── KaopizAuth.E2ETests/
│
├── docker/
│   ├── Dockerfile.api
│   ├── Dockerfile.webapp
│   └── docker-compose.yml
│
├── docs/
│   ├── api/
│   ├── deployment/
│   └── architecture/
│
├── scripts/
│   ├── build.sh
│   ├── test.sh
│   └── deploy.sh
│
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── cd.yml
│
├── KaopizAuth.sln
├── Directory.Build.props
├── global.json
├── .gitignore
├── README.md
└── docker-compose.yml
```

## 🚀 Quick Start Guide

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [PostgreSQL 15+](https://www.postgresql.org/download/) (or use Docker)
- [Redis 7+](https://redis.io/download/) (or use Docker)
- [Git](https://git-scm.com/)

### 1. Clone Repository

```bash
git clone https://github.com/your-username/kaopizauth.git
cd kaopizauth
```

### 2. Setup Development Environment

#### Option A: Using Docker (Recommended)

```bash
# Start all services (PostgreSQL, Redis, API, Web)
docker-compose up -d

# View logs
docker-compose logs -f
```

#### Option B: Local Development Setup

##### Backend Setup

```bash
# Navigate to API project
cd src/KaopizAuth.WebAPI

# Restore packages
dotnet restore

# Update connection string in appsettings.Development.json
# Create and run migrations
dotnet ef database update

# Run API
dotnet run
```

##### Frontend Setup

```bash
# Navigate to React app
cd src/KaopizAuth.WebApp

# Install dependencies (minimal - only official React packages)
npm install

# Start development server
npm run dev
```

### 3. Configuration Files

#### Backend Configuration (`appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=KaopizAuth;Username=postgres;Password=password",
    "RedisConnection": "localhost:6379"
  },
  "JWT": {
    "SecretKey": "your-super-secret-key-here-min-32-chars",
    "Issuer": "KaopizAuth",
    "Audience": "KaopizAuth",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

#### Frontend Environment (`.env.development`)

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_APP_NAME=KaopizAuth
VITE_APP_VERSION=1.0.0
```

#### Frontend Package.json (Minimal Dependencies)

````json
{
  "name": "kaopizauth-webapp",
  "private": true,
  "version": "1.0.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview",
    "type-check": "tsc --noEmit",
    "lint": "eslint src --ext ts,tsx --report-unused-disable-directives --max-warnings 0"
  },
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0"
  },
  "devDependencies": {
    "@types/react": "^18.2.15",
    "@types/react-dom": "^18.2.7",
    "@typescript-eslint/eslint-plugin": "^6.0.0",
    "@typescript-eslint/parser": "^6.0.0",
    "@vitejs/plugin-react": "^4.0.3",
    "eslint": "^8.45.0",
    "eslint-plugin-react-hooks": "^4.6.0",
    "eslint-plugin-react-refresh": "^0.4.3",
    "typescript": "^5.0.2",
    "vite": "^4.4.5"
  }
}

### 4. Database Setup

#### Create Database & Run Migrations
```bash
# From KaopizAuth.WebAPI directory
dotnet ef migrations add InitialCreate
dotnet ef database update
````

#### Seed Test Data

```bash
dotnet run --seed-data
```

### 5. Running Tests

#### Unit Tests

```bash
dotnet test tests/KaopizAuth.UnitTests/
```

#### Integration Tests

```bash
dotnet test tests/KaopizAuth.IntegrationTests/
```

#### Frontend Tests

```bash
cd src/KaopizAuth.WebApp
npm test
```

#### E2E Tests

```bash
cd tests/KaopizAuth.E2ETests
npx playwright test
```

### 6. Access Applications

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **React App**: http://localhost:3000
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379

### 7. Development Workflow

#### Creating New Feature Branch

```bash
git checkout develop
git pull origin develop
git checkout -b feature/ticket-123-description
```

#### Running Code Quality Checks

```bash
# Format code
dotnet format

# Run security scan
dotnet list package --vulnerable

# Run code analysis
dotnet build --verbosity normal
```

#### Before Committing

```bash
# Run all tests
./scripts/test.sh

# Check code coverage
dotnet test --collect:"XPlat Code Coverage"

# Lint frontend
cd src/KaopizAuth.WebApp
npm run lint
npm run type-check
```

## 🔧 Development Tools & Extensions

### VS Code Extensions

- C# Dev Kit
- REST Client
- Thunder Client
- GitLens
- Docker
- TypeScript Importer
- CSS Modules
- Prettier
- ESLint

### Recommended Tools

- **Postman**: API testing
- **pgAdmin**: PostgreSQL management
- **Redis Desktop Manager**: Redis management
- **Docker Desktop**: Container management
- **Git Kraken**: Git GUI (optional)

## 📊 Monitoring & Observability

### Health Checks

- Database connectivity: `/health/database`
- Redis connectivity: `/health/redis`
- Overall health: `/health`

### Metrics Endpoints

- Application metrics: `/metrics`
- Prometheus format: `/metrics/prometheus`

### Logging

- Structured logging with Serilog
- Log levels: Information, Warning, Error, Critical
- Correlation IDs for request tracking

## 🔒 Security Considerations

### Authentication & Authorization

- JWT with RS256 signing
- Refresh token rotation
- Role-based access control (RBAC)
- Claims-based authorization

### Security Headers

- HSTS (HTTP Strict Transport Security)
- X-Frame-Options
- X-Content-Type-Options
- X-XSS-Protection
- Content Security Policy (CSP)

### Data Protection

- Password hashing with BCrypt
- Sensitive data encryption
- GDPR compliance ready
- Audit logging

## 🚀 Deployment

### Production Environment Variables

```env
ASPNETCORE_ENVIRONMENT=Production
CONNECTION_STRING=your-production-db-connection
REDIS_CONNECTION=your-production-redis-connection
JWT_SECRET_KEY=your-production-secret-key
SERILOG_SEQ_URL=your-seq-logging-url
```

### Docker Production Build

```bash
docker build -t kaopizauth-api -f docker/Dockerfile.api .
docker build -t kaopizauth-webapp -f docker/Dockerfile.webapp .
```

### CI/CD Pipeline

- **Build**: Restore, build, test
- **Quality Gate**: Code coverage > 80%
- **Security Scan**: Dependency vulnerabilities
- **Deploy**: Docker containers to production

---

**Happy Coding! 🎉**

For questions or issues, please check the documentation in the `/docs` folder or create an issue in the repository.
