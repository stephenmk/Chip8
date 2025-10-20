using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Input;
using OpenTK.Windowing.Common.Input;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.Drawing;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Chip8;

public class Window : GameWindow, IVmWindow
{
    bool running, playingSound;

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
        Keys.A => 0xA,
        Keys.B => 0xB,
        Keys.C => 0xC,
        Keys.D => 0xD,
        Keys.E => 0xE,
        Keys.F => 0xF,
        _ => null,
    };

    private VirtualMachine vm;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        FileDrop += Window_FileDrop;
    }

    private void Window_FileDrop(FileDropEventArgs obj)
    {
        string rom = obj.FileNames[0];
        vm = new VirtualMachine(this, rom);

        running = true;
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
            vm?.KeyUp(value);
        }
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (KeyToByte(e.Key) is byte value)
        {
            vm?.KeyDown(value);
            return;
        }

        switch (e.Key)
        {
            case Keys.Escape:
                Close();
                break;
            case Keys.G:
                vm?.DebugGraphics();
                break;
            case Keys.M:
                vm?.DebugMemory();
                break;
            case Keys.R:
                vm?.DebugRegisters();
                break;
            case Keys.P:
                running = !running;
                break;
            case Keys.S:
                vm?.EmulateCycle();
                vm?.DebugRegisters();
                break;
            case Keys.Backspace:
                vm?.Reset();
                break;
            default:
                break;
        }
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (running)
        {
            vm?.EmulateCycle();
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
    }

    public void Render()
    {
        var buffer = vm?.Gfx;
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

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        Environment.Exit(0);
    }

    public void Beep()
    {
        Task.Run(() =>
        {
            if (!playingSound)
            {
                playingSound = true;
                if (OperatingSystem.IsWindows())
                    Console.Beep(440, 500);
                playingSound = false;
            }
        });
    }
}
