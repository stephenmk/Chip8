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

    private readonly State _state;
    private readonly IChip8Window? _window;

    public VirtualMachine(IChip8Window? window, string romPath)
        : this(window, File.ReadAllBytes(romPath)) { }

    public VirtualMachine(IChip8Window? window, byte[] rom)
    {
        _state = new State(Fonts, rom);
        _window = window;
    }

    public StateSnapshot Snapshot() => _state.Snapshot();
    public void KeyUp(byte key) => _state.UnsetKey(key);
    public void KeyDown(byte key) => _state.SetKey(key);
    public void Reset() => _state.Reset();

    public void Cycle(uint times)
    {
        for (uint i = 0; i < times; i++)
        {
            CycleOnce();
        }
    }

    private void CycleOnce()
    {
        var opCode = _state.NextOpCode();

        ushort nnn = (ushort)(opCode & 0x0FFF);   // Address
        byte nn = (byte)(opCode & 0x00FF);        // 8-bit constant
        byte n = (byte)(opCode & 0x000F);         // 4-bit constant
        byte x = (byte)((opCode & 0x0F00) >> 8);  // 4-bit register identifier
        byte y = (byte)((opCode & 0x00F0) >> 4);  // 4-bit register identifier

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
            case 0x8000 when (opCode & 0x000F) == 0:
                _state.OpCode8XY0(x, y);
                break;
            case 0x8000 when (opCode & 0x000F) == 1:
                _state.OpCode8XY1(x, y);
                break;
            case 0x8000 when (opCode & 0x000F) == 2:
                _state.OpCode8XY2(x, y);
                break;
            case 0x8000 when (opCode & 0x000F) == 3:
                _state.OpCode8XY3(x, y);
                break;
            case 0x8000 when (opCode & 0x000F) == 4:
                _state.OpCode8XY4(x, y);
                break;
            case 0x8000 when (opCode & 0x000F) == 5:
                _state.OpCode8XY5(x, y);
                break;
            case 0x8000 when (opCode & 0x000F) == 6:
                _state.OpCode8XY6(x, y);
                break;
            case 0x8000 when (opCode & 0x000F) == 7:
                _state.OpCode8XY7(x, y);
                break;
            case 0x8000 when (opCode & 0x000F) == 0xE:
                _state.OpCode8XYE(x, y);
                break;
            case 0x9000:
                _state.OpCode9XY0(x, y);
                break;
            case 0xA000:
                _state.OpCodeANNN(nnn);
                break;
            case 0xB000:
                _state.OpCodeBNNN(nnn);
                break;
            case 0xC000:
                _state.OpCodeCXNN(x, nn);
                break;
            case 0xD000:
                var screen = _state.OpCodeDXYN(x, y, n);
                _window?.Render(screen);
                break;
            case 0xE000 when (opCode & 0x00FF) == 0x9E:
                _state.OpCodeEX9E(x);
                break;
            case 0xE000 when (opCode & 0x00FF) == 0xA1:
                _state.OpCodeEXA1(x);
                break;
            case 0xF000 when (opCode & 0x00FF) == 0x07:
                _state.OpCodeFX07(x);
                break;
            case 0xF000 when (opCode & 0x00FF) == 0x0A:
                _state.OpCodeFX0A(x);
                break;
            case 0xF000 when (opCode & 0x00FF) == 0x15:
                _state.OpCodeFX15(x);
                break;
            case 0xF000 when (opCode & 0x00FF) == 0x18:
                _state.OpCodeFX18(x);
                break;
            case 0xF000 when (opCode & 0x00FF) == 0x1E:
                _state.OpCodeFX1E(x);
                break;
            case 0xF000 when (opCode & 0x00FF) == 0x29:
                _state.OpCodeFX29(x);
                break;
            case 0xF000 when (opCode & 0x00FF) == 0x33:
                _state.OpCodeFX33(x);
                break;
            case 0xF000 when (opCode & 0x00FF) == 0x55:
                _state.OpCodeFX55(x);
                break;
            case 0xF000 when (opCode & 0x00FF) == 0x65:
                _state.OpCodeFX65(x);
                break;
            default:
                throw new InvalidOperationException($"Invalid OpCode {opCode:X4}");
        }

        if (_state.Beep)
        {
            _window?.Beep();
        }

        _state.UpdateTimers();
    }
}
