using ColorDrain.IO;
using ColorDrain.Maths;
using System.ComponentModel.Design;
using System.Data;

namespace ColorDrain.Game;

internal class Level
{
    private LevelInfo template;
    
    public (int chapter, int level) Meta { get => (template.Chapter, template.LevelNum); }

    public int Width { get => w; }
    public int Height { get => h; }

    public int MoveCount { get => steps.Count; }
    public Coord[] Solution { get => [.. steps]; }
    private List<Coord> steps;

    private bool[,] polarities;
    private SubColor?[,] droplets;
    private List<Element> elements;
    private bool[] walls;
    private bool[,] hasElement;
    private int w, h;

    public (bool, SubColor?, Element?) FieldAt(Coord coord) => (polarities[coord.X, coord.Y], droplets[coord.X, coord.Y], hasElement[coord.X, coord.Y] ? elements.Find(el => el.Position == coord) : null);
    public SubColor? DropletAt(Coord coord) => droplets[coord.X, coord.Y];

    public Level(LevelInfo template)
    {
        this.template = template;
        w = template.Width;
        h = template.Height;

        polarities = new bool[w, h];
        droplets = new SubColor?[w, h];
        hasElement = new bool[w, h];
        elements = [];
        steps = [];
        walls = new bool[w * (h-1) + h * (w-1)];

        Reset();
    }

    public bool GetWall(Coord coord, Direction direction)
    {
        if (direction == Direction.Left)
            return coord.X <= 0 || GetWall((coord.X - 1, coord.Y), Direction.Right);
        if (direction == Direction.Up)
            return coord.Y <= 0 || GetWall((coord.X, coord.Y - 1), Direction.Down);
        if (direction == Direction.Right)
            return coord.X >= w - 1 || walls[coord.Y * (w - 1) + coord.X];
        if (direction == Direction.Down)
            return coord.Y >= h - 1 || walls[h * (w - 1) + coord.Y * w + coord.X];
        return false;
    }

    public void Reset()
    {
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                droplets[x, y] = null;
                polarities[x, y] = false;
                hasElement[x, y] = false;
            }
        elements.Clear();
        steps.Clear();

        foreach (Element el in template.Elements)
        {
            if (el is InitialDroplet d)
                droplets[d.Position.X, d.Position.Y] = d.SColor;
            else if (el is Wall wl)
            {
                walls[(wl.Vertical ? 0 : h * (w - 1)) + wl.Position.Y * (wl.Vertical ? w - 1 : w) + wl.Position.X] = true;
                continue;
            }
            else
            {
                elements.Add(el);
                hasElement[el.Position.X, el.Position.Y] = true;
            }
            polarities[el.Position.X, el.Position.Y] = true;
        }
    }

    public void Undo()
    {
        if (steps.Count == 0) return;
        Coord[] stepsUndone = new Coord[steps.Count - 1];
        steps.CopyTo(0, stepsUndone, 0, steps.Count - 1);
        Reset();
        foreach (var step in stepsUndone)
        {
            SwitchPolarity(step.X, step.Y);
            Step();
        }
    }

    public void Step()
    {
        // Droplet Spreading Behaviour
        bool updated = true;
        while (updated)
        {
            updated = false;
            for (int x = 0; x < w; x++) for (int y = 0; y < h; y++)
                // Empty space
                if (!hasElement[x, y] && polarities[x, y] && !droplets[x, y].HasValue)
                {
                    SubColor? first = null;
                    bool mixed = false;
                    foreach (var (nx, ny) in GetNeighbors((x, y)))
                    {
                        if (droplets[nx, ny].HasValue)
                        {
                            updated = true;
                            // Mix Event
                            if (first != null && first != droplets[nx, ny])
                            {
                                ColorConnectedRegion(x, y);
                                mixed = true;
                                break;
                            }
                            first = droplets[nx, ny];
                        }
                    }
                    if (first != null && !mixed)
                        droplets[x, y] = first;
                }
        }
        // Element Behaviour
        for (int i = 0; i < elements.Count; i++)
        {
            switch (elements[i])
            {
                case Drain d:
                    if (!d.Filled)
                        foreach (var (nx, ny) in GetNeighbors(d.Position))
                            if (droplets[nx, ny].HasValue && droplets[nx, ny]!.Value == d.SColor)
                            {
                                DrainConnectedRegion(nx, ny);
                                elements[i] = d with { Filled = true };
                            }
                    break;
            }
        }
    }

    public IEnumerable<Coord> GetNeighbors(Coord coord)
    {
        List<Coord> neighbors = [];
        if (!GetWall(coord, Direction.Left)) neighbors.Add((coord.X - 1, coord.Y));
        if (!GetWall(coord, Direction.Right)) neighbors.Add((coord.X + 1, coord.Y));
        if (!GetWall(coord, Direction.Up)) neighbors.Add((coord.X, coord.Y - 1));
        if (!GetWall(coord, Direction.Down)) neighbors.Add((coord.X, coord.Y + 1));
        return neighbors;
    }

    private void DrainConnectedRegion(int x, int y)
    {
        Queue<Coord> toCheck = new();
        toCheck.Enqueue((x, y));
        bool[,] visited = new bool[w, h];
        while (toCheck.TryDequeue(out Coord f))
        {
            if (droplets[f.X, f.Y].HasValue)
            {
                droplets[f.X, f.Y] = null;
                visited[f.X, f.Y] = true;
                foreach (var (nx, ny) in GetNeighbors(f))
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
        foreach(var n in GetNeighbors((x, y))) toCheck.Enqueue(n);
        List<Coord> region = [(x, y)];
        HashSet<SubColor> colorsInRegion = [];
        bool[,] visited = new bool[w, h];
        while (toCheck.TryDequeue(out Coord f))
        {
            visited[f.X, f.Y] = true;
            if (droplets[f.X, f.Y].HasValue)
            {
                colorsInRegion.Add(droplets[f.X, f.Y]!.Value);
                region.Add(f);
                foreach (var (nx, ny) in GetNeighbors(f))
                {
                    if (!visited[nx, ny] && polarities[nx, ny])
                        toCheck.Enqueue((nx, ny));
                }
            }
        }
        var newColor = SubColor.Mix(colorsInRegion);
        foreach ((int fx, int fy) in region)
            droplets[fx, fy] = newColor;
    }

    public bool IsLost()
    {
        SubColor[] drains = [..elements.Where(el => el is Drain d && !d.Filled).Cast<Drain>().Select(d => d.SColor)];
        if (drains.Length == 0) return false;
        SubColor[] sources = [..droplets.Cast<SubColor?>().Where(s => s != null).Cast<SubColor>().Distinct()];
        HashSet<SubColor> visited = [];
        Queue<SubColor> queue = new();
        int maxC = drains.Max(c => c.C);
        int maxM = drains.Max(c => c.M);
        int maxY = drains.Max(c => c.Y);
        foreach (var s in sources)
        {
            visited.Add(s);
            queue.Enqueue(s);
        }
        while (queue.TryDequeue(out var s))
        {
            foreach (var e in sources)
            {
                SubColor n = SubColor.Mix(s, e);
                if (n.C > maxC || n.M > maxM || n.Y > maxY) continue;
                if (visited.Add(n))
                    queue.Enqueue(n);
            }
        }
        return !drains.All(visited.Contains);
    }

    public (bool won, Grade grade) GetGrade()
    {
        bool drainsFilled = true;
        bool polesFlipped = true;
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                polesFlipped = polesFlipped && (hasElement[x, y] || !polarities[x, y]);
        foreach (Element el in elements)
            if (el is Drain d) drainsFilled = drainsFilled && d.Filled;
        return (
            drainsFilled,
            MoveCount <= template.MoveThresh.Yel ?
                (polesFlipped ? Grade.Key : Grade.Yellow) :
                MoveCount <= template.MoveThresh.Mag ?
                    Grade.Magenta :
                    Grade.Cyan
        );
    }

    public bool SwitchPolarity(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < w && y < h && !elements.Any(el => el.Position == (x, y)))
        {
            polarities[x, y] = !polarities[x, y];
            droplets[x, y] = null;
            steps.Add((x, y));
            return true;
        }
        return false;
    }
}

enum Direction
{
    Left,
    Right,
    Up,
    Down
}