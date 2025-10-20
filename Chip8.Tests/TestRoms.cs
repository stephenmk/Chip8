using static Chip8.Tests.TestMethods;

namespace Chip8.Tests;

[TestClass]
public class TestRoms
{
    private static byte[] GetScreen(string filename) =>
        File.ReadAllBytes(Path.Join("Screens", filename));

    [TestMethod]
    public void Test_RunC8TestRom_ShouldReturnOKScreen()
    {
        var screen = GetScreen("c8_ok_screen.dat");
        TestScreen(rom: "c8_test.ch8", cycles: 512, expected: screen);
    }

    [TestMethod]
    public void Test_RunBCTestRom_ShouldReturnBonScreen()
    {
        var screen = GetScreen("bc_bon_screen.dat");
        TestScreen(rom: "BC_test.ch8", cycles: 512, expected: screen);
    }

    [TestMethod]
    public void Test_RunCorax89TestRom_ShouldReturnAllOKScreen()
    {
        var screen = GetScreen("corax89_all_ok_screen.dat");
        TestScreen(rom: "test_opcode.ch8", cycles: 512, expected: screen);
    }
}
