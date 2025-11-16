# HnH Mapper Production Deployment Files (Multi-Tenant)

This directory contains all the files needed to deploy HnH Mapper to a production Linux VPS using Docker with **full multi-tenancy support**.

**Multi-Tenancy Features:**
- Invitation-based user registration (self-registration disabled in production)
- Per-tenant data isolation (database and file system)
- Storage quotas per tenant
- Audit logging for all sensitive operations
- Granular role-based permissions

## Files

- **`docker-compose.yml`** - Docker Compose stack definition with API, Web, Caddy, and Watchtower services
- **`Caddyfile`** - Caddy reverse proxy configuration for path-based routing with security headers
- **`VPS-SETUP.md`** - Comprehensive step-by-step guide for VPS setup and deployment
- **`FIRST-TIME-SETUP.md`** - Detailed first-time deployment guide with multi-tenancy setup
- **`SECURITY.md`** - Production security configuration and hardening guide with multi-tenant security model

## Quick Start

### 1. Prerequisites

- Linux VPS (Ubuntu/Debian recommended)
- Docker and Docker Compose installed
- GitHub Container Registry images published (via GitHub Actions)

### 2. Initial Setup

```bash
# On VPS: Create directories
sudo mkdir -p /srv/hnh-map /opt/hnhmap
sudo chown $USER:$USER /srv/hnh-map /opt/hnhmap

# Copy files to VPS
scp docker-compose.yml Caddyfile USER@VPS_IP:/opt/hnhmap/

# On VPS: Edit docker-compose.yml
cd /opt/hnhmap
nano docker-compose.yml
# Replace 'OWNER' with your GitHub username
```

### 3. Deploy

```bash
# Pull and start services
docker compose pull
docker compose up -d

# Open firewall
sudo ufw allow 80/tcp

# Check logs
docker compose logs -f
```

### 4. Access and First-Time Setup

Navigate to `http://YOUR_VPS_IP` in your browser.

**Default login:**
- Username: `admin`
- Password: `admin123!`

**⚠️ Change the password immediately after first login!**

**Multi-Tenant First-Time Setup:**
1. **Change admin password** (Admin → Account → Change Password)
2. **Create invitation codes** for new users (Admin → Invitations → Create Invitation)
3. **Share invitation codes** with users who need access
4. **Approve new users** after they register (Admin → Pending Users → Approve)
5. **Assign permissions** to users (Map, Markers, Pointer, Upload, Writer)
6. **Generate tokens** for game client users (Admin → Tokens → Create Token)

**For detailed first-time setup:** See [FIRST-TIME-SETUP.md](FIRST-TIME-SETUP.md)

## Architecture

```
Internet (HTTP :80)
    ↓
Caddy Reverse Proxy
    ├── /client/* → API Service (game client endpoints)
    ├── /map/api/* → API Service (map viewer API)
    ├── /map/updates → API Service (SSE real-time updates)
    ├── /map/grids/* → API Service (tile images)
    ├── /admin/* → API Service (admin API endpoints)
    ├── /admin → Web Service (admin panel UI)
    └── /* → Web Service (default, login, dashboard)
```

Both API and Web services share `/srv/hnh-map` for:
- SQLite database with multi-tenant tables (`grids.db`)
- Tenant-isolated tile storage (`tenants/{tenantId}/grids/`)
- Cookie encryption keys (`DataProtection-Keys/`)

**Multi-Tenant Storage Structure:**
```
/srv/hnh-map/
├── grids.db                    # SQLite database with multi-tenant tables
├── DataProtection-Keys/        # Cookie encryption keys
└── tenants/                    # Tenant-isolated file storage
    ├── default-tenant-1/grids/ # Default tenant tiles
    └── {other-tenants}/grids/  # Additional tenant tiles
```

## Services

| Service | Container Name | Purpose | Exposed Port |
|---------|---------------|---------|--------------|
| **api** | hnhm-api | Game client APIs, map endpoints, admin APIs | Internal only |
| **web** | hnhm-web | Blazor UI (login, dashboard, admin panel) | Internal only |
| **caddy** | hnhm-caddy | Reverse proxy with path-based routing | 80 (HTTP) |
| **watchtower** | hnhm-watchtower | Automatic container updates from GHCR | N/A |

## Multi-Tenancy

**The application is fully multi-tenant** with complete data isolation between tenants.

### Default Tenant

On first deployment:
- Default tenant `default-tenant-1` is automatically created
- Bootstrap admin assigned to this tenant with TenantAdmin role
- All permissions granted (Map, Markers, Pointer, Upload, Writer)

### User Registration

**Self-registration is disabled in production.** New users must:
1. Receive an invitation code from a TenantAdmin
2. Register at `/register` with the invitation code
3. Wait for admin approval (appear in "Pending Users" tab)
4. Admin approves and assigns permissions

### Roles and Permissions

**Roles:**
- **TenantAdmin:** Manage users, tokens, and invitations within their tenant
- **TenantUser:** Standard user with granular permissions

**Permissions:**
- **Map:** View maps
- **Markers:** View and create markers
- **Pointer:** View character positions
- **Upload:** Upload tiles via game client (required for game client users)
- **Writer:** Edit/delete tiles and markers (admin-level permission)

### Token Format

Game client tokens include the tenant ID prefix:
```
Format: {tenantId}_{secret}
Example: default-tenant-1_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6
```

Tokens are generated in the Admin panel → Tokens tab.

### Storage Quotas

Each tenant has a storage quota (default: 1024 MB):
- Real-time tracking of tile storage usage
- Upload rejected when quota exceeded (HTTP 413)
- Quotas adjustable in Admin panel → Config tab

### Audit Logging

All sensitive operations are logged:
- User creation, deletion, role changes
- Permission grants/revokes
- Token creation/revocation
- Invitation creation/usage
- Admin panel actions

**Access audit logs:** Admin panel → Audit Logs tab (tenant-scoped)

## Auto-Updates

Watchtower monitors GHCR for new `:main` tagged images and automatically updates containers every 60 seconds.

**To trigger an update:**
1. Push changes to GitHub `main` branch
2. GitHub Actions builds and pushes new images to GHCR
3. Watchtower detects new images and updates containers
4. Services restart automatically with zero downtime

**Manual update:**
```bash
docker compose pull
docker compose up -d
```

## GHCR Authentication

### Public Images (Recommended)

Make your GHCR packages public:
- Go to https://github.com/YOURUSERNAME?tab=packages
- Select package → Settings → Change visibility → Public

No authentication needed.

### Private Images

1. Create GitHub Personal Access Token with `read:packages` scope
2. Login to GHCR on VPS:
   ```bash
   echo YOUR_PAT | docker login ghcr.io -u YOUR_USERNAME --password-stdin
   ```
3. Uncomment `WATCHTOWER_REGISTRY_AUTH=true` in `docker-compose.yml`

## Backups

### Automated Backups (Recommended)

```bash
# Edit crontab
crontab -e

# Nightly database backup at 2 AM
0 2 * * * sqlite3 /srv/hnh-map/grids.db ".backup '/srv/hnh-map/backups/grids-$(date +\%F).db'"

# Cleanup old backups (30 days) at 3 AM
0 3 * * * find /srv/hnh-map/backups -name "grids-*.db" -mtime +30 -delete

# Weekly tenant storage backup (Sundays at 3 AM)
0 3 * * 0 tar -czf /srv/hnh-map/backups/tenant-storage-$(date +\%F).tar.gz -C /srv/hnh-map tenants/
```

**Note:** Multi-tenant version stores tiles in `/srv/hnh-map/tenants/{tenantId}/grids/`

### Manual Backup

```bash
# Database only
sqlite3 /srv/hnh-map/grids.db ".backup '/srv/hnh-map/backups/manual-$(date +%F).db'"

# Tenant storage only
tar -czf /srv/hnh-map/backups/tenant-storage-$(date +%F).tar.gz -C /srv/hnh-map tenants/

# Full backup (database + tenant storage + keys)
tar -czf backup-$(date +%F).tar.gz -C /srv hnh-map/
```

## Security

### Essential Steps

1. **Change admin password** immediately after first login
2. **Enable firewall:**
   ```bash
   sudo ufw enable
   sudo ufw allow ssh
   sudo ufw allow 80/tcp
   ```
3. **Set permissions:**
   ```bash
   chmod 750 /srv/hnh-map
   chmod 640 /srv/hnh-map/grids.db
   ```
4. **Enable auto-updates:**
   ```bash
   sudo apt install -y unattended-upgrades
   sudo dpkg-reconfigure -plow unattended-upgrades
   ```
5. **Review security settings:** See [SECURITY.md](SECURITY.md) for detailed security configuration

## Maintenance

### View Logs
```bash
docker compose logs -f           # All services
docker compose logs -f api       # API only
docker compose logs -f web       # Web only
```

### Restart Services
```bash
docker compose restart           # All services
docker compose restart api       # API only
```

### Stop Services
```bash
docker compose down              # Stop all
docker compose up -d             # Start all
```

### Database Maintenance
```bash
# Check integrity
sqlite3 /srv/hnh-map/grids.db "PRAGMA integrity_check;"

# Optimize
sqlite3 /srv/hnh-map/grids.db "VACUUM;"
```

### Disk Cleanup
```bash
# Check usage
df -h
docker system df

# Clean up unused Docker resources
docker system prune -a
```

## Systemd Service (Auto-start on Boot)

Create `/etc/systemd/system/hnhmap.service`:

```ini
[Unit]
Description=HnH Mapper Docker Stack
After=docker.service
Requires=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/opt/hnhmap
ExecStart=/usr/bin/docker compose up -d
ExecStop=/usr/bin/docker compose down

[Install]
WantedBy=multi-user.target
```

Enable:
```bash
sudo systemctl daemon-reload
sudo systemctl enable hnhmap.service
sudo systemctl start hnhmap.service
```

## Future Domain Setup

When you get a domain:

1. Point DNS A record to VPS IP
2. Edit `Caddyfile`: replace `:80 {` with `yourdomain.com {`
3. Restart Caddy: `docker compose restart caddy`
4. Caddy automatically provisions HTTPS with Let's Encrypt

No other changes needed!

## Troubleshooting

### Containers won't start
```bash
docker compose logs api
docker compose logs web
```

### Login not working
- Check both API and Web are running: `docker compose ps`
- Verify shared volume: `ls -la /srv/hnh-map/DataProtection-Keys/`

### Watchtower not updating
```bash
docker compose logs watchtower
docker pull ghcr.io/YOUR_USERNAME/hnhmapper-api:main
```

### Database locked
```bash
lsof /srv/hnh-map/grids.db
docker compose restart
```

## Documentation

**Deployment Guides:**
- **`FIRST-TIME-SETUP.md`** - Step-by-step first-time deployment guide with multi-tenancy setup
- **`VPS-SETUP.md`** - Comprehensive VPS setup and deployment guide
- **`SECURITY.md`** - Security configuration and hardening guide

**Project Documentation:**
- `README.md` - Project overview
- `CLAUDE.md` - Technical documentation for AI assistants
- `DEPLOYMENT.md` - Deployment architecture and CI/CD overview
- `DATABASE_SCHEMA.md` - Complete database schema documentation
- `MULTI_TENANCY_DESIGN.md` - Multi-tenancy architecture design

## Support

If you encounter issues:
1. Check logs: `docker compose logs -f`
2. Verify firewall: `sudo ufw status`
3. Check disk space: `df -h`
4. Review `VPS-SETUP.md` for missed steps

