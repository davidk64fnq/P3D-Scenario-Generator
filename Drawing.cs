using ImageMagick;

namespace P3D_Scenario_Generator
{
    internal class Drawing
    {
        internal static int xAxis = 0, yAxis = 1;
        internal static int xTile = 0, yTile = 1, xOffset = 2, yOffset = 3;
        internal static int tileSize = 256;

        static internal void DrawRoute(List<List<int>> tiles, List<List<int>> boundingBox, string filename)
        {
            string imagePath = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\";
            using MagickImage image = new($"{imagePath}{filename}.png");
            DrawableStrokeColor strokeColor = new(new MagickColor("blue"));
            DrawableStrokeWidth stokeWidth = new(1);
            DrawableFillColor fillColor = new(MagickColors.Transparent);

            int centrePrevX = 0, centrePrevY = 0;
            for (int tileNo = 0; tileNo < tiles.Count; tileNo++)
            {
                int centreX = (boundingBox[xAxis].IndexOf(tiles[tileNo][xTile]) * tileSize) + tiles[tileNo][xOffset];
                int centreY = (boundingBox[yAxis].IndexOf(tiles[tileNo][yTile]) * tileSize) + tiles[tileNo][yOffset];
                DrawableCircle circle = new(centreX, centreY, centreX, centreY + 10);
                image.Draw(strokeColor, stokeWidth, fillColor, circle);
                if (tileNo > 0)
                {
                    DrawableLine line = new(centrePrevX, centrePrevY, centreX, centreY);
                    image.Draw(strokeColor, stokeWidth, fillColor, line);
                }
                centrePrevX = centreX;
                centrePrevY = centreY;
            }

            image.Write($"{imagePath}{filename}.png");
        }
    }
}
