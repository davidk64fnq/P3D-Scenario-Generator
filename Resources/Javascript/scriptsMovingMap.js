// Constants populated by P3D Scenario Generator application, the images for 3 zoom levels for each leg 
// all have the same lat/lon boundaries (0 array entries so first leg is referenced as index 1)
// Constants populated by P3D Scenario Generator application
var mapNorthX = null;
var mapEastX = null;
var mapSouthX = null;
var mapWestX = null;
var imagePixelsX = null;
var viewPortWidthX = null;
var viewPortHeightX = null;
var zoom1FilenameSuffixX = null;
var zoom2FilenameSuffixX = null;
var zoom3FilenameSuffixX = null;

// The 0 entry ensures first leg starts at index 1
const mapNorth = [0, ...mapNorthX];
const mapEast = [0, ...mapEastX];
const mapSouth = [0, ...mapSouthX];
const mapWest = [0, ...mapWestX];
const imagePixels = [0, ...imagePixelsX];
const viewPortWidth = viewPortWidthX;
const viewPortHeight = viewPortHeightX;

var zoomFactor = 1;     // Takes values from 1 to 3 for the 3 levels of increasing zoom
var zoomCycle = 0;      // Tracks state of a simulator variable toggle to know when to cycle to next zoom level
var planeToggle = 0;    // Tracks state of a simulator variable toggle to know when to show/hide plane icon
var oldLegNo = 0;       // Tracks scenario variable that indicates current leg number
var dragEnabled = 0;    // Mouse drag of map is enabled when not visible, disabled when plane visible
var dragHappening = 0;  // Indicates mouse is being dragged
var clipTop = 0;        // Pixel reference of top of viewport in map image
var clipRight = 0;      // Pixel reference to righthand edge of viewport in map image
var dragStartX = 0;     // Records x coordinate of mouse down event
var dragStartY = 0;     // Records y coordinate of mouse down event
var dragXamount = 0;    // On mouse move event records how much the x coordinate has changed from dragStartX
var dragYamount = 0;    // On mouse move event records how much the y coordinate has changed from dragStartY


// Coordinate system for image references in pixels is origin as top lefthand corner, so top left is always 1,1 and bottom right 
// of viewport references is 512,512 (1024,1024), bottom right of map images is e.g. for zoom 3 2048,2048 (4096,4096). What update() does is track current
// plane lat/lon on zoom level map image then calculate viewport centred on plane and refresh display. In addition user can
// show/hide plane and cycle through zoom levels via html button, joystick button, or voice command if P3D configured correctly.
// If plane is toggled not visible the map stops updating automatically relative to the plane instead mouse drag is enabled.
function update(timestamp)
{
	checkTogglePlane();                // Check whether user has selected to toggle plane via joystick button or voice command
	checkCycleZoom();                  // Check whether user has elected to cycle zoom via joystick button or voice command
	if (!dragEnabled){                 // Plane is visible and map is moved to keep plane centred in viewport
		refreshMapNewPlanePosition();  // Re-center map based on new plane position and show new plane direction (and position if near edge of map image)
	} else if (dragEnabled){           // Plane is hidden and map is only moved in response to mouse drag
                                       // See refreshMapMouseDrag, doMouseDown, doMouseMove, doMouseUp and addEventListeners
		refeshMapImages();			   // Refresh map images if leg number has changed	
	}

	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);

function refreshMapNewPlanePosition(){
	var legNo = refeshMapImages();                                         // Check whether user has reached next leg start point
	var planeTopPixels = getTopPixels(legNo);                              // Number of pixels from top of map image to plane centre based on lat/lon calcs
	var planeLeftPixels = getLeftPixels(legNo);                            // Number of pixels from left edge of map image to plane centre based on lat/lon calcs
	clipTop = validateClipTop(planeTopPixels - (viewPortHeight / 2));      // Pixel reference of top of viewport in map image
	clipRight = validateClipRight(planeLeftPixels + (viewPortWidth / 2));  // Pixel reference to righthand edge of viewport in map image
	refreshClip(clipTop, clipRight);                                       // Adjust viewport in map image to reflect any plane movement
	refreshPlane(planeTopPixels, planeLeftPixels, clipTop, clipRight);     // Refresh plane position and heading in viewport
}

function refreshMapMouseDrag(){
	clipTop = validateClipTop(clipTop - dragYamount);        // Pixel reference of top of viewport in map image
	clipRight = validateClipRight(clipRight - dragXamount);  // Pixel reference to righthand edge of viewport in map image
	refreshClip(clipTop, clipRight);                         // Adjust viewport in map image to reflect any mouse drag movement
}

document.addEventListener("mousedown", doMouseDown);

function doMouseDown(event){
	if (dragEnabled == 1){           // Occurs when plane visibility is toggled off
  		dragStartX = event.clientX;  // Store x corordinate when mouse down event occurs
		dragStartY = event.clientY;  // Store y corordinate when mouse down event occurs
		dragHappening = 1;           // So that mouse move event triggers update of map position for mouse drag
	}
}

document.addEventListener("mousemove", doMouseMove);

function doMouseMove(event){
	if (dragHappening == 1){
		dragXamount = event.clientX - dragStartX;  // How far has the mouse moved in x coordinate since last mouse move event
		dragYamount = event.clientY - dragStartY;  // How far has the mouse moved in y coordinate since last mouse move event
		dragStartX = event.clientX;
		dragStartY = event.clientY;
		refreshMapMouseDrag();
	}
}

document.addEventListener("mouseup", doMouseUp);

function doMouseUp(event){
	if (dragEnabled == 1){
		dragXamount = event.clientX - dragStartX;
		dragYamount = event.clientY - dragStartY;
		refreshMapMouseDrag();
		dragHappening = 0;  // Stops mouse move events updating position of map
	}
}

function getTopPixels(legNo){
	var planeLatDeg = VarGet("A:PLANE LATITUDE" ,"Radians") * 180 / Math.PI;  // y
	var planeTopPixels = Math.round((planeLatDeg - mapNorth[legNo]) / (mapSouth[legNo] - mapNorth[legNo]) * imagePixels[zoomFactor]);
	return planeTopPixels;
}

function getLeftPixels(legNo){
	var planeLonDeg = VarGet("A:PLANE LONGITUDE" ,"Radians") * 180 / Math.PI; // x
	var planeLeftPixels = Math.round((planeLonDeg - mapWest[legNo]) / (mapEast[legNo] - mapWest[legNo]) * imagePixels[zoomFactor]);
	return planeLeftPixels;
}

// Clip top pixel reference is usually half the viewport above the plane position, but special cases
// are 1) it can't be less than pixel 0 and 2) it can't be more than image height minus viewport height
function validateClipTop(clipTop){
	if (clipTop < 0) {
		clipTop = 0;
	} else if (clipTop > (imagePixels[zoomFactor] - viewPortHeight)) {
		clipTop = (imagePixels[zoomFactor] - viewPortHeight);
	}
	return clipTop;
}

// Clip right pixel reference is usually half the viewport to the right of plane position, but special cases
// are 1) it can't be less than viewport width and 2) it can't be more than image width 
function validateClipRight(clipRight){
	if (clipRight > imagePixels[zoomFactor]) {
		clipRight = imagePixels[zoomFactor];
	} else if (clipRight < viewPortWidth) {
		clipRight = viewPortWidth;
	}
	return clipRight;
}

function refreshClip(clipTop, clipRight){
	var clipBottom = clipTop + viewPortHeight;
	var clipLeft = clipRight - viewPortWidth;
	var element = document.getElementById('roadMapZoom' + zoomFactor + 'Img');
	element.style.top = '-' + clipTop + 'px'; // Where the map image top is relative to the viewport top edge
	element.style.left = '-' + clipLeft + 'px'; // Where the map image lefthand edge is relative to the viewport lefthand edge
	element.style.position = 'absolute';
	// Set where viewport is in current map image, clip values are relative to top left 1,1 coordinate
	element.style.clip = 'rect(' + clipTop + 'px,' + clipRight + 'px,' + clipBottom + 'px,' + clipLeft + 'px)';
}

function refreshPlane(planeTopPixels, planeLeftPixels, clipTop, clipRight) {
	var planeHeadingDeg = VarGet("A:PLANE HEADING DEGREES TRUE" ,"Radians") * 180 / Math.PI;
	var plane = document.getElementById("plane");
	var clipLeft = clipRight - viewPortWidth;
	plane.style.top = planeTopPixels - clipTop - 15 + "px";		// Plane top pixel reference in viewport coordinate system
	plane.style.left = planeLeftPixels - clipLeft - 15 + "px";	// Plane left pixel reference in viewport coordinate system
	plane.style.transform = "rotate(" + planeHeadingDeg + "deg)";
}

function refeshMapImages() {
	legNo = VarGet("S:currentLegNo" ,"NUMBER");
	if (legNo != oldLegNo) {
		oldLegNo = legNo;
		legNoStr = String(legNo).padStart(2, '0'); // Assuming <= 99 legs
		document.getElementById('roadMapZoom1Img').src='LegRoute_' + legNoStr + '_zoom' + zoom1FilenameSuffixX + '.jpg';
		document.getElementById('roadMapZoom2Img').src='LegRoute_' + legNoStr + '_zoom' + zoom2FilenameSuffixX + '.jpg';
		document.getElementById('roadMapZoom3Img').src='LegRoute_' + legNoStr + '_zoom' + zoom3FilenameSuffixX + '.jpg';
		if (zoomFactor == 1){
			showZoom1Map()
		} else if (zoomFactor == 2){
			showZoom2Map()
		} else{
			showZoom3Map()
		}
	}
	return legNo;
}

function togglePlaneButton() {
	if (document.getElementById('plane').style.display == 'none'){
		document.getElementById('plane').style.display = 'inline';
		dragEnabled = 0;
	} else{
		document.getElementById('plane').style.display = 'none';
		dragEnabled = 1;
	}
}

function checkTogglePlane() {
	var changePlane = VarGet("A:CABIN NO SMOKING ALERT SWITCH" ,"Bool"); 
	if (changePlane != planeToggle){
		planeToggle = changePlane;
		togglePlaneButton()
	}
}

function cycleZoomButton() {
	if (zoomFactor == 1){
		showZoom2Map()
	} else if (zoomFactor == 2){
		showZoom3Map()
	} else{
		showZoom1Map()
	}
}

function checkCycleZoom() {
	var changeZoom = VarGet("A:ALTERNATE STATIC SOURCE OPEN" ,"Bool"); 
	if (changeZoom != zoomCycle){
		zoomCycle = changeZoom;
		cycleZoomButton()
	}
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