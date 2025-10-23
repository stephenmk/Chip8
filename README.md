# Chip-8
A CHIP-8 interpreter written in C# (.NET 9.0)

https://github.com/user-attachments/assets/89a3885f-59dc-465d-9e16-8ddaf357cff8

*Roms/invaders.ch8*

This repo is forked from [ronazulay/Chip8](https://github.com/ronazulay/Chip8).
I added the following features:

* Updated to .NET 9.0 from .NET Core 3.1
* Updated to OpenGL4 from legacy OpenGL.
* Added a visual bell (flashing screen) effect.
* Implemented [Timendus's chip8 test suite v4.2](https://github.com/Timendus/chip8-test-suite) (work in progress)
    * Fixed various bugs to pass the `corax+` and `flags` tests
    * Added new features to pass the `quirks` tests

## To do
* Pass Timendus `keypad` and `beep` tests.
* Add implementation for Super Chip-8
* Add implementation for XO-CHIP
* Improve UI, add file menus and customizable keybinds, etc.

## Running

Keyboard keys 0–9, A–F map to the corresponding chip8 keys.

Key | Shortcut
-- | --
<kbd>Esc</kbd> | Exit
<kbd>Backspace</kbd> | Reset
<kbd>P</kbd> | Toggle pause
<kbd>S</kbd> | Pause, step, and print register contents to console
<kbd>R</kbd> | Print register contents to console
<kbd>G</kbd> | Print screen contents to console
<kbd>M</kbd> | Print memory contents to console
