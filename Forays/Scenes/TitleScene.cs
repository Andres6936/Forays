using GLDrawing;

namespace Forays.Scenes
{
    public class TitleScene : IScene
    {
        // Member Variables

        private Surface logo;

        // Construct

        public TitleScene()
        {
            const int logoW = 512;
            const int logoH = 412;
            logo = Surface.Create(Screen.gl, Global.ForaysImageResources + "logo.png",
                true,
                Shader.DefaultFS(), false, 2);
            SpriteType.DefineSingleRowSprite(logo, logoW);
            CellLayout.CreateGrid(logo, 1, 1, logoH, logoW,
                (Screen.gl.ClientRectangle.Height - logoH) / 16,
                (Screen.gl.ClientRectangle.Width - logoW) / 2);
            logo.SetEasyLayoutCounts(1);
            logo.SetDefaultSpriteType(0);
            logo.SetDefaultSprite(0);
            logo.DefaultUpdate();
        }

        // Methods Public

        public void Draw()
        {
            if (Screen.GLMode)
            {
                Screen.WriteString(Global.SCREEN_H - 2, Global.SCREEN_W - 14,
                    "version " + Global.VERSION + " ",
                    Color.DarkGray);
                Screen.WriteString(Global.SCREEN_H - 1, Global.SCREEN_W - 19, "by Derrick Creamer ",
                    Color.DarkGray);
                Screen.WriteString(Global.SCREEN_H - 1, 0, "logo by Soundlust", Color.DarkerGray);
            }
            else
            {
                for (int i = 0; i < Global.title[0].GetLength(0); ++i)
                {
                    for (int j = 0; j < Global.title[0][0].Length; ++j)
                    {
                        if (Global.title[0][i][j] != ' ')
                        {
                            const int row_offset = 4;
                            const int col_offset = 19;
                            if (Global.title[0][i][j] == '#' && (!Global.LINUX || Screen.GLMode))
                            {
                                Screen.WriteChar(i + row_offset, j + col_offset, ' ', Color.Black,
                                    Color.Yellow);
                            }
                            else
                            {
                                Screen.WriteChar(i + row_offset, j + col_offset,
                                    Global.title[0][i][j], Color.Yellow);
                            }
                        }
                    }
                }

                for (int i = 0; i < Global.title[1].GetLength(0); ++i)
                {
                    for (int j = 0; j < Global.title[1][0].Length; ++j)
                    {
                        Screen.WriteChar(i + 19, j + 37, Global.title[1][i][j], Color.Green);
                    }
                }

                Screen.WriteString(Global.SCREEN_H - 3, Global.SCREEN_W - 14,
                    "version " + Global.VERSION + " ",
                    Color.DarkGray);
                Screen.WriteString(Global.SCREEN_H - 2, Global.SCREEN_W - 19, "by Derrick Creamer ",
                    Color.DarkGray);
                Input.ReadKey(false);
            }
        }

        public void Clear()
        {
        }

        public NextScene ProcessInput()
        {
            Input.ReadKey(false);
            Screen.gl.Surfaces.Remove(logo);
            return NextScene.Play;
        }
    }
}