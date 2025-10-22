// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

using static Chip8.Tests.TestMethods;

namespace Chip8.Tests;

[TestClass]
public class TestTimendusRom1
{
    private const string RomFilename = "1-chip8-logo.ch8";

    [TestMethod]
    public void Test_1_chip8_logo_cycle01()
    {
        TestScreen
        (
            romFilename: RomFilename,
            cycles: 1,
            screenFilename: "1-chip8-logo-cycle01.txt"
        );
    }

    [TestMethod]
    public void Test_1_chip8_logo_cycle05()
    {
        TestScreen
        (
            romFilename: RomFilename,
            cycles: 5,
            screenFilename: "1-chip8-logo-cycle05.txt"
        );
    }

    [TestMethod]
    public void Test_1_chip8_logo_cycle20()
    {
        TestScreen
        (
            romFilename: RomFilename,
            cycles: 20,
            screenFilename: "1-chip8-logo-cycle20.txt"
        );
    }

    [TestMethod]
    public void Test_1_chip8_logo_cycle30()
    {
        TestScreen
        (
            romFilename: RomFilename,
            cycles: 30,
            screenFilename: "1-chip8-logo-cycle30.txt"
        );
    }

    [TestMethod]
    public void Test_1_chip8_logo_cycle38()
    {
        TestScreen
        (
            romFilename: RomFilename,
            cycles: 38,
            screenFilename: "1-chip8-logo-cycle38.txt"
        );
    }

    [TestMethod]
    public void Test_1_chip8_logo_cycle39()
    {
        uint[] cyclesList = [39, 60, 99, 10_000];
        foreach (var cycles in cyclesList)
        {
            TestScreen
            (
                romFilename: RomFilename,
                cycles: cycles,
                screenFilename: "1-chip8-logo-cycle39.txt"
            );
        }
    }
}
