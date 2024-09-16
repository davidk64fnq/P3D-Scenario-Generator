// Constants populated by P3D Scenario Generator application (0 array entry so first item is referenced as index 1)
itemURLs = ["", itemURLsX];

var oldLegNo = 0;			// Tracks scenario variable that indicates current leg number

function update(timestamp)
{
	refreshLegURL();		// Check whether user has reached next leg start point
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);

function refreshLegURL() {
	legNo = VarGet("S:currentLegNo" ,"NUMBER");
	if (legNo != oldLegNo) {
		oldLegNo = legNo;
		const element = document.getElementById("content");
		element.innerHTML = '<object type="text/html" data="' + itemURLs[legNo] + '" height="950px" width="1000px"></object>';
	}
}