using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Tests;

public sealed class Xorshift64 : RandomNumberGenerator
{
    public new static Xorshift64 Create()
    {
        Span<byte> span = stackalloc byte[8];
        Fill(span);
        var state = MemoryMarshal.Read<ulong>(span);
        return Create(state);
    }
    public static Xorshift64 Create(ulong state)
    {
        return new Xorshift64(state);
    }
    private ulong _state;

    private Xorshift64(ulong state)
    {
        _state = state;
    }

    public ulong GetULong()
    {
        _state ^= _state << 13;
        _state ^= _state >> 7;
        _state ^= _state << 17;
        return _state;
    }
    public override void GetBytes(byte[] data)
    {
        int c = sizeof(ulong);
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        for (int i = 0; i < data.Length; ++i)
        {
            if (c >= sizeof(ulong))
            {
                var raw = GetULong();
                MemoryMarshal.Write(bytes, ref raw);
                c = 0;
            }
            data[i] = bytes[c++];
        }
    }
}
