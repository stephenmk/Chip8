# Chip-8
A CHIP-8 emulator written in C# (.NET Core)

![Beautiful IBM logo](screenshot.png "Beautiful IBM logo")
*IBM logo*

> "CHIP-8 is an interpreted programming language, developed by Joseph Weisbecker. It was initially used on the COSMAC VIP and Telmac 1800 8-bit microcomputers in the mid-1970s. CHIP-8 programs are run on a CHIP-8 virtual machine. It was made to allow video games to be more easily programmed for these computers." — Wikipedia

This chip8 emulator was written by [@ronazulay](https://github.com/ronazulay) in 2020.
See [the readme](https://github.com/ronazulay/Chip8) in his repo for more information.

I updated the program to target .NET 9.0 and the latest version of the OpenTK library.
I also refactored and tidied up the program to make it a little more object-oriented.

## Running

Keyboard keys 0–9, A–F maps to the corresponding chip8 keys.

Keyboard shortcuts:

<kbd>Esc</kbd> = Exit

<kbd>Backspace</kbd> = Reset

<kbd>P</kbd>     = Pause / Play execution

<kbd>S</kbd>     = Single step

<kbd>R</kbd>     = Dump registers to console

<kbd>G</kbd>     = Dump screen contents to console

<kbd>M</kbd>     = Print memory contents to console
