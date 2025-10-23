// Copyright (c) 2020 Ron
// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

using System.CommandLine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;

namespace Chip8;

class Program
{
    static void Main(string[] args)
    {
        var romFileArgument = new Option<FileInfo>("--rom-file")
        {
            Description = "Path to CHIP-8 ROM file (*.ch8)",
            Required = false,
        };

        var rootCommand = new RootCommand("Chip8")
        {
            romFileArgument,
        };

        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            foreach (var parseError in parseResult.Errors)
            {
                Console.Error.WriteLine(parseError.Message);
            }
            return;
        }

        var romFile = parseResult.GetValue(romFileArgument);

        var gameSettings = new GameWindowSettings
        {
            UpdateFrequency = 60
        };

        var nativeSettings = new NativeWindowSettings
        {
            ClientSize = new Vector2i(1024, 640),
            Profile = ContextProfile.Core,
            Title = "Chip8 Emulator"
        };

        using var window = new Window(gameSettings, nativeSettings);

        if (romFile is not null)
        {
            window.LoadRom(romFile.FullName);
        }

        window.Run();
    }
}
