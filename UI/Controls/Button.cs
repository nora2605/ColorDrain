using Raylib_cs;

namespace ColorDrain.UI.Controls;

internal class Button(string text, int x, int y, int width, int height) : Control
{
    public int fontSize = 20;

    public string Text { get; set; } = text;
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;
    public Action? OnClick { get; set; }

    private bool hovering = false;
    private bool pressed = false;

    public void Update()
    {
        if (X < Raylib.GetMouseX() && Raylib.GetMouseX() < X + Width &&
            Y < Raylib.GetMouseY() && Raylib.GetMouseY() < Y + Height)
        {
            hovering = true;
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                pressed = true;
                OnClick?.Invoke();
            }
            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                pressed = false;
        }
        else
        {
            hovering = false;
            pressed = false;
        }
    }

    public void Render()
    {
        int t = Raylib.MeasureText(Text, fontSize);
        Color color = pressed ? Color.Beige : hovering ? Color.Gray : Color.LightGray;
        Raylib.DrawRectangleRec(new Rectangle(X, Y, Width, Height), color);
        Raylib.DrawText(Text, X + (Width - t) / 2, Y + (Height - fontSize) / 2, fontSize, pressed ? Color.White : Color.Black);
        Raylib.DrawRectangleLinesEx(new Rectangle(X, Y, Width, Height), 2, Color.Black);
    }
}
