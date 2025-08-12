using P3D_Scenario_Generator.Models;

namespace P3D_Scenario_Generator.Interfaces
{
    /// <summary>
    /// Defines the contract for an aircraft management service, which handles user selection,
    /// management, and persistence of aircraft variants for a scenario.
    /// </summary>
    public interface IAircraft
    {
        /// <summary>
        /// List of aircraft variants maintained by user with current selection shown on General tab of form.
        /// Sorted alphabetically by display name.
        /// </summary>
        List<AircraftVariant> AircraftVariants { get; }

        /// <summary>
        /// Currently selected aircraft variant displayed on General tab of form
        /// </summary>
        int CurrentAircraftVariantIndex { get; }

        /// <summary>
        /// Prompts the user to select an aircraft variant thumbnail image and collects the aircraft title, 
        /// cruise speed, whether it has wheels or equivalent, and whether it has floats, from the aircraft.cfg file.
        /// Display name is initialised to be the aircraft variant title but can be changed by user subsequently.
        /// Adds new variant to <see cref="AircraftVariants"/>, maintaining display name alphabetical sort.
        /// </summary>
        /// <param name="formData">The scenario form data containing paths like the P3D install directory.</param>
        /// <returns>The display name of the new aircraft variant, or an empty string if none was selected or an error occurred.</returns>
        string ChooseAircraftVariant(ScenarioFormData formData);

        /// <summary>
        /// Adds aircraft variant to <see cref="AircraftVariants"/> ensuring no duplicates, maintains list
        /// in alphabetical order on display name, adjusts <see cref="CurrentAircraftVariantIndex"/>
        /// </summary>
        /// <param name="aircraftVariant">The aircraft variant to be added</param>
        /// <returns>True if new aircraft variant added to <see cref="AircraftVariants"/></returns>
        bool AddAircraftVariant(AircraftVariant aircraftVariant);

        /// <summary>
        /// Reset <see cref="CurrentAircraftVariantIndex"/> to the instance of <see cref="AircraftVariant"/>
        /// with displayName. If displayName not found in <see cref="AircraftVariants"/> then does nothing.
        /// </summary>
        /// <param name="displayName">The display name of the new instance to be set as <see cref="CurrentAircraftVariantIndex"/></param>
        void ChangeCurrentAircraftVariantIndex(string displayName);

        /// <summary>
        /// Deletes the aircraft variant with displayName from <see cref="AircraftVariants"/> and reduces
        /// <see cref="CurrentAircraftVariantIndex"/> by 1, i.e. the prior item in alphabetical list of display names
        /// </summary>
        /// <param name="displayName">The display name of the instance to be deleted from <see cref="AircraftVariants"/></param>
        /// <returns>True if aircraft variant with displayName deleted, false otherwise</returns>
        bool DeleteAircraftVariant(string displayName);

        /// <summary>
        /// Changes the display name of <see cref="CurrentAircraftVariantIndex"/> aircraft variant in
        /// <see cref="AircraftVariants"/> to displayName. Resorts <see cref="AircraftVariants"/> and
        /// updates <see cref="CurrentAircraftVariantIndex"/>. If <see cref="AircraftVariants"/> is null
        /// or empty then does nothing.
        /// </summary>
        /// <param name="displayName">The new display name</param>
        void UpdateAircraftVariantDisplayName(string displayName);

        /// <summary>
        /// Get a sorted list of the aircraft variant display names
        /// </summary>
        /// <returns>Alphabetically sorted list of the aircraft variant display names</returns>
        List<string> GetAircraftVariantDisplayNames();

        /// <summary>
        /// Builds a formatted string showing the <see cref="CurrentAircraftVariantIndex"/> aircraft variant in
        /// <see cref="AircraftVariants"/>.
        /// </summary>
        /// <returns>Formatted string showing title, display name, cruise speed, whether aircraft has wheels (equivalent) and/or floats</returns>
        string SetTextBoxGeneralAircraftValues();

        /// <summary>
        /// Retrieves the currently selected aircraft variant.
        /// Handles cases where no variant is selected or the index is invalid.
        /// </summary>
        /// <returns>The selected AircraftVariant, or null if no valid variant is currently selected.</returns>
        AircraftVariant GetCurrentVariant();

        /// <summary>
        /// Saves the current list of aircraft variants to a file in JSON format using CacheManagerAsync.
        /// The save operation is skipped if the list of variants is empty to prevent overwriting
        /// a valid file with an empty list.
        /// </summary>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        Task SaveAircraftVariantsAsync(IProgress<string> progressReporter);

        /// <summary>
        /// Loads the list of aircraft variants from a file stored in JSON format using CacheManagerAsync.
        /// It first attempts to load a local user-created version. If the local file is not found,
        /// it falls back to an embedded resource.
        /// </summary>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>A <see cref="Task{TResult}"/> that returns <see langword="true"/> if aircraft variants were successfully loaded, <see langword="false"/> otherwise.</returns>
        Task<bool> LoadAircraftVariantsAsync(IProgress<string> progressReporter = null);
    }
}
