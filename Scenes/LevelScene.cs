using Raylib_cs;
using ColorDrain.UI;
using ColorDrain.IO;
using ColorDrain.Maths;
using ColorDrain.Game;
using static ColorDrain.IO.AssetManager;
using ColorDrain.UI.Controls;

namespace ColorDrain.Scenes;

internal class LevelScene : Scene
{
    private Level level;

    private Button buttonReset;
    private Button buttonUndo;
    private Button buttonBack;
    private Button buttonNext;

    List<Control> controls;

    int OFFSET_X = 30;
    int OFFSET_Y = 40;
    int GRID_SIZE = 100;
    float LINE_WIDTH = 5.0f;
    int TEXT_SIZE = 20;

    Texture2D armsCrossed;

    public LevelScene(LevelInfo template)
    {
        level = new(template);
        armsCrossed = Raylib.LoadTexture(GetPath("Sprites/axel.png"));

        int w = level.Width; int h = level.Height;
        int w_width = Raylib.GetRenderWidth();
        int w_height = Raylib.GetRenderHeight();

        GRID_SIZE = Math.Min((int)(2 / 3f * (w_width - 2 * OFFSET_X) / w), (w_height - 2 * OFFSET_Y) / h);
        LINE_WIDTH = GRID_SIZE / 30f;
        Rlgl.SetLineWidth(LINE_WIDTH);

        if (hollowCircle != null)
            Raylib.UnloadRenderTexture(hollowCircle.Value);
        hollowCircle = Raylib.LoadRenderTexture(GRID_SIZE, GRID_SIZE);
        Raylib.BeginTextureMode(hollowCircle.Value);
        Raylib.ClearBackground(Color.Blank);
        Raylib.DrawCircle(GRID_SIZE / 2, GRID_SIZE / 2, GRID_SIZE / 4 + LINE_WIDTH / 2, Color.DarkGray);
        Raylib.BeginBlendMode(BlendMode.SubtractColors);
        Raylib.DrawCircle(GRID_SIZE / 2, GRID_SIZE / 2, GRID_SIZE / 4 - LINE_WIDTH / 2, Color.White);
        Raylib.EndBlendMode();
        Raylib.EndTextureMode();

        int uiRight = GRID_SIZE * w + 3*OFFSET_X/2;

        buttonBack = new(T("ui.back"), new Rectangle(uiRight, 500, 120, 40));
        buttonNext = new(T("ui.next"), new Rectangle(uiRight + 130, 500, 120, 40));
        buttonReset = new(T("ui.reset"), new Rectangle(uiRight, 450, 120, 40));
        buttonUndo = new(T("ui.undo"), new Rectangle(uiRight + 130, 450, 120, 40));

        buttonBack.OnClick = () =>
        {
            Runtime.SceneTransition(new LevelSelect());
        };

        buttonReset.OnClick = level.Reset;
        buttonUndo.OnClick = level.Undo;
        buttonNext.OnClick = () =>
        {
            var previousEntry = Runtime.Save.levelCompletion.FindIndex(c => c.chapter == level.Meta.chapter && c.level == level.Meta.level);
            if (previousEntry != -1)
            {
                var (_, _, sol, gr) = Runtime.Save.levelCompletion[previousEntry];
                if (gr < level.GetGrade().grade || sol.Length < level.MoveCount && gr <= level.GetGrade().grade)
                    Runtime.Save.levelCompletion[previousEntry] = (level.Meta.chapter, level.Meta.level, level.Solution, level.GetGrade().grade);
            }
            else
                Runtime.Save.levelCompletion.Add((level.Meta.chapter, level.Meta.level, level.Solution, level.GetGrade().grade));

            Runtime.Save.Save();
            // TODO: Just go to next level
            Runtime.SceneTransition(new LevelSelect());
        };

        buttonNext.Disabled = true;

        controls = [
            buttonBack,
            buttonNext,
            buttonReset,
            buttonUndo
        ];
    }

    public void Update()
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            var pos = Raylib.GetMousePosition();
            int x = (int)Math.Floor((pos.X - OFFSET_X) / GRID_SIZE);
            int y = (int)Math.Floor((pos.Y - OFFSET_Y) / GRID_SIZE);
            if (level.SwitchPolarity(x, y))
                level.Step();
        }
        char keycode = (char)Raylib.GetCharPressed();
        while (keycode > 0)
        {
            switch (keycode)
            {
                case 'R':
                case 'r':
                    level.Reset();
                    break;
                case 'Z':
                case 'z':
                    level.Undo();
                    break;
            }
            keycode = (char)Raylib.GetCharPressed();
        }

        foreach (Control c in controls)
            c.Update();
    }

    RenderTexture2D? hollowCircle = null;

    public void Render()
    {
        Raylib.ClearBackground(Color.RayWhite);

        int w = level.Width;
        int h = level.Height;

        Raylib.DrawText($"{T("ui.level")} {level.Meta.chapter}-{level.Meta.level}: {T($"levels.{level.Meta.chapter}.{level.Meta.level}")}", OFFSET_X, 12, TEXT_SIZE, Color.DarkGray);
        Raylib.DrawText($"{T("ui.moves")}: {level.MoveCount}", 2 * OFFSET_X + w * GRID_SIZE, OFFSET_Y, TEXT_SIZE, Color.DarkGray);
        
        Raylib.DrawRectangleLinesEx(new Rectangle(OFFSET_X, OFFSET_Y, GRID_SIZE * w, GRID_SIZE * h), LINE_WIDTH, Color.Black);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                (bool polarity, SubColor? droplet, Element? element) field = level.FieldAt((x, y));
                int cx = OFFSET_X + x * GRID_SIZE;
                int cy = OFFSET_Y + y * GRID_SIZE;
                if (!field.polarity) Raylib.DrawTexture(hollowCircle!.Value.Texture, cx, cy, Color.White);
                if (field.droplet.HasValue)
                {
                    Raylib.DrawCircle(cx + GRID_SIZE/2, cy + GRID_SIZE/2, GRID_SIZE/4, field.droplet.Value);
                    foreach (var (nx, ny) in level.GetNeighbors((x, y)))
                    {
                        var n = level.DropletAt((nx, ny));
                        if (n.HasValue && n.Value == field.droplet.Value)
                        {
                            if (x != nx)
                                Raylib.DrawRectangle(OFFSET_X + Math.Min(x, nx) * GRID_SIZE + GRID_SIZE/2, cy + GRID_SIZE/4, GRID_SIZE, GRID_SIZE/2, n.Value);
                            else
                                Raylib.DrawRectangle(cx + GRID_SIZE/4, OFFSET_Y + Math.Min(y, ny) * GRID_SIZE + GRID_SIZE/2, GRID_SIZE/2, GRID_SIZE, n.Value);
                        }
                    }
                }
                if (x < w - 1 && level.GetWall((x, y), Direction.Right))
                    Raylib.DrawLine(cx + GRID_SIZE, cy, cx + GRID_SIZE, cy + GRID_SIZE, Color.Black);
                if (y < h - 1 && level.GetWall((x, y), Direction.Down))
                    Raylib.DrawLine(cx, cy + GRID_SIZE, cx + GRID_SIZE, cy + GRID_SIZE, Color.Black);

                if (field.element == null) continue;
                switch (field.element)
                {
                    case Drain d:
                        Rectangle r = new(cx + GRID_SIZE / 4, cy + GRID_SIZE / 4, GRID_SIZE / 2, GRID_SIZE / 2);
                        if (d.Filled)
                            Raylib.DrawRectangleRec(r, d.SColor);
                        else
                            Raylib.DrawRectangleLinesEx(r, LINE_WIDTH, d.SColor);
                        break;
                }
            }
        }

        for (int x = 0; x < w - 1; x++)
        {
            for (int y = 0; y < h - 1; y++)
            {
                Coord[] bordering = [(x, y), (x + 1, y), (x, y + 1), (x + 1, y + 1)];
                int cx = OFFSET_X + x * GRID_SIZE;
                int cy = OFFSET_Y + y * GRID_SIZE;
                if (bordering.Select(c => {
                    var d = level.DropletAt(c);
                    return (same: d.HasValue, droplet: d);
                }).Aggregate((c, a) => (c.same && c.droplet == a.droplet, a.droplet)).same &&
                    !level.GetWall((x, y), Direction.Right) &&
                    !level.GetWall((x, y), Direction.Down) &&
                    !level.GetWall((x + 1, y), Direction.Down) &&
                    !level.GetWall((x, y + 1), Direction.Right)
                )
                    Raylib.DrawRectangle(cx + GRID_SIZE/2, cy + GRID_SIZE/2, GRID_SIZE, GRID_SIZE, level.DropletAt(bordering[0])!.Value);
            }
        }

        var grade = level.GetGrade();
        if (grade.won)
        {
            buttonNext.Disabled = false;
            Raylib.DrawText($"{T("ui.win")} {T("ui.grade")}: {T($"ui.grade.{(int)grade.grade}")}", 2 * OFFSET_X + w * GRID_SIZE, 420, 20, Color.Black);
        }

        if (level.IsLost())
            Raylib.DrawText(T("ui.softlock"), 2 * OFFSET_X + w * GRID_SIZE, 420, 20, Color.Black);

        Raylib.DrawTexturePro(armsCrossed,
            new Rectangle(0, 0, armsCrossed.Width, armsCrossed.Height),
            new Rectangle(2 * OFFSET_X + w * GRID_SIZE, 2*OFFSET_Y, 200, 300),
            System.Numerics.Vector2.Zero, 0, Color.White
        );

        foreach (Control c in controls)
            c.Render();
    }

    public void Dispose()
    {
        if (hollowCircle != null)
            Raylib.UnloadRenderTexture(hollowCircle.Value);
    }
}
