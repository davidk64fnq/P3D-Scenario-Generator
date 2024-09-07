// Constants populated by P3D Scenario Generator application
itemURLs = ["", "https://en.wikipedia.org/wiki/Ringley_Old_Bridge", "https://en.wikipedia.org/wiki/Smithills_Hall"];

var oldLegNo = 0;
function update(timestamp)
{
	var newLegNo = VarGet("S:currentLegNo" ,"NUMBER");
	if (newLegNo != oldLegNo) {
		oldLegNo = newLegNo;
		document.getElementById("content").innerHTML='<object type="text/html" data="' + itemURLs[newLegNo] + '" height="950px" width="1000px"></object>';
	}
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);