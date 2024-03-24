using static LibSM64.Native;

namespace LibSM64;

public class Mario
{
    public struct GamepadState
    {
        public Vec2 Stick;
        public Vec2 CameraLook;
        public bool AButtonDown;
        public bool BButtonDown;
        public bool ZButtonDown;
    }
    
    private readonly int id;
    
    private SM64MarioState state;
    
    public GamepadState Gamepad;
    public readonly MarioMesh Mesh;
    
    public Mario(float x, float y, float z)
    {
        id = sm64_mario_create(x, y, z);
        if (id == -1)
            throw new Exception("Failed to create Mario! Have you created a floor for him to stand on yet?");
        
        Mesh = new MarioMesh();
    }
    
    public Mario(Vec3 vec) : this(vec.X, vec.Y, vec.Z) { }

    ~Mario() => Dispose();

    public void Dispose()
    {
        sm64_mario_delete(id);
        GC.SuppressFinalize(this);
    }
    
    public SM64Vector3f Position
    {
        get => state.position;
        set => sm64_set_mario_position(id, value.x, value.y, value.z);
    }
    public SM64Vector3f Velocity
    {
        get => state.velocity;
        set => sm64_set_mario_velocity(id, value.x, value.y, value.z);
    }
    public float FaceAngle
    {
        get => state.faceAngle;
        set => sm64_set_mario_faceangle(id, value);
    }
    
    public SM64Action Action
    {
        get => (SM64Action)state.action;
        set => sm64_set_mario_action(id, (uint)value);
    }
    
    public SM64MarioAnimID Animation
    {
        get => throw new NotImplementedException();
        set => sm64_set_mario_animation(id, (int)value);
    }
    
    public void Kill() => sm64_mario_kill(id);
    
    public bool ReadyToSpeak
    {
        get
        {
            uint actionGroup = state.action & (uint)SM64Action.GROUP_MASK;
            return (state.action == (uint)SM64Action.WAITING_FOR_DIALOG || actionGroup == (uint)SM64Action.GROUP_STATIONARY || actionGroup == (uint)SM64Action.GROUP_MOVING)
                  && (state.action & ((uint)SM64Action.FLAG_RIDING_SHELL | (uint)SM64Action.FLAG_INVULNERABLE)) == 0
                  && state.action != (uint)SM64Action.FIRST_PERSON;
        }
    }

    public unsafe void Tick()
    {
        var inputs = new SM64MarioInputs
        {
            buttonA = (byte)(Gamepad.AButtonDown ? 1 : 0),
            buttonB = (byte)(Gamepad.BButtonDown ? 1 : 0),
            buttonZ = (byte)(Gamepad.ZButtonDown ? 1 : 0),
            stickX = Gamepad.Stick.X,
            stickY = Gamepad.Stick.Y,
            camLookX = Gamepad.CameraLook.X,
            camLookZ = Gamepad.CameraLook.Y,
        };
        
        fixed (SM64Vector3f* pPos = Mesh.PositionsBuffer)
        fixed (SM64Vector3f* pNormal = Mesh.NormalsBuffer)
        fixed (SM64Vector3f* pColor = Mesh.ColorsBuffer)
        fixed (SM64Vector2f* pUV = Mesh.UvsBuffer)
        {
            var buffers = new SM64MarioGeometryBuffers
            {
                position = (IntPtr)pPos,
                normal = (IntPtr)pNormal,
                color = (IntPtr)pColor,
                uv = (IntPtr)pUV,
            };

            sm64_mario_tick(id, ref inputs, ref state, ref buffers);
            
            Mesh.UpdateBuffers(buffers.numTrianglesUsed);
        }
        
    }
}