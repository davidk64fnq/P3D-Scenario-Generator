

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
	var zuluDay = VarGet("E:ZULU DAY OF WEEK", "Number");
	var zuluMonth = VarGet("E:ZULU MONTH OF YEAR", "Number");
	var zuluYear = VarGet("E:ZULU YEAR", "Number");

	var decTime = zuluTime / (60 * 60 * 24);
	var daysToBegMth = daysToBeginMth[zuluMonth];
	if ((zuluYear % 4 == 0) && (zuluMonth > 2))
		daysToBegMth = daysToBegMth + 1
	var daysToBegYear = -0.5 + (zuluYear - 2000) * 365 + Math.floor((zuluYear - 2001) / 4);

	return decTime + daysToBegMth + zuluDay + daysToBegYear;
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
