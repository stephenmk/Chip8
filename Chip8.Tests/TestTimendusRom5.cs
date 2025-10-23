// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

using static Chip8.Tests.TestMethods;

namespace Chip8.Tests;

[TestClass]
public class TestTimendusRom5
{
    [TestMethod]
    public void Test_5_all_quirks_off()
    {
        Quirks quirks = default; // all properties false
        Test(quirks, 600, "5-quirks-all-off.txt");
    }

    [TestMethod]
    public void Test_5_default_quirks()
    {
        Quirks quirks = new(); // constructor initializes properties to normal defaults.
        Test(quirks, 1200, "5-quirks-defaults.txt");
    }

    [TestMethod]
    public void Test_5_all_quirks_on()
    {
        Quirks quirks = new()
        {
            VfReset = true,
            Memory = true,
            DisplayWait = true,
            Clipping = true,
            Shifting  = true,
            Jumping  = true,
        };
        Test(quirks, 1200, "5-quirks-all-on.txt");
    }

    /// <remarks>
    /// The `DisplayWait` quirk requires a higher instruction rate (ips) because
    /// the machine is limited to drawing 1 sprite per frame (at 60 FPS). The thread blocks
    /// when the limit is reached, and many cycles are spent doing nothing but waiting.
    /// </remarks>
    private static void Test(Quirks quirks, int ips, string expectedScreenFilename)
    {
        var romBytes = TimendusRomBytes("5-quirks.ch8");
        VirtualMachine vm = new(null, romBytes, quirks, ips);

        // Cycle to the result screen
        vm.Cycle(10_000);
        vm.KeyDown(0x1);
        vm.Cycle((uint)ips); // hold it down for 1 second worth of instructions.
        vm.KeyUp(0x1);
        vm.Cycle(10_000);

        var state = vm.Snapshot();

        var expectedScreen = ExpectedScreen(expectedScreenFilename);
        var actualScreen = state.Screen.ToArray();

        CollectionAssert.AreEqual(expectedScreen, actualScreen);
    }
}
