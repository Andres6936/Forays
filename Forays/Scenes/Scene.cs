using Forays.Renderer;

namespace Forays.Scenes
{
    public abstract class Scene
    {
        // Members Variables

        /// <summary>
        /// Current instance of renderer engine, current the application support
        /// two renderers: OpenTk (OpenGL) and Console (Terminal).
        /// Only exist a unique instance of this render for all the class that
        /// inheritance of this abstract class.
        /// Is responsibility of Scene Manager that only a scene can be drawn at
        /// a time in the application. 
        /// </summary>
        protected static IRenderer Renderer;
        
        // Static Constructor

        /// <summary>
        /// Initialize the default render to use (aka. OpenTK).
        /// </summary>
        static Scene()
        {
            // The responsibility of initialize Renderer lies with us.
            Renderer = Screen.gl;
        }

        // Methods Public

        public abstract void Draw();

        public abstract void Clear();

        public abstract NextScene ProcessInput();
    }
}