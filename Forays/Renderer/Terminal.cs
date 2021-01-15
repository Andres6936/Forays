using Forays.Scenes;

namespace Forays.Renderer
{
    public class Terminal : IRenderer
    {
        public bool IsRunning()
        {
            throw new System.NotImplementedException();
        }

        public void Draw()
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public void WriteString(int x, int y, string text)
        {
            throw new System.NotImplementedException();
        }

        public void WriteString(int x, int y, string text, Color foreground)
        {
            throw new System.NotImplementedException();
        }

        public void WriteString(int x, int y, string text, Color foreground, Color background)
        {
            throw new System.NotImplementedException();
        }

        public NextScene ProcessInput()
        {
            throw new System.NotImplementedException();
        }
    }
}