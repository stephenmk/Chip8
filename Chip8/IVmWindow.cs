namespace Chip8;

/// <summary>
/// These methods get called by the virtual machine.
/// </summary>
public interface IVmWindow
{
    public void Render();
    public bool ProcessEvents(double timeout);
    public void Beep();
}
