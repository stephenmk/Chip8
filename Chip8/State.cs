// Copyright (c) 2020 Ron
// Copyright (c) 2025 Stephen Kraus
// Opcode method documentation comments from https://en.wikipedia.org/wiki/CHIP-8
// SPDX-License-Identifier: MIT

namespace Chip8;

internal class State
{
    private const ushort RomStart = 0x200;

    private ushort OpCode;
    private ushort I;  // 12bit register (for memory address)
    private ushort PC;  // Program Counter
    private ushort SP;  // Stack Pointer

    private bool Beep;
    private bool Blocked;
    private byte DelayTimer;
    private byte SoundTimer;
    private ushort CycleCount;
    private UInt128 InstructionCycles;

    private readonly byte[] V;  // Variables (16 available, 0 to F)
    private readonly byte[] Memory;
    private readonly bool[] Screen;
    private readonly ushort[] Stack;
    private readonly bool[] Keys;  // Pressed keys, ranging from 0 to F.

    public State(IList<byte> font, IList<byte> rom)
    {
        OpCode = 0x0000;
        I = 0;
        PC = RomStart;
        SP = 0;

        Blocked = false;
        DelayTimer = 0;
        SoundTimer = 0;
        CycleCount = 0;
        InstructionCycles = 0;

        Stack = new ushort[12];
        Memory = new byte[4096];
        Screen = new bool[64 * 32];
        V = new byte[16];
        Keys = new bool[16];

        // Load fonts.
        font.CopyTo(Memory, 0x0);

        // Load ROM.
        rom.CopyTo(Memory, RomStart);
    }

    public StateSnapshot Snapshot() => new()
    {
        OpCode = OpCode,
        MemoryAddress = I,
        ProgramCounter = PC,
        StackPointer = SP,
        Beep = Beep,
        Blocked = Blocked,
        DelayTimer = DelayTimer,
        SoundTimer = SoundTimer,
        CycleCount = InstructionCycles,
        Stack = Stack.AsSpan(),
        Variables = V.AsSpan(),
        Memory = Memory.AsSpan(),
        Screen = Screen.AsSpan(),
        Keys = Keys.AsSpan(),
    };

    public void SetKey(byte key)
    {
        Keys[key] = true;
    }

    public void UnsetKey(byte key)
    {
        Keys[key] = false;
    }

    public void Reset()
    {
        OpCode = 0x0000;
        I = 0;
        PC = RomStart;
        SP = 0;
        Blocked = false;
        DelayTimer = 0;
        SoundTimer = 0;
        CycleCount = 0;
        InstructionCycles = 0;
        OpCode00E0();  // Clear screen
    }

    public ushort NextOpCode()
    {
        if (!Blocked)
        {
            OpCode = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);
            PC += 2;
        }
        return OpCode;
    }

    /// <remarks>
    /// The update frequency is 600 Hz. Timers should be
    /// updated at 60 Hz, so update timers every 10th cycle.
    /// </remarks>
    /// <returns><c>true</c> if beeping, otherwise <c>false</c>.</returns>
    public bool UpdateTimers()
    {
        InstructionCycles++;
        if ((++CycleCount % 10) == 0)
        {
            CycleCount = 0;
            if (DelayTimer > 0)
            {
                DelayTimer--;
            }
            Beep = SoundTimer > 0;
            if (Beep)
            {
                SoundTimer--;
            }
        }
        return Beep;
    }

    /// <summary>
    /// Clears the screen.
    /// </summary>
    public void OpCode00E0()
    {
        for (int i = 0; i < Screen.Length; i++)
        {
            Screen[i] = false;
        }
    }

    /// <summary>
    /// Returns from a subroutine.
    /// </summary>
    public void OpCode00EE()
    {
        PC = Stack[SP--];
    }

    /// <summary>
    /// Calls machine code routine (RCA 1802 for COSMAC VIP)
    /// at address NNN. Not necessary for most ROMs.
    /// </summary>
    public void OpCode0NNN(ushort nnn)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Jumps to address NNN.
    /// </summary>
    public void OpCode1NNN(ushort nnn)
    {
        PC = nnn;
    }

    /// <summary>
    /// Calls subroutine at NNN.
    /// </summary>
    public void OpCode2NNN(ushort nnn)
    {
        Stack[++SP] = PC;
        PC = nnn;
    }

    /// <summary>
    /// Skips the next instruction if Vx equals NN.
    /// </summary>
    /// <remarks>
    /// Usually the next instruction is a jump to skip a code block.
    /// </remarks>
    public void OpCode3XNN(byte x, ushort nn)
    {
        if (V[x] == nn)
        {
            PC += 2;
        }
    }

    /// <summary>
    /// Skips the next instruction if Vx does not equal NN.
    /// </summary>
    /// <remarks>
    /// Usually the next instruction is a jump to skip a code block.
    /// </remarks>
    public void OpCode4XNN(byte x, ushort nn)
    {
        if (V[x] != nn)
        {
            PC += 2;
        }
    }

    /// <summary>
    /// Skips the next instruction if Vx equals Vy.
    /// </summary>
    /// <remarks>
    /// Usually the next instruction is a jump to skip a code block.
    /// </remarks>
    public void OpCode5XY0(byte x, byte y)
    {
        if (V[x] == V[y])
        {
            PC += 2;
        }
    }

    /// <summary>
    /// Sets Vx to NN.
    /// </summary>
    public void OpCode6XNN(ushort x, ushort nn)
    {
        V[x] = (byte)nn;
    }

    /// <summary>
    /// Adds NN to Vx (carry flag is not changed).
    /// </summary>
    public void OpCode7XNN(byte x, byte nn)
    {
        V[x] += nn;
    }

    /// <summary>
    /// Sets Vx to the value of Vy.
    /// </summary>
    public void OpCode8XY0(byte x, byte y)
    {
        V[x] = V[y];
    }

    /// <summary>
    /// Sets Vx to Vx or Vy (bitwise OR operation).
    /// </summary>
    public void OpCode8XY1(byte x, byte y)
    {
        V[x] = (byte)(V[x] | V[y]);
    }

    /// <summary>
    /// Sets Vx to Vx and Vy (bitwise AND operation).
    /// </summary>
    public void OpCode8XY2(byte x, byte y)
    {
        V[x] = (byte)(V[x] & V[y]);
    }

    /// <summary>
    /// Sets Vx to Vx xor Vy.
    /// </summary>
    public void OpCode8XY3(byte x, byte y)
    {
        V[x] = (byte)(V[x] ^ V[y]);
    }

    /// <summary>
    /// Adds Vy to Vx. VF is set to 1 when there's an overflow,
    /// and to 0 when there is not.
    /// </summary>
    public void OpCode8XY4(byte x, byte y)
    {
        int sum = V[x] + V[y];
        V[x] = (byte)sum;
        V[0xF] = (byte)(sum > 255 ? 0x01 : 0x00);
    }

    /// <summary>
    /// Subtract Vy from Vx. VF is set to 0 when there's an underflow,
    /// and 1 when there is not.
    /// </summary>
    public void OpCode8XY5(byte x, byte y)
    {
        int diff = V[x] - V[y];
        V[x] = (byte)diff;
        V[0xF] = (byte)(diff < 0 ? 0x00 : 0x01);
    }

    /// <summary>
    /// Shifts Vx to the right by 1, then stores the least significant
    /// bit of Vx prior to the shift into VF.
    /// </summary>
    /// <remarks>
    /// Note that x is allowed to be F, and the assignment to VF must
    /// be done after the assignment to Vx.
    /// </remarks>
    public void OpCode8XY6(byte x, byte _)
    {
        byte leastSigBit = (byte)(V[x] & 0b0000_0001);
        V[x] >>= 0x01;
        V[0xF] = leastSigBit;
    }

    /// <summary>
    /// Sets Vx to Vy minus Vx. VF is set to 0 when there's an underflow,
    /// and 1 when there is not.
    /// </summary>
    public void OpCode8XY7(byte x, byte y)
    {
        int diff = V[y] - V[x];
        V[x] = (byte)diff;
        V[0xF] = (byte)(diff < 0 ? 0 : 1);
    }

    /// <summary>
    /// Shifts VX to the left by 1, then sets VF to 1 if the most significant
    /// bit of VX prior to that shift was set, or to 0 if it was unset.
    /// </summary>
    /// <remarks>
    /// Note that x is allowed to be F, and the assignment to VF must
    /// be done after the assignment to Vx.
    /// </remarks>
    public void OpCode8XYE(byte x, byte _)
    {
        bool isMostSigBitSet = (V[x] & 0b1000_0000) == 0b1000_0000;
        V[x] <<= 0x01;
        V[0xF] = (byte)(isMostSigBitSet ? 0x01 : 0x00);
    }

    /// <summary>
    /// Skips the next instruction if Vx does not equal Vy.
    /// </summary>
    /// <remarks>
    /// Usually the next instruction is a jump to skip a code block
    /// </remarks>
    public void OpCode9XY0(byte x, byte y)
    {
        if (V[x] != V[y])
        {
            PC += 2;
        }
    }

    /// <summary>
    /// Sets I to the address NNN.
    /// </summary>
    public void OpCodeANNN(ushort nnn)
    {
        I = nnn;
    }

    /// <summary>
    /// Jumps to the address NNN plus V0.
    /// </summary>
    public void OpCodeBNNN(ushort nnn)
    {
        PC = (ushort)(nnn + V[0]);
    }

    /// <summary>
    /// Sets VX to the result of a bitwise and operation
    /// on a random number (Typically: 0 to 255) and NN.
    /// </summary>
    public void OpCodeCXNN(byte x, byte nn)
    {
        var random = new Random();
        V[x] = (byte)(random.Next(0, 0xFF) & nn);
    }

    /// <summary>
    /// Draws a sprite at coordinate (Vx, Vy) that has a width of 8 pixels and a height of N pixels.
    /// </summary>
    /// <remarks>
    /// Source: https://stackoverflow.com/questions/17346592/how-does-chip-8-graphics-rendered-on-screen
    /// </remarks>
    public void OpCodeDXYN(byte X, byte Y, byte N)
    {
        // Initialize the collision detection as no collision detected (yet).
        V[0xF] = 0;

        // Draw N lines on the screen.
        for (int line = 0; line < N; line++)
        {
            // y is the starting line Y + current line. If y is larger than the total width of the screen then wrap around (this is the modulo operation).
            var y = (V[Y] + line) % 32;

            // The current sprite being drawn, each line is a new sprite.
            byte sprite = Memory[I + line];

            // Each bit in the sprite is a pixel on or off.
            for (int column = 0; column < 8; column++)
            {
                // Start with the current most significant bit. The next bit will be left shifted in from the right.
                if ((sprite & 0x80) != 0)
                {
                    // Get the current x position and wrap around if needed.
                    var x = (V[X] + column) % 64;

                    // Collision detection: If the target pixel is already set then set the collision detection flag in register VF.
                    if (Screen[y * 64 + x])
                    {
                        V[0xF] = 1;
                    }

                    // Enable or disable the pixel (XOR operation).
                    Screen[y * 64 + x] = !Screen[y * 64 + x];
                }

                // Shift the next bit in from the right.
                sprite <<= 0x1;
            }
        }
    }

    /// <summary>
    /// Skips the next instruction if the key stored in Vx is pressed.
    /// </summary>
    /// <remarks>
    /// Usually the next instruction is a jump to skip a code block.
    /// </remarks>
    public void OpCodeEX9E(byte x)
    {
        if (Keys[V[x]])
        {
            PC += 2;
        }
    }

    /// <summary>
    /// Skips the next instruction if the key stored in Vx is not pressed.
    /// </summary>
    /// <remarks>
    /// Usually the next instruction is a jump to skip a code block.
    /// </remarks>
    public void OpCodeEXA1(byte x)
    {
        if (!Keys[V[x]])
        {
            PC += 2;
        }
    }

    /// <summary>
    /// Sets Vx to the value of the delay timer.
    /// </summary>
    public void OpCodeFX07(byte x)
    {
        V[x] = DelayTimer;
    }

    /// <summary>
    /// A key press is awaited, and then stored in Vx.
    /// </summary>
    /// <remarks>
    /// Blocking operation; all instruction halted until next key event.
    /// Delay and sound timers should continue processing.
    /// </remarks>
    public void OpCodeFX0A(byte x)
    {
        Blocked = true;
        for (byte i = 0; i < 0xF; i++)
        {
            if (Keys[i])
            {
                V[x] = i;
                Blocked = false;
            }
        }
    }

    /// <summary>
    /// Sets the delay timer to Vx.
    /// </summary>
    public void OpCodeFX15(byte x)
    {
        DelayTimer = V[x];
    }

    /// <summary>
    /// Sets the sound timer to Vx.
    /// </summary>
    public void OpCodeFX18(byte x)
    {
        SoundTimer = V[x];
    }

    /// <summary>
    /// Adds Vx to I. VF is not affected.
    /// </summary>
    public void OpCodeFX1E(byte x)
    {
        I += V[x];
    }

    /// <summary>
    /// Sets I to the location of the sprite for the character in Vx.
    /// </summary>
    public void OpCodeFX29(byte x)
    {
        I = (byte)(V[x] * 5);
    }

    /// <summary>
    /// Stores the binary-coded decimal representation of Vx, with the hundreds
    /// digit in memory at location in I, the tens digit at location I+1, and
    /// the ones digit at location I+2.
    /// </summary>
    public void OpCodeFX33(byte x)
    {
        var number = V[x];
        Memory[I] = (byte)(number / 100);
        Memory[I + 1] = (byte)(number / 10 % 10);
        Memory[I + 2] = (byte)(number % 100 % 10);
    }

    /// <summary>
    /// Stores from V0 to Vx (including Vx) in memory, starting at address I.
    /// </summary>
    /// <remarks>
    /// The offset from I is increased by 1 for each value written, but I itself is left unmodified.
    /// </remarks>
    public void OpCodeFX55(byte x)
    {
        for (int i = 0; i <= x; i++)
        {
            Memory[I + i] = V[i];
        }
    }

    /// <summary>
    /// Fills from V0 to Vx (including Vx) with values from memory, starting at address I.
    /// </summary>
    /// <remarks>
    /// The offset from I is increased by 1 for each value read, but I itself is left unmodified.
    /// </remarks>
    public void OpCodeFX65(byte x)
    {
        for (int i = 0; i <= x; i++)
        {
            V[i] = Memory[I + i];
        }
    }
}
