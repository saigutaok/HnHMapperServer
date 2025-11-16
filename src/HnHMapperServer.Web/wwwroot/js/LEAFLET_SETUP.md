# Leaflet Setup Instructions

## Required Libraries

Download and place the following files in the `/wwwroot/js/` and `/wwwroot/css/` directories:

### 1. Leaflet Core (v1.9.4)
- **Download from**: https://leafletjs.com/download.html
- **Files needed**:
  - `leaflet.min.js` → place in `/wwwroot/js/`
  - `leaflet.css` → place in `/wwwroot/css/`
  - `images/` folder → place in `/wwwroot/css/images/` (marker icons, etc.)

### 2. Leaflet.RotatedMarker Plugin
- **Download from**: https://github.com/bbecquet/Leaflet.RotatedMarker
- **File needed**:
  - `leaflet.rotatedMarker.js` → place in `/wwwroot/js/`

### 3. Leaflet.Marker.SlideTo Plugin
- **Download from**: https://github.com/ewoken/Leaflet.Marker.SlideTo
- **File needed**:
  - `Leaflet.Marker.SlideTo.js` → place in `/wwwroot/js/`

## Quick Download Commands

```bash
# From HnHMapperServer/src/HnHMapperServer.Web/wwwroot directory

# Download Leaflet 1.9.4
cd js
curl -O https://unpkg.com/leaflet@1.9.4/dist/leaflet.js
cd ../css
curl -O https://unpkg.com/leaflet@1.9.4/dist/leaflet.css

# Download rotated marker plugin
cd ../js
curl -O https://raw.githubusercontent.com/bbecquet/Leaflet.RotatedMarker/master/leaflet.rotatedMarker.js

# Download slideto plugin
curl -O https://raw.githubusercontent.com/ewoken/Leaflet.Marker.SlideTo/master/Leaflet.Marker.SlideTo.js
```

## Verification

After downloading, your wwwroot structure should look like:

```
wwwroot/
├── js/
│   ├── leaflet.js (or leaflet.min.js)
│   ├── leaflet.rotatedMarker.js
│   ├── Leaflet.Marker.SlideTo.js
│   └── leaflet-interop.js (custom - already created)
├── css/
│   ├── leaflet.css
│   └── images/
│       ├── marker-icon.png
│       ├── marker-shadow.png
│       └── ...
```
