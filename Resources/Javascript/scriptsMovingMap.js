const mapNorth = mapNorthX; //these 4 map edges are for the central square of 375 pixels in zoom1 images
const mapEast = mapEastX;
const mapSouth = mapSouthX;
const mapWest = mapWestX;
const mapWidth = mapWidthX;
const mapHeight = mapHeightX;
var zoomN, zoomE, zoomS, zoomW; // adjusted map edges allowing for different zoom factor of 1, 2, or 4
const imagePixels = 1500;
var zoomFactor = 1;
var zoomToggle = 0;
var curMap = "aerialLabelsMap";
var planeTopPixels, planeLeftPixels;
var clipTop, clipRight, clipBottom, clipLeft;

function update(timestamp)
{
	handleButtonChoices();
	var plane = document.getElementById("plane");
	var planeHeadingDeg = VarGet("A:PLANE HEADING DEGREES MAGNETIC" ,"Radians") * 180 / Math.PI;
	var planeLonDeg = VarGet("A:PLANE LONGITUDE" ,"Radians") * 180 / Math.PI; // x
	var planeLatDeg = VarGet("A:PLANE LATITUDE" ,"Radians") * 180 / Math.PI;  // y
	if (zoomFactor > 1) {
		zoomN = mapNorth + (mapNorth - mapSouth) * ((4 / zoomFactor - 1) * 0.5);
		zoomE = mapEast + (mapEast - mapWest) * ((4 / zoomFactor - 1) * 0.5);
		zoomS = mapSouth - (mapNorth - mapSouth) * ((4 / zoomFactor - 1) * 0.5);
		zoomW = mapWest - (mapEast - mapWest) * ((4 / zoomFactor - 1) * 0.5);
		planeTopPixels = Math.round((planeLatDeg - zoomN) / (zoomS - zoomN) * imagePixels);
		planeLeftPixels = Math.round((planeLonDeg - zoomW) / (zoomE - zoomW) * imagePixels);
		clipTop = planeTopPixels - (mapHeight / 2);
		if (clipTop < 0) {
			clipTop = 0;
		} else if (clipTop > (imagePixels - mapHeight)) {
			clipTop = (imagePixels - mapHeight);
		}
		clipRight = planeLeftPixels + (mapWidth / 2);
		if (clipRight > imagePixels) {
			clipRight = imagePixels;
		} else if (clipRight < mapWidth) {
			clipRight = mapWidth;
		}
		clipBottom = clipTop + mapHeight;
		clipLeft = clipRight - mapWidth;
		document.getElementById(curMap + 'Zoom' + zoomFactor + 'Img').style.top = '-' + clipTop + 'px';
		document.getElementById(curMap + 'Zoom' + zoomFactor + 'Img').style.left = '-' + clipLeft + 'px';
		document.getElementById(curMap + 'Zoom' + zoomFactor + 'Img').style.position = 'absolute';
		document.getElementById(curMap + 'Zoom' + zoomFactor + 'Img').style.clip = 'rect(' + clipTop + 'px,' + clipRight + 'px,' + clipBottom + 'px,' + clipLeft + 'px)';
		document.getElementById('plane').style.top=planeTopPixels - clipTop - 15 + "px";
		document.getElementById('plane').style.left=planeLeftPixels - clipLeft - 15 + "px";
	}
	else {
		zoomN = mapNorth + (mapNorth - mapSouth) * (mapHeight - 375) / 375 * 0.5;
		zoomE = mapEast + (mapEast - mapWest) * (mapWidth - 375) / 375 * 0.5;
		zoomS = mapSouth - (mapNorth - mapSouth) * (mapHeight - 375) / 375 * 0.5;
		zoomW = mapWest - (mapEast - mapWest) * (mapWidth - 375) / 375 * 0.5;
		planeTopPixels = Math.round((planeLatDeg - zoomN) / (zoomS - zoomN) * mapHeight);
		planeLeftPixels = Math.round((planeLonDeg - zoomW) / (zoomE - zoomW) * mapWidth);
		document.getElementById('plane').style.top=planeTopPixels - 15 + "px";
		document.getElementById('plane').style.left=planeLeftPixels - 15 + "px";
	}
	plane.style.transform = "rotate(" + planeHeadingDeg + "deg)";
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);

function handleButtonChoices() {
	var changeZoom = VarGet("A:ALTERNATE STATIC SOURCE OPEN" ,"Bool"); 
	if (changeZoom != zoomToggle){
		zoomToggle = changeZoom;
		if (zoomFactor == 1){
			showZoom2Map()
		} else if (zoomFactor == 2){
			showZoom4Map()
		} else{
			showZoom1Map()
		}
	}
}

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
	curMap = 'aerialMap';
	if (zoomFactor == 1){
		document.getElementById('aerialLabelsMapZoom1').style.display = 'none';
		document.getElementById('roadMapZoom1').style.display = 'none';
		document.getElementById('aerialMapZoom1').style.display = 'inline';
	} else if (zoomFactor == 2){
		document.getElementById('aerialLabelsMapZoom2').style.display = 'none';
		document.getElementById('roadMapZoom2').style.display = 'none';
		document.getElementById('aerialMapZoom2').style.display = 'inline';
	} else {
		document.getElementById('aerialLabelsMapZoom4').style.display = 'none';
		document.getElementById('roadMapZoom4').style.display = 'none';
		document.getElementById('aerialMapZoom4').style.display = 'inline';
	}
	document.getElementById('aerialButton').style.display = 'none';
	document.getElementById('aerialLabelsButton').style.display = 'inline';
	document.getElementById('roadButton').style.display = 'inline';
}

function showAerialLabelsMap() {
	curMap = 'aerialLabelsMap';
	if (zoomFactor == 1){
		document.getElementById('aerialLabelsMapZoom1').style.display = 'inline';
		document.getElementById('roadMapZoom1').style.display = 'none';
		document.getElementById('aerialMapZoom1').style.display = 'none';
	} else if (zoomFactor == 2){
		document.getElementById('aerialLabelsMapZoom2').style.display = 'inline';
		document.getElementById('roadMapZoom2').style.display = 'none';
		document.getElementById('aerialMapZoom2').style.display = 'none';
	} else {
		document.getElementById('aerialLabelsMapZoom4').style.display = 'inline';
		document.getElementById('roadMapZoom4').style.display = 'none';
		document.getElementById('aerialMapZoom4').style.display = 'none';
	}
	document.getElementById('aerialButton').style.display = 'inline';
	document.getElementById('aerialLabelsButton').style.display = 'none';
	document.getElementById('roadButton').style.display = 'inline';
}

function showRoadMap() {
	curMap = 'roadMap';
	if (zoomFactor == 1){
		document.getElementById('aerialLabelsMapZoom1').style.display = 'none';
		document.getElementById('roadMapZoom1').style.display = 'inline';
		document.getElementById('aerialMapZoom1').style.display = 'none';
	} else if (zoomFactor == 2){
		document.getElementById('aerialLabelsMapZoom2').style.display = 'none';
		document.getElementById('roadMapZoom2').style.display = 'inline';
		document.getElementById('aerialMapZoom2').style.display = 'none';
	} else {
		document.getElementById('aerialLabelsMapZoom4').style.display = 'none';
		document.getElementById('roadMapZoom4').style.display = 'inline';
		document.getElementById('aerialMapZoom4').style.display = 'none';
	}
	document.getElementById('aerialButton').style.display = 'inline';
	document.getElementById('aerialLabelsButton').style.display = 'inline';
	document.getElementById('roadButton').style.display = 'none';
}

function showZoom1Map() {
	zoomFactor = 1;
	document.getElementById('zoom1Button').style.display = 'none';
	document.getElementById('zoom2Button').style.display = 'inline';
	document.getElementById('zoom4Button').style.display = 'inline';
	if (curMap == 'aerialMap') {
		document.getElementById('aerialMapZoom1').style.display = 'inline';
		document.getElementById('aerialMapZoom2').style.display = 'none';
		document.getElementById('aerialMapZoom4').style.display = 'none';
	} else if (curMap == 'aerialLabelsMap') {
		document.getElementById('aerialLabelsMapZoom1').style.display = 'inline';
		document.getElementById('aerialLabelsMapZoom2').style.display = 'none';
		document.getElementById('aerialLabelsMapZoom4').style.display = 'none';
	} else {
		document.getElementById('roadMapZoom1').style.display = 'inline';
		document.getElementById('roadMapZoom2').style.display = 'none';
		document.getElementById('roadMapZoom4').style.display = 'none';
	} 
}

function showZoom2Map() {
	zoomFactor = 2;
	document.getElementById('zoom1Button').style.display = 'inline';
	document.getElementById('zoom2Button').style.display = 'none';
	document.getElementById('zoom4Button').style.display = 'inline';
	if (curMap == 'aerialMap') {
		document.getElementById('aerialMapZoom1').style.display = 'none';
		document.getElementById('aerialMapZoom2').style.display = 'inline';
		document.getElementById('aerialMapZoom4').style.display = 'none';
	} else if (curMap == 'aerialLabelsMap') {
		document.getElementById('aerialLabelsMapZoom1').style.display = 'none';
		document.getElementById('aerialLabelsMapZoom2').style.display = 'inline';
		document.getElementById('aerialLabelsMapZoom4').style.display = 'none';
	} else {
		document.getElementById('roadMapZoom1').style.display = 'none';
		document.getElementById('roadMapZoom2').style.display = 'inline';
		document.getElementById('roadMapZoom4').style.display = 'none';
	} 
}

function showZoom4Map() {
	zoomFactor = 4;
	document.getElementById('zoom1Button').style.display = 'inline';
	document.getElementById('zoom2Button').style.display = 'inline';
	document.getElementById('zoom4Button').style.display = 'none';
	if (curMap == 'aerialMap') {
		document.getElementById('aerialMapZoom1').style.display = 'none';
		document.getElementById('aerialMapZoom2').style.display = 'none';
		document.getElementById('aerialMapZoom4').style.display = 'inline';
	} else if (curMap == 'aerialLabelsMap') {
		document.getElementById('aerialLabelsMapZoom1').style.display = 'none';
		document.getElementById('aerialLabelsMapZoom2').style.display = 'none';
		document.getElementById('aerialLabelsMapZoom4').style.display = 'inline';
	} else {
		document.getElementById('roadMapZoom1').style.display = 'none';
		document.getElementById('roadMapZoom2').style.display = 'none';
		document.getElementById('roadMapZoom4').style.display = 'inline';
	} 
}