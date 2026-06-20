using ColorDrain.IO;
using ColorDrain.UI;
using ColorDrain.UI.Controls;
using static ColorDrain.IO.AssetManager;

using Raylib_cs;

namespace ColorDrain.Scenes;

internal class LevelSelect : Scene
{
    LevelInfo[][] levelTable;
    Button buttonNext;
    Button buttonPrev;
    Button buttonBack;

    int currentChapter;

    int cols = 4;

    int levelBoxWidth = 160;
    int levelBoxHeight = 150;
    int gapX = 32;
    int gapY = 100;
    int lineWidth = 5;

    int textSize = 20;

    public LevelSelect()
    {
        levelTable = LevelManager.LoadAll();
        buttonNext = new Button(">", new Rectangle(580, 20, 50, 50));
        buttonPrev = new Button("<", new Rectangle(170, 20, 50, 50))
        {
            Disabled = true
        };
        buttonBack = new Button(T("ui.back"), new Rectangle(20, 20, 100, 50))
        {
            OnClick = () =>
            {
                Runtime.SceneTransition(new Title());
            }
        };

        buttonNext.OnClick = NextPage;
        buttonPrev.OnClick = PrevPage;

        currentChapter = 0;
        solveds = [];
        UpdateSolvedStates();
    }

    int hoveredIndex = -1;

    public void Update()
    {
        buttonPrev.Update();
        buttonNext.Update();
        buttonBack.Update();

        int mx = Raylib.GetMouseX() - gapX;
        int clickedCol = mx / (levelBoxWidth + gapX);
        int my = Raylib.GetMouseY() - gapY - gapY / 2 * (clickedCol % 2);
        int clickedRow = my / (levelBoxHeight + gapY);
        if (
            mx >= 0 && my >= 0 &&
            mx % (levelBoxWidth + gapX) <= levelBoxWidth &&
            my % (levelBoxHeight + gapY) <= levelBoxHeight &&
            clickedRow * cols + clickedCol >= 0 &&
            clickedRow * cols + clickedCol < levelTable[currentChapter].Length
        )
        {
            hoveredIndex = clickedRow * cols + clickedCol;
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                Runtime.SceneTransition(new LevelScene(levelTable[currentChapter][hoveredIndex]));
        }
        else hoveredIndex = -1;
    }

    void NextPage()
    {
        if (currentChapter < levelTable.Length - 1) currentChapter++;
        buttonNext.Disabled = currentChapter == levelTable.Length - 1;
        buttonPrev.Disabled = currentChapter == 0;
        UpdateSolvedStates();
    }

    void PrevPage()
    {
        if (currentChapter > 0) currentChapter--;
        buttonNext.Disabled = currentChapter == levelTable.Length - 1;
        buttonPrev.Disabled = currentChapter == 0;
        UpdateSolvedStates();
    }

    public void Render()
    {
        Raylib.ClearBackground(Color.RayWhite);

        buttonPrev.Render();
        buttonNext.Render();
        buttonBack.Render();

        string chapterTitle = $"{T("ui.chapter")} {currentChapter+1} - {T($"levels.chapter.{currentChapter}")}";
        var m = Raylib.MeasureText(chapterTitle, buttonNext.fontSize);
        Raylib.DrawText(chapterTitle, (Raylib.GetRenderWidth() - m) / 2, 30, buttonNext.fontSize, Color.DarkGray);

        for (int i = 0; i < levelTable[currentChapter].Length; i++)
        {
            int o = gapY + (levelBoxHeight + gapY) * (i / cols) + gapY / 2 * (i % 2);
            int p = gapX + (levelBoxWidth + gapX) * (i % cols);
            Color gradeColor = solveds[i].won ? solveds[i].grade switch
            {
                Grade.Cyan => new Color(0, 255, 255),
                Grade.Magenta => new Color(255, 0, 255),
                Grade.Yellow => new Color(255, 255, 0),
                Grade.Key => Color.Black,
                _ => Color.Gray
            } : Color.Gray;
            if (hoveredIndex == i)
                Raylib.DrawRectangle(p, o, levelBoxWidth, levelBoxHeight, Color.LightGray);
            Raylib.DrawRectangleLinesEx(
                new Rectangle(p, o, levelBoxWidth, levelBoxHeight),
                lineWidth,
                gradeColor
            );
            if (solveds[i].won)
            {
                Raylib.DrawText(T("ui.solved"), p + 10, o + 10, textSize, gradeColor);
                Raylib.DrawText($"{T("ui.grade")}: {T($"ui.grade.{(int)solveds[i].grade}")}", p + 10, o + 12 + textSize, textSize - 2, gradeColor);
            }
            var l = levelTable[currentChapter][i];
            string levelTitle = $"{T($"levels.{l.Chapter}.{l.LevelNum}")}";
            var bm = Raylib.MeasureText(levelTitle, textSize);
            Raylib.DrawText(levelTitle, p + (levelBoxWidth - bm) / 2, o + levelBoxHeight + 10, textSize, Color.DarkGray);
        }
    }

    (bool won, Grade grade)[] solveds;
    void UpdateSolvedStates()
    {
        solveds = [..levelTable[currentChapter]
            .Select(l => Runtime.Save.levelCompletion.FindIndex(c => c.level == l.LevelNum && c.chapter == currentChapter + 1))
            .Select(l => (l != -1, l == -1 ? Grade.Cyan : Runtime.Save.levelCompletion[l].grade))];
    }

    public void Dispose()
    {

    }
}
