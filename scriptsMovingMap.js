const mapNorth = mapNorthX;
const mapEast = mapEastX;
const mapSouth = mapSouthX;
const mapWest = mapWestX;
const mapWidth = mapWidthX;
const mapHeight = mapHeightX;
var zoom = false;
var curMap = "aerialLabelsMap";
var pixelsTop, pixelsTopZoom;
var pixelsLeft, pixelsLeftZoom;
var clipTop, clipRight, clipBottom, clipLeft;

function update(timestamp)
{
	var plane = document.getElementById("plane");
	var planeHeadingDeg = VarGet("A:PLANE HEADING DEGREES MAGNETIC" ,"Radians") * 180 / Math.PI;
	var planeLonDeg = VarGet("A:PLANE LONGITUDE" ,"Radians") * 180 / Math.PI; // x
	var planeLatDeg = VarGet("A:PLANE LATITUDE" ,"Radians") * 180 / Math.PI;  // y
	pixelsTop = Math.round((planeLatDeg - mapNorth) / (mapSouth - mapNorth) * mapHeight);
	pixelsLeft = Math.round((planeLonDeg - mapWest) / (mapEast - mapWest) * mapWidth);
	if (!Boolean(zoom)){
		document.getElementById('plane').style.top=pixelsTop - 15 + "px";
		document.getElementById('plane').style.left=pixelsLeft - 15 + "px";
	}
	else {
		pixelsTopZoom = pixelsTop * 2;
		pixelsLeftZoom = pixelsLeft * 2;
		clipTop = pixelsTopZoom - (mapHeight / 2);
		if (clipTop < 0) {
			clipTop = 0;
		} else if (clipTop > mapHeight) {
			clipTop = mapHeight;
		}
		clipRight = pixelsLeftZoom + (mapWidth / 2);
		if (clipRight > mapWidth * 2) {
			clipRight = mapWidth * 2;
		} else if (clipRight < mapWidth) {
			clipRight = mapWidth;
		}
		clipBottom = clipTop + mapHeight;
		clipLeft = clipRight - mapWidth;
		document.getElementById(curMap + 'Img').style.top = '-' + clipTop + 'px';
		document.getElementById(curMap + 'Img').style.left = '-' + clipLeft + 'px';
		document.getElementById(curMap + 'Img').style.position = 'absolute';
		document.getElementById(curMap + 'Img').style.clip = 'rect(' + clipTop + 'px,' + clipRight + 'px,' + clipBottom + 'px,' + clipLeft + 'px)';
		document.getElementById('plane').style.top=pixelsTopZoom - clipTop - 15 + "px";
		document.getElementById('plane').style.left=pixelsLeftZoom - clipLeft - 15 + "px";
	}
	plane.style.transform = "rotate(" + planeHeadingDeg + "deg)";
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);

function hidePlane() {
	document.getElementById('plane').style.display = 'none';
	document.getElementById('showButton').style.display = 'inline';
	document.getElementById('hideButton').style.display = 'none';
}
function showPlane() {
	document.getElementById('plane').style.display = 'inline';
	document.getElementById('hideButton').style.display = 'inline';
	document.getElementById('showButton').style.display = 'none';
}
function showAerialMap() {
	if (!Boolean(zoom)){
		document.getElementById('aerialLabelsMap').style.display = 'none';
		document.getElementById('roadMap').style.display = 'none';
		document.getElementById('aerialMap').style.display = 'inline';
		curMap = 'aerialMap';
	} else {
		document.getElementById('aerialLabelsMapZoom').style.display = 'none';
		document.getElementById('roadMapZoom').style.display = 'none';
		document.getElementById('aerialMapZoom').style.display = 'inline';
		curMap = 'aerialMapZoom';
	}
	document.getElementById('aerialButton').style.display = 'none';
	document.getElementById('aerialLabelsButton').style.display = 'inline';
	document.getElementById('roadButton').style.display = 'inline';
}
function showAerialLabelsMap() {
	if (!Boolean(zoom)){
		document.getElementById('aerialMap').style.display = 'none';
		document.getElementById('aerialLabelsMap').style.display = 'inline';
		document.getElementById('roadMap').style.display = 'none';
		curMap = 'aerialLabelsMap';
	} else {
		document.getElementById('aerialMapZoom').style.display = 'none';
		document.getElementById('aerialLabelsMapZoom').style.display = 'inline';
		document.getElementById('roadMapZoom').style.display = 'none';
		curMap = 'aerialLabelsMapZoom';
	}
	document.getElementById('aerialButton').style.display = 'inline';
	document.getElementById('aerialLabelsButton').style.display = 'none';
	document.getElementById('roadButton').style.display = 'inline';
}
function showRoadMap() {
	if (!Boolean(zoom)){
		document.getElementById('aerialMap').style.display = 'none';
		document.getElementById('aerialLabelsMap').style.display = 'none';
		document.getElementById('roadMap').style.display = 'inline';
		curMap = 'roadMap';
	} else {
		document.getElementById('aerialMapZoom').style.display = 'none';
		document.getElementById('aerialLabelsMapZoom').style.display = 'none';
		document.getElementById('roadMapZoom').style.display = 'inline';
		curMap = 'roadMapZoom';
	}
	document.getElementById('aerialButton').style.display = 'inline';
	document.getElementById('aerialLabelsButton').style.display = 'inline';
	document.getElementById('roadButton').style.display = 'none';
}
function showZoomMap() {
	zoom = true;
	document.getElementById('zoomButton').style.display = 'none';
	document.getElementById('overviewButton').style.display = 'inline';
	if (curMap == 'aerialMap') {
		document.getElementById('aerialMap').style.display = 'none';
		document.getElementById('aerialMapZoom').style.display = 'inline';
		curMap = 'aerialMapZoom';
	} else if (curMap == 'aerialLabelsMap') {
		document.getElementById('aerialLabelsMap').style.display = 'none';
		document.getElementById('aerialLabelsMapZoom').style.display = 'inline';
		curMap = 'aerialLabelsMapZoom';
	} else {
		document.getElementById('roadMap').style.display = 'none';
		document.getElementById('roadMapZoom').style.display = 'inline';
		curMap = 'roadMapZoom';
	} 
}
function showOverviewMap() {
	zoom = false;
	document.getElementById('zoomButton').style.display = 'inline';
	document.getElementById('overviewButton').style.display = 'none';
	if (curMap == 'aerialMapZoom') {
		document.getElementById('aerialMap').style.display = 'inline';
		document.getElementById('aerialMapZoom').style.display = 'none';
		curMap = 'aerialMap';
	} else if (curMap == 'aerialLabelsMapZoom') {
		document.getElementById('aerialLabelsMap').style.display = 'inline';
		document.getElementById('aerialLabelsMapZoom').style.display = 'none';
		curMap = 'aerialLabelsMap';
	} else {
		document.getElementById('roadMap').style.display = 'inline';
		document.getElementById('roadMapZoom').style.display = 'none';
		curMap = 'roadMap';
	} 
}