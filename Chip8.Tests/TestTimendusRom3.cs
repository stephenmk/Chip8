// Copyright (c) 2020 Ron
// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

using static Chip8.Tests.TestMethods;

namespace Chip8.Tests;

[TestClass]
public class TestTimendusRom3
{
    [TestMethod]
    public void Test_3_corax_plus()
    {
        TestScreen
        (
            romFilename: "3-corax+.ch8",
            cycles: 308,
            screenFilename: "3-corax+-success.txt"
        );
    }
}
