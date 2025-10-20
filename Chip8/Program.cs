using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;

namespace Chip8;

class Program
{
    static void Main()
    {
        var gameSettings = new GameWindowSettings
        {
            UpdateFrequency = 600
        };

        var nativeSettings = new NativeWindowSettings
        {
            ClientSize = new Vector2i(1024, 512),
            Profile = ContextProfile.Compatability,
            Title = "Chip8 Emulator"
        };

        var window = new Window(gameSettings, nativeSettings);
        window.Run();
    }
}
