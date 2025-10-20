namespace Chip8;

/// <summary>
/// Window methods expected by Chip8.
/// </summary>
public interface IChip8Window
{
    void Render(IList<byte> buffer);
    bool ProcessEvents(double timeout);
    void Beep();
}
