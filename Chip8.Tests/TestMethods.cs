namespace Chip8.Tests;

internal static class TestMethods
{
    private static MachineState GetState<T>(T rom, uint cycles) where T: class
    {
        var vm = rom switch
        {
            string romPath => new VirtualMachine(null, romPath),
            byte[] romBytes => new VirtualMachine(null, romBytes),
            _ => throw new Exception()
        };
        vm.EmulateCycles(cycles);
        return vm.Snapshot();
    }

    public static void TestScreen<T>(T rom, uint cycles, byte[] expected) where T: class
    {
        var state = GetState(rom, cycles);
        var actual = state.Screen.ToArray();
        CollectionAssert.AreEqual(expected, actual);
    }

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
}
