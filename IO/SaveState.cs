using ColorDrain.Maths;

namespace ColorDrain.IO;

internal class SaveState
{
    public List<(int chapter, int level, Coord[] solution, Grade grade)> levelCompletion = [];
    public string language = "en";
    // #easteregg moments
    public string intern = "Beatrice Shunt";

    private string SavePath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ColorDrain/save.dat"); }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
            new SaveState().Save();
        }
        string[] lines = File.ReadAllLines(SavePath);
        intern = lines[0];
        language = lines[1];
        levelCompletion = [.. lines
            .Skip(2)
            .Select(l => l.Split(":"))
            .Select(l => (
                int.Parse(l[0]),
                int.Parse(l[1]),
                l[2]
                    .Split(';')
                    .Select(c => c[1..^1].Split(", ").Select(int.Parse))
                    .Select(c => new Coord(c.First(), c.Last()))
                    .ToArray(),
                (Grade)int.Parse(l[3])
            ))];
    }

    public void Save()
    {
        List<string> lines = [];
        lines.Add(intern);
        lines.Add(language);
        lines.AddRange(levelCompletion.Select(l => $"{l.chapter}:{l.level}:{string.Join(";", l.solution)}:{(int)l.grade}"));
        File.WriteAllLines(SavePath, [..lines]);
    }

    public void Delete()
    {
        File.Delete(SavePath);
    }
}