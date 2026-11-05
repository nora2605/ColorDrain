using ColorDrain.IO;
using ColorDrain.Maths;

namespace ColorDrain.Game;

public class Solver
{
    private LevelInfo li;

    internal Solver(LevelInfo li)
    {
        this.li = li;
    }

    public Solver(string levelId) : this(LevelInfo.FromFile(AssetManager.GetPath($"Levels/level{levelId}.lvl"))) { }

    internal string Solve(bool key = true, int maxDepth = 20, Coord[]? seed = null)
    {
        var sol = FindSolution(key, maxDepth, seed is null ? [] : seed);
        seed ??= [];
        if (sol.Length == 0)
            return "Out of reach or unsolvable";
        return $"Optimal Solution (Length {sol.Length + seed.Length}): " + string.Join("; ", seed.Concat(sol).Select(x => x.ToString()));
    }

    internal Coord[] FindSolution(bool key, int maxDepth, Coord[] seed)
    {
        int w = li.Width, h = li.Height;

        Level init = new(li);
        foreach (Coord m in seed)
        {
            init.SwitchPolarity(m.X, m.Y);
            init.Step();
        }
        Queue<SearchNode> q = [];
        SearchNode root = new(init, null, default, 0);
        q.Enqueue(root);
        HashSet<string> seen = [];
        seen.Add(State(init));

        List<Coord> moves = [];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (init.FieldAt((x, y)).Item3 is null)
                    moves.Add((x, y));

        int t = 0;

        while (q.TryDequeue(out var c))
        {
            if (c.Depth > maxDepth) return [];
            if (seen.Count > t + 477) Console.Write($"Depth: {c.Depth}, States: {t=seen.Count}\r");

            foreach (Coord m in moves)
            {
                Level n = c.State.Clone();
                if (!n.SwitchPolarity(m.X, m.Y))
                    continue;
                n.Step();
                if (n.IsLost()) continue;
                var g = n.GetGrade();
                if (g.grade == Grade.Key || !key && g.won)
                    return BuildSolution(c, m);
                if (seen.Add(State(n)))
                    q.Enqueue(new(n, c, m, c.Depth + 1));
            }
        }

        return [];
    }

    static string State(Level l)
    {
        string h = "";
        for (int x = 0; x < l.Width; x++)
        {
            for (int y = 0; y < l.Height; y++)
            {
                var (pol, d, e) = l.FieldAt((x, y));
                if (!pol) h += "X";
                else h += (d.HasValue ? d.Value.ToString() : e is null ? "_" : e.ToString());
            }
        }
        return h;
    }

    static Coord[] BuildSolution(SearchNode parent, Coord last)
    {
        Coord[] sol = new Coord[parent.Depth + 1];
        sol[^1] = last;
        SearchNode c = parent;
        for (int i = parent.Depth - 1; i >= 0; i--)
        {
            sol[i] = c.Move;
            c = c.Parent!;
        }
        return sol;
    }

    private record SearchNode(Level State, SearchNode? Parent, Coord Move, int Depth);

}
