﻿using CoordinateSharp;
using System.Collections.Generic;

namespace P3D_Scenario_Generator.MapTiles
{
    /// <summary>
    /// Provides functionality for generating various OpenStreetMap (OSM) based map images,
    /// including overview maps and location-specific thumbnails, by orchestrating
    /// tile retrieval, montage creation, and image manipulation. It manages the
    /// workflow from geographic coordinates to final image files.
    /// </summary>
    internal class MapTileImageMaker
    {
        /// <summary>
        /// Generates an overview image with dimensions 2 x 2 map tiles from OpenStreetMap tiles based on a set of geographical coordinates.
        /// Stores the image in scenario images folder.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates to be included on the image.</param>
        /// <param name="drawRoute">Whether to draw route on the image.</param>
        /// <returns>True if the overview image was successfully created, false otherwise.</returns>
        static internal bool CreateOverviewImage(IEnumerable<Coordinate> coordinates, bool drawRoute, ScenarioFormData formData)
        {
            // Input validation for coordinates
            if (coordinates == null || !coordinates.Any())
            {
                Log.Error("MapTileImageMaker.CreateOverviewImage: Input coordinates list is null or empty. Cannot create overview image.");
                return false;
            }

            // Call the updated GetOptimalZoomLevel method
            if (!MapTileCalculator.GetOptimalZoomLevel(coordinates, Constants.overviewImageTileFactor, Constants.overviewImageTileFactor, out int zoom))
            {
                // GetOptimalZoomLevel already logs specific errors internally.
                Log.Error("MapTileImageMaker.CreateOverviewImage: Failed to determine optimal zoom level. See previous logs for details.");
                return false;
            }

            // Build list of OSM tiles at required zoom for all coordinates
            List<Tile> tiles = [];
            MapTileCalculator.SetOSMTilesForCoordinates(tiles, zoom, coordinates);

            // Ensure tiles were successfully retrieved before proceeding
            // but if the list is empty, it's a critical failure here.
            if (tiles == null || tiles.Count == 0)
            {
                Log.Error($"MapTileImageMaker.CreateOverviewImage: No OSM tiles found for the given coordinates at zoom {zoom}.");
                return false;
            }

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            if (!BoundingBoxCalculator.GetBoundingBox(tiles, zoom, out BoundingBox boundingBox))
            {
                Log.Error($"MapTileImageMaker.CreateOverviewImage: Failed to calculate bounding box at zoom {zoom}.");
                return false;
            }

            // Create montage of tiles in images folder
            string imageName = "Charts_01";
            if (!MapTileMontager.MontageTiles(boundingBox, zoom, imageName, formData))
            {
                Log.Error($"MapTileImageMaker.CreateOverviewImage: Failed to montage tiles for image '{imageName}'.");
                return false;
            }

            // Draw a line connecting coordinates onto image
            if (drawRoute && !ImageUtils.DrawRoute(tiles, boundingBox, imageName, formData))
            {
                Log.Error($"MapTileImageMaker.CreateOverviewImage: Failed to draw route on image '{imageName}'.");
                return false;
            }

            // Extend montage of tiles to make the image square (if it isn't already)
            // New out parameter for MakeSquare
            if (!MakeSquare(boundingBox, imageName, zoom, out _, formData))
            {
                Log.Error($"MapTileImageMaker.CreateOverviewImage: Failed to make image '{imageName}' square.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a location image with dimensions of 1 x 1 map tiles from OpenStreetMap tiles based on a set of geographical coordinates.
        /// Stores the image in scenario images folder.
        /// </summary>
        /// <param name="coordinates">A collection of geographical coordinates to be included on the map.</param>
        /// <returns>True if the location image was successfully created, false otherwise.</returns>
        static internal bool CreateLocationImage(IEnumerable<Coordinate> coordinates, ScenarioFormData formData)
        {
            // Input validation for coordinates
            if (coordinates == null || !coordinates.Any())
            {
                Log.Error("MapTileImageMaker.CreateLocationImage: Input coordinates list is null or empty. Cannot create location image.");
                return false;
            }

            // Build list of OSM tiles at zoom 15 for all coordinates (the approx zoom to see airport on 1 x 1 map tile image)
            List<Tile> tiles = [];
            int locationImageZoomLevel = 15;
            MapTileCalculator.SetOSMTilesForCoordinates(tiles, locationImageZoomLevel, coordinates);

            // Ensure tiles were successfully retrieved and the list is not empty before proceeding
            if (tiles.Count == 0) 
            {
                Log.Error($"MapTileImageMaker.CreateLocationImage: No unique OSM tiles were found for the given coordinates at zoom {locationImageZoomLevel}. This may indicate an issue with coordinate data or tile calculation.");
                return false;
            }

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            if (!BoundingBoxCalculator.GetBoundingBox(tiles, locationImageZoomLevel, out BoundingBox boundingBox)) // Update call to GetBoundingBox
            {
                Log.Error($"MapTileImageMaker.CreateLocationImage: Failed to calculate bounding box at zoom {locationImageZoomLevel}.");
                return false;
            }

            // Create montage of tiles in images folder
            string imageName = "chart_thumb";
            if (!MapTileMontager.MontageTiles(boundingBox, locationImageZoomLevel, imageName, formData))
            {
                Log.Error($"MapTileImageMaker.CreateLocationImage: Failed to montage tiles for image '{imageName}'.");
                return false;
            }

            // If image is 1 x 2 or 2 x 1 then extend montage of tiles to make the image 2 x 2 and then resize to 1 x 1
            // This situation arises where coordinate is too close to tile edge on 1 x 1.
            if (boundingBox.XAxis.Count != 1 || boundingBox.YAxis.Count != 1)
            {
                // Attempt to make the image square.
                if (MakeSquare(boundingBox, imageName, locationImageZoomLevel, out _, formData)) 
                {
                    // ONLY if MakeSquare succeeds, then attempt to resize to the final 1x1 target size.
                    if (!ImageUtils.Resize($"{imageName}.png", Constants.tileSize, Constants.tileSize, formData))
                    {
                        Log.Error($"MapTileImageMaker.CreateLocationImage: Failed to resize image '{imageName}.png' after successful MakeSquare. Aborting.");
                        return false;
                    }
                }
                else
                {
                    // MakeSquare failed. The whole operation for this 'if' branch fails.
                    Log.Error($"MapTileImageMaker.CreateLocationImage: Failed to make image '{imageName}' square. Aborting.");
                    return false;
                }
            }
            // If boundingBox.XAxis.Count == boundingBox.YAxis.Count == 1, it means it's already a 1x1 tile, so no squaring or resizing is needed for the 1x1 location image.

            return true;
        }

        /// <summary>
        /// Adjusts the provided bounding box and corresponding image to a 2 x 2 square format, potentially by adding padding tiles and
        /// then cropping. This method determines which specific padding operation is required based on the current dimensions of the bounding
        /// box relative to the target size. Possible input sizes are 1 x 1, 1 x 2, 2 x 1. An additional out parameter returns a bounding box 
        /// calculated at the next higher zoom level.
        /// </summary>
        /// <param name="boundingBox">The current <see cref="BoundingBox"/> representing the tile grid. This object is used
        /// as input for padding operations.</param>
        /// <param name="filename">The base filename of the image being processed. This file will be modified.</param>
        /// <param name="zoom">The current zoom level of the map tiles.</param>
        /// <param name="newBoundingBox">When this method returns, contains the updated <see cref="BoundingBox"/>
        /// reflecting the new tile coordinates after padding and/or zooming; otherwise, a default <see cref="BoundingBox"/>.</param>
        /// <returns><see langword="true"/> if the image was successfully made square or zoomed in; otherwise, <see langword="false"/>.</returns>
        static internal bool MakeSquare(BoundingBox boundingBox, string filename, int zoom, out PaddingMethod paddingMethod, ScenarioFormData formData)
        {
            try
            {
                // Input validation
                if (boundingBox == null || boundingBox.XAxis.Count == 0 || boundingBox.YAxis.Count == 0)
                {
                    Log.Error($"MapTileImageMaker.MakeSquare: Input boundingBox is null or empty for file '{filename}'.");
                    paddingMethod = PaddingMethod.None; 
                    return false;
                }

                // Get next tile East and West - allow for possible wrap around meridian
                int newEastXIndex = MapTileCalculator.IncXtileNo(boundingBox.XAxis[^1], zoom);
                int newWestXindex = MapTileCalculator.DecXtileNo(boundingBox.XAxis[0], zoom);

                // Get next tile South and North - don't go below bottom or top edge of map.
                // -1 means no tile can be added in that direction (pole reached).
                int newSouthYindex = MapTileCalculator.IncYtileNo(boundingBox.YAxis[^1], zoom);
                int newNorthYindex = MapTileCalculator.DecYtileNo(boundingBox.YAxis[0]);

                // Determine padding strategy based on current bounding box dimensions
                if (boundingBox.XAxis.Count < boundingBox.YAxis.Count) // Current image is taller than it is wide, pad horizontally
                {
                    Log.Info($"MakeSquare: Padding West/East for {filename}.");
                    paddingMethod = PaddingMethod.WestEast; 
                    if (!MapTilePadder.PadWestEast(boundingBox, newWestXindex, newEastXIndex, filename, zoom, formData))
                    {
                        Log.Error($"MakeSquare: Failed to pad West/East for '{filename}'.");
                        return false;
                    }
                }
                else if (boundingBox.YAxis.Count < boundingBox.XAxis.Count) // Current image is wider than it is tall, pad vertically
                {
                    if (newSouthYindex < 0) // At or near South Pole, can only pad North
                    {
                        Log.Info($"MakeSquare: At South Pole, padding North only for {filename}.");
                        paddingMethod = PaddingMethod.North;
                        if (!MapTilePadder.PadNorth(boundingBox, newNorthYindex, filename, zoom, formData))
                        {
                            Log.Error($"MakeSquare: Failed to pad North (South Pole) for '{filename}'.");
                            return false;
                        }
                    }
                    else if (newNorthYindex < 0) // At or near North Pole, can only pad South
                    {
                        Log.Info($"MakeSquare: At North Pole, padding South only for {filename}.");
                        paddingMethod = PaddingMethod.South;
                        if (!MapTilePadder.PadSouth(boundingBox, newSouthYindex, filename, zoom, formData))
                        {
                            Log.Error($"MakeSquare: Failed to pad South (North Pole) for '{filename}'.");
                            return false;
                        }
                    }
                    else // Can pad both North and South
                    {
                        Log.Info($"MakeSquare: Padding North/South (general) for {filename}.");
                        paddingMethod = PaddingMethod.NorthSouth;
                        if (!MapTilePadder.PadNorthSouth(boundingBox, newNorthYindex, newSouthYindex, filename, zoom, formData))
                        {
                            Log.Error($"MakeSquare: Failed to pad North/South (general) for '{filename}'.");
                            return false;
                        }
                    }
                }
                else if (boundingBox.XAxis.Count == boundingBox.YAxis.Count && boundingBox.XAxis.Count == 1) // Image is square but smaller than target size of 2 x 2
                {
                    Log.Info($"MakeSquare: Image is square but smaller than target size of 2 x 2, attempting NorthSouthWestEast padding for {filename}.");
                    paddingMethod = PaddingMethod.NorthSouthWestEast;
                    if (!MapTilePadder.PadNorthSouthWestEast(boundingBox, newNorthYindex, newSouthYindex, newWestXindex, newEastXIndex, filename, zoom, formData))
                    {
                        Log.Error($"MakeSquare: Failed to pad North/South/West/East for '{filename}'.");
                        return false;
                    }
                }
                else // Already square and at desired size. 
                {
                    Log.Info($"MakeSquare: Image is already 2 x 2 square. Calling ZoomIn to get bounding box for next zoom level for {filename}.");
                    paddingMethod = PaddingMethod.None; 
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"MapTileImageMaker.MakeSquare: An unexpected error occurred while trying to make image square for '{filename}': {ex.Message}", ex);
                paddingMethod = PaddingMethod.None;
                return false;
            }
        }

        static internal bool SetLegRouteImages(IEnumerable<Coordinate> coordinates, List<MapEdges> legMapEdges, int legNo, bool drawRoute, ScenarioFormData formData)
        {

            int legZoomLabel = 1;
            // Create first zoom level image for leg route
            if (!SetFirstLegRouteImage(coordinates, legNo, drawRoute, legZoomLabel, out int zoom, out PaddingMethod paddingMethod, out BoundingBox boundingBox, formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to create route image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                return false;
            }

            // Calculate next zoom level bounding box
            if (!MapTilePadder.GetNextZoomBoundingBox(paddingMethod, boundingBox, out BoundingBox zoomInBoundingBox))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to calculate next zoom level bounding box image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                return false;
            }

            // zoom 2, 3 (and 4) images, zoom 1 is base level for map window size of 512 pixels, zoom 2 is base level for map window of 1024 pixels
            // then there are two additional map images for the higher zoom levels.
            int numberZoomLevels = 2;
            if (formData.MapWindowSize == MapWindowSizeOption.Size1024)
                numberZoomLevels = 3;
            BoundingBox nextBoundingBox = zoomInBoundingBox.DeepCopy(); // Use the updated bounding box for the next zoom level
            for (int inc = 1; inc <= numberZoomLevels; inc++)
            {
                legZoomLabel = 1 + inc;
                // Create subsequent zoom level images for leg route
                if (!SetNextLegRouteImage(coordinates, legNo, drawRoute, legZoomLabel, zoom + inc, nextBoundingBox, formData))
                {
                    Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to create route image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                    return false;
                }

                // Calculate next zoom level bounding box
                paddingMethod = PaddingMethod.None;
                if (!MapTilePadder.GetNextZoomBoundingBox(paddingMethod, nextBoundingBox, out nextBoundingBox))
                {
                    Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to calculate next zoom level bounding box image LegRoute_{legNo:00}_zoom{legZoomLabel}.");
                    return false;
                }
            }

            // Calculate leg map photoURL lat/lon boundaries, assumes called in leg number sequence starting with first leg
            if (!SetLegImageBoundaries(nextBoundingBox, zoom + numberZoomLevels + 1, legMapEdges))
            {
                string imageName = $"LegRoute_{legNo:00}_zoom{legZoomLabel}";
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to calculate leg route lat/lon boundaries on image '{imageName}'.");
                return false;
            }

            return true;
        }

        static internal bool SetFirstLegRouteImage(IEnumerable<Coordinate> coordinates, int legNo, bool drawRoute, int legZoomLabel, 
            out int zoom, out PaddingMethod paddingMethod, out BoundingBox boundingBox, ScenarioFormData formData)
        {
            // Initialize out parameters
            zoom = 0; // Default value for out parameter
            paddingMethod = PaddingMethod.None; // Default value for out parameter
            boundingBox = new BoundingBox(); // Initialize the bounding box

            // Input validation for coordinates
            if (coordinates == null || !coordinates.Any())
            {
                Log.Error("MapTileImageMaker.SetLegRouteImage: Input coordinates list is null or empty. Cannot create route image.");
                return false;
            }

            // Call the updated GetOptimalZoomLevel method
            if (!MapTileCalculator.GetOptimalZoomLevel(coordinates, Constants.legRouteImageTileFactor, Constants.legRouteImageTileFactor, out zoom))
            {
                // GetOptimalZoomLevel already logs specific errors internally.
                Log.Error("MapTileImageMaker.SetLegRouteImage: Failed to determine optimal zoom level. See previous logs for details.");
                return false;
            }

            // Build list of OSM tiles at required zoom for all coordinates
            List<Tile> tiles = [];
            MapTileCalculator.SetOSMTilesForCoordinates(tiles, zoom, coordinates);

            // Ensure tiles were successfully retrieved before proceeding
            // but if the list is empty, it's a critical failure here.
            if (tiles == null || tiles.Count == 0)
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: No OSM tiles found for the given coordinates at zoom {zoom}.");
                return false;
            }

            // Build list of x axis and y axis tile numbers that make up montage of tiles to cover set of coordinates
            if (!BoundingBoxCalculator.GetBoundingBox(tiles, zoom, out boundingBox))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to calculate bounding box at zoom {zoom}.");
                return false;
            }

            // Create montage of tiles in images folder
            string imageName = $"LegRoute_{legNo:00}_zoom{legZoomLabel}";
            if (!MapTileMontager.MontageTiles(boundingBox, zoom, imageName, formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to montage tiles for image '{imageName}'.");
                return false;
            }

            // Draw a line connecting coordinates onto image
            if (drawRoute && !ImageUtils.DrawRoute(tiles, boundingBox, imageName, formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to draw route on image '{imageName}'.");
                return false;
            }

            // Extend montage of tiles to make the image square (if it isn't already)
            if (!MakeSquare(boundingBox, imageName, zoom, out paddingMethod, formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to make image '{imageName}' square.");
                return false;
            }

            // Convert image format from png to jpg
            if (!ImageUtils.ConvertImageformat(imageName, "png", "jpg", formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to convert from png to jpg on image '{imageName}'.");
                return false;
            }

            return true;
        }

        static internal bool SetNextLegRouteImage(IEnumerable<Coordinate> coordinates, int legNo, bool drawRoute, int legZoomLabel, int zoom, 
            BoundingBox nextBoundingBox, ScenarioFormData formData)
        {
            // Build list of OSM tiles at required zoom for all coordinates
            List<Tile> tiles = [];
            MapTileCalculator.SetOSMTilesForCoordinates(tiles, zoom, coordinates);

            // Ensure tiles were successfully retrieved before proceeding
            // but if the list is empty, it's a critical failure here.
            if (tiles == null || tiles.Count == 0)
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: No OSM tiles found for the given coordinates at zoom {zoom}.");
                return false;
            }

            // Create montage of tiles in images folder
            string imageName = $"LegRoute_{legNo:00}_zoom{legZoomLabel}";
            if (!MapTileMontager.MontageTiles(nextBoundingBox, zoom, imageName, formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to montage tiles for image '{imageName}'.");
                return false;
            }

            // Draw a line connecting coordinates onto image
            if (drawRoute && !ImageUtils.DrawRoute(tiles, nextBoundingBox, imageName, formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to draw route on image '{imageName}'.");
                return false;
            }

            // Convert image format from png to jpg
            if (!ImageUtils.ConvertImageformat(imageName, "png", "jpg", formData))
            {
                Log.Error($"MapTileImageMaker.SetLegRouteImage: Failed to convert from png to jpg on image '{imageName}'.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates leg map photoURL lat/lon boundaries, assumes called in leg number sequence starting with first leg
        /// </summary>
        /// <param name="boundingBox">The OSM tile numbers for x and y axis that cover the set of coordinates depicted in an image</param>
        /// <param name="zoom">The OSM tile zoom level for the boundingBox</param>
        static internal bool SetLegImageBoundaries(BoundingBox boundingBox, int zoom, List<MapEdges> legMapEdges)
        {
            MapEdges legEdges = new();
            Coordinate c;

            // Get the lat/lon coordinates of top left corner of bounding box
            c = MapTileCalculator.TileNoToLatLon(boundingBox.XAxis[0], boundingBox.YAxis[0], zoom);
            legEdges.north = c.Latitude;
            legEdges.west = c.Longitude;

            // Get the lat/lon coordinates of top left corner of tile immediately below and right of bottom right corner of bounding box
            c = MapTileCalculator.TileNoToLatLon(boundingBox.XAxis[^1] + 1, boundingBox.YAxis[^1] + 1, zoom);
            legEdges.south = c.Latitude;
            legEdges.east = c.Longitude;

            legMapEdges.Add(legEdges);

            return true;
        }
    }
}
