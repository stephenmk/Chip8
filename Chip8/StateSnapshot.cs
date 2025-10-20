namespace Chip8;

public readonly ref struct StateSnapshot
{
    public ushort OpCode { get; init; }
    public ushort I { get; init; }
    public ushort PC { get; init; }
    public ReadOnlySpan<ushort> Stack { get; init; }
    public ushort SP { get; init; }
    public byte DelayTimer { get; init; }
    public byte SoundTimer { get; init; }
    public ReadOnlySpan<byte> V { get; init; }
    public ReadOnlySpan<byte> Memory { get; init; }
    public ReadOnlySpan<byte> Screen { get; init; }
}
