# Caching Guide for HnH Mapper Server

This guide provides recommendations for configuring reverse proxy and CDN caching to maximize performance and minimize server load.

## Overview

The HnH Mapper Server implements a comprehensive caching strategy across multiple layers:
- **Client-side**: Leaflet tile layer with 2-minute negative cache TTL
- **Server-side**: HTTP cache headers for both tiles and missing tile responses
- **Proxy/CDN**: (Optional) Shared caching layer for distributed deployments

## Server Cache Headers

### Tile Hits (200 OK)
```
Cache-Control: public, max-age=31536000, immutable
```
- **Public**: Safe for shared caches (CDN, proxy)
- **1 year max-age**: Tiles are immutable once created
- **Immutable**: Prevents conditional requests during freshness period

### Missing Tiles (404 Not Found)
```
Cache-Control: public, max-age=300, stale-while-revalidate=60
```
- **5 minute max-age**: Reduces repeated requests over unmapped areas
- **60 second stale-while-revalidate**: Allows background revalidation
- **Public**: Safe for shared caches

## Reverse Proxy Configuration

### Nginx

```nginx
# Map tile caching
location /map/grids/ {
    proxy_pass http://backend;
    
    # Cache configuration
    proxy_cache tile_cache;
    proxy_cache_key "$request_uri";  # Include ?v= revision param
    proxy_cache_valid 200 365d;      # Cache hits for 1 year
    proxy_cache_valid 404 5m;        # Cache missing tiles for 5 minutes
    proxy_cache_use_stale error timeout updating http_500 http_502 http_503 http_504;
    
    # Headers
    proxy_cache_bypass $http_pragma $http_authorization;
    add_header X-Cache-Status $upstream_cache_status;
    
    # Performance
    proxy_buffering on;
    proxy_buffer_size 4k;
    proxy_buffers 8 4k;
    
    # Forward auth cookies for authentication
    proxy_set_header Cookie $http_cookie;
    proxy_pass_header Set-Cookie;
}

# Cache zone definition (add to http block)
http {
    proxy_cache_path /var/cache/nginx/tiles
        levels=1:2
        keys_zone=tile_cache:100m
        max_size=10g
        inactive=365d
        use_temp_path=off;
}
```

### Apache (mod_cache)

```apache
<Location /map/grids/>
    # Enable caching
    CacheEnable disk
    CacheRoot /var/cache/apache2/tiles
    
    # Cache hits for 1 year
    CacheMaxExpire 31536000
    
    # Cache 404s for 5 minutes
    CacheStoreExpired On
    CacheIgnoreNoLastMod On
    
    # Include query string in cache key (for ?v= revision)
    CacheKeyBaseURL Off
    
    # Forward authentication
    ProxyPreserveHost On
    Header edit Set-Cookie ^(.*)$ $1;path=/;SameSite=Lax
</Location>
```

### Caddy

```caddy
example.com {
    # Tile caching with automatic HTTPS
    route /map/grids/* {
        # Forward authentication
        header_up Cookie {http.request.header.Cookie}
        
        # Cache configuration via response headers (Caddy respects Cache-Control)
        reverse_proxy localhost:5000 {
            header_up X-Forwarded-Proto {scheme}
            header_up X-Forwarded-Host {host}
        }
    }
}
```

## CDN Configuration

### Cloudflare

**Cache Rules** (under Rules → Cache Rules):

1. **Tile Hits Rule**
   - **Match**: URL Path contains `/map/grids/` AND Status Code equals `200`
   - **Cache Everything**: On
   - **Edge Cache TTL**: 1 year
   - **Browser Cache TTL**: Respect origin headers
   - **Cache Key**: Include query string (for `?v=` revision)

2. **Missing Tiles Rule**
   - **Match**: URL Path contains `/map/grids/` AND Status Code equals `404`
   - **Cache Everything**: On
   - **Edge Cache TTL**: 5 minutes
   - **Browser Cache TTL**: Respect origin headers

**Authentication Handling**:
- Cloudflare caches authenticated responses by default (respects `public` cache-control)
- If you need per-user tile filtering, add a `Vary: Cookie` header server-side
- For public maps, no additional configuration needed

### AWS CloudFront

**Behavior Configuration**:

```json
{
  "PathPattern": "/map/grids/*",
  "TargetOriginId": "hnhmapper-origin",
  "ViewerProtocolPolicy": "redirect-to-https",
  "AllowedMethods": ["GET", "HEAD"],
  "CachedMethods": ["GET", "HEAD"],
  "Compress": true,
  "CachePolicyId": "custom-tile-cache-policy",
  "OriginRequestPolicyId": "Managed-AllViewer"
}
```

**Custom Cache Policy** (`custom-tile-cache-policy`):

```json
{
  "Name": "TileCachePolicy",
  "MinTTL": 300,           // 5 minutes (for 404s)
  "MaxTTL": 31536000,      // 1 year (for 200s)
  "DefaultTTL": 86400,     // 1 day
  "ParametersInCacheKeyAndForwardedToOrigin": {
    "EnableAcceptEncodingGzip": true,
    "EnableAcceptEncodingBrotli": true,
    "QueryStringsConfig": {
      "QueryStringBehavior": "all"  // Include ?v= revision param
    },
    "HeadersConfig": {
      "HeaderBehavior": "whitelist",
      "Headers": ["Cookie"]  // Forward auth cookies
    },
    "CookiesConfig": {
      "CookieBehavior": "all"  // Forward all cookies for auth
    }
  }
}
```

## Cache Invalidation

### When Tiles Change

Tiles use a **revision-based cache busting** mechanism via the `?v=` query parameter. When maps are updated:

1. Server increments map revision number
2. SSE pushes revision update to clients
3. Clients request tiles with new `?v=X` parameter
4. Proxy/CDN treats this as a new URL → cache miss → fresh tile

**No manual cache invalidation needed** for tile updates.

### When Server Configuration Changes

If you change tile generation logic or need to force-refresh all tiles:

**Nginx**:
```bash
# Clear all tile cache
rm -rf /var/cache/nginx/tiles/*
nginx -s reload
```

**Cloudflare**:
```bash
# Purge by URL pattern (requires API token)
curl -X POST "https://api.cloudflare.com/client/v4/zones/{zone_id}/purge_cache" \
  -H "Authorization: Bearer {api_token}" \
  -H "Content-Type: application/json" \
  --data '{"files":["https://example.com/map/grids/*"]}'
```

**CloudFront**:
```bash
# Invalidate path pattern
aws cloudfront create-invalidation \
  --distribution-id EDFDVBD632BHDS5 \
  --paths "/map/grids/*"
```

## Performance Considerations

### Cache Size Estimation

**Per Tile**: ~5-15 KB (100x100 PNG)
**Zoom Levels**: 0-6 (7 levels)
**Coverage**: Depends on mapped area

Example: 10,000 grids × 7 zoom levels = 70,000 tiles × 10 KB average = **~700 MB**

### Cache Warm-up

For production deployments, consider pre-warming the cache:

```bash
#!/bin/bash
# Warm up cache for all known tiles
# Fetch from database and request each tile

sqlite3 /path/to/grids.db <<EOF
SELECT DISTINCT MapId, Zoom, CoordX, CoordY FROM Tiles;
EOF | while IFS='|' read -r mapId zoom x y; do
    curl -s "https://example.com/map/grids/$mapId/$zoom/${x}_${y}.png" > /dev/null
    echo "Cached: $mapId/$zoom/${x}_${y}.png"
done
```

## Monitoring

### Key Metrics to Track

1. **Cache Hit Ratio**: Target >90% for established maps
2. **404 Rate**: High 404 rate indicates unmapped areas (expected)
3. **Cache Size**: Monitor disk usage for cache directory
4. **Bandwidth Savings**: Compare origin vs edge traffic

### Nginx Cache Status

```nginx
add_header X-Cache-Status $upstream_cache_status;
```

Possible values:
- `HIT`: Served from cache
- `MISS`: Cache miss, served from origin
- `BYPASS`: Cache bypassed (auth, pragma, etc.)
- `EXPIRED`: Cache expired, revalidating
- `STALE`: Serving stale while revalidating

### Cloudflare Cache Analytics

Dashboard → Analytics → Caching:
- Cached Bandwidth
- Saved Bandwidth
- Cache Hit Ratio by Status Code

## Troubleshooting

### Tiles Not Caching

**Check**:
1. Verify `Cache-Control: public` header is present
2. Ensure proxy config includes query string in cache key
3. Check if `Set-Cookie` or `Vary` headers prevent caching
4. Review proxy error logs for cache storage issues

### Stale Tiles After Update

**Cause**: Browser/proxy cached old tile with old `?v=` revision
**Fix**: SSE should push new revision; if not:
1. Check browser SSE connection status
2. Verify server increments revision on tile updates
3. Hard-refresh browser (Ctrl+F5) as temporary workaround

### 404 Storms

**Symptom**: Excessive 404 requests for same tiles
**Cause**: Client negative cache expired; proxy not caching 404s
**Fix**:
1. Verify proxy caches 404 responses (5-minute TTL)
2. Check client `negativeCacheTTL` is set to 120000 (2 minutes)
3. Consider enabling `ReturnTransparentTilesOnMissing=true` to return 200 instead

## Optional: Transparent Tiles for Missing Areas

To eliminate browser console 404 errors while maintaining cache benefits:

**appsettings.json** (both Web and API):
```json
{
  "ReturnTransparentTilesOnMissing": true
}
```

This returns a 1×1 transparent PNG (200 OK) instead of 404 for missing tiles. Benefits:
- No console errors for unmapped areas
- Same cache headers (5-minute TTL)
- Slightly higher bandwidth (67 bytes per miss vs 0 bytes for 404)

**Proxy configuration remains the same** (cache `proxy_cache_valid 200 5m` instead of `404 5m`).

## Summary

✅ **Server**: Optimized cache headers (1 year for hits, 5 minutes for misses)
✅ **Client**: 2-minute negative cache to reduce request storms
✅ **Proxy/CDN**: Aggressive caching with revision-based invalidation
✅ **Authentication**: Forwarded for user-specific access control
✅ **Performance**: Minimal origin load, <10% cache miss rate in production

For questions or issues, see [CLAUDE.md](CLAUDE.md) or open an issue on GitHub.






