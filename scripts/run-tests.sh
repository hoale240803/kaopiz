#!/bin/bash

echo "🚀 Running KaopizAuth Comprehensive Testing Suite"
echo "================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Set error handling
set -e
trap 'print_error "Test execution failed!"' ERR

# Build the solution
print_status "Building solution..."
dotnet build KaopizAuth.sln --configuration Release --no-restore
print_success "Solution built successfully"

# Run Unit Tests
print_status "Running Unit Tests..."
dotnet test tests/KaopizAuth.UnitTests/KaopizAuth.UnitTests.csproj \
    --configuration Release \
    --no-build \
    --logger "console;verbosity=minimal" \
    --logger "trx;LogFileName=unit-tests.trx" \
    --results-directory ./TestResults \
    --collect:"XPlat Code Coverage"
print_success "Unit Tests completed"

# Run Architecture Tests
print_status "Running Architecture Tests..."
dotnet test tests/KaopizAuth.ArchitectureTests/KaopizAuth.ArchitectureTests.csproj \
    --configuration Release \
    --no-build \
    --logger "console;verbosity=minimal" \
    --logger "trx;LogFileName=architecture-tests.trx" \
    --results-directory ./TestResults
print_success "Architecture Tests completed"

# Run Integration Tests
print_status "Running Integration Tests..."
dotnet test tests/KaopizAuth.IntegrationTests/KaopizAuth.IntegrationTests.csproj \
    --configuration Release \
    --no-build \
    --logger "console;verbosity=minimal" \
    --logger "trx;LogFileName=integration-tests.trx" \
    --results-directory ./TestResults \
    --collect:"XPlat Code Coverage"
print_success "Integration Tests completed"

# Run Performance Tests
print_status "Running Performance Tests..."
dotnet test tests/KaopizAuth.PerformanceTests/KaopizAuth.PerformanceTests.csproj \
    --configuration Release \
    --no-build \
    --logger "console;verbosity=minimal" \
    --logger "trx;LogFileName=performance-tests.trx" \
    --results-directory ./TestResults
print_success "Performance Tests completed"

# Install Playwright browsers if E2E tests directory exists
if [ -d "tests/KaopizAuth.E2ETests" ]; then
    print_status "Installing Playwright browsers..."
    cd tests/KaopizAuth.E2ETests
    npm install
    npx playwright install
    
    # Start the application in background
    print_status "Starting application for E2E tests..."
    cd ../../
    dotnet run --project src/KaopizAuth.WebAPI --urls "https://localhost:5001" &
    APP_PID=$!
    
    # Wait for app to start
    sleep 10
    
    # Run E2E Tests
    print_status "Running E2E Tests..."
    cd tests/KaopizAuth.E2ETests
    npm test
    
    # Stop the application
    kill $APP_PID
    cd ../../
    
    print_success "E2E Tests completed"
fi

# Generate Code Coverage Report
print_status "Generating Code Coverage Report..."
dotnet tool install -g dotnet-reportgenerator-globaltool 2>/dev/null || true
reportgenerator \
    -reports:"TestResults/*/coverage.cobertura.xml" \
    -targetdir:"TestResults/CoverageReport" \
    -reporttypes:"Html;Cobertura;JsonSummary"
print_success "Code Coverage Report generated in TestResults/CoverageReport"

# Display summary
echo ""
echo "🎉 All tests completed successfully!"
echo "📊 Test Results:"
echo "  - Unit Tests: TestResults/unit-tests.trx"
echo "  - Architecture Tests: TestResults/architecture-tests.trx"
echo "  - Integration Tests: TestResults/integration-tests.trx"
echo "  - Performance Tests: TestResults/performance-tests.trx"
echo "  - Code Coverage: TestResults/CoverageReport/index.html"
echo ""

print_success "Testing suite execution completed! 🚀"