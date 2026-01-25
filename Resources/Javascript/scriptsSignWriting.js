// --- Title comment block ---
/**
 * @file scriptsSignWriting.js
 * @description Handles the drawing logic for the plane's smoke trail and gate caps on the canvas.
 * Integrates with external simulator variables to update plane position and smoke state.
 * @author [David Kilpatrick]
 * @version 1.0.0
 * @date 2025-07-19
*/

// Import the necessary Geodesy classes.
import LatLon from './third-party/geodesy/latlon-ellipsoidal.js';

// #region Global declarations

// #region JSDoc Global Variable Declarations (Injected by C#)

// These variables are declared globally. Their string values are expected to be
// injected/replaced by the C# application before script execution.
// The JSDoc provides type information for the TypeScript checker.

/** @type {Gate[]} gates - Array of gates information comprising the complete sign writing message.*/
const gates = null;

/** @type {HTMLCanvasElement | null} canvas - Reference to the main HTML canvas element for drawing.*/
let canvas = null;

/** @type {CanvasRenderingContext2D | null} context - The 2D rendering context of the canvas, used for drawing operations.*/
let context = null;

/** @type {number} charPaddingLeft - Padding from left edge to the point of horizontal segments.*/
const charPaddingLeft = null;

/** @type {number} charPaddingTop - Padding from top to the point of vertical segments.*/
const charPaddingTop = null;

/** @type {number} canvasWidth - Message canvas width in pixels.*/
const canvasWidth = null;

/** @type {number} canvasHeight - Message canvas height in pixels.*/
const canvasHeight = null;

/** @type {number} consoleWidth - Message console width in pixels.*/
const consoleWidth = null;

/** @type {number} consoleHeight - Message console height in pixels.*/
const consoleHeight = null;

/** @type {number} windowHorizontalPadding - Horizontal window padding around and between canvas and console in pixels.*/
const windowHorizontalPadding = null;

/** @type {number} windowVerticalPadding - Vertical window padding around and between canvas and console in pixels.*/
const windowVerticalPadding = null;

// #endregion

// #region Application State Variables (Script-Managed Globals)

/** @type {number} currentSegmentDistance - Should be constant if gates have been set up correctly by C# application code.*/
var currentSegmentDistance = 0;

/** @type {number} smokeOld - Tracks the previous value of the smokeOn variable to detect when its state changes.*/
var smokeOld = 0;

/** @type {boolean} smokeHasToggled - Acts as a flag to indicate if smokeOn has just toggled, controlling when certain drawing logic should execute.*/
var smokeHasToggled = false;

/** @type {number} planeTopPixels - Origin is 0,0 top left corner of canvas. References top coordinate of pixel representing plane position on canvas.*/
var planeTopPixels;

/** @type {number} planeLeftPixels - Origin is 0,0 top left corner of canvas. References left coordinate of pixel representing plane position on canvas.*/
var planeLeftPixels;

/** @type {string[]} colours - Segments are red and outlined in black.*/
const colours = ["", "red", "black"];

/** @type {number} metresInFoot - The number of metres in one foot.*/
const metresInFoot = .3048;

/** @type {number} segmentLengthPixels - The length of a segment from point of start endcap to point of finish endcap in pixels.*/
const segmentLengthPixels = 32;

/** @type {number[][]} endCap - The cap at each end of segment is 11 x 6 pixels. Array is 11 x 11 as it gets rotated for rendering for different
 * approach directions to gate. The array is indexed by row, then column.
 */
const endCap = [];
endCap[0] = [0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0];
endCap[1] = [0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0];
endCap[2] = [0, 0, 0, 2, 2, 1, 2, 2, 0, 0, 0];
endCap[3] = [0, 0, 2, 2, 1, 1, 1, 2, 2, 0, 0];
endCap[4] = [0, 2, 2, 1, 1, 1, 1, 1, 2, 2, 0];
endCap[5] = [2, 2, 1, 1, 1, 1, 1, 1, 1, 2, 2];
endCap[6] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
endCap[7] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
endCap[8] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
endCap[9] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
endCap[10] = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

/** @type {number[][]} curCap - Used to hold current orientation version of endCap.*/
var curCap = [];
curCap[0] = [];
curCap[1] = [];
curCap[2] = [];
curCap[3] = [];
curCap[4] = [];
curCap[5] = [];
curCap[6] = [];
curCap[7] = [];
curCap[8] = [];
curCap[9] = [];
curCap[10] = [];

// #endregion

// #endregion

// --- Script-Managed Global Variables ---
// These variables are declared here and will be populated/assigned
// by the JavaScript code itself, typically after DOM content is loaded.

/**
 * @summary Initializes the application upon DOM content loading.
 *
 * @description
 * This event listener waits for the entire HTML document to be parsed
 * (but not necessarily all sub-resources like images to be loaded).
 * It performs the following critical setup tasks:
 * 1. Identifies and initializes the main drawing canvas and its 2D rendering context.
 * Includes error handling if the canvas element is not found or context cannot be obtained.
 * 2. Sets CSS custom properties (variables) on the document's root element (`:root`)
 * based on the parsed dimensions, which allows CSS to adapt to the dynamic layout.
 * 3. Initiates the main animation loop by calling `window.requestAnimationFrame(update)`.
 *
 * @listens DOMContentLoaded
 */
document.addEventListener('DOMContentLoaded', () => {
	// Assign canvas and context ONLY after the DOM is fully loaded
	const foundCanvas = document.getElementById('canvas');

    // Calculate segment distance based on the first two gates
	currentSegmentDistance = calculate3DDistance(gates[0].coordinates.latitude, gates[0].coordinates.longitude, gates[0].altitude,
		gates[1].coordinates.latitude, gates[1].coordinates.longitude, gates[1].altitude);

	if (foundCanvas instanceof HTMLCanvasElement) {
		// Now TypeScript knows foundCanvas is an HTMLCanvasElement
		canvas = foundCanvas;
		canvas.width = canvasWidth;
		canvas.height = canvasHeight;
		context = canvas.getContext('2d');

		if (!context) {
			console.error('Failed to get 2D rendering context for canvas!');
		}
	} else {
		console.error('Canvas element with id "canvas" not found or is not an HTMLCanvasElement!');
	}

	document.documentElement.style.setProperty('--canvas-width', canvasWidth + 'px');
	document.documentElement.style.setProperty('--canvas-height', canvasHeight + 'px');
	document.documentElement.style.setProperty('--console-width', consoleWidth + 'px');
	document.documentElement.style.setProperty('--console-height', consoleHeight + 'px');
	document.documentElement.style.setProperty('--window-horizontal-padding', windowHorizontalPadding + 'px');
	document.documentElement.style.setProperty('--window-vertical-padding', windowVerticalPadding + 'px');
	// ** NEW: Start the throttled update loop **
	scheduleUpdate(); // Call a new function to start the loop
});

/**
 * @summary Schedules the next execution of the update function.
 * @description Uses setTimeout to control the execution rate to approximately 5 times per second (200ms).
 */
function scheduleUpdate() {
	setTimeout(() => {
		update();
		// Recursively call scheduleUpdate to maintain the loop
		scheduleUpdate();
	}, 200); // 200ms = 5 updates per second
}

/**
 * Calculates the 3D Euclidean distance between two geographical points,
 * including their altitudes, by converting them to Earth-Centered, Earth-Fixed (ECEF)
 * Cartesian coordinates.
 *
 * This function relies on the `LatLon` class from the Geodesy library (latlon-ellipsoidal.js).
 *
 * @param {number} lat1 - Latitude of point 1 in degrees.
 * @param {number} lon1 - Longitude of point 1 in degrees.
 * @param {number} alt1 - Altitude of point 1 in meters AMSL.
 * @param {number} lat2 - Latitude of point 2 in degrees.
 * @param {number} lon2 - Longitude of point 2 in degrees.
 * @param {number} alt2 - Altitude of point 2 in meters AMSL.
 * @returns {number} The 3D distance in meters. Returns NaN if inputs are invalid.
 */
function calculate3DDistance(lat1, lon1, alt1, lat2, lon2, alt2) {
	try {
		// Create LatLon objects for each point, including altitude
		const p1 = new LatLon(lat1, lon1, alt1);
		const p2 = new LatLon(lat2, lon2, alt2);

		// Convert geographical coordinates to ECEF Cartesian coordinates
		const cartesian1 = p1.toCartesian();
		const cartesian2 = p2.toCartesian();

		// Calculate the Euclidean distance between the two Cartesian points
		const dx = cartesian1.x - cartesian2.x;
		const dy = cartesian1.y - cartesian2.y;
		const dz = cartesian1.z - cartesian2.z; // Corrected: 'const' added for declaration

		const distance = Math.sqrt(dx * dx + dy * dy + dz * dz);

		return distance;
	} catch (error) {
		console.error(`Calculation error in calculate3DDistance: ${error.message}`);
		return NaN;
	}
}

/**
 * @summary Rotates a given 2D square array (matrix) by a specified angle.
 *
 * @description
 * This function takes an input square array and rotates its elements
 * based on the `rotation` parameter (0, 90, 180, or 270 degrees clockwise).
 * The result of the rotation is stored in the `outArray`.
 * It's particularly useful for rotating pixel-based shapes or patterns
 * like the `endCap` array to match different orientations.
 *
 * @param {number[][]} inArray - The input square 2D array of numbers to be rotated.
 * @param {number[][]} outArray - The output square 2D array of numbers where the rotated elements will be stored.
 * Must be pre-initialized to the same dimensions as `inArray`.
 * @param {number} rotation - The rotation angle in degrees (expected values: 0, 90, 180, 270).
 * Rotation is assumed to be clockwise.
 */
function rotateSquareArray(inArray, outArray, rotation) {
	if (rotation == 0)
		for (let row = 0; row < inArray.length; row++)
			for (let col = 0; col < inArray.length; col++) {
				outArray[row][col] = inArray[row][col];
			}
	else if (rotation == 90)
		for (let row = 0; row < inArray.length; row++)
			for (let col = 0; col < inArray.length; col++) {
				outArray[col][inArray.length - 1 - row] = inArray[row][col];
			}
	else if (rotation == 180)
		for (let row = 0; row < inArray.length; row++)
			for (let col = 0; col < inArray.length; col++) {
				outArray[inArray.length - 1 - row][col] = inArray[row][col];
			}
	else if (rotation == 270)
		for (let row = 0; row < inArray.length; row++)
			for (let col = 0; col < inArray.length; col++) {
				outArray[inArray.length - 1 - col][row] = inArray[row][col];
			}
}

/**
 * @summary Draws a pixel-based segment cap onto the canvas at a specified position.
 *
 * @description
 * This function iterates through a given 2D numerical array (`capArray`),
 * treating each non-zero element as a pixel to be drawn. It uses the value
 * of the element to determine the color from the global `colours` array.
 * Each pixel is drawn as a 1x1 rectangle on the canvas, offset by the
 * provided `top` (Y-coordinate) and `left` (X-coordinate) origin.
 * Elements with a value of `0` in `capArray` are skipped (not drawn).
 *
 * @param {number} top - The top (Y-coordinate) pixel position on the canvas
 * where the top-left corner of the `capArray` drawing should start.
 * @param {number} left - The left (X-coordinate) pixel position on the canvas
 * where the top-left corner of the `capArray` drawing should start.
 * @param {number[][]} capArray - A 2D array of numbers representing the pixel pattern of the cap.
 * Non-zero values correspond to indices in the `colours` array (1 for red, 2 for black).
 */
function drawCap(top, left, capArray) {
	for (let row = 0; row < capArray.length; row++) {
		for (let col = 0; col < capArray.length; col++) {
			if (capArray[row][col] != 0) {
				context.fillStyle = colours[capArray[row][col]];
				context.fillRect(left + col, top + row, 1, 1);
			}
		}
	}
}

/**
 * @summary Draws a segment of the plane's smoke trail on the canvas.
 *
 * @description
 * This function renders a colored line segment representing the smoke trail.
 * The line's direction, length, and position are determined by the current plane's pixel coordinates
 * and the coordinates and bearing of the specified `finishGateNo`.
 * It calculates the appropriate starting point and dimensions to draw an 11-pixel wide line
 * (red with a black outline) extending from the plane's current mapped position towards
 * or up to the vicinity of the target gate.
 *
 * @param {number} finishGateNo - The index of the gate in gates array
 * that defines the target bearing and relative coordinates for the line's endpoint.
 */
function drawLine(finishGateNo) {

	const segmentBearing = gates[finishGateNo - 1].bearing;
	const startTopPixels = gates[finishGateNo - 2].pixels.top;
	const finishTopPixels = gates[finishGateNo - 1].pixels.top;
	const startLeftPixels = gates[finishGateNo - 2].pixels.left;
	const finishLeftPixels = gates[finishGateNo - 1].pixels.left;

	if (segmentBearing == 0) {
		if (planeTopPixels >= startTopPixels - 5)
			planeTopPixels = startTopPixels - 6;
		if (planeTopPixels <= finishTopPixels + 5)
			planeTopPixels = finishTopPixels + 6;
		var lineHeight = (startTopPixels - 6) - planeTopPixels;
		var lineStartTop = planeTopPixels + charPaddingTop;
		var lineStartLeftMiddle = startLeftPixels + charPaddingLeft;
		context.fillStyle = colours[2];
		context.fillRect(lineStartLeftMiddle - 5, lineStartTop, 1, lineHeight);
		context.fillRect(lineStartLeftMiddle + 5, lineStartTop, 1, lineHeight);
		context.fillStyle = colours[1];
		context.fillRect(lineStartLeftMiddle - 4, lineStartTop, 9, lineHeight);
	}
	else if (segmentBearing == 90) {
		if (planeLeftPixels <= startLeftPixels + 5)
			planeLeftPixels = startLeftPixels + 6;
		if (planeLeftPixels >= finishLeftPixels - 5)
			planeLeftPixels = finishLeftPixels - 6;
		var lineLength = planeLeftPixels - (startLeftPixels + 6);
		var lineStartLeft = startLeftPixels + 6 + charPaddingLeft;
		var lineStartTopMiddle = startTopPixels + charPaddingTop;
		context.fillStyle = colours[2];
		context.fillRect(lineStartLeft, lineStartTopMiddle - 5, lineLength, 1);
		context.fillRect(lineStartLeft, lineStartTopMiddle + 5, lineLength, 1);
		context.fillStyle = colours[1];
		context.fillRect(lineStartLeft, lineStartTopMiddle - 4, lineLength, 9);
	}
	else if (segmentBearing == 180) {
		if (planeTopPixels <= startTopPixels + 5)
			planeTopPixels = startTopPixels + 6;
		if (planeTopPixels >= finishTopPixels - 5)
			planeTopPixels = finishTopPixels - 6;
		var lineHeight = planeTopPixels - (startTopPixels + 6);
		var lineStartTop = startTopPixels + 6 + charPaddingTop;
		var lineStartLeftMiddle = startLeftPixels + charPaddingLeft;
		context.fillStyle = colours[2];
		context.fillRect(lineStartLeftMiddle - 5, lineStartTop, 1, lineHeight);
		context.fillRect(lineStartLeftMiddle + 5, lineStartTop, 1, lineHeight);
		context.fillStyle = colours[1];
		context.fillRect(lineStartLeftMiddle - 4, lineStartTop, 9, lineHeight);
	}
	else if (segmentBearing == 270) {
		if (planeLeftPixels >= startLeftPixels - 5)
			planeLeftPixels = startLeftPixels - 6;
		if (planeLeftPixels <= finishLeftPixels + 5)
			planeLeftPixels = finishLeftPixels + 6;
		var lineLength = (startLeftPixels - 6) - planeLeftPixels;
		var lineStartLeft = planeLeftPixels + charPaddingLeft;
		var lineStartTopMiddle = startTopPixels + charPaddingTop;
		context.fillStyle = colours[2];
		context.fillRect(lineStartLeft, lineStartTopMiddle - 5, lineLength, 1);
		context.fillRect(lineStartLeft, lineStartTopMiddle + 5, lineLength, 1);
		context.fillStyle = colours[1];
		context.fillRect(lineStartLeft, lineStartTopMiddle - 4, lineLength, 9);
	}
}

/**
 * @summary Adjusts the plane's drawing pixel coordinates to precisely match the end point of a line segment at a given gate.
 *
 * @description
 * This function is typically called when the smoke trail is being turned off.
 * It ensures that the `planeTopPixels` or `planeLeftPixels` are set to a fixed position
 * relative to the `currentGateNo` based on its bearing. This "snaps" the drawing
 * position of the plane to the exact location where the smoke line segment ends,
 * ensuring visual continuity and a clean cutoff of the smoke trail at the gate.
 *
 * @param {number} currentGateNo - The index of the gate which defines the target end position for the plane's drawing coordinates.
 */
function setPlaneEndOfLine(currentGateNo) {

	const segmentBearing = gates[currentGateNo - 1].bearing;
	const finishTop = gates[currentGateNo - 1].pixels.top;
	const finishLeft = gates[currentGateNo - 1].pixels.left;

	if (segmentBearing == 0) {
		planeTopPixels = finishTop + 6
	}
	else if (segmentBearing == 90) {
		planeLeftPixels = finishLeft - 6
	}
	else if (segmentBearing == 180) {
		planeTopPixels = finishTop - 6
	}
	else if (segmentBearing == 270) {
		planeLeftPixels = finishLeft + 6
	}
}

/**
 * @summary Appends a given message (or multiple arguments) as a new line to the jsConsole textarea.
 * @description Converts all arguments to strings to ensure they display correctly.
 * @param {...any} args - The arguments to log to the console.
 */
function logToConsole(...args) {
	const consoleElement = document.getElementById('jsConsole');
	if (consoleElement instanceof HTMLTextAreaElement) {
		const timestamp = new Date().toLocaleTimeString();
		// Convert all arguments to strings and join them with a space
		const message = args.map(arg => {
			if (typeof arg === 'object' && arg !== null) {
				// For objects/arrays, use JSON.stringify for better readability
				return JSON.stringify(arg);
			}
			return String(arg); // Convert primitives, null, undefined
		}).join(' ');

		consoleElement.value += (consoleElement.value ? '\n' : '') + `[${timestamp}] ${message}`;
		// Scroll to the bottom to show the latest message
		consoleElement.scrollTop = consoleElement.scrollHeight;
	} else {
		// Fallback if jsConsole element isn't found or isn't a textarea
		// In P3D, this might not show anywhere, but it's good practice.
		// If you see no output in your jsConsole, this error might be why.
		console.error('jsConsole textarea not found or is not an HTMLTextAreaElement!');
	}
}

/**
 * @summary Calculates the clamped 3D distance of the plane's projection onto the gate segment from the start gate.
 *
 * @description
 * This method determines the plane's forward progress along the straight line connecting the two gates
 * by calculating the scalar projection of the vector (Gate Start to Plane) onto the vector (Gate Start to Gate Finish).
 * The resulting distance is clamped between 0 and the total segment length.
 * Note: Altitude values are expected to be in METERS.
 *
 * @param {LatLon} pStart - LatLon object for the start gate (Gate 1). Altitude must be in meters.
 * @param {LatLon} pFinish - LatLon object for the finish gate (Gate 2). Altitude must be in meters.
 * @param {LatLon} pPlane - LatLon object for the plane's position. Altitude must be in meters.
 * @returns {number} The distance in meters from pStart to the projection point on the segment, clamped to [0, segment length].
 */
function getProjectedProgressDistance(pStart, pFinish, pPlane) {

	// 1. Convert to ECEF Cartesian Coordinates (X, Y, Z in meters)
	const cartesianStart = pStart.toCartesian();
	const cartesianFinish = pFinish.toCartesian();
	const cartesianPlane = pPlane.toCartesian();

	// 2. Define the Segment Vector (V_seg = P_finish - P_start)
	const V_seg_x = cartesianFinish.x - cartesianStart.x;
	const V_seg_y = cartesianFinish.y - cartesianStart.y;
	const V_seg_z = cartesianFinish.z - cartesianStart.z;

	// 3. Define the Vector from Start Gate to Plane (V_plane = P_plane - P_start)
	const V_plane_x = cartesianPlane.x - cartesianStart.x;
	const V_plane_y = cartesianPlane.y - cartesianStart.y;
	const V_plane_z = cartesianPlane.z - cartesianStart.z;

	// 4. Calculate Dot Product (V_plane . V_seg) and Squared Magnitude (||V_seg||^2)
	// The dot product is the numerator for the projection.
	const dotProduct = V_plane_x * V_seg_x + V_plane_y * V_seg_y + V_plane_z * V_seg_z;

	// The squared magnitude is the denominator for the ratio (t).
	const segMagnitudeSq = V_seg_x * V_seg_x + V_seg_y * V_seg_y + V_seg_z * V_seg_z;

	// 5. Calculate the total segment length (Magnitude of V_seg)
	const segmentLength = Math.sqrt(segMagnitudeSq);

	// Check for a zero-length segment to prevent division by zero
	if (segMagnitudeSq === 0) {
		return 0;
	}

	// 6. Calculate the progress ratio (t)
	// t = (V_plane . V_seg) / ||V_seg||^2
	let progressRatio = dotProduct / segMagnitudeSq;

	// 7. Calculate the raw projected distance
	const rawProjectedDistance = progressRatio * segmentLength;

	// 8. Clamp the distance to ensure it lies within the segment [0, segmentLength]
	const clampedDistance = Math.max(0, Math.min(segmentLength, rawProjectedDistance));

	return clampedDistance;
}

/**
 * @summary Calculates and updates the plane's pixel coordinates on the canvas.
 *
 * @description
 * This function determines the plane's current drawing position on the canvas based on its
 * real-time geographic coordinates (latitude, longitude, altitude) relative to the
 * start and end gates of the current segment. It first fetches the plane's current
 * 3D position from the simulator.
 *
 * It calculates its progress along the segment as a ratio
 * of the distance from the start gate to the total segment length. This ratio is then
 * used to interpolate the plane's pixel position along the appropriate axis (vertical
 * for 0/180 degree bearings, horizontal for 90/270 degree bearings) on the canvas,
 * updating the global `planeTopPixels` and `planeLeftPixels` variables.
 *
 * @param {number} currentGateNo - The index of the start gate in the sequence.
 * @returns {number} The distance the plane has progressed along the current segment, in pixels.
 */
function getPlanePixelCoords(currentGateNo) {
	// Fetch real-time simulation variables for plane's geographic position
	var planeLonDeg = VarGet("A:PLANE LONGITUDE", "Radians") * 180 / Math.PI; // Convert longitude from radians to degrees
	var planeLatDeg = VarGet("A:PLANE LATITUDE", "Radians") * 180 / Math.PI; // Convert latitude from radians to degrees
	var planeAltMetres = VarGet("A:PLANE ALTITUDE", "Feet") * metresInFoot; // Convert altitude from feet to metres

	const pStart = new LatLon(gates[currentGateNo - 1].coordinates.latitude, gates[currentGateNo - 1].coordinates.longitude, gates[currentGateNo - 1].altitude);
	const pFinish = new LatLon(gates[currentGateNo].coordinates.latitude, gates[currentGateNo].coordinates.longitude, gates[currentGateNo].altitude);
	const pPlane = new LatLon(planeLatDeg, planeLonDeg, planeAltMetres);
	const distanceFromLastGate = getProjectedProgressDistance(pStart, pFinish, pPlane)

	// Calculate the plane's progress along the current segment as a ratio (0 to 1).
	var distanceProgressedRatio = distanceFromLastGate / currentSegmentDistance;
	// Convert this ratio into pixels to determine the plane's position on the canvas segment.
	var distanceProgressedPixels = Math.round(distanceProgressedRatio * segmentLengthPixels);

	const segmentBearing = gates[currentGateNo - 1].bearing;
	const startTop = gates[currentGateNo - 1].pixels.top;
	const startLeft = gates[currentGateNo - 1].pixels.left;

	// Update plane's pixel coordinates based on the gate bearing (segment orientation).
	if (segmentBearing == 0 || segmentBearing == 180) {
		// Vertical segment (North or South)
		planeLeftPixels = startLeft; // X-coordinate remains fixed relative to the gate's X.

		if (segmentBearing == 180) {
			// South: Moving DOWN. Y increases.
			planeTopPixels = startTop + distanceProgressedPixels;
		} else { // segmentBearing == 0
			// North: Moving UP. Y decreases.
			planeTopPixels = startTop - distanceProgressedPixels;
		}
	} else {
		// Horizontal segment (East or West)
		planeTopPixels = startTop; // Y-coordinate remains fixed relative to the gate's Y.

		if (segmentBearing == 90) {
			// East: Moving RIGHT. X increases.
			planeLeftPixels = startLeft + distanceProgressedPixels;
		} else { // segmentBearing == 270
			// West: Moving LEFT. X decreases.
			planeLeftPixels = startLeft - distanceProgressedPixels;
		}
	}

	// Return the calculated pixel progress
	return distanceProgressedPixels;
}

/**
 * @summary Main animation loop responsible for continuously updating the canvas rendering.
 *
 * @description
 * This function is called repeatedly by `window.requestAnimationFrame` to manage
 * the visual state of the simulation. Its responsibilities include:
 * 
 * <p>1. Retrieving real-time plane data (longitude, latitude) and simulator states (current gate, smoke status).</p>
 * <p>2. Calculating the plane's pixel coordinates on the canvas based on its geographic position
 * and the defined map boundaries.</p>
 * <p>3. Detecting changes in the `smokeOn` state to trigger specific drawing logic (e.g., drawing gate caps).</p>
 * <p>4. Drawing the smoke trail line segment dynamically based on the plane's position
 * and the current/next gate's parameters.</p>
 * <p>5. Managing the drawing of entry and exit caps at gates when smoke is toggled.</p>
 * <p>6. Scheduling itself for the next animation frame.</p>
 */
function update() {
	// Fetch real-time simulation variables
	var currentGateNo = VarGet("S:currentGateNo", "NUMBER");
	var smokeOn = VarGet("S:smokeOn", "NUMBER");

	if (currentGateNo <= 0) {
		// The scheduleUpdate() function handles the next execution, so we just exit here.
		return;
	}

	// Smoke toggle detection and logging
	if (smokeOn != smokeOld) {
		smokeHasToggled = true;
		if (smokeOn == 1) {
			logToConsole(`Smoke ON: Gate ${currentGateNo} triggered.`);
		} else {
			logToConsole(`Smoke OFF: Gate ${currentGateNo} triggered.`);
		}
	}

	smokeOld = smokeOn; // Update previous smoke state

	// Map plane's geographic coordinates to canvas pixel coordinates ONLY when smoke is ON,
	// as this position is only needed to draw the smoke trail endpoint.
	if (smokeOn == 1) {
		const distanceProgressedPixels = getPlanePixelCoords(currentGateNo);
	}

	const segmentBearing = gates[currentGateNo - 1].bearing;
	const startTopPixels = gates[currentGateNo - 1].pixels.top;
	const startLeftPixels = gates[currentGateNo - 1].pixels.left;

	// Drawing logic when smoke is turned ON or is ON 
	if (smokeHasToggled && smokeOn == 1) {
		// Rotate cap for entry direction (180 degrees from gate bearing)
		rotateSquareArray(endCap, curCap, (segmentBearing + 180) % 360);
		if (segmentBearing == 0)
			drawCap(startTopPixels - 11 + charPaddingTop, startLeftPixels - 5 + charPaddingLeft, curCap);
		else if (segmentBearing == 90)
			drawCap(startTopPixels - 5 + charPaddingTop, startLeftPixels + charPaddingLeft, curCap);
		else if (segmentBearing == 180)
			drawCap(startTopPixels + charPaddingTop, startLeftPixels - 5 + charPaddingLeft, curCap);
		else if (segmentBearing == 270)
			drawCap(startTopPixels - 5 + charPaddingTop, startLeftPixels - 11 + charPaddingLeft, curCap);
		smokeHasToggled = false; // Reset toggle flag
	}
	if (smokeOn == 1) {
		drawLine(currentGateNo + 1); // Draw smoke line segment towards the next gate
	}

	// Drawing logic when smoke is turned OFF
	if (smokeHasToggled && smokeOn == 0) {
		// Adjust plane position to end precisely at the gate
		setPlaneEndOfLine(currentGateNo);
		// Draw the final smoke line segment up to the gate
		drawLine(currentGateNo);
		// Rotate cap for exit direction (same as gate bearing)
		rotateSquareArray(endCap, curCap, segmentBearing);
		if (segmentBearing == 0)
			drawCap(startTopPixels + charPaddingTop, startLeftPixels - 5 + charPaddingLeft, curCap);
		else if (segmentBearing == 90)
			drawCap(startTopPixels - 5 + charPaddingTop, startLeftPixels - 11 + charPaddingLeft, curCap);
		else if (segmentBearing == 180)
			drawCap(startTopPixels - 11 + charPaddingTop, startLeftPixels - 5 + charPaddingLeft, curCap);
		else if (segmentBearing == 270)
			drawCap(startTopPixels - 5 + charPaddingTop, startLeftPixels + charPaddingLeft, curCap);
		smokeHasToggled = false; // Reset toggle flag
	}
}