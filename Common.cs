﻿using CoordinateSharp;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Methods shared across multiple scenario types
    /// </summary>
    internal class Common
    {

        /// <summary>
        /// Creates "Charts_01.jpg" using a montage of OSM tiles
        /// </summary>
        static internal void SetOverviewImage()
        {
            List<Tile> tiles = [];      // List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile
            BoundingBox boundingBox;    // List of x axis and y axis tile numbers that make up montage of tiles to cover set of coords
            int zoom = GetBoundingBoxZoom(tiles, 2, 2);
            SetOSMtiles(tiles, zoom);
            boundingBox = OSM.GetBoundingBox(tiles, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "Charts_01"); 
            if (Parameters.SelectedScenario != nameof(ScenarioTypes.Celestial))
            {
                Drawing.DrawRoute(tiles, boundingBox, "Charts_01");
            }
            Drawing.MakeSquare(boundingBox, "Charts_01", zoom, 2);
        }

        /// <summary>
        /// Works out most zoomed in level that includes all items specified by startItemIndex and finishItemIndex, 
        /// where the montage of OSM tiles doesn't exceed tilesWidth and tilesHeight in size
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="tilesWidth">Maximum number of tiles allowed for x axis</param>
        /// <param name="tilesHeight">Maximum number of tiles allowed for x axis</param>
        /// <returns>The maximum zoom level that meets constraints</returns>
        static internal int GetBoundingBoxZoom(List<Tile> tiles, int tilesWidth, int tilesHeight)
        {
            BoundingBox boundingBox;
            for (int zoom = 2; zoom <= 18; zoom++)
            {
                tiles.Clear();
                SetOSMtiles(tiles, zoom);
                boundingBox = OSM.GetBoundingBox(tiles, zoom);
                if ((boundingBox.xAxis.Count > tilesWidth) || (boundingBox.yAxis.Count > tilesHeight))
                {
                    return zoom - 1;
                }
            }
            return 18;
        }

        /// <summary>
        /// Works out most zoomed in level that includes all items specified by startItemIndex and finishItemIndex, 
        /// where the montage of OSM tiles doesn't exceed tilesWidth and tilesHeight in size
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="tilesWidth">Maximum number of tiles allowed for x axis</param>
        /// <param name="tilesHeight">Maximum number of tiles allowed for x axis</param>
        /// <returns>The maximum zoom level that meets constraints</returns>
        static internal int GetBoundingBoxZoom(List<Tile> tiles, int tilesWidth, int tilesHeight, int startItemIndex, int finishItemIndex)
        {
            BoundingBox boundingBox;
            for (int zoom = 2; zoom <= 18; zoom++)
            {
                tiles.Clear();
                SetOSMtiles(tiles, zoom, startItemIndex, finishItemIndex);
                boundingBox = OSM.GetBoundingBox(tiles, zoom);
                if ((boundingBox.xAxis.Count > tilesWidth) || (boundingBox.yAxis.Count > tilesHeight))
                {
                    return zoom - 1;
                }
            }
            return 18;
        }

        /// <summary>
        /// Works out what OSM tiles are needed for a set of coordinates. Two version of this method. This one
        /// covers all coordinates of a scenario.
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="zoom">The OSM tile zoom level for the boundingBox</param>
        static internal void SetOSMtiles(List<Tile> tiles, int zoom)
        {
            if (Parameters.SelectedScenario == nameof(ScenarioTypes.Circuit))
            {
                Circuit.SetCircuitOSMtiles(tiles, zoom, 0, Circuit.gates.Count - 1);
            }
            else if (Parameters.SelectedScenario == nameof(ScenarioTypes.PhotoTour))
            {
                PhotoTour.SetPhotoTourOSMtiles(tiles, zoom, 0, PhotoTour.PhotoCount - 1);
            }
            else if (Parameters.SelectedScenario == nameof(ScenarioTypes.WikiList))
            {
                Wikipedia.SetWikiOSMtiles(tiles, zoom, 0, Wikipedia.WikiTour.Count - 1);
            }
            else if (Parameters.SelectedScenario == nameof(ScenarioTypes.Celestial))
            {
                CelestialNav.SetCelestialOSMtiles(tiles, zoom, 0, 1);
            }
        }

        /// <summary>
        /// Works out what OSM tiles are needed for a set of coordinates. Two version of this method. This one
        /// is called with a start and finish index.
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="zoom">The OSM tile zoom level for the boundingBox</param>
        /// <param name="startItemIndex">Index of start item</param>
        /// <param name="finishItemIndex">Index of finish item</param>
        static internal void SetOSMtiles(List<Tile> tiles, int zoom, int startItemIndex, int finishItemIndex)
        {
            if (Parameters.SelectedScenario == nameof(ScenarioTypes.Circuit))
            {
                Circuit.SetCircuitOSMtiles(tiles, zoom, startItemIndex, finishItemIndex);
            }
            else if (Parameters.SelectedScenario == nameof(ScenarioTypes.PhotoTour))
            {
                PhotoTour.SetPhotoTourOSMtiles(tiles, zoom, startItemIndex, finishItemIndex);
            }
            else if (Parameters.SelectedScenario == nameof(ScenarioTypes.WikiList))
            {
                Wikipedia.SetWikiOSMtiles(tiles, zoom, startItemIndex, finishItemIndex);
            }
            else if (Parameters.SelectedScenario == nameof(ScenarioTypes.Celestial))
            {
                CelestialNav.SetCelestialOSMtiles(tiles, zoom, 1, 1);   // Use destination airport as scenario is a mid air start
            }
        }

        /// <summary>
        /// Creates "chart_thumb.jpg" using an OSM tile that covers the starting airport
        /// </summary>
        static internal void SetLocationImage()
        {
            List<Tile> tiles = [];      // List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile
            BoundingBox boundingBox;    // List of x axis and y axis tile numbers that make up montage of tiles to cover set of coords
            int zoom = 15;
            SetOSMtiles(tiles, zoom, 0, 0);
            boundingBox = OSM.GetBoundingBox(tiles, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "chart_thumb");
            if (boundingBox.xAxis.Count != boundingBox.yAxis.Count)
            {
                Drawing.MakeSquare(boundingBox, "chart_thumb", zoom, 2);
            }
            if (boundingBox.xAxis.Count == 2)
            {
                Drawing.Resize("chart_thumb.png", 256, 0);
            }
        }

        /// <summary>
        /// Creates "LegRoute_XX.jpg" images for all legs using a montage of OSM tiles that covers the start and finish leg items
        /// </summary>
        /// <param name="startItemIndex">Index of start item</param>
        /// <param name="finishItemIndex">Index of finish item</param>
        static internal void SetAllLegRouteImages(int startItemIndex, int finishItemIndex)
        {
            for (int itemNo = startItemIndex; itemNo <= finishItemIndex; itemNo++)
            {
                SetOneRouteImages(itemNo, itemNo + 1);
            }
        }

        /// <summary>
        /// Creates "LegRoute_XX.jpg" images for one leg using a montage of OSM tiles that covers the start and finish leg items 
        /// </summary>
        /// <param name="startItemIndex">Index of start item</param>
        /// <param name="finishItemIndex">Index of finish item</param>
        static internal void SetOneRouteImages(int startItemIndex, int finishItemIndex)
        {
            List<Tile> tiles = [];
            BoundingBox boundingBox;
            BoundingBox zoomInBoundingBox;

            int zoom = GetBoundingBoxZoom(tiles, 2, 2, startItemIndex, finishItemIndex);
            SetOSMtiles(tiles, zoom, startItemIndex, finishItemIndex);
            boundingBox = OSM.GetBoundingBox(tiles, zoom);
            int legNo = startItemIndex + 1;

            // zoom 1 image
            Drawing.MontageTiles(boundingBox, zoom, $"LegRoute_{legNo:00}_zoom1");
            Drawing.DrawRoute(tiles, boundingBox, $"LegRoute_{legNo:00}_zoom1");
            zoomInBoundingBox = Drawing.MakeSquare(boundingBox, $"LegRoute_{legNo:00}_zoom1", zoom, Con.tileFactor);
            Drawing.ConvertImageformat($"LegRoute_{legNo:00}_zoom1", "png", "jpg");

            // zoom 2, 3 (and 4) images, zoom 1 is base level for map window size of 512 pixels, zoom 2 is base level for map window of 1024 pixels
            // then there are two additional map images for the higher zoom levels.
            int numberZoomLevels = 2;
            if (Parameters.CommonMovingMapWindowSize == 1024)
                numberZoomLevels = 3;
            for (int inc = 1; inc <= numberZoomLevels; inc++)
            {
                SetOSMtiles(tiles, zoom + inc, startItemIndex, finishItemIndex);
                Drawing.MontageTiles(zoomInBoundingBox, zoom + inc, $"LegRoute_{legNo:00}_zoom{inc + 1}");
                Drawing.DrawRoute(tiles, zoomInBoundingBox, $"LegRoute_{legNo:00}_zoom{inc + 1}");
                zoomInBoundingBox = Drawing.MakeSquare(zoomInBoundingBox, $"LegRoute_{legNo:00}_zoom{inc + 1}", zoom + inc, (int)Math.Pow(2, inc + 1));
                Drawing.ConvertImageformat($"LegRoute_{legNo:00}_zoom{inc + 1}", "png", "jpg");
            }

            SetLegImageBoundaries(zoomInBoundingBox, zoom + numberZoomLevels + 1);
        }

        /// <summary>
        /// Calculates leg map photoURL lat/lon boundaries, assumes called in leg number sequence starting with first leg
        /// </summary>
        /// <param name="legNo">Leg numbers run from 0</param>
        /// <param name="boundingBox">The OSM tile numbers for x and y axis that cover the set of coordinates depicted in an image</param>
        /// <param name="zoom">The OSM tile zoom level for the boundingBox</param>
        static internal void SetLegImageBoundaries(BoundingBox boundingBox, int zoom)
        {
            MapEdges legEdges = new();
            Coordinate c;

            // Get the lat/lon coordinates of top left corner of bounding box
            c = OSM.TileNoToLatLon(boundingBox.xAxis[0], boundingBox.yAxis[0], zoom);
            legEdges.north = c.Latitude;
            legEdges.west = c.Longitude;

            // Get the lat/lon coordinates of top left corner of tile immediately below and right of bottom right corner of bounding box
            c = OSM.TileNoToLatLon(boundingBox.xAxis[^1] + 1, boundingBox.yAxis[^1] + 1, zoom);
            legEdges.south = c.Latitude;
            legEdges.east = c.Longitude;

            // Assumes this method called in leg number sequence starting with first leg
            if (Parameters.SelectedScenario == nameof(ScenarioTypes.PhotoTour))
            {
                PhotoTour.PhotoTourLegMapEdges.Add(legEdges);
            }
            else if (Parameters.SelectedScenario == nameof(ScenarioTypes.WikiList))
            {
                Wikipedia.WikiLegMapEdges.Add(legEdges); ;
            }
            
        }
    }
}
