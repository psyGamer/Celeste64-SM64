using System.Runtime.InteropServices;
using LibSM64Sharp.LowLevel;

namespace LibSM64;

public class DynamicCollisionMesh
{
    #region Native Interop

    [StructLayout(LayoutKind.Sequential)]
    private struct ObjectTransform
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] position;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] eulerRotation;
    };
    
    [StructLayout(LayoutKind.Sequential)]
    private struct SurfaceObject
    {
        public ObjectTransform transform;
        public uint surfaceCount;
        public IntPtr surfaces;
    }
    
    private static extern uint sm64_surface_object_create(ref SurfaceObject surfaceObject);
    private static extern void sm64_surface_object_move(uint objectId, ref ObjectTransform transform);
    private static extern void sm64_surface_object_delete(uint objectId);

    #endregion
}