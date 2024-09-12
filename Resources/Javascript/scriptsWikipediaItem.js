// Constants populated by P3D Scenario Generator application
itemURLs = ["", itemURLsX];

var oldLegNo = 0;
function update(timestamp)
{
	var newLegNo = VarGet("S:currentLegNo" ,"NUMBER");
	if (newLegNo != oldLegNo) {
		oldLegNo = newLegNo;
		const element = document.getElementById("content");
		element.innerHTML = '<object type="text/html" data="' + itemURLs[newLegNo] + '" height="950px" width="1000px"></object>';
	}
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);