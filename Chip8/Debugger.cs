// Copyright (c) 2020 Ron
// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

using System.Text;

namespace Chip8;

public class Debugger
{
    private readonly VirtualMachine _virtualMachine;

    public Debugger(VirtualMachine virtualMachine)
    {
        _virtualMachine = virtualMachine;
    }

    public void PrintRegisters()
    {
        var state = _virtualMachine.Snapshot();
        Console.WriteLine
        (
            $"""
            PC              0x{state.ProgramCounter:X4}
            OpCode          0x{state.OpCode:X4}
            I               0x{state.MemoryAddress:X4}
            sp              0x{state.StackPointer:X4}
            V{0x0:X}              0x{state.Variables[0x0]:X2}
            V{0x1:X}              0x{state.Variables[0x1]:X2}
            V{0x2:X}              0x{state.Variables[0x2]:X2}
            V{0x3:X}              0x{state.Variables[0x3]:X2}
            V{0x4:X}              0x{state.Variables[0x4]:X2}
            V{0x5:X}              0x{state.Variables[0x5]:X2}
            V{0x6:X}              0x{state.Variables[0x6]:X2}
            V{0x7:X}              0x{state.Variables[0x7]:X2}
            V{0x8:X}              0x{state.Variables[0x8]:X2}
            V{0x9:X}              0x{state.Variables[0x9]:X2}
            V{0xA:X}              0x{state.Variables[0xA]:X2}
            V{0xB:X}              0x{state.Variables[0xB]:X2}
            V{0xC:X}              0x{state.Variables[0xC]:X2}
            V{0xD:X}              0x{state.Variables[0xD]:X2}
            V{0xE:X}              0x{state.Variables[0xE]:X2}
            V{0xF:X}              0x{state.Variables[0xF]:X2}
            DelayTimer      {state.DelayTimer}
            SoundTimer      {state.SoundTimer}
            """
        );
    }

    public void PrintMemory()
    {
        var state = _virtualMachine.Snapshot();
        var output = new StringBuilder();

        for (int i = 0; i <= 0xfff; i += 8)
        {
            output.Append($"0x{i:X3}:");
            output.Append($" 0x{state.Memory[i]:X2}");
            output.Append($" 0x{state.Memory[i + 1]:X2}");
            output.Append($" 0x{state.Memory[i + 2]:X2}");
            output.Append($" 0x{state.Memory[i + 3]:X2}");
            output.Append($" 0x{state.Memory[i + 4]:X2}");
            output.Append($" 0x{state.Memory[i + 5]:X2}");
            output.Append($" 0x{state.Memory[i + 6]:X2}");
            output.Append($" 0x{state.Memory[i + 7]:X2}");
            output.AppendLine();
        }

        Console.WriteLine(output);
    }

    public void PrintScreen()
    {
        var state = _virtualMachine.Snapshot();
        var output = new StringBuilder();

        for (int i = 0; i < state.Screen.Length; i += 64)
        {
            output.Append($"0x{i:X3}:");
            output.Append($" 0x{state.Screen[i]:X2}");
            output.Append($" 0x{state.Screen[i + 1]:X2}");
            output.Append($" 0x{state.Screen[i + 2]:X2}");
            output.Append($" 0x{state.Screen[i + 3]:X2}");
            output.Append($" 0x{state.Screen[i + 4]:X2}");
            output.Append($" 0x{state.Screen[i + 5]:X2}");
            output.Append($" 0x{state.Screen[i + 6]:X2}");
            output.Append($" 0x{state.Screen[i + 7]:X2}");
            output.AppendLine();
        }

        output.AppendLine(" ----------------------------------------------------------------");
        for (int i = 0; i < 32; i++)
        {
            output.Append('|');
            for (int j = 0; j < 64; j++)
            {
                output.Append(state.Screen[i * 64 + j] > 0 ? "█" : " ");
            }
            output.AppendLine("|");
        }
        output.AppendLine(" ----------------------------------------------------------------");

        Console.WriteLine(output);
    }
}
