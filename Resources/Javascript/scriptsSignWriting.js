
	/* 
		Smoke variables refer to use of scenario variable "smokeOn" which is toggled on when first
		gate of a segment is triggered and off when second gate of a segment is triggered. 
	*/
	var smokeOld = 0;
	var smokeHasToggled = false;
	var canvas = document.getElementById('canvas');
	var context = canvas.getContext('2d');
	var planeTopPixels;
	var planeLeftPixels;
	var startTopPixels;
	var startLeftPixels;
	const padding = 20; // Between canvas edges and message characters
	const capExtra = 5; // The number of pixels either side of central row/col of segemnt line
	
	const colours = ["", "red", "black"];
	
	const endCap = [];
	endCap[0] = [0,0,0,0,0,2,0,0,0,0,0];
	endCap[1] = [0,0,0,0,2,2,2,0,0,0,0];
	endCap[2] = [0,0,0,2,2,1,2,2,0,0,0];
	endCap[3] = [0,0,2,2,1,1,1,2,2,0,0];
	endCap[4] = [0,2,2,1,1,1,1,1,2,2,0];
	endCap[5] = [2,2,1,1,1,1,1,1,1,2,2];
	endCap[6] = [0,0,0,0,0,0,0,0,0,0,0];
	endCap[7] = [0,0,0,0,0,0,0,0,0,0,0];
	endCap[8] = [0,0,0,0,0,0,0,0,0,0,0];
	endCap[9] = [0,0,0,0,0,0,0,0,0,0,0];
	endCap[10] = [0,0,0,0,0,0,0,0,0,0,0];
	
	var curCap = [];
	curCap[0] = [];
	curCap[1] = [];
	curCap[2] = [];
	curCap[3] = [];
	curCap[4] = [];
	curCap[5] = [];
	curCap[6] = [];
	curCap[7] = [];
	curCap[8] = [];
	curCap[9] = [];
	curCap[10] = [];
	
	const gateTopPixels = [gateTopPixelsX];
	const gateLeftPixels = [gateLeftPixelsX];
	const gateBearings = [gateBearingsX];
	
	function rotateSquareArray(inArray, outArray, rotation)
	{
		if (rotation == 0)
			for(let row = 0; row < inArray.length; row++)
				for(let col = 0; col < inArray.length; col++)
				{
					outArray[row][col] = inArray[row][col];
				}
		else if (rotation == 90)
			for(let row = 0; row < inArray.length; row++)
				for(let col = 0; col < inArray.length; col++)
				{
					outArray[col][inArray.length - 1 - row] = inArray[row][col];
				}
		else if (rotation == 180)
			for(let row = 0; row < inArray.length; row++)
				for(let col = 0; col < inArray.length; col++)
				{
					outArray[inArray.length - 1 - row][col] = inArray[row][col];
				}
		else if (rotation == 270)
			for(let row = 0; row < inArray.length; row++)
				for(let col = 0; col < inArray.length; col++)
				{
					outArray[inArray.length - 1 - col][row] = inArray[row][col];
				}
	}
	
	function drawCap(top, left, capArray)
	{
		for(let row = 0; row < capArray.length; row++)
		{
			for(let col = 0; col < capArray.length; col++)
			{
				if(capArray[row][col] != 0)
				{
					context.fillStyle = colours[capArray[row][col]];
					context.fillRect(left + col, top + row, 1, 1);
				}
			}
		}
	}
	
	function drawLine(finishGateNo)
	{
		if (gateBearings[finishGateNo] == 0){
			if (planeTopPixels >= gateTopPixels[finishGateNo - 1] - 5)
				planeTopPixels = gateTopPixels[finishGateNo - 1] - 6;
			if (planeTopPixels <= gateTopPixels[finishGateNo] + 5)
				planeTopPixels = gateTopPixels[finishGateNo] + 6;
			var lineHeight = (gateTopPixels[finishGateNo - 1] - 6) - planeTopPixels;
			var lineStartTop = planeTopPixels + padding;
			var lineStartLeftMiddle = gateLeftPixels[finishGateNo] + padding;
			context.fillStyle = colours[2];
			context.fillRect(lineStartLeftMiddle - 5, lineStartTop, 1, lineHeight);
			context.fillRect(lineStartLeftMiddle + 5, lineStartTop, 1, lineHeight);
			context.fillStyle = colours[1];
			context.fillRect(lineStartLeftMiddle - 4, lineStartTop, 9, lineHeight);
		}
		else if (gateBearings[finishGateNo] == 90){
			if (planeLeftPixels <= gateLeftPixels[finishGateNo - 1] + 5)
				planeLeftPixels = gateLeftPixels[finishGateNo - 1] + 6;
			if (planeLeftPixels >= gateLeftPixels[finishGateNo] - 5)
				planeLeftPixels = gateLeftPixels[finishGateNo] - 6;
			var lineLength = planeLeftPixels - (gateLeftPixels[finishGateNo - 1] + 6);
			var lineStartLeft = gateLeftPixels[finishGateNo - 1] + 6 + padding;
			var lineStartTopMiddle = gateTopPixels[finishGateNo] + padding;
			context.fillStyle = colours[2];
			context.fillRect(lineStartLeft, lineStartTopMiddle - 5, lineLength, 1);
			context.fillRect(lineStartLeft, lineStartTopMiddle + 5, lineLength, 1);
			context.fillStyle = colours[1];
			context.fillRect(lineStartLeft, lineStartTopMiddle - 4, lineLength, 9);
		}
		else if (gateBearings[finishGateNo] == 180){
			if (planeTopPixels <= gateTopPixels[finishGateNo - 1] + 5)
				planeTopPixels = gateTopPixels[finishGateNo - 1] + 6;
			if (planeTopPixels >= gateTopPixels[finishGateNo] - 5)
				planeTopPixels = gateTopPixels[finishGateNo] - 6;
			var lineHeight = planeTopPixels - (gateTopPixels[finishGateNo - 1] + 6);
			var lineStartTop = gateTopPixels[finishGateNo - 1] + 6 + padding;
			var lineStartLeftMiddle = gateLeftPixels[finishGateNo] + padding;
			context.fillStyle = colours[2];
			context.fillRect(lineStartLeftMiddle - 5, lineStartTop, 1, lineHeight);
			context.fillRect(lineStartLeftMiddle + 5, lineStartTop, 1, lineHeight);
			context.fillStyle = colours[1];
			context.fillRect(lineStartLeftMiddle - 4, lineStartTop, 9, lineHeight);
		}
		else if (gateBearings[finishGateNo] == 270){
			if (planeLeftPixels >= gateLeftPixels[finishGateNo - 1] - 5)
				planeLeftPixels = gateLeftPixels[finishGateNo - 1] - 6;
			if (planeLeftPixels <= gateLeftPixels[finishGateNo] + 5)
				planeLeftPixels = gateLeftPixels[finishGateNo] + 6;
			var lineLength = (gateLeftPixels[finishGateNo - 1] - 6) - planeLeftPixels;
			var lineStartLeft = planeLeftPixels + padding;
			var lineStartTopMiddle = gateTopPixels[finishGateNo] + padding;
			context.fillStyle = colours[2];
			context.fillRect(lineStartLeft, lineStartTopMiddle - 5, lineLength, 1);
			context.fillRect(lineStartLeft, lineStartTopMiddle + 5, lineLength, 1);
			context.fillStyle = colours[1];
			context.fillRect(lineStartLeft, lineStartTopMiddle - 4, lineLength, 9);
		}
	}

	function setPlaneEndOfLine(currentGateNo){
		if (gateBearings[currentGateNo] == 0){
			planeTopPixels = gateTopPixels[currentGateNo] + 6
		}
		else if (gateBearings[currentGateNo] == 90){
			planeLeftPixels = gateLeftPixels[currentGateNo] - 6
		}
		else if (gateBearings[currentGateNo] == 180){
			planeTopPixels = gateTopPixels[currentGateNo] - 6
		}
		else if (gateBearings[currentGateNo] == 270){
			planeLeftPixels = gateLeftPixels[currentGateNo] + 6
		}
	}
	
	function update(timestamp)
	{
		const mapNorth = mapNorthX; 
		const mapEast = mapEastX;
		const mapSouth = mapSouthX;
		const mapWest = mapWestX;
		const messageLength = messageLengthX;
		const magVar = magVarX;
	
		var currentGateNo = VarGet("S:currentGateNo" ,"NUMBER");
		var planeHeadingT = VarGet("A:PLANE HEADING DEGREES MAGNETIC" ,"Radians") * 180 / Math.PI + magVar;
		if (planeHeadingT > 360)
			planeHeadingT = planeHeadingT - 360;
		if (planeHeadingT < 0)
			planeHeadingT = planeHeadingT + 360;
		var planeLonDeg = VarGet("A:PLANE LONGITUDE" ,"Radians") * 180 / Math.PI; // x
		var planeLatDeg = VarGet("A:PLANE LATITUDE" ,"Radians") * 180 / Math.PI;  // y
		var messageHeight = document.getElementsByTagName("canvas")[0].getAttribute("height") - capExtra * 2 - padding * 2;
		var messageWidth = document.getElementsByTagName("canvas")[0].getAttribute("width") - capExtra * 2 - padding * 2;
		planeTopPixels = Math.round((planeLatDeg - mapNorth) / (mapSouth - mapNorth) * messageHeight);
		planeLeftPixels = Math.round((planeLonDeg - mapWest) / (mapEast - mapWest) * messageWidth);
		var smokeOn = VarGet("S:smokeOn", "NUMBER"); 
		if (smokeOn != smokeOld)
			smokeHasToggled = true;
		smokeOld = smokeOn;
		if (smokeHasToggled && smokeOn == 1) {
			startTopPixels = gateTopPixels[currentGateNo];
			startLeftPixels = gateLeftPixels[currentGateNo];
			rotateSquareArray(endCap, curCap, (gateBearings[currentGateNo] + 180) % 360);
			if (gateBearings[currentGateNo] == 0)
				drawCap(startTopPixels - 11 + padding, startLeftPixels - 5 + padding, curCap);
			else if (gateBearings[currentGateNo] == 90)
				drawCap(startTopPixels - 5 + padding, startLeftPixels + padding, curCap);
			else if (gateBearings[currentGateNo] == 180)
				drawCap(startTopPixels + padding, startLeftPixels - 5 + padding, curCap);
			else if (gateBearings[currentGateNo] == 270)
				drawCap(startTopPixels - 5 + padding, startLeftPixels - 11 + padding, curCap);
			smokeHasToggled = false;
		}
		if (smokeOn == 1) {
			drawLine(currentGateNo + 1);
		}
		if (smokeHasToggled && smokeOn == 0) {
			setPlaneEndOfLine(currentGateNo);	// We may not have been updated yet on plane position when sim turns smoke off
			drawLine(currentGateNo);
			startTopPixels = gateTopPixels[currentGateNo];
			startLeftPixels = gateLeftPixels[currentGateNo];
			rotateSquareArray(endCap, curCap, gateBearings[currentGateNo]);
			if (gateBearings[currentGateNo] == 0)
				drawCap(startTopPixels + padding, startLeftPixels - 5 + padding, curCap);
			else if (gateBearings[currentGateNo] == 90)
				drawCap(startTopPixels - 5 + padding, startLeftPixels - 11 + padding, curCap);
			else if (gateBearings[currentGateNo] == 180)
				drawCap(startTopPixels - 11 + padding, startLeftPixels - 5 + padding, curCap);
			else if (gateBearings[currentGateNo] == 270)
				drawCap(startTopPixels - 5 + padding, startLeftPixels + padding, curCap);
			smokeHasToggled = false;
		}
		window.requestAnimationFrame(update);
	}
	window.requestAnimationFrame(update);