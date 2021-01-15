using Forays.Renderer;

namespace Forays.Scenes
{
    public abstract class Scene
    {
        // Members Variables

        /// <summary>
        /// Current instance of renderer engine, current the application support
        /// two renderers: OpenTk (OpenGL) and Console (Terminal).
        /// 
        /// The first scene instantiated has the responsibility of initialize
        /// the renderer.  
        /// </summary>
        private IRenderer renderer;

        // Methods Public

        public abstract void Draw();

        public abstract void Clear();

        public abstract NextScene ProcessInput();
    }
}