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
        public int CellHeightPx;

        /// <summary>
        /// Width of cell. (In pixels).
        /// </summary>
        public int CellWidthPx;

        /// <summary>
        /// The vertical offset of cell. (In pixels).
        /// </summary>
        public int VerticalOffsetPx;

        /// <summary>
        /// The horizontal offset of cell. (In pixels). 
        /// </summary>
        public int HorizontalOffsetPx;

        /// <summary>
        /// Create and added a new cell layout to surface's layout.
        /// </summary>
        /// <param name="s">Surface where the layout will be added.</param>
        /// <param name="cols">Determine how many columns the layout will have.</param>
        /// <param name="cellHeightPx">Determine the height of each cell.</param>
        /// <param name="cellWidthPx">Determine the width of each cell,</param>
        /// <param name="vOffsetPx"></param>
        /// <param name="hOffsetPx"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static void CreateGrid(Surface s, int cols, int cellHeightPx,
            int cellWidthPx, int vOffsetPx, int hOffsetPx)
        {
            var cellLayout = new CellLayout();
            cellLayout.CellHeightPx = cellHeightPx;
            cellLayout.CellWidthPx = cellWidthPx;
            cellLayout.VerticalOffsetPx = vOffsetPx;
            cellLayout.HorizontalOffsetPx = hOffsetPx;
            cellLayout.X = idx => (idx % cols) * cellLayout.CellWidthPx;
            cellLayout.Y = idx => (idx / cols) * cellLayout.CellHeightPx;
            s.layouts.Add(cellLayout);
        }
    }
}