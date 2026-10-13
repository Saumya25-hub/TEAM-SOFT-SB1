# Database Architecture & Backup Guide

## 1. Relational Model & Database Provider
The backend utilizes **Prisma ORM** targeting **PostgreSQL**.
- Schema file: `backend/prisma/schema.prisma`
- Connection URL: Specified via `DATABASE_URL` in `backend/.env`.

### Production PostgreSQL Deployment
Run migrations against the target database:
```bash
cd backend
npx prisma migrate deploy
```

---

## 2. Automated PostgreSQL Backup Strategy

### Daily Backup Script (Windows PowerShell)
Create a scheduled task running `scripts/backup-db.ps1`:
```powershell
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupDir = "C:\Backups\SimpleBilling"
$BackupFile = "$BackupDir\simple_billing_backup_$Timestamp.dump"

if (!(Test-Path $BackupDir)) { New-Item -ItemType Directory -Path $BackupDir }

# Execute pg_dump with custom compressed archive format (-Fc)
& "C:\Program Files\PostgreSQL\16\bin\pg_dump.exe" -U postgres -h localhost -p 5432 -F c -b -v -f $BackupFile simple_billing_db

# Retain only last 30 days of backups
Get-ChildItem $BackupDir -Filter *.dump | Where-Object { $_.CreationTime -lt (Get-Date).AddDays(-30) } | Remove-Item
```

### Database Restore Procedure
To restore from a backup dump file:
```bash
pg_restore -U postgres -h localhost -p 5432 -d simple_billing_db -v "C:\Backups\SimpleBilling\simple_billing_backup_XXXX.dump"
```

---

## 3. Network & Offline Behavior Policy

1. **Connected Client-Server Operation**:
   - The desktop client connects to the central Backend API over HTTPS.
   - All multi-tenant queries are authenticated via JWT tokens and scoped to the active `businessId`.
2. **Connection Resilience**:
   - The desktop client displays clear network status alerts if the central API is temporarily unreachable.
   - No data is silently lost; in-flight invoices prompt retry or queueing.
   - Multi-computer offline synchronization is explicitly restricted to connected network mode to prevent split-brain inventory conflicts.
