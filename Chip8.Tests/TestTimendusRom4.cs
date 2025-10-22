// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

using static Chip8.Tests.TestMethods;

namespace Chip8.Tests;

[TestClass]
public class TestTimendusRom4
{
    [TestMethod]
    public void Test_4_flags()
    {
        TestScreen
        (
            romFilename: "4-flags.ch8",
            cycles: 952,
            screenFilename: "4-flags-success.txt"
        );
    }
}
