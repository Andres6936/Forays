namespace Forays.Scenes
{
    public interface IScene
    {
        void Draw();

        void Clear();

        NextScene ProcessInput();
    }
}