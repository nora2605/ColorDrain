using ColorDrain.Objects;
using Raylib_cs;

namespace ColorDrain.IO;

internal struct LevelInfo
{
    public int Chapter { get; private set; }
    public int LevelNum { get; private set; }
    public string Name { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public (int Mag, int Yel) MoveThresh { get; private set; }
    public Element[] Elements { get; private set; }
    

    public static LevelInfo FromFile(string path)
    {
        string[] lines = File.ReadAllLines(path);
        string[] lDesc = lines[0].Split(':');
        if (lDesc.Length != 3)
            throw new FormatException($"Invalid Level File: {path}");
        int chapter = int.Parse(lDesc[0]);
        int level = int.Parse(lDesc[1]);
        string name = lDesc[2];
        string[] lSize = lines[1].Split('x');
        int w = int.Parse(lSize[0]);
        int h = int.Parse(lSize[1]);

        string[] lMoves = lines[2].Split(':');
        int mag = int.Parse(lMoves[0]);
        int yel = int.Parse(lMoves[1]);

        Element[] elements = [.. lines.Skip(3).Select(ParseElementDecl)];

        return new LevelInfo()
        {
            Chapter = chapter,
            LevelNum = level,
            Name = name,
            Width = w,
            Height = h,
            MoveThresh = (mag, yel),
            Elements = elements
        };
    }

    private static Element ParseElementDecl(string line)
    {
        string[] tokens = [..line.Replace("(", " ").Replace(")", " ").Replace(",", " ").Split(" ").Where(t => !string.IsNullOrEmpty(t))];
        switch (tokens[0])
        {
            case "Drain":
            {
                int[] args = [..tokens.Skip(1).Select(int.Parse)];
                return new Drain((args[0], args[1]), (args[2], args[3], args[4]), false);
            }
            case "Droplet":
            {
                int[] args = [.. tokens.Skip(1).Select(int.Parse)];
                return new Droplet((args[0], args[1]), (args[2], args[3], args[4]));
            }
            default:
                throw new FormatException($"Unrecognized Element: {tokens[0]}");
        }
    }

    public string[] ToLines()
    {
        List<string> lines = [];
        lines.Add($"{Chapter}:{LevelNum}:{Name}");
        lines.Add($"{Width}x{Height}");
        lines.Add($"{MoveThresh.Mag}:{MoveThresh.Yel}");
        lines.AddRange(Elements.Select(el => $"{el}"));
        return [.. lines];
    }
}

internal enum Grade
{
    Cyan,
    Magenta,
    Yellow,
    Key
}
