using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace LibSM64;

public class SM64Context
{
    public const int SM64_TEXTURE_WIDTH = 64 * 11;
    public const int SM64_TEXTURE_HEIGHT = 64;
    private readonly Texture MarioTexture;

    private static readonly byte[] ExpectedRomHash =
    [
        0x20, 0xb8, 0x54, 0xb2, 0x39, 0x20, 0x3b, 0xaf, 0x6c, 0x96, 0x1b, 0x85, 0x0a, 0x4a, 0x51, 0xa2
    ];

    public SM64Context(byte[] romBytes)
    {
        var romHash = MD5.HashData(romBytes);
        if (!romHash.SequenceEqual(ExpectedRomHash))
        {
            throw new InvalidDataException(
                "MD5 checksum did not match the expected value - Please use the .z64 (big-endian) version of the USA ROM.");
        }

        var textureData = new byte[4 * SM64_TEXTURE_WIDTH * SM64_TEXTURE_HEIGHT];

        unsafe
        {
            fixed (byte* pRom = romBytes)
            fixed (byte* pTexture = textureData)
            {
                sm64_global_init(pRom, pTexture);
                sm64_audio_init(pRom);
            }
        }

        MarioTexture = new Texture(SM64_TEXTURE_WIDTH, SM64_TEXTURE_HEIGHT);
        MarioTexture.SetData<byte>(textureData);
    }

    #region Native Interop

    [DllImport("sm64")]
    private static extern unsafe void sm64_global_init(byte* rom, byte* outTexture);

    [DllImport("sm64")]
    private static extern void sm64_global_terminate();

    [DllImport("sm64")]
    private static extern unsafe void sm64_audio_init(byte* rom);

    #endregion
}