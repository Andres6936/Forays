using OpenTK.Graphics.OpenGL;

namespace Forays.Graphics
{
    /// <summary>
    /// A vertex buffer object (VBO) is an OpenGL feature that provides methods
    /// for uploading vertex data (position, normal vector, color, etc.) to the
    /// video device for non-immediate-mode rendering. VBOs offer substantial
    /// performance gains over immediate mode rendering primarily because the
    /// data reside in video device memory rather than system memory and so it
    /// can be rendered directly by the video device. These are equivalent to
    /// vertex buffers in Direct3D.
    ///
    /// The vertex buffer objects (VBO) that can store a large number of
    /// vertices in the GPU's memory. The advantage of using those buffer
    /// objects is that we can send large batches of data all at once to the
    /// graphics card without having to send data a vertex a time. Sending data
    /// to the graphics card from the CPU is relatively slow, so wherever we
    /// can, we try to send as much data as possible at once. Once the data is
    /// in the graphics card's memory the vertex shader has almost instant
    /// access to the vertices making it extremely fast.
    /// </summary>
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
            // The method 'GenBuffers' generates a new VBO and returns its ID
            // number as an unsigned integer. Id 0 is reserved.
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