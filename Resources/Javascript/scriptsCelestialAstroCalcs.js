function calcHcZn(ptsList, ptsListIndex) {
	var HcZnIndirect = new Array();
	var topPixels = ptsList[ptsListIndex + 3];
	var leftPixels = ptsList[ptsListIndex + 2];
	HcZnIndirect[0] = toRadians((windowH - topPixels) / windowH * fovV + sexALT);
	HcZnIndirect[1] = toRadians(leftPixels / windowW * fovH - fovH / 2 + sexAZ + planeHeadDeg);
	if (HcZnIndirect[1] > 360)
		HcZnIndirect[1] -= 360;
	if (HcZnIndirect[1] < 0)
		HcZnIndirect[1] += 360;

	return HcZnIndirect;
}

function calcLocalStarPositions(latitude, longitude, planeHeadDeg) {
	var ptsList = new Array(); // Pixel positions for stars in current sextant FOV

	for (let starIndex = 0; starIndex < raH.length; starIndex++) {
		// Using formulae from here http://www.stargazing.net/kepler/altaz.html
		// Note: working in radians rather than degrees until ALT/AZ as Javascript trig uses radians
		var RAhr = hmsToDecimal(raH[starIndex], raM[starIndex], raS[starIndex]);
		var RA = toRadians(RAhr * 15);
		var DEC = toRadians(hmsToDecimal(decD[starIndex], decM[starIndex], decS[starIndex]));
		var LST = getLocalSiderialTime(longitude);
		var HA = getHourAngle(LST, RA);
		var ALT = getALT(DEC, latitude, HA);
		var AZ = getAZ(DEC, ALT, latitude, HA);
		var relSexAZ = sexAZ + planeHeadDeg;
		var AZbearingDelta = getBearingDif(AZ, relSexAZ); // Comparing star AZ to sextant window mid-point bearing
		if ((ALT > sexALT) && (ALT < sexALT + fovV) && (AZbearingDelta <= fovH / 2)) {
			var relativeAZ = getRelativeAZ(AZ, relSexAZ, fovH); // Number of degrees from left edge of sextant window
			var left = Math.round(relativeAZ / fovH * windowW);
			var top = Math.round((sexALT + fovV - ALT) / fovV * windowH);
			updatePtsList(starIndex, ptsList, left, top);
		}
	}

	return ptsList;
}

// Input is a coord in degrees, output is top and left pixel values to position coord on plotting image
function convertCoordToPixels(coord) {
	const lat = coord[0];
	const lon = coord[1];

	if (lon >= westEdge && lon <= eastEdge) {
		var left = (lon - westEdge) / (eastEdge - westEdge) * windowW;
	}
	else {
		var left = 0;
	}
	if (lat <= northEdge && lat >= southEdge) {
		var top = (northEdge - lat) / (northEdge - southEdge) * windowH; 
	}
	else {
		var top = 0;
    }
	var coordPixels = [top, left];

	return coordPixels;
}

function convertZtoZn(Z, LHA, latitude) {
	if (latitude >= 0) {
		if (LHA >= 180) {
			return Z;
		}
		else {
			return 360 - Z;
        }
	}
	else {
		if (LHA >= 180) {
			return 180 - Z;
		}
		else {
			return 180 + Z;
		}
    }
}

function getALT(DEC, LAT, HA) {
	return toDegrees(Math.asin(Math.sin(DEC) * Math.sin(LAT) + Math.cos(DEC) * Math.cos(LAT) * Math.cos(HA)));
}

function getAZ(DEC, ALT, LAT, HA) {
	var ALTrad = toRadians(ALT);
	var A = Math.acos((Math.sin(DEC) - Math.sin(ALTrad) * Math.sin(LAT)) / (Math.cos(ALTrad) * Math.cos(LAT)));
	if (Math.sin(HA) < 0)
		return toDegrees(A);
	else
		return 360 - toDegrees(A);
}

function getBearing(lat1, lon1, lat2, lon2) {
	const y = Math.sin(lon2 - lon1) * Math.cos(lat2);
	const x = Math.cos(lat1) * Math.sin(lat2) -
		Math.sin(lat1) * Math.cos(lat2) * Math.cos(lon2 - lon1);
	const angle = Math.atan2(y, x);
	return (angle * 180 / Math.PI + 360) % 360; // in degrees
}

// Inputs in degrees, output in feet
function getDistance(lat1, lon1, lat2, lon2) {
	// http://www.movable-type.co.uk/scripts/latlong.html

	const pt1lat = toRadians(lat1);
	const pt1lon = toRadians(lon1);
	const pt2lat = toRadians(lat2);
	const pt2lon = toRadians(lon2);
	const deltaLat = pt2lat - pt1lat;
	const deltaLon = pt2lon - pt1lon;
	const R = 20902230.971129; // Radius of earth at equator in feet

	const a = Math.sin(deltaLat / 2) * Math.sin(deltaLat / 2) + Math.cos(lat1) * Math.cos(lat2) * Math.sin(deltaLon / 2) * Math.sin(deltaLon / 2);
	const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

	return R * c; 
}

function getFixCoord() {
	// http://www.movable-type.co.uk/scripts/latlong.html (Intermediate point)

	const pt1lat = LOPLOPcoordLat[sightNumber - 2];
	const pt1lon = LOPLOPcoordLon[sightNumber - 2];
	const pt2lat = LOPLOPcoordLat[sightNumber - 1];
	const pt2lon = LOPLOPcoordLon[sightNumber - 1];
	const pt1pt2dist = getDistance(pt1lat, pt1lon, pt2lat, pt2lon);
	const pt1pt2midpt = getIntermediatePoint(pt1pt2dist, 0.5, pt1lat, pt1lon, pt2lat, pt2lon);
	const pt3lat = LOPLOPcoordLat[sightNumber];
	const pt3lon = LOPLOPcoordLon[sightNumber];
	const pt3distance = getDistance(pt1pt2midpt[0], pt1pt2midpt[1], pt3lat, pt3lon);
	return getIntermediatePoint(pt3distance, 1 / 3, pt1pt2midpt[0], pt1pt2midpt[1], pt3lat, pt3lon);
}

function getHourAngle(LST, RA) {
	var HA = LST - RA;
	while (HA < 0)
		HA += toRadians(360);

	return HA;
}

// Inputs in feet, decimal and degrees and output in degrees 
function getIntermediatePoint(distance, fraction, lat1, lon1, lat2, lon2) {
	// http://www.movable-type.co.uk/scripts/latlong.html

	const pt1lat = toRadians(lat1);
	const pt1lon = toRadians(lon1);
	const pt2lat = toRadians(lat2);
	const pt2lon = toRadians(lon2);
	const R = 20902230.971129; // Radius of earth at equator in feet
	const d = distance / R;
	const a = Math.sin((1 - fraction) * d) / Math.sin(d);
	const b = Math.sin(fraction * d) / Math.sin(d)
	const x = a * Math.cos(pt1lat) * Math.cos(pt1lon) + b * Math.cos(pt2lat) * Math.cos(pt2lon);
	const y = a * Math.cos(pt1lat) * Math.sin(pt1lon) + b * Math.cos(pt2lat) * Math.sin(pt2lon);
	const z = a * Math.sin(pt1lat) + b * Math.sin(pt2lat);
	var lat3 = Math.atan2(z, Math.sqrt(x * x + y * y));
	var lon3 = Math.atan2(y, x);
	lat3 = toDegrees(lat3);
	lon3 = toDegrees(lon3);
	lon3 = (lon3 + 540) % 360 - 180;

	return [lat3, lon3];
}

function getJ2000Day(zuluTime) {
	var zuluDayOfMonth = VarGet("E:ZULU DAY OF MONTH", "Number");
	var zuluMonth = VarGet("E:ZULU MONTH OF YEAR", "Number");
	var zuluYear = VarGet("E:ZULU YEAR", "Number");

	var dayFraction = zuluTime / (60 * 60 * 24);
	var daysToBegMth = daysToBeginMth[zuluMonth];
	if ((zuluYear % 4 == 0) && (zuluMonth > 2))
		daysToBegMth = daysToBegMth + 1
	var daysToBegYear = -1.5 + (zuluYear - 2000) * 365 + Math.ceil((zuluYear - 2000) / 4);

	return dayFraction + daysToBegMth + zuluDayOfMonth + daysToBegYear;
}

function getLocalSiderialTime(longitude) {
	var zuluTime = VarGet("E:ZULU TIME", "Seconds");
	var j2000Days = getJ2000Day(zuluTime);
	var UT = zuluTime / (60 * 60);
	var lonDeg = toDegrees(longitude);
	var LSTdeg = 100.46 + 0.985647 * j2000Days + lonDeg + 15 * UT;
	while (LSTdeg < 0)
		LSTdeg += 360;
	while (LSTdeg > 360)
		LSTdeg -= 360;

	return toRadians(LSTdeg);
}

function getLOPLOPcoord() {
	// http://www.mygeodesy.id.au/documents/Chapter%2010.pdf

	var LOPLOPcoord = [0];
	for (let pt1 = sightNumber - 2; pt1 <= sightNumber; pt1++) {
		var pt2 = pt1 + 1;
		if (pt2 == sightNumber + 1) {
			pt2 = sightNumber - 2;
        }
		var LatPt1 = LOPZncoordLat[pt1];
		var LatPt2 = LOPZncoordLat[pt2];
		var LonPt1 = LOPZncoordLon[pt1];
		var LonPt2 = LOPZncoordLon[pt2];
		with (Math) {
			var BearingPt1 = Zn[pt1] + PI / 2;
			var BearingPt2 = Zn[pt2] + PI / 2;
			var LOPLOPlat = (LatPt1 * tan(BearingPt1) - LatPt2 * tan(BearingPt2) + LonPt2 - LonPt1) / (tan(BearingPt1) - tan(BearingPt2));
			var LOPLOPlon = LOPLOPlat * tan(BearingPt1) - LatPt1 * tan(BearingPt1) + LonPt1;
		}
		LOPLOPcoord.push(LOPLOPlat);
		LOPLOPcoord.push(LOPLOPlon);
	}

	return LOPLOPcoord;
}

function getLOPZncoord(lat1, lon1, bearing, distance) {
	// http://www.movable-type.co.uk/scripts/latlong.html (Destination point given distance and bearing from start point)
	const earthRadius = 3963; // nm

	lat1 = toRadians(lat1);
	lon1 = toRadians(lon1);
	bearing = toRadians(bearing);
	var lat2 = Math.asin(Math.sin(lat1) * Math.cos(distance / earthRadius) + Math.cos(lat1) * Math.sin(distance / earthRadius) * Math.cos(bearing));
	var lon2 = lon1 + Math.atan2(Math.sin(bearing) * Math.sin(distance / earthRadius) * Math.cos(lat1), Math.cos(distance / earthRadius) - Math.sin(lat1) * Math.sin(lat2));
	lat2 = toDegrees(lat2);
	lon2 = toDegrees(lon2);
	lon2 = (lon2 + 540) % 360 - 180; // Normalise to -180 ... +180
	var LOPcoord = [lat2, lon2];
	return LOPcoord;
}

function hmsToDecimal(hour, minute, second) {
    if (hour >= 0) {
        return hour + minute / 60 + second / 3600
    }
    else {
        return hour - minute / 60 - second / 3600
    }
}

function toDegrees(radians) {
	return radians * 180 / Math.PI;
}

function toRadians(degrees) {
	return degrees * Math.PI / 180;
}
