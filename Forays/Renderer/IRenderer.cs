using System;

namespace Forays.Renderer
{
    public interface IRenderer
    {
        void WriteString(int x, int y, string text);

        void WriteString(int x, int y, string text, Color foreground);

        void WriteString(int x, int y, string text, Color foreground, Color background);
    }
}