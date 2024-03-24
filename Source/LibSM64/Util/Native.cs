using System.Runtime.InteropServices;
using Sledge.Formats.Map.Objects;

namespace LibSM64.Util;

public class Native
{
    #region Structs
    
    [StructLayout(LayoutKind.Sequential)]
    public struct SM64Surface
    {
        public SM64SurfaceType type;
        public short force;
        public SM64TerrainType terrain;
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
    
    [DllImport("sm64")]
    public static extern int sm64_mario_create(float x, float y, float z);
    [DllImport("sm64")]
    public static extern void sm64_mario_tick(int marioId, ref SM64MarioInputs inputs, ref SM64MarioState outState, ref SM64MarioGeometryBuffers outBuffers);
    [DllImport("sm64")]
    public static extern void sm64_mario_delete(int marioId);

    [DllImport("sm64")]
    public static extern void sm64_set_mario_action(int marioId, uint action);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_action_arg(int marioId, uint action, uint actionArg);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_animation(int marioId, int animID);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_anim_frame(int marioId, short animFrame);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_state(int marioId, uint flags);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_position(int marioId, float x, float y, float z);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_angle(int marioId, float x, float y, float z);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_faceangle(int marioId, float y);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_velocity(int marioId, float x, float y, float z);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_forward_velocity(int marioId, float vel);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_invincibility(int marioId, short timer);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_water_level(int marioId, int level);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_gas_level(int marioId, int level);
    [DllImport("sm64")]
    public static extern void sm64_set_mario_health(int marioId, ushort health);
    [DllImport("sm64")]
    public static extern void sm64_mario_take_damage(int marioId, uint damage, uint subtype, float x, float y, float z);
    [DllImport("sm64")]
    public static extern void sm64_mario_heal(int marioId, byte healCounter);
    [DllImport("sm64")]
    public static extern void sm64_mario_kill(int marioId);
    [DllImport("sm64")]
    public static extern void sm64_mario_interact_cap(int marioId, uint capFlag, ushort capTime, byte playMusic);
    [DllImport("sm64")]
    public static extern void sm64_mario_extend_cap(int marioId, ushort capTime);
    [DllImport("sm64")]
    public static extern bool sm64_mario_attack(int marioId, float x, float y, float z, float hitboxHeight);
    
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
    
    #endregion
}