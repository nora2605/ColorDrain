using Raylib_cs;
using ColorDrain.UI;
using ColorDrain.IO;
using ColorDrain.Maths;
using ColorDrain.Game;

namespace ColorDrain.Scenes;

internal class LevelScene : Scene
{
    private Level level;

    private string[] gradeStrings = ["Cyan", "Magenta", "Yellow", "Key"];

    int OFFSET_X = 50;
    int OFFSET_Y = 50;
    int GRID_SIZE = 100;
    float LINE_WIDTH = 5.0f;
    int TEXT_SIZE = 20;

    public LevelScene(LevelInfo template)
    {
        level = new(template);
        RecalculateLayout();
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
                case 'B':
                case 'b':
                    Runtime.SceneTransition(new LevelSelect());
                    break;
            }
            keycode = (char)Raylib.GetCharPressed();
        }
    }

    private void RecalculateLayout()
    {
        int w = level.Width; int h = level.Height;
        if (w == 0 || h == 0) return;
        int w_width = Raylib.GetRenderWidth();
        int w_height = Raylib.GetRenderHeight();

        GRID_SIZE = Math.Min((int)(2/3f * (w_width - 2 * OFFSET_X) / w), (w_height - 2 * OFFSET_Y) / h);
        LINE_WIDTH = GRID_SIZE / 30f;
        TEXT_SIZE = Math.Min(GRID_SIZE / 5, OFFSET_Y - 10);
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
    }

    RenderTexture2D? hollowCircle = null;

    public void Render()
    {
        Raylib.ClearBackground(Color.RayWhite);
        if (Raylib.IsWindowResized())
            RecalculateLayout();

        int w = level.Width;
        int h = level.Height;

        Raylib.DrawText($"Level {level.Meta.chapter}-{level.Meta.level}: {level.Meta.name}", OFFSET_X, 12, TEXT_SIZE, Color.DarkGray);
        Raylib.DrawText($"Moves: {level.MoveCount}", 2 * OFFSET_X + w * GRID_SIZE, 2 * OFFSET_Y, TEXT_SIZE, Color.DarkGray);
        
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
                }).Aggregate((c, a) => (c.same && c.droplet == a.droplet, a.droplet)).same)
                    Raylib.DrawRectangle(cx + GRID_SIZE/2, cy + GRID_SIZE/2, GRID_SIZE, GRID_SIZE, level.DropletAt(bordering[0])!.Value);
            }
        }

        var grade = level.GetGrade();
        if (grade.won)
        {
            if (!written)
            {
                Console.WriteLine(string.Join(";", level.Solution));
                written = true;
            }
            Raylib.DrawText($"You win! Grade: {gradeStrings[(int)grade.grade]}", 2 * OFFSET_X + w * GRID_SIZE, 3*OFFSET_Y, 20, Color.Black);
        }

        if (level.IsLost())
            Raylib.DrawText($"Softlock :/ [R]/[Z]", 2 * OFFSET_X + w * GRID_SIZE, 5 * OFFSET_Y, 20, Color.Black);
    }
    bool written = false;

    public void Dispose()
    {
        if (hollowCircle != null)
            Raylib.UnloadRenderTexture(hollowCircle.Value);
    }
}
