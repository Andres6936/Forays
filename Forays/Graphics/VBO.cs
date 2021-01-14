using OpenTK.Graphics.OpenGL;

namespace Forays
{
    public class VBO
    {
        public int PositionArrayBufferID;
        public int OtherArrayBufferID;
        public int ElementArrayBufferID;
        public VertexAttributes VertexAttribs;
        public int PositionDimensions = 2; //this value controls whether 2 or 3 values are stored for position.
        public int NumElements = 0;
        public int PositionDataSize = 0; //these 2 values track the number of float values in the VBOs.
        public int OtherDataSize = 0;

        protected VBO()
        {
        }

        public static VBO Create()
        {
            VBO v = new VBO();
            GL.GenBuffers(1, out v.PositionArrayBufferID);
            GL.GenBuffers(1, out v.OtherArrayBufferID);
            GL.GenBuffers(1, out v.ElementArrayBufferID);
            return v;
        }

        public static VBO Create(int position_dimensions, VertexAttributes attribs)
        {
            VBO v = new VBO();
            GL.GenBuffers(1, out v.PositionArrayBufferID);
            GL.GenBuffers(1, out v.OtherArrayBufferID);
            GL.GenBuffers(1, out v.ElementArrayBufferID);
            v.PositionDimensions = position_dimensions;
            v.VertexAttribs = attribs;
            return v;
        }
    }
}