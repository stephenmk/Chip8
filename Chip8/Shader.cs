// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT
// Reference: https://opentk.net/learn/chapter1/2-hello-triangle.html

using OpenTK.Graphics.OpenGL4;

namespace Chip8;

public sealed class Shader : IDisposable
{
    private readonly int _handle;
    private bool _disposedValue = false;

    public Shader(string vertexSourceFilename, string fragmentSourceFilename)
    {
        var vertexShader = GetShader(vertexSourceFilename, ShaderType.VertexShader);
        var fragmentShader = GetShader(fragmentSourceFilename, ShaderType.FragmentShader);
        _handle = GetHandle(vertexShader, fragmentShader);
    }

    private static int GetShader(string filename, ShaderType type)
    {
        var path = Path.Join("Shaders", filename);
        var shaderSourceCode = File.ReadAllText(path);
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, shaderSourceCode);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(shader);
            Console.Error.WriteLine(infoLog);
        }
        return shader;
    }

    private static int GetHandle(int vertexShader, int fragmentShader)
    {
        int handle = GL.CreateProgram();
        GL.AttachShader(handle, vertexShader);
        GL.AttachShader(handle, fragmentShader);
        GL.LinkProgram(handle);
        GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(handle);
            Console.Error.WriteLine(infoLog);
        }
        GL.DetachShader(handle, vertexShader);
        GL.DetachShader(handle, fragmentShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);
        return handle;
    }

    public void Use()
    {
        GL.UseProgram(_handle);
    }

    private void Dispose(bool _)
    {
        if (!_disposedValue)
        {
            GL.DeleteProgram(_handle);
            _disposedValue = true;
        }
    }

    ~Shader()
    {
        if (_disposedValue == false)
        {
            Console.Error.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool _)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
