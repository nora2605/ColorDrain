using ColorDrain.IO;
using ColorDrain.UI;
using ColorDrain.UI.Controls;
using Raylib_cs;
using System.Numerics;
using static ColorDrain.IO.AssetManager;

namespace ColorDrain.Scenes;

internal class Title : Scene
{
    Texture2D bg;
    Music bgm;
    List<Control> mainControls;
    List<Control> settingControls;

    bool settingsShown = false;

    public Title()
    {
        bg = Raylib.LoadTexture(GetPath("Backgrounds/title.png"));

        bgm = Raylib.LoadMusicStream(GetPath("BGM/themesong.qoa"));
        bgm.Looping = true;
        Raylib.PlayMusicStream(bgm);

        var buttonPlay = new Button(T("ui.play"), new Rectangle(450, 200, 250, 80));
        var buttonSettings = new Button(T("ui.settings"), new Rectangle(450, 300, 250, 80));
        var buttonExit = new Button(T("ui.exit"), new Rectangle(450, 400, 250, 80));
        mainControls = [
            buttonPlay,
            buttonSettings,
            buttonExit
        ];

        var buttonLang = new Button($"{T("ui.language")}: {Runtime.Save.language}", new Rectangle(450, 300, 250, 80));
        var buttonBack = new Button(T("ui.back"), new Rectangle(450, 400, 250, 80));
        var buttonDelete = new Button(T("ui.deletesave"), new Rectangle(450, 200, 250, 80));

        settingControls = [
            buttonLang,
            buttonBack,
            buttonDelete
        ];

        buttonPlay.OnClick = () =>
        {
            // very awesome way to do this
            if (Runtime.Save.intern == "Beatrice Shunt")
                Runtime.SceneTransition(new NewSaveScene());
            else Runtime.SceneTransition(new LevelSelect());
        };

        buttonSettings.OnClick = () =>
        {
            settingsShown = true;
        };

        string[] availableLanguages = [..Directory.GetFileSystemEntries(GetPath("Languages"), "", SearchOption.TopDirectoryOnly).Select(e => Path.GetFileName(e)!)];
        int selectedLanguage = availableLanguages.IndexOf(Runtime.Save.language);
        if (selectedLanguage == -1) selectedLanguage = 0;
        buttonLang.OnClick = () =>
        {
            selectedLanguage = (selectedLanguage + 1) % availableLanguages.Length;
            buttonLang.Text = $"{T("ui.language")}: {availableLanguages[selectedLanguage]}";
        };

        buttonBack.OnClick = () =>
        {
            // w translation loading method
            Runtime.Save.language = availableLanguages[selectedLanguage];
            LoadLanguage(Runtime.Save.language);
            Runtime.SceneTransition(new Title());
        };

        buttonDelete.OnClick = () =>
        {
            Runtime.Save.Delete();
            Runtime.Save.Load();
        };

        buttonExit.OnClick = () =>
        {
            Runtime.Exit();
        };
    }

    public void Update()
    {
        Raylib.UpdateMusicStream(bgm);
        if (!settingsShown)
            foreach (Control c in mainControls)
                c.Update();
        else
            foreach (Control c in settingControls)
                c.Update();
    }

    public void Dispose()
    {
        Raylib.StopMusicStream(bgm);
        Raylib.UnloadMusicStream(bgm);
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

        if (!settingsShown)
            foreach (Control cm in mainControls)
                cm.Render();
        else
            foreach (Control cs in settingControls)
                cs.Render();
    }
}
