# Developer Setup Guide

This guide will help you set up the Kaopiz Auth development environment on your local machine.

## 📋 Prerequisites

### Required Software
- **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** - Latest LTS version
- **[PostgreSQL 15+](https://www.postgresql.org/download/)** - Database server
- **[Docker](https://docs.docker.com/get-docker/)** - For containerization (optional)
- **[Git](https://git-scm.com/downloads)** - Version control

### Recommended Tools
- **[Visual Studio 2022](https://visualstudio.microsoft.com/vs/)** or **[VS Code](https://code.visualstudio.com/)**
- **[pgAdmin](https://www.pgadmin.org/)** - PostgreSQL management tool
- **[Postman](https://www.postman.com/)** or **[Thunder Client](https://www.thunderclient.com/)** - API testing

## 🚀 Quick Setup

### 1. Clone the Repository
```bash
git clone https://github.com/hoale240803/kaopiz.git
cd kaopiz
```

### 2. Setup Database
```bash
# Create PostgreSQL database
createdb kaopiz_auth_dev

# Or using psql
psql -U postgres -c "CREATE DATABASE kaopiz_auth_dev;"
```

### 3. Configure Environment
```bash
# Copy the example configuration
cp appsettings.Development.example.json src/KaopizAuth.WebAPI/appsettings.Development.json

# Edit the configuration file with your database connection string
```

### 4. Install Dependencies & Run
```bash
# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run database migrations
dotnet ef database update --project src/KaopizAuth.Infrastructure --startup-project src/KaopizAuth.WebAPI

# Start the application
dotnet run --project src/KaopizAuth.WebAPI
```

### 5. Verify Setup
- Navigate to `https://localhost:7001/swagger` to see the API documentation
- Check the health endpoint: `GET https://localhost:7001/health`

## 🔧 Detailed Setup

### Database Configuration

#### PostgreSQL Connection String
Update your `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=kaopiz_auth_dev;Username=your_username;Password=your_password"
  }
}
```

#### Using Docker for PostgreSQL
```bash
# Run PostgreSQL in Docker
docker run --name kaopiz-postgres \
  -e POSTGRES_PASSWORD=dev_password \
  -e POSTGRES_DB=kaopiz_auth_dev \
  -p 5432:5432 \
  -d postgres:15

# Connection string for Docker
"Host=localhost;Database=kaopiz_auth_dev;Username=postgres;Password=dev_password"
```

### JWT Configuration

Update your `appsettings.Development.json`:

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "KaopizAuth",
    "Audience": "KaopizAuthUsers",
    "ExpireMinutes": 15
  }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/KaopizAuth.Tests/
```

## 🔨 Development Workflow

### 1. Feature Development
```bash
# Create feature branch
git checkout -b feature/your-feature-name

# Make changes and commit
git add .
git commit -m "feat: add your feature description"

# Push and create PR
git push origin feature/your-feature-name
```

### 2. Database Migrations
```bash
# Add new migration
dotnet ef migrations add YourMigrationName --project src/KaopizAuth.Infrastructure --startup-project src/KaopizAuth.WebAPI

# Update database
dotnet ef database update --project src/KaopizAuth.Infrastructure --startup-project src/KaopizAuth.WebAPI

# Remove last migration (if needed)
dotnet ef migrations remove --project src/KaopizAuth.Infrastructure --startup-project src/KaopizAuth.WebAPI
```

### 3. Code Quality
```bash
# Format code
dotnet format

# Analyze code
dotnet analyze

# Run security scan
dotnet list package --vulnerable
```

## 🐛 Troubleshooting

### Common Issues

#### 1. Database Connection Failed
```bash
# Check PostgreSQL is running
sudo service postgresql status

# Test connection
psql -h localhost -U postgres -d kaopiz_auth_dev
```

#### 2. Migration Errors
```bash
# Drop and recreate database
dotnet ef database drop --project src/KaopizAuth.Infrastructure --startup-project src/KaopizAuth.WebAPI --force
dotnet ef database update --project src/KaopizAuth.Infrastructure --startup-project src/KaopizAuth.WebAPI
```

#### 3. Port Already in Use
```bash
# Find process using port 7001
lsof -i :7001

# Kill the process
kill -9 <PID>
```

### Performance Tips

1. **Use hot reload during development**:
   ```bash
   dotnet watch run --project src/KaopizAuth.WebAPI
   ```

2. **Skip tests during build for faster development**:
   ```bash
   dotnet build --no-restore /p:RunAnalyzersDuringBuild=false
   ```

3. **Use in-memory database for testing**:
   - Tests automatically use in-memory database
   - No setup required for running tests

## 📚 Next Steps

- Read the [API Documentation](../api/README.md)
- Explore the [Architecture Guide](../architecture/README.md)
- Check out [Contributing Guidelines](../development/contributing.md)
- Review [Coding Standards](../development/standards.md)

## 🆘 Getting Help

- **Documentation**: Check this guide and other docs
- **Issues**: Create an issue in the repository
- **Team Chat**: Contact the development team
- **Stack Overflow**: Tag questions with `kaopiz-auth`

---

**Need help?** Contact the development team or create an issue in the repository.