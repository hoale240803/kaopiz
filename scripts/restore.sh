#!/bin/bash

# Database restore script for production
set -euo pipefail

# Configuration
DB_HOST=${DB_HOST:-kaopizauth-db}
DB_NAME=${DB_NAME:-KaopizAuth}
DB_USER=${DB_USER:-postgres}
BACKUP_FILE="$1"

# Usage function
usage() {
    echo "Usage: $0 <backup_file>"
    echo "Example: $0 /backups/kaopizauth_backup_20240821_120000.sql.gz"
    exit 1
}

# Check if backup file is provided
if [ $# -eq 0 ]; then
    echo "ERROR: No backup file specified"
    usage
fi

# Check if backup file exists
if [ ! -f "$BACKUP_FILE" ]; then
    echo "ERROR: Backup file not found: $BACKUP_FILE"
    exit 1
fi

echo "Starting database restore process..."
echo "Host: $DB_HOST"
echo "Database: $DB_NAME"
echo "User: $DB_USER"
echo "Backup file: $BACKUP_FILE"

# Function to check database connectivity
check_database() {
    local max_attempts=30
    local attempt=1
    
    echo "Checking database connectivity..."
    
    while [ $attempt -le $max_attempts ]; do
        if pg_isready -h "$DB_HOST" -U "$DB_USER" >/dev/null 2>&1; then
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

# Function to create pre-restore backup
create_pre_restore_backup() {
    echo "Creating pre-restore backup..."
    
    local timestamp=$(date +"%Y%m%d_%H%M%S")
    local backup_dir=$(dirname "$BACKUP_FILE")
    local pre_restore_file="$backup_dir/pre_restore_backup_$timestamp.sql"
    
    if pg_dump -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" \
        --no-password \
        --verbose \
        --clean \
        --if-exists \
        --create \
        --format=custom \
        --compress=9 \
        > "$pre_restore_file"; then
        
        gzip "$pre_restore_file"
        echo "Pre-restore backup created: $pre_restore_file.gz"
        return 0
    else
        echo "WARNING: Failed to create pre-restore backup, continuing anyway..."
        return 0
    fi
}

# Function to restore database
restore_database() {
    echo "Restoring database from backup..."
    
    # Determine if backup is compressed
    if [[ "$BACKUP_FILE" == *.gz ]]; then
        echo "Detected compressed backup file"
        
        # Verify backup integrity first
        if ! gunzip -t "$BACKUP_FILE" 2>/dev/null; then
            echo "ERROR: Backup file integrity check failed"
            return 1
        fi
        
        # Restore from compressed backup
        if gunzip -c "$BACKUP_FILE" | pg_restore -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" \
            --clean \
            --if-exists \
            --no-owner \
            --no-privileges \
            --verbose; then
            echo "Database restored successfully from compressed backup"
        else
            echo "ERROR: Database restore failed"
            return 1
        fi
    else
        # Restore from uncompressed backup
        if pg_restore -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" \
            --clean \
            --if-exists \
            --no-owner \
            --no-privileges \
            --verbose \
            "$BACKUP_FILE"; then
            echo "Database restored successfully from backup"
        else
            echo "ERROR: Database restore failed"
            return 1
        fi
    fi
}

# Function to verify restore
verify_restore() {
    echo "Verifying database restore..."
    
    # Check if database can be accessed
    if ! psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -c "SELECT 1;" >/dev/null 2>&1; then
        echo "ERROR: Cannot access database after restore"
        return 1
    fi
    
    # Check if migration history table exists
    local migration_count
    migration_count=$(psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" \
        -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '__EFMigrationsHistory';" 2>/dev/null | tr -d ' ')
    
    if [[ "$migration_count" =~ ^[0-9]+$ ]] && [ "$migration_count" -gt 0 ]; then
        local migrations
        migrations=$(psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" \
            -t -c "SELECT COUNT(*) FROM \"__EFMigrationsHistory\";" 2>/dev/null | tr -d ' ')
        
        echo "Restore verification successful: $migrations migrations found"
        return 0
    else
        echo "WARNING: Migration history table not found, but database is accessible"
        return 0
    fi
}

# Main restore process
main() {
    # Check database connectivity
    if ! check_database; then
        exit 1
    fi
    
    # Create pre-restore backup
    create_pre_restore_backup
    
    # Confirm restore operation
    echo ""
    echo "WARNING: This will replace the current database with the backup data."
    echo "Backup file: $BACKUP_FILE"
    echo "Target database: $DB_NAME on $DB_HOST"
    echo ""
    read -p "Are you sure you want to continue? (yes/no): " -r
    echo ""
    
    if [[ ! $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
        echo "Restore operation cancelled by user"
        exit 0
    fi
    
    # Restore database
    if restore_database; then
        echo "Database restore completed successfully"
        
        # Verify restore
        if verify_restore; then
            echo "Restore verification successful"
            echo "Database restore process completed successfully"
            
            # Log restore completion
            local log_dir=$(dirname "$BACKUP_FILE")
            echo "$(date): Restore completed successfully from $BACKUP_FILE" >> "$log_dir/restore.log"
            
            exit 0
        else
            echo "WARNING: Restore verification failed, but restore may have succeeded"
            exit 0
        fi
    else
        echo "ERROR: Database restore failed"
        exit 1
    fi
}

# Run main function
main "$@"