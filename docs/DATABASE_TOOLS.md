# Database Management Tools

This guide covers how to view and manage the SQLite database for the HnH Mapper Server.

## Built-in Database UI (Admin Panel)

The admin panel includes a built-in database browser accessible after logging in.

### Access
1. Start the Aspire AppHost
2. Login with your admin credentials
3. Navigate to **Admin Panel → Database tab**

### Features

#### SQL Query Tab
- Write and execute SELECT queries
- View results in a data grid
- Quick way to inspect data

**Example Queries:**
```sql
-- View all users
SELECT * FROM Users;

-- Count active sessions
SELECT COUNT(*) FROM Sessions;

-- View recent grids
SELECT * FROM Grids ORDER BY Id DESC LIMIT 10;

-- Database statistics
SELECT 'Users' as Table, COUNT(*) as Count FROM Users
UNION ALL
SELECT 'Sessions', COUNT(*) FROM Sessions
UNION ALL
SELECT 'Grids', COUNT(*) FROM Grids
UNION ALL
SELECT 'Markers', COUNT(*) FROM Markers
UNION ALL
SELECT 'Tiles', COUNT(*) FROM Tiles;
```

#### Browse Tables Tab
- Select any table from dropdown
- View table contents (max 1000 rows)
- Filter and sort data

#### Schema Tab
- View all tables and their structure
- See column names, types, and constraints
- Identify primary keys

## DB Browser for SQLite (Advanced Tool)

For advanced database management, we recommend **DB Browser for SQLite** - a free, open-source tool.

### Download & Install

**Windows:**
- Download from: https://sqlitebrowser.org/dl/
- Run the installer or use portable version
- No configuration needed

**macOS:**
- Download .dmg from website
- Or install via Homebrew: `brew install --cask db-browser-for-sqlite`

**Linux:**
- Ubuntu/Debian: `sudo apt-get install sqlitebrowser`
- Fedora: `sudo dnf install sqlitebrowser`

### Connect to Database

1. **Stop the application** (important - prevents file locks)
2. Open DB Browser for SQLite
3. Click **"Open Database"**
4. Navigate to: `HnHMapperServer/src/HnHMapperServer.Api/map/grids.db`

**OR** for read-only access while app is running:
- Hold **Shift** while clicking "Open Database"
- This opens in read-only mode

### Features

#### 1. Database Structure Tab
- View all tables
- See table schemas
- Modify structure (when stopped)

#### 2. Browse Data Tab
- Browse table contents
- Edit data directly
- Filter and search

#### 3. Execute SQL Tab
- Write and run any SQL query
- Multiple queries at once
- Export results

#### 4. Export Data
- Right-click any table → **Export to CSV**
- Export entire database
- Backup before making changes

### Common Tasks

#### View User Permissions
```sql
SELECT Username, AuthsJson, TokensJson
FROM Users;
```

#### Check Active Sessions
```sql
SELECT s.Id, s.Username, s.TempAdmin, s.CreatedAt
FROM Sessions s
ORDER BY s.CreatedAt DESC;
```

#### Grid Statistics
```sql
SELECT
    COUNT(DISTINCT gc) as GridCount,
    COUNT(*) as TileCount,
    SUM(LENGTH(Data)) as TotalDataSize
FROM Grids;
```

#### Find Markers
```sql
SELECT m.Id, m.GridId, m.Name, m.X, m.Y
FROM Markers m
ORDER BY m.GridId, m.Id;
```

### Safety Tips

✅ **DO:**
- Make backups before editing
- Use read-only mode when app is running
- Test queries on copies first

❌ **DON'T:**
- Edit database while app is running
- Delete system tables (__EFMigrationsHistory)
- Modify primary keys
- Remove foreign key constraints

### Backup Database

**Method 1: Using Admin Panel**
- Go to Admin Panel → System tab
- Click "Backup Database"
- Backup saved to `map/grids.backup.[timestamp].db`

**Method 2: Manual Copy**
```bash
# Stop the application first
cp HnHMapperServer/src/HnHMapperServer.Api/map/grids.db grids.backup.db
```

**Method 3: DB Browser**
- File → Export → Database to SQL file
- Creates portable SQL dump

### Restore Database

```bash
# Stop application
cd HnHMapperServer/src/HnHMapperServer.Api/map
cp grids.backup.[timestamp].db grids.db
# Start application
```

### Troubleshooting

#### "Database is locked" Error
- Stop the ASP.NET application
- Close all DB Browser windows
- Wait 5 seconds and try again

#### Migrations Won't Run
- Delete `grids.db`, `grids.db-shm`, `grids.db-wal`
- Restart application
- Fresh database will be created

#### Can't See Recent Changes
- Click **File → Reload Database** in DB Browser
- Or restart DB Browser

## Database Schema Reference

### Users Table
- `Username` (PK) - User login name
- `PasswordHash` - BCrypt hashed password
- `AuthsJson` - JSON array of permissions
- `TokensJson` - JSON array of API tokens

### Sessions Table
- `Id` (PK) - Session identifier (cookie value)
- `Username` - Associated user
- `TempAdmin` - Temporary admin flag
- `CreatedAt` - Session creation time

### Grids Table
- `gc` (PK) - Grid coordinates
- `Data` - Grid tile data
- Additional grid metadata

### Markers Table
- `Id` (PK) - Marker identifier
- `GridId` - Associated grid
- `Name` - Marker name
- `X`, `Y` - Coordinates
- `Image` - Marker icon

### Tiles Table
- Cached tile data for zoom levels

### Maps Table
- Map metadata and configurations

### Tokens Table
- `Token` (PK) - API token value
- `Username` - Token owner
- Linked to Users table

### Config Table
- `Key` (PK) - Configuration key
- `Value` - Configuration value

## Quick Reference Commands

### View All Tables
```sql
SELECT name FROM sqlite_master WHERE type='table';
```

### Table Row Counts
```sql
SELECT
    (SELECT COUNT(*) FROM Users) as Users,
    (SELECT COUNT(*) FROM Sessions) as Sessions,
    (SELECT COUNT(*) FROM Grids) as Grids,
    (SELECT COUNT(*) FROM Markers) as Markers,
    (SELECT COUNT(*) FROM Tiles) as Tiles,
    (SELECT COUNT(*) FROM Maps) as Maps,
    (SELECT COUNT(*) FROM Tokens) as Tokens,
    (SELECT COUNT(*) FROM Config) as Config;
```

### Database Size
```sql
SELECT page_count * page_size as size FROM pragma_page_count(), pragma_page_size();
```

### Vacuum Database (Reclaim Space)
```sql
VACUUM;
```

## Support

For issues with:
- **Admin Panel Database UI**: Check application logs
- **DB Browser**: Visit https://sqlitebrowser.org/
- **SQLite**: Check https://www.sqlite.org/docs.html
