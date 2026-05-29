using Raylib_cs;
using ColorDrain.UI;
using ColorDrain.IO;
using ColorDrain.Maths;
using ColorDrain.Objects;
using System.Numerics;

namespace ColorDrain.Scenes;

internal class Level : Scene
{
    private LevelInfo levelInfo;
    private Element[,] field;
    private bool[,] polarities;
    private int w, h;

    private List<Coord> solution = [];

    int OFFSET_X = 50;
    int OFFSET_Y = 50;
    int GRID_SIZE = 100;
    float LINE_WIDTH = 5.0f;
    int TEXT_SIZE = 20;

    public Level(LevelInfo template)
    {
        levelInfo = template;
        w = levelInfo.Width;
        h = levelInfo.Height;
        field = new Element[w, h];
        polarities = new bool[w, h];
        Init();
        RecalculateLayout();
    }

    private void InitField(int x, int y, bool drain, SubColor color)
    {
        polarities[x, y] = true;
        field[x, y] = new Element(!drain, drain, color);
    }

    private void Init()
    {
        field = new Element[w, h];
        polarities = new bool[w, h];
        solution = [];
        written = false;

        foreach (var el in levelInfo.Elements)
        {
            switch (el)
            {
                case Droplet dr:
                    InitField(dr.Position.X, dr.Position.Y, false, dr.SColor);
                    break;
                case Drain d:
                    InitField(d.Position.X, d.Position.Y, true, d.SColor);
                    break;
            }
        }
    }

    public void Update()
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            var pos = Raylib.GetMousePosition();
            int x = (int)Math.Floor((pos.X - OFFSET_X) / GRID_SIZE);
            int y = (int)Math.Floor((pos.Y - OFFSET_Y) / GRID_SIZE);
            if (x >= 0 && x < w && y >= 0 && y < h && !field[x, y].Drain)
            {
                polarities[x, y] = !polarities[x, y];
                solution.Add((x, y));
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.R))
            Init();

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Element el = field[x, y];
                if (el.Filled && !el.Drain)
                {
                    if (!polarities[x, y])
                    {
                        field[x, y] = Element.Empty;
                        continue;
                    }
                    foreach (var (nx, ny) in new Coord(x, y).GetNeighbors(w, h))
                    {
                        if (!polarities[nx, ny]) continue;
                        Element n = field[nx, ny];
                        if (n.Drain)
                        {
                            if (!n.Filled && el.SColor == n.SColor)
                            {
                                field[nx, ny].Filled = true;
                                DrainConnectedRegion(x, y);
                                break;
                            }
                        }
                        else if (n.Filled)
                        {
                            if (n.SColor != el.SColor)
                                ColorConnectedRegion(x, y);
                        }
                        else field[nx, ny] = new Element(true, false, el.SColor);
                    }
                }
            }
        }
    }

    private void DrainConnectedRegion(int x, int y)
    {
        Queue<Coord> toCheck = new();
        toCheck.Enqueue((x, y));
        bool[,] visited = new bool[w, h];
        while (toCheck.TryDequeue(out Coord f))
        {
            if (field[f.X, f.Y].Filled && !field[f.X, f.Y].Drain)
            {
                field[f.X, f.Y] = Element.Empty;
                visited[f.X, f.Y] = true;
                foreach (var (nx, ny) in f.GetNeighbors(w, h))
                {
                    if (!visited[nx, ny] && polarities[nx, ny])
                        toCheck.Enqueue((nx, ny));
                }
            }
        }
    }

    private void ColorConnectedRegion(int x, int y)
    {
        Queue<Coord> toCheck = new();
        toCheck.Enqueue((x, y));
        List<Coord> region = [(x, y)];
        HashSet<SubColor> colorsInRegion = [];
        bool[,] visited = new bool[w, h];
        while (toCheck.TryDequeue(out Coord f))
        {
            visited[f.X, f.Y] = true;
            Element el = field[f.X, f.Y];
            if (el.Filled && !el.Drain)
            {
                colorsInRegion.Add(el.SColor);
                region.Add(f);
                foreach (var (nx, ny) in f.GetNeighbors(w, h))
                {
                    if (!visited[nx, ny] && polarities[nx, ny])
                        toCheck.Enqueue((nx, ny));
                }
            }
        }
        var newColor = SubColor.Mix(colorsInRegion);
        foreach ((int fx, int fy) in region)
            field[fx, fy] = new Element(true, false, newColor);
    }

    public bool CheckWin() => field.Cast<Element>().All(e => e.Drain == e.Filled);
    public bool CheckKey() => Enumerable.Range(0, w).All(x => Enumerable.Range(0, h).All(y => field[x, y].Drain || !polarities[x, y]));

    private void RecalculateLayout()
    {
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

        Raylib.DrawText($"Level {levelInfo.Chapter}-{levelInfo.LevelNum}: {levelInfo.Name}", OFFSET_X, 12, TEXT_SIZE, Color.DarkGray);
        Raylib.DrawText($"Moves: {solution.Count}", 2 * OFFSET_X + w * GRID_SIZE, 2 * OFFSET_Y, TEXT_SIZE, Color.DarkGray);
        
        Raylib.DrawRectangleLinesEx(new Rectangle(OFFSET_X, OFFSET_Y, GRID_SIZE * w, GRID_SIZE * h), LINE_WIDTH, Color.Black);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int cx = OFFSET_X + x * GRID_SIZE;
                int cy = OFFSET_Y + y * GRID_SIZE;
                bool pol = polarities[x, y];
                if (!pol) Raylib.DrawTexture(hollowCircle!.Value.Texture, cx, cy, Color.White);
                Element el = field[x, y];
                if (el.Drain)
                {
                    Rectangle r = new(cx + GRID_SIZE/4, cy + GRID_SIZE/4, GRID_SIZE/2, GRID_SIZE/2);
                    if (el.Filled)
                        Raylib.DrawRectangleRec(r, el.SColor);
                    else
                        Raylib.DrawRectangleLinesEx(r, LINE_WIDTH, el.SColor);
                }
                else if (el.Filled)
                {
                    Raylib.DrawCircle(cx + GRID_SIZE/2, cy + GRID_SIZE/2, GRID_SIZE/4, el.SColor);
                    foreach (var (nx, ny) in new Coord(x, y).GetNeighbors(w, h))
                    {
                        Element n = field[nx, ny];
                        if (n.SColor == el.SColor && !n.Drain)
                        {
                            if (x != nx)
                                Raylib.DrawRectangle(OFFSET_X + Math.Min(x, nx) * GRID_SIZE + GRID_SIZE/2, cy + GRID_SIZE/4, GRID_SIZE, GRID_SIZE/2, el.SColor);
                            else
                                Raylib.DrawRectangle(cx + GRID_SIZE/4, OFFSET_Y + Math.Min(y, ny) * GRID_SIZE + GRID_SIZE/2, GRID_SIZE/2, GRID_SIZE, el.SColor);
                        }
                    }
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
                if (bordering.Select(c => (same: true, el: field[c.X, c.Y])).Aggregate((c, a) => (c.same && c.el.SColor == a.el.SColor, a.el)).same)
                {
                    Element el = field[x, y];
                    if (el.Filled && !el.Drain)
                        Raylib.DrawRectangle(cx + GRID_SIZE/2, cy + GRID_SIZE/2, GRID_SIZE, GRID_SIZE, el.SColor);
                }
            }
        }

        if (CheckWin())
        {
            if (!written)
            {
                Console.WriteLine(string.Join(";", solution));
                written = true;
            }
            Raylib.DrawText($"You win! Grade: {(solution.Count <= levelInfo.MoveThresh.Yel ? CheckKey() ? "Key" : "Yellow" : solution.Count <= levelInfo.MoveThresh.Mag ? "Magenta" : "Cyan")}", 2 * OFFSET_X + w * GRID_SIZE, 3*OFFSET_Y, 20, Color.Black);
        }
    }
    bool written = false;

    public void Dispose()
    {
        if (hollowCircle != null)
            Raylib.UnloadRenderTexture(hollowCircle.Value);
    }
}

struct Element(bool Filled, bool Drain, (int C, int M, int Y) CMY)
{
    public bool Filled { get; set; } = Filled;
    public bool Drain { get; } = Drain;
    public SubColor SColor { get; } = CMY;

    public static Element Empty = new(false, false, (0, 0, 0));
}