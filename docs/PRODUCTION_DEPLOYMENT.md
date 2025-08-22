# Production Deployment Guide

## Overview

This guide covers the complete production deployment setup for KaopizAuth, including monitoring, security, and performance optimizations.

## Prerequisites

- Docker and Docker Compose installed on production servers
- SSL certificates for HTTPS
- Environment variables configured
- Database server (PostgreSQL) available
- Redis server for caching

## Environment Setup

### 1. Environment Files

Copy the environment template files and configure them for your environment:

```bash
# For production
cp .env.production.example .env.production
# Edit .env.production with your actual values

# For staging
cp .env.staging.example .env.staging
# Edit .env.staging with your actual values
```

### 2. SSL Certificates

Place your SSL certificates in the `docker/ssl/` directory:
- `server.crt` - SSL certificate
- `server.key` - Private key

## Deployment Instructions

### Production Deployment

1. **Prepare the production server:**
   ```bash
   # Create application directory
   sudo mkdir -p /opt/kaopizauth
   cd /opt/kaopizauth
   
   # Clone or copy the application files
   git clone <repository-url> .
   
   # Set up environment
   cp .env.production.example .env.production
   # Edit .env.production with production values
   ```

2. **Deploy the application:**
   ```bash
   # Pull latest images
   docker-compose -f docker-compose.production.yml pull
   
   # Run database migrations
   docker-compose -f docker-compose.production.yml run --rm \
     -v $(pwd)/scripts:/scripts \
     kaopizauth-api /scripts/migrate.sh
   
   # Start services
   docker-compose -f docker-compose.production.yml up -d
   ```

3. **Verify deployment:**
   ```bash
   # Check service health
   curl -f https://your-domain.com/api/health
   
   # Check detailed health status
   curl -f https://your-domain.com/api/health/detailed
   ```

### Staging Deployment

```bash
cd /opt/kaopizauth-staging

# Deploy to staging
docker-compose -f docker-compose.staging.yml pull
docker-compose -f docker-compose.staging.yml up -d

# Verify staging deployment
curl -f https://staging.your-domain.com/api/health
```

## Monitoring and Observability

### Health Checks

The application provides comprehensive health check endpoints:

- `/api/health` - Basic health status
- `/api/health/ready` - Readiness probe for container orchestration
- `/api/health/live` - Liveness probe for container orchestration
- `/api/health/database` - Database connectivity check
- `/api/health/detailed` - Comprehensive health status with all dependencies

### Metrics

Prometheus metrics are exposed at `/metrics` endpoint:

- HTTP request metrics (duration, count, errors)
- Application performance metrics
- Database connection metrics
- Custom business metrics

### Logging

Structured logging is configured with Serilog:

- Console logging for container environments
- File logging with daily rotation
- JSON structured format for log aggregation
- Request/response logging with performance tracking

### Log Aggregation

Promtail is configured to collect logs from:
- API application logs
- WebApp logs
- Nginx access/error logs

## Security Features

### Security Headers

All responses include security headers:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: SAMEORIGIN`
- `X-XSS-Protection: 1; mode=block`
- `Strict-Transport-Security: max-age=63072000`
- `Content-Security-Policy` with restrictive policies
- `Referrer-Policy: strict-origin-when-cross-origin`

### Rate Limiting

Implemented at multiple levels:
- Nginx rate limiting for API endpoints
- Application-level rate limiting for authentication
- Configurable rate limits per endpoint type

### SSL/TLS Configuration

- TLS 1.2+ only
- Modern cipher suites
- HSTS enabled
- HTTP to HTTPS redirection

## Performance Optimizations

### Application Level

- Connection pooling for database
- Redis caching for sessions and data
- Request/response compression
- Static asset caching

### Infrastructure Level

- Nginx load balancing with least connections
- HTTP/2 support
- Gzip compression
- Static asset optimization

### Database Optimizations

- Connection pooling
- Optimized PostgreSQL configuration
- Regular backup and maintenance

## Backup and Recovery

### Automated Backups

Database backups are automatically created:
- Daily backups with 30-day retention (production)
- Weekly backups with 7-day retention (staging)
- Compressed and verified backups
- Pre-deployment backups for rollback capability

### Backup Commands

```bash
# Manual backup
docker-compose -f docker-compose.production.yml run --rm \
  -v $(pwd)/scripts:/scripts \
  db-backup /scripts/backup.sh

# Restore from backup
gunzip -c /backups/backup_file.sql.gz | \
  docker-compose -f docker-compose.production.yml exec -T kaopizauth-db \
  psql -U postgres -d KaopizAuth
```

## Scaling Considerations

### Horizontal Scaling

To scale the application:

1. **API Scaling:**
   ```bash
   # Scale API instances
   docker-compose -f docker-compose.production.yml up -d --scale kaopizauth-api=3
   ```

2. **Database Scaling:**
   - Configure read replicas
   - Use connection pooling
   - Implement database sharding if needed

3. **Load Balancer Configuration:**
   - Nginx is pre-configured for load balancing
   - Add additional upstream servers in nginx configuration

### Resource Limits

Current resource limits (configurable in docker-compose files):

**Production:**
- API: 512MB RAM, 0.5 CPU
- WebApp: 256MB RAM, 0.25 CPU

**Staging:**
- API: 256MB RAM, 0.3 CPU
- WebApp: 128MB RAM, 0.2 CPU

## Troubleshooting

### Common Issues

1. **Health Check Failures:**
   ```bash
   # Check application logs
   docker-compose -f docker-compose.production.yml logs kaopizauth-api
   
   # Check database connectivity
   docker-compose -f docker-compose.production.yml exec kaopizauth-api \
     curl http://localhost:8080/api/health/database
   ```

2. **Performance Issues:**
   ```bash
   # Check metrics
   curl http://localhost/metrics
   
   # Review slow request logs
   docker-compose -f docker-compose.production.yml logs kaopizauth-api | grep "slow"
   ```

3. **Database Issues:**
   ```bash
   # Check database logs
   docker-compose -f docker-compose.production.yml logs kaopizauth-db
   
   # Connect to database
   docker-compose -f docker-compose.production.yml exec kaopizauth-db \
     psql -U postgres -d KaopizAuth
   ```

### Recovery Procedures

1. **Application Recovery:**
   ```bash
   # Restart services
   docker-compose -f docker-compose.production.yml restart
   
   # Rollback to previous version
   # (Would require implementing blue-green deployment)
   ```

2. **Database Recovery:**
   ```bash
   # Restore from backup
   ./scripts/restore.sh /backups/backup_file.sql.gz
   ```

## CI/CD Pipeline

The GitHub Actions pipeline includes:

- **Testing:** Backend and frontend tests
- **Security Scanning:** Vulnerability checks
- **Building:** Docker image creation and pushing
- **Deployment:** Automated deployment to staging and production
- **Monitoring:** Post-deployment health checks

### Manual Deployment Commands

If needed, you can trigger deployments manually:

```bash
# Deploy to staging
gh workflow run ci.yml --ref develop

# Deploy to production
gh workflow run ci.yml --ref main
```

## Maintenance

### Regular Tasks

1. **Log Rotation:** Automated via Serilog configuration
2. **Backup Verification:** Automated via backup scripts
3. **Security Updates:** Monitor and apply security patches
4. **Performance Monitoring:** Review metrics and logs regularly
5. **Disk Space Monitoring:** Monitor `/app/logs` and `/backups` directories

### Scheduled Maintenance

1. **Database Maintenance:**
   ```bash
   # Vacuum and analyze (weekly)
   docker-compose -f docker-compose.production.yml exec kaopizauth-db \
     psql -U postgres -d KaopizAuth -c "VACUUM ANALYZE;"
   ```

2. **Log Cleanup:**
   ```bash
   # Clean old logs (if needed beyond automatic rotation)
   find /var/log -name "*.log" -mtime +30 -delete
   ```

## Support and Monitoring

### Monitoring Dashboards

Set up dashboards for:
- Application performance metrics
- Infrastructure metrics (CPU, memory, disk)
- Business metrics (user registrations, logins)
- Error rates and response times

### Alerting

Configure alerts for:
- Application health check failures
- High error rates
- Performance degradation
- Infrastructure issues
- Security events

### Contact Information

For production issues:
- Application team: [team-email]
- Infrastructure team: [infra-email]
- On-call rotation: [on-call-contact]