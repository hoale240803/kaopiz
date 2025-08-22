#!/bin/bash

# Database backup script for production
set -euo pipefail

# Configuration
DB_HOST=${DB_HOST:-kaopizauth-db}
DB_NAME=${DB_NAME:-KaopizAuth}
DB_USER=${DB_USER:-postgres}
BACKUP_DIR=${BACKUP_DIR:-/backups}
RETENTION_DAYS=${RETENTION_DAYS:-30}

# Create backup directory if it doesn't exist
mkdir -p "$BACKUP_DIR"

# Create timestamp
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="$BACKUP_DIR/kaopizauth_backup_$TIMESTAMP.sql"

echo "Starting database backup..."
echo "Host: $DB_HOST"
echo "Database: $DB_NAME"
echo "User: $DB_USER"
echo "Backup file: $BACKUP_FILE"

# Create database backup
if pg_dump -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" \
    --no-password \
    --verbose \
    --clean \
    --if-exists \
    --create \
    --format=custom \
    --compress=9 \
    > "$BACKUP_FILE"; then
    
    echo "Database backup completed successfully"
    
    # Compress the backup
    gzip "$BACKUP_FILE"
    COMPRESSED_FILE="$BACKUP_FILE.gz"
    
    # Verify backup integrity
    if gunzip -t "$COMPRESSED_FILE" 2>/dev/null; then
        echo "Backup compression verified successfully"
        FINAL_SIZE=$(du -h "$COMPRESSED_FILE" | cut -f1)
        echo "Final backup size: $FINAL_SIZE"
    else
        echo "ERROR: Backup compression verification failed"
        exit 1
    fi
    
    # Clean up old backups
    echo "Cleaning up backups older than $RETENTION_DAYS days..."
    find "$BACKUP_DIR" -name "kaopizauth_backup_*.sql.gz" -type f -mtime +$RETENTION_DAYS -delete
    
    echo "Backup cleanup completed"
    
    # Log backup completion
    echo "$(date): Backup completed successfully - $COMPRESSED_FILE" >> "$BACKUP_DIR/backup.log"
    
else
    echo "ERROR: Database backup failed"
    echo "$(date): Backup failed" >> "$BACKUP_DIR/backup.log"
    exit 1
fi

echo "Backup process completed successfully"