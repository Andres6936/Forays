using GLDrawing;

namespace Forays
{
    public class CellLayout
    {
        public PositionFromIndex X;
        public PositionFromIndex Y;

        /// <summary>
        /// Z isn't used unless the VBO object has PositionDimensions set to 3.
        /// </summary>
        public PositionFromIndex Z;

        /// <summary>
        /// Height of cell. (In pixels).
        /// </summary>
        public readonly int CellHeightPx;

        /// <summary>
        /// Width of cell. (In pixels).
        /// </summary>
        public readonly int CellWidthPx;

        /// <summary>
        /// The vertical offset of cell. (In pixels).
        /// </summary>
        public readonly int VerticalOffsetPx;

        /// <summary>
        /// The horizontal offset of cell. (In pixels). 
        /// </summary>
        public readonly int HorizontalOffsetPx;

        // Constructs

        /// <summary>
        /// Create a new cell layout.
        /// </summary>
        /// <param name="cols">Determine how many columns the layout will have.</param>
        /// <param name="cellHeightPx">Determine the height of each cell.</param>
        /// <param name="cellWidthPx">Determine the width of each cell,</param>
        public CellLayout(int cols, int cellHeightPx, int cellWidthPx)
        {
            CellWidthPx = cellWidthPx;
            CellHeightPx = cellHeightPx;
            X = idx => (idx % cols) * CellWidthPx;
            Y = idx => (idx / cols) * CellHeightPx;
        }

        /// <summary>
        /// Create a new cell layout.
        /// </summary>
        /// <param name="cols">Determine how many columns the layout will have.</param>
        /// <param name="cellHeightPx">Determine the height of each cell.</param>
        /// <param name="cellWidthPx">Determine the width of each cell,</param>
        /// <param name="vOffsetPx">Determine the vertical offset of the cell.</param>
        /// <param name="hOffsetPx">Determine the horizontal offset of the cell.</param>
        public CellLayout(int cols, int cellHeightPx, int cellWidthPx, int vOffsetPx, int hOffsetPx)
            : this(cols, cellHeightPx, cellWidthPx)
        {
            // Delegate the other parameters to another construct.
            VerticalOffsetPx = vOffsetPx;
            HorizontalOffsetPx = hOffsetPx;
        }
    }
}