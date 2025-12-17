namespace P3D_Scenario_Generator.CelestialScenario
{
    /// <summary>
    /// Represents a celestial star with various astronomical properties. Defined as a record for
    /// immutable, value-based data with automatic properties and equality comparison.
    /// </summary>
    /// <param name="Constellation">The constellation the star belongs to.</param>
    /// <param name="Id">A unique identifier for the star, used to draw lines between stars in a constellation.</param>
    /// <param name="ConnectedId">An identifier for a connected star, if there is one, used to draw lines between stars in a constellation.</param>
    /// <param name="StarNumber">Navigation stars have an identifying integer, blank otherwise.</param>
    /// <param name="StarName">Navigation stars have a common name, blank otherwise.</param>
    /// <param name="WikiLink">A link to the stars Wikipedia page.</param>
    /// <param name="Bayer">The Bayer designation of the star.</param>
    /// <param name="RaH">The Right Ascension in hours.</param>
    /// <param name="RaM">The Right Ascension in minutes.</param>
    /// <param name="RaS">The Right Ascension in seconds.</param>
    /// <param name="DecD">The Declination in degrees.</param>
    /// <param name="DecM">The Declination in arcminutes.</param>
    /// <param name="DecS">The Declination in arcseconds.</param>
    /// <param name="VisMag">The visual magnitude of the star.</param>
    public record Star(
        string Constellation,
        string Id,
        string ConnectedId,
        string StarNumber,
        string StarName,
        string WikiLink,
        string Bayer,
        double RaH,
        double RaM,
        double RaS,
        double DecD,
        double DecM,
        double DecS,
        double VisMag
    );
}