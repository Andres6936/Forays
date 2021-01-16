using GLDrawing;

namespace Forays
{
    public class CellLayout
    {
        public readonly PositionFromIndex X;

        public readonly PositionFromIndex Y;

        /// <summary>
        /// Height of cell. (In pixels).
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// Width of cell. (In pixels).
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// The vertical offset of cell. (In pixels).
        /// </summary>
        public readonly int VerticalOffset;

        /// <summary>
        /// The horizontal offset of cell. (In pixels). 
        /// </summary>
        public readonly int HorizontalOffset;

        // Constructs

        /// <summary>
        /// Create a new cell layout.
        /// </summary>
        /// <param name="cols">Determine how many columns the layout will have.</param>
        /// <param name="height">Determine the height of each cell.</param>
        /// <param name="width">Determine the width of each cell,</param>
        public CellLayout(int cols, int height, int width)
        {
            Width = width;
            Height = height;
            X = idx => (idx % cols) * Width;
            Y = idx => (idx / cols) * Height;
        }

        /// <summary>
        /// Create a new cell layout.
        /// </summary>
        /// <param name="cols">Determine how many columns the layout will have.</param>
        /// <param name="height">Determine the height of each cell.</param>
        /// <param name="width">Determine the width of each cell,</param>
        /// <param name="vOffset">Determine the vertical offset of the cell.</param>
        /// <param name="hOffset">Determine the horizontal offset of the cell.</param>
        public CellLayout(int cols, int height, int width, int vOffset, int hOffset)
            : this(cols, height, width)
        {
            // Delegate the other parameters to another construct.
            VerticalOffset = vOffset;
            HorizontalOffset = hOffset;
        }
    }
}