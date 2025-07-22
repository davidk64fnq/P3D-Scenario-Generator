/**
 * @file scriptsSignWriting.js
 * @description Handles the drawing logic for the plane's smoke trail and gate caps on the canvas.
 * Integrates with external simulator variables to update plane position and smoke state.
 * @author [David Kilpatrick]
 * @version 1.0.0
 * @date 2025-07-19
 */


const initialCanvasWidth = "canvasWidthX";
const initialCanvasHeight = "canvasHeightX";
const initialConsoleWidth = "consoleWidthX";
const initialConsoleHeight = "consoleHeightX";
const initialWindowHorizontalPadding = "windowHorizontalPaddingX";
const initialWindowVerticalPadding = "windowVerticalPaddingX";

document.addEventListener('DOMContentLoaded', () => {
	const canvas = document.getElementById('canvas');
	if (canvas) {
		canvas.width = parseInt(initialCanvasWidth);
		canvas.height = parseInt(initialCanvasHeight);
	}
	// Set the CSS Custom Properties for both width and height
	document.documentElement.style.setProperty('--console-width', parseInt(initialConsoleWidth) + 'px');
	document.documentElement.style.setProperty('--console-height', parseInt(initialConsoleHeight) + 'px');
	document.documentElement.style.setProperty('--window-horizontal-padding', parseInt(initialWindowHorizontalPadding) + 'px');
	document.documentElement.style.setProperty('--window-vertical-padding', parseInt(initialWindowVerticalPadding) + 'px');
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

/**
 * 
 */
var canvas = document.getElementById('canvas');
var context = canvas.getContext('2d');

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
	
const gateTopPixels = [gateTopPixelsX];
const gateLeftPixels = [gateLeftPixelsX];
const gateBearings = [gateBearingsX];

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
 * Appends a given string as a new line to the jsConsole textarea.
 * @param {string} message The string message to log.
 */
function logToConsole(message) {
	const consoleElement = document.getElementById('jsConsole');
	if (consoleElement) {
		const timestamp = new Date().toLocaleTimeString();
		consoleElement.value += (consoleElement.value ? '\n' : '') + `[${timestamp}] ${message}`;
		// Scroll to the bottom to show the latest message
		consoleElement.scrollTop = consoleElement.scrollHeight;
	} else {
		console.error('jsConsole textarea not found!');
	}
}

	
function update(timestamp)
{
	const mapNorth = mapNorthX; 
	const mapEast = mapEastX;
	const mapSouth = mapSouthX;
	const mapWest = mapWestX;
	const messageLength = messageLengthX;
	const magVar = magVarX;
	
	var currentGateNo = VarGet("S:currentGateNo" ,"NUMBER");
	var planeHeadingT = VarGet("A:PLANE HEADING DEGREES MAGNETIC" ,"Radians") * 180 / Math.PI + magVar;
	if (planeHeadingT > 360)
		planeHeadingT = planeHeadingT - 360;
	if (planeHeadingT < 0)
		planeHeadingT = planeHeadingT + 360;
	var planeLonDeg = VarGet("A:PLANE LONGITUDE" ,"Radians") * 180 / Math.PI; // x
	var planeLatDeg = VarGet("A:PLANE LATITUDE" ,"Radians") * 180 / Math.PI;  // y
	var messageHeight = document.getElementsByTagName("canvas")[0].getAttribute("height") - capExtra * 2 - paddingTop * 2;
	var messageWidth = document.getElementsByTagName("canvas")[0].getAttribute("width") - capExtra * 2 - paddingLeft * 2;
	planeTopPixels = Math.round((planeLatDeg - mapNorth) / (mapSouth - mapNorth) * messageHeight);
	planeLeftPixels = Math.round((planeLonDeg - mapWest) / (mapEast - mapWest) * messageWidth);
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
window.requestAnimationFrame(update);