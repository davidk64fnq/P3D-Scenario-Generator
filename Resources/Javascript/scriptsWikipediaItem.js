// Constants populated by P3D Scenario Generator application (0 array entry so first item is referenced as index 1)
// The X variables will be replaced with [item, item]
var itemURLsX = null;

// Use spread operator to merge the arrays
const itemURLs = ["", ...itemURLsX];

var oldLegNo = 0;			// Tracks scenario variable that indicates current leg number

// Automatically converts desktop Wikipedia links to forced Mobile Wikipedia links
function formatMobileURL(url) {
    if (!url) return "";
    var cleanURL = url.split('#')[0];
    
    var mobileURL = cleanURL.replace("https://en.wikipedia.org", "https://en.m.wikipedia.org");
    var separator = mobileURL.indexOf('?') !== -1 ? '&' : '?';
    
    // Forces MediaWiki mobile skin on desktop User-Agents
    return mobileURL + separator + "useformat=mobile";
}

function update(timestamp) {
    refreshLegURL();
    window.requestAnimationFrame(update);
}
window.requestAnimationFrame(update);

function refreshLegURL() {
    var legNo = VarGet("S:currentLegNo", "NUMBER");
    const frameElement = document.getElementById("item-iframe");

    if (!frameElement) return;

    const originalURL = itemURLs[legNo - 1];

    if (!originalURL) return;

    // --- Handle Leg Change Only ---
    if (legNo != oldLegNo) {
        oldLegNo = legNo;
        
        // Load the forced mobile version
        frameElement.src = formatMobileURL(originalURL);
    }
}