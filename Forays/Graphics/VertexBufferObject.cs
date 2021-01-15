using OpenTK.Graphics.OpenGL;

namespace Forays.Graphics
{
    public class VertexBufferObject
    {
        // Member Variables

        public int PositionArrayBufferId;
        public int OtherArrayBufferId;
        public int ElementArrayBufferId;
        public VertexAttributes VertexAttributes;

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

        // Methods Private

        /// <summary>
        /// Create and return a instance for default of Vertex Buffer Object.
        /// </summary>
        /// <returns>Instance for default of Vertex Buffer Object.</returns>
        private static VertexBufferObject Create()
        {
            var vertexBufferObject = new VertexBufferObject();
            GL.GenBuffers(1, out vertexBufferObject.PositionArrayBufferId);
            GL.GenBuffers(1, out vertexBufferObject.OtherArrayBufferId);
            GL.GenBuffers(1, out vertexBufferObject.ElementArrayBufferId);
            return vertexBufferObject;
        }

        // Methods Public

        public static VertexBufferObject Create(int positionDimensions, VertexAttributes attributes)
        {
            var vertexBufferObject = Create();
            vertexBufferObject.PositionDimensions = positionDimensions;
            vertexBufferObject.VertexAttributes = attributes;
            return vertexBufferObject;
        }
    }
}