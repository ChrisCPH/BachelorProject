#!/bin/bash

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
until /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P Password123 -Q "SELECT 1" &> /dev/null
do
  echo "SQL Server is not ready yet. Sleeping..."
  sleep 5
done

echo "SQL Server is ready. Restoring database..."

# Restore the database
/opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P Password123 -Q \
"RESTORE DATABASE [RunningDB] FROM DISK = '/backup/RunningDB.bak' \
WITH MOVE 'RunningDB' TO '/var/opt/mssql/data/RunningDB.mdf', \
MOVE 'RunningDB_log' TO '/var/opt/mssql/data/RunningDB_log.ldf', REPLACE"
