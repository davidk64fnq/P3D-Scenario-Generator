# P3D Scenario Generator

An automated procedural mission and scenario generator for **Lockheed Martin Prepar3D v5**.

Instead of authoring complex mission logic trees manually inside SimDirector, **P3D Scenario Generator** generates ready-to-fly flight scenarios complete with objectives, 3D spatial triggers, custom audio, in-game moving map panels, and interactive HTML5 overlays.

---

## 📥 How to Download (First Time Users)

> ⚠️ **Important:** Do **NOT** click the green **"<> Code"** button at the top of this page. That only downloads the raw, unbuilt source code.

1. Click here: **[Download the Latest Release](https://github.com/davidk64fnq/P3D-Scenario-Generator/releases/latest)**
2. Under the **Assets** header at the bottom of the release notes, click the `.zip` file (e.g. `P3D-Scenario-Generator-vX.X.X.zip`) to download it.
3. Once downloaded, **right-click the ZIP file > Extract All** into a regular folder on your PC (such as `C:\P3D Scenario Generator` or your Desktop).
4. Open the extracted folder and double-click **`P3D Scenario Generator.exe`** to start.

---

## ✈️ Scenario Types

* **Circuit Training:** Automated 8-gate rectangular traffic patterns calculated from aircraft cruise speed and climb performance.
* **Photo Tours:** Low-level visual navigation flights ("IFR – I Follow Roads") visiting real-world geolocated photo waypoints sourced from Pic2Map, complete with local landmark captions.
* **Sign Writing:** Precision aerobatic skywriting missions using a 22-segment font grid, complete with real-time smoke activation, in-cockpit HUD telemetry canvas, and return-to-base compass guidance.
* **Celestial Navigation:** Long-range astronomical navigation exercises featuring true star catalog calculations, an interactive sextant view, sight reduction worksheets, and automated "Cocked Hat" plotting.
* **Wikipedia List Tours:** Visual landmark discovery flights along routes curated from Wikipedia sortable tables (castles, lighthouses, historic sites), complete with an in-sim mobile encyclopedia viewer.

---

## 🛠️ Prerequisites & Setup

1. **Simulator:** Lockheed Martin **Prepar3D v5** (v5.4 recommended).
2. **Runtime:** None required. (.NET 8 Desktop Runtime is fully self-contained and bundled inside the application folder, so no separate .NET installation is needed).
3. **Folder Paths (Configured on the Settings tab):**
   * **P3D Install:** Main Prepar3D v5 root folder (e.g., `C:\Program Files\Lockheed Martin\Prepar3D v5`).
   * **P3D Data:** Prepar3D ProgramData folder (e.g., `C:\ProgramData\Lockheed Martin\Prepar3D v5`).
   * **Scenario Folder:** Your target scenario directory (e.g., `C:\Users\<Username>\Documents\Prepar3D v5 Files`).
4. **Map Tile API Key (Required for all scenarios):**
   * Briefing overview charts and in-game moving map displays require a free RapidAPI key to retrieve OpenStreetMap tiles.
   * **Setup:** Step-by-step registration instructions with screenshots are provided directly inside the app. Open **P3D Scenario Generator**, switch to the **Settings** tab, and click the **Help** button in the top-right corner.
5. **Airports Database:**
   * Includes a stock P3D v5 runway database by default.
   * Add-on scenery runways can be imported using Pete & John Dowson's `MakeRunways` utility (see General Tab Help for details).

---

## 🚀 Quick Start for New Testers

To verify your installation and make sure everything is working properly, we recommend creating a **Circuit Scenario** first:

1. Download and extract the app following the instructions in the [Download section](#-how-to-download-first-time-users) above.
2. Run `P3D Scenario Generator.exe`.
3. Go to the **Settings** tab:
   * Click **P3D Install**, **P3D Data**, and **Scenario Folder** to select your respective local directories.
   * Paste your **RapidAPI key** into the **Server / API key** field.
4. Go to the **General** tab:
   * Under **Aircraft Selection**, click **Add Aircraft** and select any thumbnail image (`thumbnail.jpg`) inside one of your installed aircraft's `texture` folders.
   * Under **Runway Selection**, click **Random** (or search for a specific ICAO code).
   * Under **Scenario Selection**, select **Circuit** and type a unique name in the **Title** box (e.g., `Test Circuit 1`).
5. *(Optional)* Switch to the **Circuit** tab to inspect the leg distances, speeds, and pattern altitudes automatically calculated for your chosen aircraft (see Circuit Tab Help for details).
6. Click **Generate Scenario** at the bottom of the window.
7. Launch Prepar3D, select **Scenarios**, load your generated flight, and confirm that the spatial gates, briefing charts, takeoff triggers, and landing detection work as expected!

---

## 🔄 Upgrading from an Earlier Beta

Your saved aircraft profiles, location favourites, and RapidAPI keys are stored safely in your Windows user profile (`%APPDATA%\P3D Scenario Generator\`) and will carry over automatically.

1. Ensure Prepar3D and **P3D Scenario Generator** are both closed.
2. Delete or archive your previous application folder.
3. Download the latest release `.zip` and extract it to your chosen location.
4. Launch `P3D Scenario Generator.exe`.

> **Note on Runway Cache:** If you ever experience issues loading runways after an update, close the app and delete `%APPDATA%\P3D Scenario Generator\runways.cache`. The application will automatically rebuild a fresh binary cache on the next launch.

---

## 📋 Help & Documentation

Comprehensive in-app documentation covering each scenario type, parameter bounds, multi-monitor window alignment options, and in-flight operational procedures can be accessed at any time by clicking the **Help** button located in the upper-right corner of the application window.

---

## 🐛 Feedback & Issue Reporting

Please submit bug reports, log files (`%APPDATA%\P3D Scenario Generator\ErrorLog.txt`), or suggestions using the [GitHub Issues tab](https://github.com/davidk64fnq/P3D-Scenario-Generator/issues).