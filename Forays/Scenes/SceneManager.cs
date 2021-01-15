using Forays.Renderer;

namespace Forays.Scenes
{
    public class SceneManager
    {
        // Members Variables

        private bool running = true;

        /// <summary>
        /// A point to current scene.
        /// </summary>
        private IScene current;

        /// <summary>
        /// First scene rendered to execute the app.
        /// </summary>
        private readonly IScene titleScene = new TitleScene();

        // Constructs

        public SceneManager()
        {
            // For default the title scene is the first in be rendered.
            current = titleScene;
        }

        // Methods Public

        public void Draw()
        {
            current.Draw();
        }

        public void Clear()
        {
            current.Clear();
        }

        public void ProcessInput()
        {
            NextScene nextScene = current.ProcessInput();

            switch (nextScene)
            {
                case NextScene.None:
                    // Exit the function if no scene is requested to be rendered.
                    return;
                case NextScene.Exit:
                    running = false;
                    return;
            }
        }

        public bool IsRunning()
        {
            return running;
        }
    }
}