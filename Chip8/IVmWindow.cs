namespace Chip8;

/// <summary>
/// These methods get called by the virtual machine.
/// </summary>
public interface IVmWindow
{
    void Render(IList<byte> buffer);
    bool ProcessEvents(double timeout);
    void Beep();
}
