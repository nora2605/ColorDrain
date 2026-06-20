using Raylib_cs;

namespace ColorDrain.UI;

internal interface Control
{
    public Rectangle Bounds { get; set; }

    public void Update();
    public void Render();
}
