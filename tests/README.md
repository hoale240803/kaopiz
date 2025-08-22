# KaopizAuth Comprehensive Testing Suite

This document provides a complete guide to the comprehensive testing suite implemented for the KaopizAuth application following Clean Architecture principles.

## 🎯 Testing Strategy

The testing suite consists of multiple layers of testing to ensure code quality, architectural compliance, and performance:

### 1. **Unit Tests** (`KaopizAuth.UnitTests`)
- **Purpose**: Test individual components in isolation
- **Scope**: Application layer handlers, validators, and services
- **Tools**: xUnit, FluentAssertions, Moq, Bogus
- **Location**: `tests/KaopizAuth.UnitTests/`

### 2. **Integration Tests** (`KaopizAuth.IntegrationTests`)
- **Purpose**: Test component interactions and API endpoints
- **Scope**: Full API testing with real dependencies
- **Tools**: WebApplicationFactory, Testcontainers, PostgreSQL, Redis
- **Location**: `tests/KaopizAuth.IntegrationTests/`

### 3. **Architecture Tests** (`KaopizAuth.ArchitectureTests`)
- **Purpose**: Validate Clean Architecture principles and constraints
- **Scope**: Dependency rules, naming conventions, architectural patterns
- **Tools**: NetArchTest.Rules
- **Location**: `tests/KaopizAuth.ArchitectureTests/`

### 4. **E2E Tests** (`KaopizAuth.E2ETests`)
- **Purpose**: Test complete user workflows and API interactions
- **Scope**: Browser automation and API testing
- **Tools**: Playwright, TypeScript
- **Location**: `tests/KaopizAuth.E2ETests/`

### 5. **Performance Tests** (`KaopizAuth.PerformanceTests`)
- **Purpose**: Validate response times and load handling
- **Scope**: API endpoint performance under load
- **Tools**: NBomber, concurrent testing
- **Location**: `tests/KaopizAuth.PerformanceTests/`

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+ (for E2E tests)
- Docker (for Integration tests with Testcontainers)

### Running All Tests

**Windows:**
```bash
./scripts/run-tests.bat
```

**Linux/macOS:**
```bash
./scripts/run-tests.sh
```

### Running Individual Test Suites

**Unit Tests:**
```bash
dotnet test tests/KaopizAuth.UnitTests/
```

**Integration Tests:**
```bash
dotnet test tests/KaopizAuth.IntegrationTests/
```

**Architecture Tests:**
```bash
dotnet test tests/KaopizAuth.ArchitectureTests/
```

**Performance Tests:**
```bash
dotnet test tests/KaopizAuth.PerformanceTests/
```

**E2E Tests:**
```bash
cd tests/KaopizAuth.E2ETests
npm install
npx playwright install
npm test
```

## 📊 Test Data Generation

The testing suite uses **Bogus** for generating realistic test data:

### Test Data Builders

**User Data:**
```csharp
var user = TestDataBuilders.Users.Valid();
var verifiedUser = TestDataBuilders.Users.Verified();
var userWithEmail = TestDataBuilders.Users.WithEmail("test@example.com");
```

**Command Data:**
```csharp
var registerCommand = TestDataBuilders.Commands.ValidRegister();
var loginCommand = TestDataBuilders.Commands.ValidLogin();
```

## 🏗️ Architecture Validation

The Architecture Tests enforce Clean Architecture principles:

### Dependency Rules
- ✅ Domain layer has no dependencies on other layers
- ✅ Application layer doesn't depend on Infrastructure or WebAPI
- ✅ Infrastructure layer doesn't depend on WebAPI
- ✅ Controllers only depend on Application and Domain

### Design Patterns
- ✅ Entities are sealed
- ✅ Value Objects are sealed
- ✅ Handlers are sealed and follow naming conventions
- ✅ Repository interfaces are in Domain layer
- ✅ Domain services are interfaces

## 🔄 Integration Testing

### Database Testing
- Uses **Testcontainers** for PostgreSQL instances
- Each test gets a fresh database
- Real database interactions without mocking

### Cache Testing
- Redis integration via Testcontainers
- Tests caching behavior and invalidation

### API Testing
- Full HTTP request/response testing
- Authentication flow validation
- Error scenario testing

## ⚡ Performance Requirements

All API endpoints must meet performance requirements:

- **Response Time**: < 200ms for individual requests
- **Concurrent Load**: Handle 50+ concurrent requests
- **Load Testing**: Sustained 10-15 requests/second

### Load Testing Scenarios

**Registration Endpoint:**
- 10 requests/second for 30 seconds
- Expected: 0% failure rate

**Login Endpoint:**
- 15 requests/second for 30 seconds
- Expected: 0% failure rate

## 🎭 E2E Testing

### UI Flow Testing
- User registration flow
- Login/logout functionality
- Form validation
- Error handling

### API Testing
- Direct API endpoint testing
- Token refresh workflows
- Error response validation

### Cross-Browser Testing
- Chrome, Firefox, Safari, Edge
- Mobile browsers (iOS/Android)

## 📈 Code Coverage

Target code coverage: **80%+**

### Coverage Report Generation
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"CoverageReport"
```

### Coverage Exclusions
- Generated code
- Program.cs and Startup configurations
- Third-party integrations

## 🔧 CI/CD Integration

### GitHub Actions Workflow

The comprehensive testing suite runs on:
- ✅ Every pull request
- ✅ Push to main/develop branches
- ✅ Scheduled runs (optional)

### Pipeline Jobs
1. **Unit & Architecture Tests** - Fast feedback
2. **Integration Tests** - With database services
3. **Performance Tests** - Load validation
4. **E2E Tests** - Full user workflow validation

### Artifacts
- Test results (TRX format)
- Code coverage reports
- Playwright test recordings
- Performance test reports

## 📝 Writing Tests

### Unit Test Example
```csharp
[Fact]
public async Task Handle_ValidRegisterCommand_ShouldCreateUser()
{
    // Arrange
    var command = TestDataBuilders.Commands.ValidRegister();
    var handler = new RegisterCommandHandler(_userRepository, _passwordService);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue();
}
```

### Integration Test Example
```csharp
[Fact]
public async Task Register_WithValidData_ShouldReturnSuccess()
{
    // Arrange
    var request = TestDataBuilders.DTOs.ValidRegisterRequest();

    // Act
    var response = await _client.PostAsJsonAsync("/api/auth/register", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### Architecture Test Example
```csharp
[Fact]
public void Domain_Should_Not_HaveDependencyOnOtherProjects()
{
    var result = Types.InAssembly(typeof(User).Assembly)
        .Should()
        .NotHaveDependencyOn("KaopizAuth.Application")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

## 🛠️ Test Configuration

### Test Settings
- **Timeout**: 30 seconds per test
- **Parallel Execution**: Enabled for unit tests
- **Retry Policy**: 2 retries in CI environment
- **Test Data Isolation**: Each test gets fresh data

### Environment Variables
```bash
ASPNETCORE_ENVIRONMENT=Testing
ConnectionStrings__DefaultConnection=<test-db-connection>
ConnectionStrings__Redis=<test-redis-connection>
```

## 📚 Best Practices

### Test Organization
- ✅ One test class per system under test
- ✅ Descriptive test method names
- ✅ Arrange-Act-Assert pattern
- ✅ Test data builders for complex objects

### Test Isolation
- ✅ No shared state between tests
- ✅ Fresh database per integration test
- ✅ Proper cleanup in test fixtures

### Performance Testing
- ✅ Realistic load scenarios
- ✅ Performance budgets defined
- ✅ Resource monitoring during tests

### Maintenance
- ✅ Regular test review and cleanup
- ✅ Update test data builders with domain changes
- ✅ Monitor test execution times
- ✅ Keep dependencies up to date

## 🎉 Benefits

This comprehensive testing suite provides:

1. **High Confidence**: Multiple testing layers catch different types of issues
2. **Fast Feedback**: Unit tests run in seconds
3. **Realistic Testing**: Integration tests use real dependencies
4. **Architecture Compliance**: Automated validation of design principles
5. **Performance Assurance**: Load testing validates scalability
6. **User Experience**: E2E tests validate actual user workflows
7. **Maintainability**: Test data builders simplify test creation
8. **CI/CD Ready**: Automated testing in pipelines

## 📞 Support

For questions about the testing suite:
- Review test examples in each project
- Check CI/CD workflow files for configuration
- Refer to individual test documentation
- Follow established patterns when adding new tests