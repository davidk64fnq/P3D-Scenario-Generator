/**
 * @file scriptsCelestialSextant.js
 * @description One of two script files for the Celestial Navigation type mission.
 * This one handles user interface aspects, the other handles calculations.
 * @author [David Kilpatrick]
 * @version 1.0.0
 * @date 2025-12-01
 * */

// #region Global declarations

// #region JSDoc Global Variable Declarations (Injected by C#)

// These variables are declared globally. Their string values are expected to be
// injected/replaced by the C# application before script execution.
// The JSDoc provides type information for the TypeScript checker.

/**
 * @type {StarData[]} starCatalog - Array of 446 StarData objects, where each object contains all properties for a single star.
 */
const starCatalog = null;

/**
 * @type {Array<NavStarData>} navStarCatalog - Array of 57 NavStarData objects, containing the precise SHA/DEC coordinates and name for each navigational star.
 */
const navStarCatalog = null;

/**
 * @type {PlotArea} plotBoundaries - The four edges defining the bounds of the navigational plotting area, all in radians.
 */
const plotBoundaries = null;

/**
 * @type {AriesGHAData} ariesGHAData - Consolidated structure holding the GHA data for Aries (Degrees and Minutes) 
 * across three days (start day and one day before/after).
 */
const ariesGHAData = null;

/** 
 * @type {string[]} constellationLines - List of pairs of stars optionally connected by a line when constellations displayed 
 * e.g. "Peg5" "Peg6" would result in a line between those 2 stars.
 * */
const constellationLines = null;

/** 
 * @type {CoordPair} destCoord - The coordinate pair (Latitude and Longitude, both in radians) of the destination airport.
 * */
const destCoord = null;

/** 
 * @type {CoordPair} currentDRCoord - The current Dead Reckoning coordinate pair (Latitude and Longitude, both in radians), initialised to destination.
 */
let currentDRCoord;

/** 
 * @type {string} startDate - The local date selected when scenario generated (mm/dd/yyyy).
 * */
const startDate = null;

// #endregion

// #region Application State Variables (Script-Managed Globals)

// #region Constants (Used throughout the script)

/**
 * @type {number[]} daysToBeginMth - Array of cumulative days to the beginning of each month (1-indexed, Jan = 1).
 */
const daysToBeginMth = [0, 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334];

/**
 * @type {number} windowW - Width of the sextant view window (pixels).
 */
const windowW = 960;

/**
 * @type {number} windowH - Height of the sextant view window (pixels).
 */
const windowH = 540;

/**
 * @type {number} defaultFOV - Default field of view (degrees) for the visualization.
 */
const defaultFOV = 45;

/**
 * @type {number} TWO_PI - Constant representing $2\pi$ (full circle in radians).
 */
const TWO_PI = Math.PI * 2;

/**
 * @type {number} HALF_PI - Constant representing $\pi/2$ (a right angle in radians).
 */
const HALF_PI = Math.PI / 2;

/**
 * @type {number} SLOW_UPDATE_MS - the period for the heavy calculation (e.g., once every 5 seconds).
 */
const SLOW_UPDATE_MS = 5000;

// #endregion

// #region Plane/Mission Status

/**
 * @type {PlaneStatus}
 * @description The main object holding the plane's navigational state.
 */
const planeStatus = {
	headingTrueRad: 0,
	magVarRad: 0,
	position: {
		latitude: 0,
		longitude: 0,
	},
	speedKnots: 0,
};

/** 
 * @type {VisibleStarData[]} globalVisibleStarsData - to store the result of the slow, heavy calculation.
 */
let globalVisibleStarsData = [];

/** 
 * @type {number} lastDRUpdateTime - to track when the last DR update occurred.
 */
let lastDRUpdateTime = 0;

// #endregion

// #region Sextant View State

/** 
 * @type {SextantView} 
 */
const sextantView = {
	fovH: defaultFOV,
	fovV: defaultFOV * windowH / windowW,
	azTrueDeg: 0, 
	altDeg: 0,    
	centerPixelY: windowH / 2
};

/**
 * @type {InputControl}
 */
const inputControl = {
	azimuth: 0,
	altitude: 0,
	fieldOfView: 0
};

// #endregion

// #region Starmap display Toggles (0=off, 1=on)

/**
 * @type {StarDisplayConfig}
 */
const starDisplayConfig = {
	labelStars: 0,
	drawConstellations: 0,
	labelConstellations: 0
};

// #endregion

// #region Plotting and Fix Calculation Variables

/**
 * @type {Array<{RA: number, DEC: number}>} starCelestialCoords - Fixed celestial coordinates (RA, DEC in Radians) 
 * for all stars. Calculated once at startup.
 */
const starCelestialCoords = precalculateStarCoordinates();

/**
 * @type {SightingLOPData[]} 
 */
const sightHistory = [];

/**
 * @type {FixResult[]}
 * @description Array containing the historical results of every calculated navigational fix (estimated position). This array is updated every three sights.
 */
const fixHistory = [];

/**
 * @type {PlotDisplayConfig}
 */
const plotDisplayConfig = {
	showFinalLeg: 0,
	plotPlane: 0,
	plotNeedsUpdate: 1
};

// #endregion

// #endregion

// #endregion

// #region Setting up canvasses and contexts

/**
* Utility function to display status messages... 
* @param {'INFO' | 'SUCCESS' | 'ERROR'} type - INFO/ERROR/SUCCESS etc.
* @param {string} message - the text string to display.
*/
function displayStatusMessage(type, message) {
	const statusElement = document.getElementById('app-status');
	const colorMap = {
		'INFO': 'blue',
		'SUCCESS': 'green',
		'ERROR': 'red'
	};

	if (statusElement) {
		statusElement.textContent = `[${type}] ${message}`;

		// You can add color/style by finding the element or using a wrapper div, 
		// but for safety, the simplest overwrite is best.
		statusElement.style.color = colorMap[type] || 'black';
		statusElement.style.fontWeight = type === 'ERROR' ? 'bold' : 'normal';
	}
}

// --- Star Chart tab CANVAS INITIALIZATION ---

// 1. Declare starCanvas as const, initialized via lookup
const starCanvas = /** @type {HTMLCanvasElement | null} */(document.getElementById('canvas'));

// 2. Declare starContext as const, initialized conditionally
const starContext = starCanvas instanceof HTMLCanvasElement ? starCanvas.getContext('2d') : null;

// Error checking logic moved below the declarations:
if (!starCanvas) {
	displayStatusMessage('ERROR', 'Canvas element with id "canvas" not found or is not an HTMLCanvasElement!');
} else if (!starContext) {
	displayStatusMessage('ERROR', 'Failed to get 2D rendering context for canvas!');
}

// --- Plotting tab CANVAS INITIALIZATION ---

// 3. Declare plotCanvas as const, initialized via lookup
const plotCanvas = /** @type {HTMLCanvasElement | null} */(document.getElementById('plottingCanvas'));

// 4. Declare plotContext as const, initialized conditionally
const plotContext = plotCanvas instanceof HTMLCanvasElement ? plotCanvas.getContext('2d') : null;

// Error checking logic moved below the declarations:
if (!plotCanvas) {
	displayStatusMessage('ERROR', 'Canvas element with id "plottingCanvas" not found or is not an HTMLCanvasElement!');
} else if (!plotContext) {
	displayStatusMessage('ERROR', 'Failed to get 2D rendering plotContext for plotCanvas!');
}

// --- IMAGE LOADING ---

// 5. Declare plotBgImage as const, initialized directly
const plotBgImage = new Image();

// 1. Image Error Handler
plotBgImage.onerror = () => {
	displayStatusMessage('ERROR', "CRITICAL: Failed to load plotImage.jpg. Plotting tab will be blank.");

	initializeApplicationLoops();
	displayStatusMessage('INFO', 'Application started (with image error).');
};

// 2. Image Success Handler
plotBgImage.onload = () => {
	displayStatusMessage('INFO', "Plotting background image loaded successfully. Starting application loops.");
	initializeApplicationLoops();
};

// 3. Start Loading
plotBgImage.src = "plotImage.jpg";

// 4. Loop Initialization Function

/**
 * @summary Starts the stable Two-Tier Update System (rAF for fast display, setTimeout for slow calc).
 */
function initializeApplicationLoops() {
	// 1. Start the Fast Loop (Display) using the safe wrapper function
	window.requestAnimationFrame(function (timestamp) {
		update(timestamp, planeStatus);
	});

	// 2. Start the Slow Loop (Heavy Star Calculation)
	// You must ensure the slowStarUpdate function is defined and SLOW_UPDATE_MS is defined (e.g., 5000)
	setTimeout(slowStarUpdate, SLOW_UPDATE_MS);
}

// #endregion

/**
 * Tier 1: Slow Loop (Heavy Calculation)
 * @summary Runs the full star calculation infrequently (e.g., every 5 seconds).
 */
function slowStarUpdate() {
	// 1. Get the latest stable position from the fast loop's update.
	const currentPosition = planeStatus.position;

	// 2. RUN THE HEAVY CALCULATION (This is the high-CPU work that caused the crash)
	globalVisibleStarsData = calcLocalStarPositions(
		currentPosition,
		sextantView,
		starCelestialCoords
	);

	// 3. Schedule the next heavy calculation
	setTimeout(slowStarUpdate, SLOW_UPDATE_MS);
}

/**
 * The main application loop.
 * @param {number} timestamp - The timestamp provided by requestAnimationFrame (in ms). 
 * @param {PlaneStatus} planeStatus - The plane's current navigational state. 
 * Fetches current plane data, handles sextant control input,
 * recalculates star positions, and orchestrates the drawing of the
 * sextant view, sight reduction data, and navigation plots.
 */
function update(timestamp, planeStatus) {
	// --- 1. Update Plane Status ---
	planeStatus.headingTrueRad = VarGet("A:PLANE HEADING DEGREES TRUE", "Radians");
	planeStatus.magVarRad = toRadians(VarGet("A:MAGVAR", "Degrees"));
	planeStatus.position.longitude = VarGet("A:PLANE LONGITUDE", "Radians");
	planeStatus.position.latitude = VarGet("A:PLANE LATITUDE", "Radians");
	planeStatus.speedKnots = VarGet("A:GROUND VELOCITY", "Number");

	// --- 2. Dead Reckoning ---
	if (fixHistory.length >= 1) {
		updateDeadReckoning(planeStatus, timestamp);
	}

	// --- 3. Input Handling ---
	handleSextantInput(sextantView, inputControl);

	// --- 4. Read the pre-calculated global data. This is very fast. ---
	const visibleStarsData = globalVisibleStarsData;

	// Clear star map from last update
	starContext.fillStyle = "black";
	starContext.fillRect(0, 0, windowW, windowH);

	// --- 5. Display Updates ---
	// Refresh information lines
	setInfoLine(starContext, sextantView);
	setHoLine(starContext, sextantView); 

	// Plot local position of stars and optionally information labels and lines
	setConstellationLines(starContext, starDisplayConfig, visibleStarsData);
	setConstellationLabels(starContext, starDisplayConfig, visibleStarsData);
	setStarIcons(starContext, visibleStarsData)
	setStarLabels(starContext, starDisplayConfig, visibleStarsData);

	// --- 6. Tab Updates ---
	updateSightReductionTab();
	updatePlotTab(fixHistory, plotDisplayConfig);

	// --- 7. Restart the loop (rAF) ---
	// FIX: Use an anonymous function or bind to ensure planeStatus is passed 
	// to the next execution of update() along with the browser's timestamp.
	window.requestAnimationFrame(function (nextTimestamp) {
		update(nextTimestamp, planeStatus);
	});

	//document.getElementById('debug1').innerHTML = "";
	//document.getElementById('debug2').innerHTML = "";
}

// #region Information Line functions

/**
 * @summary Calculates the Observed Altitude (Ho) in degrees based on the pixel position 
 * of the observed line (sextantView.centerPixelY).
 * @description Correctly maps the vertical pixel coordinate (measured from the top, Y=0) 
 * to the altitude angle, where altitude increases as the line moves towards the top of the window.
 * @param {SextantView} viewState - The current state of the sextant view.
 * @returns {number} The observed altitude (Ho) in degrees.
 */
function getHoInDeg(viewState) {

	// 1. Calculate the distance in pixels from the BOTTOM of the window 
	const distanceFromBottom = windowH - viewState.centerPixelY;

	// 2. Normalize the distance (ratio of total height, 0 at bottom, 1 at top)
	const normalizedHeight = distanceFromBottom / windowH;

	// 3. Scale by the Vertical Field of View (fovV) and add the base elevation 
	return (normalizedHeight * viewState.fovV) + viewState.altDeg;
}

/**
 * @summary Updates the information line text displayed on the canvas.
 * @description Displays the sextant view parameters (FOV, AZ, ALT) and the observed altitude 
 * (Ho) along with the pixel resolution, using True North reference where applicable.
 * @param {CanvasRenderingContext2D} starContext - The 2D rendering context for the canvas.
 * @param {SextantView} viewState - The current state of the sextant view configuration.
 * @returns {void}
 */
function setInfoLine(starContext, viewState) {
	starContext.fillStyle = "red";

	// Access properties from viewState
	const fovV = viewState.fovV;
	const fovH = viewState.fovH;

	// Calculate the resolution: Angle subtended by one pixel vertically, in minutes of arc
	// Resolution = (Vertical FOV / Window Height) * 60
	const sexHoResolution = fovV / windowH * 60;

	// Fetch Ho once and use higher precision for display
	const HoDeg = getHoInDeg(viewState);

	// Construct the display string, using toFixed(1) for consistent angular precision
	const infoText =
		"FOVH: " + fovH.toFixed(1) + "° " +
		"FOVV: " + fovV.toFixed(1) + "° " +
		"AZ: " + viewState.azTrueDeg.toFixed(0) + "° " +
		"ALT: " + viewState.altDeg.toFixed(1) + "° " +
		"Ho: " + HoDeg.toFixed(2) + "° " +
		"(" + sexHoResolution.toFixed(1) + " arcmin/pixel)";

	// Use the global constant windowH
	starContext.fillText(infoText, 10, windowH - 10);
}

/**
 * @summary Draws a horizontal red line on the canvas indicating the sextant's observed altitude (Ho) pixel position.
 * @description Uses the viewCenterPixelY coordinate directly, which is measured from the top (Y=0), to draw the horizontal line.
 * @param {CanvasRenderingContext2D} starContext - The 2D rendering context for the canvas.
 * @param {SextantView} viewState - The current state of the sextant view configuration.
 * @returns {void}
 */
function setHoLine(starContext, viewState) {

	starContext.fillStyle = "red";
	starContext.fillRect(0, viewState.centerPixelY, windowW, 1);
}

// #endregion Information Line Functions

// #region Star Map Drawing functions

/**
 * @summary Draws lines between stars to visualize constellations if the drawConstellations flag is set.
 * @description Uses the global 'constellationLines' array and finds the coordinates of connected stars 
 * in 'visibleStarsData' (VisibleStarData[]) using an optimized lookup map.
 * @param {CanvasRenderingContext2D} starContext - The 2D rendering context for the canvas.
 * @param {StarDisplayConfig} displayConfig - The object containing display toggles.
 * @param {Array<VisibleStarData>} visibleStarsData - The list of stars to draw.
 * */
function setConstellationLines(starContext, displayConfig, visibleStarsData) {
	if (displayConfig.drawConstellations == 1) {
		starContext.strokeStyle = "purple";

		// Create a map for O(1) lookups: Map<Star ID, {left, top}>
		const starCoordsMap = new Map();

		// Refactored Map Creation: Use object property names 
		// visibleStarsData is assumed to be globally accessible
		for (const starData of visibleStarsData) {
			/** @type {VisibleStarData} */
			const star = starData;

			starCoordsMap.set(star.starCatalogId, {
				left: star.leftPixel,
				top: star.topPixel
			});
		}

		// constellationLines is assumed to be globally accessible
		for (let linePt = 0; linePt < constellationLines.length; linePt += 2) {
			const startId = constellationLines[linePt];
			const finishId = constellationLines[linePt + 1];

			const startCoords = starCoordsMap.get(startId);
			const finishCoords = starCoordsMap.get(finishId);

			if (startCoords && finishCoords) {
				const left1 = startCoords.left;
				const top1 = startCoords.top;

				const left2 = finishCoords.left;
				const top2 = finishCoords.top;

				starContext.beginPath();
				starContext.moveTo(left1, top1);
				starContext.lineTo(left2, top2);
				starContext.stroke();
			}
		}
	}
}


/**
 * Draws star icons on the canvas. The size and shape of the icon are determined
 * by the star's visual magnitude (visMag), with brighter stars appearing larger.
 * @param {CanvasRenderingContext2D} starContext - The 2D rendering context for the canvas. (Assumed parameter)
 * @param {Array<VisibleStarData>} visibleStarsData - The list of stars to draw.
 * */
function setStarIcons(starContext, visibleStarsData) {
	starContext.fillStyle = "yellow";

	// Iterate over visibleStarsData, asserting the type for property access
	for (const starData of /** @type {VisibleStarData[]} */ (visibleStarsData)) {

		// Extract properties using descriptive names
		const curVisMag = starData.visMag;
		const left = starData.leftPixel;
		const top = starData.topPixel;

		// Icon size is based on magnitude (brighter stars get bigger icons)
		// Magnitude <= 1.5 (Large Cross/Diamond)
		if (curVisMag < 1.5) {
			starContext.fillRect(left, top - 2, 1, 1);
			starContext.fillRect(left - 1, top - 1, 3, 1);
			starContext.fillRect(left - 2, top, 5, 1);
			starContext.fillRect(left - 1, top + 1, 3, 1);
			starContext.fillRect(left, top + 2, 1, 1);
		}
		// Magnitude < 2.3 (3x3 Square)
		else if (curVisMag < 2.3) {
			starContext.fillRect(left - 1, top - 1, 3, 3);
		}
		// Magnitude < 3 (Small Cross)
		else if (curVisMag < 3) {
			starContext.fillRect(left, top - 1, 1, 1);
			starContext.fillRect(left - 1, top, 3, 1);
			starContext.fillRect(left, top + 1, 1, 1);
		}
		// Magnitude >= 3 (Single Pixel)
		else {
			starContext.fillRect(left, top - 1, 1, 1);
		}
	}
}

/**
 * Draws labels (Name or Bayer designation) for stars on the canvas
 * if the labelStars flag is set to 1.
 * @param {CanvasRenderingContext2D} starContext - The 2D rendering context for the canvas.
 * @param {StarDisplayConfig} displayConfig - The object containing display toggles.
 * @param {Array<VisibleStarData>} visibleStarsData - The list of stars to draw.
 * */
function setStarLabels(starContext, displayConfig, visibleStarsData) {
	if (displayConfig.labelStars == 1) {
		starContext.fillStyle = "red";

		// visibleStarsData is assumed to be globally accessible
		for (const starData of /** @type {VisibleStarData[]} */ (visibleStarsData)) {

			const left = starData.leftPixel;
			const top = starData.topPixel;

			const curStarSHAIndex = starData.shaIndex;
			const curStarName = starData.navName;
			const curBayer = starData.bayerDesignation;

			/** @type {string} */
			let starLabel;

			// Logic: Check if the SHA Index string is non-empty (i.e., it is a recognized navigational star).
			if (curStarSHAIndex.length > 0) {
				// Use SHA Index and Nav Name for bright, important stars
				starLabel = "[" + curStarSHAIndex + "] " + curStarName;
			}
			else {
				// Use Bayer Designation for fainter/non-navigational stars
				starLabel = curBayer;
			}

			// Draw the label with a slight offset
			starContext.fillText(starLabel, left + 5, top - 5);
		}
	}
}

/**
 * Draws the constellation name labels next to specific stars
 * if the labelConstellations flag is set to 1.
 * @param {CanvasRenderingContext2D} starContext - The 2D rendering context for the canvas.
 * @param {StarDisplayConfig} displayConfig - The object containing display toggles.
 * @param {Array<VisibleStarData>} visibleStarsData - The list of stars to draw.
 * */
function setConstellationLabels(starContext, displayConfig, visibleStarsData) {
	if (displayConfig.labelConstellations == 1) {
		starContext.fillStyle = "red";

		// visibleStarsData is assumed to be globally accessible
		for (const starData of /** @type {VisibleStarData[]} */ (visibleStarsData)) {

			// Use the descriptive property names
			const left = starData.leftPixel;
			const top = starData.topPixel;

			// The SHA index property is used for filtering bright/important stars
			const curStarSHAIndex = starData.shaIndex;

			// The full constellation name property
			const curConstellation = starData.constellationName;

			// Logic: Only display the constellation name next to bright/navigational stars.
			if (curStarSHAIndex.length > 0) {
				starContext.fillText(curConstellation, left + 5, top - 5);
			}
		}
	}
}

// #endregion Star Map Drawing Functions

// #region Sight reduction tab functions

/**
 * @summary Calculates and returns the fixed celestial coordinates (Right Ascension and Declination)
 * for all stars in the catalog.
 * @description This function iterates through the global starCatalog array,
 * converts the raw Hour-Minute-Second (HMS) coordinates into decimal degrees, and then converts
 * these to Radians.
 * * @global
 * @requires hmsToDecimal - Utility function to convert HMS to decimal degrees.
 * @requires toRadians - Utility function to convert degrees to radians.
 * * @returns {Array<{RA: number, DEC: number}>} The new array storing calculated Right Ascension (RA) and Declination (DEC) values, both in Radians.
 */
function precalculateStarCoordinates() {
	/** @type {Array<{RA: number, DEC: number}>} */
	const calculatedCoords = []; // 

	// *** REFACTORED: Iterate over the consolidated starCatalog array ***
	for (let i = 0; i < starCatalog.length; i++) {
		const star = starCatalog[i];

		// Calculate RA in Radians
		const RAhr = hmsToDecimal(star.RaH, star.RaM, star.RaS);
		const RA = toRadians(RAhr * 15); // Note: RA hours (0-24) * 15 = RA degrees (0-360)

		// Calculate DEC in Radians
		const DEC = toRadians(hmsToDecimal(star.DecD, star.DecM, star.DecS));

		calculatedCoords.push({ RA: RA, DEC: DEC });
	}

	return calculatedCoords;
}

/**
 * @summary Updates the estimated Dead Reckoning (DR) position incrementally based on the plane's
 * current speed and heading over a variable time interval (Δt).
 * @description This function performs a small-step calculation to advance the current DR position 
 * from the last known point using the actual time elapsed between rAF frames.
 * @global
 * @fires VarGet
 * @param {PlaneStatus} planeStatus - The plane's current navigational state, used for speed and heading. 
 * @param {number} currentTimeStamp - The timestamp provided by requestAnimationFrame (in ms). 
 * @modifies {CoordPair} currentDRCoord The current Dead Reckoning coordinate pair (in Radians).
 * @modifies {function(number): number} clampLongitude Function used to wrap longitude to [-PI, +PI].
 * @modifies {number} lastDRUpdateTime Global state for time tracking.
 * @returns {void}
 */
function updateDeadReckoning(planeStatus, currentTimeStamp) {
	// --- 1. Calculate Actual Time Elapsed (Δt) ---
	const deltaTimeMS = currentTimeStamp - lastDRUpdateTime;

	// Prevents issues if the frame rate drops significantly
	const MAX_DELTA_MS = 250;

	// Ignore the first call or large jumps in time
	if (lastDRUpdateTime === 0 || deltaTimeMS > MAX_DELTA_MS) {
		lastDRUpdateTime = currentTimeStamp;
		return;
	}

	// Update the tracker for the next frame
	lastDRUpdateTime = currentTimeStamp;

	// --- 2. Use Δt for Calculation ---
	const MS_PER_HOUR = 3600000;
	const NM_PER_RADIAN_LATITUDE = 3437.74677;

	// Convert time interval from milliseconds to hours (using calculated deltaTimeMS)
	const deltaTimeHours = deltaTimeMS / MS_PER_HOUR;

	/// --- Retrieve Live Data ---
	const speedKnots = planeStatus.speedKnots;
	const headingRadians = planeStatus.headingTrueRad;

	// Calculate Distance Traveled (ΔD) in Nautical Miles (nm)
	const deltaDistanceNM = speedKnots * deltaTimeHours;

	// 3. Calculate Change in Latitude (ΔLat) and Departure (ΔDep) in nm
	const deltaLatNM = deltaDistanceNM * Math.cos(headingRadians);
	const deltaDepNM = deltaDistanceNM * Math.sin(headingRadians);

	// 4. Convert Changes from nm to Radians

	// ΔLat: 1 nm = 1 minute of latitude. Convert minutes to radians.
	const deltaLatRad = deltaLatNM / NM_PER_RADIAN_LATITUDE;

	// ΔLon: Change in Longitude depends on Latitude (parallel of latitude correction).
	const deltaLonRad = deltaDepNM / (NM_PER_RADIAN_LATITUDE * Math.cos(currentDRCoord.latitude));

	// 5. Update Global DR Position
	currentDRCoord.latitude += deltaLatRad;
	currentDRCoord.longitude += deltaLonRad;

	// 6. Apply clamping to Longitude
	currentDRCoord.longitude = clampLongitude(currentDRCoord.longitude);
}

/**
 * @summary Fetches time/date/star data, calculates GHA, SHA, and Dec, 
 * and populates the corresponding HTML elements for a single sighting row.
 * @param {number} curIndex - The row index (0-based) to update in the Sight Reduction table.
 * @param {HTMLCollectionOf<HTMLSelectElement>} starArray - Collection of star name select elements.
 * @returns {number | string} The calculated GHA total in decimal degrees, or "No data" string if time data is unavailable.
 */
function populateSightDataRow(curIndex, starArray) {
	// --- Get DOM Element Collections (Need to be fetched here as they are used across the function)
	/** @type {HTMLCollectionOf<HTMLInputElement>} */
	const APlatArray = /** @type {HTMLCollectionOf<HTMLInputElement>} */(document.getElementsByClassName("AP Lat"));
	/** @type {HTMLCollectionOf<HTMLInputElement>} */
	const APlonArray = /** @type {HTMLCollectionOf<HTMLInputElement>} */(document.getElementsByClassName("AP Lon"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const DateArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Date"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const UTArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("UT"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const GHAhourArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("GHAhour"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const GHAincArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("GHAinc"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const SHAincArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("SHAinc"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const GHAtotalArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("GHAtotal"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const DecArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Dec"));

	// --- 1. Get Time/Date Data ---
	const dayOfMonth = VarGet("E:ZULU DAY OF MONTH", "Number");
	const monthOfYear = VarGet("E:ZULU MONTH OF YEAR", "Number");
	const year = VarGet("E:ZULU YEAR", "Number");
	const time = VarGet("E:ZULU TIME", "Seconds");
	const currentDate = monthOfYear + "/" + dayOfMonth + "/" + year;
	const dayIndexOffset = getElapsedDays(startDate, currentDate);
	const hour = Math.floor(time / 3600);

	// --- 2. Display AP, Date, UT ---
	APlatArray[curIndex].value = formatLatDeg(toDegrees(currentDRCoord.latitude));
	APlonArray[curIndex].value = formatLonDeg(toDegrees(currentDRCoord.longitude));
	DateArray[curIndex].innerHTML = dayOfMonth + "/" + monthOfYear + "/" + year;
	UTArray[curIndex].innerHTML = secondsToTime(time);

	// --- 3. Calculate and Display Aries GHA (Hour) ---
	const ghaDegrees = ariesGHAData.Degrees;
	const ghaMinutes = ariesGHAData.Minutes;
	const dayIndex = dayIndexOffset + 1; // Index 0, 1, or 2 for the three-day window

	let ariesGHA = 0;
	if (dayIndexOffset >= -1 && dayIndexOffset <= 1) {
		ariesGHA = ghaDegrees[dayIndex][hour] + ghaMinutes[dayIndex][hour] / 60;

		GHAhourArray[curIndex].innerHTML = ghaDegrees[dayIndex][hour] + "° " + ghaMinutes[dayIndex][hour] + "'";
	} else {
		GHAhourArray[curIndex].innerHTML = "No data";
		return "No data";
	}

	// --- 4. Interpolate and Display Aries GHA (Increment) ---
	const ariesGHAinc = getGHAincrement(dayIndexOffset + 1, hour, time);
	if (typeof ariesGHAinc === 'number') {
		GHAincArray[curIndex].innerHTML = formatDMdecimal(ariesGHAinc, 1);
	} else {
		GHAincArray[curIndex].innerHTML = ariesGHAinc;
		// Do not return "No data" yet, as star data might still be valuable
	}

	// --- 5. Display Star SHA ---
	// The star selection element holds the star's name, which is used to find its index in the navStarCatalog.

	const curStarName = starArray[curIndex].value;
	const curStarIndex = navStarCatalog.findIndex(star => star.NavStarName === curStarName);

	if (curStarIndex === -1) {
		// Handle case where star name is not found (shouldn't happen if UI is populated correctly)
		SHAincArray[curIndex].innerHTML = "---";
		DecArray[curIndex].innerHTML = "---";
		// Continue, but starSHA will be 0 below, leading to incorrect GHA total
	}

	// Get the consolidated star data object
	const starData = navStarCatalog[curStarIndex];
	const starSHA = starData.SHADegrees + starData.SHAMinutes / 60;
	SHAincArray[curIndex].innerHTML = starData.SHADegrees + "° " + starData.SHAMinutes + "'";

	// --- 6. Display GHA Total ---
	const ariesIncVal = (typeof ariesGHAinc === 'number' ? ariesGHAinc : 0);
	let GHAtotal = ariesGHA + ariesIncVal + starSHA;
	while (GHAtotal > 360) {
		GHAtotal -= 360;
	}
	GHAtotalArray[curIndex].innerHTML = formatDMdecimal(GHAtotal, 1);

	// --- 7. Display Star Dec ---
	let starDEC;
	if (starData.DECdegrees > 0) {
		starDEC = starData.DECdegrees + starData.DECMinutes / 60;
	} else {
		starDEC = starData.DECdegrees - starData.DECMinutes / 60;
	}
	DecArray[curIndex].innerHTML = formatLatDeg(starDEC);

	// Return the total GHA for any post-processing calculation
	return GHAtotal;
}

/**
 * Updates the Sight Reduction Table user interface with current flight and celestial data.
 * This function populates fields related to Assumed Position (AP), Date, Universal Time (UT),
 * Aries GHA, Star SHA, Total GHA, and Star Declination (Dec) for the next available sight.
 */
function updateSightReductionTab() {
	// Cast to HTMLSelectElement collection to access the '.options' property
	/** @type {HTMLCollectionOf<HTMLSelectElement>} */
	const starArray = /** @type {HTMLCollectionOf<HTMLSelectElement>} */(document.getElementsByClassName("starName"));

	// Cast to HTMLElement collection to access the '.innerHTML' property
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const HsArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Hs"));

	// Variable to hold the index
	/** @type {number} */
	let curIndex = -1;

	for (let index = 0; index < HsArray.length; index++) {

		// Check if a star is selected AND the Hs field is empty
		if ((starArray[index].options[0].selected == false) && (HsArray[index].innerHTML == "")) {

			curIndex = index;

			populateSightDataRow(curIndex, starArray);
		}
	}
}

/**
 * @summary Wrapper to call the core takeSighting function, injecting the global arrays for 
 * sighting history (LOP data) and fix history.
 * @returns {void}
 */
function takeSightingWrapper() {
	// Assumes the global arrays are named 'sightHistory' and 'fixHistory'
	takeSighting(sightHistory, fixHistory);
}

/**
 * Initiates a sight reduction calculation when the user takes a sighting.
 * It populates the Sight Reduction Table with current time, position data, and sextant reading.
 * @param {SightingLOPData[]} sightHistory - The array of historical sighting results (LOP data).
 * @param {FixResult[]} fixHistory - The array of historical fix results.
 * @returns {void}
 */
function takeSighting(sightHistory, fixHistory) { 
	// locate first empty Hs field

	/** @type {HTMLCollectionOf<HTMLElement>} */
	const HsArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Hs"));

	let found = false;
	let curIndex = -1;

	// Explicitly cast the collection to HTMLSelectElement array to access '.options'
	/** @type {HTMLCollectionOf<HTMLSelectElement>} */
	const selectStarNameArray = /** @type {HTMLCollectionOf<HTMLSelectElement>} */(document.getElementsByClassName("starName"));

	for (let index = 0; index < HsArray.length && found == false; index++) {
		const hsElement = HsArray[index];
		const starSelectElement = /** @type {HTMLSelectElement} */(selectStarNameArray[index]);

		if ((hsElement.innerHTML == "") && (starSelectElement.options[0].selected == false)) {
			found = true;
			curIndex = index;
		}
	}

	if (found) {
		// Display sextant reading (unique to takeSighting)
		const Hs = getHoInDeg(sextantView);
		HsArray[curIndex].innerHTML = formatDMdecimal(Hs, 1);

		/** @type {CoordPair} */
		const observerDRPosition = currentDRCoord; // Global access maintained

		// Calc Hc and Zn
		/** @type {VisibleStarData[]} */
		const curStarData = calcLocalStarPositions(observerDRPosition, sextantView, starCelestialCoords);

		const selectedStarName = selectStarNameArray[curIndex].value;
		const singleStarData = curStarData.find(starData => starData.navName === selectedStarName);

		if (!singleStarData) {
			// Error handling (Star not found in visible list)
			VarSet("S:errorMsgVar", "NUMBER", 1);
			HsArray[curIndex].innerHTML = "";
			return;
		}

		// The single star object is passed directly to calcHcZn.
		/** @type {SightCalculationResult} */
		const HcZn = calcHcZn(singleStarData, sextantView);

		const HcRad = HcZn.HcRad;
		const ZnRadValue = HcZn.ZnRad;

		/** @type {HTMLCollectionOf<HTMLElement>} */
		const HcArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Hc"));
		HcArray[curIndex].innerHTML = formatDMdecimal(toDegrees(HcRad), 1);
		/** @type {HTMLCollectionOf<HTMLElement>} */
		const ZnArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Zn"));
		ZnArray[curIndex].innerHTML = formatDMdecimal(toDegrees(ZnRadValue), 1);

		// Calculate Intercept 
		const intercept = (Hs - toDegrees(HcRad)) * 60;
		/** @type {HTMLCollectionOf<HTMLElement>} */
		const interceptArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Intercept"));
		interceptArray[curIndex].innerHTML = intercept.toFixed(1) + "nm";

		// Get coordinates of LOP and Zn line intersection (Intercept Point)
		const interceptPointCoord = calculateInterceptPoint(observerDRPosition, toDegrees(ZnRadValue), intercept);
		const interceptPointPixelsCoord = convertCoordToPixels(interceptPointCoord);

		// Store the historical data in the new object structure
		/** @type {SightingLOPData} */
		const currentLOPData = {
			ZnRad: ZnRadValue,
			interceptPointCoord: interceptPointCoord,
			interceptPointPixels: interceptPointPixelsCoord
		};
		sightHistory.push(currentLOPData);

		// Get fix point and store as next assumed point
		if (sightHistory.length % 3 === 0) {

			const cockedHatCoordsFlat = getCockedHatCoords(sightHistory); // Returns [Lat1, Lon1, ...]

			// Arrays to be stored in the FixResult object
			const cockedHatVerticesCoord = [];
			const cockedHatVerticesPixels = [];

			for (let indexNo = 0; indexNo < 6; indexNo += 2) {

				/** @type {CoordPair} */
				const vertexCoord = {
					latitude: cockedHatCoordsFlat[indexNo],
					longitude: cockedHatCoordsFlat[indexNo + 1]
				};
				cockedHatVerticesCoord.push(vertexCoord);

				const vertexPixels = convertCoordToPixels(vertexCoord);
				cockedHatVerticesPixels.push(vertexPixels);
			}

			/** @type {CoordPair} */
			const fixCoord = /** @type {CoordPair} */(getFixCoord(cockedHatVerticesCoord));

			currentDRCoord.latitude = fixCoord.latitude;  
			currentDRCoord.longitude = fixCoord.longitude;  
			const fixCoordPixels = convertCoordToPixels(fixCoord);

			/** @type {FixResult} */
			const currentFixResult = {
				cockedHatVerticesCoord: cockedHatVerticesCoord,
				cockedHatVerticesPixels: cockedHatVerticesPixels,
				fixPositionPixels: fixCoordPixels,
				fixPositionCoord: fixCoord
			};
			fixHistory.push(currentFixResult);
			plotDisplayConfig.plotNeedsUpdate = 1;
		}
	}
}

/**
 * Clears the sightings display area without impacting the stored sighting history. This provides space to take and display more sightings.
 * Display area only shows data for up to six sightings at a time. This button normally pressed after user has completed 6 sightings and 
 * needs more space in display area.
 */
function clearSightingsDisplay() {

	// Cast to HTMLElement for collections only using '.innerHTML'
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const HsArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Hs"));

	// Only clear sighting display if not part way through making a fix - to avoid orphans
	if (sightHistory.length % 3 !== 0) {
		return;
	}

	for (let index = 0; index < HsArray.length; index++) {
		clearSighting(index)
	}
}

/**
 * Clears the most recently used sighting display area (completed) and deletes that sighting from the sight history.
 * If last sighting was part of a fix then deletes associated fix from history. This button is used when user has made a sighting
 * and then realises they had selected the wrong star. If the incorrect sighting was the third in a set and resulted in a fix
 * being recorded the dead reckoning position will be invalid so in that case adjust it to prior fix coordinates.
 */
function deleteLastSighting() {

	// Cast to HTMLElement for collections only using '.innerHTML'
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const interceptArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Intercept"));

	let latestIndex = -1;
	// Find the most recent completed sighting (value in interceptArray)
	for (let index = interceptArray.length - 1; index >= 0; index--) {
		if (interceptArray[index].innerHTML != "") {
			latestIndex = index;
			break;
		}
	}

	// Nothing to delete if no completed sightings
	if (latestIndex === -1) {
		return;
	}

	// Clear sighting display and remove it from sight history
	clearSighting(latestIndex);
	sightHistory.pop();

	// If latest sighting invalidates a fix then remove last fix from fix history, adjust dead reckoning position and flag redraw of plot tab
	if (latestIndex === 2 || latestIndex === 5) {
		fixHistory.pop();
		plotDisplayConfig.plotNeedsUpdate = 1;
		if (fixHistory.length >= 1) {
			currentDRCoord = fixHistory[fixHistory.length - 1].fixPositionCoord
		}
		else {
			currentDRCoord = destCoord;
		}
	}
}

/**
 * Resets the display area for the specified sighting index. Doesn't affect sight history.
 * @param {number} sightingIndex - The index in sighting data display area to be cleared.
 * @returns {void}
 */
function clearSighting(sightingIndex) {
	// Cast to HTMLSelectElement collection to access the '.options' property
	/** @type {HTMLCollectionOf<HTMLSelectElement>} */
	const starArray = /** @type {HTMLCollectionOf<HTMLSelectElement>} */(document.getElementsByClassName("starName"));

	// Cast to HTMLInputElement collection to access the '.value' property
	/** @type {HTMLCollectionOf<HTMLInputElement>} */
	const APlatArray = /** @type {HTMLCollectionOf<HTMLInputElement>} */(document.getElementsByClassName("AP Lat"));
	/** @type {HTMLCollectionOf<HTMLInputElement>} */
	const APlonArray = /** @type {HTMLCollectionOf<HTMLInputElement>} */(document.getElementsByClassName("AP Lon"));

	// Cast to HTMLElement for collections only using '.innerHTML'
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const DateArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Date"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const UTArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("UT"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const HsArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Hs"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const GHAhourArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("GHAhour"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const GHAincArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("GHAinc"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const SHAincArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("SHAinc"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const GHAtotalArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("GHAtotal"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const DecArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Dec"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const HcArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Hc"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const ZnArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Zn"));
	/** @type {HTMLCollectionOf<HTMLElement>} */
	const interceptArray = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("Intercept"));

	starArray[sightingIndex].options[0].selected = true;
	APlatArray[sightingIndex].value = "";
	APlonArray[sightingIndex].value = "";

	DateArray[sightingIndex].innerHTML = "";
	UTArray[sightingIndex].innerHTML = "";
	HsArray[sightingIndex].innerHTML = "";
	GHAhourArray[sightingIndex].innerHTML = "";
	GHAincArray[sightingIndex].innerHTML = "";
	SHAincArray[sightingIndex].innerHTML = "";
	GHAtotalArray[sightingIndex].innerHTML = "";
	DecArray[sightingIndex].innerHTML = "";
	HcArray[sightingIndex].innerHTML = "";
	ZnArray[sightingIndex].innerHTML = "";
	interceptArray[sightingIndex].innerHTML = "";
}

// #endregion

// #region Plotting tab functions

/**
 * Updates the navigation plot tab by drawing the background map,
 * plotting calculated navigation fixes (LOPLOP and Fix coordinates),
 * and optionally showing the final leg to destination and the current plane position.
 * @param {FixResult[]} fixHistory - The array of historical fix results.
 * @param {PlotDisplayConfig} plotDisplayConfig - Configuration for plot tab display. 
 * */
function updatePlotTab(fixHistory, plotDisplayConfig) {// 1. Context and Canvas Safety Check

	// Only update plot if a new fix has been added or displaying plane
	if (plotDisplayConfig.plotNeedsUpdate == 0 && plotDisplayConfig.plotPlane == 0) {
		return;
	}

	// If context is missing or canvas dimensions are 0, exit immediately.
	if (!plotContext || !plotCanvas) return;

	// 2. Image Loading State
	if (!plotBgImage.complete || plotBgImage.naturalWidth === 0) {
		// Draw a placeholder background if image isn't ready
		plotContext.fillStyle = "black";
		plotContext.fillRect(0, 0, plotCanvas.width, plotCanvas.height);
		return;
	}

	// 3. Clear and Draw Background
	plotContext.clearRect(0, 0, plotCanvas.width, plotCanvas.height);
	plotContext.drawImage(plotBgImage, 0, 0);

	// 3. PLOTTING LOGIC: This code runs only if the context and image are confirmed working.

	let hasOutOfBoundsVertex = false;

	// Plot cocked hat vertices (intersections of Lines of Position) and fix coordinates
	for (let fixResult of fixHistory) {
		// Reset flag for EACH fix in the history
		let isCurrentFixOutOfBounds = false;

		// 1. Check if any part of this specific fix is out of bounds
		for (let vertex of fixResult.cockedHatVerticesPixels) {
			if (vertex.left === -1 || vertex.top === -1) {
				isCurrentFixOutOfBounds = true;
				break;
			}
		}

		// 2. Conditional Rendering
		if (!isCurrentFixOutOfBounds) {
			// Plot Cocked Hat Vertices (Green)
			plotContext.fillStyle = "green";
			for (let vertex of fixResult.cockedHatVerticesPixels) {
				plotContext.fillRect(vertex.left - 1, vertex.top - 1, 3, 3);
			}

			// Plot Centroid/Fix Position (Purple)
			plotContext.fillStyle = "purple";
			const fixPixel = fixResult.fixPositionPixels;
			plotContext.fillRect(fixPixel.left - 1, fixPixel.top - 1, 3, 3);
		}
		else {
			// Only show warning for the most recent fix if it's the one off-screen
			// Or keep it simple and show a general warning
			plotContext.fillStyle = "red";
			plotContext.fillText("⚠️ FIX OUTSIDE PLOT AREA", 10, 20);
		}
	}

	// Optionally plot leg info
	if ((plotDisplayConfig.showFinalLeg == 1) && (fixHistory.length >= 1)) {

		/** @type {CoordPair} */
		const lastFixCoord = fixHistory[fixHistory.length - 1].fixPositionCoord;

		/** @type {CoordPair} */
		const currentDestCoord = destCoord; 

		const feetInNauticalMile = 6076.12;
		const finalDistFeet = getDistance(lastFixCoord, currentDestCoord);
		let finalBearingRad = getBearing(lastFixCoord, currentDestCoord);

		finalBearingRad -= planeStatus.magVarRad;
		let finalBearingDeg = Math.floor(toDegrees(finalBearingRad));

		let finalBearingString;
		if (finalBearingDeg < 10) {
			finalBearingString = "00" + finalBearingDeg;
		}
		else if (finalBearingDeg < 100) {
			finalBearingString = "0" + finalBearingDeg;
		}
		else {
			finalBearingString = String(finalBearingDeg);
		}

		plotContext.fillStyle = "red";
		plotContext.fillText("Final Leg: " + finalBearingString + " (" + Math.floor(finalDistFeet / feetInNauticalMile) + "nm)", 70, windowH - 10);

		// Draw lines connecting the fixes
		plotContext.strokeStyle = 'red';
		plotContext.lineWidth = 1;

		// Iterate up to the second-to-last fix (index < fixHistory.length - 1)
		for (let index = 0; index < fixHistory.length - 1; index++) {

			const startFix = fixHistory[index].fixPositionPixels;
			const endFix = fixHistory[index + 1].fixPositionPixels;

			plotContext.beginPath();
			// Start at the current fix
			plotContext.moveTo(startFix.left, startFix.top);
			// Draw a line to the next fix
			plotContext.lineTo(endFix.left, endFix.top);
			plotContext.stroke();
		}
	}

	// Optionally show plane
	if (plotDisplayConfig.plotPlane == 1) {

		/** @type {PixelPosition} */
		const coordPixels = convertCoordToPixels(planeStatus.position);

		const plane = document.getElementById("plane");
		// Check for the plane DOM element before accessing its style
		if (plane) {
			// Update the style of the separate plane DOM element
			plane.style.transform = "rotate(" + toDegrees(planeStatus.headingTrueRad) + "deg)";
			// 99 is a hardcoded offset for the surrounding UI elements
			plane.style.top = coordPixels.top - 15 + 99 + "px";
			plane.style.left = coordPixels.left - 15 + "px";
		}
		// Draw a small red box on the canvas for the plane's position
		plotContext.fillStyle = "red";
		plotContext.fillRect(coordPixels.left - 1, coordPixels.top - 1, 3, 3);
	}

	plotDisplayConfig.plotNeedsUpdate = 0;
}

// #endregion

// #region Formatting utility functions

/**
 * @summary Formats a decimal degree coordinate into a Degrees/Minutes string (D° M.M').
 * @param {number} coordinate - The angle in decimal degrees.
 * @param {number} numberPlaces - The number of decimal places for the minutes part.
 * @returns {string} The formatted coordinate string.
 */
function formatDMdecimal(coordinate, numberPlaces) {
	// Separate the integer degrees part
	const degrees = Math.floor(coordinate);

	// Calculate the decimal minutes part and fix the number of decimal places
	// (coordinate - degrees) gives the fractional part, which is then multiplied by 60
	const minutes = ((coordinate - degrees) * 60).toFixed(numberPlaces);

	return degrees + "° " + minutes + "'";
}

/**
 * @summary Formats a decimal degree latitude into a labeled Degrees/Minutes string (e.g., "N 34° 15.5'").
 * @param {number} latitude - The latitude in decimal degrees (-90 to +90).
 * @returns {string} The formatted latitude string.
 */
function formatLatDeg(latitude) {
	// Determine the hemisphere prefix
	const prefix = latitude >= 0 ? "N" : "S";

	// Work with the absolute value to simplify degree/minute calculation
	const absLatitude = Math.abs(latitude);

	// Separate the integer degrees part
	const degrees = Math.floor(absLatitude);

	// Calculate the decimal minutes part and fix to one decimal place
	// absLatitude - degrees gives the fractional part, which is multiplied by 60
	const minutes = ((absLatitude - degrees) * 60).toFixed(1);

	// Construct and return the final string
	return `${prefix} ${degrees}° ${minutes}'`;
}

/**
 * @summary Formats a decimal degree longitude into a labeled Degrees/Minutes string (e.g., "E 120° 45.3'").
 * @param {number} longitude - The longitude in decimal degrees (-180 to +180).
 * @returns {string} The formatted longitude string.
 */
function formatLonDeg(longitude) {
	// 1. Determine the hemisphere prefix (East for >= 0, West for < 0)
	const prefix = longitude >= 0 ? "E" : "W";

	// 2. Work with the absolute value to simplify degree/minute calculation
	const absLongitude = Math.abs(longitude);

	// 3. Separate the integer degrees part
	const degrees = Math.floor(absLongitude);

	// 4. Calculate the decimal minutes part and fix to one decimal place
	// (absLongitude - degrees) gives the fractional part, multiplied by 60
	const minutes = ((absLongitude - degrees) * 60).toFixed(1);

	// 5. Construct and return the final string using a template literal
	return `${prefix} ${degrees}° ${minutes}'`;
}

/**
 * @summary Converts total seconds into a "HH:MM:SS" time string, zero-padded.
 * @param {number} e - The time in total seconds (e.g., from midnight UT).
 * @returns {string} The formatted time string.
 */
function secondsToTime(e) {
	const totalSeconds = e;

	// Calculate hours (h), minutes (m), and seconds (s), ensuring they are 2-digit strings
	const h = Math.floor(totalSeconds / 3600).toString().padStart(2, '0');
	const m = Math.floor(totalSeconds % 3600 / 60).toString().padStart(2, '0');
	const s = Math.floor(totalSeconds % 60).toString().padStart(2, '0');

	// Use standard string concatenation for maximum compatibility with older browser engines
	return h + ':' + m + ':' + s;
}

// #endregion

// #region Button onClick functions for handling info buttons

/**
 * @summary Wrapper to call toggleStarNames(), injecting the global display configuration object.
 * @returns {void}
 */
function toggleStarNamesWrapper() {
	// Assumes the global object is named 'starDisplayConfig'
	toggleStarNames(starDisplayConfig);
}

/**
 * @summary Toggles the display of star names/designations (labelStars) property.
 * @param {StarDisplayConfig} displayConfig - The object containing display toggles.
 * @returns {void}
 */
function toggleStarNames(displayConfig) {
	// Toggles the property in the state object
	if (displayConfig.labelStars == 0)
		displayConfig.labelStars = 1;
	else
		displayConfig.labelStars = 0;

	// Alternative concise syntax: displayConfig.labelStars = 1 - displayConfig.labelStars;
}

/**
 * @summary Wrapper to call toggleConstellationLines(), injecting the global display configuration object.
 * @returns {void}
 */
function toggleConstellationLinesWrapper() {
	// Assumes the global object is named 'starDisplayConfig'
	toggleConstellationLines(starDisplayConfig);
}

/**
 * @summary Toggles the display of lines between stars (drawConstellations) property.
 * @param {StarDisplayConfig} displayConfig - The object containing display toggles.
 * @returns {void}
 */
function toggleConstellationLines(displayConfig) {
	// Toggles the property in the state object
	if (displayConfig.drawConstellations == 0)
		displayConfig.drawConstellations = 1;
	else
		displayConfig.drawConstellations = 0;
}

/**
 * @summary Wrapper to call toggleConstellationNames(), injecting the global display configuration object.
 * @returns {void}
 */
function toggleConstellationNamesWrapper() {
	// Assumes the global object is named 'starDisplayConfig'
	toggleConstellationNames(starDisplayConfig);
}

/**
 * @summary Toggles the display of constellation names (labelConstellations) property.
 * @param {StarDisplayConfig} displayConfig - The object containing display toggles.
 * @returns {void}
 */
function toggleConstellationNames(displayConfig) {
	// Toggles the property in the state object
	if (displayConfig.labelConstellations == 0)
		displayConfig.labelConstellations = 1;
	else
		displayConfig.labelConstellations = 0;
}

// #endregion

// #region Calculation functions used in this script file only
/**
 * @summary Calculates the interpolated Greenwich Hour Angle (GHA) increment for minutes and seconds.
 * @param {number} day - The day index offset (0, 1, or 2).
 * @param {number} hour - The current hour (0-23).
 * @param {number} time - The current Universal Time in seconds.
 * @returns {number | string} The GHA increment in decimal degrees, or the string "No data" on error.
 */
function getGHAincrement(day, hour, time) {
	const ghaDegrees = ariesGHAData.Degrees;
	const ghaMinutes = ariesGHAData.Minutes;

	// Keep original day/hour for start GHA access
	const startDay = day;
	const startHour = hour;

	// Convert start hour GHA to decimal
	const startGHAdec = ghaDegrees[startDay][startHour] + ghaMinutes[startDay][startHour] / 60;

	// Get finish hour GHA and convert to decimal
	if (day < 0) {
		return "No data"
	}

	// Update day/hour indices for the finish GHA access (the next hour's value)
	if (hour == 23) {
		day += 1;
		hour = 0;
	}
	else {
		hour += 1;
	}

	if (day > 2) {
		return "No data"
	}

	// Now use the updated day/hour indices to get the finish GHA.
	const finishGHAdec = ghaDegrees[day][hour] + ghaMinutes[day][hour] / 60;

	// Calc increment as decimal
	const hourProportion = (time % 3600) / 3600;

	// Calculate the difference, ensuring it's a positive angle (mod 360) for interpolation
	return ((finishGHAdec - startGHAdec + 360) % 360) * hourProportion;
}

/**
 * @summary Calculates the difference in days between two dates.
 * @param {string} startDate - The starting date string (e.g., "MM/DD/YYYY").
 * @param {string} currentdate - The current date string (e.g., "MM/DD/YYYY").
 * @returns {number} The number of days elapsed (can be negative).
 */
function getElapsedDays(startDate, currentdate) {
	// Create Date objects from the input strings
	const dateStart = new Date(startDate);
	const dateCurrent = new Date(currentdate);

	// Calculate the time difference in milliseconds
	const Difference_In_Time = dateCurrent.getTime() - dateStart.getTime();

	// Define constants for conversion
	const MILLISECONDS_IN_SECOND = 1000;
	const SECONDS_IN_HOUR = 3600;
	const HOURS_IN_DAY = 24;
	const MILLISECONDS_IN_DAY = MILLISECONDS_IN_SECOND * SECONDS_IN_HOUR * HOURS_IN_DAY;

	// To calculate the no. of days between two dates
	return Difference_In_Time / MILLISECONDS_IN_DAY;
}

// #endregion

// #region Functions for adjusting FOV, AV, ALT, Ho

/**
 * @summary Processes user input flags to update the sextant view state (viewState).
 * This function consolidates all view manipulation logic.
 * @param {SextantView} viewState - The mutable state object for the sextant view.
 * @param {InputControl} inputFlags - The object containing input control flags (azimuth, altitude, fov).
 * @returns {void}
 */
function handleSextantInput(viewState, inputFlags) {

	// Alias movement deltas from the inputFlags object
	const deltaAZ = inputFlags.azimuth;
	const deltaALT = inputFlags.altitude;
	const deltaFOV = inputFlags.fieldOfView;

	// AZIMUTH (Horizontal Rotation)
	if (deltaAZ !== 0) {
		viewState.azTrueDeg = (viewState.azTrueDeg + deltaAZ + 360) % 360;
	}

	// ALTITUDE (Vertical Panning)
	if (deltaALT !== 0) {
		viewState.altDeg = viewState.altDeg + deltaALT;

		// Bottom Clamp
		if (viewState.altDeg < 0) {
			viewState.altDeg = 0;
		}

		// Top Clamp
		if (viewState.altDeg > (90 - viewState.fovV)) {
			viewState.altDeg = 90 - viewState.fovV;
		}
	}

	// FIELD OF VIEW (FOV)
	if (deltaFOV !== 0) {
		// adjustFOV must be updated to accept the delta and the viewState
		adjustFOV(deltaFOV, viewState);
	}
}

/**
 * @summary Adjusts and clamps the fovH and fovV properties of the SextantView object based on input.
 * @param {number} alterFOV - The change in horizontal FOV requested.
 * @param {SextantView} viewState - The mutable state object for the sextant view.
 * @returns {void}
 */
function adjustFOV(alterFOV, viewState) {
	const MAX_HUMAN_FOV_H = 170;
	const MAX_HUMAN_FOV_V = 130;
	const ASPECT_RATIO = windowH / windowW;

	if (alterFOV === 0) {
		return;
	}

	// Read current fovH from state
	let newFovH = viewState.fovH + alterFOV;

	// 1. Enforce minimum FOV (H cannot be negative)
	newFovH = Math.max(0, newFovH);

	// 2. Calculate the maximum FOV H allowed by the vertical limit
	// maxFovH_by_V = MAX_HUMAN_FOV_V / (windowH / windowW)
	const maxFovH_by_V = MAX_HUMAN_FOV_V / ASPECT_RATIO;

	// 3. The limiting factor is the smaller of MAX_HUMAN_FOV_H or maxFovH_by_V
	const maxFovH = Math.min(MAX_HUMAN_FOV_H, maxFovH_by_V);

	// 4. Clamp the new horizontal FOV to the final determined maximum
	newFovH = Math.min(newFovH, maxFovH);

	// 5. Update both Fovs in the state object (maintaining aspect ratio)
	viewState.fovH = newFovH;
	viewState.fovV = newFovH * ASPECT_RATIO; // Recalculate V based on the final H
}

/**
 * @summary Wrapper to call the refactored panFOV function, passing the required panAmount and the global InputControl state object.
 * @param {number} panAmount - The rate of change for the FOV (e.g., -5 or 5).
 */
function panFOVWrapper(panAmount) {
	panFOV(panAmount, inputControl);
}

/**
 * @summary Sets the value of the inputControl.fieldOfView property, which is used to continuously pan the Field of View (FOV).
 * @param {number} panAmount - The rate of change for the FOV (e.g., -5, 5, or 0).
 * @param {InputControl} inputFlags - The object containing input control flags.
 * @returns {void}
 */
function panFOV(panAmount, inputFlags) {
	inputFlags.fieldOfView = panAmount;
}

function freezeFOVWrapper() {
	freezeFOV(inputControl);
}

/**
 * @summary Resets the inputControl.fieldOfView property to 0, stopping any continuous Field of View (FOV) panning.
 * @param {InputControl} inputFlags - The object containing input control flags.
 * @returns {void}
 */
function freezeFOV(inputFlags) {
	inputFlags.fieldOfView = 0;
}

function moveFOVresetWrapper() {
	moveFOVreset(sextantView);
}

/**
 * @summary Resets the horizontal and vertical Fields of View (FOV) to the default setting in the viewState object.
 * @description The vertical FOV (fovV) is calculated based on the aspect ratio (windowH / windowW) to maintain visual consistency.
 * @param {SextantView} viewState - The mutable state object for the sextant view.
 * @returns {void}
 */
function moveFOVreset(viewState) {
	// 1. Reset horizontal FOV to the global default constant.
	viewState.fovH = defaultFOV;

	// 2. Calculate vertical FOV based on the horizontal FOV and the window's aspect ratio.
	/** @type {number} */
	const aspectRatio = windowH / windowW;
	viewState.fovV = viewState.fovH * aspectRatio;
}

/**
 * @summary Wrapper to call the refactored panAZ function, passing the required panAmount and the global InputControl state object.
 * @param {number} panAmount - The rate of change for the Azimuth (e.g., -1, 1, -5, or 5).
 */
function panAZWrapper(panAmount) {
	panAZ(panAmount, inputControl);
}

/**
 * @summary Sets the value of inputFlags.azimuth, which is used to continuously pan the Azimuth (AZ) view.
 * @param {number} panAmount - The rate of change for the Azimuth (e.g., -1, 1, or 0).
 * @param {InputControl} inputFlags - The object containing input control flags.
 * @returns {void}
 */
function panAZ(panAmount, inputFlags) {
	inputFlags.azimuth = panAmount;
}

function freezeAZWrapper() {
	freezeAZ(inputControl);
}

/**
 * @summary Resets the inputFlags.azimuth property to 0, stopping any continuous Azimuth (AZ) panning.
 * @param {InputControl} inputFlags - The object containing input control flags.
 * @returns {void}
 */
function freezeAZ(inputFlags) {
	inputFlags.azimuth = 0;
}

function moveAZresetWrapper() {
	moveAZreset(sextantView, planeStatus);
}

/**
 * @summary Resets the Sextant Azimuth (viewState.azTrueDeg) to the plane's current **True Heading**.
 * @param {SextantView} viewState - The mutable state object for the sextant view.
 * @param {PlaneStatus} planeStatus - The plane's current navigational state. 
 * @returns {void}
 */
function moveAZreset(viewState, planeStatus) { 
	// 1. Calculate the desired azimuth (True Heading (T) in Degrees)
	const TRUE_HEADING_DEG = Math.floor(toDegrees(planeStatus.headingTrueRad));

	// 2. Normalize the angle to be between 0 and 360 degrees.
	viewState.azTrueDeg = (TRUE_HEADING_DEG % 360 + 360) % 360;
}

/**
 * @summary Wrapper to call the refactored panALT function, passing the required panAmount and the global InputControl state object.
 * @param {number} panAmount - The rate of change for the Altitude (e.g., -1, 1, -5, or 5).
 */
function panALTWrapper(panAmount) {
	panALT(panAmount, inputControl);
}

/**
 * @summary Sets the value of inputFlags.altitude, which is used to continuously pan the Altitude (ALT) view.
 * @param {number} panAmount - The rate of change for the Altitude (e.g., -1, 1, or 0).
 * @param {InputControl} inputFlags - The object containing input control flags.
 * @returns {void}
 */
function panALT(panAmount, inputFlags) {
	inputFlags.altitude = panAmount;
}

function freezeALTWrapper() {
	freezeALT(inputControl);
}

/**
 * @summary Resets the inputFlags.altitude property to 0, stopping any continuous Altitude (ALT) panning.
 * @param {InputControl} inputFlags - The object containing input control flags.
 * @returns {void}
 */
function freezeALT(inputFlags) {
	inputFlags.altitude = 0;
}

function moveALTresetWrapper() {
	moveALTreset(sextantView);
}

/**
 * @summary Resets the Sextant Altitude (viewState.altDeg) to 0 degrees, centering the view on the horizon.
 * @param {SextantView} viewState - The mutable state object for the sextant view.
 * @returns {void}
 */
function moveALTreset(viewState) {
	viewState.altDeg = 0;
}

/**
 * @summary Wrapper to call the refactored adjustHo function, passing the pixel adjustment amount and the global sextantView state object.
 * @param {number} adjustment - The change in pixel position (e.g., -50 for Ho +50, or +50 for Ho -50).
 */
function adjustHoWrapper(adjustment) {
	// Passes the global sextantView object
	adjustHo(adjustment, sextantView);
}

/**
 * @summary Adjusts the Observed Altitude (Ho) by modifying the pixel position (viewState.centerPixelY).
 * @param {number} adjustment - The change in pixel position (Negative for Ho UP/move UP, Positive for Ho DOWN/move DOWN).
 * @param {SextantView} viewState - The mutable state object for the sextant view.
 * @returns {void}
 */
function adjustHo(adjustment, viewState) {
	let currentY = viewState.centerPixelY;
	let newY = currentY + adjustment;

	// windowH is assumed to be a top-level constant

	if (adjustment < 0) {
		// Moving UP (decrementing Y, increasing Ho)
		// Clamp at the top edge (Y=0)
		viewState.centerPixelY = Math.max(0, newY);
	} else if (adjustment > 0) {
		// Moving DOWN (incrementing Y, decreasing Ho)
		// Clamp at the bottom edge (Y=windowH - 1)
		viewState.centerPixelY = Math.min(windowH - 1, newY);
	}
	// If adjustment is 0, nothing changes.
}

/**
 * @summary Wrapper to call the refactored moveHoReset function, passing the global sextantView state object.
 */
function moveHoResetWrapper() {
	// Passes the global sextantView object
	moveHoReset(sextantView);
}

/**
 * @summary Resets the Sextant Observed Line (viewState.centerPixelY) to the vertical center of the window/canvas.
 * @param {SextantView} viewState - The mutable state object for the sextant view.
 * @returns {void}
 */
function moveHoReset(viewState) {
	viewState.centerPixelY = Math.floor(windowH / 2);
}

// #endregion

// #region Button onClick functions for plotting tab

/**
 * @summary Wrapper to call the refactored plotFinalLeg with the global configuration object.
 */
function plotFinalLegWrapper() {
	// Passes the global const object to the refactored function
	plotFinalLeg(plotDisplayConfig);
}

/**
 * @summary Toggles the display of the final navigation leg on the plot tab.
 * @description Modifies the showFinalLeg property of the plot configuration object and updates the button text.
 * @param {PlotDisplayConfig} plotDisplayConfig - The configuration object for the plot display.
 */
function plotFinalLeg(plotDisplayConfig) {
	// Access the property directly on the configuration object
	if (plotDisplayConfig.showFinalLeg == 0) {
		plotDisplayConfig.showFinalLeg = 1;
		document.getElementById("finalLegButton").innerHTML = "Hide Leg Info";
		plotDisplayConfig.plotNeedsUpdate = 1;
	}
	else {
		plotDisplayConfig.showFinalLeg = 0;
		document.getElementById("finalLegButton").innerHTML = "Show Leg Info";
		plotDisplayConfig.plotNeedsUpdate = 1;
	}
}

/**
 * @summary Wrapper to call the refactored hidePlane with the global configuration object.
 */
function hidePlaneWrapper() {
	// Passes the global const object to the refactored function
	hidePlane(plotDisplayConfig);
}

/**
 * @summary Hides the plane marker on the plot and updates the button visibility.
 * @param {PlotDisplayConfig} plotDisplayConfig - The configuration object for the plot display.
 */
function hidePlane(plotDisplayConfig) {
	plotDisplayConfig.plotPlane = 0;
	plotDisplayConfig.plotNeedsUpdate = 1;

	// 2. DOM Manipulation (Kept as is for button logic)
	document.getElementById('plane').style.display = 'none';
	document.getElementById('showButton').style.display = 'inline';
	document.getElementById('hideButton').style.display = 'none';
}

/**
 * @summary Wrapper to call the refactored showPlane with the global configuration object.
 */
function showPlaneWrapper() {
	// Passes the global const object to the refactored function
	showPlane(plotDisplayConfig);
}

/**
 * @summary Shows the plane marker on the plot and updates the button visibility.
 * @param {PlotDisplayConfig} plotDisplayConfig - The configuration object for the plot display.
 */
function showPlane(plotDisplayConfig) {
	plotDisplayConfig.plotPlane = 1;

	// 2. DOM Manipulation (Kept as is for button logic)
	document.getElementById('plane').style.display = 'inline';
	document.getElementById('hideButton').style.display = 'inline';
	document.getElementById('showButton').style.display = 'none';
}

// #endregion

// #region openPage function

/**
 * @summary Opens the selected tab and updates the button styling.
 * @param {string} pageName - The name of the tab to open.
 */
function openPage(pageName) {
	/** @type {HTMLCollectionOf<HTMLElement>} */
	var tabcontent;

	tabcontent = /** @type {HTMLCollectionOf<HTMLElement>} */(document.getElementsByClassName("tabcontent"));
	for (let i = 0; i < tabcontent.length; i++) { // Use 'let i' for safety
		tabcontent[i].style.display = "none";
	}

	/** @type {HTMLElement | null} */
	const tab = document.getElementById(pageName);
	if (tab) tab.style.display = "block";
}

// #endregion