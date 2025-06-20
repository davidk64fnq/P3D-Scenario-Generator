// Constants populated by P3D Scenario Generator application

// These arrays have an entry for every star in almanac
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

const lines = [linesX];					// pairs of stars connected by a line when constellations displayed
const destLat = destLatX;				// latitude of dest airport (degrees)
const destLon = destLonX;
const ariesGHAd = ariesGHAdX;			// Three days of data starting the day BEFORE scenario selected start date to allow for UT going backwards to previous day
const ariesGHAm = ariesGHAmX;
const starsSHAd = [starsSHAdX];
const starsSHAm = [starsSHAmX];
const starsDECd = [starsDECdX];
const starsDECm = [starsDECmX];
const starNameList = [starNameListX];	// list of the 57 navigational stars in alphabetical order, spelling per almanac
const startDate = startDateX;			// The local date selected when scenario generated
const northEdge = northEdgeX;			// North edge of area covered by plotting image (degrees)
const eastEdge = eastEdgeX;
const southEdge = southEdgeX;
const westEdge = westEdgeX;

// Constants
const daysToBeginMth = [0,0,31,59,90,120,151,181,212,243,273,304,334];
const windowW = 960;
const windowH = 540;
const defaultFOV = 45;

// Global variables referenced in the button onClick functions		
var fovH = defaultFOV;					// Sextant window horizontal field of view of sky (degrees)
var fovV = fovH * windowH / windowW;	// Sextant window vertical field of view of sky (degrees)
var sexAZ = 0;							// Sextant window mid-point bearing - magnetic (degrees)
var sexALT = 0;							// Sextant window base elevation (degrees)
var sexHo = windowH / 2;				// Sextant observed altitude line (pixels)
var planeHeadDeg;						// Retrieved from P3D
var planeLon; 
var planeLat;
var magVar;
var labelStars = 0;						// Whether to show star labels
var labelConstellations = 0;			// Whether to show lines between stars in constellations

// Plotting global variables
var fixNumber = 1;						// Refers to cumulative number of sets of three sight reductions made close together temporally
var sightNumber = 1;					// Refers to cumulative number of sight reductions made
var assumedLat = [0, destLatX];			// First fix assumes destination as that's the only known point (degrees)
var assumedLon = [0, destLonX];			// Subsequent fixes use last obtained fix as assumed position
var Zn = [0];							// Calculated fix star azimuth
var LOPZncoordLat = [0];				// LOPZn coord is intercept distance from fix assumed position on Zn bearing,
var LOPZncoordLon = [0];				// if intercept positive or Zn - 180 if negative
var LOPZnpixelsTop = [0];
var LOPZnpixelsLeft = [0];
var LOPLOPcoordLat = [0];				// LOPLOP coord is intersection of right angle lines drawn from pairs of LOPZn coords
var LOPLOPcoordLon = [0];
var LOPLOPpixelsTop = [0];
var LOPLOPpixelsLeft = [0];
var fixCoordPixelsTop = [0];
var fixCoordPixelsLeft = [0];
var showFinalLeg = 0;
var plotPlane = 0;
var moveAZ = 0;
var moveALT = 0;
var alterFOV = 0;

// Main function that loops refreshing star map
function update(timestamp)
{
	planeHeadDeg = toDegrees(VarGet("A:PLANE HEADING DEGREES TRUE", "Radians"));
	magVar = VarGet("A:MAGVAR", "Degrees");
	planeLon = VarGet("A:PLANE LONGITUDE" ,"Radians"); // x
	planeLat = VarGet("A:PLANE LATITUDE" ,"Radians");  // y 
	var canvas = document.getElementById('canvas');
	var context = canvas.getContext('2d');
	var ptsList = new Array(); // Pixel positions for stars in current sextant FOV

	// Handle held buttons
	if (moveAZ != 0)
		sexAZ = (sexAZ + moveAZ + 360) % 360;
	if (moveALT != 0) {
		sexALT = sexALT + moveALT;
		if (sexALT < 0)
			sexALT = 0;
		if (sexALT > (90 - fovV / 2))
			sexALT = 90 - fovV / 2;
	}
	if (alterFOV != 0) {
		fovH = fovH + alterFOV;
		if (fovH < 0)
			fovH = 0;
		if (fovH > 180)
			fovH = 180;
		fovV = fovH * windowH / windowW
	}
	
	
	// Calculate local position of stars
	ptsList = calcLocalStarPositions(planeLat, planeLon);
	
	// Clear star map from last update
	context.fillStyle = "black";
	context.fillRect(0, 0, windowW, windowH);
	
	// Refresh information lines
	setInfoLine(context);
	setHoLine(context);

	// Plot local position of stars
	setConstellationLines(context, ptsList);
	setStarIcons(context, ptsList)
	setStarLabels(context, ptsList);
	ptsList.splice(0, ptsList.length) // clear array ready for next iteration

	// Update sight reduction tab
	updateSightReductionTab();

	// Update plotting tab
	updatePlotTab();
	
	//document.getElementById('debug1').innerHTML = "";
	//document.getElementById('debug2').innerHTML = "";
	
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);

function getHoInDeg() {
	return (sexHo / windowH) * fovV + sexALT;
}
		
function setInfoLine(context)
{
	context.fillStyle = "red";
	var sexHoAccuracy = fovV / windowH * 60;
	context.fillText("FOVH: " + Math.floor(fovH) + " ° FOVV: " + Math.floor(fovV) + "° AZ: " + Math.floor(sexAZ) + "° ALT: " + Math.floor(sexALT) + "° Ho: " + Math.floor(getHoInDeg()) + " (" + sexHoAccuracy.toFixed(1) + ")", 10, windowH - 10);
}

function setHoLine(context)
{
	context.fillStyle = "red";
	context.fillRect(0, windowH - sexHo, windowW, 1);
}

function setConstellationLines(context, ptsList)
{
	if (labelConstellations == 1)
	{
		context.strokeStyle = "purple";
		for(let linePt = 0; linePt < lines.length; linePt += 2)
		{
			let startPt = lines[linePt];
			let startPtIndex = ptsList.indexOf(startPt);
			if (startPtIndex > -1)
			{
				let finishPt = lines[linePt + 1];
				let finishPtIndex = ptsList.indexOf(finishPt);
				if (finishPtIndex > -1)
				{
					context.beginPath();
					context.moveTo(ptsList[startPtIndex + 2], ptsList[startPtIndex + 3]);
					context.lineTo(ptsList[finishPtIndex + 2], ptsList[finishPtIndex + 3]);
					context.stroke();
				}
			}
		}
	}
}

function setStarIcons(context, ptsList)
{
	context.fillStyle = "yellow";
	for(let starIndex = 0; starIndex < ptsList.length; starIndex += 7)
	{
		let visMag = ptsList[starIndex + 1];
		let left = ptsList[starIndex + 2];
		let top = ptsList[starIndex + 3];
		if (visMag < 1.5)
		{
			context.fillRect(left, top - 2, 1, 1);
			context.fillRect(left - 1, top - 1, 3, 1);
			context.fillRect(left - 2, top, 5, 1);
			context.fillRect(left - 1, top + 1, 3, 1);
			context.fillRect(left, top + 2, 1, 1);
		}
		else if (visMag < 2.3)
		{
			context.fillRect(left - 1, top - 1, 3, 3);
		}
		else if (visMag < 3)
		{
			context.fillRect(left, top - 1, 1, 1);
			context.fillRect(left - 1, top, 3, 1);
			context.fillRect(left, top + 1, 1, 1);
		}
		else{
			context.fillRect(left, top - 1, 1, 1);
		}
	}
}

function setStarLabels(context, ptsList)
{
	if (labelStars == 1)
	{
		context.fillStyle = "red";
		for(let starIndex = 0; starIndex < ptsList.length; starIndex += 7)
		{
			let left = ptsList[starIndex + 2];
			let top = ptsList[starIndex + 3];
			let starNumber = ptsList[starIndex + 4];
			let starName = ptsList[starIndex + 5];
			let bayer = ptsList[starIndex + 6];
			if (starNumber >= 1)
				starLabel = "[" + starNumber + "] " + starName;
			else
				starLabel = bayer;
			context.fillText(starLabel, left + 5, top - 5);
		}
	}
}

function updatePlotTab() {
	var plotCanvas = document.getElementById("plottingCanvas");
	var context = plotCanvas.getContext("2d");
	var image = new Image(960, 540)
	image.src = "plotImage.jpg"
	context.drawImage(image, 0, 0);

	// Plot LOPZn coordinates
	context.fillStyle = "red";
	for (let ptNo = 1; ptNo <= sightNumber - sightNumber % 3; ptNo++) {
	//	context.fillRect(LOPZnpixelsLeft[ptNo] - 1, LOPZnpixelsTop[ptNo] - 1, 3, 3);
	}
	// Plot LOPLOP coordinates
	context.fillStyle = "green";
	for (let ptNo = 1; ptNo <= sightNumber - sightNumber % 3; ptNo++) {
		context.fillRect(LOPLOPpixelsLeft[ptNo] - 1, LOPLOPpixelsTop[ptNo] - 1, 3, 3);
	}
	// Plot fix coordinates
	context.fillStyle = "purple";
	for (let fixIndex = 1; fixIndex <= fixNumber; fixIndex++) {
		context.fillRect(fixCoordPixelsLeft[fixIndex] - 1, fixCoordPixelsTop[fixIndex] - 1, 3, 3);
	}

	// Optionally plot leg info
	if (showFinalLeg == 1) {
		const feetInNauticalMile = 6076.12;
		const finalDistFeet = getDistance(assumedLat[assumedLat.length - 1], assumedLon[assumedLon.length - 1], destLat, destLon);
		var finalBearing = getBearing(toRadians(assumedLat[assumedLat.length - 1]), toRadians(assumedLon[assumedLon.length - 1]), toRadians(destLat), toRadians(destLon));
		finalBearing -= magVar;
		finalBearing = Math.floor(finalBearing + 360) % 360;
		if (finalBearing < 10) {
			finalBearing = "00" + finalBearing
		}
		else if (finalBearing < 100) {
			finalBearing = "0" + finalBearing
		}

		context.fillStyle = "red";
		context.fillText("Final Leg: " + finalBearing + " (" + Math.floor(finalDistFeet / feetInNauticalMile) + "nm)", 70, windowH - 10);

		context.strokeStyle = 'red';
		context.lineWidth = 1;
		for (let index = 1; index < fixCoordPixelsLeft.length - 1; index++) {
			context.beginPath();
			context.moveTo(fixCoordPixelsLeft[index], fixCoordPixelsTop[index]);
			context.lineTo(fixCoordPixelsLeft[index + 1], fixCoordPixelsTop[index + 1]);
			context.stroke();
		}
	}

	// Optionally show plane
	if (plotPlane == 1) {
		var coord = [toDegrees(planeLat), toDegrees(planeLon)];
		var coordPixels = convertCoordToPixels(coord);
		var plane = document.getElementById("plane");
		plane.style.transform = "rotate(" + planeHeadDeg + "deg)";
		plane.style.top = coordPixels[0] - 15 + 99 + "px"; // 99 is nav bar and buttons above context
		plane.style.left = coordPixels[1] - 15 + "px";
		context.fillStyle = "red";
		context.fillRect(coordPixels[1] - 1, coordPixels[0] - 1, 3, 3);
	}
}

function updatePtsList(starIndex, ptsList, left, top)
{
	ptsList.push(id[starIndex]);
	ptsList.push(visMag[starIndex]);
	ptsList.push(left);
	ptsList.push(top);
	ptsList.push(starNumber[starIndex]);
	ptsList.push(starName[starIndex]);
	ptsList.push(bayer[starIndex]);
}

// Handle tabs

function openPage(pageName) {
	var i, tabcontent;
	tabcontent = document.getElementsByClassName("tabcontent");
	for (i = 0; i < tabcontent.length; i++) {
		tabcontent[i].style.display = "none";
	}
	document.getElementById(pageName).style.display = "block";
}

// Functions used in calculating local position of stars

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
	var leftDeg = sexAZ - fovH / 2;
	if (leftDeg < 0)
		leftDeg += 360;
	if (AZ > leftDeg)
		return AZ - leftDeg;
	else 
		return 360 - leftDeg + AZ;
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

// Button onClick functions for handling fix buttons

function takeSighting() {
	// locate first empty Hs field
	var HsArray = document.getElementsByClassName("Hs");
	var found = false;
	var curIndex = -1;
	var selectStarNameArray = document.getElementsByClassName("starName");
	for (let index = 0; index < HsArray.length && found == false; index++) {
		if ((HsArray[index].innerHTML == "") && (selectStarNameArray[index].options[0].selected == false)) {
			found = true;
			curIndex = index;
		}
	}

	if (found) {
		// Display Assumed Position Latitude
		var APlatArray = document.getElementsByClassName("AP Lat");
		APlatArray[curIndex].value = formatLatDeg(assumedLat[fixNumber]);

		// Display Assumed Position Longitude
		var APlonArray = document.getElementsByClassName("AP Lon");
		APlonArray[curIndex].value = formatLonDeg(assumedLon[fixNumber]);

		// Display date
		var dayOfMonth = VarGet("E:ZULU DAY OF MONTH", "Number");
		var monthOfYear = VarGet("E:ZULU MONTH OF YEAR", "Number");
		var year = VarGet("E:ZULU YEAR", "Number");
		var DateArray = document.getElementsByClassName("Date");
		DateArray[curIndex].innerHTML = dayOfMonth + "/" + monthOfYear + "/" + year;

		// Display current Universal Time
		var time = VarGet("E:ZULU TIME", "Seconds");
		var UTArray = document.getElementsByClassName("UT");
		UTArray[curIndex].innerHTML = secondsToTime(time);

		// Display sextant reading
		var Hs = getHoInDeg();
		HsArray[curIndex].innerHTML = formatDMdecimal(Hs, 1);

		// Display Aries GHA for current hour
		const currentDate = monthOfYear + "/" + dayOfMonth + "/" + year;
		var dayIndexOffset = getElapsedDays(startDate, currentDate); // Will be -1 if UT current day is one day earlier than local scenario start day
		var hour = Math.floor(time / 3600);
		var GHAhourArray = document.getElementsByClassName("GHAhour");
		var ariesGHA = ariesGHAd[dayIndexOffset + 1][hour] + ariesGHAm[dayIndexOffset + 1][hour] / 60;
		if (dayIndexOffset >= -1 && dayIndexOffset <= 1) {
			GHAhourArray[curIndex].innerHTML = ariesGHAd[dayIndexOffset + 1][hour] + "° " + ariesGHAm[dayIndexOffset + 1][hour] + "'";
		}
		else {
			GHAhourArray[curIndex].innerHTML = "No data";
		}

		// Interpolate Aries GHA for minutes and seconds
		var GHAincArray = document.getElementsByClassName("GHAinc");
		var ariesGHAinc = getGHAincrement(dayIndexOffset + 1, hour, time);
		GHAincArray[curIndex].innerHTML = formatDMdecimal(ariesGHAinc, 1);

		// Display star SHA
		var SHAincArray = document.getElementsByClassName("SHAinc");
		var starNameIndex = starNameList.findIndex(x => x == selectStarNameArray[curIndex].value);
		var starSHA = starsSHAd[starNameIndex] + starsSHAm[starNameIndex] / 60;
		SHAincArray[curIndex].innerHTML = starsSHAd[starNameIndex] + "° " + starsSHAm[starNameIndex] + "'";

		// Display GHA total
		var GHAtotal = ariesGHA + ariesGHAinc + starSHA;
		while (GHAtotal > 360) {
			GHAtotal -= 360;
        }
		var GHAtotalArray = document.getElementsByClassName("GHAtotal");
		GHAtotalArray[curIndex].innerHTML = formatDMdecimal(GHAtotal, 1);

		// Display star Dec
		var DecArray = document.getElementsByClassName("Dec");
		if (starsDECd[starNameIndex] > 0) {
			var starDEC = starsDECd[starNameIndex] + starsDECm[starNameIndex] / 60;
		}
		else {
			var starDEC = starsDECd[starNameIndex] - starsDECm[starNameIndex] / 60;
		}
		DecArray[curIndex].innerHTML = formatLatDeg(starDEC);

		// Calc Hc and Zn indirectly using formulae from here http://www.stargazing.net/kepler/altaz.html
		var ptsList = calcLocalStarPositions(toRadians(assumedLat[fixNumber]), toRadians(assumedLon[fixNumber]));
		var starNameIndex = ptsList.findIndex(x => x == selectStarNameArray[curIndex].value);
		if (starNameIndex == -1) {
			VarSet("S:errorMsgVar", "NUMBER", 1);
			HsArray[curIndex].innerHTML = "";
			return;
		}
		var ptsListIndex = starNameIndex - 5; // star name is 6th item of 7 items stored for each star
		var HcZn = calcHcZn(ptsList, ptsListIndex);
		Zn.push(HcZn[1]);
		var HcArray = document.getElementsByClassName("Hc");
		HcArray[curIndex].innerHTML = formatDMdecimal(toDegrees(HcZn[0]), 1);
		var ZnArray = document.getElementsByClassName("Zn");
		ZnArray[curIndex].innerHTML = formatDMdecimal(toDegrees(HcZn[1]), 1);

		// Calculate Intercept 
		var intercept = (Hs - toDegrees(HcZn[0])) * 60;
		var interceptArray = document.getElementsByClassName("Intercept");
		interceptArray[curIndex].innerHTML = intercept.toFixed(1) + "nm";

		// Get coordinates of LOP and Zn line intersection
		var LOPZncoord = getLOPZncoord(assumedLat[fixNumber], assumedLon[fixNumber], toDegrees(HcZn[1]), intercept);
		LOPZncoordLat.push(LOPZncoord[0]);
		LOPZncoordLon.push(LOPZncoord[1]);
		var LOPZnpixels = convertCoordToPixels(LOPZncoord);
		LOPZnpixelsTop.push(LOPZnpixels[0]);
		LOPZnpixelsLeft.push(LOPZnpixels[1]);

		// Get fix point and store as next assumed point
		if (sightNumber % 3 == 0) {
			var LOPLOPcoords = getLOPLOPcoord(); // returns 3 coord pairs
			for (let indexNo = 1; indexNo <= 6; indexNo += 2) {
				LOPLOPcoordLat.push(LOPLOPcoords[indexNo]);
				LOPLOPcoordLon.push(LOPLOPcoords[indexNo + 1]);
				var LOPLOPcoord = [LOPLOPcoords[indexNo], LOPLOPcoords[indexNo + 1]];
				var LOPLOPpixels = convertCoordToPixels(LOPLOPcoord);
				LOPLOPpixelsTop.push(LOPLOPpixels[0]);
				LOPLOPpixelsLeft.push(LOPLOPpixels[1]);
			}
			var fixCoord = getFixCoord();
			assumedLat.push(fixCoord[0]);
			assumedLon.push(fixCoord[1]);
			var fixCoordPixels = convertCoordToPixels(fixCoord);
			fixCoordPixelsTop.push(fixCoordPixels[0]);
			fixCoordPixelsLeft.push(fixCoordPixels[1]);
			fixNumber += 1;
        }
		sightNumber += 1;
	}
}

function getGHAincrement(day, hour, time) {
	// Convert start hour GHA to decimal
	var startGHAdec = ariesGHAd[day][hour] + ariesGHAm[day][hour] / 60;

	// Get finish hour GHA and convert to decimal
	if (day < 0) {
		return "No data"
	}
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
	var finishGHAdec = ariesGHAd[day][hour] + ariesGHAm[day][hour] / 60;

	// Calc increment as decimal
	var hourProportion = (time % 3600) / 3600;
	return ((finishGHAdec - startGHAdec + 360) % 360) * hourProportion;
}

function getElapsedDays(startDate, currentdate) {

	// JavaScript program to illustrate 
	// calculation of no. of days between two date 

	// To set two dates to two variables
	var dateStart = new Date(startDate);
	var dateCurrent = new Date(currentdate);

	// To calculate the time difference of two dates
	var Difference_In_Time = dateCurrent.getTime() - dateStart.getTime();

	// To calculate the no. of days between two dates
	return Difference_In_Time / (1000 * 3600 * 24);
}

function formatDMdecimal(coordinate, numberPlaces){
	return Math.floor(coordinate) + "° " + ((coordinate - Math.floor(coordinate)) * 60).toFixed(numberPlaces) + "'"
}

function formatLatDeg(latitude) {
	if (latitude >= 0) {
		var degrees = Math.floor(latitude);
		var minutes = (latitude - degrees) * 60;
		return "N " + degrees + "° " + minutes.toFixed(1) + "'";
	}
	else {
		var degrees = Math.floor(latitude * -1);
		var minutes = (latitude * -1 - degrees) * 60;
		return "S " + degrees + "° " + minutes.toFixed(1) + "'";
    }
}

function formatLonDeg(longitude) {
	if (longitude >= 0) {
		var degrees = Math.floor(longitude);
		var minutes = (longitude - degrees) * 60;
		return "E " + degrees + "° " + minutes.toFixed(1) + "'";
	}
	else {
		var degrees = Math.floor(longitude * -1);
		var minutes = (longitude * -1 - degrees) * 60;
		return "W " + degrees + "° " + minutes.toFixed(1) + "'";
	}
}

function secondsToTime(e) {
	var h = Math.floor(e / 3600).toString().padStart(2, '0'),
		m = Math.floor(e % 3600 / 60).toString().padStart(2, '0'),
		s = Math.floor(e % 60).toString().padStart(2, '0');

	return h + ':' + m + ':' + s;
	//return `${h}:${m}:${s}`;
}

// Button onClick functions for adjusting FOV, AV, ALT, Ho 

function panFOV(panAmount) {
	alterFOV = panAmount;
}

function freezeFOV() {
	alterFOV = 0;
}

function moveFOVreset()
{
	fovH = defaultFOV;
	fovV = fovH * windowH / windowW
}

function panAZ(panAmount) {
	moveAZ = panAmount;
}

function freezeAZ() {
	moveAZ = 0;
}

function moveAZreset()
{
	sexAZ = Math.floor(planeHeadDeg - magVar);
	while (sexAZ < 0)
		sexAZ += 360;
	while (sexAZ > 360)
		sexAZ -= 360;
}

function panALT(panAmount) {
	moveALT = panAmount;
}

function freezeALT() {
	moveALT = 0;
}

function moveALTreset()
{
	sexALT = 0;
}

function moveHup01()
{
	if (sexHo <= windowH - 1)
		sexHo += 1;
}

function moveHup1()
{
	if (sexHo <= windowH - 10)
		sexHo += 10;
}

function moveHup5()
{
	if (sexHo <= windowH - 50)
		sexHo += 50;
}

function moveHreset()
{
	sexHo = Math.floor(windowH / 2);
}

function moveHdown5()
{
	if (sexHo >= 50)
		sexHo -= 50;
}

function moveHdown1()
{
	if (sexHo >= 10)
		sexHo -= 10;
}

function moveHdown01()
{
	if (sexHo >= 1)
		sexHo -= 1;
}

// Button onClick functions for sight reduction tab 

function clearSightings() {
	var starArray = document.getElementsByClassName("starName");
	var APlatArray = document.getElementsByClassName("AP Lat");
	var APlonArray = document.getElementsByClassName("AP Lon");
	var DateArray = document.getElementsByClassName("Date");
	var UTArray = document.getElementsByClassName("UT");
	var HsArray = document.getElementsByClassName("Hs");
	var GHAhourArray = document.getElementsByClassName("GHAhour");
	var GHAincArray = document.getElementsByClassName("GHAinc");
	var SHAincArray = document.getElementsByClassName("SHAinc");
	var GHAtotalArray = document.getElementsByClassName("GHAtotal");
	var DecArray = document.getElementsByClassName("Dec");
	var HcArray = document.getElementsByClassName("Hc");
	var ZnArray = document.getElementsByClassName("Zn");
	var interceptArray = document.getElementsByClassName("Intercept");

	for (let index = 0; index < HsArray.length; index++) {
		starArray[index].options[0].selected = true;
		APlatArray[index].value = "";
		APlonArray[index].value = "";
		DateArray[index].innerHTML = "";
		UTArray[index].innerHTML = "";
		HsArray[index].innerHTML = "";
		GHAhourArray[index].innerHTML = "";
		GHAincArray[index].innerHTML = "";
		SHAincArray[index].innerHTML = "";
		GHAtotalArray[index].innerHTML = "";
		DecArray[index].innerHTML = "";
		HcArray[index].innerHTML = "";
		ZnArray[index].innerHTML = "";
		interceptArray[index].innerHTML = "";
	}
}

function updateSightReductionTab() {
	var starArray = document.getElementsByClassName("starName");
	var HsArray = document.getElementsByClassName("Hs");

	for (let index = 0; index < HsArray.length; index++) {


		if ((starArray[index].options[0].selected == false) && (HsArray[index].innerHTML == "")) {

			curIndex = index;

			// Display Assumed Position Latitude
			var APlatArray = document.getElementsByClassName("AP Lat");
			APlatArray[curIndex].value = formatLatDeg(assumedLat[fixNumber]);

			// Display Assumed Position Longitude
			var APlonArray = document.getElementsByClassName("AP Lon");
			APlonArray[curIndex].value = formatLonDeg(assumedLon[fixNumber]);

			// Display date
			var dayOfMonth = VarGet("E:ZULU DAY OF MONTH", "Number");
			var monthOfYear = VarGet("E:ZULU MONTH OF YEAR", "Number");
			var year = VarGet("E:ZULU YEAR", "Number");
			var DateArray = document.getElementsByClassName("Date");
			DateArray[curIndex].innerHTML = dayOfMonth + "/" + monthOfYear + "/" + year;

			// Display current Universal Time
			var time = VarGet("E:ZULU TIME", "Seconds");
			var UTArray = document.getElementsByClassName("UT");
			UTArray[curIndex].innerHTML = secondsToTime(time);

			// Display Aries GHA for current hour
			const currentDate = monthOfYear + "/" + dayOfMonth + "/" + year;
			var dayIndexOffset = getElapsedDays(startDate, currentDate); // Will be -1 if UT current day is one day earlier than local scenario start day
			var hour = Math.floor(time / 3600);
			var GHAhourArray = document.getElementsByClassName("GHAhour");
			var ariesGHA = ariesGHAd[dayIndexOffset + 1][hour] + ariesGHAm[dayIndexOffset + 1][hour] / 60;
			if (dayIndexOffset >= -1 && dayIndexOffset <= 1) {
				GHAhourArray[curIndex].innerHTML = ariesGHAd[dayIndexOffset + 1][hour] + "° " + ariesGHAm[dayIndexOffset + 1][hour] + "'";
			}
			else {
				GHAhourArray[curIndex].innerHTML = "No data";
			}

			// Interpolate Aries GHA for minutes and seconds
			var GHAincArray = document.getElementsByClassName("GHAinc");
			var ariesGHAinc = getGHAincrement(dayIndexOffset + 1, hour, time);
			GHAincArray[curIndex].innerHTML = formatDMdecimal(ariesGHAinc, 1);

			// Display star SHA
			var SHAincArray = document.getElementsByClassName("SHAinc");
			var selectStarNameArray = document.getElementsByClassName("starName");
			var starNameIndex = starNameList.findIndex(x => x == selectStarNameArray[curIndex].value);
			var starSHA = starsSHAd[starNameIndex] + starsSHAm[starNameIndex] / 60;
			SHAincArray[curIndex].innerHTML = starsSHAd[starNameIndex] + "° " + starsSHAm[starNameIndex] + "'";

			// Display GHA total
			var GHAtotal = ariesGHA + ariesGHAinc + starSHA;
			while (GHAtotal > 360) {
				GHAtotal -= 360;
			}
			var GHAtotalArray = document.getElementsByClassName("GHAtotal");
			GHAtotalArray[curIndex].innerHTML = formatDMdecimal(GHAtotal, 1);

			// Display star Dec
			var DecArray = document.getElementsByClassName("Dec");
			if (starsDECd[starNameIndex] > 0) {
				var starDEC = starsDECd[starNameIndex] + starsDECm[starNameIndex] / 60;
			}
			else {
				var starDEC = starsDECd[starNameIndex] - starsDECm[starNameIndex] / 60;
			}
			DecArray[curIndex].innerHTML = formatLatDeg(starDEC);
		}
	}
}

// Button onClick functions for plotting tab

function plotFinalLeg() {
	if (showFinalLeg == 0) {
		showFinalLeg = 1;
		document.getElementById("finalLegButton").innerHTML = "Hide Leg Info";
	}
	else {
		showFinalLeg = 0;
		document.getElementById("finalLegButton").innerHTML = "Show Leg Info";
    }
}

function hidePlane() {
	plotPlane = 0;
	document.getElementById('plane').style.display = 'none';
	document.getElementById('showButton').style.display = 'inline';
	document.getElementById('hideButton').style.display = 'none';
}

function showPlane() {
	plotPlane = 1;
	document.getElementById('plane').style.display = 'inline';
	document.getElementById('hideButton').style.display = 'inline';
	document.getElementById('showButton').style.display = 'none';
}