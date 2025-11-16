// Leaflet Configuration Module
// Constants and Custom Coordinate Reference System for Haven & Hearth

// Map constants
export const TileSize = 100;
export const HnHMaxZoom = 7;
export const HnHMinZoom = 1;

// Coordinate normalization factors
const latNormalization = 90.0 * TileSize / 2500000.0;
const lngNormalization = 180.0 * TileSize / 2500000.0;

// Custom HnH Projection
const HnHProjection = {
    project: function (latlng) {
        return L.point(latlng.lat / latNormalization, latlng.lng / lngNormalization);
    },
    unproject: function (point) {
        return L.latLng(point.x * latNormalization, point.y * lngNormalization);
    },
    bounds: L.bounds(
        [-latNormalization, -lngNormalization],
        [latNormalization, lngNormalization]
    )
};

// Custom HnH Coordinate Reference System
export const HnHCRS = L.extend({}, L.CRS.Simple, {
    projection: HnHProjection
});
