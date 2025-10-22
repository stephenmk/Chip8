// Copyright (c) 2020 Ron
// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

namespace Chip8;

/// <summary>
/// Window methods expected by Chip8.
/// </summary>
public interface IChip8Window
{
    void Beep();
    void UpdateScreen(IList<bool> screen);
}
