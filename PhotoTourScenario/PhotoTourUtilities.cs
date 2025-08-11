﻿using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator.PhotoTourScenario
{
    /// <summary>
    /// Provides a collection of static utility methods related to photo tour data processing,
    /// including geographical coordinate extraction for mapping, tour distance calculations,
    /// photo location retrieval, and photo download/resizing operations.
    /// </summary>
    internal class PhotoTourUtilities
    {
        /// <summary>
        /// Creates and returns an enumerable collection of <see cref="Coordinate"/> objects
        /// representing the geographical locations (latitude and longitude) for all entries
        /// in the provided list of photo and airport parameters.
        /// </summary>
        /// <param name="photoLocations">A list of <see cref="PhotoLocParams"/> objects,
        /// where each object contains latitude and longitude information for a tour stop.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// the latitude and longitude for each location in the input list.
        /// </returns>
        public static IEnumerable<Coordinate> SetOverviewCoords(List<PhotoLocParams> photoLocations)
        {
            // The Select method iterates over each photo/airport in the PhotoLocations
            // and projects it into a new 'Coordinate' object using the location's lat and lon.
            return photoLocations.Select(photo => new Coordinate(photo.latitude, photo.longitude));
        }

        /// <summary>
        /// Creates and returns an enumerable collection containing a single <see cref="Coordinate"/> object
        /// that represents the geographical location (latitude and longitude) of the start runway.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// only the start runway's latitude and longitude.
        /// </returns>
        static internal IEnumerable<Coordinate> SetLocationCoords(ScenarioFormData formData)
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(formData.StartRunway.AirportLat, formData.StartRunway.AirportLon)
            ];
            return coordinates;
        }

        /// <summary>
        /// Creates and returns an enumerable collection of two <see cref="Coordinate"/> objects
        /// representing a specific segment of the photo tour's route.
        /// The segment starts from the photo at the given index and ends at the next photo in the sequence.
        /// </summary>
        /// <param name="photoLocations">A list of <see cref="PhotoLocParams"/> objects,
        /// representing the ordered locations (photos) along the tour.</param>
        /// <param name="index">The zero-based index of the starting photo in the <paramref name="photoLocations"/> list
        /// for which the route segment is to be generated.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// the latitude and longitude of the photo at <paramref name="index"/> and the photo at <paramref name="index"/> + 1.
        /// </returns>
        static internal IEnumerable<Coordinate> SetRouteCoords(List<PhotoLocParams> photoLocations, int index)
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(photoLocations[index].latitude, photoLocations[index].longitude),
                new Coordinate(photoLocations[index + 1].latitude, photoLocations[index + 1].longitude)
            ];
            return coordinates;
        }

        /// <summary>
        /// Calculates the total distance of the photo tour by summing the forward distances
        /// of all individual photo locations within the provided list.
        /// </summary>
        /// <param name="photoLocations">A list of <see cref="PhotoLocParams"/> objects,
        /// where each object represents a leg in the photo tour and contains its forward distance.</param>
        /// <returns>
        /// A <see cref="double"/> representing the cumulative total distance of the entire photo tour.
        /// </returns>
        static internal double GetPhotoTourDistance(List<PhotoLocParams> photoLocations)
        {
            double distance = 0;
            foreach (PhotoLocParams location in photoLocations)
            {
                distance += location.forwardDist;
            }

            return distance;
        }

        /// <summary>
        /// Downloads all photos specified in the provided list of photo locations to the scenario images directory.
        /// It checks if each downloaded photo's width or height exceeds 95% of the monitor dimensions
        /// on which it will initially be displayed. If a dimension exceeds this threshold, the photo is
        /// proportionally resized to be at least 40 pixels less than the corresponding monitor dimension.
        /// Photos are loaded into a memory stream for dimension checking to avoid file locking issues
        /// before potential resizing by Magick.NET.
        /// </summary>
        /// <param name="photoLocations">A list of <see cref="PhotoLocParams"/> objects,
        /// each containing the URL of a photo to be downloaded and processed.</param>
        /// <returns>
        /// <see langword="true"/> if all photos are successfully downloaded and resized as needed;
        /// <see langword="false"/> if any photo download, load, or resize operation fails.
        /// </returns>
        static internal PhotoLocParams GetPhotoLocation(List<PhotoLocParams> photoLocations, int index)
        {
            return photoLocations[index];
        }

        /// <summary>
        /// Downloads all photos specified in the provided list of photo locations to the scenario images directory.
        /// It checks if each downloaded photo's width or height exceeds the monitor dimensions
        /// on which it will initially be displayed. If a dimension exceeds this threshold, the photo is
        /// proportionally resized.
        /// </summary>
        /// <param name="photoLocations">A list of <see cref="PhotoLocParams"/> objects,
        /// each containing the URL of a photo to be downloaded and processed.</param>
        /// <returns>
        /// <see langword="true"/> if all photos are successfully downloaded and resized as needed;
        /// <see langword="false"/> if any photo download or resize operation fails.
        /// </returns>
        static internal bool GetPhotos(List<PhotoLocParams> photoLocations, ScenarioFormData formData)
        {
            for (int index = 1; index < photoLocations.Count - 1; index++)
            {
                string filename = $"photo_{index:00}.jpg";
                string filePath = Path.Combine(formData.ScenarioImageFolder, filename);

                // 1. Download the photo
                if (!HttpRoutines.DownloadBinaryFile(photoLocations[index].photoURL, filePath))
                {
                    Log.Error($"PhotoTour.GetPhotos: Failed to download photo from '{photoLocations[index].photoURL}'.");
                    return false;
                }

                // 2. Load the photo for dimension checking into a MemoryStream to avoid file locking.
                int originalWidth;
                int originalHeight;
                try
                {
                    // Read all bytes from the file into a byte array
                    byte[] imageBytes = File.ReadAllBytes(filePath);

                    // Create a MemoryStream from the byte array
                    using MemoryStream ms = new(imageBytes);
                    // Create the Bitmap from the MemoryStream
                    using Bitmap drawing = new(ms);
                    originalWidth = drawing.Width;
                    originalHeight = drawing.Height;
                    // MemoryStream is disposed here, but the file was already released by File.ReadAllBytes
                    // Bitmap is disposed, MemoryStream is still open
                }
                catch (Exception ex)
                {
                    Log.Error($"PhotoTour.GetPhotos: Could not load image '{filePath}' into memory for dimension check. Error: {ex.Message}");
                    return false;
                }
                // At this point, the file `filePath` is no longer locked by your `Bitmap` object.
                // It's free for Magick.NET to open.

                // Determine if resizing is needed and calculate new dimensions while maintaining aspect ratio
                int newWidth = originalWidth;
                int newHeight = originalHeight;
                bool needsResize = false;

                // Calculate scaling factors for both dimensions relative to the "safe" monitor size 
                double targetWidth;
                double targetHeight;
                if (formData.PhotoTourPhotoAlignment == WindowAlignment.Centered)
                {
                    targetWidth = formData.PhotoTourPhotoMonitorWidth - Constants.PhotoSizeEdgeMarginPixels * 2;
                    targetHeight = formData.PhotoTourPhotoMonitorHeight - Constants.PhotoSizeEdgeMarginPixels * 2;
                }
                else
                {
                    targetWidth = formData.PhotoTourPhotoMonitorWidth - formData.PhotoTourPhotoOffset - Constants.PhotoSizeEdgeMarginPixels;
                    targetHeight = formData.PhotoTourPhotoMonitorHeight - formData.PhotoTourPhotoOffset - Constants.PhotoSizeEdgeMarginPixels;
                }

                // Check if either dimension exceeds safe monitor dimensions
                if (originalWidth > targetWidth ||
                    originalHeight > targetHeight)
                {
                    needsResize = true;

                    // Calculate ratios of target dimensions to original dimensions
                    double ratioX = targetWidth / originalWidth;
                    double ratioY = targetHeight / originalHeight;

                    // Use the smaller ratio to ensure the image fits within both target dimensions
                    double ratio = Math.Min(ratioX, ratioY);

                    newWidth = (int)Math.Round(originalWidth * ratio);
                    newHeight = (int)Math.Round(originalHeight * ratio);

                    // Ensure dimensions are positive integers to avoid errors in ImageUtils.Resize
                    if (newWidth <= 0) newWidth = 1;
                    if (newHeight <= 0) newHeight = 1;

                    Log.Info($"PhotoTour.GetPhotos: Resizing '{filename}' from {originalWidth}x{originalHeight} to {newWidth}x{newHeight}.");
                }

                // If photo needs resizing, call ImageUtils.Resize with calculated dimensions
                if (needsResize)
                {
                    // ImageUtils.Resize should now be able to open and modify the file
                    if (!ImageUtils.Resize(filePath, newWidth, newHeight))
                    {
                        Log.Error($"PhotoTour.GetPhotos: Failed to resize image '{filename}'.");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
