namespace Chip8;

/// <remarks>
/// Not truly a snapshot since the ReadOnlySpans point to the current VM state.
/// </remarks>
public readonly ref struct StateSnapshot
{
    public ushort OpCode { get; init; }
    public ushort MemoryAddress { get; init; }
    public ushort ProgramCounter { get; init; }
    public ushort StackPointer { get; init; }
    public byte DelayTimer { get; init; }
    public byte SoundTimer { get; init; }
    public bool Blocked { get; init; }
    public ReadOnlySpan<ushort> Stack { get; init; }
    public ReadOnlySpan<byte> Variables { get; init; }
    public ReadOnlySpan<byte> Memory { get; init; }
    public ReadOnlySpan<byte> Screen { get; init; }
    public ReadOnlySpan<byte> Keys { get; init; }
}
