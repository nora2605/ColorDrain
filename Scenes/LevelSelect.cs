using ColorDrain.IO;
using ColorDrain.UI;
using ColorDrain.UI.Controls;

using Raylib_cs;

namespace ColorDrain.Scenes;

internal class LevelSelect : Scene
{
    LevelInfo[][] levelTable;
    Button buttonNext;
    Button buttonPrev;

    int currentChapter;
    string[] chapterTitles = [
        "The Stold Lab",
        "idk"
    ];

    int levelBoxWidth = 120;
    int levelBoxHeight = 200;
    int offsetX = 33;
    int offsetY = 100;
    int lineWidth = 5;

    public LevelSelect()
    {
        levelTable = LevelManager.LoadAll();
        buttonNext = new Button(">", 730, 20, 50, 50);
        buttonPrev = new Button("<", 20, 20, 50, 50);

        currentChapter = 0;
        RecalculateLayout();
    }

    public void Update()
    {
        if (Raylib.IsWindowResized())
            RecalculateLayout();

        buttonPrev.Update();
        buttonNext.Update();

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            int mx = Raylib.GetMouseX();
            int my = Raylib.GetMouseY();
            int clickedCol = (mx - offsetX) / (levelBoxWidth + offsetX);
            int clickedRow = (my - offsetY) / (levelBoxHeight + offsetY);
            if (
                mx > offsetX && my > offsetY &&
                (mx - offsetX) % (levelBoxWidth + offsetX) <= levelBoxWidth &&
                (my - offsetY) % (levelBoxHeight + offsetY) <= levelBoxHeight &&
                clickedRow * 5 + clickedCol >= 0 &&
                clickedRow * 5 + clickedCol < levelTable[currentChapter].Length
            )
            {
                Runtime.SceneTransition(new LevelScene(levelTable[currentChapter][clickedRow * 5 + clickedCol]));
            }
        }
    }

    void RecalculateLayout()
    {
        int w_width = Raylib.GetRenderWidth();
        int w_height = Raylib.GetRenderHeight();
        int w_ = Math.Min(w_width, w_height);

        buttonPrev.Height = buttonPrev.Width = buttonNext.Height = buttonNext.Width = w_ / 12;
        buttonPrev.fontSize = buttonNext.fontSize = w_ / 30;
        buttonNext.X = w_width - w_ / 12 - 20;

        lineWidth = w_ / 120;

        levelBoxWidth = 3 * w_width / 20;
        levelBoxHeight = w_height / 4;
        offsetY = w_height / 6;
        offsetX = w_width / 24;
    }

    void NextPage()
    {

    }

    void PrevPage()
    {

    }

    public void Render()
    {
        Raylib.ClearBackground(Color.RayWhite);

        buttonPrev.Render();
        buttonNext.Render();

        string chapterTitle = $"Chapter {currentChapter+1} - {chapterTitles[currentChapter]}";
        var m = Raylib.MeasureText(chapterTitle, buttonNext.fontSize);
        Raylib.DrawText(chapterTitle, (Raylib.GetRenderWidth() - m) / 2, 30, buttonNext.fontSize, Color.DarkGray);

        for (int i = 0; i < levelTable[currentChapter].Length; i++)
        {
            int o = offsetY + (levelBoxHeight + offsetY) * (i / 5);
            int p = offsetX + (levelBoxWidth + offsetX) * (i % 5);
            Raylib.DrawRectangleLinesEx(new Rectangle(p, o, levelBoxWidth, levelBoxHeight), lineWidth, Color.Black);
            var l = levelTable[currentChapter][i];
            string levelTitle = $"{l.LevelNum}: {l.Name}";
            var bm = Raylib.MeasureText(levelTitle, buttonNext.fontSize);
            Raylib.DrawText(levelTitle, p + (levelBoxWidth - bm) / 2, o + levelBoxHeight + 10, buttonNext.fontSize, Color.DarkGray);
        }
    }

    public void Dispose()
    {

    }
}
