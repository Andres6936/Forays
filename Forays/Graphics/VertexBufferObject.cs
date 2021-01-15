using OpenTK.Graphics.OpenGL;

namespace Forays
{
    public class VertexBufferObject
    {
        public int PositionArrayBufferID;
        public int OtherArrayBufferID;
        public int ElementArrayBufferID;
        public VertexAttributes VertexAttribs;

        public int
            PositionDimensions =
                2; //this value controls whether 2 or 3 values are stored for position.

        public int NumElements = 0;

        public int
            PositionDataSize = 0; //these 2 values track the number of float values in the VBOs.

        public int OtherDataSize = 0;

        protected VertexBufferObject()
        {
        }

        public static VertexBufferObject Create()
        {
            VertexBufferObject v = new VertexBufferObject();
            GL.GenBuffers(1, out v.PositionArrayBufferID);
            GL.GenBuffers(1, out v.OtherArrayBufferID);
            GL.GenBuffers(1, out v.ElementArrayBufferID);
            return v;
        }

        public static VertexBufferObject Create(int position_dimensions, VertexAttributes attribs)
        {
            VertexBufferObject v = new VertexBufferObject();
            GL.GenBuffers(1, out v.PositionArrayBufferID);
            GL.GenBuffers(1, out v.OtherArrayBufferID);
            GL.GenBuffers(1, out v.ElementArrayBufferID);
            v.PositionDimensions = position_dimensions;
            v.VertexAttribs = attribs;
            return v;
        }
    }
}