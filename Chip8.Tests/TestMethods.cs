// Copyright (c) 2020 Ron
// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

namespace Chip8.Tests;

internal static class TestMethods
{
    public static void TestPC(byte[] rom, uint cycles, ushort expected)
    {
        var vm = new VirtualMachine(null, rom);
        vm.Cycle(cycles);
        var state = vm.Snapshot();
        Assert.AreEqual(expected, state.ProgramCounter);
    }

    public static void TestVariable(byte[] rom, uint cycles, int register, byte expected)
    {
        var vm = new VirtualMachine(null, rom);
        vm.Cycle(cycles);
        var state = vm.Snapshot();
        Assert.AreEqual(expected, state.Variables[register]);
    }

    public static void TestScreen(byte[] rom, uint cycles, bool[] expected)
    {
        var vm = new VirtualMachine(null, rom);
        vm.Cycle(cycles);
        var state = vm.Snapshot();
        CollectionAssert.AreEqual(expected, state.Screen.ToArray());
    }

    public static void TestScreen(string romFilename, uint cycles, string screenFilename)
    {
        var romPath = Path.Join("Timendus-chip8-test-suite", romFilename);
        var vm = new VirtualMachine(null, romPath);
        vm.Cycle(cycles);
        var state = vm.Snapshot();

        var expected = File.ReadAllText(Path.Join("Screens", screenFilename))
            .Where(static c => c != '\n')
            .Select(static c => c != '█')
            .ToArray();

        CollectionAssert.AreEqual(expected, state.Screen.ToArray());
    }
}
