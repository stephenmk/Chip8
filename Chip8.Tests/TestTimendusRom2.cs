// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

using static Chip8.Tests.TestMethods;

namespace Chip8.Tests;

[TestClass]
public class TestTimendusRom2
{
    private const string RomFilename = "2-ibm-logo.ch8";

    [TestMethod]
    public void Test_2_ibm_logo_loading()
    {
        Test_2_ibm_logo_cycle_range(start: 1, count: 4);
        Test_2_ibm_logo_cycle_range(start: 5, count: 3);
        Test_2_ibm_logo_cycle_range(start: 8, count: 3);
        Test_2_ibm_logo_cycle_range(start: 11, count: 3);
        Test_2_ibm_logo_cycle_range(start: 14, count: 3);
        Test_2_ibm_logo_cycle_range(start: 17, count: 3);
    }

    public static void Test_2_ibm_logo_cycle_range(int start, int count)
    {
        foreach (int cycles in Enumerable.Range(start, count))
        {
            TestScreen
            (
                romFilename: RomFilename,
                cycles: (uint)cycles,
                screenFilename: $"2-ibm-logo-cycle{start:D2}.txt"
            );
        }
    }

    [TestMethod]
    public void Test_2_ibm_logo_cycle20()
    {
        uint[] cycleList = [20, 39, 60, 90, 10_000];
        foreach (uint cycles in cycleList)
        {
            TestScreen
            (
                romFilename: RomFilename,
                cycles: cycles,
                screenFilename: "2-ibm-logo-cycle20.txt"
            );
        }
    }
}
