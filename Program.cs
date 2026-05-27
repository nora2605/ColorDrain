using Raylib_cs;
using ColorDrain;

Level l = new();

Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.TopmostWindow);
Raylib.InitWindow(900, 900, "Color Drain");

while (!Raylib.WindowShouldClose())
{
    l.Update();

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.RayWhite);
    l.Render();
    Raylib.EndDrawing();
}

Raylib.CloseWindow();