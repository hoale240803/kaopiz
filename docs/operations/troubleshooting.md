# Troubleshooting Guide

This guide helps diagnose and resolve common issues with the Kaopiz Auth system.

## 🚨 Common Issues & Solutions

### 1. Authentication Issues

#### Problem: "Invalid or expired token" errors
**Symptoms:**
- 401 Unauthorized responses
- Users being logged out unexpectedly
- Token validation failures

**Possible Causes & Solutions:**

**JWT Secret Key Mismatch**
```bash
# Check if JWT keys match across instances
kubectl get secret kaopiz-jwt-secret -o jsonpath='{.data.key}' | base64 -d

# Verify in logs
kubectl logs -f deployment/kaopiz-auth-api | grep "JWT"
```

**Clock Synchronization Issues**
```bash
# Check system time on all nodes
date
ntpdate -q pool.ntp.org

# Sync time if needed
sudo ntpdate -s pool.ntp.org
```

**Token Expiration**
```bash
# Check JWT token details
echo "YOUR_JWT_TOKEN" | cut -d. -f2 | base64 -d | jq '.'

# Verify expiration time
echo "YOUR_JWT_TOKEN" | cut -d. -f2 | base64 -d | jq '.exp' | xargs -I {} date -d @{}
```

#### Problem: Login attempts failing with correct credentials
**Symptoms:**
- Valid credentials rejected
- "Invalid email or password" errors
- Account lockout issues

**Diagnostic Steps:**
```sql
-- Check user account status
SELECT Id, Email, IsActive, LockoutEnd, AccessFailedCount 
FROM AspNetUsers 
WHERE NormalizedEmail = UPPER('user@example.com');

-- Check recent login attempts (if audit logging is enabled)
SELECT * FROM AuditLogs 
WHERE UserId = 'user-id' 
AND Action LIKE '%Login%' 
ORDER BY CreatedAt DESC 
LIMIT 10;
```

**Solutions:**
```csharp
// Unlock user account (admin operation)
var user = await _userManager.FindByEmailAsync(email);
if (user != null)
{
    await _userManager.SetLockoutEndDateAsync(user, null);
    await _userManager.ResetAccessFailedCountAsync(user);
}
```

### 2. Database Connection Issues

#### Problem: "Could not connect to database" errors
**Symptoms:**
- Application startup failures
- 500 Internal Server Error responses
- Database timeout exceptions

**Diagnostic Steps:**
```bash
# Test database connectivity
psql -h your-db-host -U username -d kaopiz_auth -c "SELECT 1;"

# Check connection pool status
docker exec -it api-container dotnet run -- --check-db

# Monitor database connections
SELECT application_name, client_addr, state, query 
FROM pg_stat_activity 
WHERE datname = 'kaopiz_auth';
```

**Solutions:**

**Connection String Issues**
```bash
# Verify environment variables
echo $DB_CONNECTION_STRING

# Test with minimal connection string
Host=localhost;Database=kaopiz_auth;Username=user;Password=pass
```

**Connection Pool Exhaustion**
```csharp
// Update connection string with proper pooling
"Host=localhost;Database=kaopiz_auth;Username=user;Password=pass;Maximum Pool Size=100;Timeout=30;"
```

**Network Issues**
```bash
# Test network connectivity
telnet your-db-host 5432
nc -zv your-db-host 5432

# Check firewall rules
sudo iptables -L | grep 5432
```

### 3. Performance Issues

#### Problem: Slow API responses
**Symptoms:**
- High response times (>2 seconds)
- Timeout errors
- Poor user experience

**Diagnostic Tools:**
```bash
# Check API response times
curl -w "@curl-format.txt" -o /dev/null -s "https://api.kaopiz.com/api/auth/login"

# Monitor container resources
docker stats kaopiz-api

# Check database query performance
SELECT query, calls, total_time, mean_time 
FROM pg_stat_statements 
WHERE mean_time > 100 
ORDER BY mean_time DESC;
```

**Performance Optimizations:**

**Database Query Optimization**
```csharp
// Add proper indexes
CREATE INDEX IX_AspNetUsers_NormalizedEmail_Active 
ON AspNetUsers (NormalizedEmail) 
WHERE IsActive = true;

// Use efficient queries
var user = await _context.Users
    .Where(u => u.NormalizedEmail == email.ToUpper() && u.IsActive)
    .FirstOrDefaultAsync();
```

**Caching Implementation**
```csharp
// Add Redis caching for user data
public async Task<User> GetUserAsync(string userId)
{
    var cacheKey = $"user:{userId}";
    var cached = await _cache.GetStringAsync(cacheKey);
    
    if (cached != null)
        return JsonSerializer.Deserialize<User>(cached);
        
    var user = await _repository.GetByIdAsync(userId);
    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user), 
        TimeSpan.FromMinutes(5));
        
    return user;
}
```

### 4. Rate Limiting Issues

#### Problem: Users getting rate limited unexpectedly
**Symptoms:**
- 429 Too Many Requests errors
- Legitimate users blocked
- API becoming unusable

**Diagnostic Steps:**
```bash
# Check rate limit logs
kubectl logs -f deployment/kaopiz-auth-api | grep "RateLimit"

# Monitor Redis for rate limit data
redis-cli keys "*ratelimit*"
redis-cli get "ratelimit:192.168.1.100"
```

**Solutions:**

**Adjust Rate Limits**
```json
{
  "RateLimit": {
    "LoginAttempts": {
      "PermitLimit": 10,
      "WindowInMinutes": 1
    },
    "GeneralApi": {
      "PermitLimit": 1000,
      "WindowInMinutes": 1
    }
  }
}
```

**Whitelist Internal IPs**
```csharp
public class RateLimitMiddleware
{
    private readonly string[] _whitelistedIPs = { "192.168.1.0/24", "10.0.0.0/8" };
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var clientIP = context.Connection.RemoteIpAddress;
        
        if (IsWhitelisted(clientIP))
        {
            await next(context);
            return;
        }
        
        // Apply rate limiting
    }
}
```

### 5. Memory and Resource Issues

#### Problem: High memory usage or out-of-memory errors
**Symptoms:**
- Application crashes
- Slow performance
- Container restarts

**Monitoring Commands:**
```bash
# Check container memory usage
docker stats --no-stream

# Monitor .NET memory usage
dotnet-counters monitor --process-id $(pgrep -f "KaopizAuth.WebAPI")

# Check for memory leaks
dotnet-dump collect -p $(pgrep -f "KaopizAuth.WebAPI")
```

**Solutions:**

**Configure Memory Limits**
```yaml
# Kubernetes resource limits
resources:
  requests:
    memory: "512Mi"
    cpu: "250m"
  limits:
    memory: "1Gi"
    cpu: "500m"
```

**Optimize Entity Framework**
```csharp
// Disable change tracking for read-only queries
var users = await _context.Users
    .AsNoTracking()
    .Where(u => u.IsActive)
    .ToListAsync();

// Dispose DbContext properly
using var scope = serviceProvider.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
```

## 🔍 Debugging Techniques

### 1. Enable Detailed Logging

**appsettings.Development.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.AspNetCore.Authentication": "Debug",
      "KaopizAuth": "Debug"
    }
  }
}
```

**Structured Logging with Serilog**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.WithProperty("Application", "KaopizAuth")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
    .CreateLogger();
```

### 2. Request/Response Logging

**Custom Middleware**
```csharp
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log request
        var request = await FormatRequest(context.Request);
        _logger.LogInformation("HTTP Request: {Request}", request);

        // Capture response
        var originalResponseBody = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Log response
        var response = await FormatResponse(context.Response);
        _logger.LogInformation("HTTP Response: {Response}", response);

        await responseBody.CopyToAsync(originalResponseBody);
    }
}
```

### 3. Database Query Debugging

**Enable EF Core Query Logging**
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .EnableSensitiveDataLogging() // Only in development
        .EnableDetailedErrors()
        .LogTo(Console.WriteLine, LogLevel.Information);
}
```

**SQL Query Analysis**
```sql
-- Enable query logging in PostgreSQL
ALTER SYSTEM SET log_statement = 'all';
SELECT pg_reload_conf();

-- Monitor slow queries
SELECT query, calls, total_time, mean_time, stddev_time
FROM pg_stat_statements
WHERE mean_time > 100
ORDER BY mean_time DESC;
```

## 🛠️ Diagnostic Scripts

### Health Check Script
```bash
#!/bin/bash
# health-check.sh

echo "=== Kaopiz Auth Health Check ==="

# API Health
echo "1. Checking API health..."
response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health)
if [ $response = "200" ]; then
    echo "✅ API is healthy"
else
    echo "❌ API is unhealthy (HTTP $response)"
fi

# Database connectivity
echo "2. Checking database..."
if pg_isready -h localhost -p 5432; then
    echo "✅ Database is reachable"
else
    echo "❌ Database is unreachable"
fi

# Redis connectivity
echo "3. Checking Redis..."
if redis-cli ping | grep -q "PONG"; then
    echo "✅ Redis is reachable"
else
    echo "❌ Redis is unreachable"
fi

# Disk space
echo "4. Checking disk space..."
disk_usage=$(df / | tail -1 | awk '{print $5}' | sed 's/%//')
if [ $disk_usage -lt 90 ]; then
    echo "✅ Disk space is adequate ($disk_usage% used)"
else
    echo "⚠️ Disk space is low ($disk_usage% used)"
fi

# Memory usage
echo "5. Checking memory..."
memory_usage=$(free | grep Mem | awk '{printf("%.0f", $3/$2 * 100.0)}')
if [ $memory_usage -lt 90 ]; then
    echo "✅ Memory usage is normal ($memory_usage% used)"
else
    echo "⚠️ Memory usage is high ($memory_usage% used)"
fi

echo "=== Health check completed ==="
```

### Log Analysis Script
```bash
#!/bin/bash
# analyze-logs.sh

LOG_FILE=${1:-/app/logs/log-$(date +%Y%m%d).txt}

echo "=== Log Analysis for $LOG_FILE ==="

# Error count
echo "1. Error Analysis:"
error_count=$(grep -c "ERROR" $LOG_FILE)
echo "   Total errors: $error_count"

if [ $error_count -gt 0 ]; then
    echo "   Recent errors:"
    grep "ERROR" $LOG_FILE | tail -5
fi

# Warning count
echo "2. Warning Analysis:"
warning_count=$(grep -c "WARNING" $LOG_FILE)
echo "   Total warnings: $warning_count"

# Login attempts
echo "3. Authentication Analysis:"
failed_logins=$(grep -c "Login attempt failed" $LOG_FILE)
successful_logins=$(grep -c "Login successful" $LOG_FILE)
echo "   Successful logins: $successful_logins"
echo "   Failed logins: $failed_logins"

# Rate limiting
echo "4. Rate Limiting Analysis:"
rate_limited=$(grep -c "Rate limit exceeded" $LOG_FILE)
echo "   Rate limited requests: $rate_limited"

# Performance
echo "5. Performance Analysis:"
slow_requests=$(grep -c "Request took longer than" $LOG_FILE)
echo "   Slow requests (>2s): $slow_requests"
```

## 📞 Escalation Procedures

### Level 1: Application Issues
- **Response Time**: 15 minutes
- **Contact**: Development team
- **Actions**: 
  - Check application logs
  - Verify configuration
  - Restart services if needed

### Level 2: Infrastructure Issues
- **Response Time**: 30 minutes
- **Contact**: DevOps team
- **Actions**:
  - Check infrastructure health
  - Scale resources if needed
  - Investigate network issues

### Level 3: Security Incidents
- **Response Time**: Immediate
- **Contact**: Security team + Management
- **Actions**:
  - Isolate affected systems
  - Preserve evidence
  - Follow incident response plan

### Emergency Contacts
```yaml
Development Team:
  - Primary: dev-team@kaopiz.com
  - Phone: +1-xxx-xxx-xxxx
  
DevOps Team:
  - Primary: devops@kaopiz.com
  - Phone: +1-xxx-xxx-xxxx
  
Security Team:
  - Primary: security@kaopiz.com
  - Phone: +1-xxx-xxx-xxxx
```

## 📊 Monitoring Dashboards

### Key Metrics to Monitor
- **Response Time**: <200ms for 95th percentile
- **Error Rate**: <1% of total requests
- **Database Connections**: <80% of pool size
- **Memory Usage**: <80% of allocated memory
- **CPU Usage**: <70% average
- **Disk Space**: <80% used

### Alerting Thresholds
```yaml
Critical Alerts:
  - API down (health check fails)
  - Error rate >5%
  - Response time >2s for 95th percentile
  - Database connection failures
  
Warning Alerts:
  - Error rate >1%
  - Response time >500ms for 95th percentile
  - Memory usage >80%
  - Disk space >80%
```

---

This troubleshooting guide should help quickly identify and resolve most common issues with the Kaopiz Auth system.