using PieceOfCake.Core;
using UnityEngine;

namespace PieceOfCake.Game.World
{
    /// <summary>
    /// Converts simulation pixels (x right, y down, origin top-left) into Unity world units
    /// (x right, y up, origin at the bottom-left of the map, one unit per cell). The gameplay
    /// plane is z = 0; cake fronts face the camera at z = -0.5.
    /// </summary>
    public readonly struct WorldMapper
    {
        private const float UnitsPerPixel = 1f / SimConstants.CellSize;
        private readonly double mapPixelHeight;

        public WorldMapper(LevelLayout layout)
        {
            mapPixelHeight = layout.PixelHeight;
            Width = layout.Width;
            Height = layout.Height;
        }

        public int Width { get; }
        public int Height { get; }

        public Vector3 Point(double simX, double simY, float z = 0f) =>
            new Vector3((float)simX * UnitsPerPixel, (float)(mapPixelHeight - simY) * UnitsPerPixel, z);

        /// <summary>Bottom-centre of a map cell.</summary>
        public Vector3 CellBottom(GridPoint cell, float z = 0f) =>
            Point((cell.X + 0.5) * SimConstants.CellSize, (cell.Y + 1) * SimConstants.CellSize, z);

        public static float Length(double pixels) => (float)pixels * UnitsPerPixel;

        public Bounds MapBounds => new Bounds(new Vector3(Width / 2f, Height / 2f, 0f), new Vector3(Width + 4f, Height + 8f, 6f));
    }
}
