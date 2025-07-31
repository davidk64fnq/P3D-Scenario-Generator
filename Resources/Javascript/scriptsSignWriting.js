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
import { Cartesian } from './third-party/geodesy/latlon-ellipsoidal.js';

// --- JSDoc Global Variable Declarations (Injected by C#) ---
// These variables are declared globally. Their string values are expected to be
// injected/replaced by the C# application before script execution.
// The JSDoc provides type information for the TypeScript checker.

/** @type {string | null} gateTopPixelsX - String representation of top pixel coordinates for gates, injected by C#.*/
var gateTopPixelsX = null;

/** @type {string | null} gateLeftPixelsX - String representation of left pixel coordinates for gates, injected by C#.*/
var gateLeftPixelsX = null;

/** @type {string | null} gateBearingsX - String representation of gate bearings in degrees, injected by C#.*/
var gateBearingsX = null;

/** @type {string | null} gateLatitudesX - String representation of gate bearings in degrees, injected by C#.*/
var gateLatitudesX = null;

/** @type {string | null} gateLongitudesX - String representation of gate bearings in degrees, injected by C#.*/
var gateLongitudesX = null;

/** @type {string | null} gateAltitudesX - String representation of gate altitudes in degrees, injected by C#.*/
var gateAltitudesX = null;

/** @type {string | null} paddingLeft - String representation of vertical window padding in pixels, injected by C#*/
var charPaddingLeftX = null;

/** @type {string | null} paddingTop - String representation of vertical window padding in pixels, injected by C#*/
var charPaddingTopX = null;

/** @type {string | null} canvasWidthX - String representation of the message canvas width in pixels, injected by C#.*/
var canvasWidthX = null;

/** @type {string | null} canvasHeightX - String representation of the message canvas height in pixels, injected by C#.*/
var canvasHeightX = null;

/** @type {string | null} consoleWidthX - String representation of the message console width in pixels, injected by C#.*/
var consoleWidthX = null;

/** @type {string | null} consoleHeightX - String representation of the message console height in pixels, injected by C#.*/
var consoleHeightX = null;

/** @type {string | null} windowHorizontalPaddingX - String representation of horizontal window padding in pixels, injected by C#.*/
var windowHorizontalPaddingX = null;

/** @type {string | null} windowVerticalPaddingX - String representation of vertical window padding in pixels, injected by C#.*/
var windowVerticalPaddingX = null;


// --- Script-Managed Global Variables ---
// These variables are declared here and will be populated/assigned
// by the JavaScript code itself, typically after DOM content is loaded.

/** @type {HTMLCanvasElement | null} canvas - Reference to the main HTML canvas element for drawing.*/
let canvas = null;

/** @type {CanvasRenderingContext2D | null} context - The 2D rendering context of the canvas, used for drawing operations.*/
let context = null;

/** @type {number[]} gateTopPixels - Origin is 0,0 top left corner of canvas. References top coordinate of point pixel in segment cap.*/
let gateTopPixels;

/** @type {number[]} gateLeftPixels - Origin is 0,0 top left corner of canvas. References left coordinate of point pixel in segment cap.*/
let gateLeftPixels;

/** @type {number[]} gateBearings - Each value is an integer representing the bearing in degrees (0, 90, 180, 270), the direction plane heading when it flys through the gate.*/
let gateBearings;

/** @type {number[]} gateLatitudes - Each value is a double representing gate latitude.*/
let gateLatitudes;

/** @type {number[]} gateLongitudes - Each value is a double representing gate longitude.*/
let gateLongitudes;

/** @type {number[]} gateAltitudes - Each value is a double representing gate altitude.*/
let gateAltitudes;

/** @type {number} paddingLeft - Padding from left edge to the point of horizontal segments.*/
let parsedCharPaddingLeft;

/** @type {number} paddingTop - Padding from top to the point of vertical segments.*/
let parsedCharPaddingTop;

/** @type {number} initialCanvasWidthParsed - Message canvas width in pixels after parsing.*/
let initialCanvasWidthParsed;

/** @type {number} initialCanvasHeightParsed - Message canvas height in pixels after parsing.*/
let initialCanvasHeightParsed;

/** @type {number} initialConsoleWidthParsed - Message console width in pixels after parsing.*/
let initialConsoleWidthParsed;

/** @type {number} initialConsoleHeightParsed - Message console height in pixels after parsing.*/
let initialConsoleHeightParsed;

/** @type {number} initialWindowHorizontalPaddingParsed - Horizontal window padding around and between canvas and console in pixels after parsing.*/
let initialWindowHorizontalPaddingParsed;

/** @type {number} initialWindowVerticalPaddingParsed - Vertical window padding around and between canvas and console in pixels after parsing.*/
let initialWindowVerticalPaddingParsed;

/**
 * @summary Initializes the application upon DOM content loading.
 *
 * @description
 * This event listener waits for the entire HTML document to be parsed
 * (but not necessarily all sub-resources like images to be loaded).
 * It performs the following critical setup tasks:
 * 1. Retrieves and parses all configuration values injected by the C# application
 * (e.g., map coordinates, canvas/console dimensions, gate pixel data).
 * 2. Identifies and initializes the main drawing canvas and its 2D rendering context.
 * Includes error handling if the canvas element is not found or context cannot be obtained.
 * 3. Sets CSS custom properties (variables) on the document's root element (`:root`)
 * based on the parsed dimensions, which allows CSS to adapt to the dynamic layout.
 * 4. Initiates the main animation loop by calling `window.requestAnimationFrame(update)`.
 *
 * @listens DOMContentLoaded
 */
document.addEventListener('DOMContentLoaded', () => {
	// Assign canvas and context ONLY after the DOM is fully loaded
	const foundCanvas = document.getElementById('canvas');

	// Parse the injected X variables here, where they are guaranteed to exist
	gateTopPixels = String(gateTopPixelsX).split(',').map(Number);
	gateLeftPixels = String(gateLeftPixelsX).split(',').map(Number);
	gateBearings = String(gateBearingsX).split(',').map(Number);
	gateLatitudes = String(gateLatitudesX).split(',').map(Number);
	gateLongitudes = String(gateLongitudesX).split(',').map(Number);
	gateAltitudes = String(gateAltitudesX).split(',').map(Number);

    // Calculate segment distance based on the first two gates
	currentSegmentDistance = calculate3DDistance(gateLatitudes[1], gateLongitudes[1], gateAltitudes[1], gateLatitudes[2], gateLongitudes[2], gateAltitudes[2]);

	parsedCharPaddingLeft = parseInt(String(charPaddingLeftX));
	parsedCharPaddingTop = parseInt(String(charPaddingTopX));

	initialCanvasWidthParsed = parseInt(String(canvasWidthX));
	initialCanvasHeightParsed = parseInt(String(canvasHeightX));
	initialConsoleWidthParsed = parseInt(String(consoleWidthX));
	initialConsoleHeightParsed = parseInt(String(consoleHeightX));
	initialWindowHorizontalPaddingParsed = parseInt(String(windowHorizontalPaddingX));
	initialWindowVerticalPaddingParsed = parseInt(String(windowVerticalPaddingX));

	if (foundCanvas instanceof HTMLCanvasElement) {
		// Now TypeScript knows foundCanvas is an HTMLCanvasElement
		canvas = foundCanvas;
		canvas.width = initialCanvasWidthParsed;
		canvas.height = initialCanvasHeightParsed;
		context = canvas.getContext('2d');

		if (!context) {
			console.error('Failed to get 2D rendering context for canvas!');
		}
	} else {
		console.error('Canvas element with id "canvas" not found or is not an HTMLCanvasElement!');
	}

	document.documentElement.style.setProperty('--canvas-width', initialCanvasWidthParsed + 'px');
	document.documentElement.style.setProperty('--canvas-height', initialCanvasHeightParsed + 'px');
	document.documentElement.style.setProperty('--console-width', initialConsoleWidthParsed + 'px');
	document.documentElement.style.setProperty('--console-height', initialConsoleHeightParsed + 'px');
	document.documentElement.style.setProperty('--window-horizontal-padding', initialWindowHorizontalPaddingParsed + 'px');
	document.documentElement.style.setProperty('--window-vertical-padding', initialWindowVerticalPaddingParsed + 'px');
	window.requestAnimationFrame(update);
});

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

/** @type {number} startTopPixels - Origin is 0,0 top left corner of canvas. References top coordinate of point pixel in segment cap for current gate.*/
var startTopPixels;

/** @type {number} startLeftPixels - Origin is 0,0 top left corner of canvas. References left coordinate of point pixel in segment cap for current gate.*/
var startLeftPixels;

/** @type {number} capExtra - The number of pixels either side of central row/col of segment line.*/
const capExtra = 5;

/** @type {string[]} colours - Segments are red and outlined in black.*/
const colours = ["", "red", "black"];

/** @type {number} metresInFoot - The number of metres in one foot.*/
const metresInFoot = .3048;

/** @type {number} segmentLengthPixels - The length of a segment from point of start endcap to point of finsih endcap in pixels.*/
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
 * @param {number} finishGateNo - The index of the gate (from `gateBearings`, `gateTopPixels`, `gateLeftPixels` arrays)
 * that defines the target bearing and relative coordinates for the line's endpoint.
 */
function drawLine(finishGateNo) {
	if (gateBearings[finishGateNo] == 0) {
		if (planeTopPixels >= gateTopPixels[finishGateNo - 1] - 5)
			planeTopPixels = gateTopPixels[finishGateNo - 1] - 6;
		if (planeTopPixels <= gateTopPixels[finishGateNo] + 5)
			planeTopPixels = gateTopPixels[finishGateNo] + 6;
		var lineHeight = (gateTopPixels[finishGateNo - 1] - 6) - planeTopPixels;
		var lineStartTop = planeTopPixels + parsedCharPaddingTop;
		var lineStartLeftMiddle = gateLeftPixels[finishGateNo -1] + parsedCharPaddingLeft;
		context.fillStyle = colours[2];
		context.fillRect(lineStartLeftMiddle - 5, lineStartTop, 1, lineHeight);
		context.fillRect(lineStartLeftMiddle + 5, lineStartTop, 1, lineHeight);
		context.fillStyle = colours[1];
		context.fillRect(lineStartLeftMiddle - 4, lineStartTop, 9, lineHeight);
	}
	else if (gateBearings[finishGateNo] == 90) {
		if (planeLeftPixels <= gateLeftPixels[finishGateNo - 1] + 5)
			planeLeftPixels = gateLeftPixels[finishGateNo - 1] + 6;
		if (planeLeftPixels >= gateLeftPixels[finishGateNo] - 5)
			planeLeftPixels = gateLeftPixels[finishGateNo] - 6;
		var lineLength = planeLeftPixels - (gateLeftPixels[finishGateNo - 1] + 6);
		var lineStartLeft = gateLeftPixels[finishGateNo - 1] + 6 + parsedCharPaddingLeft;
		var lineStartTopMiddle = gateTopPixels[finishGateNo -1] + parsedCharPaddingTop;
		context.fillStyle = colours[2];
		context.fillRect(lineStartLeft, lineStartTopMiddle - 5, lineLength, 1);
		context.fillRect(lineStartLeft, lineStartTopMiddle + 5, lineLength, 1);
		context.fillStyle = colours[1];
		context.fillRect(lineStartLeft, lineStartTopMiddle - 4, lineLength, 9);
	}
	else if (gateBearings[finishGateNo] == 180) {
		if (planeTopPixels <= gateTopPixels[finishGateNo - 1] + 5)
			planeTopPixels = gateTopPixels[finishGateNo - 1] + 6;
		if (planeTopPixels >= gateTopPixels[finishGateNo] - 5)
			planeTopPixels = gateTopPixels[finishGateNo] - 6;
		var lineHeight = planeTopPixels - (gateTopPixels[finishGateNo - 1] + 6);
		var lineStartTop = gateTopPixels[finishGateNo - 1] + 6 + parsedCharPaddingTop;
		var lineStartLeftMiddle = gateLeftPixels[finishGateNo -1] + parsedCharPaddingLeft;
		context.fillStyle = colours[2];
		context.fillRect(lineStartLeftMiddle - 5, lineStartTop, 1, lineHeight);
		context.fillRect(lineStartLeftMiddle + 5, lineStartTop, 1, lineHeight);
		context.fillStyle = colours[1];
		context.fillRect(lineStartLeftMiddle - 4, lineStartTop, 9, lineHeight);
	}
	else if (gateBearings[finishGateNo] == 270) {
		if (planeLeftPixels >= gateLeftPixels[finishGateNo - 1] - 5)
			planeLeftPixels = gateLeftPixels[finishGateNo - 1] - 6;
		if (planeLeftPixels <= gateLeftPixels[finishGateNo] + 5)
			planeLeftPixels = gateLeftPixels[finishGateNo] + 6;
		var lineLength = (gateLeftPixels[finishGateNo - 1] - 6) - planeLeftPixels;
		var lineStartLeft = planeLeftPixels + parsedCharPaddingLeft;
		var lineStartTopMiddle = gateTopPixels[finishGateNo -1] + parsedCharPaddingTop;
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
	if (gateBearings[currentGateNo] == 0) {
		planeTopPixels = gateTopPixels[currentGateNo] + 6
	}
	else if (gateBearings[currentGateNo] == 90) {
		planeLeftPixels = gateLeftPixels[currentGateNo] - 6
	}
	else if (gateBearings[currentGateNo] == 180) {
		planeTopPixels = gateTopPixels[currentGateNo] - 6
	}
	else if (gateBearings[currentGateNo] == 270) {
		planeLeftPixels = gateLeftPixels[currentGateNo] + 6
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
 * @summary Calculates and updates the plane's pixel coordinates on the canvas.
 *
 * @description
 * This function determines the plane's current drawing position on the canvas based on its
 * real-time geographic coordinates (latitude, longitude, altitude) relative to the
 * start and end gates of the current segment. It first fetches the plane's current
 * 3D position from the simulator.
 *
 * It then checks if the plane is within a defined `currentSegmentDistance` from both
 * the previous gate and the current (next) gate. If the plane is outside this range
 * for either gate, it logs a warning and exits, indicating the plane is not correctly
 * positioned within the expected segment bounds.
 *
 * If the plane is within bounds, it calculates its progress along the segment as a ratio
 * of the distance from the last gate to the total segment length. This ratio is then
 * used to interpolate the plane's pixel position along the appropriate axis (vertical
 * for 0/180 degree bearings, horizontal for 90/270 degree bearings) on the canvas,
 * updating the global `planeTopPixels` and `planeLeftPixels` variables.
 *
 * @param {number} currentGateNo - The index of the *current* (or next) gate in the sequence,
 * which defines the end point of the active segment and helps identify the previous gate.
 * @returns {void} This function updates global variables (`planeTopPixels`, `planeLeftPixels`)
 * and does not return a value.
 */
function getPlanePixelCoords(currentGateNo) {
	// Fetch real-time simulation variables for plane's geographic position
	var planeLonDeg = VarGet("A:PLANE LONGITUDE", "Radians") * 180 / Math.PI; // Convert longitude from radians to degrees
	var planeLatDeg = VarGet("A:PLANE LATITUDE", "Radians") * 180 / Math.PI; // Convert latitude from radians to degrees
	var planeAltMetres = VarGet("A:PLANE ALTITUDE", "Feet") * metresInFoot; // Convert altitude from feet to metres

	// Check that plane is within segment distance from BOTH gates that mark the start and end of the current segment.
	// This helps ensure the plane is flying along the intended path.
	var distanceFromLastGate = calculate3DDistance(
		gateLatitudes[currentGateNo - 1], gateLongitudes[currentGateNo - 1], gateAltitudes[currentGateNo - 1],
		planeLatDeg, planeLonDeg, planeAltMetres
	);
	var distanceToNextGate = calculate3DDistance(
		planeLatDeg, planeLonDeg, planeAltMetres,
		gateLatitudes[currentGateNo], gateLongitudes[currentGateNo], gateAltitudes[currentGateNo]
	);

	// If the plane is outside the defined operational distance from either gate,
	// log a warning and exit to prevent drawing errors for out-of-segment positions.
	if (distanceFromLastGate > currentSegmentDistance || distanceToNextGate > currentSegmentDistance) {
		logToConsole(`Plane is outside segment distance from gates ${currentGateNo - 1} and ${currentGateNo}.`);
		return; // Exit if plane is not within the segment distance
	}

	// Calculate the plane's progress along the current segment as a ratio (0 to 1).
	var distanceProgressedRatio = distanceFromLastGate / currentSegmentDistance;
	// Convert this ratio into pixels to determine the plane's position on the canvas segment.
	var distanceProgressedPixels = Math.round(distanceProgressedRatio * segmentLengthPixels);

	// Update plane's pixel coordinates based on the gate bearing (segment orientation).
	// For vertical segments (bearings 0 or 180), movement is along the Y-axis (planeTopPixels).
	// For horizontal segments (bearings 90 or 270), movement is along the X-axis (planeLeftPixels).
	if (gateBearings[currentGateNo] == 0 || gateBearings[currentGateNo] == 180) {
		// Vertical segment: Adjust Y-coordinate. X-coordinate remains fixed relative to the gate's X.
		planeTopPixels = gateTopPixels[currentGateNo - 1] + distanceProgressedPixels;
		planeLeftPixels = gateLeftPixels[currentGateNo - 1]; // X-coordinate is not used for rendering vertical segment progression
	} else {
		// Horizontal segment: Adjust X-coordinate. Y-coordinate remains fixed relative to the gate's Y.
		planeTopPixels = gateTopPixels[currentGateNo - 1]; // Y-coordinate is not used for rendering horizontal segment progression
		planeLeftPixels = gateLeftPixels[currentGateNo - 1] + distanceProgressedPixels;
	}
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
 * 
 * @param {DOMHighResTimeStamp} timestamp - The current time provided by `requestAnimationFrame`,
 * typically used for frame-rate independent animations, though not directly used for time calculations in this implementation.
 */
function update(timestamp) {
	// Fetch real-time simulation variables
	var currentGateNo = VarGet("S:currentGateNo", "NUMBER");
	var smokeOn = VarGet("S:smokeOn", "NUMBER");

    // Map planes geographic coordinates to canvas pixel coordinates
	getPlanePixelCoords(currentGateNo);

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

	// Drawing logic when smoke is turned ON or is ON
	if (smokeHasToggled && smokeOn == 1) {
		startTopPixels = gateTopPixels[currentGateNo];
		startLeftPixels = gateLeftPixels[currentGateNo];
		// Rotate cap for entry direction (180 degrees from gate bearing)
		rotateSquareArray(endCap, curCap, (gateBearings[currentGateNo] + 180) % 360);
		if (gateBearings[currentGateNo] == 0)
			drawCap(startTopPixels - 11 + parsedCharPaddingTop, startLeftPixels - 5 + parsedCharPaddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 90)
			drawCap(startTopPixels - 5 + parsedCharPaddingTop, startLeftPixels + parsedCharPaddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 180)
			drawCap(startTopPixels + parsedCharPaddingTop, startLeftPixels - 5 + parsedCharPaddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 270)
			drawCap(startTopPixels - 5 + parsedCharPaddingTop, startLeftPixels - 11 + parsedCharPaddingLeft, curCap);
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
		startTopPixels = gateTopPixels[currentGateNo];
		startLeftPixels = gateLeftPixels[currentGateNo];
		// Rotate cap for exit direction (same as gate bearing)
		rotateSquareArray(endCap, curCap, gateBearings[currentGateNo]);
		if (gateBearings[currentGateNo] == 0)
			drawCap(startTopPixels + parsedCharPaddingTop, startLeftPixels - 5 + parsedCharPaddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 90)
			drawCap(startTopPixels - 5 + parsedCharPaddingTop, startLeftPixels - 11 + parsedCharPaddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 180)
			drawCap(startTopPixels - 11 + parsedCharPaddingTop, startLeftPixels - 5 + parsedCharPaddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 270)
			drawCap(startTopPixels - 5 + parsedCharPaddingTop, startLeftPixels + parsedCharPaddingLeft, curCap);
		smokeHasToggled = false; // Reset toggle flag
	}

	// Schedule the next animation frame
	window.requestAnimationFrame(update);
}