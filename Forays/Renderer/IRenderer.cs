using System;
using Forays.Scenes;

namespace Forays.Renderer
{
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