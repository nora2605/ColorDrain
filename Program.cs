using Raylib_cs;
using ColorDrain.Scenes;
using ColorDrain.IO;
using ColorDrain.UI;

Runtime.Save.Load();
AssetManager.LoadLanguage(Runtime.Save.language);

Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
Raylib.InitWindow(800, 600, "Color Drain");

Raylib.InitAudioDevice();

Raylib.SetTargetFPS(Raylib.GetMonitorRefreshRate(Raylib.GetCurrentMonitor()));
Raylib.SetExitKey(KeyboardKey.Null);

Runtime.CurrentScene = new Title();

while (!Raylib.WindowShouldClose() && !Runtime.ShouldClose)
{
    Runtime.CurrentScene.Update();

    Raylib.BeginDrawing();
    Runtime.CurrentScene.Render();
    Raylib.EndDrawing();
}

Raylib.CloseWindow();

internal static class Runtime
{
    internal static bool ShouldClose { get; private set; } = false;
    internal static Scene? CurrentScene { get; set; }
    internal static SaveState Save { get; } = new SaveState();

    internal static void Exit()
    {
        Save.Save();
        ShouldClose = true;
    }

    internal static void SceneTransition(Scene newScene)
    {
        CurrentScene?.Dispose();
        CurrentScene = newScene;
    }
}