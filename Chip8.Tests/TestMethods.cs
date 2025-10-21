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

    public static void TestScreen(byte[] rom, uint cycles, byte[] expected)
    {
        var vm = new VirtualMachine(null, rom);
        vm.Cycle(cycles);
        var state = vm.Snapshot();
        CollectionAssert.AreEqual(expected, state.Screen.ToArray());
    }

    public static void TestScreen(string rom, uint cycles, byte[] expected)
    {
        var vm = new VirtualMachine(null, rom);
        vm.Cycle(cycles);
        var state = vm.Snapshot();
        CollectionAssert.AreEqual(expected, state.Screen.ToArray());
    }
}
