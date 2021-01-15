using System;
using Forays.Scenes;

namespace Forays.Renderer
{
    /// <summary>
    /// The IRenderer has the responsibility of define the method that the
    /// engines must be follow to allow drawn in the window and that the user
    /// can visualize objects, text, figures, images, etc ....  
    /// </summary>
    public interface IRenderer
    {
        bool IsRunning();

        void Draw();

        void Clear();

        void WriteString(int x, int y, string text);

        void WriteString(int x, int y, string text, Color foreground);

        void WriteString(int x, int y, string text, Color foreground, Color background);

        NextScene ProcessInput();
    }
}