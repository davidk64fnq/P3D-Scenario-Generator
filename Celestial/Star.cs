namespace P3D_Scenario_Generator.Celestial
{
    /// <summary>
    /// Represents a celestial star with various astronomical properties.
    /// </summary>
    /// <param name="constellation">The constellation the star belongs to.</param>
    /// <param name="id">A unique identifier for the star, used to draw lines between stars in a constellation.</param>
    /// <param name="connectedId">An identifier for a connected star, if there is one, used to draw lines between stars in a constellation.</param>
    /// <param name="starNumber">Navigation stars have an identifying integer, blank otherwise.</param>
    /// <param name="starName">Navigation stars have a common name, blank otherwise.</param>
    /// <param name="wikiLink">A link to the stars Wikipedia page.</param>
    /// <param name="bayer">The Bayer designation of the star.</param>
    /// <param name="raH">The Right Ascension in hours.</param>
    /// <param name="raM">The Right Ascension in minutes.</param>
    /// <param name="raS">The Right Ascension in seconds.</param>
    /// <param name="decD">The Declination in degrees.</param>
    /// <param name="decM">The Declination in arcminutes.</param>
    /// <param name="decS">The Declination in arcseconds.</param>
    /// <param name="visMag">The visual magnitude of the star.</param>
    public class Star(
        string constellation,
        string id,
        string connectedId,
        string starNumber,
        string starName,
        string wikiLink,
        string bayer,
        double raH,
        double raM,
        double raS,
        double decD,
        double decM,
        double decS,
        double visMag)
    {
        /// <summary>
        /// Gets the constellation the star belongs to.
        /// </summary>
        public string Constellation { get; init; } = constellation;

        /// <summary>
        /// Gets a unique identifier for the star, used to draw lines between stars in a constellation.
        /// </summary>
        public string Id { get; init; } = id;

        /// <summary>
        /// Gets an identifier for a connected star, if there is one, used to draw lines between stars in a constellation.
        /// </summary>
        public string ConnectedId { get; init; } = connectedId;

        /// <summary>
        /// For navigation stars gets an identifying integer, blank otherwise.
        /// </summary>
        public string StarNumber { get; init; } = starNumber;

        /// <summary>
        /// For navigation stars gets a common name, blank otherwise.
        /// </summary>
        public string StarName { get; init; } = starName;

        /// <summary>
        /// Gets a link to the stars Wikipedia page.
        /// </summary>
        public string WikiLink { get; init; } = wikiLink;

        /// <summary>
        /// Gets the Bayer designation of the star.
        /// </summary>
        public string Bayer { get; init; } = bayer;

        /// <summary>
        /// Gets the Right Ascension in hours.
        /// </summary>
        public double RaH { get; init; } = raH;

        /// <summary>
        /// Gets the Right Ascension in minutes.
        /// </summary>
        public double RaM { get; init; } = raM;

        /// <summary>
        /// Gets the Right Ascension in seconds.
        /// </summary>
        public double RaS { get; init; } = raS;

        /// <summary>
        /// Gets the Declination in degrees.
        /// </summary>
        public double DecD { get; init; } = decD;

        /// <summary>
        /// Gets the Declination in arcminutes.
        /// </summary>
        public double DecM { get; init; } = decM;

        /// <summary>
        /// Gets the Declination in arcseconds.
        /// </summary>
        public double DecS { get; init; } = decS;

        /// <summary>
        /// Gets the visual magnitude of the star.
        /// </summary>
        public double VisMag { get; init; } = visMag;
    }
}
