/**
 * @typedef {object} StarData
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
 */


/**
 * @typedef {object} CoordPair
 * @property {number} latitude - Latitude in radians.
 * @property {number} longitude - Longitude in radians.
 */

/**
 * @typedef {number[]} IntersectionList
 * @description A flat array of coordinates representing line intersections.
 * @export
 */

/**
 * @typedef {object} NavStarData
 * @property {number} SHADegrees - Sidereal Hour Angle (SHA) Degrees.
 * @property {number} SHAMinutes - Sidereal Hour Angle (SHA) Minutes.
 * @property {number} DECdegrees - Declination (DEC) Signed Degrees.
 * @property {number} DECMinutes - Declination (DEC) Minutes.
 * @property {string} NavStarName - The navigational star's common name (e.g., "Alpheratz").
 */

/**
 * @typedef {object} PlotArea
 * @property {number} north - North edge of the area (radians).
 * @property {number} east - East edge of the area (radians).
 * @property {number} south - South edge of the area (radians).
 * @property {number} west - West edge of the area (radians).
 */

/**
 * @typedef {object} AriesGHAData
 * @property {number[][]} Degrees - Two-dimensional array of GHA Degrees [Day][Hour].
 * @property {number[][]} Minutes - Two-dimensional array of GHA Minutes [Day][Hour].
 */

/**
 * @typedef {object} PlaneStatus
 * @property {number} speedKnots - The current ground velocity of the plane (Knots).
 * @property {number | null} headingTrueRad - Current true heading of the plane (radians).
 * @property {CoordPair} position - Current latitude and longitude in radians.
 * @property {number | null} magVarRad - Magnetic Variation at the plane's location (radians).
 */

/**
 * @typedef {object} SextantView
 * @property {number} fovH - Horizontal field of view of sky (degrees).
 * @property {number} fovV - Vertical field of view of sky (degrees).
 * @property {number} azTrueDeg - Sextant window mid-point bearing - TRUE (degrees).
 * @property {number} altDeg - Sextant window base elevation (degrees).
 * @property {number} centerPixelY - Sextant view center line (Y-axis pixel coordinate, measured from top).
 */

/**
 * @typedef {object} StarDisplayConfig
 * @property {number} labelStars - Whether to show star labels (0=off, 1=on).
 * @property {number} drawConstellations - Whether to show constellation lines (0=off, 1=on).
 * @property {number} labelConstellations - Whether to show constellation labels (0=off, 1=on).
 */

/**
 * @typedef {object} PlotDisplayConfig
 * @property {number} showFinalLeg - Whether to show the final leg of the flight (0 or 1).
 * @property {number} plotPlane - Whether to show the plane marker on the plot (0 or 1).
 * @property {number} plotNeedsUpdate - Whether to redraw the plotting tab, usually only when a new fix is made (0 or 1).
 */

/**
 * @typedef {object} InputControl
 * @property {number} azimuth - Control variable for changing the sextant azimuth.
 * @property {number} altitude - Control variable for changing the sextant altitude.
 * @property {number} fieldOfView - Control variable for changing the field of view.
 */

/**
 * @typedef {object} SightCalculationResult
 * @property {number} HcRad - Calculated Altitude (Hc) in Radians.
 * @property {number} ZnRad - Calculated Azimuth (Zn) in Radians.
 */

/**
 * @typedef {object} SightingLOPData
 * @property {number} ZnRad - The calculated true Azimuth (Zn) of the celestial body from the AP, in radians.
 * @property {CoordPair} interceptPointCoord - The geographical coordinate (lat/lon in radians) of the point used to define the LOP.
 * @property {PixelPosition} interceptPointPixels - The pixel coordinates (top/left) for plotting the LOP intercept point.
 */

/**
 * @typedef {object} PixelPosition
 * @property {number} left - The horizontal pixel coordinate (distance from left edge).
 * @property {number} top - The vertical pixel coordinate (distance from top edge).
 */

/**
 * @typedef {object} FixResult
 * @property {Array<CoordPair>} cockedHatVerticesCoord - An array of the three geographical coordinates (lat/lon in radians) that form the vertices of the 'Cocked Hat' (LOP intersection points).
 * @property {Array<PixelPosition>} cockedHatVerticesPixels - An array of the three pixel coordinates (top/left) for the Cocked Hat vertices.
 * @property {PixelPosition} fixPositionPixels - The pixel coordinates (top/left) for the final calculated Fix Position (e.g., the Centroid).
 * @property {CoordPair} fixPositionCoord - The coordinate for the final calculated Fix Position (e.g., the Centroid).
 */

/**
 * @typedef {object} VisibleStarData
 * @property {string} starCatalogId - Unique star ID (from starCatalogIDs).
 * @property {number} visMag - Visual Magnitude.
 * @property {number} leftPixel - Calculated X-coordinate on canvas.
 * @property {number} topPixel - Calculated Y-coordinate on canvas.
 * @property {string} shaIndex - Official SHA index (from starSHAIndex).
 * @property {string} navName - Navigation star name (from starNavNamesMap).
 * @property {string} bayerDesignation - Bayer designation character.
 * @property {string} constellationName - Full constellation name.
 */