using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chip8.Tests;

[TestClass]
public class TestOpCodes
{
    [TestMethod]
    public void Test_OpCode00E0_ShouldClearScreen()
    {
        byte[] rom = [
            0x00, 0xE0  // 0x00E0 - Clear the screen
        ];
        var vm = new VirtualMachine(null, rom);
        vm.EmulateCycles(1);
        var empty = new byte[64 * 32];
        CollectionAssert.AreEqual(empty, vm.Gfx);
    }

    [TestMethod]
    public void Test_OpCode00EE_ShouldSetPCTo0x202()
    {
        byte[] rom = [
            0x22, 0x04,  // Call subroutine at 0x204
            0x13, 0x37,  // Execution should continue here at 0x202
            0x00, 0xEE,  // Return to caller
        ];
        var vm = new VirtualMachine(null, rom);
        vm.EmulateCycles(2);
        Assert.AreEqual(0x202, vm.PC);
    }

    [TestMethod]
    public void Test_OpCode1NNN_ShouldSetPCTo0x123()
    {
        byte[] rom = [
            0x11, 0x23  // Jump to address 0x123
        ];
        var vm = new VirtualMachine(null, rom);
        vm.EmulateCycles(1);

        Assert.AreEqual(0x123, vm.PC);
    }

    [TestMethod]
    public void Test_OpCode3XNN_ShouldSetPCTo0x206()
    {
        byte[] rom = [
            0x60, 0x11,  // Set V0 to 0x11.
            0x30, 0x11,  // Should skip the next instruction
        ];
        var vm = new VirtualMachine(null, rom);
        vm.EmulateCycles(2);
        Assert.AreEqual(0x206, vm.PC);
    }

    [TestMethod]
    public void Test_OpCode4XNN_ShouldSetPCTo0x206()
    {
        byte[] rom = [
            0x60, 0x11,  // Set V0 to 0x11.
            0x40, 0x12,  // Should skip the next instruction
        ];
        var vm = new VirtualMachine(null, rom);
        vm.EmulateCycles(2);
        Assert.AreEqual(0x206, vm.PC);
    }

    [TestMethod]
    public void Test_OpCode5XY0_ShouldSetPCTo0x208()
    {
        byte[] rom = [
            0x60, 0x11,  // Set V0 to 0x11.
            0x61, 0x11,  // Set V1 to 0x11.
            0x50, 0x10,  // Should skip the next instruction
        ];
        var vm = new VirtualMachine(null, rom);
        vm.EmulateCycles(3);
        Assert.AreEqual(0x208, vm.PC);
    }

    [TestMethod]
    public void Test_OpCode6XNN_ShouldSetV0To0xAA()
    {
        byte[] rom = [
            0x60, 0xAA  // Set V0 to 0xAA.
        ];
        var vm = new VirtualMachine(null, rom);
        vm.EmulateCycles(1);
        Assert.AreEqual(0xAA, vm.V[0]);
    }

    [TestMethod]
    public void Test_OpCode8XY5_ShouldSetVFFlag0()
    {
        byte[] rom = [
            0x60, 0x05,  // Set V0 to 0x5.
            0x61, 0x06,  // Set V1 to 0x6.
            0x80, 0x15,  // Should set VF flag to 0 (borrow)
        ];
        var vm = new VirtualMachine(null, rom);
        vm.EmulateCycles(3);
        Assert.AreEqual(0x0, vm.V[0xF]);
    }

    [TestMethod]
    public void Test_OpCode8XY5_ShouldSetVFFlag1()
    {
        byte[] rom = [
            0x60, 0xF1,  // Set VF to 0x1.
            0x60, 0x05,  // Set V0 to 0x6.
            0x61, 0x04,  // Set V1 to 0x4.
            0x80, 0x15,  // Should set VF flag to 1 (no borrow)
        ];
        var vm = new VirtualMachine(null, rom);
        vm.EmulateCycles(4);
        Assert.AreEqual(0x1, vm.V[0xF]);
    }
}
