using ColorDrain.IO;
using ColorDrain.UI;
using ColorDrain.UI.Controls;
using Raylib_cs;
using System.Numerics;

namespace ColorDrain.Scenes;

internal class Title : Scene
{
    Texture2D bg;
    Music bgm;
    List<Control> controls;
    Button buttonPlay;
    Button buttonSettings;
    Button buttonExit;

    public Title()
    {
        bg = Raylib.LoadTexture(AssetManager.GetPath("Backgrounds/title.png"));

        bgm = Raylib.LoadMusicStream(AssetManager.GetPath("BGM/themesong.qoa"));
        bgm.Looping = true;
        Raylib.PlayMusicStream(bgm);

        buttonPlay = new Button("Play", 450, 200, 250, 80);
        buttonSettings = new Button("Settings", 450, 300, 250, 80);
        buttonExit = new Button("Exit", 450, 400, 250, 80);
        controls = [
            buttonPlay,
            buttonSettings,
            buttonExit
        ];

        buttonPlay.OnClick = () =>
        {
            Runtime.SceneTransition(new LevelSelect());
        };

        buttonExit.OnClick = () =>
        {
            Runtime.Exit();
        };
    }

    public void Update()
    {
        Raylib.UpdateMusicStream(bgm);
        foreach (Control c in controls)
            c.Update();
        if (Raylib.IsWindowResized())
            RecalculateLayout();
    }

    public void Dispose()
    {
        Raylib.StopMusicStream(bgm);
        Raylib.UnloadMusicStream(bgm);
    }

    public void RecalculateLayout()
    {
        int w_width = Raylib.GetRenderWidth();
        int w_height = Raylib.GetRenderHeight();
        buttonPlay.Y = w_height / 3;
        buttonSettings.Y = w_height / 2;
        buttonExit.Y = 2 * w_height / 3;
        foreach (Control c in controls)
        {
            ((Button)c).X = 9 * w_width / 16;
            ((Button)c).Width = 5 * w_width / 16;
            ((Button)c).Height = 2 * w_height / 15;
            ((Button)c).fontSize = w_height / 30;
        }
    }

    public void Render()
    {
        Raylib.DrawTexturePro(
            bg,
            new Rectangle(0, 0, 800, 600),
            new Rectangle(0, 0, Raylib.GetRenderWidth(), Raylib.GetRenderHeight()),
            new Vector2(0, 0),
            0f,
            Color.White
        );

        foreach (Control c in controls)
            c.Render();
    }
}
