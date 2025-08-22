@echo off
echo 🚀 Running KaopizAuth Comprehensive Testing Suite
echo =================================================

echo ℹ️  Building solution...
dotnet build KaopizAuth.sln --configuration Release --no-restore
if %ERRORLEVEL% neq 0 goto error
echo ✅ Solution built successfully

echo ℹ️  Running Unit Tests...
dotnet test tests/KaopizAuth.UnitTests/KaopizAuth.UnitTests.csproj ^
    --configuration Release ^
    --no-build ^
    --logger "console;verbosity=minimal" ^
    --logger "trx;LogFileName=unit-tests.trx" ^
    --results-directory ./TestResults ^
    --collect:"XPlat Code Coverage"
if %ERRORLEVEL% neq 0 goto error
echo ✅ Unit Tests completed

echo ℹ️  Running Architecture Tests...
dotnet test tests/KaopizAuth.ArchitectureTests/KaopizAuth.ArchitectureTests.csproj ^
    --configuration Release ^
    --no-build ^
    --logger "console;verbosity=minimal" ^
    --logger "trx;LogFileName=architecture-tests.trx" ^
    --results-directory ./TestResults
if %ERRORLEVEL% neq 0 goto error
echo ✅ Architecture Tests completed

echo ℹ️  Running Integration Tests...
dotnet test tests/KaopizAuth.IntegrationTests/KaopizAuth.IntegrationTests.csproj ^
    --configuration Release ^
    --no-build ^
    --logger "console;verbosity=minimal" ^
    --logger "trx;LogFileName=integration-tests.trx" ^
    --results-directory ./TestResults ^
    --collect:"XPlat Code Coverage"
if %ERRORLEVEL% neq 0 goto error
echo ✅ Integration Tests completed

echo ℹ️  Running Performance Tests...
dotnet test tests/KaopizAuth.PerformanceTests/KaopizAuth.PerformanceTests.csproj ^
    --configuration Release ^
    --no-build ^
    --logger "console;verbosity=minimal" ^
    --logger "trx;LogFileName=performance-tests.trx" ^
    --results-directory ./TestResults
if %ERRORLEVEL% neq 0 goto error
echo ✅ Performance Tests completed

if exist "tests/KaopizAuth.E2ETests" (
    echo ℹ️  Installing Playwright browsers...
    cd tests/KaopizAuth.E2ETests
    call npm install
    call npx playwright install
    
    echo ℹ️  Starting application for E2E tests...
    cd ../../
    start /b dotnet run --project src/KaopizAuth.WebAPI --urls "https://localhost:5001"
    
    timeout /t 10 /nobreak > nul
    
    echo ℹ️  Running E2E Tests...
    cd tests/KaopizAuth.E2ETests
    call npm test
    
    cd ../../
    taskkill /f /im dotnet.exe > nul 2>&1
    
    echo ✅ E2E Tests completed
)

echo ℹ️  Generating Code Coverage Report...
dotnet tool install -g dotnet-reportgenerator-globaltool > nul 2>&1
reportgenerator ^
    -reports:"TestResults/*/coverage.cobertura.xml" ^
    -targetdir:"TestResults/CoverageReport" ^
    -reporttypes:"Html;Cobertura;JsonSummary"
echo ✅ Code Coverage Report generated

echo.
echo 🎉 All tests completed successfully!
echo 📊 Test Results:
echo   - Unit Tests: TestResults/unit-tests.trx
echo   - Architecture Tests: TestResults/architecture-tests.trx
echo   - Integration Tests: TestResults/integration-tests.trx
echo   - Performance Tests: TestResults/performance-tests.trx
echo   - Code Coverage: TestResults/CoverageReport/index.html
echo.
echo ✅ Testing suite execution completed! 🚀
goto end

:error
echo ❌ Test execution failed!
exit /b 1

:end