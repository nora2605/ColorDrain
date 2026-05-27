namespace ColorDrain.UI;

internal interface Scene : IDisposable
{
    public void Update();
    public void Render();
}
