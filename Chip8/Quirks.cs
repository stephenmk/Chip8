// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

namespace Chip8;

public readonly record struct Quirks
{
    public bool VfReset { get; init; } = true;
    public bool Memory { get; init; } = true;
    public bool DisplayWait { get; init; } = true;
    public bool Clipping { get; init; } = true;
    public bool Shifting { get; init; } = false;
    public bool Jumping { get; init; } = false;
    public Quirks() { }
}
