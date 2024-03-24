using System.Security.Cryptography;
using LibSM64.Util;
using static LibSM64.Native;

namespace LibSM64;

public static class SM64Context
{
    public const int SM64_TEXTURE_WIDTH = 64 * 11;
    public const int SM64_TEXTURE_HEIGHT = 64;
    
    public static Texture MarioTexture { get; private set; } = null!;

    private static readonly byte[] ExpectedRomHash =
    [
        0x20, 0xb8, 0x54, 0xb2, 0x39, 0x20, 0x3b, 0xaf, 0x6c, 0x96, 0x1b, 0x85, 0x0a, 0x4a, 0x51, 0xa2
    ];
    
    public static void InitializeFromROM(byte[] romBytes)
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
    
    public static void Terminate()
    {
        sm64_global_terminate();
    }
    
    public static unsafe uint TickAudio(uint numQueuedSamples, uint numDesiredSamples, short[] audioBuffer)
    {
        fixed (short* pBuf = audioBuffer)
        {
            return sm64_audio_tick(numQueuedSamples, numDesiredSamples, pBuf);
        }
    }
    
    public static unsafe void PlaySound(SM64Sound sound, SM64Vector3f pos) => sm64_play_sound((int)sound, &pos);
    public static void PlaySoundGlobal(SM64Sound sound) => sm64_play_sound_global((int)sound);
    
    public static unsafe Texture LoadTextureFromROM(byte[] romBytes, uint offset, uint textureWidth, uint textureHeight, uint textureDepth)
    {
        fixed (byte* pIn = romBytes)
        {
            var header = Mio0.DecodeHeader(pIn);

            var outBuf = new byte[header.dest_size];
            fixed (byte* pOut = outBuf)
            {
                Mio0.Decode(header, pIn + offset, pOut);
        
                // Convert internal SM64 data to RGBA texture
                var imgData = new Color[textureWidth * textureHeight];
        
                if (textureDepth == 16) {
                    for (int i = 0; i < textureWidth * textureHeight; i++) {
                        imgData[i].R = (byte)(((outBuf[i*2] & 0xF8) >> 3) * 0xFF / 0x1F);
                        imgData[i].G = (byte)((((outBuf[i*2] & 0x07) << 2) | ((outBuf[i*2+1] & 0xC0) >> 6)) * 0xFF / 0x1F);
                        imgData[i].B = (byte)(((outBuf[i*2+1] & 0x3E) >> 1) * 0xFF / 0x1F);
                        imgData[i].A = (byte)((outBuf[i * 2 + 1] & 0x01) != 0 ? 0xFF : 0x00);
                    }
                } else if (textureDepth == 32) {
                    for (int i = 0; i < textureWidth * textureHeight; i++) {
                        imgData[i].R = outBuf[i*4];
                        imgData[i].G = outBuf[i*4+1];
                        imgData[i].B = outBuf[i*4+2];
                        imgData[i].A = outBuf[i*4+3];
                    }
                }

                return new Texture((int)textureWidth, (int)textureHeight, imgData);                
            }
        }
    }
}