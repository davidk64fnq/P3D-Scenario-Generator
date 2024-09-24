var oldLegNo = 0;			// Tracks scenario variable that indicates current leg number

function update(timestamp)
{
	refreshLegPhoto();		// Check whether user has reached next leg start point
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);

function refreshLegPhoto() {
	legNo = VarGet("S:currentLegNo" ,"NUMBER");
	if (legNo != oldLegNo) {
		oldLegNo = legNo;
		const element = document.getElementById("content");
		legNoStr = String(legNo).padStart(2, '0'); // Assuming <= 99 legs
		element.src='photo_' + legNoStr + '.jpg';
	}
}