using System.Runtime.InteropServices;

namespace LibSM64;

public class Native
{
    #region Structs
    
    [StructLayout(LayoutKind.Sequential)]
    public struct SM64Vector2f(float x, float y)
    {
        public float x = x; 
        public float y = y;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct SM64Vector3f(float x, float y, float z)
    {
        public float x = x;
        public float y = y;
        public float z = z;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct SM64Surface
    {
        public short type;
        public short force;
        public ushort terrain;
        public int v0x, v0y, v0z;
        public int v1x, v1y, v1z;
        public int v2x, v2y, v2z;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct SM64MarioInputs
    {
        public float camLookX, camLookZ;
        public float stickX, stickY;
        public byte buttonA, buttonB, buttonZ;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SM64MarioState
    {
        public SM64Vector3f position;
        public SM64Vector3f velocity;
        public float faceAngle;
        public short health;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SM64MarioGeometryBuffers
    {
        public IntPtr position;
        public IntPtr normal;
        public IntPtr color;
        public IntPtr uv;
        public ushort numTrianglesUsed;
    }
    
    #endregion

    #region Functions

    private const string SM64_LIB = "sm64";
    
    [DllImport(SM64_LIB)]
    public static extern unsafe void sm64_global_init(byte* rom, byte* outTexture);
    [DllImport(SM64_LIB)]
    public static extern void sm64_global_terminate();
    
    [DllImport(SM64_LIB)]
    public static extern unsafe void sm64_audio_init(byte* rom);
    [DllImport(SM64_LIB)]
    public static extern unsafe uint sm64_audio_tick(uint numQueuedSamples, uint numDesiredSamples, short* audioBuffer);
    
    [DllImport(SM64_LIB)]
    public static extern void sm64_static_surfaces_load(SM64Surface[] surfaceArray, uint numSurfaces);
    
    [DllImport(SM64_LIB)]
    public static extern int sm64_mario_create(float x, float y, float z);
    [DllImport(SM64_LIB)]
    public static extern void sm64_mario_tick(int marioId, ref SM64MarioInputs inputs, ref SM64MarioState outState, ref SM64MarioGeometryBuffers outBuffers);
    [DllImport(SM64_LIB)]
    public static extern void sm64_mario_delete(int marioId);

    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_action(int marioId, uint action);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_action_arg(int marioId, uint action, uint actionArg);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_animation(int marioId, int animID);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_anim_frame(int marioId, short animFrame);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_state(int marioId, uint flags);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_position(int marioId, float x, float y, float z);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_angle(int marioId, float x, float y, float z);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_faceangle(int marioId, float y);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_velocity(int marioId, float x, float y, float z);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_forward_velocity(int marioId, float vel);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_invincibility(int marioId, short timer);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_water_level(int marioId, int level);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_gas_level(int marioId, int level);
    [DllImport(SM64_LIB)]
    public static extern void sm64_set_mario_health(int marioId, ushort health);
    [DllImport(SM64_LIB)]
    public static extern void sm64_mario_take_damage(int marioId, uint damage, uint subtype, float x, float y, float z);
    [DllImport(SM64_LIB)]
    public static extern void sm64_mario_heal(int marioId, byte healCounter);
    [DllImport(SM64_LIB)]
    public static extern void sm64_mario_kill(int marioId);
    [DllImport(SM64_LIB)]
    public static extern void sm64_mario_interact_cap(int marioId, uint capFlag, ushort capTime, byte playMusic);
    [DllImport(SM64_LIB)]
    public static extern void sm64_mario_extend_cap(int marioId, ushort capTime);
    [DllImport(SM64_LIB)]
    public static extern bool sm64_mario_attack(int marioId, float x, float y, float z, float hitboxHeight);
    
    [DllImport(SM64_LIB)]
    public static extern unsafe void sm64_play_sound(int soundBits, SM64Vector3f* pos);
    [DllImport(SM64_LIB)]
    public static extern void sm64_play_sound_global(int soundBits);
    
    #endregion
    
    #region Enums
    
    public enum SM64SurfaceType : short
    {
        DEFAULT = 0x0000, // Environment default

        BURNING = 0x0001, // Lava / Frostbite (in SL), but is used mostly for Lava
        HANGABLE = 0x0005, // Ceiling that Mario can climb on
        SLOW = 0x0009, // Slow down Mario, unused
        DEATH_PLANE = 0x000A, // Death floor
        CLOSE_CAMERA = 0x000B, // Close camera

        WATER = 0x000D, // Water, has no action, used on some waterboxes below
        FLOWING_WATER = 0x000E, // Water (flowing), has parameters

        INTANGIBLE = 0x0012, // Intangible (Separates BBH mansion from merry-go-round, for room usage)
        VERY_SLIPPERY = 0x0013, // Very slippery, mostly used for slides
        SLIPPERY = 0x0014, // Slippery
        NOT_SLIPPERY = 0x0015, // Non-slippery, climbable
        TTM_VINES = 0x0016, // TTM vines, has no action defined

        MGR_MUSIC = 0x001A, // Plays the Merry go round music, see handle_merry_go_round_music in bbh_merry_go_round.inc.c for more details

        INSTANT_WARP_1B = 0x001B, // Instant warp to another area, used to warp between areas in WDW and the endless stairs to warp back
        INSTANT_WARP_1C = 0x001C, // Instant warp to another area, used to warp between areas in WDW
        INSTANT_WARP_1D = 0x001D, // Instant warp to another area, used to warp between areas in DDD, SSL and TTM
        INSTANT_WARP_1E = 0x001E, // Instant warp to another area, used to warp between areas in DDD, SSL and TTM
        
        SHALLOW_QUICKSAND = 0x0021, // Shallow Quicksand (depth of 10 units)
        DEEP_QUICKSAND = 0x0022, // Quicksand (lethal, slow, depth of 160 units)
        INSTANT_QUICKSAND = 0x0023, // Quicksand (lethal, instant)
        DEEP_MOVING_QUICKSAND = 0x0024, // Moving quicksand (flowing, depth of 160 units)
        SHALLOW_MOVING_QUICKSAND = 0x0025, // Moving quicksand (flowing, depth of 25 units)
        QUICKSAND = 0x0026, // Moving quicksand (60 units)
        MOVING_QUICKSAND = 0x0027, // Moving quicksand (flowing, depth of 60 units)

        WALL_MISC = 0x0028, // Used for some walls, Cannon to adjust the camera, and some objects like Warp Pipe
        NOISE_DEFAULT = 0x0029, // Default floor with noise
        NOISE_SLIPPERY = 0x002A, // Slippery floor with noise
        HORIZONTAL_WIND = 0x002C, // Horizontal wind, has parameters
        INSTANT_MOVING_QUICKSAND = 0x002D, // Quicksand (lethal, flowing)
        ICE = 0x002E, // Slippery Ice, in snow levels and THI's water floor
        LOOK_UP_WARP = 0x002F, // Look up and warp (Wing cap entrance)
        HARD = 0x0030, // Hard floor (Always has fall damage)
        WARP = 0x0032, // Surface warp
        TIMER_START = 0x0033, // Timer start (Peach's secret slide)
        TIMER_END = 0x0034, // Timer stop (Peach's secret slide)

        HARD_SLIPPERY = 0x0035, // Hard and slippery (Always has fall damage)
        HARD_VERY_SLIPPERY = 0x0036, // Hard and very slippery (Always has fall damage)
        HARD_NOT_SLIPPERY = 0x0037, // Hard and Non-slippery (Always has fall damage)

        VERTICAL_WIND = 0x0038, // Death at bottom with vertical wind
        BOSS_FIGHT_CAMERA = 0x0065, // Wide camera for BOB and WF bosses
        CAMERA_FREE_ROAM = 0x0066, // Free roam camera for THI and TTC

        THI3_WALLKICK = 0x0068, // Surface where there's a wall kick section in THI 3rd area, has no action defined

        CAMERA_8_DIR = 0x0069, // Surface that enables far camera for platforms, used in THI
        CAMERA_MIDDLE = 0x006E, // Surface camera that returns to the middle, used on the 4 pillars of SSL
        CAMERA_ROTATE_RIGHT = 0x006F, // Surface camera that rotates to the right (Bowser 1 & THI)
        CAMERA_ROTATE_LEFT = 0x0070, // Surface camera that rotates to the left (BOB & TTM)
        CAMERA_BOUNDARY = 0x0072, // Intangible Area, only used to restrict camera movement

        NOISE_VERY_SLIPPERY_73 = 0x0073, // Very slippery floor with noise, unused
        NOISE_VERY_SLIPPERY_74 = 0x0074, // Very slippery floor with noise, unused
        NOISE_VERY_SLIPPERY = 0x0075, // Very slippery floor with noise, used in CCM
        
        NO_CAM_COLLISION = 0x0076, // Surface with no cam collision flag
        NO_CAM_COLLISION_77 = 0x0077, // Surface with no cam collision flag, unused
        NO_CAM_COL_VERY_SLIPPERY = 0x0078, // Surface with no cam collision flag, very slippery with noise (THI)
        NO_CAM_COL_SLIPPERY = 0x0079, // Surface with no cam collision flag, slippery with noise (CCM, PSS and TTM slides)

        SWITCH = 0x007A, // Surface with no cam collision flag, non-slippery with noise, used by switches and Dorrie

        VANISH_CAP_WALLS = 0x007B, // Vanish cap walls, pass through them with Vanish Cap

        PAINTING_WOBBLE_A6 = 0x00A6, // Painting wobble (BOB Left)
        PAINTING_WOBBLE_A7 = 0x00A7, // Painting wobble (BOB Middle)
        PAINTING_WOBBLE_A8 = 0x00A8, // Painting wobble (BOB Right)
        PAINTING_WOBBLE_A9 = 0x00A9, // Painting wobble (CCM Left)
        PAINTING_WOBBLE_AA = 0x00AA, // Painting wobble (CCM Middle)
        PAINTING_WOBBLE_AB = 0x00AB, // Painting wobble (CCM Right)
        PAINTING_WOBBLE_AC = 0x00AC, // Painting wobble (WF Left)
        PAINTING_WOBBLE_AD = 0x00AD, // Painting wobble (WF Middle)
        PAINTING_WOBBLE_AE = 0x00AE, // Painting wobble (WF Right)
        PAINTING_WOBBLE_AF = 0x00AF, // Painting wobble (JRB Left)
        PAINTING_WOBBLE_B0 = 0x00B0, // Painting wobble (JRB Middle)
        PAINTING_WOBBLE_B1 = 0x00B1, // Painting wobble (JRB Right)
        PAINTING_WOBBLE_B2 = 0x00B2, // Painting wobble (LLL Left)
        PAINTING_WOBBLE_B3 = 0x00B3, // Painting wobble (LLL Middle)
        PAINTING_WOBBLE_B4 = 0x00B4, // Painting wobble (LLL Right)
        PAINTING_WOBBLE_B5 = 0x00B5, // Painting wobble (SSL Left)
        PAINTING_WOBBLE_B6 = 0x00B6, // Painting wobble (SSL Middle)
        PAINTING_WOBBLE_B7 = 0x00B7, // Painting wobble (SSL Right)
        PAINTING_WOBBLE_B8 = 0x00B8, // Painting wobble (Unused - Left)
        PAINTING_WOBBLE_B9 = 0x00B9, // Painting wobble (Unused - Middle)
        PAINTING_WOBBLE_BA = 0x00BA, // Painting wobble (Unused - Right)
        PAINTING_WOBBLE_BB = 0x00BB, // Painting wobble (DDD - Left), makes the painting wobble if touched
        PAINTING_WOBBLE_BC = 0x00BC, // Painting wobble (Unused, DDD - Middle)
        PAINTING_WOBBLE_BD = 0x00BD, // Painting wobble (Unused, DDD - Right)
        PAINTING_WOBBLE_BE = 0x00BE, // Painting wobble (WDW Left)
        PAINTING_WOBBLE_BF = 0x00BF, // Painting wobble (WDW Middle)
        PAINTING_WOBBLE_C0 = 0x00C0, // Painting wobble (WDW Right)
        PAINTING_WOBBLE_C1 = 0x00C1, // Painting wobble (THI Tiny - Left)
        PAINTING_WOBBLE_C2 = 0x00C2, // Painting wobble (THI Tiny - Middle)
        PAINTING_WOBBLE_C3 = 0x00C3, // Painting wobble (THI Tiny - Right)
        PAINTING_WOBBLE_C4 = 0x00C4, // Painting wobble (TTM Left)
        PAINTING_WOBBLE_C5 = 0x00C5, // Painting wobble (TTM Middle)
        PAINTING_WOBBLE_C6 = 0x00C6, // Painting wobble (TTM Right)
        PAINTING_WOBBLE_C7 = 0x00C7, // Painting wobble (Unused, TTC - Left)
        PAINTING_WOBBLE_C8 = 0x00C8, // Painting wobble (Unused, TTC - Middle)
        PAINTING_WOBBLE_C9 = 0x00C9, // Painting wobble (Unused, TTC - Right)
        PAINTING_WOBBLE_CA = 0x00CA, // Painting wobble (Unused, SL - Left)
        PAINTING_WOBBLE_CB = 0x00CB, // Painting wobble (Unused, SL - Middle)
        PAINTING_WOBBLE_CC = 0x00CC, // Painting wobble (Unused, SL - Right)
        PAINTING_WOBBLE_CD = 0x00CD, // Painting wobble (THI Huge - Left)
        PAINTING_WOBBLE_CE = 0x00CE, // Painting wobble (THI Huge - Middle)
        PAINTING_WOBBLE_CF = 0x00CF, // Painting wobble (THI Huge - Right)
        PAINTING_WOBBLE_D0 = 0x00D0, // Painting wobble (HMC & COTMC - Left), makes the painting wobble if touched
        PAINTING_WOBBLE_D1 = 0x00D1, // Painting wobble (Unused, HMC & COTMC - Middle)
        PAINTING_WOBBLE_D2 = 0x00D2, // Painting wobble (Unused, HMC & COTMC - Right)

        PAINTING_WARP_D3 = 0x00D3, // Painting warp (BOB Left)
        PAINTING_WARP_D4 = 0x00D4, // Painting warp (BOB Middle)
        PAINTING_WARP_D5 = 0x00D5, // Painting warp (BOB Right)
        PAINTING_WARP_D6 = 0x00D6, // Painting warp (CCM Left)
        PAINTING_WARP_D7 = 0x00D7, // Painting warp (CCM Middle)
        PAINTING_WARP_D8 = 0x00D8, // Painting warp (CCM Right)
        PAINTING_WARP_D9 = 0x00D9, // Painting warp (WF Left)
        PAINTING_WARP_DA = 0x00DA, // Painting warp (WF Middle)
        PAINTING_WARP_DB = 0x00DB, // Painting warp (WF Right)
        PAINTING_WARP_DC = 0x00DC, // Painting warp (JRB Left)
        PAINTING_WARP_DD = 0x00DD, // Painting warp (JRB Middle)
        PAINTING_WARP_DE = 0x00DE, // Painting warp (JRB Right)
        PAINTING_WARP_DF = 0x00DF, // Painting warp (LLL Left)
        PAINTING_WARP_E0 = 0x00E0, // Painting warp (LLL Middle)
        PAINTING_WARP_E1 = 0x00E1, // Painting warp (LLL Right)
        PAINTING_WARP_E2 = 0x00E2, // Painting warp (SSL Left)
        PAINTING_WARP_E3 = 0x00E3, // Painting warp (SSL Medium)
        PAINTING_WARP_E4 = 0x00E4, // Painting warp (SSL Right)
        PAINTING_WARP_E5 = 0x00E5, // Painting warp (Unused - Left)
        PAINTING_WARP_E6 = 0x00E6, // Painting warp (Unused - Medium)
        PAINTING_WARP_E7 = 0x00E7, // Painting warp (Unused - Right)
        PAINTING_WARP_E8 = 0x00E8, // Painting warp (DDD - Left)
        PAINTING_WARP_E9 = 0x00E9, // Painting warp (DDD - Middle)
        PAINTING_WARP_EA = 0x00EA, // Painting warp (DDD - Right)
        PAINTING_WARP_EB = 0x00EB, // Painting warp (WDW Left)
        PAINTING_WARP_EC = 0x00EC, // Painting warp (WDW Middle)
        PAINTING_WARP_ED = 0x00ED, // Painting warp (WDW Right)
        PAINTING_WARP_EE = 0x00EE, // Painting warp (THI Tiny - Left)
        PAINTING_WARP_EF = 0x00EF, // Painting warp (THI Tiny - Middle)
        PAINTING_WARP_F0 = 0x00F0, // Painting warp (THI Tiny - Right)
        PAINTING_WARP_F1 = 0x00F1, // Painting warp (TTM Left)
        PAINTING_WARP_F2 = 0x00F2, // Painting warp (TTM Middle)
        PAINTING_WARP_F3 = 0x00F3, // Painting warp (TTM Right)

        TTC_PAINTING_1 = 0x00F4, // Painting warp (TTC Left)
        TTC_PAINTING_2 = 0x00F5, // Painting warp (TTC Medium)
        TTC_PAINTING_3 = 0x00F6, // Painting warp (TTC Right)
        
        PAINTING_WARP_F7 = 0x00F7, // Painting warp (SL Left)
        PAINTING_WARP_F8 = 0x00F8, // Painting warp (SL Middle)
        PAINTING_WARP_F9 = 0x00F9, // Painting warp (SL Right)
        PAINTING_WARP_FA = 0x00FA, // Painting warp (THI Tiny - Left)
        PAINTING_WARP_FB = 0x00FB, // Painting warp (THI Tiny - Middle)
        PAINTING_WARP_FC = 0x00FC, // Painting warp (THI Tiny - Right)

        WOBBLING_WARP = 0x00FD, // Pool warp (HMC & DDD)

        TRAPDOOR = 0x00FF, // Bowser Left trapdoor, has no action defined
    }
    
    public enum SM64TerrainType : ushort
    {
        GRASS = 0x0000,
        STONE = 0x0001,
        SNOW = 0x0002,
        SAND = 0x0003,
        SPOOKY = 0x0004,
        WATER = 0x0005,
        SLIDE = 0x0006,
        MASK = 0x0007,
    }
    
    public enum SM64Action : uint
    {
        GROUP_STATIONARY = 0u << 6,
        GROUP_MOVING     = 1u << 6,
        GROUP_AIRBORNE   = 2u << 6,
        GROUP_SUBMERGED  = 3u << 6,
        GROUP_CUTSCENE   = 4u << 6,
        GROUP_AUTOMATIC  = 5u << 6,
        GROUP_OBJECT     = 6u << 6,

        FLAG_STATIONARY                  = 1u << 9,
        FLAG_MOVING                      = 1u << 10,
        FLAG_AIR                         = 1u << 11,
        FLAG_INTANGIBLE                  = 1u << 12,
        FLAG_SWIMMING                    = 1u << 13,
        FLAG_METAL_WATER                 = 1u << 14,
        FLAG_SHORT_HITBOX                = 1u << 15,
        FLAG_RIDING_SHELL                = 1u << 16,
        FLAG_INVULNERABLE                = 1u << 17,
        FLAG_BUTT_OR_STOMACH_SLIDE       = 1u << 18,
        FLAG_DIVING                      = 1u << 19,
        FLAG_ON_POLE                     = 1u << 20,
        FLAG_HANGING                     = 1u << 21,
        FLAG_IDLE                        = 1u << 22,
        FLAG_ATTACKING                   = 1u << 23,
        FLAG_ALLOW_VERTICAL_WIND_ACTION  = 1u << 24,
        FLAG_CONTROL_JUMP_HEIGHT         = 1u << 25,
        FLAG_ALLOW_FIRST_PERSON          = 1u << 26,
        FLAG_PAUSE_EXIT                  = 1u << 27,
        FLAG_SWIMMING_OR_FLYING          = 1u << 28,
        FLAG_WATER_OR_TEXT               = 1u << 29,
        FLAG_THROWING                    = 1u << 31,

        UNINITIALIZED              = 0x00000000,

// group 0x000: stationary actions
        IDLE                       = 0x001 | FLAG_STATIONARY | FLAG_IDLE | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        START_SLEEPING             = 0x002 | FLAG_STATIONARY | FLAG_IDLE | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        SLEEPING                   = 0x003 | FLAG_STATIONARY | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        WAKING_UP                  = 0x004 | FLAG_STATIONARY | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        PANTING                    = 0x005 | FLAG_STATIONARY | FLAG_IDLE | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        HOLD_PANTING_UNUSED        = 0x006 | FLAG_STATIONARY | FLAG_PAUSE_EXIT,
        HOLD_IDLE                  = 0x007 | FLAG_STATIONARY | FLAG_PAUSE_EXIT,
        HOLD_HEAVY_IDLE            = 0x008 | FLAG_STATIONARY | FLAG_PAUSE_EXIT,
        STANDING_AGAINST_WALL      = 0x009 | FLAG_STATIONARY | FLAG_IDLE | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        COUGHING                   = 0x00A | FLAG_STATIONARY | FLAG_IDLE | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        SHIVERING                  = 0x00B | FLAG_STATIONARY | FLAG_IDLE | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        IN_QUICKSAND               = 0x00D | FLAG_STATIONARY | FLAG_INVULNERABLE,
        UNKNOWN_0002020E           = 0x00E | FLAG_STATIONARY | FLAG_INVULNERABLE,
        CROUCHING                  = 0x020 | FLAG_STATIONARY | FLAG_SHORT_HITBOX | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        START_CROUCHING            = 0x021 | FLAG_STATIONARY | FLAG_SHORT_HITBOX | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        STOP_CROUCHING             = 0x022 | FLAG_STATIONARY | FLAG_SHORT_HITBOX | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        START_CRAWLING             = 0x023 | FLAG_STATIONARY | FLAG_SHORT_HITBOX | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        STOP_CRAWLING              = 0x024 | FLAG_STATIONARY | FLAG_SHORT_HITBOX | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        SLIDE_KICK_SLIDE_STOP      = 0x025 | FLAG_STATIONARY | FLAG_PAUSE_EXIT,
        SHOCKWAVE_BOUNCE           = 0x026 | FLAG_STATIONARY | FLAG_INVULNERABLE,
        FIRST_PERSON               = 0x027 | FLAG_STATIONARY | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        BACKFLIP_LAND_STOP         = 0x02F | FLAG_STATIONARY | FLAG_PAUSE_EXIT,
        JUMP_LAND_STOP             = 0x030 | FLAG_STATIONARY | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        DOUBLE_JUMP_LAND_STOP      = 0x031 | FLAG_STATIONARY | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        FREEFALL_LAND_STOP         = 0x032 | FLAG_STATIONARY | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        SIDE_FLIP_LAND_STOP        = 0x033 | FLAG_STATIONARY | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        HOLD_JUMP_LAND_STOP        = 0x034 | FLAG_STATIONARY | FLAG_PAUSE_EXIT,
        HOLD_FREEFALL_LAND_STOP    = 0x035 | FLAG_STATIONARY | FLAG_PAUSE_EXIT,
        AIR_THROW_LAND             = 0x036 | FLAG_STATIONARY | FLAG_AIR | FLAG_THROWING,
        TWIRL_LAND                 = 0x038 | FLAG_STATIONARY | FLAG_ATTACKING | FLAG_PAUSE_EXIT | FLAG_SWIMMING_OR_FLYING,
        LAVA_BOOST_LAND            = 0x039 | FLAG_STATIONARY | FLAG_PAUSE_EXIT,
        TRIPLE_JUMP_LAND_STOP      = 0x03A | FLAG_STATIONARY | FLAG_PAUSE_EXIT,
        LONG_JUMP_LAND_STOP        = 0x03B | FLAG_STATIONARY | FLAG_PAUSE_EXIT,
        GROUND_POUND_LAND          = 0x03C | FLAG_STATIONARY | FLAG_ATTACKING,
        BRAKING_STOP               = 0x03D | FLAG_STATIONARY | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        BUTT_SLIDE_STOP            = 0x03E | FLAG_STATIONARY | FLAG_ALLOW_FIRST_PERSON | FLAG_PAUSE_EXIT,
        HOLD_BUTT_SLIDE_STOP       = 0x03F | FLAG_MOVING | FLAG_PAUSE_EXIT,

// group 0x040: moving (ground) actions
        WALKING                    = 0x040 | FLAG_MOVING | FLAG_ALLOW_FIRST_PERSON,
        HOLD_WALKING               = 0x042 | FLAG_MOVING,
        TURNING_AROUND             = 0x043 | FLAG_MOVING,
        FINISH_TURNING_AROUND      = 0x044 | FLAG_MOVING,
        BRAKING                    = 0x045 | FLAG_MOVING | FLAG_ALLOW_FIRST_PERSON,
        RIDING_SHELL_GROUND        = 0x046 | FLAG_MOVING | FLAG_RIDING_SHELL | FLAG_ATTACKING | FLAG_WATER_OR_TEXT,
        HOLD_HEAVY_WALKING         = 0x047 | FLAG_MOVING,
        CRAWLING                   = 0x048 | FLAG_MOVING | FLAG_SHORT_HITBOX | FLAG_ALLOW_FIRST_PERSON,
        BURNING_GROUND             = 0x049 | FLAG_MOVING | FLAG_INVULNERABLE,
        DECELERATING               = 0x04A | FLAG_MOVING | FLAG_ALLOW_FIRST_PERSON,
        HOLD_DECELERATING          = 0x04B | FLAG_MOVING,
        BEGIN_SLIDING              = 0x050,
        HOLD_BEGIN_SLIDING         = 0x051,
        BUTT_SLIDE                 = 0x052 | FLAG_MOVING | FLAG_BUTT_OR_STOMACH_SLIDE | FLAG_ATTACKING,
        STOMACH_SLIDE              = 0x053 | FLAG_MOVING | FLAG_BUTT_OR_STOMACH_SLIDE | FLAG_DIVING | FLAG_ATTACKING,
        HOLD_BUTT_SLIDE            = 0x054 | FLAG_MOVING | FLAG_BUTT_OR_STOMACH_SLIDE | FLAG_ATTACKING,
        HOLD_STOMACH_SLIDE         = 0x055 | FLAG_MOVING | FLAG_BUTT_OR_STOMACH_SLIDE | FLAG_DIVING | FLAG_ATTACKING,
        DIVE_SLIDE                 = 0x056 | FLAG_MOVING | FLAG_DIVING | FLAG_ATTACKING,
        MOVE_PUNCHING              = 0x057 | FLAG_MOVING | FLAG_ATTACKING,
        CROUCH_SLIDE               = 0x059 | FLAG_MOVING | FLAG_SHORT_HITBOX | FLAG_ATTACKING | FLAG_ALLOW_FIRST_PERSON,
        SLIDE_KICK_SLIDE           = 0x05A | FLAG_MOVING | FLAG_ATTACKING,
        HARD_BACKWARD_GROUND_KB    = 0x060 | FLAG_MOVING | FLAG_INVULNERABLE,
        HARD_FORWARD_GROUND_KB     = 0x061 | FLAG_MOVING | FLAG_INVULNERABLE,
        BACKWARD_GROUND_KB         = 0x062 | FLAG_MOVING | FLAG_INVULNERABLE,
        FORWARD_GROUND_KB          = 0x063 | FLAG_MOVING | FLAG_INVULNERABLE,
        SOFT_BACKWARD_GROUND_KB    = 0x064 | FLAG_MOVING | FLAG_INVULNERABLE,
        SOFT_FORWARD_GROUND_KB     = 0x065 | FLAG_MOVING | FLAG_INVULNERABLE,
        GROUND_BONK                = 0x066 | FLAG_MOVING | FLAG_INVULNERABLE,
        DEATH_EXIT_LAND            = 0x067 | FLAG_MOVING | FLAG_INVULNERABLE,
        JUMP_LAND                  = 0x070 | FLAG_MOVING | FLAG_ALLOW_FIRST_PERSON,
        FREEFALL_LAND              = 0x071 | FLAG_MOVING | FLAG_ALLOW_FIRST_PERSON,
        DOUBLE_JUMP_LAND           = 0x072 | FLAG_MOVING | FLAG_ALLOW_FIRST_PERSON,
        SIDE_FLIP_LAND             = 0x073 | FLAG_MOVING | FLAG_ALLOW_FIRST_PERSON,
        HOLD_JUMP_LAND             = 0x074 | FLAG_MOVING,
        HOLD_FREEFALL_LAND         = 0x075 | FLAG_MOVING,
        QUICKSAND_JUMP_LAND        = 0x076 | FLAG_MOVING,
        HOLD_QUICKSAND_JUMP_LAND   = 0x077 | FLAG_MOVING,
        TRIPLE_JUMP_LAND           = 0x078 | FLAG_MOVING | FLAG_ALLOW_FIRST_PERSON,
        LONG_JUMP_LAND             = 0x079 | FLAG_MOVING,
        BACKFLIP_LAND              = 0x07A | FLAG_MOVING | FLAG_ALLOW_FIRST_PERSON,

// group 0x080: airborne actions
        JUMP                       = 0x080 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION | FLAG_CONTROL_JUMP_HEIGHT,
        DOUBLE_JUMP                = 0x081 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION | FLAG_CONTROL_JUMP_HEIGHT,
        TRIPLE_JUMP                = 0x082 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        BACKFLIP                   = 0x083 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        STEEP_JUMP                 = 0x085 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION | FLAG_CONTROL_JUMP_HEIGHT,
        WALL_KICK_AIR              = 0x086 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION | FLAG_CONTROL_JUMP_HEIGHT,
        SIDE_FLIP                  = 0x087 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        LONG_JUMP                  = 0x088 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION | FLAG_CONTROL_JUMP_HEIGHT,
        WATER_JUMP                 = 0x089 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        DIVE                       = 0x08A | FLAG_AIR | FLAG_DIVING | FLAG_ATTACKING | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        FREEFALL                   = 0x08C | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        TOP_OF_POLE_JUMP           = 0x08D | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION | FLAG_CONTROL_JUMP_HEIGHT,
        BUTT_SLIDE_AIR             = 0x08E | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION | FLAG_CONTROL_JUMP_HEIGHT,
        FLYING_TRIPLE_JUMP         = 0x094 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION | FLAG_CONTROL_JUMP_HEIGHT,
        SHOT_FROM_CANNON           = 0x098 | FLAG_AIR | FLAG_DIVING | FLAG_ATTACKING,
        FLYING                     = 0x099 | FLAG_AIR | FLAG_DIVING | FLAG_ATTACKING | FLAG_SWIMMING_OR_FLYING,
        RIDING_SHELL_JUMP          = 0x09A | FLAG_AIR | FLAG_RIDING_SHELL | FLAG_ATTACKING | FLAG_CONTROL_JUMP_HEIGHT,
        RIDING_SHELL_FALL          = 0x09B | FLAG_AIR | FLAG_RIDING_SHELL | FLAG_ATTACKING,
        VERTICAL_WIND              = 0x09C | FLAG_AIR | FLAG_DIVING | FLAG_SWIMMING_OR_FLYING,
        HOLD_JUMP                  = 0x0A0 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION | FLAG_CONTROL_JUMP_HEIGHT,
        HOLD_FREEFALL              = 0x0A1 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        HOLD_BUTT_SLIDE_AIR        = 0x0A2 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        HOLD_WATER_JUMP            = 0x0A3 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        TWIRLING                   = 0x0A4 | FLAG_AIR | FLAG_ATTACKING | FLAG_SWIMMING_OR_FLYING,
        FORWARD_ROLLOUT            = 0x0A6 | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        AIR_HIT_WALL               = 0x0A7 | FLAG_AIR,
        RIDING_HOOT                = 0x0A8 | FLAG_MOVING,
        GROUND_POUND               = 0x0A9 | FLAG_AIR | FLAG_ATTACKING,
        SLIDE_KICK                 = 0x0AA | FLAG_AIR | FLAG_ATTACKING | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        AIR_THROW                  = 0x0AB | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION | FLAG_CONTROL_JUMP_HEIGHT | FLAG_THROWING,
        JUMP_KICK                  = 0x0AC | FLAG_AIR | FLAG_ATTACKING | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        BACKWARD_ROLLOUT           = 0x0AD | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        CRAZY_BOX_BOUNCE           = 0x0AE | FLAG_AIR,
        SPECIAL_TRIPLE_JUMP        = 0x0AF | FLAG_AIR | FLAG_ALLOW_VERTICAL_WIND_ACTION | FLAG_CONTROL_JUMP_HEIGHT,
        BACKWARD_AIR_KB            = 0x0B0 | FLAG_AIR | FLAG_INVULNERABLE | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        FORWARD_AIR_KB             = 0x0B1 | FLAG_AIR | FLAG_INVULNERABLE | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        HARD_FORWARD_AIR_KB        = 0x0B2 | FLAG_AIR | FLAG_INVULNERABLE | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        HARD_BACKWARD_AIR_KB       = 0x0B3 | FLAG_AIR | FLAG_INVULNERABLE | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        BURNING_JUMP               = 0x0B4 | FLAG_AIR | FLAG_INVULNERABLE | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        BURNING_FALL               = 0x0B5 | FLAG_AIR | FLAG_INVULNERABLE | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        SOFT_BONK                  = 0x0B6 | FLAG_AIR | FLAG_INVULNERABLE | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        LAVA_BOOST                 = 0x0B7 | FLAG_AIR | FLAG_INVULNERABLE | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        GETTING_BLOWN              = 0x0B8 | FLAG_AIR | FLAG_INVULNERABLE | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        THROWN_FORWARD             = 0x0BD | FLAG_AIR | FLAG_INVULNERABLE | FLAG_ALLOW_VERTICAL_WIND_ACTION,
        THROWN_BACKWARD            = 0x0BE | FLAG_AIR | FLAG_INVULNERABLE | FLAG_ALLOW_VERTICAL_WIND_ACTION,

// group 0x0C0: submerged actions
        WATER_IDLE                 = 0x0C0 | FLAG_STATIONARY | FLAG_SWIMMING | FLAG_PAUSE_EXIT | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        HOLD_WATER_IDLE            = 0x0C1 | FLAG_STATIONARY | FLAG_SWIMMING | FLAG_PAUSE_EXIT | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        WATER_ACTION_END           = 0x0C2 | FLAG_STATIONARY | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        HOLD_WATER_ACTION_END      = 0x0C3 | FLAG_STATIONARY | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        DROWNING                   = 0x0C4 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        BACKWARD_WATER_KB          = 0x0C5 | FLAG_STATIONARY | FLAG_SWIMMING | FLAG_INVULNERABLE | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        FORWARD_WATER_KB           = 0x0C6 | FLAG_STATIONARY | FLAG_SWIMMING | FLAG_INVULNERABLE | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        WATER_DEATH                = 0x0C7 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        WATER_SHOCKED              = 0x0C8 | FLAG_STATIONARY | FLAG_SWIMMING | FLAG_INVULNERABLE | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        BREASTSTROKE               = 0x0D0 | FLAG_MOVING | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        SWIMMING_END               = 0x0D1 | FLAG_MOVING | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        FLUTTER_KICK               = 0x0D2 | FLAG_MOVING | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        HOLD_BREASTSTROKE          = 0x0D3 | FLAG_MOVING | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        HOLD_SWIMMING_END          = 0x0D4 | FLAG_MOVING | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        HOLD_FLUTTER_KICK          = 0x0D5 | FLAG_MOVING | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        WATER_SHELL_SWIMMING       = 0x0D6 | FLAG_MOVING | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        WATER_THROW                = 0x0E0 | FLAG_MOVING | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        WATER_PUNCH                = 0x0E1 | FLAG_MOVING | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        WATER_PLUNGE               = 0x0E2 | FLAG_STATIONARY | FLAG_SWIMMING | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        CAUGHT_IN_WHIRLPOOL        = 0x0E3 | FLAG_STATIONARY | FLAG_SWIMMING | FLAG_INVULNERABLE | FLAG_SWIMMING_OR_FLYING | FLAG_WATER_OR_TEXT,
        METAL_WATER_STANDING       = 0x0F0 | FLAG_STATIONARY | FLAG_METAL_WATER | FLAG_PAUSE_EXIT,
        HOLD_METAL_WATER_STANDING  = 0x0F1 | FLAG_STATIONARY | FLAG_METAL_WATER | FLAG_PAUSE_EXIT,
        METAL_WATER_WALKING        = 0x0F2 | FLAG_MOVING | FLAG_METAL_WATER,
        HOLD_METAL_WATER_WALKING   = 0x0F3 | FLAG_MOVING | FLAG_METAL_WATER,
        METAL_WATER_FALLING        = 0x0F4 | FLAG_STATIONARY | FLAG_METAL_WATER,
        HOLD_METAL_WATER_FALLING   = 0x0F5 | FLAG_STATIONARY | FLAG_METAL_WATER,
        METAL_WATER_FALL_LAND      = 0x0F6 | FLAG_STATIONARY | FLAG_METAL_WATER,
        HOLD_METAL_WATER_FALL_LAND = 0x0F7 | FLAG_STATIONARY | FLAG_METAL_WATER,
        METAL_WATER_JUMP           = 0x0F8 | FLAG_MOVING | FLAG_METAL_WATER,
        HOLD_METAL_WATER_JUMP      = 0x0F9 | FLAG_MOVING | FLAG_METAL_WATER,
        METAL_WATER_JUMP_LAND      = 0x0FA | FLAG_MOVING | FLAG_METAL_WATER,
        HOLD_METAL_WATER_JUMP_LAND = 0x0FB | FLAG_MOVING | FLAG_METAL_WATER,

// group 0x100: cutscene actions
        DISAPPEARED                = 0x100 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        INTRO_CUTSCENE             = 0x101 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_ALLOW_FIRST_PERSON,
        STAR_DANCE_EXIT            = 0x102 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        STAR_DANCE_WATER           = 0x103 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        FALL_AFTER_STAR_GRAB       = 0x104 | FLAG_AIR | FLAG_INTANGIBLE,
        READING_AUTOMATIC_DIALOG   = 0x105 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_WATER_OR_TEXT,
        READING_NPC_DIALOG         = 0x106 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_WATER_OR_TEXT,
        STAR_DANCE_NO_EXIT         = 0x107 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        READING_SIGN               = 0x108 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        JUMBO_STAR_CUTSCENE        = 0x109 | FLAG_AIR | FLAG_INTANGIBLE,
        WAITING_FOR_DIALOG         = 0x10A | FLAG_STATIONARY | FLAG_INTANGIBLE,
        DEBUG_FREE_MOVE            = 0x10F | FLAG_STATIONARY | FLAG_INTANGIBLE,
        STANDING_DEATH             = 0x111 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_INVULNERABLE,
        QUICKSAND_DEATH            = 0x112 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_INVULNERABLE,
        ELECTROCUTION              = 0x113 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_INVULNERABLE,
        SUFFOCATION                = 0x114 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_INVULNERABLE,
        DEATH_ON_STOMACH           = 0x115 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_INVULNERABLE,
        DEATH_ON_BACK              = 0x116 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_INVULNERABLE,
        EATEN_BY_BUBBA             = 0x117 | FLAG_STATIONARY | FLAG_INTANGIBLE | FLAG_INVULNERABLE,
        END_PEACH_CUTSCENE         = 0x118 | FLAG_AIR | FLAG_INTANGIBLE,
        CREDITS_CUTSCENE           = 0x119 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        END_WAVING_CUTSCENE        = 0x11A | FLAG_STATIONARY | FLAG_INTANGIBLE,
        PULLING_DOOR               = 0x120 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        PUSHING_DOOR               = 0x121 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        WARP_DOOR_SPAWN            = 0x122 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        EMERGE_FROM_PIPE           = 0x123 | FLAG_AIR | FLAG_INTANGIBLE,
        SPAWN_SPIN_AIRBORNE        = 0x124 | FLAG_AIR | FLAG_INTANGIBLE,
        SPAWN_SPIN_LANDING         = 0x125 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        EXIT_AIRBORNE              = 0x126 | FLAG_AIR | FLAG_INTANGIBLE,
        EXIT_LAND_SAVE_DIALOG      = 0x127 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        DEATH_EXIT                 = 0x128 | FLAG_AIR | FLAG_INTANGIBLE,
        UNUSED_DEATH_EXIT          = 0x129 | FLAG_AIR | FLAG_INTANGIBLE,
        FALLING_DEATH_EXIT         = 0x12A | FLAG_AIR | FLAG_INTANGIBLE,
        SPECIAL_EXIT_AIRBORNE      = 0x12B | FLAG_AIR | FLAG_INTANGIBLE,
        SPECIAL_DEATH_EXIT         = 0x12C | FLAG_AIR | FLAG_INTANGIBLE,
        FALLING_EXIT_AIRBORNE      = 0x12D | FLAG_AIR | FLAG_INTANGIBLE,
        UNLOCKING_KEY_DOOR         = 0x12E | FLAG_STATIONARY | FLAG_INTANGIBLE,
        UNLOCKING_STAR_DOOR        = 0x12F | FLAG_STATIONARY | FLAG_INTANGIBLE,
        ENTERING_STAR_DOOR         = 0x131 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        SPAWN_NO_SPIN_AIRBORNE     = 0x132 | FLAG_AIR | FLAG_INTANGIBLE,
        SPAWN_NO_SPIN_LANDING      = 0x133 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        BBH_ENTER_JUMP             = 0x134 | FLAG_AIR | FLAG_INTANGIBLE,
        BBH_ENTER_SPIN             = 0x135 | FLAG_MOVING | FLAG_INTANGIBLE,
        TELEPORT_FADE_OUT          = 0x136 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        TELEPORT_FADE_IN           = 0x137 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        SHOCKED                    = 0x138 | FLAG_STATIONARY | FLAG_INVULNERABLE,
        SQUISHED                   = 0x139 | FLAG_STATIONARY | FLAG_INVULNERABLE,
        HEAD_STUCK_IN_GROUND       = 0x13A | FLAG_STATIONARY | FLAG_INVULNERABLE,
        BUTT_STUCK_IN_GROUND       = 0x13B | FLAG_STATIONARY | FLAG_INVULNERABLE,
        FEET_STUCK_IN_GROUND       = 0x13C | FLAG_STATIONARY | FLAG_INVULNERABLE,
        PUTTING_ON_CAP             = 0x13D | FLAG_STATIONARY | FLAG_INTANGIBLE,

// group 0x140: "automatic" actions
        HOLDING_POLE               = 0x140 | FLAG_STATIONARY | FLAG_ON_POLE | FLAG_PAUSE_EXIT,
        GRAB_POLE_SLOW             = 0x141 | FLAG_STATIONARY | FLAG_ON_POLE,
        GRAB_POLE_FAST             = 0x142 | FLAG_STATIONARY | FLAG_ON_POLE,
        CLIMBING_POLE              = 0x143 | FLAG_STATIONARY | FLAG_ON_POLE,
        TOP_OF_POLE_TRANSITION     = 0x144 | FLAG_STATIONARY | FLAG_ON_POLE,
        TOP_OF_POLE                = 0x145 | FLAG_STATIONARY | FLAG_ON_POLE,
        START_HANGING              = 0x148 | FLAG_STATIONARY | FLAG_HANGING | FLAG_PAUSE_EXIT,
        HANGING                    = 0x149 | FLAG_STATIONARY | FLAG_HANGING,
        HANG_MOVING                = 0x14A | FLAG_MOVING | FLAG_HANGING,
        LEDGE_GRAB                 = 0x14B | FLAG_STATIONARY | FLAG_PAUSE_EXIT,
        LEDGE_CLIMB_SLOW_1         = 0x14C | FLAG_MOVING,
        LEDGE_CLIMB_SLOW_2         = 0x14D | FLAG_MOVING,
        LEDGE_CLIMB_DOWN           = 0x14E | FLAG_MOVING,
        LEDGE_CLIMB_FAST           = 0x14F | FLAG_MOVING,
        GRABBED                    = 0x170 | FLAG_STATIONARY | FLAG_INVULNERABLE,
        IN_CANNON                  = 0x171 | FLAG_STATIONARY | FLAG_INTANGIBLE,
        TORNADO_TWIRLING           = 0x172 | FLAG_STATIONARY | FLAG_INVULNERABLE | FLAG_SWIMMING_OR_FLYING,

// group 0x180: object actions
        PUNCHING                   = 0x180 | FLAG_STATIONARY | FLAG_ATTACKING,
        PICKING_UP                 = 0x183 | FLAG_STATIONARY,
        DIVE_PICKING_UP            = 0x185 | FLAG_STATIONARY,
        STOMACH_SLIDE_STOP         = 0x186 | FLAG_STATIONARY,
        PLACING_DOWN               = 0x187 | FLAG_STATIONARY,
        THROWING                   = 0x188 | FLAG_MOVING | FLAG_THROWING,
        HEAVY_THROW                = 0x189 | FLAG_MOVING | FLAG_THROWING,
        PICKING_UP_BOWSER          = 0x190 | FLAG_STATIONARY,
        HOLDING_BOWSER             = 0x191 | FLAG_STATIONARY,
        RELEASING_BOWSER           = 0x192 | FLAG_STATIONARY,
    }
    
    private static int SOUND_ARG_LOAD(int bank, int playFlags, int soundID, int priority, int flags2)
        => bank << 28 | playFlags << 24 | soundID << 16 | priority << 8 | flags2 << 4 | 1;
    public enum SM64Sound : int
    {
        TERRAIN_DEFAULT   = 0, // e.g. air
        TERRAIN_GRASS     = 1,
        TERRAIN_WATER     = 2,
        TERRAIN_STONE     = 3,
        TERRAIN_SPOOKY    = 4, // squeaky floor
        TERRAIN_SNOW      = 5,
        TERRAIN_ICE       = 6,
        TERRAIN_SAND      = 7,

        ACTION_TERRAIN_JUMP               = 0 << 28 | 4 << 24 | 0x00 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_TERRAIN_LANDING            = 0 << 28 | 4 << 24 | 0x08 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_TERRAIN_STEP               = 0 << 28 | 6 << 24 | 0x10 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_TERRAIN_BODY_HIT_GROUND    = 0 << 28 | 4 << 24 | 0x18 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_TERRAIN_STEP_TIPTOE        = 0 << 28 | 6 << 24 | 0x20 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_TERRAIN_STUCK_IN_GROUND    = 0 << 28 | 4 << 24 | 0x48 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_TERRAIN_HEAVY_LANDING      = 0 << 28 | 4 << 24 | 0x60 << 16 | 0x80 << 8 | 8 << 4 | 1,

        ACTION_METAL_JUMP                         = 0 << 28 | 4 << 24 | 0x28 << 16 | 0x90 << 8 | 8 << 4 | 1,
        ACTION_METAL_LANDING                      = 0 << 28 | 4 << 24 | 0x29 << 16 | 0x90 << 8 | 8 << 4 | 1,
        ACTION_METAL_STEP                         = 0 << 28 | 4 << 24 | 0x2A << 16 | 0x90 << 8 | 8 << 4 | 1,
        ACTION_METAL_HEAVY_LANDING                = 0 << 28 | 4 << 24 | 0x2B << 16 | 0x90 << 8 | 8 << 4 | 1,
        ACTION_CLAP_HANDS_COLD                    = 0 << 28 | 6 << 24 | 0x2C << 16 | 0x00 << 8 | 8 << 4 | 1,
        ACTION_HANGING_STEP                       = 0 << 28 | 4 << 24 | 0x2D << 16 | 0xA0 << 8 | 8 << 4 | 1,
        ACTION_QUICKSAND_STEP                     = 0 << 28 | 4 << 24 | 0x2E << 16 | 0x00 << 8 | 8 << 4 | 1,
        ACTION_METAL_STEP_TIPTOE                  = 0 << 28 | 4 << 24 | 0x2F << 16 | 0x90 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_UNKNOWN430      = 0 << 28 | 4 << 24 | 0x30 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_UNKNOWN431      = 0 << 28 | 4 << 24 | 0x31 << 16 | 0x60 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_UNKNOWN432      = 0 << 28 | 4 << 24 | 0x32 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_SWIM                               = 0 << 28 | 4 << 24 | 0x33 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_UNKNOWN434      = 0 << 28 | 4 << 24 | 0x34 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_THROW                              = 0 << 28 | 4 << 24 | 0x35 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_KEY_SWISH                          = 0 << 28 | 4 << 24 | 0x36 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_SPIN                               = 0 << 28 | 4 << 24 | 0x37 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_TWIRL                              = 0 << 28 | 4 << 24 | 0x38 << 16 | 0x80 << 8 | 8 << 4 | 1, // same sound as spin
/* not verified */         ACTION_CLIMB_UP_TREE   = 0 << 28 | 4 << 24 | 0x3A << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_CLIMB_DOWN_TREE = 0x003B,
/* not verified */         ACTION_UNK3C           = 0x003C,
/* not verified */         ACTION_UNKNOWN43D      = 0 << 28 | 4 << 24 | 0x3D << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_UNKNOWN43E      = 0 << 28 | 4 << 24 | 0x3E << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_PAT_BACK        = 0 << 28 | 4 << 24 | 0x3F << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_BRUSH_HAIR                         = 0 << 28 | 4 << 24 | 0x40 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_CLIMB_UP_POLE   = 0 << 28 | 4 << 24 | 0x41 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_METAL_BONK                         = 0 << 28 | 4 << 24 | 0x42 << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_UNSTUCK_FROM_GROUND                = 0 << 28 | 4 << 24 | 0x43 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_HIT             = 0 << 28 | 4 << 24 | 0x44 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_HIT_2           = 0 << 28 | 4 << 24 | 0x44 << 16 | 0xB0 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_HIT_3           = 0 << 28 | 4 << 24 | 0x44 << 16 | 0xA0 << 8 | 8 << 4 | 1,
        ACTION_BONK                               = 0 << 28 | 4 << 24 | 0x45 << 16 | 0xA0 << 8 | 8 << 4 | 1,
        ACTION_SHRINK_INTO_BBH                    = 0 << 28 | 4 << 24 | 0x46 << 16 | 0xA0 << 8 | 8 << 4 | 1,
        ACTION_SWIM_FAST                          = 0 << 28 | 4 << 24 | 0x47 << 16 | 0xA0 << 8 | 8 << 4 | 1,
        ACTION_METAL_JUMP_WATER                   = 0 << 28 | 4 << 24 | 0x50 << 16 | 0x90 << 8 | 8 << 4 | 1,
        ACTION_METAL_LAND_WATER                   = 0 << 28 | 4 << 24 | 0x51 << 16 | 0x90 << 8 | 8 << 4 | 1,
        ACTION_METAL_STEP_WATER                   = 0 << 28 | 4 << 24 | 0x52 << 16 | 0x90 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_UNK53           = 0x0053,
/* not verified */         ACTION_UNK54           = 0x0054,
/* not verified */         ACTION_UNK55           = 0x0055,
/* not verified */         ACTION_FLYING_FAST     = 0 << 28 | 4 << 24 | 0x56 << 16 | 0x80 << 8 | 8 << 4 | 1, // "swoop"?
        ACTION_TELEPORT                           = 0 << 28 | 4 << 24 | 0x57 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_UNKNOWN458      = 0 << 28 | 4 << 24 | 0x58 << 16 | 0xA0 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_BOUNCE_OFF_OBJECT   = 0 << 28 | 4 << 24 | 0x59 << 16 | 0xB0 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_SIDE_FLIP_UNK   = 0 << 28 | 4 << 24 | 0x5A << 16 | 0x80 << 8 | 8 << 4 | 1,
        ACTION_READ_SIGN                          = 0 << 28 | 4 << 24 | 0x5B << 16 | 0xFF << 8 | 8 << 4 | 1,
/* not verified */         ACTION_UNKNOWN45C      = 0 << 28 | 4 << 24 | 0x5C << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_UNK5D           = 0x005D,
/* not verified */         ACTION_INTRO_UNK45E    = 0 << 28 | 4 << 24 | 0x5E << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         ACTION_INTRO_UNK45F    = 0 << 28 | 4 << 24 | 0x5F << 16 | 0x80 << 8 | 8 << 4 | 1,

/* Moving Sound Effects */

// Terrain-dependent moving sounds; a value 0-7 is added to the sound ID before
// playing. See higher up for the different terrain types.
        MOVING_TERRAIN_SLIDE              = 1 << 28 | 4 << 24 | 0x00 << 16 | 0x00 << 8 | 0 << 4 | 1,
        MOVING_TERRAIN_RIDING_SHELL       = 1 << 28 | 4 << 24 | 0x20 << 16 | 0x00 << 8 | 0 << 4 | 1,

        MOVING_LAVA_BURN                  = 1 << 28 | 4 << 24 | 0x10 << 16 | 0x00 << 8 | 0 << 4 | 1, // ?
        MOVING_SLIDE_DOWN_POLE            = 1 << 28 | 4 << 24 | 0x11 << 16 | 0x00 << 8 | 0 << 4 | 1, // ?
        MOVING_SLIDE_DOWN_TREE            = 1 << 28 | 4 << 24 | 0x12 << 16 | 0x80 << 8 | 0 << 4 | 1,
        MOVING_QUICKSAND_DEATH            = 1 << 28 | 4 << 24 | 0x14 << 16 | 0x00 << 8 | 0 << 4 | 1,
        MOVING_SHOCKED                    = 1 << 28 | 4 << 24 | 0x16 << 16 | 0x00 << 8 | 0 << 4 | 1,
        MOVING_FLYING                     = 1 << 28 | 4 << 24 | 0x17 << 16 | 0x00 << 8 | 0 << 4 | 1,
        MOVING_ALMOST_DROWNING            = 1 << 28 | 0xC << 24 | 0x18 << 16 | 0x00 << 8 | 0 << 4 | 1,
        MOVING_AIM_CANNON                 = 1 << 28 | 0xD << 24 | 0x19 << 16 | 0x20 << 8 | 0 << 4 | 1,
        MOVING_UNK1A                      = 0x101A, // ?
        MOVING_RIDING_SHELL_LAVA          = 1 << 28 | 4 << 24 | 0x28 << 16 | 0x00 << 8 | 0 << 4 | 1,

/* Mario Sound Effects */
// A random number 0-2 is added to the sound ID before playing, producing Yah/Wah/Hoo
        MARIO_YAH_WAH_HOO                         = 2 << 28 | 4 << 24 | 0x00 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_HOOHOO           = 2 << 28 | 4 << 24 | 0x03 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_YAHOO            = 2 << 28 | 4 << 24 | 0x04 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_UH               = 2 << 28 | 4 << 24 | 0x05 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_HRMM             = 2 << 28 | 4 << 24 | 0x06 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_WAH2             = 2 << 28 | 4 << 24 | 0x07 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_WHOA             = 2 << 28 | 4 << 24 | 0x08 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_EEUH             = 2 << 28 | 4 << 24 | 0x09 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_ATTACKED         = 2 << 28 | 4 << 24 | 0x0A << 16 | 0xFF << 8 | 8 << 4 | 1,
/* not verified */         MARIO_OOOF             = 2 << 28 | 4 << 24 | 0x0B << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_OOOF2            = 2 << 28 | 4 << 24 | 0x0B << 16 | 0xD0 << 8 | 8 << 4 | 1,
        MARIO_HERE_WE_GO                          = 2 << 28 | 4 << 24 | 0x0C << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_YAWNING          = 2 << 28 | 4 << 24 | 0x0D << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_SNORING1                            = 2 << 28 | 4 << 24 | 0x0E << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_SNORING2                            = 2 << 28 | 4 << 24 | 0x0F << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_WAAAOOOW         = 2 << 28 | 4 << 24 | 0x10 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_HAHA             = 2 << 28 | 4 << 24 | 0x11 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_HAHA_2           = 2 << 28 | 4 << 24 | 0x11 << 16 | 0xF0 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_UH2              = 2 << 28 | 4 << 24 | 0x13 << 16 | 0xD0 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_UH2_2            = 2 << 28 | 4 << 24 | 0x13 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_ON_FIRE          = 2 << 28 | 4 << 24 | 0x14 << 16 | 0xA0 << 8 | 8 << 4 | 1,
/* not verified */         MARIO_DYING            = 2 << 28 | 4 << 24 | 0x15 << 16 | 0xFF << 8 | 8 << 4 | 1,
        MARIO_PANTING_COLD                        = 2 << 28 | 4 << 24 | 0x16 << 16 | 0x80 << 8 | 8 << 4 | 1,

// A random number 0-2 is added to the sound ID before playing
        MARIO_PANTING                     = 2 << 28 | 4 << 24 | 0x18 << 16 | 0x80 << 8 | 8 << 4 | 1,

        MARIO_COUGHING1                   = 2 << 28 | 4 << 24 | 0x1B << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_COUGHING2                   = 2 << 28 | 4 << 24 | 0x1C << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_COUGHING3                   = 2 << 28 | 4 << 24 | 0x1D << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_PUNCH_YAH                   = 2 << 28 | 4 << 24 | 0x1E << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_PUNCH_HOO                   = 2 << 28 | 4 << 24 | 0x1F << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_MAMA_MIA                    = 2 << 28 | 4 << 24 | 0x20 << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_OKEY_DOKEY                  = 0x2021,
        MARIO_GROUND_POUND_WAH            = 2 << 28 | 4 << 24 | 0x22 << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_DROWNING                    = 2 << 28 | 4 << 24 | 0x23 << 16 | 0xF0 << 8 | 8 << 4 | 1,
        MARIO_PUNCH_WAH                   = 2 << 28 | 4 << 24 | 0x24 << 16 | 0x80 << 8 | 8 << 4 | 1,

/* Mario Sound Effects (US/EU only), */
        PEACH_DEAR_MARIO                  = 2 << 28 | 4 << 24 | 0x28 << 16 | 0xFF << 8 | 8 << 4 | 1,

// A random number 0-4 is added to the sound ID before playing, producing one of
// Yahoo! (60% chance),, Waha! (20%),, or Yippee! (20%),.
        MARIO_YAHOO_WAHA_YIPPEE           = 2 << 28 | 4 << 24 | 0x2B << 16 | 0x80 << 8 | 8 << 4 | 1,

        MARIO_DOH                         = 2 << 28 | 4 << 24 | 0x30 << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_GAME_OVER                   = 2 << 28 | 4 << 24 | 0x31 << 16 | 0xFF << 8 | 8 << 4 | 1,
        MARIO_HELLO                       = 2 << 28 | 4 << 24 | 0x32 << 16 | 0xFF << 8 | 8 << 4 | 1,
        MARIO_PRESS_START_TO_PLAY         = 2 << 28 | 4 << 24 | 0x33 << 16 | 0xFF << 8 | 0xA << 4 | 1,
        MARIO_TWIRL_BOUNCE                = 2 << 28 | 4 << 24 | 0x34 << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_SNORING3                    = 2 << 28 | 4 << 24 | 0x35 << 16 | 0xFF << 8 | 8 << 4 | 1,
        MARIO_SO_LONGA_BOWSER             = 2 << 28 | 4 << 24 | 0x36 << 16 | 0x80 << 8 | 8 << 4 | 1,
        MARIO_IMA_TIRED                   = 2 << 28 | 4 << 24 | 0x37 << 16 | 0x80 << 8 | 8 << 4 | 1,

/* Princess Peach Sound Effects (US/EU only), */
        PEACH_MARIO                       = 2 << 28 | 4 << 24 | 0x38 << 16 | 0xFF << 8 | 8 << 4 | 1,
        PEACH_POWER_OF_THE_STARS          = 2 << 28 | 4 << 24 | 0x39 << 16 | 0xFF << 8 | 8 << 4 | 1,
        PEACH_THANKS_TO_YOU               = 2 << 28 | 4 << 24 | 0x3A << 16 | 0xFF << 8 | 8 << 4 | 1,
        PEACH_THANK_YOU_MARIO             = 2 << 28 | 4 << 24 | 0x3B << 16 | 0xFF << 8 | 8 << 4 | 1,
        PEACH_SOMETHING_SPECIAL           = 2 << 28 | 4 << 24 | 0x3C << 16 | 0xFF << 8 | 8 << 4 | 1,
        PEACH_BAKE_A_CAKE                 = 2 << 28 | 4 << 24 | 0x3D << 16 | 0xFF << 8 | 8 << 4 | 1,
        PEACH_FOR_MARIO                   = 2 << 28 | 4 << 24 | 0x3E << 16 | 0xFF << 8 | 8 << 4 | 1,
        PEACH_MARIO2                      = 2 << 28 | 4 << 24 | 0x3F << 16 | 0xFF << 8 | 8 << 4 | 1,

/* General Sound Effects */
        GENERAL_ACTIVATE_CAP_SWITCH                   = 3 << 28 | 0 << 24 | 0x00 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_FLAME_OUT          = 3 << 28 | 0 << 24 | 0x03 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_OPEN_WOOD_DOOR     = 3 << 28 | 0 << 24 | 0x04 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_CLOSE_WOOD_DOOR    = 3 << 28 | 0 << 24 | 0x05 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_OPEN_IRON_DOOR     = 3 << 28 | 0 << 24 | 0x06 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_CLOSE_IRON_DOOR    = 3 << 28 | 0 << 24 | 0x07 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BUBBLES            = 0x3008,
/* not verified */         GENERAL_MOVING_WATER       = 3 << 28 | 0 << 24 | 0x09 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_SWISH_WATER        = 3 << 28 | 0 << 24 | 0x0A << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_QUIET_BUBBLE       = 3 << 28 | 0 << 24 | 0x0B << 16 | 0x00 << 8 | 8 << 4 | 1,
        GENERAL_VOLCANO_EXPLOSION                     = 3 << 28 | 0 << 24 | 0x0C << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_QUIET_BUBBLE2      = 3 << 28 | 0 << 24 | 0x0D << 16 | 0x00 << 8 | 8 << 4 | 1,
        GENERAL_CASTLE_TRAP_OPEN                      = 3 << 28 | 0 << 24 | 0x0E << 16 | 0x80 << 8 | 8 << 4 | 1,
        GENERAL_WALL_EXPLOSION                        = 3 << 28 | 0 << 24 | 0x0F << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_COIN               = 3 << 28 | 8 << 24 | 0x11 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_COIN_WATER         = 3 << 28 | 8 << 24 | 0x12 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_SHORT_STAR         = 3 << 28 | 0 << 24 | 0x16 << 16 | 0x00 << 8 | 9 << 4 | 1,
/* not verified */         GENERAL_BIG_CLOCK          = 3 << 28 | 0 << 24 | 0x17 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_LOUD_POUND         = 0x3018, // _TERRAIN?
/* not verified */         GENERAL_LOUD_POUND2        = 0x3019,
/* not verified */         GENERAL_SHORT_POUND1       = 0x301A,
/* not verified */         GENERAL_SHORT_POUND2       = 0x301B,
/* not verified */         GENERAL_SHORT_POUND3       = 0x301C,
/* not verified */         GENERAL_SHORT_POUND4       = 0x301D,
/* not verified */         GENERAL_SHORT_POUND5       = 0x301E,
/* not verified */         GENERAL_SHORT_POUND6       = 0x301F,
        GENERAL_OPEN_CHEST                            = 3 << 28 | 1 << 24 | 0x20 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_CLAM_SHELL1        = 3 << 28 | 1 << 24 | 0x22 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BOX_LANDING        = 3 << 28 | 0 << 24 | 0x24 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BOX_LANDING_2      = 3 << 28 | 2 << 24 | 0x24 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_UNKNOWN1           = 3 << 28 | 0 << 24 | 0x25 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_UNKNOWN1_2         = 3 << 28 | 2 << 24 | 0x25 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_CLAM_SHELL2        = 3 << 28 | 0 << 24 | 0x26 << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_CLAM_SHELL3        = 3 << 28 | 0 << 24 | 0x27 << 16 | 0x40 << 8 | 8 << 4 | 1,
        GENERAL_PAINTING_EJECT                        = 3 << 28 | 9 << 24 | 0x28 << 16 | 0x00 << 8 | 8 << 4 | 1,
        GENERAL_LEVEL_SELECT_CHANGE                   = 3 << 28 | 0 << 24 | 0x2B << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_PLATFORM           = 3 << 28 | 0 << 24 | 0x2D << 16 | 0x80 << 8 | 8 << 4 | 1,
        GENERAL_DONUT_PLATFORM_EXPLOSION              = 3 << 28 | 0 << 24 | 0x2E << 16 | 0x20 << 8 | 8 << 4 | 1,
        GENERAL_BOWSER_BOMB_EXPLOSION                 = 3 << 28 | 1 << 24 | 0x2F << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_COIN_SPURT         = 3 << 28 | 0 << 24 | 0x30 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_COIN_SPURT_2       = 3 << 28 | 8 << 24 | 0x30 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_COIN_SPURT_EU      = 3 << 28 | 8 << 24 | 0x30 << 16 | 0x20 << 8 | 8 << 4 | 1,

/* not verified */         GENERAL_EXPLOSION6         = 0x3031,
/* not verified */         GENERAL_UNK32              = 0x3032,
/* not verified */         GENERAL_BOAT_TILT1         = 3 << 28 | 0 << 24 | 0x34 << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BOAT_TILT2         = 3 << 28 | 0 << 24 | 0x35 << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_COIN_DROP          = 3 << 28 | 0 << 24 | 0x36 << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_UNKNOWN3_LOWPRIO   = 3 << 28 | 0 << 24 | 0x37 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_UNKNOWN3           = 3 << 28 | 0 << 24 | 0x37 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_UNKNOWN3_2         = 3 << 28 | 8 << 24 | 0x37 << 16 | 0x80 << 8 | 8 << 4 | 1,
        GENERAL_PENDULUM_SWING                        = 3 << 28 | 0 << 24 | 0x38 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_CHAIN_CHOMP1       = 3 << 28 | 0 << 24 | 0x39 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_CHAIN_CHOMP2       = 3 << 28 | 0 << 24 | 0x3A << 16 | 0x00 << 8 | 8 << 4 | 1,
        GENERAL_DOOR_TURN_KEY                         = 3 << 28 | 0 << 24 | 0x3B << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_MOVING_IN_SAND     = 3 << 28 | 0 << 24 | 0x3C << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_UNKNOWN4_LOWPRIO   = 3 << 28 | 0 << 24 | 0x3D << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_UNKNOWN4           = 3 << 28 | 0 << 24 | 0x3D << 16 | 0x80 << 8 | 8 << 4 | 1,
        GENERAL_MOVING_PLATFORM_SWITCH                = 3 << 28 | 0 << 24 | 0x3E << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_CAGE_OPEN          = 3 << 28 | 0 << 24 | 0x3F << 16 | 0xA0 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_QUIET_POUND1_LOWPRIO   = 3 << 28 | 0 << 24 | 0x40 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_QUIET_POUND1       = 3 << 28 | 0 << 24 | 0x40 << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BREAK_BOX          = 3 << 28 | 0 << 24 | 0x41 << 16 | 0xC0 << 8 | 8 << 4 | 1,
        GENERAL_DOOR_INSERT_KEY                       = 3 << 28 | 0 << 24 | 0x42 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_QUIET_POUND2       = 3 << 28 | 0 << 24 | 0x43 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BIG_POUND          = 3 << 28 | 0 << 24 | 0x44 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_UNK45              = 3 << 28 | 0 << 24 | 0x45 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_UNK46_LOWPRIO      = 3 << 28 | 0 << 24 | 0x46 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_UNK46              = 3 << 28 | 0 << 24 | 0x46 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_CANNON_UP          = 3 << 28 | 0 << 24 | 0x47 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_GRINDEL_ROLL       = 3 << 28 | 0 << 24 | 0x48 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_EXPLOSION7         = 0x3049,
/* not verified */         GENERAL_SHAKE_COFFIN       = 0x304A,
/* not verified */         GENERAL_RACE_GUN_SHOT      = 3 << 28 | 1 << 24 | 0x4D << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_STAR_DOOR_OPEN     = 3 << 28 | 0 << 24 | 0x4E << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_STAR_DOOR_CLOSE    = 3 << 28 | 0 << 24 | 0x4F << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_POUND_ROCK         = 3 << 28 | 0 << 24 | 0x56 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_STAR_APPEARS       = 3 << 28 | 0 << 24 | 0x57 << 16 | 0xFF << 8 | 9 << 4 | 1,
        GENERAL_COLLECT_1UP                           = 3 << 28 | 0 << 24 | 0x58 << 16 | 0xFF << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BUTTON_PRESS_LOWPRIO   = 3 << 28 | 0 << 24 | 0x5A << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BUTTON_PRESS       = 3 << 28 | 0 << 24 | 0x5A << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BUTTON_PRESS_2_LOWPRIO = 3 << 28 | 1 << 24 | 0x5A << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BUTTON_PRESS_2     = 3 << 28 | 1 << 24 | 0x5A << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_ELEVATOR_MOVE      = 3 << 28 | 0 << 24 | 0x5B << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_ELEVATOR_MOVE_2    = 3 << 28 | 1 << 24 | 0x5B << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_SWISH_AIR          = 3 << 28 | 0 << 24 | 0x5C << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_SWISH_AIR_2        = 3 << 28 | 1 << 24 | 0x5C << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_HAUNTED_CHAIR      = 3 << 28 | 0 << 24 | 0x5D << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_SOFT_LANDING       = 3 << 28 | 0 << 24 | 0x5E << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_HAUNTED_CHAIR_MOVE = 3 << 28 | 0 << 24 | 0x5F << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BOWSER_PLATFORM    = 3 << 28 | 0 << 24 | 0x62 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BOWSER_PLATFORM_2  = 3 << 28 | 1 << 24 | 0x62 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_HEART_SPIN         = 3 << 28 | 0 << 24 | 0x64 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_POUND_WOOD_POST    = 3 << 28 | 0 << 24 | 0x65 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_WATER_LEVEL_TRIG   = 3 << 28 | 0 << 24 | 0x66 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_SWITCH_DOOR_OPEN   = 3 << 28 | 0 << 24 | 0x67 << 16 | 0xA0 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_RED_COIN           = 3 << 28 | 0 << 24 | 0x68 << 16 | 0x90 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BIRDS_FLY_AWAY     = 3 << 28 | 0 << 24 | 0x69 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_METAL_POUND        = 3 << 28 | 0 << 24 | 0x6B << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BOING1             = 3 << 28 | 0 << 24 | 0x6C << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BOING2_LOWPRIO     = 3 << 28 | 0 << 24 | 0x6D << 16 | 0x20 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BOING2             = 3 << 28 | 0 << 24 | 0x6D << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_YOSHI_WALK         = 3 << 28 | 0 << 24 | 0x6E << 16 | 0x20 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_ENEMY_ALERT1       = 3 << 28 | 0 << 24 | 0x6F << 16 | 0x30 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_YOSHI_TALK         = 3 << 28 | 0 << 24 | 0x70 << 16 | 0x30 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_SPLATTERING        = 3 << 28 | 0 << 24 | 0x71 << 16 | 0x30 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BOING3             = 0x3072,
/* not verified */         GENERAL_GRAND_STAR         = 3 << 28 | 0 << 24 | 0x73 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_GRAND_STAR_JUMP    = 3 << 28 | 0 << 24 | 0x74 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_BOAT_ROCK          = 3 << 28 | 0 << 24 | 0x75 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         GENERAL_VANISH_SFX         = 3 << 28 | 0 << 24 | 0x76 << 16 | 0x20 << 8 | 8 << 4 | 1,

/* Environment Sound Effects */
/* not verified */         ENV_WATERFALL1             = 4 << 28 | 0 << 24 | 0x00 << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_WATERFALL2             = 4 << 28 | 0 << 24 | 0x01 << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_ELEVATOR1              = 4 << 28 | 0 << 24 | 0x02 << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_DRONING1               = 4 << 28 | 1 << 24 | 0x03 << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_DRONING2               = 4 << 28 | 0 << 24 | 0x04 << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_WIND1                  = 4 << 28 | 0 << 24 | 0x05 << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_MOVING_SAND_SNOW       = 0x4006,
/* not verified */         ENV_UNK07                  = 0x4007,
/* not verified */         ENV_ELEVATOR2              = 4 << 28 | 0 << 24 | 0x08 << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_WATER                  = 4 << 28 | 0 << 24 | 0x09 << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_UNKNOWN2               = 4 << 28 | 0 << 24 | 0x0A << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_BOAT_ROCKING1          = 4 << 28 | 0 << 24 | 0x0B << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_ELEVATOR3              = 4 << 28 | 0 << 24 | 0x0C << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_ELEVATOR4              = 4 << 28 | 0 << 24 | 0x0D << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_ELEVATOR4_2            = 4 << 28 | 1 << 24 | 0x0D << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_MOVINGSAND             = 4 << 28 | 0 << 24 | 0x0E << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_MERRY_GO_ROUND_CREAKING    = 4 << 28 | 0 << 24 | 0x0F << 16 | 0x40 << 8 | 0 << 4 | 1,
/* not verified */         ENV_WIND2                  = 4 << 28 | 0 << 24 | 0x10 << 16 | 0x80 << 8 | 0 << 4 | 1,
/* not verified */         ENV_UNK12                  = 0x4012,
/* not verified */         ENV_SLIDING                = 4 << 28 | 0 << 24 | 0x13 << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_STAR                   = 4 << 28 | 0 << 24 | 0x14 << 16 | 0x00 << 8 | 1 << 4 | 1,
/* not verified */         ENV_UNKNOWN4               = 4 << 28 | 1 << 24 | 0x15 << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_WATER_DRAIN            = 4 << 28 | 1 << 24 | 0x16 << 16 | 0x00 << 8 | 0 << 4 | 1,
/* not verified */         ENV_METAL_BOX_PUSH         = 4 << 28 | 0 << 24 | 0x17 << 16 | 0x80 << 8 | 0 << 4 | 1,
/* not verified */         ENV_SINK_QUICKSAND         = 4 << 28 | 0 << 24 | 0x18 << 16 | 0x80 << 8 | 0 << 4 | 1,

/* Object Sound Effects */
        OBJ_SUSHI_SHARK_WATER_SOUND                   = 5 << 28 | 0 << 24 | 0x00 << 16 | 0x80 << 8 | 8 << 4 | 1,
        OBJ_MRI_SHOOT                                 = 5 << 28 | 0 << 24 | 0x01 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_BABY_PENGUIN_WALK                         = 5 << 28 | 0 << 24 | 0x02 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_BOWSER_WALK                               = 5 << 28 | 0 << 24 | 0x03 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_BOWSER_TAIL_PICKUP                        = 5 << 28 | 0 << 24 | 0x05 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_BOWSER_DEFEATED                           = 5 << 28 | 0 << 24 | 0x06 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_BOWSER_SPINNING                           = 5 << 28 | 0 << 24 | 0x07 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_BOWSER_INHALING                           = 5 << 28 | 0 << 24 | 0x08 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_BIG_PENGUIN_WALK                          = 5 << 28 | 0 << 24 | 0x09 << 16 | 0x80 << 8 | 8 << 4 | 1,
        OBJ_BOO_BOUNCE_TOP                            = 5 << 28 | 0 << 24 | 0x0A << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_BOO_LAUGH_SHORT                           = 5 << 28 | 0 << 24 | 0x0B << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_THWOMP                                    = 5 << 28 | 0 << 24 | 0x0C << 16 | 0xA0 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_CANNON1                = 5 << 28 | 0 << 24 | 0x0D << 16 | 0xF0 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_CANNON2                = 5 << 28 | 0 << 24 | 0x0E << 16 | 0xF0 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_CANNON3                = 5 << 28 | 0 << 24 | 0x0F << 16 | 0xF0 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_JUMP_WALK_WATER        = 0x5012,
/* not verified */         OBJ_UNKNOWN2               = 5 << 28 | 0 << 24 | 0x13 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_MRI_DEATH                                 = 5 << 28 | 0 << 24 | 0x14 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_POUNDING1              = 5 << 28 | 0 << 24 | 0x15 << 16 | 0x50 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_POUNDING1_HIGHPRIO     = 5 << 28 | 0 << 24 | 0x15 << 16 | 0x80 << 8 | 8 << 4 | 1,
        OBJ_WHOMP_LOWPRIO                             = 5 << 28 | 0 << 24 | 0x16 << 16 | 0x60 << 8 | 8 << 4 | 1,
        OBJ_KING_BOBOMB                               = 5 << 28 | 0 << 24 | 0x16 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_BULLY_METAL            = 5 << 28 | 0 << 24 | 0x17 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_BULLY_EXPLODE          = 5 << 28 | 0 << 24 | 0x18 << 16 | 0xA0 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_BULLY_EXPLODE_2        = 5 << 28 | 1 << 24 | 0x18 << 16 | 0xA0 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_POUNDING_CANNON        = 5 << 28 | 0 << 24 | 0x1A << 16 | 0x50 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_BULLY_WALK             = 5 << 28 | 0 << 24 | 0x1B << 16 | 0x30 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_UNKNOWN3               = 5 << 28 | 0 << 24 | 0x1D << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_UNKNOWN4               = 5 << 28 | 0 << 24 | 0x1E << 16 | 0xA0 << 8 | 8 << 4 | 1,
        OBJ_BABY_PENGUIN_DIVE                         = 5 << 28 | 0 << 24 | 0x1F << 16 | 0x40 << 8 | 8 << 4 | 1,
        OBJ_GOOMBA_WALK                               = 5 << 28 | 0 << 24 | 0x20 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_UKIKI_CHATTER_LONG                        = 5 << 28 | 0 << 24 | 0x21 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_MONTY_MOLE_ATTACK                         = 5 << 28 | 0 << 24 | 0x22 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_EVIL_LAKITU_THROW                         = 5 << 28 | 0 << 24 | 0x22 << 16 | 0x20 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_UNK23                  = 0x5023,
        OBJ_DYING_ENEMY1                              = 5 << 28 | 0 << 24 | 0x24 << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_CANNON4                = 5 << 28 | 0 << 24 | 0x25 << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_DYING_ENEMY2           = 0x5026,
        OBJ_BOBOMB_WALK                               = 5 << 28 | 0 << 24 | 0x27 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_SOMETHING_LANDING      = 5 << 28 | 0 << 24 | 0x28 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_DIVING_IN_WATER        = 5 << 28 | 0 << 24 | 0x29 << 16 | 0xA0 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_SNOW_SAND1             = 5 << 28 | 0 << 24 | 0x2A << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_SNOW_SAND2             = 5 << 28 | 0 << 24 | 0x2B << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_DEFAULT_DEATH                             = 5 << 28 | 0 << 24 | 0x2C << 16 | 0x80 << 8 | 8 << 4 | 1,
        OBJ_BIG_PENGUIN_YELL                          = 5 << 28 | 0 << 24 | 0x2D << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_WATER_BOMB_BOUNCING                       = 5 << 28 | 0 << 24 | 0x2E << 16 | 0x80 << 8 | 8 << 4 | 1,
        OBJ_GOOMBA_ALERT                              = 5 << 28 | 0 << 24 | 0x2F << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_WIGGLER_JUMP                              = 5 << 28 | 0 << 24 | 0x2F << 16 | 0x60 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_STOMPED                = 5 << 28 | 0 << 24 | 0x30 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_UNKNOWN6               = 5 << 28 | 0 << 24 | 0x31 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_DIVING_INTO_WATER      = 5 << 28 | 0 << 24 | 0x32 << 16 | 0x40 << 8 | 8 << 4 | 1,
        OBJ_PIRANHA_PLANT_SHRINK                      = 5 << 28 | 0 << 24 | 0x33 << 16 | 0x40 << 8 | 8 << 4 | 1,
        OBJ_KOOPA_THE_QUICK_WALK                      = 5 << 28 | 0 << 24 | 0x34 << 16 | 0x20 << 8 | 8 << 4 | 1,
        OBJ_KOOPA_WALK                                = 5 << 28 | 0 << 24 | 0x35 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_BULLY_WALKING                             = 5 << 28 | 0 << 24 | 0x36 << 16 | 0x60 << 8 | 8 << 4 | 1,
        OBJ_DORRIE                                    = 5 << 28 | 0 << 24 | 0x37 << 16 | 0x60 << 8 | 8 << 4 | 1,
        OBJ_BOWSER_LAUGH                              = 5 << 28 | 0 << 24 | 0x38 << 16 | 0x80 << 8 | 8 << 4 | 1,
        OBJ_UKIKI_CHATTER_SHORT                       = 5 << 28 | 0 << 24 | 0x39 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_UKIKI_CHATTER_IDLE                        = 5 << 28 | 0 << 24 | 0x3A << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_UKIKI_STEP_DEFAULT                        = 5 << 28 | 0 << 24 | 0x3B << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_UKIKI_STEP_LEAVES                         = 5 << 28 | 0 << 24 | 0x3C << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_KOOPA_TALK                                = 5 << 28 | 0 << 24 | 0x3D << 16 | 0xA0 << 8 | 8 << 4 | 1,
        OBJ_KOOPA_DAMAGE                              = 5 << 28 | 0 << 24 | 0x3E << 16 | 0xA0 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_KLEPTO1                = 5 << 28 | 0 << 24 | 0x3F << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_KLEPTO2                = 5 << 28 | 0 << 24 | 0x40 << 16 | 0x60 << 8 | 8 << 4 | 1,
        OBJ_KING_BOBOMB_TALK                          = 5 << 28 | 0 << 24 | 0x41 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_KING_BOBOMB_JUMP                          = 5 << 28 | 0 << 24 | 0x46 << 16 | 0x80 << 8 | 8 << 4 | 1,
        OBJ_KING_WHOMP_DEATH                          = 5 << 28 | 1 << 24 | 0x47 << 16 | 0xC0 << 8 | 8 << 4 | 1,
        OBJ_BOO_LAUGH_LONG                            = 5 << 28 | 0 << 24 | 0x48 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_EEL                    = 5 << 28 | 0 << 24 | 0x4A << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_EEL_2                  = 5 << 28 | 2 << 24 | 0x4A << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_EYEROK_SHOW_EYE                           = 5 << 28 | 2 << 24 | 0x4B << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_MR_BLIZZARD_ALERT                         = 5 << 28 | 0 << 24 | 0x4C << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_SNUFIT_SHOOT                              = 5 << 28 | 0 << 24 | 0x4D << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_SKEETER_WALK                              = 5 << 28 | 0 << 24 | 0x4E << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_WALKING_WATER          = 5 << 28 | 0 << 24 | 0x4F << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_BIRD_CHIRP3                               = 5 << 28 | 0 << 24 | 0x51 << 16 | 0x40 << 8 | 0 << 4 | 1,
        OBJ_PIRANHA_PLANT_APPEAR                      = 5 << 28 | 0 << 24 | 0x54 << 16 | 0x20 << 8 | 8 << 4 | 1,
        OBJ_FLAME_BLOWN                               = 5 << 28 | 0 << 24 | 0x55 << 16 | 0x80 << 8 | 8 << 4 | 1,
        OBJ_MAD_PIANO_CHOMPING                        = 5 << 28 | 2 << 24 | 0x56 << 16 | 0x40 << 8 | 8 << 4 | 1,
        OBJ_BOBOMB_BUDDY_TALK                         = 5 << 28 | 0 << 24 | 0x58 << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_SPINY_UNK59            = 5 << 28 | 0 << 24 | 0x59 << 16 | 0x10 << 8 | 8 << 4 | 1,
        OBJ_WIGGLER_HIGH_PITCH                        = 5 << 28 | 0 << 24 | 0x5C << 16 | 0x40 << 8 | 8 << 4 | 1,
        OBJ_HEAVEHO_TOSSED                            = 5 << 28 | 0 << 24 | 0x5D << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_WIGGLER_DEATH          = 0x505E,
        OBJ_BOWSER_INTRO_LAUGH                        = 5 << 28 | 0 << 24 | 0x5F << 16 | 0x80 << 8 | 9 << 4 | 1,
/* not verified */         OBJ_ENEMY_DEATH_HIGH       = 5 << 28 | 0 << 24 | 0x60 << 16 | 0xB0 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_ENEMY_DEATH_LOW        = 5 << 28 | 0 << 24 | 0x61 << 16 | 0xB0 << 8 | 8 << 4 | 1,
        OBJ_SWOOP_DEATH                               = 5 << 28 | 0 << 24 | 0x62 << 16 | 0xB0 << 8 | 8 << 4 | 1,
        OBJ_KOOPA_FLYGUY_DEATH                        = 5 << 28 | 0 << 24 | 0x63 << 16 | 0xB0 << 8 | 8 << 4 | 1,
        OBJ_POKEY_DEATH                               = 5 << 28 | 0 << 24 | 0x63 << 16 | 0xC0 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_SNOWMAN_BOUNCE         = 5 << 28 | 0 << 24 | 0x64 << 16 | 0xC0 << 8 | 8 << 4 | 1,
        OBJ_SNOWMAN_EXPLODE                           = 5 << 28 | 0 << 24 | 0x65 << 16 | 0xD0 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_POUNDING_LOUD          = 5 << 28 | 0 << 24 | 0x68 << 16 | 0x40 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_MIPS_RABBIT            = 5 << 28 | 0 << 24 | 0x6A << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         OBJ_MIPS_RABBIT_WATER      = 5 << 28 | 0 << 24 | 0x6C << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_EYEROK_EXPLODE                            = 5 << 28 | 0 << 24 | 0x6D << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_CHUCKYA_DEATH                             = 5 << 28 | 1 << 24 | 0x6E << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_WIGGLER_TALK                              = 5 << 28 | 0 << 24 | 0x6F << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ_WIGGLER_ATTACKED                          = 5 << 28 | 0 << 24 | 0x70 << 16 | 0x60 << 8 | 8 << 4 | 1,
        OBJ_WIGGLER_LOW_PITCH                         = 5 << 28 | 0 << 24 | 0x71 << 16 | 0x20 << 8 | 8 << 4 | 1,
        OBJ_SNUFIT_SKEETER_DEATH                      = 5 << 28 | 0 << 24 | 0x72 << 16 | 0xC0 << 8 | 8 << 4 | 1,
        OBJ_BUBBA_CHOMP                               = 5 << 28 | 0 << 24 | 0x73 << 16 | 0x40 << 8 | 8 << 4 | 1,
        OBJ_ENEMY_DEFEAT_SHRINK                       = 5 << 28 | 0 << 24 | 0x74 << 16 | 0x40 << 8 | 8 << 4 | 1,

        AIR_BOWSER_SPIT_FIRE              = 6 << 28 | 0 << 24 | 0x00 << 16 | 0x00 << 8 | 0 << 4 | 1,
        AIR_UNK01                         = 0x6001, // ?
        AIR_LAKITU_FLY                    = 6 << 28 | 0 << 24 | 0x02 << 16 | 0x80 << 8 | 0 << 4 | 1,
        AIR_LAKITU_FLY_HIGHPRIO           = 6 << 28 | 0 << 24 | 0x02 << 16 | 0xFF << 8 | 0 << 4 | 1,
        AIR_AMP_BUZZ                      = 6 << 28 | 0 << 24 | 0x03 << 16 | 0x40 << 8 | 0 << 4 | 1,
        AIR_BLOW_FIRE                     = 6 << 28 | 0 << 24 | 0x04 << 16 | 0x80 << 8 | 0 << 4 | 1,
        AIR_BLOW_WIND                     = 6 << 28 | 0 << 24 | 0x04 << 16 | 0x40 << 8 | 0 << 4 | 1,
        AIR_ROUGH_SLIDE                   = 6 << 28 | 0 << 24 | 0x05 << 16 | 0x00 << 8 | 0 << 4 | 1,
        AIR_HEAVEHO_MOVE                  = 6 << 28 | 0 << 24 | 0x06 << 16 | 0x40 << 8 | 0 << 4 | 1,
        AIR_UNK07                         = 0x6007, // ?
        AIR_BOBOMB_LIT_FUSE               = 6 << 28 | 0 << 24 | 0x08 << 16 | 0x60 << 8 | 0 << 4 | 1,
        AIR_HOWLING_WIND                  = 6 << 28 | 0 << 24 | 0x09 << 16 | 0x80 << 8 | 0 << 4 | 1,
        AIR_CHUCKYA_MOVE                  = 6 << 28 | 0 << 24 | 0x0A << 16 | 0x40 << 8 | 0 << 4 | 1,
        AIR_PEACH_TWINKLE                 = 6 << 28 | 0 << 24 | 0x0B << 16 | 0x40 << 8 | 0 << 4 | 1,
        AIR_CASTLE_OUTDOORS_AMBIENT       = 6 << 28 | 0 << 24 | 0x10 << 16 | 0x40 << 8 | 0 << 4 | 1,

/* Menu Sound Effects */
        MENU_CHANGE_SELECT                            = 7 << 28 | 0 << 24 | 0x00 << 16 | 0xF8 << 8 | 8 << 4 | 1,
/* not verified */         MENU_REVERSE_PAUSE         = 0x7001,
        MENU_PAUSE                                    = 7 << 28 | 0 << 24 | 0x02 << 16 | 0xF0 << 8 | 8 << 4 | 1,
        MENU_PAUSE_HIGHPRIO                           = 7 << 28 | 0 << 24 | 0x02 << 16 | 0xFF << 8 | 8 << 4 | 1,
        MENU_PAUSE_2                                  = 7 << 28 | 0 << 24 | 0x03 << 16 | 0xFF << 8 | 8 << 4 | 1,
        MENU_MESSAGE_APPEAR                           = 7 << 28 | 0 << 24 | 0x04 << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_MESSAGE_DISAPPEAR                        = 7 << 28 | 0 << 24 | 0x05 << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_CAMERA_ZOOM_IN                           = 7 << 28 | 0 << 24 | 0x06 << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_CAMERA_ZOOM_OUT                          = 7 << 28 | 0 << 24 | 0x07 << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_PINCH_MARIO_FACE                         = 7 << 28 | 0 << 24 | 0x08 << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_LET_GO_MARIO_FACE                        = 7 << 28 | 0 << 24 | 0x09 << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_HAND_APPEAR                              = 7 << 28 | 0 << 24 | 0x0A << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_HAND_DISAPPEAR                           = 7 << 28 | 0 << 24 | 0x0B << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         MENU_UNK0C                 = 7 << 28 | 0 << 24 | 0x0C << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         MENU_POWER_METER           = 7 << 28 | 0 << 24 | 0x0D << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_CAMERA_BUZZ                              = 7 << 28 | 0 << 24 | 0x0E << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_CAMERA_TURN                              = 7 << 28 | 0 << 24 | 0x0F << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         MENU_UNK10                 = 0x7010,
        MENU_CLICK_FILE_SELECT                        = 7 << 28 | 0 << 24 | 0x11 << 16 | 0x00 << 8 | 8 << 4 | 1,
/* not verified */         MENU_MESSAGE_NEXT_PAGE     = 7 << 28 | 0 << 24 | 0x13 << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_COIN_ITS_A_ME_MARIO                      = 7 << 28 | 0 << 24 | 0x14 << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_YOSHI_GAIN_LIVES                         = 7 << 28 | 0 << 24 | 0x15 << 16 | 0x00 << 8 | 8 << 4 | 1,
        MENU_ENTER_PIPE                               = 7 << 28 | 0 << 24 | 0x16 << 16 | 0xA0 << 8 | 8 << 4 | 1,
        MENU_EXIT_PIPE                                = 7 << 28 | 0 << 24 | 0x17 << 16 | 0xA0 << 8 | 8 << 4 | 1,
        MENU_BOWSER_LAUGH                             = 7 << 28 | 0 << 24 | 0x18 << 16 | 0x80 << 8 | 8 << 4 | 1,
        MENU_ENTER_HOLE                               = 7 << 28 | 1 << 24 | 0x19 << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MENU_CLICK_CHANGE_VIEW     = 7 << 28 | 0 << 24 | 0x1A << 16 | 0x80 << 8 | 8 << 4 | 1,
/* not verified */         MENU_CAMERA_UNUSED1        = 0x701B,
/* not verified */         MENU_CAMERA_UNUSED2        = 0x701C,
/* not verified */         MENU_MARIO_CASTLE_WARP     = 7 << 28 | 0 << 24 | 0x1D << 16 | 0xB0 << 8 | 8 << 4 | 1,
        MENU_STAR_SOUND                               = 7 << 28 | 0 << 24 | 0x1E << 16 | 0xFF << 8 | 8 << 4 | 1,
        MENU_THANK_YOU_PLAYING_MY_GAME                = 7 << 28 | 0 << 24 | 0x1F << 16 | 0xFF << 8 | 8 << 4 | 1,
/* not verified */         MENU_READ_A_SIGN           = 0x7020,
/* not verified */         MENU_EXIT_A_SIGN           = 0x7021,
/* not verified */         MENU_MARIO_CASTLE_WARP2    = 7 << 28 | 0 << 24 | 0x22 << 16 | 0x20 << 8 | 8 << 4 | 1,
        MENU_STAR_SOUND_OKEY_DOKEY                    = 7 << 28 | 0 << 24 | 0x23 << 16 | 0xFF << 8 | 8 << 4 | 1,
        MENU_STAR_SOUND_LETS_A_GO                     = 7 << 28 | 0 << 24 | 0x24 << 16 | 0xFF << 8 | 8 << 4 | 1,

// US/EU only; an index between 0-7 or 0-4 is added to the sound ID before
// playing, producing the same sound with different pitch.
        MENU_COLLECT_RED_COIN             = 7 << 28 | 8 << 24 | 0x28 << 16 | 0x90 << 8 | 8 << 4 | 1,
        MENU_COLLECT_SECRET               = 7 << 28 | 0 << 24 | 0x30 << 16 | 0x20 << 8 | 8 << 4 | 1,

// Channel 8 loads sounds from the same place as channel 3, making it possible
// to play two channel 3 sounds at once (since just one sound from each channel
// can play at a given time),.
        GENERAL2_BOBOMB_EXPLOSION         = 8 << 28 | 0 << 24 | 0x2E << 16 | 0x20 << 8 | 8 << 4 | 1,
        GENERAL2_PURPLE_SWITCH            = 8 << 28 | 0 << 24 | 0x3E << 16 | 0xC0 << 8 | 8 << 4 | 1,
        GENERAL2_ROTATING_BLOCK_CLICK     = 8 << 28 | 0 << 24 | 0x40 << 16 | 0x00 << 8 | 8 << 4 | 1,
        GENERAL2_SPINDEL_ROLL             = 8 << 28 | 0 << 24 | 0x48 << 16 | 0x20 << 8 | 8 << 4 | 1,
        GENERAL2_PYRAMID_TOP_SPIN         = 8 << 28 | 1 << 24 | 0x4B << 16 | 0xE0 << 8 | 8 << 4 | 1,
        GENERAL2_PYRAMID_TOP_EXPLOSION    = 8 << 28 | 1 << 24 | 0x4C << 16 | 0xF0 << 8 | 8 << 4 | 1,
        GENERAL2_BIRD_CHIRP2              = 8 << 28 | 0 << 24 | 0x50 << 16 | 0x40 << 8 | 0 << 4 | 1,
        GENERAL2_SWITCH_TICK_FAST         = 8 << 28 | 0 << 24 | 0x54 << 16 | 0xF0 << 8 | 1 << 4 | 1,
        GENERAL2_SWITCH_TICK_SLOW         = 8 << 28 | 0 << 24 | 0x55 << 16 | 0xF0 << 8 | 1 << 4 | 1,
        GENERAL2_STAR_APPEARS             = 8 << 28 | 0 << 24 | 0x57 << 16 | 0xFF << 8 | 9 << 4 | 1,
        GENERAL2_ROTATING_BLOCK_ALERT     = 8 << 28 | 0 << 24 | 0x59 << 16 | 0x00 << 8 | 8 << 4 | 1,
        GENERAL2_BOWSER_EXPLODE           = 8 << 28 | 0 << 24 | 0x60 << 16 | 0x00 << 8 | 8 << 4 | 1,
        GENERAL2_BOWSER_KEY               = 8 << 28 | 0 << 24 | 0x61 << 16 | 0x00 << 8 | 8 << 4 | 1,
        GENERAL2_1UP_APPEAR               = 8 << 28 | 0 << 24 | 0x63 << 16 | 0xD0 << 8 | 8 << 4 | 1,
        GENERAL2_RIGHT_ANSWER             = 8 << 28 | 0 << 24 | 0x6A << 16 | 0xA0 << 8 | 8 << 4 | 1,

// Channel 9 loads sounds from the same place as channel 5.
        OBJ2_BOWSER_ROAR                  = 9 << 28 | 0 << 24 | 0x04 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ2_PIRANHA_PLANT_BITE           = 9 << 28 | 0 << 24 | 0x10 << 16 | 0x50 << 8 | 8 << 4 | 1,
        OBJ2_PIRANHA_PLANT_DYING          = 9 << 28 | 0 << 24 | 0x11 << 16 | 0x60 << 8 | 8 << 4 | 1,
        OBJ2_BOWSER_PUZZLE_PIECE_MOVE     = 9 << 28 | 0 << 24 | 0x19 << 16 | 0x20 << 8 | 8 << 4 | 1,
        OBJ2_BULLY_ATTACKED               = 9 << 28 | 0 << 24 | 0x1C << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ2_KING_BOBOMB_DAMAGE           = 9 << 28 | 1 << 24 | 0x42 << 16 | 0x40 << 8 | 8 << 4 | 1,
        OBJ2_SCUTTLEBUG_WALK              = 9 << 28 | 0 << 24 | 0x43 << 16 | 0x40 << 8 | 8 << 4 | 1,
        OBJ2_SCUTTLEBUG_ALERT             = 9 << 28 | 0 << 24 | 0x44 << 16 | 0x40 << 8 | 8 << 4 | 1,
        OBJ2_BABY_PENGUIN_YELL            = 9 << 28 | 0 << 24 | 0x45 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ2_SWOOP                        = 9 << 28 | 0 << 24 | 0x49 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ2_BIRD_CHIRP1                  = 9 << 28 | 0 << 24 | 0x52 << 16 | 0x40 << 8 | 0 << 4 | 1,
        OBJ2_LARGE_BULLY_ATTACKED         = 9 << 28 | 0 << 24 | 0x57 << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ2_EYEROK_SOUND_SHORT           = 9 << 28 | 3 << 24 | 0x5A << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ2_WHOMP_SOUND_SHORT            = 9 << 28 | 3 << 24 | 0x5A << 16 | 0xC0 << 8 | 8 << 4 | 1,
        OBJ2_EYEROK_SOUND_LONG            = 9 << 28 | 2 << 24 | 0x5B << 16 | 0x00 << 8 | 8 << 4 | 1,
        OBJ2_BOWSER_TELEPORT              = 9 << 28 | 0 << 24 | 0x66 << 16 | 0x80 << 8 | 8 << 4 | 1,
        OBJ2_MONTY_MOLE_APPEAR            = 9 << 28 | 0 << 24 | 0x67 << 16 | 0x80 << 8 | 8 << 4 | 1,
        OBJ2_BOSS_DIALOG_GRUNT            = 9 << 28 | 0 << 24 | 0x69 << 16 | 0x40 << 8 | 8 << 4 | 1,
        OBJ2_MRI_SPINNING                 = 9 << 28 | 0 << 24 | 0x6B << 16 | 0x00 << 8 | 8 << 4 | 1,
    }

    #endregion
}