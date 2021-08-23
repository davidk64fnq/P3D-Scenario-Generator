function convertCoordToPixels(coord) {
	var lat = coord[0];
	var lon = coord[1];
	var left = (lon - westEdge) / (eastEdge - westEdge) * windowW;
	var top = (northEdge - lat) / (northEdge - southEdge) * windowH;
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

function getHourAngle(LST, RA) {
	var HA = LST - RA;
	while (HA < 0)
		HA += toRadians(360);

	return HA;
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

function getLOPcoord(lat1, lon1, bearing, distance) {
	const earthRadius = 3963; // nm
	lat1 = lat1 / 180 * Math.PI;
	lon1 = lon1 / 180 * Math.PI;
	var lat2 = Math.asin(Math.sin(lat1) * Math.cos(distance / earthRadius) + Math.cos(lat1) * Math.sin(distance / earthRadius) * Math.cos(bearing));
	var lon2 = lon1 + Math.atan2(Math.sin(bearing) * Math.sin(distance / earthRadius) * Math.cos(lat1), Math.cos(distance / earthRadius) - Math.sin(lat1) * Math.sin(lat2));
	lat2 = lat2 / Math.PI * 180;
	lon2 = lon2 / Math.PI * 180;
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
