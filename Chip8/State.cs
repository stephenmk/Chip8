// Copyright (c) 2020 Ron
// Copyright (c) 2025 Stephen Kraus
// Opcode method documentation comments from https://en.wikipedia.org/wiki/CHIP-8
// SPDX-License-Identifier: MIT

namespace Chip8;

internal class State
{
    private const ushort RomStart = 0x200;
    private readonly int InstructionsPerTimerCycle;

    private ushort OpCode;
    private ushort I;  // 12bit register (for memory address)
    private ushort PC;  // 12bit Program Counter
    private byte SP;  // 8bit Stack Pointer

    private bool Beep;
    private bool Blocked;
    private bool DisplayWait;
    private byte DelayTimer;
    private byte SoundTimer;

    private int TimerCycleCount;
    private UInt128 InstructionsProcessed;

    private readonly byte[] V;  // Register variables (16 available, 0 to F)
    private readonly byte[] Memory;
    private readonly bool[] Screen;
    private readonly ushort[] Stack;
    private readonly bool[] Keys;  // Pressed keys, ranging from 0 to F.

    public State(int instructionsPerSecond, IList<byte> font, IList<byte> rom)
    {
        // Timers should be updated 60 times per second (60 Hz).
        InstructionsPerTimerCycle = instructionsPerSecond / 60;

        OpCode = 0x0000;
        I = 0x000;
        PC = RomStart;
        SP = 0x0;

        Blocked = false;
        DisplayWait = false;
        DelayTimer = 0;
        SoundTimer = 0;
        TimerCycleCount = 0;
        InstructionsProcessed = 0;

        Stack = new ushort[16];
        Memory = new byte[4096];
        Screen = new bool[64 * 32];
        V = new byte[16];
        Keys = new bool[16];

        // Load fonts.
        font.CopyTo(Memory, 0);

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
        DisplayWait = DisplayWait,
        DelayTimer = DelayTimer,
        SoundTimer = SoundTimer,
        TimerCycleCount = TimerCycleCount,
        InstructionsProcessed = InstructionsProcessed,
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
        I = 0x000;
        PC = RomStart;
        SP = 0x0;
        Beep = false;
        Blocked = false;
        DisplayWait = false;
        DelayTimer = 0;
        SoundTimer = 0;
        TimerCycleCount = 0;
        InstructionsProcessed = 0;
        OpCode00E0();  // Clear screen
    }

    public ushort NextInstruction()
    {
        if (!Blocked)
        {
            OpCode = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);
            PC += 2;
        }
        return OpCode;
    }

    /// <remarks>
    /// Timers should be updated 60 times per second (60 Hz).
    /// </remarks>
    /// <returns><c>true</c> if beeping, otherwise <c>false</c>.</returns>
    public bool UpdateTimers()
    {
        InstructionsProcessed++;
        if ((++TimerCycleCount % InstructionsPerTimerCycle) == 0)
        {
            DisplayWait = false;
            TimerCycleCount = 0;
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
    public void OpCode3XNN(byte x, byte nn)
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
    public void OpCode4XNN(byte x, byte nn)
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
    public void OpCode6XNN(byte x, byte nn)
    {
        V[x] = nn;
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
    public void OpCode8XY1(byte x, byte y, bool quirk)
    {
        V[x] = (byte)(V[x] | V[y]);
        if (quirk)
        {
            V[0xF] = 0x00;
        }
    }

    /// <summary>
    /// Sets Vx to Vx and Vy (bitwise AND operation).
    /// </summary>
    public void OpCode8XY2(byte x, byte y, bool quirk)
    {
        V[x] = (byte)(V[x] & V[y]);
        if (quirk)
        {
            V[0xF] = 0x00;
        }
    }

    /// <summary>
    /// Sets Vx to Vx xor Vy.
    /// </summary>
    public void OpCode8XY3(byte x, byte y, bool quirk)
    {
        V[x] = (byte)(V[x] ^ V[y]);
        if (quirk)
        {
            V[0xF] = 0x00;
        }
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
    public void OpCode8XY6(byte x, byte y, bool quirk)
    {
        if (!quirk)
        {
            V[x] = V[y];
        }
        byte leastSigBit = (byte)(V[x] & 0b0000_0001);
        V[x] >>= 1;
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
        V[0xF] = (byte)(diff < 0 ? 0x00 : 0x01);
    }

    /// <summary>
    /// Shifts VX to the left by 1, then sets VF to 1 if the most significant
    /// bit of VX prior to that shift was set, or to 0 if it was unset.
    /// </summary>
    /// <remarks>
    /// Note that x is allowed to be F, and the assignment to VF must
    /// be done after the assignment to Vx.
    /// </remarks>
    public void OpCode8XYE(byte x, byte y, bool quirk)
    {
        if (!quirk)
        {
            V[x] = V[y];
        }
        bool isMostSigBitSet = (V[x] & 0b1000_0000) == 0b1000_0000;
        V[x] <<= 1;
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
    /// <remarks>
    /// https://tobiasvl.github.io/blog/write-a-chip-8-emulator/#bnnn-jump-with-offset
    /// </remarks>
    public void OpCodeBNNN(ushort nnn, bool quirk)
    {
        if (quirk)
        {
            byte x = (byte)((nnn & 0xF00) >> 8);
            PC = (ushort)(nnn + V[x]);
        }
        else
        {
            PC = (ushort)(nnn + V[0x0]);
        }
    }

    /// <summary>
    /// Sets VX to the result of a bitwise and operation
    /// on a random number (Typically: 0 to 255) and NN.
    /// </summary>
    public void OpCodeCXNN(byte x, byte nn)
    {
        var random = new Random();
        V[x] = (byte)(random.Next(0x00, 0xFF) & nn);
    }

    /// <summary>
    /// Draws a sprite at coordinate (Vx, Vy) that has a width of 8 pixels and a height of `n` (at most 15) pixels.
    /// </summary>
    /// <remarks>
    /// https://www.laurencescotford.net/2020/07/19/chip-8-on-the-cosmac-vip-drawing-sprites/
    /// </remarks>
    public void OpCodeDXYN(byte x, byte y, byte n, bool displayWaitQuirk, bool clippingQuirk)
    {
        if (displayWaitQuirk && DisplayWait)
        {
            Blocked = true;
            return;
        }
        else
        {
            Blocked = false;
            DisplayWait = true;
        }

        // Initialize the collision detection as no collision detected (yet).
        V[0xF] = 0x00;

        // Draw `n` lines on the screen (at most 0xF, i.e. 15)
        for (int line = 0; line < n; line++)
        {
            // screenY is the starting line `Vy` + current line. If larger than the total
            // width of the screen then wrap around (this is the modulo operation).
            int startingY = clippingQuirk ? V[y] % 32 : V[y];

            if (clippingQuirk && (startingY + line >= 32))
                continue;

            int screenY = (startingY + line) % 32;

            // The current part of the sprite being drawn. Each line has a new part.
            byte spritePart = Memory[I + line];

            // Each bit in the sprite is a pixel on or off.
            bool[] pixelIsOn = ParseSpritePart(spritePart);

            for (int column = 0; column < pixelIsOn.Length; column++)
            {
                // Get the current x position and wrap around if needed.
                int startingX = clippingQuirk ? V[x] % 64 : V[x];

                if (clippingQuirk && (startingX + column >= 64))
                    continue;

                int screenX = (startingX + column) % 64;
                int pixelIndex = screenY * 64 + screenX;

                // Collision detection: If the target pixel is already set,
                // then set the collision detection flag in register VF.
                if (Screen[pixelIndex])
                {
                    V[0xF] = 0x01;
                }

                // Enable or disable the pixel (XOR operation).
                Screen[pixelIndex] ^= pixelIsOn[column];
            }
        }
    }

    /// <summary>
    /// Parse one out of the `n` 8-bit segments of a sprite.
    /// </summary>
    private static bool[] ParseSpritePart(byte spritePart)
    {
        // Each bit in the sprite part is a pixel on or off.
        var pixelIsOn = new bool[8];

        for (int i = 0; i < pixelIsOn.Length; i++)
        {
            // Start with the current most significant bit.
            pixelIsOn[i] = (spritePart & 0b1000_0000) != 0;

            // Shift the next bit in from the right.
            spritePart <<= 1;
        }

        return pixelIsOn;
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
    /// Due to a quirk, I is left pointing to the address following the last one read into a variable.
    /// </remarks>
    public void OpCodeFX55(byte x, bool quirk)
    {
        for (int i = 0; i <= x; i++)
        {
            Memory[I + i] = V[i];
        }
        if (quirk)
        {
            I += (ushort)(x + 1);
        }
    }

    /// <summary>
    /// Fills from V0 to Vx (including Vx) with values from memory, starting at address I.
    /// </summary>
    /// <remarks>
    /// The offset from I is increased by 1 for each value read, but I itself is left unmodified.
    /// Due to a quirk, I is left pointing to the address following the last one read into a variable.
    /// </remarks>
    public void OpCodeFX65(byte x, bool quirk)
    {
        for (int i = 0; i <= x; i++)
        {
            V[i] = Memory[I + i];
        }
        if (quirk)
        {
            I += (ushort)(x + 1);
        }
    }
}
