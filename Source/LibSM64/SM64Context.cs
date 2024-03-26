using System.Runtime.InteropServices;
using System.Security.Cryptography;
using static LibSM64.Native;

namespace LibSM64;

public static class SM64Context
{
    public const int SM64_MARIO_TEXTURE_WIDTH = 64 * 11;
    public const int SM64_MARIO_TEXTURE_HEIGHT = 64;
    
    public const int SM64_MENU_TEXTURE_WIDTH = 16 * 41;
    public const int SM64_MENU_TEXTURE_HEIGHT = 16 + 64;
    
    public static Texture MarioTexture { get; private set; } = null!;
    public static Texture MenuTexture { get; private set; } = null!;

    private static readonly byte[] ExpectedRomHash =
    [
        0x20, 0xb8, 0x54, 0xb2, 0x39, 0x20, 0x3b, 0xaf, 0x6c, 0x96, 0x1b, 0x85, 0x0a, 0x4a, 0x51, 0xa2
    ];
    
    public static unsafe void InitializeFromROM(byte[] romBytes)
    {
        var romHash = MD5.HashData(romBytes);
        if (!romHash.SequenceEqual(ExpectedRomHash))
        {
            throw new InvalidDataException(
                "MD5 checksum did not match the expected value - Please use the .z64 (big-endian) version of the USA ROM.");
        }

        var marioTextureData = new byte[4 * SM64_MARIO_TEXTURE_WIDTH * SM64_MARIO_TEXTURE_HEIGHT];

        fixed (byte* pRom = romBytes)
        fixed (byte* pTexture = marioTextureData)
        {
            sm64_global_init(pRom, pTexture);
            sm64_audio_init(pRom);
        }

        MarioTexture = new Texture(SM64_MARIO_TEXTURE_WIDTH, SM64_MARIO_TEXTURE_HEIGHT);
        MarioTexture.SetData<byte>(marioTextureData);
        
        var menuTextureData = new byte[4 * SM64_MENU_TEXTURE_WIDTH * SM64_MENU_TEXTURE_HEIGHT];
        fixed (byte* pRom = romBytes)
        fixed (byte* pTexture = menuTextureData)
        {
            sm64_load_menu_texture_atlas(pRom, pTexture);
        }
        MenuTexture = new Texture(SM64_MENU_TEXTURE_WIDTH, SM64_MENU_TEXTURE_HEIGHT);
        MenuTexture.SetData<byte>(menuTextureData);
    }
    
    public static void Terminate()
    {
        sm64_global_terminate();
    }
    
    public static unsafe SM64SurfaceCollisionData*[] FindWallCollisions(SM64Vector3f position, float offsetY, float radius)
    {
        var colData = new SM64WallCollisionData()
        {
            pos = position,
            offsetY = offsetY,
            radius = radius,
        };
        
        int wallCount = sm64_surface_find_wall_collisions(ref colData);
        return wallCount switch
        {
            // Could this be done programatically? Probably
            0 => [],
            1 => [(SM64SurfaceCollisionData*)colData.walls[0]],
            2 => [(SM64SurfaceCollisionData*)colData.walls[0], (SM64SurfaceCollisionData*)colData.walls[1]],
            3 => [(SM64SurfaceCollisionData*)colData.walls[0], (SM64SurfaceCollisionData*)colData.walls[1], (SM64SurfaceCollisionData*)colData.walls[2]],
            // There will never be more than 4 walls
        };
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
}