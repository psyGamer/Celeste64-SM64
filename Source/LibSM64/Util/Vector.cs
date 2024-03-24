using System.Runtime.InteropServices;

namespace LibSM64.Util;

[StructLayout(LayoutKind.Sequential)]
public struct SM64Vector2f(float x, float y)
{
    public float x = x, y = y;
}

[StructLayout(LayoutKind.Sequential)]
public struct SM64Vector3f(float x, float y, float z)
{
    public float x = x, y = y, z = z;
}

public static class Extensions
{
    // !! IMPORTANT !! In SM64 Y and Z are flipped compared to C64.
    
    public static Vec2 ToVec2(this SM64Vector2f vec) => new(vec.x, vec.y);
    public static Vec3 ToVec3(this SM64Vector3f vec) => new(vec.x, vec.z, vec.y); 
    
    public static SM64Vector2f ToSM64Vec2(this Vec2 vec) => new(vec.X, vec.Y);
    public static SM64Vector3f ToSM64Vec3(this Vec3 vec) => new(vec.X, vec.Z, vec.Y);
}