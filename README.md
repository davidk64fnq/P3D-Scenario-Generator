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
2. **Runtime:** None required (the release build is fully self-contained).
3. **Folder Paths (Configured on the Settings tab):**
   * **P3D Install:** Main Prepar3D v5 root folder (e.g., `C:\Program Files\Lockheed Martin\Prepar3D v5`).
   * **P3D Data:** Prepar3D ProgramData folder (e.g., `C:\ProgramData\Lockheed Martin\Prepar3D v5`).
   * **Scenario Folder:** Your target scenario directory (e.g., `C:\Users\<Username>\Documents\Prepar3D v5 Files`).
4. **Map Tile API Key (Required for all scenarios):**
   * All scenarios download OpenStreetMap tiles to render briefing overview charts and in-game moving map displays.
   * A free API key from **RapidAPI** is required:
     1. Sign up for a free account at [RapidAPI](https://rapidapi.com/).
     2. In the top search bar, search for **MapTiles**.
     3. Select any endpoint (e.g., `getStandardMapTile`) and copy your alphanumeric key from the code snippet box (`'x-rapidapi-key: ...'`).
     4. Paste the key into the **Server / API key** field on the **Settings** tab.
   * *(Full step-by-step instructions are available inside the app by clicking the **Help** button on the Settings tab).*
5. **Airports Database:**
   * Includes a stock P3D v5 runway database by default.
   * Add-on scenery runways can be imported using Pete & John Dowson's `MakeRunways` utility (see General Tab Help for details).

---

## 🚀 Quick Start for Testers

To verify your installation and make sure everything is working properly, we recommend creating a **Circuit Scenario** first:

1. Go to the [Releases](https://github.com/davidk64fnq/P3D-Scenario-Generator/releases) page and download `P3D-Scenario-Generator-v1.0.0-beta.zip`.
2. Extract the ZIP folder to any convenient location on your PC.
3. Run `P3D Scenario Generator.exe`.
4. Go to the **Settings** tab:
   * Click **P3D Install**, **P3D Data**, and **Scenario Folder** to select your respective local directories.
   * Paste your **RapidAPI key** into the **Server / API key** field.
5. Go to the **General** tab:
   * Under **Aircraft Selection**, click **Add Aircraft** and select any thumbnail image (`thumbnail.jpg`) inside one of your installed aircraft's `texture` folders.
   * Under **Runway Selection**, click **Random Runway** (or search for a specific ICAO code).
   * Under **Scenario Selection**, select **Circuit** and type a unique name in the **Title** box (e.g., `Test Circuit 1`).
6. *(Optional)* Switch to the **Circuit** tab to see the leg distances, speeds, and pattern altitudes automatically calculated for your chosen aircraft (see Circuit Tab Help for details).
7. Click **Generate Scenario** at the bottom of the window.
8. Launch Prepar3D, select **Scenarios**, load your generated flight, and confirm that the spatial gates, briefing charts, takeoff triggers, and landing detection work as expected!

---

## 📋 Help & Documentation

Comprehensive in-app documentation covering each tab, parameters, window alignment options, and in-flight procedures can be accessed at any time by clicking the **Help** button located in the upper right corner of the application window.

---

## 🐛 Feedback & Issue Reporting

Please submit bug reports, log files (`%APPDATA%\P3D Scenario Generator\ErrorLog.txt`), or suggestions using the [GitHub Issues tab](https://github.com/davidk64fnq/P3D-Scenario-Generator/issues).
