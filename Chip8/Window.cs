using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Input;
using OpenTK.Windowing.Common.Input;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.Drawing;
using System.ComponentModel;

namespace Chip8;

public class Window : GameWindow, IVmWindow
{
    private bool _isRunning;
    private bool _isPlayingSound;
    private VirtualMachine? _virtualMachine;

    public Window(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings)
        : base(gameSettings, nativeSettings) { }

    public void Render()
    {
        var buffer = _virtualMachine?.Gfx;
        if (buffer != null)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (buffer[y * 64 + x] > 0)
                    {
                        GL.Rect(x, y, x + 1, y + 1);
                    }
                }
            }
            SwapBuffers();
        }
    }

    public void Beep()
    {
        Task.Run(() =>
        {
            if (!_isPlayingSound)
            {
                _isPlayingSound = true;
                if (OperatingSystem.IsWindows())
                    Console.Beep(440, 500);
                _isPlayingSound = false;
            }
        });
    }

    protected override void OnFileDrop(FileDropEventArgs obj)
    {
        string romPath = obj.FileNames[0];
        _virtualMachine = new VirtualMachine(this, romPath);
        _isRunning = true;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color.Black);
        GL.Color3(Color.White);
        GL.Ortho(0, 64, 32, 0, -1, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        SwapBuffers();
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (KeyToByte(e.Key) is byte value)
        {
            _virtualMachine?.KeyUp(value);
        }
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (KeyToByte(e.Key) is byte value)
        {
            _virtualMachine?.KeyDown(value);
            return;
        }

        switch (e.Key)
        {
            case Keys.Escape:
                Close();
                break;
            case Keys.G:
                _virtualMachine?.DebugGraphics();
                break;
            case Keys.M:
                _virtualMachine?.DebugMemory();
                break;
            case Keys.R:
                _virtualMachine?.DebugRegisters();
                break;
            case Keys.P:
                _isRunning = !_isRunning;
                break;
            case Keys.S:
                _virtualMachine?.EmulateCycle();
                _virtualMachine?.DebugRegisters();
                break;
            case Keys.Backspace:
                _virtualMachine?.Reset();
                break;
            default:
                break;
        }
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        if (_isRunning)
        {
            _virtualMachine?.EmulateCycle();
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        Environment.Exit(0);
    }

    private static byte? KeyToByte(Keys key) => key switch
    {
        Keys.KeyPad0 => 0x0,
        Keys.KeyPad1 => 0x1,
        Keys.KeyPad2 => 0x2,
        Keys.KeyPad3 => 0x3,
        Keys.KeyPad4 => 0x4,
        Keys.KeyPad5 => 0x5,
        Keys.KeyPad6 => 0x6,
        Keys.KeyPad7 => 0x7,
        Keys.KeyPad8 => 0x8,
        Keys.KeyPad9 => 0x9,
        Keys.D0 => 0x0,
        Keys.D1 => 0x1,
        Keys.D2 => 0x2,
        Keys.D3 => 0x3,
        Keys.D4 => 0x4,
        Keys.D5 => 0x5,
        Keys.D6 => 0x6,
        Keys.D7 => 0x7,
        Keys.D8 => 0x8,
        Keys.D9 => 0x9,
        Keys.A => 0xA,
        Keys.B => 0xB,
        Keys.C => 0xC,
        Keys.D => 0xD,
        Keys.E => 0xE,
        Keys.F => 0xF,
        _ => null,
    };
}
