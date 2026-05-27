using Raylib_cs;
using SubColor = (int C, int M, int Y);
using Coord = (int x, int y);

namespace ColorDrain;
internal class Level
{

    private Element[,] field;
    private bool[,] polarities;
    private int w, h;

    const int OFFSET_X = 50;
    const int OFFSET_Y = 50;
    const int GRID_SIZE = 100;
    const float LINE_STRENGTH = 5.0f;

    private int movesMade = 0;

    public Level()
    {
        w = 4; h = 4;
        field = new Element[w, h];
        polarities = new bool[w, h];
        Reset();
    }

    private void InitField(int x, int y, bool drain, SubColor color)
    {
        polarities[x, y] = true;
        field[x, y] = new Element(!drain, drain, color);
    }

    private void Reset()
    {
        field = new Element[w, h];
        polarities = new bool[w, h];
        movesMade = 0;

        InitField(0, 0, true, (1, 1, 0));
        InitField(1, 0, true, (1, 2, 1));
        InitField(2, 0, true, (0, 1, 1));
        InitField(3, 0, true, (0, 0, 1));
        InitField(1, 2, false, (0, 1, 0));
        InitField(3, 2, false, (1, 0, 0));
        InitField(2, 3, false, (0, 0, 1));
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
                movesMade++;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.R))
            Reset();

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
                    foreach (var (nx, ny) in GetNeighbors(x, y))
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
            if (field[f.x, f.y].Filled && !field[f.x, f.y].Drain)
            {
                field[f.x, f.y] = Element.Empty;
                visited[f.x, f.y] = true;
                foreach (var (nx, ny) in GetNeighbors(f.x, f.y))
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
            visited[f.x, f.y] = true;
            Element el = field[f.x, f.y];
            if (el.Filled && !el.Drain)
            {
                colorsInRegion.Add(el.SColor);
                region.Add(f);
                foreach (var (nx, ny) in GetNeighbors(f.x, f.y))
                {
                    if (!visited[nx, ny] && polarities[nx, ny])
                        toCheck.Enqueue((nx, ny));
                }
            }
        }
        SubColor newColor = Element.Mix(colorsInRegion);
        foreach ((int fx, int fy) in region)
            field[fx, fy] = new Element(true, false, newColor);
    }

    private List<Coord> GetNeighbors(int x, int y)
    {
        List<Coord> neighbors = [];
        if (x > 0) neighbors.Add((x - 1, y));
        if (x < w - 1) neighbors.Add((x + 1, y));
        if (y > 0) neighbors.Add((x, y - 1));
        if (y < h - 1) neighbors.Add((x, y + 1));
        return neighbors;
    }

    public bool CheckWin() => field.Cast<Element>().All(e => e.Drain == e.Filled);

    public void Render()
    {
        Raylib.DrawText($"Moves: {movesMade}", 12, 12, 20, Color.DarkGray);
        Raylib.DrawRectangleLinesEx(new Rectangle(OFFSET_X, OFFSET_Y, GRID_SIZE * w, GRID_SIZE * h), LINE_STRENGTH, Color.Black);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int cx = OFFSET_X + x * GRID_SIZE;
                int cy = OFFSET_Y + y * GRID_SIZE;
                bool pol = polarities[x, y];
                if (!pol) Raylib.DrawCircleLines(cx + GRID_SIZE / 2, cy + GRID_SIZE / 2, GRID_SIZE/4, Color.Gray);
                Element el = field[x, y];
                if (el.Drain)
                {
                    Rectangle r = new(cx + GRID_SIZE/4, cy + GRID_SIZE/4, GRID_SIZE/2, GRID_SIZE/2);
                    if (el.Filled)
                        Raylib.DrawRectangleRec(r, el.GetRGB());
                    else
                        Raylib.DrawRectangleLinesEx(r, LINE_STRENGTH, el.GetRGB());
                }
                else if (el.Filled)
                {
                    Raylib.DrawCircle(cx + GRID_SIZE/2, cy + GRID_SIZE/2, GRID_SIZE/4, el.GetRGB());
                    foreach (var (nx, ny) in GetNeighbors(x, y))
                    {
                        Element n = field[nx, ny];
                        if (n.SColor == el.SColor && !n.Drain)
                        {
                            if (x != nx)
                                Raylib.DrawRectangle(OFFSET_X + Math.Min(x, nx) * GRID_SIZE + GRID_SIZE/2, cy + GRID_SIZE/4, GRID_SIZE, GRID_SIZE/2, el.GetRGB());
                            else
                                Raylib.DrawRectangle(cx + GRID_SIZE/4, OFFSET_Y + Math.Min(y, ny) * GRID_SIZE + GRID_SIZE/2, GRID_SIZE/2, GRID_SIZE, el.GetRGB());
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
                if (bordering.Select(c => (same: true, el: field[c.x, c.y])).Aggregate((c, a) => (c.same && c.el.SColor == a.el.SColor, a.el)).same)
                {
                    Element el = field[x, y];
                    if (el.Filled && !el.Drain)
                        Raylib.DrawRectangle(cx + GRID_SIZE/2, cy + GRID_SIZE/2, GRID_SIZE, GRID_SIZE, el.GetRGB());
                }
                // else Raylib.DrawCircle((int)cx + (int)GRID_SIZE, (int)cy + (int)GRID_SIZE, LINE_STRENGTH, Color.Gray);
            }
        }

        if (CheckWin())
        {
            Raylib.DrawText("You win!", OFFSET_X + w * GRID_SIZE + 20, OFFSET_Y + 20, 20, Color.Black);
        }
    }
}

struct Element(bool Filled, bool Drain, (int C, int M, int Y) CMY)
{
    public bool Filled { get; set; } = Filled;
    public bool Drain { get; } = Drain;
    public SubColor SColor { get; } = CMY;

    public readonly Color GetRGB()
    {
        float max = ((int[])[SColor.C, SColor.M, SColor.Y, 1]).Max();
        return new Color(
            1f - (SColor.C / max),
            1f - (SColor.M / max),
            1f - (SColor.Y / max)
        );
    }

    public static SubColor Mix(IEnumerable<SubColor> a) => Normalize(a.Sum(e => e.C), a.Sum(e => e.M), a.Sum(e => e.Y));

    private static SubColor Normalize(int C, int M, int Y)
    {
        int gcd = GCD(GCD(C, M), Y);
        return (C / gcd, M / gcd, Y / gcd);
    }

    private static int GCD(int a, int b)
    {
        if (b == 0) return a;
        return GCD(b, a % b);
    }

    public static Element Empty = new(false, false, (0, 0, 0));
}