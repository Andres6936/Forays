using Forays.Input;
using GLDrawing;

namespace Forays.Scenes
{
    public class TitleScene : Scene
    {
        // Member Variables

        private Surface logo;

        // Construct

        public TitleScene()
        {
            const int logoW = 512;
            const int logoH = 412;
            logo = Surface.Create(Screen.gl, "logo.png",
                true,
                Shader.DefaultFS(), false, 2);
            logo.Texture.Sprite.Add(new SpriteType(logoW, logo.Texture.TextureWidthPx));
            logo.Layouts.Add(new CellLayout(1, logoH, logoW,
                (Screen.gl.ClientRectangle.Height - logoH) / 16,
                (Screen.gl.ClientRectangle.Width - logoW) / 2));
            logo.SetEasyLayoutCounts(1);
            logo.SetDefaultSpriteType(0);
            logo.SetDefaultSprite(0);
            logo.DefaultUpdate();
        }

        // Methods Public

        public override void Draw()
        {
            Screen.WriteString(Global.SCREEN_H - 2, Global.SCREEN_W - 14, "Version " + Global.VERSION, Color.DarkGray);
            Screen.WriteString(Global.SCREEN_H - 1, Global.SCREEN_W - 19, "By Derrick Creamer ", Color.DarkGray);
            Screen.WriteString(Global.SCREEN_H - 1, 1, "Logo by Soundlust", Color.DarkerGray);

            Screen.GLUpdate();
        }

        public override void Clear()
        {
        }

        public override NextScene ProcessInput()
        {
            KeyCode keyCode = Screen.GetKeyPressed().GetKeyCode();

            // Wait to user press a key
            while (keyCode.Equals(KeyCode.None))
            {
                keyCode = Screen.GetKeyPressed().GetKeyCode();
            }
            
            Screen.gl.Surfaces.Remove(logo);
            return NextScene.Play;
        }
    }
}