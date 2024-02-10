namespace LibSM64Sharp;

// TODO: Fork libsm64 to expose more features:
// TODO: - Mario's enum state
// TODO: - Mario's animation enum
// TODO: - when sounds should play
// TODO: - loading sounds from game
// TODO: - loading other data from game?
// TODO: - events?
// TODO: - support for picking things up
public interface ISm64Mario : IDisposable
{
    ISm64Gamepad Gamepad { get; }
    ISm64MarioMesh Mesh { get; }

    IReadOnlySm64Vector3<float> Position { get; }
    float FaceAngle { get; }
    IReadOnlySm64Vector3<float> Velocity { get; }
    short Health { get; }
    
    void Tick();
    
    public void SetState(uint flags);
    
    public void SetPosition(float x, float y, float z);
    public void SetVelocity(float x, float y, float z);
    public void SetForwardVelocity(float vel);

    public void SetAngle(float x, float y, float z);
    public void SetFaceAngle(float y);
        
    public void SetInvincible(float y, short timer);
        
    public void SetHealth(ushort health);
    public void TakeDamage(uint damage, uint subtype, float x, float y, float z);
    public void Heal(byte healCounter);
}