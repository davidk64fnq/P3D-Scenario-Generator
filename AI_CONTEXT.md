# P3D Scenario Generator - Architecture & Domain Context for AI Assistants

---

## 1. Executive Summary & Tech Stack
* **Project:** P3D Scenario Generator
* **Type:** Standalone Desktop Windows Forms (`WinForms`) application (.NET 8/9).
* **Language & Compiler Settings:** C# 12/13, `#nullable enable`, implicit usings enabled, strict compiler analyzers (`Roslynator.Analyzers`, `CS1591` XML documentation enabled).
* **Target Environment:** Generates mission scenarios, flight plans, HTML briefings, and scenery overlays for Lockheed Martin Prepar3D (v5/v6) flight simulator.
* **Architecture Pattern:** Layered Service Architecture with Dependency Injection (DI) registered at startup in `Program.cs`.

---

## 2. Core Functional Modules & Domain Knowledge

The application generates four primary scenario types, configured via UI tabs:

### A. Circuit Training (`/Circuits`)
* Generates realistic airport traffic patterns (Upwind, Crosswind, Downwind, Base, Final).
* Calculates runway thresholds, headings (true vs. magnetic via `MagVar`), circuit altitudes, and spatial turning radii.
* Places 3D waypoint gates and sets simulator landing triggers (`SimMission.AirportLandingTrigger`).
* Backed by `RunwaySearcher`, `RunwayData`, and a 2D spatial `KDNode` tree for instant proximity searches among global airport runways.

### B. Wikipedia Photo Tour (`/PhotoTour`, `/MapTiles`)
* Queries Wikipedia APIs for geo-referenced articles/landmarks within a radius of a starting airport.
* Downloads OpenStreetMap (OSM) satellite/street tiles, tracks daily download quotas, and caches tiles locally in `%AppData%/P3D Scenario Generator/`.
* Montages, pads, and stitches multi-tile composite images with latitude/longitude calibration for interactive moving map displays in HTML/JavaScript.
* Generates P3D scenery objects and Point of Interest (POI) indicators.

### C. Celestial Navigation (`/CelestialScenario`)
* Downloads live/historical nautical almanacs from web sources for a 3-day window centered on the scenario date.
* Parses Greenwich Hour Angle (GHA) for Aries and Sidereal Hour Angle (SHA) / Declination (DEC) for 57 standard navigational stars.
* Updates simulator star catalogs (`stars.dat`) and generates HTML sextant view overlays for manual in-flight position fixing.

### D. Aerial Sign Writing (`/SignWritingScenario`)
* Converts alphanumeric text messages into flight paths using a 22-segment character grid.
* Computes 3D gate coordinates, entrance bearings, altitude offsets, and pitch angles to support vertical or tilted aerial text displays.

---

## 3. Key Services & Infrastructure

| Service | Primary Responsibility | Critical Rules |
| :--- | :--- | :--- |
| `FileOps` | Centralized asynchronous file, directory, and stream I/O. | **Never call `System.IO` static methods directly** if `FileOps` provides the operation. Always check and handle the `bool` or tuple return value. |
| `Logger` | Central diagnostic log file writer. | Diagnostic logging (`InfoAsync`, `WarningAsync`, `ErrorAsync`). Injected into almost all services. |
| `FormProgressReporter` | Thread-safe UI status updates. | Human-friendly messages with prefixes: `INFO:`, `WARNING:`, `ERROR:`. |
| `HttpRoutines` | Web downloads, document scraping, API validation. | Uses injected `HttpClient` and `HtmlAgilityPack.HtmlDocument`. |
| `HtmlParser` | XPath querying and text/attribute extraction. | Internal parser utility. |
| `OSMTileCache` & `CacheMetadataService` | OSM tile disk management and quotas. | Operates in `%AppData%/P3D Scenario Generator/`. Tracks daily download quotas via metadata. |
| `SettingsManager` | User UI form state persistence. | Saves form control states to AppData JSON using a safe write-to-temp-then-atomic-replace pattern (`.tmp` -> `.bak` -> `.json`). |
| `ParsingHelpers` | Degree/minute and numerical string validation. | Validates ranges (0–360° for degrees, 0–60' for minutes); returns tuples with error logging. |

---

## 4. Key Models & Data Contracts

* **`ScenarioFormData`:** The central DTO carrying all user selections from the UI to the scenario generators. Passed to top-level entry points.
* **`RunwayParams` / `RunwayData`:** Airport runway parameters (ICAO, dimensions, lighting, coordinates, magnetic variation).
* **`MapData` / `Tile` / `BoundingBox`:** Spatial and tile-index structures used for OSM image montaging, handling meridian and antimeridian wrapping.
* **`ScenarioXmlModels.cs`:** 
  * Defines the complete P3D Mission XML schema (`SimBase.Document`).
  * **SPECIAL RULE:** Must remain `public` with parameterless constructors because it is serialized using `System.Xml.Serialization.XmlSerializer`.
  * Contains `#pragma warning disable CS1591` at the file header so individual schema DTO properties do not require XML documentation comments.

---

## 5. Non-Negotiable AI Rules & Guardrails

When generating or refactoring code for this repository:

1. **Accessibility Default:** All application classes, records, services, models, and enums **must be `internal`**. The only exceptions are WinForms UI forms (`Form1 : Form`) and `ScenarioXmlModels.cs` (`XmlSerializer` DTOs).
2. **Primary Constructors & DI:** Injected dependencies in primary constructors must use null-coalescing validation:
   ```csharp
   internal class ScenarioService(Logger logger, FileOps fileOps)
   {
       private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
       private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
   }