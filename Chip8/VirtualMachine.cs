using System.Collections.Immutable;

namespace Chip8;

public class VirtualMachine
{
    private const ushort RomStart = 0x200;
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

    private ushort _opCode;

    /// <summary>
    /// 12bit register (for memory address)
    /// </summary>
    private ushort _i;

    /// <summary>
    /// Program Counter
    /// </summary>
    private ushort _pc;

    private readonly ushort[] _stack;
    private ushort _sp;

    private byte _delayTimer;
    private byte _soundTimer;

    /// <summary>
    /// Variables (16 available, 0 to F)
    /// </summary>
    private readonly byte[] _v;

    public readonly byte[] _memory;
    public readonly byte[] _screen;

    private uint _cycleCountModTen;

    /// <summary>
    /// Pressed keys, ranging from 0 to F.
    /// </summary>
    private readonly bool[] _keys;

    private readonly IChip8Window? _window;

    public VirtualMachine(IChip8Window? window, string romPath)
        : this(window, File.ReadAllBytes(romPath)) { }

    public VirtualMachine(IChip8Window? window, byte[] rom)
    {
        _pc = RomStart;
        _opCode = 0;
        _i = 0;

        _stack = new ushort[12];
        _sp = 0;
        _memory = new byte[4096];
        _screen = new byte[64 * 32];
        _v = new byte[16];

        _keys = new bool[16];

        _soundTimer = 0;
        _delayTimer = 0;
        _cycleCountModTen = 0;

        _window = window;

        // Load fonts.
        Fonts.CopyTo(_memory, 0x0);

        // Load ROM.
        rom.CopyTo(_memory, RomStart);
    }

    public StateSnapshot Snapshot() => new()
    {
        OpCode = _opCode,
        I = _i,
        PC = _pc,
        Stack = _stack.AsSpan(),
        SP = _sp,
        DelayTimer = _delayTimer,
        SoundTimer = _soundTimer,
        V = _v.AsSpan(),
        Memory = _memory.AsSpan(),
        Screen = _screen.AsSpan(),
    };

    public void Reset()
    {
        _pc = RomStart;
        _i = 0;
        _sp = 0;
        ClearScreen();
    }

    private void ClearScreen()
    {
        for (int i = 0; i < _screen.Length; i++)
        {
            _screen[i] = 0;
        }
    }

    public void KeyUp(byte key)
    {
        _keys[key] = false;
    }

    public void KeyDown(byte key)
    {
        _keys[key] = true;
    }

    public void Cycle(uint times)
    {
        for (uint i = 0; i < times; i++)
        {
            CycleOnce();
        }
    }

    private void CycleOnce()
    {
        _opCode = (ushort)(_memory[_pc] << 8 | _memory[_pc + 1]);
        _pc += 2;

        ushort nnn = (ushort)(_opCode & 0x0FFF);   // Address
        byte nn = (byte)(_opCode & 0x00FF);        // 8-bit constant
        byte n = (byte)(_opCode & 0x000F);         // 4-bit constant
        byte x = (byte)((_opCode & 0x0F00) >> 8);  // 4-bit register identifier
        byte y = (byte)((_opCode & 0x00F0) >> 4);  // 4-bit register identifier

        switch (_opCode & 0xF000)
        {
            case 0x0000 when _opCode == 0x00E0:
                OpCode00E0();
                break;
            case 0x0000 when _opCode == 0x00EE:
                OpCode00EE();
                break;
            case 0x0000:
                OpCode0NNN(nnn);
                break;
            case 0x1000:
                OpCode1NNN(nnn);
                break;
            case 0x2000:
                OpCode2NNN(nnn);
                break;
            case 0x3000:
                OpCode3XNN(x, nn);
                break;
            case 0x4000:
                OpCode4XNN(x, nn);
                break;
            case 0x5000:
                OpCode5XY0(x, y);
                break;
            case 0x6000:
                OpCode6XNN(x, nn);
                break;
            case 0x7000:
                OpCode7XNN(x, nn);
                break;
            case 0x8000 when (_opCode & 0x000F) == 0:
                OpCode8XY0(x, y);
                break;
            case 0x8000 when (_opCode & 0x000F) == 1:
                OpCode8XY1(x, y);
                break;
            case 0x8000 when (_opCode & 0x000F) == 2:
                OpCode8XY2(x, y);
                break;
            case 0x8000 when (_opCode & 0x000F) == 3:
                OpCode8XY3(x, y);
                break;
            case 0x8000 when (_opCode & 0x000F) == 4:
                OpCode8XY4(x, y);
                break;
            case 0x8000 when (_opCode & 0x000F) == 5:
                OpCode8XY5(x, y);
                break;
            case 0x8000 when (_opCode & 0x000F) == 6:
                OpCode8XY6(x);
                break;
            case 0x8000 when (_opCode & 0x000F) == 7:
                OpCode8XY7(x, y);
                break;
            case 0x8000 when (_opCode & 0x000F) == 0xE:
                OpCode8XYE(x);
                break;
            case 0x9000:
                OpCode9XY0(x, y);
                break;
            case 0xA000:
                OpCodeANNN(nnn);
                break;
            case 0xB000:
                OpCodeBNNN(nnn);
                break;
            case 0xC000:
                OpCodeCXNN(x, nn);
                break;
            case 0xD000:
                OpCodeDXYN(x, y, n);
                break;
            case 0xE000 when (_opCode & 0x00FF) == 0x9E:
                OpCodeEX9E(x);
                break;
            case 0xE000 when (_opCode & 0x00FF) == 0xA1:
                OpCodeEXA1(x);
                break;
            case 0xF000 when (_opCode & 0x00FF) == 0x07:
                OpCodeFX07(x);
                break;
            case 0xF000 when (_opCode & 0x00FF) == 0x0A:
                OpCodeFX0A(x);
                break;
            case 0xF000 when (_opCode & 0x00FF) == 0x15:
                OpCodeFX15(x);
                break;
            case 0xF000 when (_opCode & 0x00FF) == 0x18:
                OpCodeFX18(x);
                break;
            case 0xF000 when (_opCode & 0x00FF) == 0x1E:
                OpCodeFX1E(x);
                break;
            case 0xF000 when (_opCode & 0x00FF) == 0x29:
                OpCodeFX29(x);
                break;
            case 0xF000 when (_opCode & 0x00FF) == 0x33:
                OpCodeFX33(x);
                break;
            case 0xF000 when (_opCode & 0x00FF) == 0x55:
                OpCodeFX55(x);
                break;
            case 0xF000 when (_opCode & 0x00FF) == 0x65:
                OpCodeFX65(x);
                break;
            default:
                throw new InvalidOperationException($"Invalid OpCode {_opCode:X4} @ PC = 0x{_pc:X3}");
        }

        // The update frequency is 600 Hz. Timers should be updated at 60 Hz, so update timers every 10th cycle.
        if ((++_cycleCountModTen % 10) == 0)
        {
            UpdateTimers();
            _cycleCountModTen = 0;
        }
    }

    private void UpdateTimers()
    {
        if (_delayTimer > 0)
        {
            _delayTimer--;
        }
        if (_soundTimer > 0)
        {
            _window?.Beep();
            _soundTimer--;
        }
    }

    #region OpCodes

    /// <summary>
    /// Jumps to address NNN.
    /// </summary>
    private void OpCode1NNN(ushort nnn)
    {
        _pc = nnn;
    }

    /// <summary>
    /// Clears the screen.
    /// </summary>
    private void OpCode00E0()
    {
        ClearScreen();
    }

    /// <summary>
    /// Returns from a subroutine.
    /// </summary>
    private void OpCode00EE()
    {
        _pc = _stack[_sp--];
    }

    /// <summary>
    /// Calls subroutine at NNN.
    /// </summary>
    private void OpCode2NNN(ushort nnn)
    {
        _stack[++_sp] = _pc;
        _pc = nnn;
    }

    /// <summary>
    /// Sets I to the address NNN.
    /// </summary>
    private void OpCodeANNN(ushort nnn)
    {
        _i = nnn;
    }

    /// <summary>
    /// Sets Vx to NN.
    /// </summary>
    private void OpCode6XNN(ushort x, ushort nn)
    {
        _v[x] = (byte)nn;
    }

    /// <summary>
    /// Draws a sprite at coordinate (Vx, Vy) that has a width of 8 pixels and a height of N pixels.
    /// </summary>
    /// <remarks>
    /// Source: https://stackoverflow.com/questions/17346592/how-does-chip-8-graphics-rendered-on-screen
    /// </remarks>
    private void OpCodeDXYN(byte X, byte Y, byte N)
    {
        // Initialize the collision detection as no collision detected (yet).
        _v[0xF] = 0;

        // Draw N lines on the screen.
        for (int line = 0; line < N; line++)
        {
            // y is the starting line Y + current line. If y is larger than the total width of the screen then wrap around (this is the modulo operation).
            var y = (_v[Y] + line) % 32;

            // The current sprite being drawn, each line is a new sprite.
            byte sprite = _memory[_i + line];

            // Each bit in the sprite is a pixel on or off.
            for (int column = 0; column < 8; column++)
            {
                // Start with the current most significant bit. The next bit will be left shifted in from the right.
                if ((sprite & 0x80) != 0)
                {
                    // Get the current x position and wrap around if needed.
                    var x = (_v[X] + column) % 64;

                    // Collision detection: If the target pixel is already set then set the collision detection flag in register VF.
                    if (_screen[y * 64 + x] == 1)
                    {
                        _v[0xF] = 1;
                    }

                    // Enable or disable the pixel (XOR operation).
                    _screen[y * 64 + x] ^= 1;
                }

                // Shift the next bit in from the right.
                sprite <<= 0x1;
            }
        }

        _window?.Render(_screen);
    }

    /// <summary>
    /// Adds NN to Vx (carry flag is not changed).
    /// </summary>
    private void OpCode7XNN(byte x, byte nn)
    {
        _v[x] += nn;
    }

    /// <summary>
    /// Calls machine code routine (RCA 1802 for COSMAC VIP) at address NNN. Not necessary for most ROMs.
    /// </summary>
    private void OpCode0NNN(ushort nnn)
    {
        throw new NotImplementedException($"error: {_opCode:X4} has not been implemented.");
    }

    /// <summary>
    /// Skips the next instruction if Vx equals NN.
    /// </summary>
    /// <remarks>
    /// Usually the next instruction is a jump to skip a code block.
    /// </remarks>
    private void OpCode3XNN(byte x, ushort nn)
    {
        if (_v[x] == nn)
        {
            _pc += 2;
        }
    }

    /// <summary>
    /// Skips the next instruction if Vx does not equal NN.
    /// </summary>
    /// <remarks>
    /// Usually the next instruction is a jump to skip a code block.
    /// </remarks>
    private void OpCode4XNN(byte x, ushort nn)
    {
        if (_v[x] != nn)
        {
            _pc += 2;
        }
    }

    /// <summary>
    /// Skips the next instruction if Vx equals Vy.
    /// </summary>
    /// <remarks>
    /// Usually the next instruction is a jump to skip a code block.
    /// </remarks>
    private void OpCode5XY0(byte x, byte y)
    {
        if (_v[x] == _v[y])
        {
            _pc += 2;
        }
    }

    /// <summary>
    /// Sets Vx to the value of Vy.
    /// </summary>
    private void OpCode8XY0(byte x, byte y)
    {
        _v[x] = _v[y];
    }

    /// <summary>
    /// Sets Vx to Vx or Vy (bitwise OR operation).
    /// </summary>
    private void OpCode8XY1(byte x, byte y)
    {
        _v[x] = (byte)(_v[x] | _v[y]);
    }

    /// <summary>
    /// Sets Vx to Vx and Vy (bitwise AND operation).
    /// </summary>
    private void OpCode8XY2(byte x, byte y)
    {
        _v[x] = (byte)(_v[x] & _v[y]);
    }

    /// <summary>
    /// Sets Vx to Vx xor Vy.
    /// </summary>
    private void OpCode8XY3(byte x, byte y)
    {
        _v[x] = (byte)(_v[x] ^ _v[y]);
    }

    /// <summary>
    /// Adds Vy to Vx. VF is set to 1 when there's an overflow, and to 0 when there is not.
    /// </summary>
    private void OpCode8XY4(byte x, byte y)
    {
        if (_v[y] > (0xFF - _v[x]))
        {
            _v[0xF] = 1;
        }
        else
        {
            _v[0xF] = 0;
        }
        _v[x] += _v[y];
    }

    /// <summary>
    /// Subtract Vy from Vx. VF is set to 0 when there's an underflow, and 1 when there is not.
    /// </summary>
    private void OpCode8XY5(byte x, byte y)
    {
        if (_v[y] > _v[x])
        {
            _v[0xF] = 0;
        }
        else
        {
            _v[0xF] = 1;
        }
        _v[x] -= _v[y];
    }

    private void OpCode8XY6(byte x)
    {
        _v[0xF] = (byte)(_v[x] & 0x1);
        _v[x] >>= 0x1;
    }

    /// <summary>
    /// Sets Vx to Vy minus Vx. VF is set to 0 when there's an underflow, and 1 when there is not.
    /// </summary>
    private void OpCode8XY7(byte x, byte y)
    {
        int diff = _v[y] - _v[x];
        _v[x] = (byte)(diff & 0xFF);
        _v[0xF] = (byte)(diff > 0 ? 1 : 0);
    }

    private void OpCode8XYE(byte x)
    {
        _v[0xF] = (byte)((_v[x] & 0x80) >> 7);
        _v[x] <<= 0x1;
    }

    private void OpCodeBNNN(ushort nnn)
    {
        _pc = (ushort)(nnn + _v[0]);
    }

    private void OpCodeCXNN(byte x, byte nn)
    {
        var random = new Random();
        _v[x] = (byte)(random.Next(0, 0xFF) & nn);
    }

    private void OpCodeFX1E(byte x)
    {
        _i += _v[x];
    }

    /// <summary>
    /// Fills from V0 to VX (including VX) with values from memory, starting at address I.
    /// </summary>
    /// <remarks>
    /// The offset from I is increased by 1 for each value read, but I itself is left unmodified.
    /// </remarks>
    private void OpCodeFX65(byte x)
    {
        for (int i = 0; i <= x; i++)
        {
            _v[i] = _memory[_i + i];
        }
    }

    private void OpCodeFX55(byte x)
    {
        for (int i = 0; i <= x; i++)
        {
            _memory[_i + i] = _v[i];
        }
    }

    /// <summary>
    /// A key press is awaited, and then stored in Vx 
    /// </summary>
    /// <remarks>
    /// Blocking operation; all instruction halted until next key event.
    /// Delay and sound timers should continue processing.
    /// </remarks>
    private void OpCodeFX0A(byte x)
    {
        while (true)
        {
            for (byte i = 0; i < 0xF; i++)
            {
                if (_keys[i])
                {
                    _v[x] = i;
                    return;
                }
            }
            _window?.ProcessEvents(1); // TODO: 1 is a guess. What should this timeout be?
        }
    }

    private void OpCodeEX9E(byte x)
    {
        if (_keys[_v[x]])
        {
            _pc += 2;
        }
    }

    private void OpCodeEXA1(byte x)
    {
        if (!_keys[_v[x]])
        {
            _pc += 2;
        }
    }

    private void OpCodeFX15(byte x)
    {
        _delayTimer = _v[x];
    }

    private void OpCodeFX18(byte x)
    {
        _soundTimer = _v[x];
    }

    private void OpCodeFX07(byte x)
    {
        _v[x] = _delayTimer;
    }

    private void OpCodeFX29(byte x)
    {
        _i = (byte)(_v[x] * 5);
    }

    private void OpCodeFX33(byte x)
    {
        var number = _v[x];
        _memory[_i] = (byte)(number / 100);
        _memory[_i + 1] = (byte)(number / 10 % 10);
        _memory[_i + 2] = (byte)(number % 100 % 10);
    }

    private void OpCode9XY0(byte x, byte y)
    {
        if (_v[x] != _v[y])
        {
            _pc += 2;
        }
    }
    #endregion
}
