using System.Runtime.InteropServices;
using LibSM64.Util;

namespace LibSM64;

public class SM64Mario
{
    public struct GamepadState
    {
        public Vec2 Stick;
        public Vec2 CameraLook;
        public bool AButtonDown;
        public bool BButtonDown;
        public bool ZButtonDown;
    }
    
    public SM64Mario(float x, float y, float z)
    {
        ID = sm64_mario_create(x, y, z);
        if (ID == -1)
            throw new Exception("Failed to create Mario! Have you created a floor for him to stand on yet?");
    }
    
    public SM64Mario(Vec3 vec) : this(vec.X, vec.Y, vec.Z) { }

    ~SM64Mario()
    {
        Dispose();
        GC.SuppressFinalize(this);
    }

    private readonly int ID;
    
    public readonly GamepadState Gamepad;

    public void Dispose() => sm64_mario_delete(ID);
    
    public void Tick()
    {
        var inputs = new Inputs
        {
            buttonA = (byte)(Gamepad.AButtonDown ? 1 : 0),
            buttonB = (byte)(Gamepad.BButtonDown ? 1 : 0),
            buttonZ = (byte)(Gamepad.ZButtonDown ? 1 : 0),
            stickX = Gamepad.Stick.X,
            stickY = Gamepad.Stick.Y,
            camLookX = Gamepad.CameraLook.X,
            camLookZ = Gamepad.CameraLook.Y,
        };
    }

    #region Native Interop

    [StructLayout(LayoutKind.Sequential)]
    private struct Inputs
    {
        public float camLookX, camLookZ;
        public float stickX, stickY;
        public byte buttonA, buttonB, buttonZ;
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct State
    {
        public SM64Vector3f position;
        public SM64Vector3f velocity;
        public float faceAngle;
        public short health;
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct GeometryBuffers
    {
        public IntPtr position;
        public IntPtr normal;
        public IntPtr color;
        public IntPtr uv;
        public ushort numTrianglesUsed;
    };

    private static extern int sm64_mario_create(float x, float y, float z);
    private static extern void sm64_mario_tick(int marioId, ref Inputs inputs, ref State outState, ref GeometryBuffers outBuffers);
    private static extern void sm64_mario_delete(int marioId);

    private static extern void sm64_set_mario_action(int marioId, uint action);
    private static extern void sm64_set_mario_action_arg(int marioId, uint action, uint actionArg);
    private static extern void sm64_set_mario_animation(int marioId, int animID);
    private static extern void sm64_set_mario_anim_frame(int marioId, short animFrame);
    private static extern void sm64_set_mario_state(int marioId, uint flags);
    private static extern void sm64_set_mario_position(int marioId, float x, float y, float z);
    private static extern void sm64_set_mario_angle(int marioId, float x, float y, float z);
    private static extern void sm64_set_mario_faceangle(int marioId, float y);
    private static extern void sm64_set_mario_velocity(int marioId, float x, float y, float z);
    private static extern void sm64_set_mario_forward_velocity(int marioId, float vel);
    private static extern void sm64_set_mario_invincibility(int marioId, short timer);
    private static extern void sm64_set_mario_water_level(int marioId, int level);
    private static extern void sm64_set_mario_gas_level(int marioId, int level);
    private static extern void sm64_set_mario_health(int marioId, ushort health);
    private static extern void sm64_mario_take_damage(int marioId, uint damage, uint subtype, float x, float y, float z);
    private static extern void sm64_mario_heal(int marioId, byte healCounter);
    private static extern void sm64_mario_kill(int marioId);
    private static extern void sm64_mario_interact_cap(int marioId, uint capFlag, ushort capTime, byte playMusic);
    private static extern void sm64_mario_extend_cap(int marioId, ushort capTime);
    private static extern bool sm64_mario_attack(int marioId, float x, float y, float z, float hitboxHeight);

    #endregion
}