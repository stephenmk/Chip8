// Copyright (c) 2020 Ron
// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace Chip8;

public class VirtualMachine
{
    private readonly ImmutableArray<byte> Fonts =
    [
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80, // F
    ];

    private readonly IChip8Window? _window;
    private readonly State _state;
    private readonly Quirks _quirks;
    private readonly int _ips;

    public VirtualMachine(IChip8Window? window, string romPath, Quirks quirks = default, int ips = 600)
        : this(window, File.ReadAllBytes(romPath), quirks, ips) { }

    public VirtualMachine(IChip8Window? window, byte[] rom, Quirks quirks = default, int ips = 600)
    {
        _window = window;
        _quirks = quirks;
        _state = new State(ips, Fonts, rom);
        _ips = ips;
    }

    public StateSnapshot Snapshot() => _state.Snapshot();
    public void KeyUp(byte key) => _state.UnsetKey(key);
    public void KeyDown(byte key) => _state.SetKey(key);
    public void Reset() => _state.Reset();

    public void CycleFrame()
    {
        if (_window is null)
        {
            throw new InvalidOperationException();
        }
        uint times = (uint)(_ips / _window?.UpdateFrequency)!;
        Console.WriteLine(times);
        Cycle(times);
    }

    public void Cycle(uint times)
    {
        for (uint i = 0; i < times; i++)
        {
            Cycle();
        }
    }

    private void Cycle()
    {
        var opCode = _state.NextInstruction();

        ushort nnn = (ushort)(opCode & 0x0FFF);   // 12-bit address
        byte nn = (byte)(opCode & 0x00FF);        // 8-bit constant
        byte x = (byte)((opCode & 0x0F00) >> 8);  // 4-bit register identifier
        byte y = (byte)((opCode & 0x00F0) >> 4);  // 4-bit register identifier
        byte n = (byte)((opCode & 0x000F) >> 0);  // 4-bit constant

        switch (opCode & 0xF000)
        {
            case 0x0000 when opCode == 0x00E0:
                _state.OpCode00E0();
                break;
            case 0x0000 when opCode == 0x00EE:
                _state.OpCode00EE();
                break;
            case 0x0000:
                _state.OpCode0NNN(nnn);
                break;
            case 0x1000:
                _state.OpCode1NNN(nnn);
                break;
            case 0x2000:
                _state.OpCode2NNN(nnn);
                break;
            case 0x3000:
                _state.OpCode3XNN(x, nn);
                break;
            case 0x4000:
                _state.OpCode4XNN(x, nn);
                break;
            case 0x5000:
                _state.OpCode5XY0(x, y);
                break;
            case 0x6000:
                _state.OpCode6XNN(x, nn);
                break;
            case 0x7000:
                _state.OpCode7XNN(x, nn);
                break;
            case 0x8000 when n == 0x0:
                _state.OpCode8XY0(x, y);
                break;
            case 0x8000 when n == 0x1:
                _state.OpCode8XY1(x, y, _quirks.VfReset);
                break;
            case 0x8000 when n == 0x2:
                _state.OpCode8XY2(x, y, _quirks.VfReset);
                break;
            case 0x8000 when n == 0x3:
                _state.OpCode8XY3(x, y, _quirks.VfReset);
                break;
            case 0x8000 when n == 0x4:
                _state.OpCode8XY4(x, y);
                break;
            case 0x8000 when n == 0x5:
                _state.OpCode8XY5(x, y);
                break;
            case 0x8000 when n == 0x6:
                _state.OpCode8XY6(x, y, _quirks.Shifting);
                break;
            case 0x8000 when n == 0x7:
                _state.OpCode8XY7(x, y);
                break;
            case 0x8000 when n == 0xE:
                _state.OpCode8XYE(x, y, _quirks.Shifting);
                break;
            case 0x9000:
                _state.OpCode9XY0(x, y);
                break;
            case 0xA000:
                _state.OpCodeANNN(nnn);
                break;
            case 0xB000:
                _state.OpCodeBNNN(nnn, _quirks.Jumping);
                break;
            case 0xC000:
                _state.OpCodeCXNN(x, nn);
                break;
            case 0xD000:
                _state.OpCodeDXYN(x, y, n, _quirks.DisplayWait, _quirks.Clipping);
                UpdateScreen();
                break;
            case 0xE000 when nn == 0x9E:
                _state.OpCodeEX9E(x);
                break;
            case 0xE000 when nn == 0xA1:
                _state.OpCodeEXA1(x);
                break;
            case 0xF000 when nn == 0x07:
                _state.OpCodeFX07(x);
                break;
            case 0xF000 when nn == 0x0A:
                _state.OpCodeFX0A(x);
                break;
            case 0xF000 when nn == 0x15:
                _state.OpCodeFX15(x);
                break;
            case 0xF000 when nn == 0x18:
                _state.OpCodeFX18(x);
                break;
            case 0xF000 when nn == 0x1E:
                _state.OpCodeFX1E(x);
                break;
            case 0xF000 when nn == 0x29:
                _state.OpCodeFX29(x);
                break;
            case 0xF000 when nn == 0x33:
                _state.OpCodeFX33(x);
                break;
            case 0xF000 when nn == 0x55:
                _state.OpCodeFX55(x, _quirks.Memory);
                break;
            case 0xF000 when nn == 0x65:
                _state.OpCodeFX65(x, _quirks.Memory);
                break;
            default:
                throw new InvalidOperationException($"Invalid OpCode {opCode:X4}");
        }

        if (_state.UpdateTimers())
        {
            _window?.StartBeep();
        }
        else
        {
            _window?.EndBeep();
        }
    }

    private void UpdateScreen()
    {
        var snapshot = Snapshot();
        var screen = snapshot.Screen.ToImmutableArray();
        _window?.UpdateScreen(screen);
    }
}
