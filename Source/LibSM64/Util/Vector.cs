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
