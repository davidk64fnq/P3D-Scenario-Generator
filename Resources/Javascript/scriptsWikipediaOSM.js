// Constants populated by P3D Scenario Generator application

// The images for 3 zoom levels for each leg all have the same lat/lon boundaries
const mapNorth = [0,mapNorthX]; 
const mapEast = [0,mapEastX];
const mapSouth = [0,mapSouthX];
const mapWest = [0,mapWestX];
const mapWidth = 512;
const mapHeight = 512;
var zoomFactor = 1;
var zoomToggle = 0;
var oldLegNo = 1;
const imagePixels = [0, 512, 1024, 2048]; // number of images pixels for zoom levels

function update(timestamp)
{
	handleButtonChoices();
	var legNo = VarGet("S:currentLegNo" ,"NUMBER");
	refeshImages(legNo);
	var plane = document.getElementById("plane");
	var planeHeadingDeg = VarGet("A:PLANE HEADING DEGREES MAGNETIC" ,"Radians") * 180 / Math.PI;
	var planeLonDeg = VarGet("A:PLANE LONGITUDE" ,"Radians") * 180 / Math.PI; // x
	var planeLatDeg = VarGet("A:PLANE LATITUDE" ,"Radians") * 180 / Math.PI;  // y
	var planeTopPixels = Math.round((planeLatDeg - mapNorth[legNo]) / (mapSouth[legNo] - mapNorth[legNo]) * imagePixels[zoomFactor]);
	var planeLeftPixels = Math.round((planeLonDeg - mapWest[legNo]) / (mapEast[legNo] - mapWest[legNo]) * imagePixels[zoomFactor]);
	var clipTop = planeTopPixels - (mapHeight / 2);
	if (clipTop < 0) {
		clipTop = 0;
	} else if (clipTop > (imagePixels[zoomFactor] - mapHeight)) {
		clipTop = (imagePixels[zoomFactor] - mapHeight);
	}
	clipRight = planeLeftPixels + (mapWidth / 2);
	if (clipRight > imagePixels[zoomFactor]) {
		clipRight = imagePixels[zoomFactor];
	} else if (clipRight < mapWidth) {
		clipRight = mapWidth;
	}
	var clipBottom = clipTop + mapHeight;
	var clipLeft = clipRight - mapWidth;
	document.getElementById('roadMapZoom' + zoomFactor + 'Img').style.top = '-' + clipTop + 'px';
	document.getElementById('roadMapZoom' + zoomFactor + 'Img').style.left = '-' + clipLeft + 'px';
	document.getElementById('roadMapZoom' + zoomFactor + 'Img').style.position = 'absolute';
	document.getElementById('roadMapZoom' + zoomFactor + 'Img').style.clip = 'rect(' + clipTop + 'px,' + clipRight + 'px,' + clipBottom + 'px,' + clipLeft + 'px)';
	document.getElementById('plane').style.top=planeTopPixels - clipTop - 15 + "px";
	document.getElementById('plane').style.left=planeLeftPixels - clipLeft - 15 + "px";
	plane.style.transform = "rotate(" + planeHeadingDeg + "deg)";
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);

function handleButtonChoices() {
	var changeZoom = VarGet("A:ALTERNATE STATIC SOURCE OPEN" ,"Bool"); 
	if (changeZoom != zoomToggle){
		zoomToggle = changeZoom;
		if (zoomFactor == 1){
			showZoom1Map()
		} else if (zoomFactor == 2){
			showZoom2Map()
		} else{
			showZoom3Map()
		}
	}
}

function refeshImages(legNo) {
	if (legNo != oldLegNo) {
		oldLegNo = legNo;
		document.getElementById('roadMapZoom1Img').src='LegRoute_0' + legNo + '_zoom1.jpg';
		document.getElementById('roadMapZoom2Img').src='LegRoute_0' + legNo + '_zoom2.jpg';
		document.getElementById('roadMapZoom3Img').src='LegRoute_0' + legNo + '_zoom3.jpg';
		if (zoomFactor == 1){
			showZoom1Map()
		} else if (zoomFactor == 2){
			showZoom2Map()
		} else{
			showZoom3Map()
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

function showZoom1Map() {
	zoomFactor = 1;
	document.getElementById('roadMapZoom1').style.display = 'inline';
	document.getElementById('roadMapZoom2').style.display = 'none';
	document.getElementById('roadMapZoom3').style.display = 'none';
}

function showZoom2Map() {
	zoomFactor = 2;
	document.getElementById('roadMapZoom1').style.display = 'none';
	document.getElementById('roadMapZoom2').style.display = 'inline';
	document.getElementById('roadMapZoom3').style.display = 'none';
}

function showZoom3Map() {
	zoomFactor = 3;
	document.getElementById('roadMapZoom1').style.display = 'none';
	document.getElementById('roadMapZoom2').style.display = 'none';
	document.getElementById('roadMapZoom3').style.display = 'inline';
}