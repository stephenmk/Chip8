// Copyright (c) 2020 Ron
// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Chip8;

public class Window : GameWindow, IChip8Window
{
    private const uint PixelCount = 64 * 32;
    private readonly bool[] _pixels = new bool[PixelCount];
    private bool _isRunning;
    private bool _isBeeping;
    private bool _disposedValue = false;

    private WindowGraphics? _graphics;
    private VirtualMachine? _virtualMachine;
    private Debugger? _debugger;

    public Window(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings)
        : base(gameSettings, nativeSettings) { }

    public void LoadRom(string romPath)
    {
        _virtualMachine = new(this, romPath, new Quirks(), 1200);
        _debugger = new(_virtualMachine);
        _isRunning = true;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        _graphics = new WindowGraphics(PixelCount);
        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);
        if (_isRunning)
        {
            _virtualMachine?.CycleFrame();
        }
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        if (_virtualMachine is not null)
        {
            _graphics?.Render(_pixels);
        }
        else
        {
            _graphics?.ClearScreen();
        }
        SwapBuffers();
    }

    public void UpdateScreen(IList<bool> screen)
    {
        for (int i = 0; i < PixelCount; i++)
        {
            _pixels[i] = screen[i] ^ _isBeeping;
        }
    }

    public void StartBeep()
    {
        if (!_isBeeping)
        {
            _isBeeping = true;
            UpdateScreen(_pixels);
        }
    }

    public void EndBeep()
    {
        if (_isBeeping)
        {
            _isBeeping = false;
            UpdateScreen(_pixels);
        }
    }

    protected override void OnFileDrop(FileDropEventArgs obj)
    {
        LoadRom(romPath: obj.FileNames[0]);
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
                _debugger?.PrintScreen();
                break;
            case Keys.M:
                _debugger?.PrintMemory();
                break;
            case Keys.R:
                _debugger?.PrintRegisters();
                break;
            case Keys.P:
                _isRunning = !_isRunning;
                break;
            case Keys.S:
                _isRunning = false;
                _virtualMachine?.Cycle(1);
                _debugger?.PrintRegisters();
                break;
            case Keys.Backspace:
                _virtualMachine?.Reset();
                break;
            default:
                break;
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        _graphics?.Resize(0, 0, Size.X, Size.Y);
    }

    protected override void Dispose(bool _)
    {
        if (!_disposedValue)
        {
            _graphics?.Dispose();
            _disposedValue = true;
        }
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
