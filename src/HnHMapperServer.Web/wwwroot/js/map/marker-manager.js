// Marker Manager Module
// Handles game marker (resource, quest, etc.) management

import { HnHMaxZoom } from './leaflet-config.js';

// Marker storage - visible markers with their Leaflet instances
const markers = {};
// All marker data storage - persists regardless of visibility for re-evaluation
const allMarkerData = {};
let currentMapId = 0;
let markerLayer = null;
let detailedMarkerLayer = null;

// Hidden marker types (by image path)
const hiddenMarkerTypes = new Set();

// Thingwall highlighting state
let thingwallHighlightEnabled = false;

// Quest giver highlighting state
let questGiverHighlightEnabled = false;

// Marker filter mode state (hide all markers except highlighted ones)
let markerFilterModeEnabled = false;

// Safely invoke .NET methods from JS
let invokeDotNetSafe = null;

/**
 * Initialize marker manager
 * @param {function} invokeFunc - Function to invoke .NET methods
 */
export function initializeMarkerManager(invokeFunc) {
    invokeDotNetSafe = invokeFunc;
}

/**
 * Set marker layers
 * @param {object} mainLayer - Main marker layer
 * @param {object} detailLayer - Detailed marker layer (for high zoom)
 */
export function setMarkerLayers(mainLayer, detailLayer) {
    markerLayer = mainLayer;
    detailedMarkerLayer = detailLayer;
}

/**
 * Set the current map ID for marker filtering
 */
export function setCurrentMapId(mapId) {
    currentMapId = mapId;
}

/**
 * Set hidden marker types (by image path)
 * @param {Array<string>} types - Array of image paths to hide
 * @param {object} mapInstance - Leaflet map instance (optional, for immediate refresh)
 */
export function setHiddenMarkerTypes(types, mapInstance) {
    hiddenMarkerTypes.clear();
    if (types && Array.isArray(types)) {
        types.forEach(t => hiddenMarkerTypes.add(t));
    }

    // If map instance provided, refresh marker visibility
    if (mapInstance) {
        refreshMarkerVisibility(mapInstance);
    }
}

/**
 * Get current hidden marker types
 * @returns {Array<string>} - Array of hidden image paths
 */
export function getHiddenMarkerTypes() {
    return Array.from(hiddenMarkerTypes);
}

/**
 * Refresh marker visibility based on current hidden types
 * Removes markers that should be hidden, keeps visible ones
 * @param {object} mapInstance - Leaflet map instance
 */
export function refreshMarkerVisibility(mapInstance) {
    // Remove markers whose type is now hidden
    const idsToRemove = [];
    Object.keys(markers).forEach(id => {
        const mark = markers[id];
        if (hiddenMarkerTypes.has(mark.data.image)) {
            // Remove from the appropriate layer group
            if (markerLayer && markerLayer.hasLayer(mark.marker)) {
                markerLayer.removeLayer(mark.marker);
            }
            if (detailedMarkerLayer && detailedMarkerLayer.hasLayer(mark.marker)) {
                detailedMarkerLayer.removeLayer(mark.marker);
            }
            idsToRemove.push(id);
        }
    });
    // Clean up markers object
    idsToRemove.forEach(id => delete markers[id]);
}

/**
 * Add a marker to the map
 * @param {object} markerData - Marker data with id, name, image, position, type, ready, minReady, maxReady, hidden
 * @param {object} mapInstance - Leaflet map instance
 * @param {boolean} skipStorage - If true, don't store in allMarkerData (used during rebuilds)
 * @returns {boolean} - True if marker was added
 */
export function addMarker(markerData, mapInstance, skipStorage = false) {
    // Only show markers on their own map
    if (markerData.map !== currentMapId) {
        return false;
    }

    // Store marker data for later re-evaluation (unless this is a rebuild)
    if (!skipStorage) {
        allMarkerData[markerData.id] = markerData;
    }

    if (markers[markerData.id]) {
        return false;
    }

    if (markerData.hidden) {
        return false;
    }

    // Skip markers whose type is hidden by user preference
    if (hiddenMarkerTypes.has(markerData.image)) {
        return false;
    }

    const iconUrl = `${markerData.image}.png`;
    const isCustom = markerData.image === "gfx/terobjs/mm/custom";
    const isCave = markerData.name.toLowerCase() === "cave";
    const isThingwall = markerData.type === "thingwall";
    const isQuestGiver = markerData.type === "questgiver";
    const shouldHighlightThingwall = isThingwall && thingwallHighlightEnabled;
    const shouldHighlightQuestGiver = isQuestGiver && questGiverHighlightEnabled;
    const shouldHighlight = shouldHighlightThingwall || shouldHighlightQuestGiver;

    // Marker filter mode: hide all markers except highlighted ones
    if (markerFilterModeEnabled) {
        if (!shouldHighlight) {
            return false; // Skip non-highlighted markers when filter mode is active
        }
    }

    // Use larger icons for highlighted thingwalls (48px vs 36px default)
    let iconSize = shouldHighlight ? [48, 48] : (isCustom && !isCave ? [36, 36] : [36, 36]);
    let iconAnchor = shouldHighlight ? [24, 24] : (isCustom && !isCave ? [11, 21] : [18, 18]);

    let icon;
    if (markerData.timerText) {
        // Use divIcon for markers with timers to allow HTML overlay
        const actualIconUrl = isCave ? 'gfx/hud/mmap/cave.png' : iconUrl;
        icon = L.divIcon({
            html: `
                <div class="marker-with-timer">
                    <img src="${actualIconUrl}" class="marker-icon-img" style="width: ${iconSize[0]}px; height: ${iconSize[1]}px;" />
                    <div class="marker-timer-text">${markerData.timerText}</div>
                </div>
            `,
            iconSize: [iconSize[0], iconSize[1] + 12], // Add 12px for timer text
            iconAnchor: [iconAnchor[0], iconAnchor[1] + 12],
            popupAnchor: [0, -(iconAnchor[1] + 12)],
            className: 'marker-timer-container'
        });
    } else {
        // Use regular icon for markers without timers
        icon = L.icon({
            iconUrl: isCave ? 'gfx/hud/mmap/cave.png' : iconUrl,
            iconSize: iconSize,
            iconAnchor: iconAnchor
        });
    }

    const position = mapInstance.unproject([markerData.position.x, markerData.position.y], HnHMaxZoom);
    const marker = L.marker(position, { icon: icon, riseOnHover: true });

    const color = getMarkerColor(markerData.type);
    const extra = getMarkerReadyText(markerData);

    // Determine tooltip and highlight class based on marker type
    const tooltipClass = shouldHighlightThingwall ? 'thingwall-label' :
                         shouldHighlightQuestGiver ? 'questgiver-label' : '';
    const highlightClass = shouldHighlightThingwall ? 'thingwall-highlighted' :
                           shouldHighlightQuestGiver ? 'questgiver-highlighted' : '';

    marker.bindTooltip(`<div style='color:${color};'><b>${markerData.name} ${extra}</b></div>`, {
        permanent: shouldHighlight,
        direction: 'top',
        sticky: true,
        opacity: 0.9,
        className: tooltipClass
    });

    // Add highlight class to marker element if highlighting is enabled
    if (shouldHighlight && highlightClass) {
        marker.on('add', () => {
            const el = marker.getElement();
            if (el) {
                el.classList.add(highlightClass);
            }
        });
    }

    marker.on('click', () => {
        invokeDotNetSafe('JsOnMarkerClicked', markerData.id);
    });

    marker.on('contextmenu', (e) => {
        L.DomEvent.stopPropagation(e);
        // Pass screen coordinates for proper context menu positioning
        const screenX = Math.floor(e.containerPoint.x);
        const screenY = Math.floor(e.containerPoint.y);
        invokeDotNetSafe('JsOnMarkerContextMenu', markerData.id, screenX, screenY);
    });

    // Add to appropriate layer
    if (markerData.image === "gfx/hud/mmap/cave" || markerData.image === "gfx/terobjs/mm/burrow") {
        marker.addTo(detailedMarkerLayer);
    } else {
        marker.addTo(markerLayer);
    }

    markers[markerData.id] = {
        marker: marker,
        data: markerData
    };

    return true;
}

/**
 * Add multiple markers to the map in a single batch (performance optimization)
 * @param {Array} markersData - Array of marker data objects
 * @param {object} mapInstance - Leaflet map instance
 * @returns {object} - Result with counts: { added, skipped }
 */
export function addMarkersBatch(markersData, mapInstance) {
    let added = 0;
    let skipped = 0;

    for (const markerData of markersData) {
        if (addMarker(markerData, mapInstance)) {
            added++;
        } else {
            skipped++;
        }
    }

    return { added, skipped };
}

/**
 * Update an existing marker
 * @param {number} markerId - Marker ID
 * @param {object} markerData - Updated marker data
 * @param {object} mapInstance - Leaflet map instance
 * @returns {boolean} - True if marker was updated
 */
export function updateMarker(markerId, markerData, mapInstance) {
    const mark = markers[markerId];
    if (mark) {
        // Remove marker if it's no longer on the current map
        if (markerData.map !== currentMapId) {
            removeMarker(markerId, mapInstance);
            return false;
        }

        // Check if timer state has changed (added, removed, or text changed)
        const oldTimerText = mark.data.timerText || null;
        const newTimerText = markerData.timerText || null;

        if (oldTimerText !== newTimerText) {
            // Timer state changed - need to recreate marker with new icon
            // Remove old marker and add new one with updated timer border/text
            removeMarker(markerId, mapInstance);
            addMarker(markerData, mapInstance);
            return true;
        }

        // Update tooltip
        const extra = getMarkerReadyText(markerData);
        const color = getMarkerColor(markerData.type);
        mark.marker.setTooltipContent(`<div style='color:${color};'><b>${markerData.name} ${extra}</b></div>`);

        // Update position (in case coordinates changed)
        const position = mapInstance.unproject([markerData.position.x, markerData.position.y], HnHMaxZoom);
        mark.marker.setLatLng(position);

        mark.data = markerData;
        return true;
    }
    return false;
}

/**
 * Remove a marker from the map
 * @param {number} markerId - Marker ID to remove
 * @param {object} mapInstance - Leaflet map instance
 * @returns {boolean} - True if marker was removed
 */
export function removeMarker(markerId, mapInstance) {
    const mark = markers[markerId];
    if (mark) {
        mapInstance.removeLayer(mark.marker);
        delete markers[markerId];
        return true;
    }
    return false;
}

/**
 * Clear all markers from the map
 * @param {object} mapInstance - Leaflet map instance
 * @returns {boolean} - Always true
 */
export function clearAllMarkers(mapInstance) {
    Object.keys(markers).forEach(id => removeMarker(parseInt(id), mapInstance));
    // Also clear stored marker data
    Object.keys(allMarkerData).forEach(id => delete allMarkerData[id]);
    return true;
}

/**
 * Rebuild all markers from stored data based on current visibility settings
 * Used when filter mode or highlight toggles change
 * @param {object} mapInstance - Leaflet map instance
 */
function rebuildAllMarkers(mapInstance) {
    // Remove all visible markers from the map
    Object.keys(markers).forEach(id => {
        const mark = markers[id];
        if (markerLayer && markerLayer.hasLayer(mark.marker)) {
            markerLayer.removeLayer(mark.marker);
        }
        if (detailedMarkerLayer && detailedMarkerLayer.hasLayer(mark.marker)) {
            detailedMarkerLayer.removeLayer(mark.marker);
        }
        mapInstance.removeLayer(mark.marker);
    });

    // Clear markers object
    Object.keys(markers).forEach(id => delete markers[id]);

    // Re-add all markers from stored data (with skipStorage=true to avoid re-storing)
    Object.values(allMarkerData).forEach(markerData => {
        addMarker(markerData, mapInstance, true);
    });
}

/**
 * Toggle marker tooltips by type
 * @param {string} type - Marker type (quest, thingwall, etc.)
 * @param {boolean} visible - Whether to show tooltips
 * @returns {boolean} - Always true
 */
export function toggleMarkerTooltips(type, visible) {
    Object.values(markers).forEach(mark => {
        if (mark.data.type === type) {
            if (visible) {
                mark.marker.openTooltip();
            } else {
                mark.marker.closeTooltip();
            }
        }
    });
    return true;
}

/**
 * Jump map view to a marker
 * @param {number} markerId - Marker ID to jump to
 * @param {object} mapInstance - Leaflet map instance
 * @returns {boolean} - True if marker was found
 */
export function jumpToMarker(markerId, mapInstance) {
    const mark = markers[markerId];
    if (mark) {
        const latlng = mark.marker.getLatLng();
        mapInstance.setView(latlng, Math.max(mapInstance.getZoom(), 5));
        return true;
    }
    return false;
}

/**
 * Enable/disable thingwall highlighting with glow effect and permanent labels
 * @param {boolean} enabled - Whether highlighting is enabled
 * @param {object} mapInstance - Leaflet map instance
 * @returns {boolean} - Always true
 */
export function setThingwallHighlightEnabled(enabled, mapInstance) {
    if (!mapInstance) {
        console.warn('[MarkerManager] Cannot toggle thingwall highlight - mapInstance is null');
        return false;
    }

    if (thingwallHighlightEnabled === enabled) {
        return true; // No change needed
    }

    thingwallHighlightEnabled = enabled;

    // Rebuild all markers to apply new visibility/highlighting rules
    rebuildAllMarkers(mapInstance);

    console.log(`[MarkerManager] Thingwall highlighting ${enabled ? 'enabled' : 'disabled'}`);
    return true;
}

/**
 * Enable/disable quest giver highlighting with glow effect and permanent labels
 * @param {boolean} enabled - Whether highlighting is enabled
 * @param {object} mapInstance - Leaflet map instance
 * @returns {boolean} - Always true
 */
export function setQuestGiverHighlightEnabled(enabled, mapInstance) {
    if (!mapInstance) {
        console.warn('[MarkerManager] Cannot toggle quest giver highlight - mapInstance is null');
        return false;
    }

    if (questGiverHighlightEnabled === enabled) {
        return true; // No change needed
    }

    questGiverHighlightEnabled = enabled;

    // Rebuild all markers to apply new visibility/highlighting rules
    rebuildAllMarkers(mapInstance);

    console.log(`[MarkerManager] Quest giver highlighting ${enabled ? 'enabled' : 'disabled'}`);
    return true;
}

/**
 * Get whether quest giver highlighting is enabled
 * @returns {boolean} - True if quest giver highlighting is enabled
 */
export function isQuestGiverHighlightEnabled() {
    return questGiverHighlightEnabled;
}

/**
 * Enable/disable marker filter mode
 * When enabled, hides all markers except those with active highlights (thingwalls, quest givers)
 * @param {boolean} enabled - Whether filter mode is enabled
 * @param {object} mapInstance - Leaflet map instance
 * @returns {boolean} - Always true
 */
export function setMarkerFilterModeEnabled(enabled, mapInstance) {
    if (!mapInstance) {
        console.warn('[MarkerManager] Cannot toggle marker filter mode - mapInstance is null');
        return false;
    }

    if (markerFilterModeEnabled === enabled) {
        return true; // No change needed
    }

    markerFilterModeEnabled = enabled;

    // Rebuild all markers to apply new visibility/highlighting rules
    rebuildAllMarkers(mapInstance);

    console.log(`[MarkerManager] Marker filter mode ${enabled ? 'enabled' : 'disabled'}`);
    return true;
}

/**
 * Get whether marker filter mode is enabled
 * @returns {boolean} - True if marker filter mode is enabled
 */
export function isMarkerFilterModeEnabled() {
    return markerFilterModeEnabled;
}

// Helper functions

function getMarkerColor(type) {
    if (type === "questgiver") return "#2CDB2C"; // Green for quest givers
    if (type === "thingwall") return "#00cffd";
    return "#FFF";
}

function getMarkerReadyText(markerData) {
    if (markerData.ready) {
        return "READY";
    }
    if (markerData.maxReady !== -1 && markerData.minReady !== -1) {
        const now = Date.now();
        const minTime = msToTimeStr(markerData.minReady - now);
        const maxTime = msToTimeStr(markerData.maxReady - now);
        return `(Ready in [${minTime}] to [${maxTime}])`;
    }
    return "";
}

function msToTimeStr(duration) {
    if (duration < 0) duration = 0;

    const seconds = Math.floor((duration / 1000) % 60);
    const minutes = Math.floor((duration / (1000 * 60)) % 60);
    const hours = Math.floor((duration / (1000 * 60 * 60)) % 24);
    const days = Math.floor(duration / (1000 * 60 * 60 * 24));

    const d = days > 0 ? `${days}d` : "";
    const h = (days > 0 || hours > 0) ? `${hours}h ` : "";
    const m = (days <= 0 && (hours > 0 || minutes > 0)) ? `${minutes}m ` : "";
    const s = (days <= 0 && hours <= 0) ? `${seconds}s` : "";

    return d + h + m + s;
}
