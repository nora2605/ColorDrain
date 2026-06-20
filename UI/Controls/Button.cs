using Raylib_cs;

namespace ColorDrain.UI.Controls;

internal class Button(string text, Rectangle bounds) : Control
{
    public int fontSize = 20;

    public string Text { get; set; } = text;
    public Rectangle Bounds { get; set; } = bounds;
    public Action? OnClick { get; set; }

    private bool hovering = false;
    private bool pressed = false;

    public bool Disabled { get; set; } = false;

    public void Update()
    {
        if (Disabled) return;
        if (Bounds.X < Raylib.GetMouseX() && Raylib.GetMouseX() < Bounds.X + Bounds.Width &&
            Bounds.Y < Raylib.GetMouseY() && Raylib.GetMouseY() < Bounds.Y + Bounds.Height)
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
        Color color = Disabled ? Color.Gray : hovering ? pressed ? Color.Beige : Color.Gray : Color.LightGray;
        Raylib.DrawRectangleRec(Bounds, color);
        Raylib.DrawText(Text, (int)(Bounds.X + (Bounds.Width - t) / 2), (int)(Bounds.Y + (Bounds.Height - fontSize) / 2), fontSize, pressed && !Disabled ? Color.White : Color.Black);
        Raylib.DrawRectangleLinesEx(Bounds, 2, Color.Black);
    }
}
