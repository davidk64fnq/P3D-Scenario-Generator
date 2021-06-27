function update(timestamp)
{
	var planeHeadingDeg = VarGet("A:PLANE HEADING DEGREES MAGNETIC" ,"Radians") * 180 / Math.PI;
	var planeLonDeg = VarGet("A:PLANE LONGITUDE" ,"Radians") * 180 / Math.PI; // x
	var planeLatDeg = VarGet("A:PLANE LATITUDE" ,"Radians") * 180 / Math.PI;  // y
	var onSmoke = VarGet("S:onSmoke");  
	document.getElementById('onSmoke')="onSmoke = " + onSmoke;
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);
