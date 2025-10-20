using System.Text;

namespace Chip8;

public class Debugger
{
    private readonly VirtualMachine _vm;

    public Debugger(VirtualMachine vm)
    {
        _vm = vm;
    }

    public void PrintRegisters()
    {
        var state = _vm.Snapshot();
        var output = new StringBuilder();

        output.AppendLine($"PC              0x{state.PC:X4}");
        output.AppendLine($"OpCode          0x{state.OpCode:X4}");
        output.AppendLine($"I               0x{state.I:X4}");
        output.AppendLine($"sp              0x{state.SP:X4}");

        foreach (var register in Enumerable.Range(0, 16))
        {
            output.AppendLine($"V{register:X}              0x{state.V[register]:X2}");
        }

        output.AppendLine($"DelayTimer      {state.DelayTimer}");
        output.AppendLine($"SoundTimer      {state.SoundTimer}");

        Console.WriteLine(output);
    }

    public void PrintMemory()
    {
        var state = _vm.Snapshot();
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
        var state = _vm.Snapshot();
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
