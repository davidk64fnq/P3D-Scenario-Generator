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

// Leg Waypoints Array injected by C# (Array of arrays containing {lat, lon})
// Example: [ [{lat: 51.1, lon: -0.2}, {lat: 51.4, lon: -0.1}], ... ]
var legCoordsX = null;

// The 0 entry ensures first leg starts at index 1
const mapNorth = [0, ...(mapNorthX || [0])];
const mapEast = [0, ...(mapEastX || [0])];
const mapSouth = [0, ...(mapSouthX || [0])];
const mapWest = [0, ...(mapWestX || [0])];
const imagePixels = [0, ...(imagePixelsX || [0, 2048, 4096, 8192])];
const legCoords = [null, ...(legCoordsX || [])];

const viewPortWidth = viewPortWidthX || 512;
const viewPortHeight = viewPortHeightX || 512;

var zoomFactor = 1;     // Takes values from 1 to 3
var zoomCycle = 0;      
var planeToggle = 0;    
var oldLegNo = 0;       
var dragEnabled = 0;    
var dragHappening = 0;  
var clipTop = 0;        
var clipRight = 0;      
var dragStartX = 0;     
var dragStartY = 0;     
var dragXamount = 0;    
var dragYamount = 0;    

// Display toggles
var showRouteLine = 1;
var showWaypoints = 1;
var showDirectTo = 1;
var showHud = 1;

var overlayCanvas = null;
var overlayCtx = null;

window.addEventListener("DOMContentLoaded", () => {
    overlayCanvas = document.getElementById("overlayCanvas");
    if (overlayCanvas) {
        overlayCanvas.width = viewPortWidth;
        overlayCanvas.height = viewPortHeight;
        overlayCtx = overlayCanvas.getContext("2d");
    }
});

function safeVarGet(name, unit) {
    if (typeof VarGet === "function") {
        return VarGet(name, unit);
    }
    return 0;
}

function update(timestamp) {
    checkTogglePlane();
    checkCycleZoom();

    if (!dragEnabled) {
        refreshMapNewPlanePosition();
    } else {
        refeshMapImages();
    }

    window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);

function refreshMapNewPlanePosition() {
    var legNo = refeshMapImages();
    var planeTopPixels = getTopPixels(legNo);
    var planeLeftPixels = getLeftPixels(legNo);

    clipTop = validateClipTop(planeTopPixels - (viewPortHeight / 2));
    clipRight = validateClipRight(planeLeftPixels + (viewPortWidth / 2));

    refreshClip(clipTop, clipRight);
    refreshPlane(planeTopPixels, planeLeftPixels, clipTop, clipRight);

    // Render Canvas Overlay & Nav HUD
    renderOverlay(legNo, clipTop, clipRight);
}

function refreshMapMouseDrag() {
    clipTop = validateClipTop(clipTop - dragYamount);
    clipRight = validateClipRight(clipRight - dragXamount);
    refreshClip(clipTop, clipRight);
    
    var legNo = oldLegNo || 1;
    renderOverlay(legNo, clipTop, clipRight);
}

// Coordinate conversions
function getTopPixels(legNo) {
    var planeLatDeg = safeVarGet("A:PLANE LATITUDE", "Radians") * 180 / Math.PI;
    return Math.round((planeLatDeg - mapNorth[legNo]) / (mapSouth[legNo] - mapNorth[legNo]) * imagePixels[zoomFactor]);
}

function getLeftPixels(legNo) {
    var planeLonDeg = safeVarGet("A:PLANE LONGITUDE", "Radians") * 180 / Math.PI;
    return Math.round((planeLonDeg - mapWest[legNo]) / (mapEast[legNo] - mapWest[legNo]) * imagePixels[zoomFactor]);
}

function latLonToViewport(latDeg, lonDeg, legNo, topClip, rightClip) {
    var totalTop = Math.round((latDeg - mapNorth[legNo]) / (mapSouth[legNo] - mapNorth[legNo]) * imagePixels[zoomFactor]);
    var totalLeft = Math.round((lonDeg - mapWest[legNo]) / (mapEast[legNo] - mapWest[legNo]) * imagePixels[zoomFactor]);
    var clipLeft = rightClip - viewPortWidth;

    return {
        x: totalLeft - clipLeft,
        y: totalTop - topClip
    };
}

// Navigation Math
function calcDistanceNM(lat1, lon1, lat2, lon2) {
    var R = 3440.065; // Earth radius in Nautical Miles
    var phi1 = lat1 * Math.PI / 180;
    var phi2 = lat2 * Math.PI / 180;
    var deltaPhi = (lat2 - lat1) * Math.PI / 180;
    var deltaLambda = (lon2 - lon1) * Math.PI / 180;

    var a = Math.sin(deltaPhi / 2) * Math.sin(deltaPhi / 2) +
            Math.cos(phi1) * Math.cos(phi2) *
            Math.sin(deltaLambda / 2) * Math.sin(deltaLambda / 2);
    var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return R * c;
}

function calcBearingMag(lat1, lon1, lat2, lon2, magVarDeg) {
    var phi1 = lat1 * Math.PI / 180;
    var phi2 = lat2 * Math.PI / 180;
    var deltaLambda = (lon2 - lon1) * Math.PI / 180;

    var y = Math.sin(deltaLambda) * Math.cos(phi2);
    var x = Math.cos(phi1) * Math.sin(phi2) - Math.sin(phi1) * Math.cos(phi2) * Math.cos(deltaLambda);
    var brgTrue = (Math.atan2(y, x) * 180 / Math.PI + 360) % 360;

    // Apply MagVar (East positive, West negative)
    return ((brgTrue - magVarDeg) + 360) % 360;
}

// Canvas Overlay & HUD Renderer
function renderOverlay(legNo, topClip, rightClip) {
    if (!overlayCtx) return;

    overlayCtx.clearRect(0, 0, viewPortWidth, viewPortHeight);

    var waypoints = (legCoords && legCoords[legNo]) ? legCoords[legNo] : null;
    if (!waypoints || waypoints.length < 2) return;

    var startWpt = waypoints[0];
    var destWpt = waypoints[waypoints.length - 1];

    var planeLat = safeVarGet("A:PLANE LATITUDE", "Radians") * 180 / Math.PI;
    var planeLon = safeVarGet("A:PLANE LONGITUDE", "Radians") * 180 / Math.PI;
    var magVar = safeVarGet("A:MAGVAR", "Degrees");

    // 1. Draw Direct Route Line (Start to Finish)
    if (showRouteLine) {
        overlayCtx.beginPath();
        overlayCtx.strokeStyle = "rgba(0, 102, 255, 0.75)";
        overlayCtx.lineWidth = 3;

        for (var i = 0; i < waypoints.length; i++) {
            var pt = latLonToViewport(waypoints[i].lat, waypoints[i].lon, legNo, topClip, rightClip);
            if (i === 0) overlayCtx.moveTo(pt.x, pt.y);
            else overlayCtx.lineTo(pt.x, pt.y);
        }
        overlayCtx.stroke();
    }

    // 2. Draw Start and Destination Markers
    if (showWaypoints) {
        // Start: Green Ring
        var startPt = latLonToViewport(startWpt.lat, startWpt.lon, legNo, topClip, rightClip);
        overlayCtx.beginPath();
        overlayCtx.arc(startPt.x, startPt.y, 6, 0, 2 * Math.PI);
        overlayCtx.strokeStyle = "#16a34a";
        overlayCtx.lineWidth = 2.5;
        overlayCtx.stroke();

        // Destination: Red Bullseye
        var destPt = latLonToViewport(destWpt.lat, destWpt.lon, legNo, topClip, rightClip);
        overlayCtx.beginPath();
        overlayCtx.arc(destPt.x, destPt.y, 7, 0, 2 * Math.PI);
        overlayCtx.strokeStyle = "#dc2626";
        overlayCtx.lineWidth = 2.5;
        overlayCtx.stroke();

        overlayCtx.beginPath();
        overlayCtx.arc(destPt.x, destPt.y, 2, 0, 2 * Math.PI);
        overlayCtx.fillStyle = "#dc2626";
        overlayCtx.fill();
    }

    // 3. Draw "Direct-To" Line (Plane to Destination)
    if (showDirectTo && !dragEnabled) {
        var planePos = latLonToViewport(planeLat, planeLon, legNo, topClip, rightClip);
        var destPos = latLonToViewport(destWpt.lat, destWpt.lon, legNo, topClip, rightClip);

        overlayCtx.beginPath();
        overlayCtx.setLineDash([4, 4]);
        overlayCtx.strokeStyle = "rgba(220, 38, 38, 0.8)";
        overlayCtx.lineWidth = 1.5;
        overlayCtx.moveTo(planePos.x, planePos.y);
        overlayCtx.lineTo(destPos.x, destPos.y);
        overlayCtx.stroke();
        overlayCtx.setLineDash([]);
    }

    // 4. Update HUD Nav Display
    if (showHud) {
        var legDist = calcDistanceNM(startWpt.lat, startWpt.lon, destWpt.lat, destWpt.lon);
        var legBrg = calcBearingMag(startWpt.lat, startWpt.lon, destWpt.lat, destWpt.lon, magVar);

        var directDist = calcDistanceNM(planeLat, planeLon, destWpt.lat, destWpt.lon);
        var directBrg = calcBearingMag(planeLat, planeLon, destWpt.lat, destWpt.lon, magVar);

        document.getElementById("hudLegTitle").innerText = `LEG ${String(legNo).padStart(2, '0')}`;
        document.getElementById("hudPlanned").innerText = `${String(Math.round(legBrg)).padStart(3, '0')}°M / ${legDist.toFixed(1)} NM`;
        document.getElementById("hudDirect").innerText = `${String(Math.round(directBrg)).padStart(3, '0')}°M / ${directDist.toFixed(1)} NM`;
    }
}

// UI Button Handlers
function toggleRouteLineButton() {
    showRouteLine = showRouteLine ? 0 : 1;
    document.getElementById("routeButton").style.color = showRouteLine ? "#0f172a" : "#94a3b8";
}

function toggleWaypointsButton() {
    showWaypoints = showWaypoints ? 0 : 1;
    document.getElementById("wptButton").style.color = showWaypoints ? "#0f172a" : "#94a3b8";
}

function toggleDirectToButton() {
    showDirectTo = showDirectTo ? 0 : 1;
    document.getElementById("directToButton").style.color = showDirectTo ? "#0f172a" : "#94a3b8";
}

function toggleHudButton() {
    showHud = showHud ? 0 : 1;
    document.getElementById("navHud").style.display = showHud ? "block" : "none";
    document.getElementById("hudButton").style.color = showHud ? "#0f172a" : "#94a3b8";
}

// Mouse events and sim triggers
document.addEventListener("mousedown", (e) => {
    if (dragEnabled == 1) {
        dragStartX = e.clientX;
        dragStartY = e.clientY;
        dragHappening = 1;
    }
});

document.addEventListener("mousemove", (e) => {
    if (dragHappening == 1) {
        dragXamount = e.clientX - dragStartX;
        dragYamount = e.clientY - dragStartY;
        dragStartX = e.clientX;
        dragStartY = e.clientY;
        refreshMapMouseDrag();
    }
});

document.addEventListener("mouseup", () => {
    if (dragEnabled == 1) {
        dragHappening = 0;
    }
});

function validateClipTop(clipTop) {
    if (clipTop < 0) return 0;
    if (clipTop > (imagePixels[zoomFactor] - viewPortHeight)) return imagePixels[zoomFactor] - viewPortHeight;
    return clipTop;
}

function validateClipRight(clipRight) {
    if (clipRight > imagePixels[zoomFactor]) return imagePixels[zoomFactor];
    if (clipRight < viewPortWidth) return viewPortWidth;
    return clipRight;
}

function refreshClip(clipTop, clipRight) {
    var clipBottom = clipTop + viewPortHeight;
    var clipLeft = clipRight - viewPortWidth;
    var element = document.getElementById('roadMapZoom' + zoomFactor + 'Img');
    element.style.top = '-' + clipTop + 'px';
    element.style.left = '-' + clipLeft + 'px';
    element.style.position = 'absolute';
    element.style.clip = 'rect(' + clipTop + 'px,' + clipRight + 'px,' + clipBottom + 'px,' + clipLeft + 'px)';
}

function refreshPlane(planeTopPixels, planeLeftPixels, clipTop, clipRight) {
    var planeHeadingDeg = safeVarGet("A:PLANE HEADING DEGREES TRUE", "Radians") * 180 / Math.PI;
    var plane = document.getElementById("plane");
    var clipLeft = clipRight - viewPortWidth;
    plane.style.top = planeTopPixels - clipTop - 15 + "px";
    plane.style.left = planeLeftPixels - clipLeft - 15 + "px";
    plane.style.transform = "rotate(" + planeHeadingDeg + "deg)";
}

function refeshMapImages() {
    var legNo = safeVarGet("S:currentLegNo", "NUMBER") || 1;
    if (legNo != oldLegNo) {
        oldLegNo = legNo;
        var legNoStr = String(legNo).padStart(2, '0');
        document.getElementById('roadMapZoom1Img').src = 'LegRoute_' + legNoStr + '_zoom' + zoom1FilenameSuffixX + '.jpg';
        document.getElementById('roadMapZoom2Img').src = 'LegRoute_' + legNoStr + '_zoom' + zoom2FilenameSuffixX + '.jpg';
        document.getElementById('roadMapZoom3Img').src = 'LegRoute_' + legNoStr + '_zoom' + zoom3FilenameSuffixX + '.jpg';
    }
    return legNo;
}

var planeVisible = 1;

function togglePlaneButton() {
    var plane = document.getElementById('plane');
    if (!plane) return;

    planeVisible = planeVisible ? 0 : 1;
    plane.style.display = planeVisible ? "block" : "none";
    dragEnabled = planeVisible ? 0 : 1;

    document.getElementById("planeButton").style.color = planeVisible ? "#0f172a" : "#94a3b8";
}

function checkTogglePlane() {
    var changePlane = safeVarGet("A:CABIN NO SMOKING ALERT SWITCH", "Bool"); 
    if (changePlane != planeToggle) {
        planeToggle = changePlane;
        togglePlaneButton();
    }
}
function cycleZoomButton() {
    if (zoomFactor == 1) showZoom2Map();
    else if (zoomFactor == 2) showZoom3Map();
    else showZoom1Map();
}

function checkCycleZoom() {
    var changeZoom = safeVarGet("A:ALTERNATE STATIC SOURCE OPEN", "Bool"); 
    if (changeZoom != zoomCycle) {
        zoomCycle = changeZoom;
        cycleZoomButton();
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