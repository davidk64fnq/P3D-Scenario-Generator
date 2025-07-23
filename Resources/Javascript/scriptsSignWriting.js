// --- Title comment block ---
/**
 * @file scriptsSignWriting.js
 * @description Handles the drawing logic for the plane's smoke trail and gate caps on the canvas.
 * Integrates with external simulator variables to update plane position and smoke state.
 * @author [David Kilpatrick]
 * @version 1.0.0
 * @date 2025-07-19
*/


// --- JSDoc Global Variable Declarations (Injected by C#) ---
// These variables are declared globally. Their string values are expected to be
// injected/replaced by the C# application before script execution.
// The JSDoc provides type information for the TypeScript checker.

/**
 * @global
 * @type {string}
 * @description Placeholder string for comma-separated gate top pixel coordinates.
 * Origin is 0,0 top left corner of canvas. References middle pixel in base of segment cap.
 * */
var gateTopPixelsX_PLACEHOLDER; // Will be injected e.g., "10,20,30"

/**
 * @global
 * @type {string}
 * @description Placeholder string for comma-separated list of gate left pixel coordinates.
 * Origin is 0,0 top left corner of canvas. References middle pixel in base of segment cap.
 */
var gateLeftPixelsX_PLACEHOLDER;

/**
 * @global
 * @type {string}
 * @description Placeholder string for comma-separated list of gate bearing values.
 * Each value is an integer representing the bearing in degrees (0, 90, 180, 270),
 * the direction plane heading when it flys through the gate.
 */
var gateBearingsX_PLACEHOLDER;

/**
 * @global
 * @type {string}
 * @description Placeholder strings for various injected configuration values:
 * - mapNorthX: Northernmost latitude of the canvas.
 * - mapEastX: Easternmost longitude of the canvas.
 * - mapSouthX: Southernmost latitude of the canvas.
 * - mapWestX: Westernmost longitude of the canvas.
 * - magVarX: Magnetic variation in degrees.
 * - canvasWidthX: Message canvas width in pixels.
 * - canvasHeightX: Message canvas height in pixels.
 * - consoleWidthX: Message console width in pixels.
 * - consoleHeightX: Message console height in pixels.
 * - windowHorizontalPaddingX: Horizontal window padding around and between canvas and console in pixels.
 * - windowVerticalPaddingX: Vertical window padding around and between canvas and console in pixels.
 */
var mapNorthX_PLACEHOLDER, mapEastX_PLACEHOLDER, mapSouthX_PLACEHOLDER, mapWestX_PLACEHOLDER, magVarX_PLACEHOLDER,
	canvasWidthX_PLACEHOLDER, canvasHeightX_PLACEHOLDER, consoleWidthX_PLACEHOLDER, consoleHeightX_PLACEHOLDER,
	windowHorizontalPaddingX_PLACEHOLDER, windowVerticalPaddingX_PLACEHOLDER;

// --- Script-Managed Global Variables ---
// These variables are declared here and will be populated/assigned
// by the JavaScript code itself, typically after DOM content is loaded.

/** @type {HTMLCanvasElement | null} */
let canvas = null;
/** @type {CanvasRenderingContext2D | null} */
let context = null;
/** @type {number[]} Origin is 0,0 top left corner of canvas. References middle pixel in base of segment cap.*/
let gateTopPixels; 
/** @type {number[]} Origin is 0,0 top left corner of canvas. References middle pixel in base of segment cap.*/
let gateLeftPixels;
/** @type {number[]} Each value is an integer representing the bearing in degrees (0, 90, 180, 270), the direction plane heading when it flys through the gate.*/
let gateBearings;
/** @type {number} Northernmost latitude of the message canvas.*/
let parsedMapNorth; 
/** @type {number} Easternmost longitude of the canvas.*/
let parsedMapEast; 
/** @type {number} Southernmost latitude of the canvas.*/
let parsedMapSouth; 
/** @type {number} Westernmost longitude of the canvas.*/
let parsedMapWest; 
/** @type {number} Magnetic variation in degrees.*/
let parsedMagVar; 
/** @type {number} Message canvas width in pixels.*/
let initialCanvasWidthParsed;
/** @type {number} Message canvas height in pixels.*/
let initialCanvasHeightParsed;
/** @type {number} Message console width in pixels.*/
let initialConsoleWidthParsed;
/** @type {number} Message console height in pixels.*/
let initialConsoleHeightParsed;
/** @type {number} Horizontal window padding around and between canvas and console in pixels.*/
let initialWindowHorizontalPaddingParsed;
/** @type {number} Vertical window padding around and between canvas and console in pixels.*/
let initialWindowVerticalPaddingParsed;


document.addEventListener('DOMContentLoaded', () => {
	// Assign canvas and context ONLY after the DOM is fully loaded
	const foundCanvas = document.getElementById('canvas');

	// Parse the injected X variables here, where they are guaranteed to exist
	gateTopPixels = String(gateTopPixelsX).split(',').map(Number);
	gateLeftPixels = String(gateLeftPixelsX).split(',').map(Number);
	gateBearings = String(gateBearingsX).split(',').map(Number);

	parsedMapNorth = parseFloat(String(mapNorthX));
	parsedMapEast = parseFloat(String(mapEastX));
	parsedMapSouth = parseFloat(String(mapSouthX));
	parsedMapWest = parseFloat(String(mapWestX));
	parsedMagVar = parseFloat(String(magVarX));

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


/**
 * Smoke variables refer to use of scenario variable "smokeOn" which is toggled on when first
 *	gate of a segment is triggered and off when second gate of a segment is triggered. 
 */

/**
 * @global
 * @type {number} smokeOld - Tracks the previous value of the smokeOn variable to detect when its state changes.
 */
var smokeOld = 0;

/**
 * @global
 * @type {boolean} smokeHasToggled - Acts as a flag to indicate if smokeOn has just toggled, controlling when certain drawing logic should execute.
 */
var smokeHasToggled = false;

var planeTopPixels;
var planeLeftPixels;
var startTopPixels;
var startLeftPixels;

/**
 * @constant
 * @type {number} paddingLeft - to the middle of vertical segments which are 11 pixels wide, so effective padding between
 * segment and canvas left edge is 15 - 5 = 10 pixels.
 */
const paddingLeft = 15;

/**
 * @constant
 * @type {number} paddingTop - to the middle of horizontal segments which are 11 pixels wide, so effective padding between
 * segment and canvas top edge is 15 - 5 = 10 pixels.
 */
const paddingTop = 15; 

/**
 * @constant
 * @type {number} capExtra - The number of pixels either side of central row/col of segment line.
 */
const capExtra = 5; 
	
const colours = ["", "red", "black"];
	
const endCap = [];
endCap[0] = [0,0,0,0,0,2,0,0,0,0,0];
endCap[1] = [0,0,0,0,2,2,2,0,0,0,0];
endCap[2] = [0,0,0,2,2,1,2,2,0,0,0];
endCap[3] = [0,0,2,2,1,1,1,2,2,0,0];
endCap[4] = [0,2,2,1,1,1,1,1,2,2,0];
endCap[5] = [2,2,1,1,1,1,1,1,1,2,2];
endCap[6] = [0,0,0,0,0,0,0,0,0,0,0];
endCap[7] = [0,0,0,0,0,0,0,0,0,0,0];
endCap[8] = [0,0,0,0,0,0,0,0,0,0,0];
endCap[9] = [0,0,0,0,0,0,0,0,0,0,0];
endCap[10] = [0,0,0,0,0,0,0,0,0,0,0];
	
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
* Test comment
* @param {any} inArray - test
* @param {any} outArray - test
* @param {any} rotation - test
*/
function rotateSquareArray(inArray, outArray, rotation)
{
	if (rotation == 0)
		for(let row = 0; row < inArray.length; row++)
			for(let col = 0; col < inArray.length; col++)
			{
				outArray[row][col] = inArray[row][col];
			}
	else if (rotation == 90)
		for(let row = 0; row < inArray.length; row++)
			for(let col = 0; col < inArray.length; col++)
			{
				outArray[col][inArray.length - 1 - row] = inArray[row][col];
			}
	else if (rotation == 180)
		for(let row = 0; row < inArray.length; row++)
			for(let col = 0; col < inArray.length; col++)
			{
				outArray[inArray.length - 1 - row][col] = inArray[row][col];
			}
	else if (rotation == 270)
		for(let row = 0; row < inArray.length; row++)
			for(let col = 0; col < inArray.length; col++)
			{
				outArray[inArray.length - 1 - col][row] = inArray[row][col];
			}
}
	
function drawCap(top, left, capArray)
{
	for(let row = 0; row < capArray.length; row++)
	{
		for(let col = 0; col < capArray.length; col++)
		{
			if(capArray[row][col] != 0)
			{
				context.fillStyle = colours[capArray[row][col]];
				context.fillRect(left + col, top + row, 1, 1);
			}
		}
	}
}
	
function drawLine(finishGateNo)
{
	if (gateBearings[finishGateNo] == 0){
		if (planeTopPixels >= gateTopPixels[finishGateNo - 1] - 5)
			planeTopPixels = gateTopPixels[finishGateNo - 1] - 6;
		if (planeTopPixels <= gateTopPixels[finishGateNo] + 5)
			planeTopPixels = gateTopPixels[finishGateNo] + 6;
		var lineHeight = (gateTopPixels[finishGateNo - 1] - 6) - planeTopPixels;
		/** @type {number} */
		var lineStartTop = planeTopPixels + paddingTop;
		var lineStartLeftMiddle = gateLeftPixels[finishGateNo] + paddingLeft;
		context.fillStyle = colours[2];
		context.fillRect(lineStartLeftMiddle - 5, lineStartTop, 1, lineHeight);
		context.fillRect(lineStartLeftMiddle + 5, lineStartTop, 1, lineHeight);
		context.fillStyle = colours[1];
		context.fillRect(lineStartLeftMiddle - 4, lineStartTop, 9, lineHeight);
	}
	else if (gateBearings[finishGateNo] == 90){
		if (planeLeftPixels <= gateLeftPixels[finishGateNo - 1] + 5)
			planeLeftPixels = gateLeftPixels[finishGateNo - 1] + 6;
		if (planeLeftPixels >= gateLeftPixels[finishGateNo] - 5)
			planeLeftPixels = gateLeftPixels[finishGateNo] - 6;
		var lineLength = planeLeftPixels - (gateLeftPixels[finishGateNo - 1] + 6);
		var lineStartLeft = gateLeftPixels[finishGateNo - 1] + 6 + paddingLeft;
		var lineStartTopMiddle = gateTopPixels[finishGateNo] + paddingTop;
		context.fillStyle = colours[2];
		context.fillRect(lineStartLeft, lineStartTopMiddle - 5, lineLength, 1);
		context.fillRect(lineStartLeft, lineStartTopMiddle + 5, lineLength, 1);
		context.fillStyle = colours[1];
		context.fillRect(lineStartLeft, lineStartTopMiddle - 4, lineLength, 9);
	}
	else if (gateBearings[finishGateNo] == 180){
		if (planeTopPixels <= gateTopPixels[finishGateNo - 1] + 5)
			planeTopPixels = gateTopPixels[finishGateNo - 1] + 6;
		if (planeTopPixels >= gateTopPixels[finishGateNo] - 5)
			planeTopPixels = gateTopPixels[finishGateNo] - 6;
		var lineHeight = planeTopPixels - (gateTopPixels[finishGateNo - 1] + 6);
		var lineStartTop = gateTopPixels[finishGateNo - 1] + 6 + paddingTop;
		var lineStartLeftMiddle = gateLeftPixels[finishGateNo] + paddingLeft;
		context.fillStyle = colours[2];
		context.fillRect(lineStartLeftMiddle - 5, lineStartTop, 1, lineHeight);
		context.fillRect(lineStartLeftMiddle + 5, lineStartTop, 1, lineHeight);
		context.fillStyle = colours[1];
		context.fillRect(lineStartLeftMiddle - 4, lineStartTop, 9, lineHeight);
	}
	else if (gateBearings[finishGateNo] == 270){
		if (planeLeftPixels >= gateLeftPixels[finishGateNo - 1] - 5)
			planeLeftPixels = gateLeftPixels[finishGateNo - 1] - 6;
		if (planeLeftPixels <= gateLeftPixels[finishGateNo] + 5)
			planeLeftPixels = gateLeftPixels[finishGateNo] + 6;
		var lineLength = (gateLeftPixels[finishGateNo - 1] - 6) - planeLeftPixels;
		/** @type {number} */
		var lineStartLeft = planeLeftPixels + paddingLeft;
		var lineStartTopMiddle = gateTopPixels[finishGateNo] + paddingTop;
		context.fillStyle = colours[2];
		context.fillRect(lineStartLeft, lineStartTopMiddle - 5, lineLength, 1);
		context.fillRect(lineStartLeft, lineStartTopMiddle + 5, lineLength, 1);
		context.fillStyle = colours[1];
		context.fillRect(lineStartLeft, lineStartTopMiddle - 4, lineLength, 9);
	}
}

function setPlaneEndOfLine(currentGateNo){
	if (gateBearings[currentGateNo] == 0){
		planeTopPixels = gateTopPixels[currentGateNo] + 6
	}
	else if (gateBearings[currentGateNo] == 90){
		planeLeftPixels = gateLeftPixels[currentGateNo] - 6
	}
	else if (gateBearings[currentGateNo] == 180){
		planeTopPixels = gateTopPixels[currentGateNo] - 6
	}
	else if (gateBearings[currentGateNo] == 270){
		planeLeftPixels = gateLeftPixels[currentGateNo] + 6
	}
}

/**
 * Appends a given message (or multiple arguments) as a new line to the jsConsole textarea.
 * Converts all arguments to strings to ensure they display correctly.
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


	
function update(timestamp)
{
	
	var currentGateNo = VarGet("S:currentGateNo" ,"NUMBER");
	var planeHeadingT = VarGet("A:PLANE HEADING DEGREES MAGNETIC" ,"Radians") * 180 / Math.PI + parsedMagVar;
	if (planeHeadingT > 360)
		planeHeadingT = planeHeadingT - 360;
	if (planeHeadingT < 0)
		planeHeadingT = planeHeadingT + 360;
	var planeLonDeg = VarGet("A:PLANE LONGITUDE" ,"Radians") * 180 / Math.PI; // x
	var planeLatDeg = VarGet("A:PLANE LATITUDE", "Radians") * 180 / Math.PI;  // y

	/**
	 * @type {number} messageHeight - The calculated height of the message area on the canvas.
	 */
	var messageHeight = parseInt(document.getElementsByTagName("canvas")[0].getAttribute("height")) - capExtra * 2 - paddingTop * 2;

	/**
	 * @type {number} messageWidth - The calculated width of the message area on the canvas.
	 */
	var messageWidth = parseInt(document.getElementsByTagName("canvas")[0].getAttribute("width")) - capExtra * 2 - paddingLeft * 2;

	planeTopPixels = Math.round((planeLatDeg - parsedMapNorth) / (parsedMapSouth - parsedMapNorth) * messageHeight);
	planeLeftPixels = Math.round((planeLonDeg - parsedMapWest) / (parsedMapEast - parsedMapWest) * messageWidth);
	var smokeOn = VarGet("S:smokeOn", "NUMBER");

	if (smokeOn != smokeOld) {
		smokeHasToggled = true;
		// Log the toggle event
		if (smokeOn == 1) {
			logToConsole(`Smoke ON: Gate ${currentGateNo} triggered.`);
		} else {
			logToConsole(`Smoke OFF: Gate ${currentGateNo} triggered.`);
		}
	}

	smokeOld = smokeOn;
	if (smokeHasToggled && smokeOn == 1) {
		startTopPixels = gateTopPixels[currentGateNo];
		startLeftPixels = gateLeftPixels[currentGateNo];
		rotateSquareArray(endCap, curCap, (gateBearings[currentGateNo] + 180) % 360);
		if (gateBearings[currentGateNo] == 0)
			drawCap(startTopPixels - 11 + paddingTop, startLeftPixels - 5 + paddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 90)
			drawCap(startTopPixels - 5 + paddingTop, startLeftPixels + paddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 180)
			drawCap(startTopPixels + paddingTop, startLeftPixels - 5 + paddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 270)
			drawCap(startTopPixels - 5 + paddingTop, startLeftPixels - 11 + paddingLeft, curCap);
		smokeHasToggled = false;
	}
	if (smokeOn == 1) {
		drawLine(currentGateNo + 1);
	}
	if (smokeHasToggled && smokeOn == 0) {
		setPlaneEndOfLine(currentGateNo);	// We may not have been updated yet on plane position when sim turns smoke off
		drawLine(currentGateNo);
		startTopPixels = gateTopPixels[currentGateNo];
		startLeftPixels = gateLeftPixels[currentGateNo];
		rotateSquareArray(endCap, curCap, gateBearings[currentGateNo]);
		if (gateBearings[currentGateNo] == 0)
			drawCap(startTopPixels + paddingTop, startLeftPixels - 5 + paddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 90)
			drawCap(startTopPixels - 5 + paddingTop, startLeftPixels - 11 + paddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 180)
			drawCap(startTopPixels - 11 + paddingTop, startLeftPixels - 5 + paddingLeft, curCap);
		else if (gateBearings[currentGateNo] == 270)
			drawCap(startTopPixels - 5 + paddingTop, startLeftPixels + paddingLeft, curCap);
		smokeHasToggled = false;
	}
	window.requestAnimationFrame(update);
}