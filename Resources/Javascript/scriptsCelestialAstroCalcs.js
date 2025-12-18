/**
 * @typedef {import("./types").IntersectionList} IntersectionList
 */

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
 * @typedef {object} FixResult
 * @property {Array<CoordPair>} cockedHatVerticesCoord - An array of the three geographical coordinates (lat/lon in radians) that form the vertices of the 'Cocked Hat' (LOP intersection points).
 * @property {Array<PixelPosition>} cockedHatVerticesPixels - An array of the three pixel coordinates (top/left) for the Cocked Hat vertices.
 * @property {PixelPosition} fixPositionPixels - The pixel coordinates (top/left) for the final calculated Fix Position (e.g., the Centroid).
 * @property {CoordPair} fixPositionCoord - The coordinate for the final calculated Fix Position (e.g., the Centroid).
 */

/**
 * @typedef {object} PixelPosition
 * @property {number} left - The horizontal pixel coordinate (distance from left edge).
 * @property {number} top - The vertical pixel coordinate (distance from top edge).
 */

/**
 * @typedef {object} CoordPair
 * @property {number} latitude - Latitude in radians.
 * @property {number} longitude - Longitude in radians.
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

/**
 * @file scriptsCelestialAstroCalcs.js
 * @description One of two script files for the Celestial Navigation type mission.
 * This one handles calculations, the other handles user interface aspects.
 * @author [David Kilpatrick]
 * @version 1.0.0
 * @date 2025-12-01
*/

// #region Utility and Conversion Functions

/**
 * @summary Converts an angle from degrees to radians.
 * @param {number} degrees - The angle value in degrees.
 * @returns {number} The angle value converted to radians.
 */
function toRadians(degrees) {
	return degrees * Math.PI / 180;
}

/**
 * @summary Converts an angle from radians to degrees.
 * @param {number} radians - The angle value in radians.
 * @returns {number} The angle value converted to degrees.
 */
function toDegrees(radians) {
	return radians * 180 / Math.PI;
}

/**
 * @summary Converts a value expressed in Hours, Minutes, and Seconds (HMS) into a single decimal number (e.g., decimal hours or decimal degrees).
 * @description The sign is determined solely by the 'hour' input.
 * @param {number} hour - The hour or degree component (signed).
 * @param {number} minute - The minute component (0 to 59).
 * @param {number} second - The second component (0 to 59.9...).
 * @returns {number} The decimal value in the same unit as the hour component (e.g., hours or degrees).
 */
function hmsToDecimal(hour, minute, second) {
	// 1. Determine the overall sign based on the hour/degree component.
	const sign = hour < 0 ? -1 : 1;

	// 2. Calculate the total magnitude (absolute value) by summing positive components.
	// Use Math.abs(hour) to ensure we are adding parts to the magnitude of the whole degree/hour.
	const magnitude = Math.abs(hour) + (minute / 60) + (second / 3600);

	// 3. Apply the sign to the total magnitude.
	return sign * magnitude;
}

/**
 * @summary Normalizes a radian angle to the standard range [-π, +π].
 * @description This method is mathematically robust as it shifts the angle to a positive 
 * range [0, 2π] before using the modulo operator, ensuring consistent results 
 * regardless of the sign of the input angle.
 * @param {number} angleRad Angle in radians.
 * @returns {number} Normalized angle in radians [-π, +π].
 */
function clampLongitude(angleRad) {
	const TWO_PI = 2 * Math.PI;

	// Shift the angle into the range [0, 2π] (by adding PI to center it around PI)
	let normalized = angleRad + Math.PI;

	// Use modulo operator to wrap around 2*PI
	normalized %= TWO_PI;

	// Correct for potentially negative results of JavaScript's % operator
	// (although unlikely with the shift, this guarantees a positive result from the modulo)
	if (normalized < 0) {
		normalized += TWO_PI;
	}

	// Shift back to the desired range [-π, +π]
	normalized -= Math.PI;

	return normalized;
}

// #endregion Utility and Conversion Functions


// #region Time and Celestial Mechanics

/**
 * @summary Calculates the number of days elapsed since the J2000.0 epoch (January 1, 2000, 12:00 UT).
 * @description Relies on external global arrays (daysToBeginMth) and the VarGet function to fetch Zulu time components.
 * @param {number} zuluTime - Current Universal Time in seconds since midnight (0-86399).
 * @returns {number} The Julian Day offset relative to J2000.0.
 */
function getJ2000Day(zuluTime) {
	// Constants
	const SECONDS_PER_DAY = 60 * 60 * 24;
	const YEAR_EPOCH = 2000;

	// 1. Fetch current date components
	const zuluDayOfMonth = VarGet("E:ZULU DAY OF MONTH", "Number");
	const zuluMonth = VarGet("E:ZULU MONTH OF YEAR", "Number");
	const zuluYear = VarGet("E:ZULU YEAR", "Number");

	// 2. Calculate time fraction of the current day
	const dayFraction = zuluTime / SECONDS_PER_DAY;

	// 3. Calculate days elapsed within the current year (pre-leap correction)
	// NOTE: daysToBeginMth is assumed to be a global array containing cumulative days before the start of zuluMonth.
	let daysToBegMth = daysToBeginMth[zuluMonth];

	// Adjust for leap year if the year is divisible by 4 AND the date is March 1st or later
	if ((zuluYear % 4 === 0) && (zuluMonth > 2)) {
		daysToBegMth += 1;
	}

	// 4. Calculate total days elapsed for full years since the J2000 epoch
	const yearDiff = zuluYear - YEAR_EPOCH;
	const leapDayCorrection = Math.ceil(yearDiff / 4);

	// -1.5 offset handles the J2000 epoch start at noon (0.5 day) on Jan 1st (index adjustment).
	const daysToBegYear = -1.5 + (yearDiff * 365) + leapDayCorrection;

	// 5. Sum all components
	return dayFraction + daysToBegMth + zuluDayOfMonth + daysToBegYear;
}

/**
 * @summary Calculates the Local Sidereal Time (LST) based on the current Universal Time and observer's longitude.
 * @description Uses an approximation formula for Greenwich Sidereal Time (GST) adjusted for longitude.
 * The output is normalized to the range [0, 2π] radians.
 * @param {number} longitude - Observer's longitude in Radians.
 * @returns {number} The Local Sidereal Time (LST) in Radians.
 */
function getLocalSiderialTime(longitude) {
	// Constants (for approximation formula)
	const C_GST_0_DEG = 100.4641;   // Approximate GST at 0h UT, 1 Jan 2000 (J2000.0)
	const ROTATION_RATE = 0.985647; // Sidereal correction factor (deg/day)
	const SECONDS_PER_HOUR = 3600;  // Conversion factor

	// 1. Fetch time data and calculate required components
	const zuluTime = VarGet("E:ZULU TIME", "Seconds");
	const j2000Days = getJ2000Day(zuluTime);
	const UT_hours = zuluTime / SECONDS_PER_HOUR;

	// 2. Calculate Greenwich Sidereal Time (GST) in Degrees

	// GST at 0h UT, J2000 + Rate * Days since J2000 + UT conversion
	// GST = C + (0.985647 * d) + (15 * UT_hours)
	let GSTdeg = C_GST_0_DEG +
		(ROTATION_RATE * j2000Days) +
		(15 * UT_hours);

	// 3. Apply Observer's Longitude to get Local Sidereal Time (LST)
	// LST = GST + Longitude (in degrees)
	const lonDeg = toDegrees(longitude); // Convert input longitude to degrees
	let LSTdeg = GSTdeg + lonDeg;

	// 4. Normalize LST to the 0-360 degree range
	// Robust normalization for positive or negative angles.
	LSTdeg = (LSTdeg % 360 + 360) % 360;

	// 5. Return the result converted back to Radians
	return toRadians(LSTdeg);
}

/**
 * @summary Calculates the Local Hour Angle (HA) by subtracting Right Ascension (RA) from Local Sidereal Time (LST).
 * @description All input and output angles are expected to be in Radians and normalized to the range [0, 2π].
 * @param {number} LST - Local Sidereal Time in Radians.
 * @param {number} RA - Right Ascension in Radians.
 * @returns {number} The Local Hour Angle (HA) in Radians (0 to 2π).
 */
function getHourAngle(LST, RA) {
	// Standard LHA calculation
	let HA = LST - RA;

	// The angle 2π radians (360 degrees)
	const TWO_PI = 2 * Math.PI;

	// Normalize the angle to be positive (0 to 2π) using the modulo pattern.
	// This replaces the while loop (while (HA < 0) HA += TWO_PI;).
	HA = (HA % TWO_PI + TWO_PI) % TWO_PI;

	return HA;
}

// #endregion Time and Celestial Mechanics


// #region Horizontal Coordinate Calculations (Altitude & Azimuth)

/**
 * @summary Calculates the celestial body's Calculated Altitude (ALT or Hc) using the astronomical triangle formula.
 * @description All input angles (DEC, LAT, HA) are expected to be in Radians.
 * @param {number} DEC - The celestial body's Declination in Radians.
 * @param {number} LAT - The observer's Latitude in Radians.
 * @param {number} HA - The Local Hour Angle (LHA) in Radians.
 * @returns {number} The Calculated Altitude (ALT) in **Radians**. 
 */
function getALT(DEC, LAT, HA) {
	// Fundamental Formula of the Astronomical Triangle (Spherical Law of Cosines):
	const sin_ALT = Math.sin(DEC) * Math.sin(LAT) + Math.cos(DEC) * Math.cos(LAT) * Math.cos(HA);

	// Calculate the Altitude in Radians using the inverse sine (arc sin) function
	const ALT_rad = Math.asin(sin_ALT);

	return ALT_rad;
}

/**
 * @summary Calculates the raw Azimuth Angle (Z) and adjusts it to the True Azimuth Bearing (AZ) 
 * using the Law of Cosines for the astronomical triangle.
 * @description All input and output angles are expected to be in Radians. The adjustment logic 
 * uses the sign of the Local Hour Angle (HA) to determine the East/West quadrant.
 * @param {number} DEC - Celestial body's Declination in Radians.
 * @param {number} ALT - Celestial body's Calculated Altitude in **Radians**.
 * @param {number} LAT - Observer's Latitude in Radians.
 * @param {number} HA - Local Hour Angle (LHA) in Radians.
 * @returns {number} The True Azimuth Bearing (AZ) in **Radians** (0 to 2π).
 */
function getAZ(DEC, ALT, LAT, HA) {

	// ALT is already in Radians from getALT, no conversion needed.
	const ALTrad = ALT;
	const TWO_PI = 2 * Math.PI;

	// 1. Calculate the term for the inverse cosine function (Law of Cosines derivation)
	// cos(Z) = [sin(DEC) - sin(ALT) * sin(LAT)] / [cos(ALT) * cos(LAT)]
	const cos_Z = (Math.sin(DEC) - Math.sin(ALTrad) * Math.sin(LAT)) / (Math.cos(ALTrad) * Math.cos(LAT));

	// 2. Handle potential floating-point errors which can push cos_Z slightly outside [-1, 1]
	const clamped_cos_Z = Math.max(-1, Math.min(1, cos_Z));

	// 3. Calculate the Azimuth Angle (Z) in Radians using inverse cosine (returns 0 to pi)
	const Z_rad = Math.acos(clamped_cos_Z);

	// 4. Adjust the acute angle Z to the 0-2π true bearing (AZ) in Radians
	// Note: The logic below assumes the azimuth (Z_rad) is measured from the elevated pole (North/South).

	let AZ_rad;

	if (Math.sin(HA) < 0) {
		// West of the meridian (HA is 180 to 360 degrees).
		// Since HA is in [0, 2pi] from getHourAngle, sin(HA) < 0 means HA is in (pi, 2pi).
		AZ_rad = Z_rad;
	} else {
		// East of the meridian (HA is 0 to 180 degrees).
		AZ_rad = TWO_PI - Z_rad;
	}

	return AZ_rad;
}

/**
 * @summary Calculates the local Altitude (ALT) and Azimuth (AZ) for all stars and filters those visible within the sextant's FOV.
 * @description Visible stars' data, along with their pixel coordinates, are collected into the localPtsList array.
 * @param {CoordPair} observerPosition - Observer's position {latitude, longitude} in Radians.
 * @param {SextantView} viewState - The current state of the sextant view configuration (all in DEGREES).
 * @param {Array<{RA: number, DEC: number}>} starCelestialCoords - The fixed celestial coordinates for all stars. // NEW PARAM
 * @returns {Array<VisibleStarData>} The localPtsList array containing star data and pixel positions (now objects). 
 */
function calcLocalStarPositions(observerPosition, viewState, starCelestialCoords) {

	const latitude = observerPosition.latitude;
	const longitude = observerPosition.longitude;

	// Alias view state properties for cleaner calculations
	const fovH = viewState.fovH;
	const fovV = viewState.fovV;
	const sexAZ = viewState.azTrueDeg; 
	const sexALT = viewState.altDeg;   

	// Define constants
	const ZENITH_ALTITUDE_THRESHOLD_DEG = 85;

	/** @type {Array<VisibleStarData>} */
	const localPtsList = [];

	// Calculate Local Sidereal Time (LST)
	const LST = getLocalSiderialTime(longitude);

	for (let starIndex = 0; starIndex < starCatalog.length; starIndex++) {

		// 1. Fetch pre-calculated constant celestial coordinates (RA, DEC are in Radians)
		const { RA, DEC } = starCelestialCoords[starIndex];

		// 2. Calculate Local Hour Angle (HA) (LST is in Radians, HA is in Radians)
		const HA = getHourAngle(LST, RA);

		// --- Celestial to Horizontal Conversion ---

		// 3. Calculate local Horizontal Coordinates (ALT & AZ) (Results are in Radians)
		const ALT_Rad = getALT(DEC, latitude, HA);
		const AZ_Rad = getAZ(DEC, ALT_Rad, latitude, HA);
		const ALT_Deg = toDegrees(ALT_Rad);
		const AZ_Deg = toDegrees(AZ_Rad);

		// 4. Check if the star is within the sextant's FOV

		// 4a. Vertical Altitude Check
		const isVerticallyVisible = (ALT_Deg >= sexALT) && (ALT_Deg <= sexALT + fovV);
		let isHorizontallyVisible = false;

		if (isVerticallyVisible) {

			// 4b. AZIMUTH FILTERING (Handling Zenith Instability)

			if (ALT_Deg >= ZENITH_ALTITUDE_THRESHOLD_DEG) {
				// Bypass: Star is near the zenith (ALT >= 85°). Horizontal visibility is guaranteed.
				isHorizontallyVisible = true;

			} else {
				// Standard Check: Star is below the Zenith Zone, use Azimuth delta filtering.
				const AZbearingDelta = getBearingDif(AZ_Deg, sexAZ);

				if (AZbearingDelta <= fovH / 2) {
					isHorizontallyVisible = true;
				}
			}
		}

		// Final check to proceed to pixel calculation
		if (isVerticallyVisible && isHorizontallyVisible) {

			// 5. Calculate Pixel Positions (The entire calculation uses DEGREES)

			let finalRelativeAZ;

			if (ALT_Deg >= ZENITH_ALTITUDE_THRESHOLD_DEG) {
				// Star is near the zenith. Override horizontal position to the center of the FOV.
				finalRelativeAZ = fovH / 2;

			} else {
				// Standard (Non-Zenith) Azimuth Calculation
				// 5.1 Calculate the azimuth of the left edge of the FOV.
				const fovHalf = fovH / 2;
				const leftEdgeAZ = (sexAZ - fovHalf + 360) % 360;

				// 5.2 Calculate the difference between the star's AZ and the left edge's AZ.
				// This is the relative displacement from the left edge of the FOV.
				finalRelativeAZ = (AZ_Deg - leftEdgeAZ + 360) % 360;
			}

			// Left pixel: relativeAZ / fovH gives the ratio across the horizontal FOV.
			// windowW is assumed to be the global constant.
			const left = Math.round(finalRelativeAZ / fovH * windowW);

			// Top pixel: (sexALT + fovV - ALT_Deg) / fovV gives the ratio down the vertical FOV (from top).
			// windowH is assumed to be the global constant.
			const top = Math.round((sexALT + fovV - ALT_Deg) / fovV * windowH);

			// 6. Update the list with star data and pixel position
			/** @type {PixelPosition} */
			const starPixelPosition = { left, top };
			updatePtsList(localPtsList, starIndex, starPixelPosition);
		}
	}
	return localPtsList;
}

// #endregion Horizontal Coordinate Calculations (Altitude & Azimuth)


// #region Sextant FOV and Display Utilities

/**
 * @summary Calculates the shortest angular difference between two azimuths (the bearing delta).
 * @param {number} AZ - Azimuth 1 in degrees (0-360).
 * @param {number} sexAZ - Azimuth 2 in degrees (0-360).
 * @returns {number} The shortest bearing difference in degrees (0-180).
 */
function getBearingDif(AZ, sexAZ) {
	/** @type {number} */
	const absDelta = Math.abs(AZ - sexAZ);

	return absDelta <= 180 ? absDelta : 360 - absDelta;
}

/**
 * @summary Updates the visibleStarsData array with a single star's data and calculated pixel position.
 * @description Creates and pushes a structured VisibleStarData object.
 * @param {Array<VisibleStarData>} listToUpdate - The array to push the data into.
 * @param {number} starIndex - The index of the star in the global starCatalog array.
 * @param {PixelPosition} pixelPosition - The calculated pixel coordinates {left, top} of the star's position in the FOV.
 * @returns {void}
 */
function updatePtsList(listToUpdate, starIndex, pixelPosition) {
	const left = pixelPosition.left;
	const top = pixelPosition.top;

	const star = starCatalog[starIndex];

	// Create the structured VisibleStarData object for the single star
	const singleStarData = {
		starCatalogId: star.CatalogID,
		visMag: star.VisualMagnitude,
		leftPixel: left,
		topPixel: top,
		shaIndex: star.ShaIndex,
		navName: star.NavName,
		bayerDesignation: star.BayerDesignation,
		constellationName: star.ConstellationName
	};

	// Push the entire object into the list of visible stars
	listToUpdate.push(singleStarData);
}

/**
 * @summary Calculates the Star's Calculated Altitude (Hc) and Azimuth (Zn) based on its pixel position 
 * within the sextant's FOV (the reverse calculation).
 * @description Calculates Hc and Zn in degrees, normalizes Zn, and then converts both to radians.
 * @param {VisibleStarData} starData - The structured object containing star data and pixel positions.
 * @param {SextantView} viewState - The current state of the sextant view configuration.
 * @returns {SightCalculationResult} An object containing {HcRad, ZnRad}.
 */
function calcHcZn(starData, viewState) {
	// Access pixel positions from the star data object
	const leftPixels = starData.leftPixel;
	const topPixels = starData.topPixel;

	// 1. Calculate Calculated Altitude (Hc) in degrees.
	const Hc_deg = ((windowH - topPixels) / windowH) * viewState.fovV + viewState.altDeg;

	// 2. Calculate Calculated Azimuth (Zn) in degrees.
	let Zn_deg = (leftPixels / windowW) * viewState.fovH - (viewState.fovH / 2) + viewState.azTrueDeg;

	// 3. Normalize Azimuth (Zn) to the 0-360 degree range.
	// (x % 360 + 360) % 360 ensures positive remainder for negative numbers
	Zn_deg = (Zn_deg % 360 + 360) % 360;

	// 4. Convert results to Radians
	const HcRad = toRadians(Hc_deg);
	const ZnRad = toRadians(Zn_deg);

	// Return as SightCalculationResult object 
	return {
		HcRad: HcRad,
		ZnRad: ZnRad
	};
}

/**
 * @summary Converts a latitude/longitude coordinate pair into pixel coordinates [left, top] for plotting.
 * @description Coordinates are mapped relative to the global plotBoundaries object (defining the map edges in radians) 
 * and window size (windowW, windowH).
 * @param {CoordPair} coord - An object containing {latitude, longitude} in radians.
 * @returns {PixelPosition} An object containing {left, top} pixel values.
 */
function convertCoordToPixels(coord) {
	// Input is CoordPair { latitude, longitude }
	const lat = coord.latitude;
	const lon = coord.longitude;

	// Initialize pixel coordinates
	let left;
	let top;

	const westEdge = plotBoundaries.west;
	const eastEdge = plotBoundaries.east;
	const northEdge = plotBoundaries.north;
	const southEdge = plotBoundaries.south;

	// --- 1. Calculate Left Pixel (Longitude Mapping) ---
	if (lon >= westEdge && lon <= eastEdge) {
		// Linear mapping: ratio of current position within map width, scaled by window width.
		left = (lon - westEdge) / (eastEdge - westEdge) * windowW;
	} else {
		// If out of horizontal bounds, set to 0.
		left = 0;
	}

	// --- 2. Calculate Top Pixel (Latitude Mapping) ---
	if (lat <= northEdge && lat >= southEdge) {
		// Linear mapping: ratio of distance from North Edge (top), scaled by window height.
		top = (northEdge - lat) / (northEdge - southEdge) * windowH;
	} else {
		// If out of vertical bounds, set to 0.
		top = 0;
	}

	return { left, top };
}

// #endregion Sextant FOV and Display Utilities


// #region Great Circle and Navigation Fixes

/**
 * @summary Calculates the initial true bearing (azimuth) in degrees from the first coordinate to the second coordinate.
 * @description Uses the Great-Circle Bearing formula. All input angles are expected to be in Radians.
 * @param {CoordPair} P1 - The starting point {latitude, longitude} in Radians.
 * @param {CoordPair} P2 - The destination point {latitude, longitude} in Radians.
 * @returns {number} The true bearing (azimuth) in Radians.
 */
function getBearing(P1, P2) {

	const lat1 = P1.latitude;
	const lon1 = P1.longitude;
	const lat2 = P2.latitude;
	const lon2 = P2.longitude;

	// Calculate the difference in longitudes
	const dLon = lon2 - lon1;

	// --- 1. Calculate the Y component (Numerator for tangent) ---
	// y = sin(dLon) * cos(lat2)
	const y = Math.sin(dLon) * Math.cos(lat2);

	// --- 2. Calculate the X component (Denominator for tangent) ---
	// x = cos(lat1) * sin(lat2) - sin(lat1) * cos(lat2) * cos(dLon)
	const x = Math.cos(lat1) * Math.sin(lat2) -
		Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);

	// --- 3. Calculate the angle in Radians ---
	// Math.atan2 correctly calculates the angle in the range [-pi, pi] (approximately [-180deg, 180deg])
	const angle_rad = Math.atan2(y, x);

	// --- 4. Normalise ---
	return clampLongitude(angle_rad);
}

/**
 * @summary Calculates the great-circle distance between two latitude/longitude points using the Haversine Formula.
 * @description All input angles are expected to be in Radians. The distance is returned in Feet.
 * @param {CoordPair} P1 - The starting point {latitude, longitude} in Radians.
 * @param {CoordPair} P2 - The destination point {latitude, longitude} in Radians.
 * @returns {number} The distance in Feet.
 */
function getDistance(P1, P2) {

	const lat1 = P1.latitude;
	const lon1 = P1.longitude;
	const lat2 = P2.latitude;
	const lon2 = P2.longitude;

	// Calculate the differences in latitude and longitude
	const deltaLat = lat2 - lat1;
	const deltaLon = lon2 - lon1;

	// Earth's radius (R) in feet. (3959 miles * 5280 ft/mile ≈ 20,903,520 ft)
	const R_feet = 20903520;

	// 1. Calculate the 'a' term of the Haversine formula
	// a = sin²(Δlat/2) + cos(lat1) * cos(lat2) * sin²(Δlon/2)
	const a = Math.sin(deltaLat / 2) * Math.sin(deltaLat / 2) +
		Math.cos(lat1) * Math.cos(lat2) * Math.sin(deltaLon / 2) * Math.sin(deltaLon / 2);

	// 2. Calculate the central angle 'c' (angular distance in radians)
	// c = 2 * atan2(sqrt(a), sqrt(1 - a))
	const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

	// 3. Distance = R * c
	return R_feet * c;
}

/**
 * @summary Calculates an intermediate point on the Great Circle path between two coordinates.
 * @description All coordinate inputs are expected to be in radians.
 * @param {number} distance - The total distance (in feet) between point P1 and P2 (pre-calculated).
 * @param {number} fraction - The fractional distance (0 to 1) along the path from P1 to calculate P3.
 * @param {CoordPair} P1 - The starting point {latitude, longitude} in radians.
 * @param {CoordPair} P2 - The destination point {latitude, longitude} in radians.
 * @returns {CoordPair} The intermediate point {latitude, longitude} in radians.
 */
function getIntermediatePoint(distance, fraction, P1, P2) {

	const lat1 = P1.latitude;
	const lon1 = P1.longitude;
	const lat2 = P2.latitude;
	const lon2 = P2.longitude;

	// Earth's radius (R) at the equator in feet, used for distance-to-angular-distance conversion.
	const R_equator_feet = 20902230.971129;

	// 1. Calculate the central angle (d) between P1 and P2 in radians (d = distance / R)
	const d = distance / R_equator_feet;

	// 2. Calculate the weight factors 'a' and 'b' for spherical linear interpolation
	// These factors determine the contribution of P1 and P2 to the intermediate point P3.
	const a = Math.sin((1 - fraction) * d) / Math.sin(d);
	const b = Math.sin(fraction * d) / Math.sin(d);

	// 3. Calculate the Cartesian components (X, Y, Z) of the intermediate point P3 on the unit sphere
	const x = a * Math.cos(lat1) * Math.cos(lon1) + b * Math.cos(lat2) * Math.cos(lon2);
	const y = a * Math.cos(lat1) * Math.sin(lon1) + b * Math.cos(lat2) * Math.sin(lon2);
	const z = a * Math.sin(lat1) + b * Math.sin(lat2);

	// 4. Convert Cartesian (X, Y, Z) back to Spherical Coordinates (Latitude and Longitude in Radians)
	// Latitude (lat3) derived from Z and the projection onto the XY plane (sqrt(x² + y²))
	let lat3_rad = Math.atan2(z, Math.sqrt(x * x + y * y));

	// Longitude (lon3) derived from Y and X
	let lon3_rad = Math.atan2(y, x);

	// 5. Normalize Longitude to the standard [-PI, +PI] range
	lon3_rad = clampLongitude(lon3_rad);

	return { latitude: lat3_rad, longitude: lon3_rad };
}

/**
 * @summary Calculates the coordinates of a destination point (P2) given a starting point (P1), a bearing, and a distance.
 * @description Uses the Great-Circle Destination formula. Inputs P1 in Radians and bearing in Degrees; distance is assumed to be in Nautical Miles (nm).
 * @param {CoordPair} P1 - The starting point {latitude, longitude} in Radians.
 * @param {number} bearing - True Bearing (Azimuth) from P1 to P2 in Degrees.
 * @param {number} distance - The **Intercept (a)** distance ($H_o - H_c$), in Nautical Miles (nm).
 * @returns {CoordPair} The destination point {latitude, longitude} in Radians.
 */
function calculateInterceptPoint(P1, bearing, distance) {
	// --- FIX 1: Consume CoordPair object ---
	const lat1 = P1.latitude;
	const lon1 = P1.longitude;

	// Earth's radius (R) in Nautical Miles (nm).
	const EARTH_RADIUS_NM = 3440.1;

	// Normalize distance by radius to get the central angle (angular distance in radians)
	const angularDistance = distance / EARTH_RADIUS_NM;

	// 1. Convert inputs from Degrees to Radians
	const bearing_rad = toRadians(bearing);

	// --- 2. Calculate Latitude of P2 (lat2) ---
	// sin(lat2) = sin(lat1) * cos(d/R) + cos(lat1) * sin(d/R) * cos(bearing)
	const sin_lat2 = Math.sin(lat1) * Math.cos(angularDistance) +
		Math.cos(lat1) * Math.sin(angularDistance) * Math.cos(bearing_rad);
	let lat2_rad = Math.asin(sin_lat2);

	// --- 3. Calculate Longitude of P2 (lon2) ---
	// lon2 = lon1 + atan2(sin(bearing) * sin(d/R) * cos(lat1), cos(d/R) - sin(lat1) * sin(lat2))
	let lon2_rad = lon1 + Math.atan2(
		Math.sin(bearing_rad) * Math.sin(angularDistance) * Math.cos(lat1),
		Math.cos(angularDistance) - Math.sin(lat1) * Math.sin(lat2_rad)
	);

	// --- 4. Normalize Longitude ---
	lon2_rad = clampLongitude(lon2_rad);

	return { latitude: lat2_rad, longitude: lon2_rad };
}

/**
 * @summary Calculates the intersection points of the last three Lines of Position (LOPs) using a planar approximation method.
 * @param {SightingLOPData[]} sightHistory - The array containing the history of LOP data.
 * @returns {number[]} A flat array of six numbers: [Lat1Rad, Lon1Rad, Lat2Rad, Lon2Rad, Lat3Rad, Lon3Rad] for the three calculated intersection points.
 */
function getCockedHatCoords(sightHistory) {

	// The output array will contain 6 elements (3 vertices * 2 coords)
	/** @type {number[]} */
	const cockedHatCoords = [];

	// Pi/2 in radians
	const PI_OVER_2 = Math.PI / 2;

	// The number of total sights taken is the length of the sightHistory array.
	const sightCount = sightHistory.length;

	// Check if we have at least 3 sights to form a fix.
	if (sightCount < 3) {
		// Return an empty array or handle error appropriately if called too early.
		return cockedHatCoords;
	}

	// The indices of the last three sights (0-based).
	// Example: if sightCount=3, indices are [0, 1, 2].
	const lastThreeIndices = [sightCount - 3, sightCount - 2, sightCount - 1];

	// Loop three times: once for each pair of LOPs (LOP1/LOP2, LOP2/LOP3, LOP3/LOP1)
	// The loop variable 'i' cycles from 0 to 2, representing the index in the lastThreeIndices array.
	for (let i = 0; i < 3; i++) {

		// Get the index of the current LOP (Pt1) and the next LOP (Pt2)
		const idx1 = lastThreeIndices[i];
		// The index wraps around (0 -> 1, 1 -> 2, 2 -> 0)
		const idx2 = lastThreeIndices[(i + 1) % 3];

		// Fetch data from the sightHistory array
		// LOP 1 Data
		const pt1Data = sightHistory[idx1];
		const LatPt1 = pt1Data.interceptPointCoord.latitude;
		const LonPt1 = pt1Data.interceptPointCoord.longitude;
		const ZnRadPt1 = pt1Data.ZnRad;

		// LOP 2 Data
		const pt2Data = sightHistory[idx2];
		const LatPt2 = pt2Data.interceptPointCoord.latitude;
		const LonPt2 = pt2Data.interceptPointCoord.longitude;
		const ZnRadPt2 = pt2Data.ZnRad;


		// --- Planar Intersection Calculation ---

		// Bearing of the LOPs: Zn is the Azimuth True Bearing.
		// The LOP is perpendicular to the bearing, so we add 90 degrees (PI/2 radians).
		const BearingPt1 = ZnRadPt1 + PI_OVER_2;
		const BearingPt2 = ZnRadPt2 + PI_OVER_2;

		// The denominator in the planar intersection formula represents the difference in the slopes (tan(Bearing))
		const denominator = Math.tan(BearingPt1) - Math.tan(BearingPt2);

		// Check for parallel or near-parallel LOPs to prevent division by zero
		if (Math.abs(denominator) < 1e-6) { // 1e-6 is a small tolerance for floating-point near-zero
			// In a real scenario, this would mean the lines are parallel. For this model, skip the intersection.
			// For simplicity, we'll continue, but this needs proper error handling in a production app.
		}

		// LOP intersection Latitude (y-coordinate)
		const vertexLat = (LatPt1 * Math.tan(BearingPt1) - LatPt2 * Math.tan(BearingPt2) + LonPt2 - LonPt1) / denominator;

		// LOP intersection Longitude (x-coordinate)
		const vertexLon = vertexLat * Math.tan(BearingPt1) - LatPt1 * Math.tan(BearingPt1) + LonPt1;

		// Add the calculated intersection point [Lat, Lon] to the list (now a clean array)
		cockedHatCoords.push(vertexLat);
		cockedHatCoords.push(vertexLon);
	}

	return cockedHatCoords;
}

/**
 * @summary Calculates the Centroid (estimated Fix Position) of the three Cocked Hat vertices.
 * @description The function uses the planar centroid formula based on the assumption that the coordinates 
 * are calculated using a planar approximation.
 * @param {Array<CoordPair>} cockedHatVertices - An array containing the three calculated vertex coordinates {latitude, longitude} in radians.
 * @returns {CoordPair} An object containing the final Fix position {latitude, longitude} in radians.
 */
function getFixCoord(cockedHatVertices) {

	// Deconstruct the three vertices
	/** @type {CoordPair} */
	const P1 = cockedHatVertices[0];
	/** @type {CoordPair} */
	const P2 = cockedHatVertices[1];
	/** @type {CoordPair} */
	const P3 = cockedHatVertices[2];

	// --- 1. Simplify Centroid Calculation (Planar Centroid Formula) ---
	// Since the intersection calculation (in getCockedHatCoords) uses a planar (Cartesian) approximation, 
	// the centroid should be calculated using the simple average of the coordinates.

	// Sum the latitudes
	const totalLat = P1.latitude + P2.latitude + P3.latitude;

	// Sum the longitudes
	const totalLon = P1.longitude + P2.longitude + P3.longitude;

	// Calculate the Centroid Latitude and Longitude
	const centroidLat = totalLat / 3;
	const centroidLon = totalLon / 3;

	// --- 2. Create and Return the Fix Coordinate ---
	/** @type {CoordPair} */
	const centroidFix = {
		latitude: centroidLat,
		longitude: centroidLon
	};

	return centroidFix;
}

// #endregion Great Circle and Navigation Fixes