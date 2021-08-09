// Constants populated by P3D Scenario Generator application
const constellation = [constellationX];
const id = [idX];
const starNumber = [starNumberX];
const starName = [starNameX];
const bayer = [bayerX];
const raH = [raHX];
const raM = [raMX];
const raS = [raSX];
const decD = [decDX];
const decM = [decMX];
const decS = [decSX];
const visMag = [visMagX];
const lines = [linesX];

// Constants
const daysToBeginMth = [0,0,31,59,90,120,151,181,212,243,273,304,334];
const windowW = 960;
const windowH = 540;
const defaultFOV = 45;

// Global variables referenced in the button onClick functions		
var fovH = defaultFOV; // Sextant window width of sky 
var fovV = fovH * windowH / windowW; // Sextant window height of sky
var sexAZ = 0; // Sextant window mid-point bearing
var sexALT = 0; // Sextant window base elevation
var sexHo = fovV / 2; // Sextant observed altitude line
var planeHeadDeg;
var labelStars = 0; // Whether to show star labels
var labelConstellations = 0; // Whether to show lines between stars in constellations

// Main function that loops refreshing star map
function update(timestamp)
{
	planeHeadDeg = toDegrees(VarGet("A:PLANE HEADING DEGREES TRUE" ,"Radians"));
	var planeLon = VarGet("A:PLANE LONGITUDE" ,"Radians"); // x
	var planeLat = VarGet("A:PLANE LATITUDE" ,"Radians");  // y 
	var canvas = document.getElementById('canvas');
	var context = canvas.getContext('2d');
	var linePtsList = new Array();
	
	// Clear star map from last update
	context.fillStyle = "black";
	context.fillRect(0, 0, windowW, windowH);
	
	// Refresh information lines
	setInfoLine(context);
	setHoLine(context);
	
	// Calculate and plot local position of stars
	for(let starIndex = 0; starIndex < raH.length; starIndex++)
	{
		// Using formulae from here http://www.stargazing.net/kepler/altaz.html
		// Note: working in radians rather than degrees until ALT/AZ as Javascript trig uses radians
		var RAhr = raH[starIndex] + raM[starIndex] / 60 + raS[starIndex] / 3600;
		var RA = toRadians(RAhr * 15);
		var DEC = toRadians(decD[starIndex] + decM[starIndex] / 60 + decS[starIndex] / 3600);
		var LST = getLocalSiderialTime(planeLon);
		var HA = getHourAngle(LST, RA);
		var ALT = getALT(DEC, planeLat, HA);
		var AZ = getAZ(DEC, ALT, planeLat, HA);
		context.fillStyle = "yellow";
		var AZbearingDelta = getBearingDif(AZ, sexAZ); // Comparing star AZ to sextant window mid-point bearing
		if ((ALT > sexALT) && (ALT < sexALT + fovV) && (AZbearingDelta <= fovH / 2))
		{
			var relativeAZ = getRelativeAZ(AZ, sexAZ, fovH); // Number of degrees from left edge of sextant window
			var left = Math.round(relativeAZ / fovH * windowW);
			var top = Math.round((sexALT + fovV - ALT) / fovV * windowH);
			setStarIcon(starIndex, context, left, top)
			setStarLabel(starIndex, context, left, top);
			updateLinePtsList(starIndex, linePtsList, left, top);
		}
	} 
	setConstellationLines(context, linePtsList);
	
	document.getElementById('debug1').innerHTML = "";
	document.getElementById('debug2').innerHTML = "";
	
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);
		
function setInfoLine(context)
{
	context.fillStyle = "red";
	context.fillText("FOVH: " + Math.floor(fovH) + " ° FOVV: " + Math.floor(fovV) + "° AZ: " + Math.floor(sexAZ) + "° ALT: " + Math.floor(sexALT) + "° Ho: " + sexHo.toFixed(2) + "°", 10, windowH - 10);
}

function setHoLine(context)
{
	context.fillStyle = "red";
	context.fillRect(0, Math.round((sexALT + fovV - sexHo) / fovV * windowH), windowW, 1);
}

function setConstellationLines(context, linePtsList)
{
	if (labelConstellations == 1)
	{
		context.strokeStyle = "red";
		for(let linePt = 0; linePt < lines.length; linePt += 2)
		{
			var startPt = lines[linePt];
			var startPtIndex = linePtsList.indexOf(startPt);
			if (startPtIndex > -1)
			{
				var finishPt = lines[linePt + 1];
				var finishPtIndex = linePtsList.indexOf(finishPt);
				if (startPtIndex > -1)
				{
					context.beginPath();
					context.moveTo(linePtsList[startPtIndex + 1], linePtsList[startPtIndex + 2]);
					context.lineTo(linePtsList[finishPtIndex + 1], linePtsList[finishPtIndex + 2]);
					context.stroke();
				}
			}
		}
	}
	
	// clear array ready for next iteration
	linePtsList.splice(0,linePtsList.length)
}

function setStarIcon(starIndex, context, left, top)
{
	context.fillStyle = "yellow";
	if (visMag[starIndex] < 1)
	{
		context.fillRect(left, top - 2, 1, 1);
		context.fillRect(left - 1, top - 1, 3, 1);
		context.fillRect(left - 2, top, 5, 1);
		context.fillRect(left - 1, top + 1, 3, 1);
		context.fillRect(left, top + 2, 1, 1);
	}
	else if (visMag[starIndex] < 2)
	{
		context.fillRect(left - 1, top - 1, 3, 3);
	}
	else if (visMag[starIndex] < 3)
	{
		context.fillRect(left, top - 1, 1, 1);
		context.fillRect(left - 1, top, 3, 1);
		context.fillRect(left, top + 1, 1, 1);
	}
	else{
		context.fillRect(left, top - 1, 1, 1);
	}
}

function setStarLabel(starIndex, context, left, top)
{
	if (labelStars == 1)
	{
		var starLabel;
		if (starNumber[starIndex] >= 1)
			starLabel = "[" + starNumber[starIndex] + "] " + starName[starIndex];
		else
			starLabel = bayer[starIndex];
		context.fillStyle = "red";
		context.fillText(starLabel, left + 5, top - 5);
	}
}

function updateLinePtsList(starIndex, linePtsList, left, top)
{
	linePtsList.push(id[starIndex]);
	linePtsList.push(left);
	linePtsList.push(top);
}

// Functions used in calculating local position of stars
		
function getJ2000Day(zuluTime)
{ 
	var zuluDay = VarGet("E:ZULU DAY OF WEEK" ,"Number");  
	var zuluMonth = VarGet("E:ZULU MONTH OF YEAR" ,"Number");  
	var zuluYear = VarGet("E:ZULU YEAR" ,"Number");

	var decTime = zuluTime / (60 * 60 * 24);
	var daysToBegMth = daysToBeginMth[zuluMonth];
	if ((zuluYear % 4 == 0) && (zuluMonth > 2))
		daysToBegMth = daysToBegMth + 1
	var daysToBegYear = -0.5 + (zuluYear - 2000) * 365 + Math.floor((zuluYear - 2001) / 4);
		
	return decTime + daysToBegMth + zuluDay + daysToBegYear;
}

function getLocalSiderialTime(planeLon)
{
	var zuluTime = VarGet("E:ZULU TIME" ,"Seconds");
	var j2000Days = getJ2000Day(zuluTime);
	var UT = zuluTime / (60 * 60);
	var planeLonDeg = toDegrees(planeLon);
	var LSTdeg = 100.46 + 0.985647 * j2000Days + planeLonDeg + 15 * UT;
	while (LSTdeg < 0)
		LSTdeg += 360;
	while (LSTdeg > 360)
		LSTdeg -= 360;
	
	return toRadians(LSTdeg);
}

function getHourAngle(LST, RA)
{
	var HA = LST - RA;
	while (HA < 0)
		HA += toRadians(360);
		
	return HA;
}

function getALT(DEC, LAT, HA)
{
	return toDegrees(Math.asin(Math.sin(DEC) * Math.sin(LAT) + Math.cos(DEC) * Math.cos(LAT) * Math.cos(HA)));
}

function getAZ(DEC, ALT, LAT, HA)
{
	var ALTrad = toRadians(ALT);
	var A = Math.acos((Math.sin(DEC) - Math.sin(ALTrad) * Math.sin(LAT)) / (Math.cos(ALTrad) * Math.cos(LAT)));
	if (Math.sin(HA) < 0)
		return toDegrees(A);
	else
		return 360 - toDegrees(A);
}

function toRadians(degrees)
{
	return degrees * Math.PI / 180;
}

function toDegrees(radians)
{
	return radians * 180 / Math.PI;
}

function getBearingDif(AZ, sexAZ)
{
	var absDelta = Math.abs(AZ - sexAZ);
	if (absDelta <= 180)
		return absDelta;
	else
		return 360 - absDelta;
}

function getRelativeAZ(AZ, sexAZ, fovH)
{
	var left = sexAZ - fovH / 2;
	if (left < 0)
		left += 360;
	if (AZ > left)
		return AZ - left;
	else 
		return 360 - left + AZ;
}

// Button onClick functions for handling info buttons
	
function toggleLabelStars()
{
	if (labelStars == 0)
		labelStars = 1;
	else
		labelStars = 0;
}	

function toggleLabelConstellations()
{
	if (labelConstellations == 0)
		labelConstellations = 1;
	else
		labelConstellations = 0;
}

// Button onClick functions for adjusting FOV, AV, ALT, Ho 
		
function moveFOVinc5()
{
	// Cap FOV horizontal at max 180°
	if (fovH <= 175){
		fovH += 5;
		fovV = fovH * windowH / windowW
	}
}

function moveFOVreset()
{
	fovH = defaultFOV;
	fovV = fovH * windowH / windowW
}

function moveFOVdec5()
{
	// Cap FOV horizontal at min 5°
	if (fovH >= 10){
		fovH -= 5;
		fovV = fovH * windowH / windowW
	}
}

function moveAZleft1()
{
	if (sexAZ >= 1)
		sexAZ -= 1;
	else
		sexAZ = sexAZ + 359;
}

function moveAZleft5()
{
	if (sexAZ >= 5)
		sexAZ -= 5;
	else
		sexAZ = sexAZ + 355;
}

function moveAZreset()
{
	sexAZ = planeHeadDeg;
}

function moveAZright5()
{
	if (sexAZ < 355)
		sexAZ += 5;
	else
		sexAZ = sexAZ - 355;
}

function moveAZright1()
{
	if (sexAZ < 359)
		sexAZ += 1;
	else
		sexAZ = sexAZ - 359;
}

function moveALTup1()
{
	if (sexALT <= 90 - fovV - 1)
		sexALT += 1;
}

function moveALTup5()
{
	if (sexALT <= 90 - fovV - 5)
		sexALT += 5;
}

function moveALTreset()
{
	sexALT = 0;
}

function moveALTdown5()
{
	if (sexALT >= 5)
		sexALT -= 5;
}

function moveALTdown1()
{
	if (sexALT >= 1)
		sexALT -= 1;
}

function moveHup01()
{
	if (sexHo <= sexALT + fovV - 0.1)
		sexHo += 0.1;
}

function moveHup1()
{
	if (sexHo <= sexALT + fovV - 1)
		sexHo += 1;
}

function moveHup5()
{
	if (sexHo <= sexALT + fovV - 5)
		sexHo += 5;
}

function moveHreset()
{
	sexHo = sexALT + fovV / 2;
}

function moveHdown5()
{
	if (sexHo >= sexALT + 5)
		sexHo -= 5;
}

function moveHdown1()
{
	if (sexHo >= sexALT + 1)
		sexHo -= 1;
}

function moveHdown01()
{
	if (sexHo >= sexALT + 0.1)
		sexHo -= 0.1;
}