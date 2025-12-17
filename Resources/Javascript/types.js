/**
 * @fileoverview Central JSDoc type definitions for the project.
 */

/**
 * @typedef {object} StarData
 * @description Data structure for a single star entry in the catalog (446 entries total).
 * @property {string} ConstellationName - The star's constellation name (e.g., "Pegasus").
 * @property {string} CatalogID - Unique star ID (e.g., "Peg5"), used for constellation lines.
 * @property {string} ShaIndex - Official 1-57 Sidereal Hour Angle (SHA) index for navigation stars. Empty string ("") otherwise.
 * @property {string} NavName - Navigation star name (e.g., "Alpheratz"). Empty string ("") otherwise.
 * @property {string} BayerDesignation - Bayer designation character.
 * @property {number} RaH - Right Ascension (Hours).
 * @property {number} RaM - Right Ascension (Minutes).
 * @property {number} RaS - Right Ascension (Seconds).
 * @property {number} DecD - Declination (Signed Degrees).
 * @property {number} DecM - Declination (Minutes).
 * @property {number} DecS - Declination (Seconds).
 * @property {number} VisualMagnitude - Visual Magnitude.
 * @export
 */

/**
 * @typedef {object} NavStarData
 * @description Data structure for a single navigational star's precise coordinates and name.
 * @property {number} SHADegrees - Sidereal Hour Angle (SHA) Degrees.
 * @property {number} SHAMinutes - Sidereal Hour Angle (SHA) Minutes.
 * @property {number} DECdegrees - Declination (DEC) Signed Degrees.
 * @property {number} DECMinutes - Declination (DEC) Minutes.
 * @property {string} NavStarName - The navigational star's common name (e.g., "Alpheratz").
 * @export
 */

/**
 * @typedef {object} PlotArea
 * @description The four edges defining the bounds of the navigational plotting area, all in radians.
 * @property {number} north - North edge of the area (radians).
 * @property {number} east - East edge of the area (radians).
 * @property {number} south - South edge of the area (radians).
 * @property {number} west - West edge of the area (radians).
 * @export
 */

/**
 * @typedef {object} AriesGHAData
 * @description Consolidated structure for 3 days x 24 hours of Aries GHA data.
 * @property {number[][]} Degrees - Two-dimensional array of GHA Degrees [Day][Hour].
 * @property {number[][]} Minutes - Two-dimensional array of GHA Minutes [Day][Hour].
 * @export
 */

/**
 * @typedef {object} PlaneStatus
 * @description Stores the primary navigational state of the aircraft.
 * @property {number} speedKnots - The current ground velocity of the plane (Knots).
 * @property {number | null} headingTrueRad - Current true heading of the plane (radians).
 * @property {CoordPair} position - Current latitude and longitude in radians.
 * @property {number | null} magVarRad - Magnetic Variation at the plane's location (radians).
 * @export
 */

/**
 * @typedef {object} SextantView
 * @description State for the sextant's view configuration.
 * @property {number} fovH - Horizontal field of view of sky (degrees).
 * @property {number} fovV - Vertical field of view of sky (degrees).
 * @property {number} azTrueDeg - Sextant window mid-point bearing - TRUE (degrees).
 * @property {number} altDeg - Sextant window base elevation (degrees).
 * @property {number} centerPixelY - Sextant view center line (Y-axis pixel coordinate, measured from top).
 * @export
 */

/**
 * @typedef {object} StarDisplayConfig
 * @description Toggles for various elements displayed on the starmap.
 * @property {number} labelStars - Whether to show star labels (0=off, 1=on).
 * @property {number} drawConstellations - Whether to show constellation lines (0=off, 1=on).
 * @property {number} labelConstellations - Whether to show constellation labels (0=off, 1=on).
 * @export
 */

/**
 * @typedef {object} PlotDisplayConfig
 * @description Configuration settings for displaying elements on the Navigation Plotting tab.
 * @property {number} showFinalLeg - Whether to show the final leg of the flight (0 or 1).
 * @property {number} plotPlane - Whether to show the plane marker on the plot (0 or 1).
 * @export
 */

/**
 * @typedef {object} InputControl
 * @description Flags used by event handlers to signal continuous control input for the next update cycle.
 * @property {number} azimuth - Control variable for changing the sextant azimuth.
 * @property {number} altitude - Control variable for changing the sextant altitude.
 * @property {number} fieldOfView - Control variable for changing the field of view.
 * @export
 */

/**
 * @typedef {object} SightCalculationResult
 * @description The calculated theoretical Altitude and Azimuth for a celestial sight.
 * @property {number} HcRad - Calculated Altitude (Hc) in Radians.
 * @property {number} ZnRad - Calculated Azimuth (Zn) in Radians.
 * @export
 */

/**
 * @typedef {object} SightingLOPData
 * @description Stores all calculated and derived data necessary to plot a single Line of Position (LOP).
 * @property {number} ZnRad - The calculated true Azimuth (Zn) of the celestial body from the AP, in radians. 
 * @property {CoordPair} interceptPointCoord - The geographical coordinate (lat/lon in radians) of the point used to define the LOP.
 * @property {PixelPosition} interceptPointPixels - The pixel coordinates (top/left) for plotting the LOP intercept point.
 * @export
 */

/**
 * @typedef {object} FixResult
 * @description Stores the calculated data for a multi-sight navigational Fix (Estimated Position) derived from three intersecting Lines of Position (LOPs).
 * @property {Array<CoordPair>} cockedHatVerticesCoord - An array of the three geographical coordinates (lat/lon in radians) that form the vertices of the 'Cocked Hat' (LOP intersection points).
 * @property {Array<PixelPosition>} cockedHatVerticesPixels - An array of the three pixel coordinates (top/left) for the Cocked Hat vertices.
 * @property {PixelPosition} fixPositionPixels - The pixel coordinates (top/left) for the final calculated Fix Position (e.g., the Centroid).
 * @property {CoordPair} fixPositionCoord - The coordinate for the final calculated Fix Position (e.g., the Centroid).
 * @export
 */

/**
 * @typedef {object} CoordPair
 * @description Latitude and Longitude pair in radians.
 * @property {number} latitude - Latitude in radians.
 * @property {number} longitude - Longitude in radians.
 * @export
 */

/**
 * @typedef {object} PixelPosition
 * @description Coordinates based on a Top-Left origin (standard canvas/DOM).
 * @property {number} left - The horizontal pixel coordinate (distance from left edge).
 * @property {number} top - The vertical pixel coordinate (distance from top edge).
 * @export
 */

/**
 * @typedef {number[]} IntersectionList
 * @description A flat array of coordinates representing line intersections.
 * @export
 */

/**
 * @typedef {object} VisibleStarData
 * @description A structured object representing a star currently visible in the sextant FOV.
 * @property {string} starCatalogId - Unique star ID (from starCatalogIDs).
 * @property {number} visMag - Visual Magnitude.
 * @property {number} leftPixel - Calculated X-coordinate on canvas.
 * @property {number} topPixel - Calculated Y-coordinate on canvas.
 * @property {string} shaIndex - Official SHA index (from starSHAIndex).
 * @property {string} navName - Navigation star name (from starNavNamesMap).
 * @property {string} bayerDesignation - Bayer designation character.
 * @property {string} constellationName - Full constellation name.
 * @export
 */

// This line remains necessary at the bottom of types.js to make it an ES Module.
export { };