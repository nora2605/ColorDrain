using ColorDrain.UI;
using ColorDrain.UI.Controls;
using Raylib_cs;
using static ColorDrain.IO.AssetManager;

namespace ColorDrain.Scenes;

internal class NewSaveScene : Scene
{
    Texture2D bg;
    Input nameInput;
    Button buttonSubmit;

    public NewSaveScene()
    {
        bg = Raylib.LoadTexture(GetPath("Backgrounds/new save.png"));

        nameInput = new(T("ui.yourname"), "", new Rectangle(300, 480, 200, 50))
        {
            Centered = true
        };

        buttonSubmit = new(T("ui.submit"), new Rectangle(350, 540, 100, 50))
        {
            Disabled = true
        };

        nameInput.OnChange = () =>
        {
            buttonSubmit.Disabled = string.IsNullOrWhiteSpace(nameInput.Value);
        };

        nameInput.OnSubmit = Submit;
        buttonSubmit.OnClick = Submit;
    }

    void Submit()
    {
        switch (new string([.. nameInput.Value.ToLower().Where(char.IsAsciiLetter)]))
        {
            case "beatriceshunt":
            case "axelprink":
            case "mrolovich":
            case "mistorolovich":
            case "iristau":
            case "kikitau":
                // Special Dialogue Scene
                break;
            default:
                Runtime.Save.intern = nameInput.Value;
                Runtime.Save.Save();
                Runtime.SceneTransition(new LevelSelect());
                break;
        }
    }

    public void Update()
    {
        nameInput.Update();
        buttonSubmit.Update();
    }

    public void Render()
    {
        Raylib.DrawTexture(bg, 0, 0, Color.White);

        int t = Raylib.MeasureText(T("ui.cdapp"), 20);
        Raylib.DrawText(T("ui.cdapp"), 400 - t / 2, 450, 20, Color.Black);

        if (nameInput.Value.Equals("maya", StringComparison.CurrentCultureIgnoreCase))
        {
            Raylib.DrawText(T("ui.meow"), 488, 128, 40, Color.Black);
            Raylib.DrawText(T("ui.meow"), 490, 130, 40, Color.Green);
        }
        nameInput.Render();
        buttonSubmit.Render();
    }

    public void Dispose()
    {

    }
}
