using System.Runtime.InteropServices;
using System.Security.Cryptography;
using LibSM64Sharp.LowLevel;

namespace LibSM64Sharp.Impl;

public sealed partial class Sm64Context : ISm64Context
{
    private const int SM64_TEXTURE_WIDTH = 64 * 11;
    private const int SM64_TEXTURE_HEIGHT = 64;
    private readonly Texture marioTextureImage_;

    public static void RegisterDebugPrintFunction(
        DebugPrintFuncDelegate handler)
    {
        LibSm64Interop.sm64_register_debug_print_function(
            Marshal.GetFunctionPointerForDelegate(handler));
    }

    public static ISm64Context InitFromRom(byte[] romBytes)
        => new Sm64Context(romBytes);

    private Sm64Context(byte[] romBytes)
    {
        var expectedUsaHash = new byte[]
        {
            0x20, 0xb8, 0x54, 0xb2, 0x39, 0x20, 0x3b, 0xaf, 0x6c, 0x96, 0x1b, 0x85, 0x0a, 0x4a, 0x51, 0xa2,
        };
        var actualUsaHash = MD5.HashData(romBytes);
        if (!expectedUsaHash.SequenceEqual(actualUsaHash))
        {
            throw new InvalidDataException(
                "MD5 checksum did not match the expected value--" +
                "please use the .z64 (big-endian) version of the USA ROM.");
        }

        var romHandle = GCHandle.Alloc(romBytes, GCHandleType.Pinned);
        var textureData = new byte[4 * SM64_TEXTURE_WIDTH * SM64_TEXTURE_HEIGHT];
        var textureDataHandle = GCHandle.Alloc(textureData, GCHandleType.Pinned);

        LibSm64Interop.sm64_global_init(
            romHandle.AddrOfPinnedObject(),
            textureDataHandle.AddrOfPinnedObject());
        LibSm64Interop.sm64_audio_init(romHandle.AddrOfPinnedObject());

        marioTextureImage_ = new Texture(SM64_TEXTURE_WIDTH, SM64_TEXTURE_HEIGHT);
        marioTextureImage_.SetData<byte>(textureData);

        romHandle.Free();
        textureDataHandle.Free();
    }

    ~Sm64Context()
    {
        ReleaseUnmanagedResources_();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources_();
        GC.SuppressFinalize(this);
    }

    private static void ReleaseUnmanagedResources_() => LibSm64Interop.sm64_global_terminate();
}