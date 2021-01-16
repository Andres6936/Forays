using GLDrawing;

namespace Forays
{
    public class CellLayout
    {
        public PositionFromIndex X;
        public PositionFromIndex Y;

        public PositionFromIndex
            Z = null; //Z isn't used unless the VBO object has PositionDimensions set to 3.

        public int CellHeightPx; //in pixels
        public int CellWidthPx;
        public int VerticalOffsetPx;
        public int HorizontalOffsetPx;

        /// <summary>
        /// Create and added a new cell layout to surface's layout.
        /// </summary>
        /// <param name="s">Surface where the layout will be added.</param>
        /// <param name="rows">Determine how many rows the layout will have.</param>
        /// <param name="cols">Determine how many columns the layout will have.</param>
        /// <param name="cell_height_px">Determine the height of each cell.</param>
        /// <param name="cell_width_px">Determine the width of each cell,</param>
        /// <param name="v_offset_px"></param>
        /// <param name="h_offset_px"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static void CreateGrid(Surface s, int rows, int cols, int cell_height_px,
            int cell_width_px,
            int v_offset_px, int h_offset_px, PositionFromIndex z = null)
        {
            CellLayout c = new CellLayout();
            c.CellHeightPx = cell_height_px;
            c.CellWidthPx = cell_width_px;
            c.VerticalOffsetPx = v_offset_px;
            c.HorizontalOffsetPx = h_offset_px;
            c.X = idx => (idx % cols) * c.CellWidthPx;
            c.Y = idx => (idx / cols) * c.CellHeightPx;
            c.Z = z;
            if (s != null)
            {
                s.layouts.Add(c);
            }
        }

        public static CellLayout Create(Surface s, int cell_height_px, int cell_width_px,
            int v_offset_px,
            int h_offset_px, PositionFromIndex x, PositionFromIndex y, PositionFromIndex z = null)
        {
            CellLayout c = new CellLayout(); //todo: fix x/y order for entire file?
            c.CellHeightPx = cell_height_px;
            c.CellWidthPx = cell_width_px;
            c.VerticalOffsetPx = v_offset_px;
            c.HorizontalOffsetPx = h_offset_px;
            c.X = x;
            c.Y = y;
            c.Z = z;
            if (s != null)
            {
                s.layouts.Add(c);
            }

            return c;
        }
    }
}