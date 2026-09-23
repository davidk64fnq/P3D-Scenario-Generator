# P3D Scenario Generator

An automated procedural mission and scenario generator for **Lockheed Martin Prepar3D v5**.

Instead of authoring complex mission logic trees manually inside SimDirector, **P3D Scenario Generator** generates ready-to-fly flight scenarios complete with objectives, 3D spatial triggers, custom audio, in-game moving map panels, and interactive HTML5 overlays.

---

## ✈️ Scenario Types

* **Circuit Training:** Automated 8-gate rectangular traffic patterns calculated from aircraft cruise speed and climb performance.
* **Photo Tours:** Low-level visual navigation flights ("IFR – I Follow Roads") visiting real-world geolocated photo waypoints sourced from Pic2Map.
* **Sign Writing:** Precision aerobatic skywriting missions using a 22-segment font grid, complete with real-time smoke activation and an in-cockpit HUD telemetry canvas.
* **Celestial Navigation:** Long-range astronomical navigation exercises featuring true star catalog calculations, an interactive sextant view, sight reduction worksheets, and automated "Cocked Hat" plotting.
* **Wikipedia List Tours:** Visual landmark discovery flights along routes curated from Wikipedia sortable tables (castles, lighthouses, historic sites), complete with an in-sim mobile encyclopedia viewer.

---

## 🛠️ Prerequisites & Setup

1. **Simulator:** Lockheed Martin **Prepar3D v5** (v5.4 recommended).
2. **Runtime:** [.NET Desktop Runtime](https://dotnet.microsoft.com/download) (if running the standalone build).
3. **Map Tile API Key (Required for Moving Maps & Tours):**
   * Generated scenarios download and stitch OpenStreetMap tiles for in-game briefings and moving map displays.
   * A free API key from **RapidAPI** is required:
     1. Sign up for a free account at [RapidAPI](https://rapidapi.com/).
     2. In the top search bar, search for **MapTiles**.
     3. Select any endpoint (e.g., `getStandardMapTile`) and copy your alphanumeric key from the code snippet box (`'x-rapidapi-key: ...'`).
     4. Open **P3D Scenario Generator**, switch to the **Settings** tab, and paste the key into the **Server / API key** field.
   * *(Full step-by-step instructions with tips are also available inside the app under **Settings Tab Help**).*
4. **Airports Database:**
   * Includes a stock P3D v5 runway database by default.
   * Add-on scenery runways can be imported using Pete & John Dowson's `MakeRunways` utility.

---

## 🚀 Quick Start for Testers

1. Go to the [Releases](https://github.com/davidk64fnq/P3D-Scenario-Generator/releases) page and download the latest `P3D-Scenario-Generator-v1.0.0.zip`.
2. Extract the ZIP folder to any convenient location on your PC.
3. Run `P3D Scenario Generator.exe`.
4. Navigate to the **Settings** tab:
   * Verify your **P3D Install**, **P3D Data**, and **Scenario Folder** paths are correctly detected.
   * Enter your RapidAPI key for map tile downloading.
5. Go to the **General** tab:
   * Click **Add Aircraft** to select one of your installed aircraft variants (select any thumbnail in its `texture` folder).
   * Pick a scenario type, give your scenario a unique **Title**, and click **Generate Scenario**.
6. Launch Prepar3D, go to **Scenarios**, load your generated flight, and test!

---

## 📋 Help & Documentation

Full in-depth documentation covering math models, workflow guides, and in-game controls is available in the application via the **Help** button on each tab.

---

## 🐛 Feedback & Issue Reporting

Please submit bug reports, log files (`%APPDATA%\P3D Scenario Generator\ErrorLog.txt`), or suggestions using the [GitHub Issues tab](https://github.com/davidk64fnq/P3D-Scenario-Generator/issues).
