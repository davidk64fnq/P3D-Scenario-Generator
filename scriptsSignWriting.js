
	function update(timestamp)
	{
		const mapNorth = mapNorthX; 
		const mapEast = mapEastX;
		const mapSouth = mapSouthX;
		const mapWest = mapWestX;
		const mapWidth = mapWidthX;
		const mapHeight = mapHeightX;
		const messageLength = messageLengthX;
		const magVar = magVarX;
		const padding = 20;
	
		var planeHeadingT = VarGet("A:PLANE HEADING DEGREES MAGNETIC" ,"Radians") * 180 / Math.PI + magVar;
		var planeLonDeg = VarGet("A:PLANE LONGITUDE" ,"Radians") * 180 / Math.PI; // x
		var planeLatDeg = VarGet("A:PLANE LATITUDE" ,"Radians") * 180 / Math.PI;  // y
		var messageHeight = mapHeight - 2 * padding;
		var messageWidth = 0.75 * messageHeight * messageLength;
		var planeTopPixels = Math.round((planeLatDeg - mapNorth) / (mapSouth - mapNorth) * messageHeight);
		var planeLeftPixels = Math.round((planeLonDeg - mapWest) / (mapEast - mapWest) * messageWidth);
		var smokeOn = VarGet("S:smokeOn", "NUMBER"); 
		var canvas = document.getElementById('canvas');
		if (smokeOn == 1) {
			if ((planeHeadingT >= 337.5 && planeHeadingT < 22.5) || (planeHeadingT >= 157.5 && planeHeadingT < 202.5))
				drawLineNS(planeLeftPixels + padding, planeTopPixels + padding);
			else if ((planeHeadingT >= 22.5 && planeHeadingT < 67.5) || (planeHeadingT >= 202.5 && planeHeadingT < 247.5))
				drawLineNESW(planeLeftPixels + padding, planeTopPixels + padding);
			else if ((planeHeadingT >= 67.5 && planeHeadingT < 112.5) || (planeHeadingT >= 247.5 && planeHeadingT < 292.5))
				drawLineEW(planeLeftPixels + padding, planeTopPixels + padding);
			else if ((planeHeadingT >= 112.5 && planeHeadingT < 157.5) || (planeHeadingT >= 292.5 && planeHeadingT < 337.5))
				drawLineSENW(planeLeftPixels + padding, planeTopPixels + padding);
		}
		window.requestAnimationFrame(update);
	}
	function drawLineNS(left, top)
	{
		var canvas = document.getElementById('canvas');
		if (canvas.getContext) {
			var context = canvas.getContext('2d');
			context.fillRect(left - 5, top, 1, 1);
			context.fillRect(left - 4, top, 1, 1);
			context.fillStyle = "red";
			context.fillRect(left - 3, top, 1, 1);
			context.fillRect(left - 2, top, 1, 1);
			context.fillRect(left - 1, top, 1, 1);
			context.fillRect(left, top, 1, 1);
			context.fillRect(left + 1, top, 1, 1);
			context.fillRect(left + 2, top, 1, 1);
			context.fillRect(left + 3, top, 1, 1);
			context.fillStyle = "black";
			context.fillRect(left + 4, top, 1, 1);
			context.fillRect(left + 5, top, 1, 1);
		}
	}
	function drawLineNESW(left, top)
	{
		var canvas = document.getElementById('canvas');
		if (canvas.getContext) {
			var context = canvas.getContext('2d');
			context.fillRect(left + 5, top - 5, 1, 1);
			context.fillRect(left + 4, top - 4, 1, 1);
			context.fillStyle = "red";
			context.fillRect(left + 3, top - 3, 1, 1);
			context.fillRect(left + 2, top - 2, 1, 1);
			context.fillRect(left + 1, top - 1, 1, 1);
			context.fillRect(left, top, 1, 1);
			context.fillRect(left - 1, top + 1, 1, 1);
			context.fillRect(left - 2, top + 2, 1, 1);
			context.fillRect(left - 3, top + 3, 1, 1);
			context.fillStyle = "black";
			context.fillRect(left - 4, top + 4, 1, 1);
			context.fillRect(left - 5, top + 5, 1, 1);
		}
	}
	function drawLineEW(left, top)
	{
		var canvas = document.getElementById('canvas');
		if (canvas.getContext) {
			var context = canvas.getContext('2d');
			context.fillRect(left, top - 5, 1, 1);
			context.fillRect(left, top - 4, 1, 1);
			context.fillStyle = "red";
			context.fillRect(left, top - 3, 1, 1);
			context.fillRect(left, top - 2, 1, 1);
			context.fillRect(left, top - 1, 1, 1);
			context.fillRect(left, top, 1, 1);
			context.fillRect(left, top + 1, 1, 1);
			context.fillRect(left, top + 2, 1, 1);
			context.fillRect(left, top + 3, 1, 1);
			context.fillStyle = "black";
			context.fillRect(left, top + 4, 1, 1);
			context.fillRect(left, top + 5, 1, 1);
		}
	}
	function drawLineSENW(left, top)
	{
		var canvas = document.getElementById('canvas');
		if (canvas.getContext) {
			var context = canvas.getContext('2d');
			context.fillRect(left - 5, top + 5, 1, 1);
			context.fillRect(left - 4, top + 4, 1, 1);
			context.fillStyle = "red";
			context.fillRect(left - 3, top + 3, 1, 1);
			context.fillRect(left - 2, top + 2, 1, 1);
			context.fillRect(left - 1, top + 1, 1, 1);
			context.fillRect(left, top, 1, 1);
			context.fillRect(left + 1, top - 1, 1, 1);
			context.fillRect(left + 2, top - 2, 1, 1);
			context.fillRect(left + 3, top - 3, 1, 1);
			context.fillStyle = "black";
			context.fillRect(left + 4, top - 4, 1, 1);
			context.fillRect(left + 5, top - 5, 1, 1);
		}
	}
	window.requestAnimationFrame(update);