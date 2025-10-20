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
        var output = new StringBuilder();
        output.AppendLine($"PC              0x{_vm.PC:X4}");
        output.AppendLine($"OpCode          0x{_vm.OpCode:X4}");
        output.AppendLine($"I               0x{_vm.I:X4}");
        output.AppendLine($"sp              0x{_vm.SP:X4}");

        foreach (var register in Enumerable.Range(0, 16))
        {
            output.AppendLine($"V{register:X}              0x{_vm.V[register]:X2}");
        }

        output.AppendLine($"DelayTimer      {_vm.DelayTimer}");
        output.AppendLine($"SoundTimer      {_vm.SoundTimer}");

        Console.WriteLine(output);
    }

    public void PrintMemory()
    {
        var output = new StringBuilder();

        for (int i = 0; i <= 0xfff; i += 8)
        {
            output.Append($"0x{i:X3}:");
            output.Append($" 0x{_vm.Memory[i]:X2}");
            output.Append($" 0x{_vm.Memory[i + 1]:X2}");
            output.Append($" 0x{_vm.Memory[i + 2]:X2}");
            output.Append($" 0x{_vm.Memory[i + 3]:X2}");
            output.Append($" 0x{_vm.Memory[i + 4]:X2}");
            output.Append($" 0x{_vm.Memory[i + 5]:X2}");
            output.Append($" 0x{_vm.Memory[i + 6]:X2}");
            output.Append($" 0x{_vm.Memory[i + 7]:X2}");
            output.AppendLine();
        }

        Console.WriteLine(output);
    }

    public void PrintScreen()
    {
        var output = new StringBuilder();

        for (int i = 0; i < _vm.Screen.Length; i += 64)
        {
            output.Append($"0x{i:X3}:");
            output.Append($" 0x{_vm.Screen[i]:X2}");
            output.Append($" 0x{_vm.Screen[i + 1]:X2}");
            output.Append($" 0x{_vm.Screen[i + 2]:X2}");
            output.Append($" 0x{_vm.Screen[i + 3]:X2}");
            output.Append($" 0x{_vm.Screen[i + 4]:X2}");
            output.Append($" 0x{_vm.Screen[i + 5]:X2}");
            output.Append($" 0x{_vm.Screen[i + 6]:X2}");
            output.Append($" 0x{_vm.Screen[i + 7]:X2}");
            output.AppendLine();
        }

        output.AppendLine(" ----------------------------------------------------------------");
        for (int i = 0; i < 32; i++)
        {
            output.Append('|');
            for (int j = 0; j < 64; j++)
            {
                output.Append(_vm.Screen[i * 64 + j] > 0 ? "█" : " ");
            }
            output.AppendLine("|");
        }
        output.AppendLine(" ----------------------------------------------------------------");

        Console.WriteLine(output);
    }
}
