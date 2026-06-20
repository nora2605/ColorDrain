using Raylib_cs;
using System.Numerics;

namespace ColorDrain.UI.Controls;

internal class Input(string placeholder, string value, Rectangle bounds) : Control
{
    public string Placeholder { get; set; } = placeholder;
    public string Value
    {
        get => _value; set
        {
            _value = value;
            cursorPosition = value.Length;
        }
    }

    private string _value = value;
    public Rectangle Bounds { get; set; } = bounds;
    public Action? OnChange { get; set; }
    public Action? OnSubmit { get; set; }

    public bool Failed { get; set; } = false;
    public bool ReadOnly { get; set; } = false;
    public bool Centered { get; set; } = false;
    public int MaxLength { get; set; } = -1;
    public bool LoseFocusOnSubmit { get; set; } = true;
    public bool IsNumeric { get; set; } = false;

    private bool hovering = false;
    private bool active = false;

    private float blinkingTime = 1f;

    private int cursorPosition = value.Length;

    public void Update()
    {
        blinkingTime -= Raylib.GetFrameTime();
        if (blinkingTime < 0) blinkingTime = 1f;

        if (Bounds.X < Raylib.GetMouseX() && Raylib.GetMouseX() < Bounds.X + Bounds.Width &&
            Bounds.Y < Raylib.GetMouseY() && Raylib.GetMouseY() < Bounds.Y + Bounds.Height)
        {
            if (!hovering) Raylib.SetMouseCursor(ReadOnly ? MouseCursor.NotAllowed : MouseCursor.IBeam);
            hovering = true;
        }
        else
        {
            if (hovering) Raylib.SetMouseCursor(MouseCursor.Default);
            hovering = false;
        }

        if (ReadOnly) return;

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            active = hovering;

        if (active)
        {
            int key;
            while ((key = Raylib.GetCharPressed()) != 0)
            {
                char c = (char)key;
                if (key >= 32)
                {
                    if (MaxLength >= 0 && _value.Length >= MaxLength) return;
                    if (IsNumeric && !char.IsDigit(c)) return;
                    _value = Value.Insert(cursorPosition++, c.ToString());
                }
                OnChange?.Invoke();
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Backspace) || Raylib.IsKeyPressedRepeat(KeyboardKey.Backspace))
            {
                _value = cursorPosition > 0 ? Value.Remove(--cursorPosition, 1) : Value;
                OnChange?.Invoke();
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                if (LoseFocusOnSubmit) active = false;
                OnSubmit?.Invoke();
            }
            if (Raylib.IsKeyPressed(KeyboardKey.V) && Raylib.IsKeyDown(KeyboardKey.LeftControl))
            {
                string clipboard = Raylib.GetClipboardText_();
                if (clipboard != null)
                {
                    if (IsNumeric)
                        clipboard = new string([.. clipboard.Where(char.IsDigit)]);
                    int allowedLength = MaxLength >= 0 ? MaxLength - _value.Length : -1;
                    if (allowedLength >= 0 && clipboard.Length > allowedLength) clipboard = clipboard[0..allowedLength];
                    _value = Value.Insert(cursorPosition, clipboard);
                    cursorPosition += clipboard.Length;
                }
                OnChange?.Invoke();
            }
            if ((Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressedRepeat(KeyboardKey.Left)) && cursorPosition > 0)
                cursorPosition--;
            if ((Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressedRepeat(KeyboardKey.Right)) && cursorPosition < Value.Length)
                cursorPosition++;
            if (Raylib.IsKeyPressed(KeyboardKey.C) && Raylib.IsKeyDown(KeyboardKey.LeftControl))
            {
                Raylib.SetClipboardText(Value);
            }
        }
    }
    public void Render()
    {
        int t = Raylib.MeasureText(Value, 20);
        Color color = hovering ? Color.LightGray : Color.RayWhite;
        Raylib.DrawRectangleRec(Bounds, color);
        Raylib.BeginScissorMode((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height);
        int textNegativeOffset = (t > (int)Bounds.Width - 8 ? t - (int)Bounds.Width + 8 : 0) / (Centered ? 2 : 1);
        Raylib.DrawText(Value, (int)(Bounds.X + 4 - textNegativeOffset + (Centered ? (Bounds.Width - t) / 2 - 2 : 0)), (int)(Bounds.Y + (Bounds.Height - 20) / 2), 20, Color.Black);
        if (active)
        {
            int ct = Raylib.MeasureText(Value[0..cursorPosition], 20);
            if (blinkingTime > 0.5f)
                Raylib.DrawLineEx(
                    new Vector2(Bounds.X + 6 + ct - textNegativeOffset + (Centered ? (Bounds.Width - t) / 2 - 2 : 0), Bounds.Y + 4),
                    new Vector2(Bounds.X + 6 + ct - textNegativeOffset + (Centered ? (Bounds.Width - t) / 2 - 2 : 0), Bounds.Y + Bounds.Height - 4),
                    2,
                    Color.DarkGray
                );
        }
        else if (Value == "")
            Raylib.DrawText(Placeholder, (int)(Bounds.X + 4 + (Centered ? (Bounds.Width - Raylib.MeasureText(Placeholder, 20)) / 2 - 2 : 0)), (int)(Bounds.Y + (Bounds.Height - 20) / 2), 20, Color.DarkGray);
        Raylib.EndScissorMode();
        Raylib.DrawRectangleLinesEx(Bounds, 2, Failed ? Color.Red : Color.Black);
    }
}
