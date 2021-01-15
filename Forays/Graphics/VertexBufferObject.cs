using OpenTK.Graphics.OpenGL;

namespace Forays
{
    public class VertexBufferObject
    {
        public int PositionArrayBufferID;
        public int OtherArrayBufferID;
        public int ElementArrayBufferID;
        public VertexAttributes VertexAttribs;

        /// <summary>
        /// This value controls whether 2 or 3 values are stored for position.
        /// </summary>
        public int PositionDimensions = 2;

        public int NumElements = 0;

        /// <summary>
        /// These 2 values track the number of float values in the VBOs.
        /// </summary>
        public int PositionDataSize = 0;

        public int OtherDataSize = 0;

        public static VertexBufferObject Create()
        {
            var vertexBufferObject = new VertexBufferObject();
            GL.GenBuffers(1, out vertexBufferObject.PositionArrayBufferID);
            GL.GenBuffers(1, out vertexBufferObject.OtherArrayBufferID);
            GL.GenBuffers(1, out vertexBufferObject.ElementArrayBufferID);
            return vertexBufferObject;
        }

        public static VertexBufferObject Create(int position_dimensions, VertexAttributes attribs)
        {
            var vertexBufferObject = new VertexBufferObject();
            GL.GenBuffers(1, out vertexBufferObject.PositionArrayBufferID);
            GL.GenBuffers(1, out vertexBufferObject.OtherArrayBufferID);
            GL.GenBuffers(1, out vertexBufferObject.ElementArrayBufferID);
            vertexBufferObject.PositionDimensions = position_dimensions;
            vertexBufferObject.VertexAttribs = attribs;
            return vertexBufferObject;
        }
    }
}