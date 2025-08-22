#!/bin/bash

# Database migration script for production deployments
set -euo pipefail

# Configuration
DB_HOST=${DB_HOST:-kaopizauth-db}
DB_NAME=${DB_NAME:-KaopizAuth}
DB_USER=${DB_USER:-postgres}
BACKUP_DIR=${BACKUP_DIR:-/backups}
MIGRATION_TIMEOUT=${MIGRATION_TIMEOUT:-300}

echo "Starting database migration process..."

# Function to check database connectivity
check_database() {
    local max_attempts=30
    local attempt=1
    
    echo "Checking database connectivity..."
    
    while [ $attempt -le $max_attempts ]; do
        if pg_isready -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1; then
            echo "Database is ready"
            return 0
        fi
        
        echo "Attempt $attempt/$max_attempts: Database not ready, waiting..."
        sleep 2
        ((attempt++))
    done
    
    echo "ERROR: Database is not accessible after $max_attempts attempts"
    return 1
}

# Function to create pre-migration backup
create_pre_migration_backup() {
    echo "Creating pre-migration backup..."
    
    local timestamp=$(date +"%Y%m%d_%H%M%S")
    local backup_file="$BACKUP_DIR/pre_migration_backup_$timestamp.sql"
    
    if pg_dump -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" \
        --no-password \
        --verbose \
        --clean \
        --if-exists \
        --create \
        --format=custom \
        --compress=9 \
        > "$backup_file"; then
        
        gzip "$backup_file"
        echo "Pre-migration backup created: $backup_file.gz"
        echo "$backup_file.gz"
    else
        echo "ERROR: Failed to create pre-migration backup"
        return 1
    fi
}

# Function to run migrations
run_migrations() {
    echo "Running database migrations..."
    
    # Navigate to the API directory
    cd /app || {
        echo "ERROR: Cannot access application directory"
        return 1
    }
    
    # Run Entity Framework migrations with timeout
    timeout "$MIGRATION_TIMEOUT" dotnet ef database update --no-build --verbose || {
        local exit_code=$?
        if [ $exit_code -eq 124 ]; then
            echo "ERROR: Migration timed out after $MIGRATION_TIMEOUT seconds"
        else
            echo "ERROR: Migration failed with exit code $exit_code"
        fi
        return $exit_code
    }
    
    echo "Migrations completed successfully"
}

# Function to verify migration
verify_migration() {
    echo "Verifying migration..."
    
    # Check if database can be accessed
    if ! psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -c "SELECT 1;" >/dev/null 2>&1; then
        echo "ERROR: Cannot access database after migration"
        return 1
    fi
    
    # Check if migration history table exists and has entries
    local migration_count
    migration_count=$(psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" \
        -t -c "SELECT COUNT(*) FROM \"__EFMigrationsHistory\";" 2>/dev/null | tr -d ' ')
    
    if [[ "$migration_count" =~ ^[0-9]+$ ]] && [ "$migration_count" -gt 0 ]; then
        echo "Migration verification successful: $migration_count migrations applied"
        return 0
    else
        echo "ERROR: Migration verification failed"
        return 1
    fi
}

# Function to rollback migration
rollback_migration() {
    local backup_file="$1"
    
    echo "Rolling back migration using backup: $backup_file"
    
    if [ -f "$backup_file" ]; then
        # Restore from backup
        if gunzip -c "$backup_file" | pg_restore -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" \
            --clean \
            --if-exists \
            --no-owner \
            --no-privileges \
            --verbose; then
            echo "Migration rollback completed successfully"
        else
            echo "ERROR: Migration rollback failed"
            return 1
        fi
    else
        echo "ERROR: Backup file not found: $backup_file"
        return 1
    fi
}

# Main migration process
main() {
    local backup_file=""
    
    # Check database connectivity
    if ! check_database; then
        exit 1
    fi
    
    # Create pre-migration backup
    if backup_file=$(create_pre_migration_backup); then
        echo "Pre-migration backup created successfully"
    else
        echo "ERROR: Failed to create pre-migration backup"
        exit 1
    fi
    
    # Run migrations
    if run_migrations; then
        echo "Migrations completed successfully"
        
        # Verify migration
        if verify_migration; then
            echo "Migration verification successful"
            echo "Migration process completed successfully"
            exit 0
        else
            echo "Migration verification failed, initiating rollback..."
            rollback_migration "$backup_file"
            exit 1
        fi
    else
        echo "Migration failed, initiating rollback..."
        rollback_migration "$backup_file"
        exit 1
    fi
}

# Run main function
main "$@"