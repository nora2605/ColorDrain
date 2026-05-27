using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDrain.IO;

internal class SaveState
{
    public int[][] levelCompletion = [];

    private string SavePath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ColorDrain/save.dat"); }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
            new SaveState().Save();
        }
        string[] lines = File.ReadAllLines(SavePath);
        int[][] completion = [.. lines.Select(l => l.Split(',').Select(int.Parse).ToArray()).ToArray()];
        levelCompletion = new int[completion.Length][];
        levelCompletion[0] = completion[0];
    }

    public void Save()
    {
        List<string> lines = [];
        lines.AddRange(levelCompletion.Select(l => string.Join(",", l)));
        File.WriteAllLines(SavePath, [..lines]);
    }
}