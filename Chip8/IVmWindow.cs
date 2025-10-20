namespace Chip8;

/// <summary>
/// These methods get called by the virtual machine.
/// </summary>
public interface IVmWindow
{
    void Render();
    bool ProcessEvents(double timeout);
    void Beep();
}
