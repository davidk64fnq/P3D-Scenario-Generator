function update(timestamp)
{
	var planeHeadingDeg = VarGet("A:PLANE HEADING DEGREES MAGNETIC" ,"Radians") * 180 / Math.PI;
	var planeLonDeg = VarGet("A:PLANE LONGITUDE" ,"Radians") * 180 / Math.PI; // x
	var planeLatDeg = VarGet("A:PLANE LATITUDE" ,"Radians") * 180 / Math.PI;  // y
	var smokeOn = VarGet("S:smokeOn", "LONG");  
	document.getElementById("smokeOn").innerHTML = "smokeOn = " + smokeOn;
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);