using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDrain.IO;

internal static class LevelManager
{
    public static LevelInfo[][] LoadAll()
    {
        return [..AssetManager.ListFiles("Levels")
            .Select(f => { try { return (LevelInfo?)LevelInfo.FromFile(f); } catch { return null; } })
            .Where(l => l != null)
            .Select(l => l!.Value)
            .GroupBy(l => l.Chapter)
            .OrderBy(g => g.Key)
            .Select(g => g.OrderBy(l => l.LevelNum).ToArray())];
    }
}
