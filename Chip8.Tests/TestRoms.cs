using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chip8.Tests;

[TestClass]
public class TestRoms
{
    private static byte[] ExpectedScreen(string filename) =>
        File.ReadAllBytes(Path.Join("Screens", filename));

    [TestMethod]
    public void Test_RunC8TestRom_ShouldReturnOKScreen()
    {
        var expectedScreen = ExpectedScreen("c8_ok_screen.dat");
        var vm = new VirtualMachine(null, "c8_test.ch8");
        vm.EmulateCycles(512);
        CollectionAssert.AreEqual(expectedScreen, vm.Screen);
    }

    [TestMethod]
    public void Test_RunBCTestRom_ShouldReturnBonScreen()
    {
        var expected = ExpectedScreen("bc_bon_screen.dat");
        var vm = new VirtualMachine(null, "BC_test.ch8");
        vm.EmulateCycles(512);
        CollectionAssert.AreEqual(expected, vm.Screen);
    }

    [TestMethod]
    public void Test_RunCorax89TestRom_ShouldReturnAllOKScreen()
    {
        var expected = ExpectedScreen("corax89_all_ok_screen.dat");
        var vm = new VirtualMachine(null, "test_opcode.ch8");
        vm.EmulateCycles(512);
        CollectionAssert.AreEqual(expected, vm.Screen);
    }
}
