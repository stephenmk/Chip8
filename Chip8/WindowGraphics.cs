// Copyright (c) 2025 Stephen Kraus
// SPDX-License-Identifier: MIT

using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace Chip8;

public class WindowGraphics : IDisposable
{
    private bool _disposedValue;
    private readonly Shader? _shader;

    public WindowGraphics(uint textureSize)
    {
        GL.ClearColor(Color.Black);

        // Create and bind VAO/VBO/EBO
        int vertexArray = GL.GenVertexArray();
        int vertexBuffer = GL.GenBuffer();
        int elementArrayBuffer = GL.GenBuffer();

        GL.BindVertexArray(vertexArray);

        // Flipping texture vertically.
        float[] vertices = [
            -1f, -1f, 0f, 1f, // Bottom-left (-1f, -1f) to top-left (0f, 1f).
             1f, -1f, 1f, 1f, // Bottom-right to top-right.
             1f,  1f, 1f, 0f, // Top-right to bottom-right.
            -1f,  1f, 0f, 0f, // Top-left to bottom-left.
        ];

        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        // Two triangles form a rectangle to cover the screen.
        uint[] indices = [
            0, 1, 2,
            2, 3, 0,
        ];

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementArrayBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        // Position attribute
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // TexCoord attribute
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Create and compile shaders
        _shader = new Shader("shader.vert", "shader.frag");

        // Create texture
        int texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);

        var pixels = new byte[textureSize];
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, 64, 32, 0, PixelFormat.Red, PixelType.Byte, pixels);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    public void Render(bool[] pixels)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _shader?.Use();

        var byteArray = pixels
            .Select(static pixel => (byte)(pixel ? 0xFF : 0x00))
            .ToArray();

        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 64, 32, PixelFormat.Red, PixelType.UnsignedByte, byteArray);

        // 6: length of `uint[] indices`
        // 0: pointer to location of the `indices` array.
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
    }

    public virtual void Resize(int x, int y, int width, int height)
    {
        GL.Viewport(x, y, width, height);
    }

    protected virtual void Dispose(bool _)
    {
        if (!_disposedValue)
        {
            _shader?.Dispose();
            _disposedValue = true;
        }
    }

    ~WindowGraphics()
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
