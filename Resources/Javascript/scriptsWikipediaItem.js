// Constants populated by P3D Scenario Generator application (0 array entry so first item is referenced as index 1)
// The X variables will be replaced with [item, item]
var itemURLsX = null;
var itemHREFsX = null;

// Use spread operator to merge the arrays
const itemURLs = ["", ...itemURLsX];
const itemHREFs = [[""], ...itemHREFsX];

var oldLegNo = 0;			// Tracks scenario variable that indicates current leg number
var hrefToggle = 0;         // Tracks state of a simulator variable toggle to know when to advance display of Wikipedia item to next href
var curHREF = 0;

function update(timestamp)
{
	refreshLegURL();		// Check whether user has reached next leg start point
	window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);

function refreshLegURL() {
    legNo = VarGet("S:currentLegNo", "NUMBER");
    // Get the object element once
    const objectElement = document.getElementById("item-object");

    // Safety check to ensure the element exists
    if (!objectElement) return;

    const newURLBase = itemURLs[legNo - 1];

    // --- 1. Handle Leg Change (Primary URL Load) ---
    if (legNo != oldLegNo) {
        oldLegNo = legNo;
        // Reset the section index for the new page
        curHREF = 0;

        // Only update the data property to load the new base URL
        objectElement.data = newURLBase;
    }

    // --- 2. Handle HREF Toggle (Section Scroll) ---
    var changeHREF = VarGet("A:CABIN SEATBELTS ALERT SWITCH", "Bool");
    if (changeHREF != hrefToggle) {
        hrefToggle = changeHREF;

        // Construct the full URL with the new section hash
        const fullURL = newURLBase + "#" + itemHREFs[legNo - 1][curHREF];

        // Only update the data property to change the hash (hoping for a scroll)
        objectElement.data = fullURL;

        // Advance the section index
        curHREF = curHREF + 1;
        if (curHREF >= itemHREFs[legNo - 1].length) {
            curHREF = 0;
        }
    }
}