namespace Chip8.Tests;

internal static class TestMethods
{
    public static void TestPC(byte[] rom, uint cycles, ushort expected)
    {
        var state = GetState(rom, cycles);
        var actual = state.PC;
        Assert.AreEqual(expected, actual);
    }

    public static void TestVariable(byte[] rom, uint cycles, int register, byte expected)
    {
        var state = GetState(rom, cycles);
        var actual = state.V.ToArray()[register];
        Assert.AreEqual(expected, actual);
    }

    public static void TestScreen(string rom, uint cycles, byte[] expected)
    {
        GenericTestScreen(rom, cycles, expected);
    }

    public static void TestScreen(byte[] rom, uint cycles, byte[] expected)
    {
        GenericTestScreen(rom, cycles, expected);
    }

    private static void GenericTestScreen<T>(T rom, uint cycles, byte[] expected) where T : class
    {
        var state = GetState(rom, cycles);
        var actual = state.Screen.ToArray();
        CollectionAssert.AreEqual(expected, actual);
    }

    private static StateSnapshot GetState<T>(T rom, uint cycles) where T : class
    {
        var vm = rom switch
        {
            string romPath => new VirtualMachine(null, romPath),
            byte[] romBytes => new VirtualMachine(null, romBytes),
            _ => throw new Exception()
        };
        vm.Cycle(cycles);
        return vm.Snapshot();
    }
}
