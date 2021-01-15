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
        private Scene current;

        private Scene playScene;

        private Scene gameOverScene;

        /// <summary>
        /// First scene rendered to execute the app.
        /// </summary>
        private readonly Scene titleScene = new TitleScene();

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
                case NextScene.Play:
                    // Lazy initialization of object
                    if (playScene == null)
                    {
                        playScene = new PlayScene();
                    }

                    // A point to play scene.
                    current = playScene;
                    return;
                case NextScene.GameOver:
                    // Lazy initialization of object
                    if (gameOverScene == null)
                    {
                        gameOverScene = new GameOverScene();
                    }

                    // A point to game over scene.
                    current = gameOverScene;
                    return;
                case NextScene.Exit:
                    // The user wanna exit of application.
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