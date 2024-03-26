using System.Reflection;
using static LibSM64.Native;

namespace LibSM64;

public class DynamicCollisionMesh
{
    public readonly uint ObjectID;
    
    public unsafe DynamicCollisionMesh(SM64Surface[] surfaces, SM64ObjectTransform transform)
    {
        fixed (SM64Surface* pSurfaces = surfaces)
        {
            var surfaceObject = new SM64SurfaceObject()
            {
                transform = transform,
                surfaceCount = (uint)surfaces.Length,
                surfaces = pSurfaces,
            };
            ObjectID = sm64_surface_object_create(ref surfaceObject);
        }
    }
    
    ~DynamicCollisionMesh() => Dispose();
    public void Dispose()
    {
        sm64_surface_object_delete(ObjectID);
        GC.SuppressFinalize(this);
    }
    
    public void Move(SM64ObjectTransform transform)
    {
        sm64_surface_object_move(ObjectID, ref transform);
    }
}

public class CollisionMeshBuilder
{
    private readonly List<SM64Surface> surfaces = [];
            
    // Access the lists internal array
    private static readonly FieldInfo f_items = typeof(List<SM64Surface>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)!;
    
    public void BuildStatic()
    {
        var surfacesArray = (SM64Surface[])f_items.GetValue(surfaces)!;
        sm64_static_surfaces_load(surfacesArray, (uint)surfacesArray.Length);
    }

    public DynamicCollisionMesh BuildDynamic(SM64ObjectTransform transform) => new((SM64Surface[])f_items.GetValue(surfaces)!, transform);
    
    public CollisionMeshBuilder AddTriangle(
        SM64SurfaceType surfaceType,
        SM64TerrainType terrainType,
        SM64Vector3f vertex1,
        SM64Vector3f vertex2,
        SM64Vector3f vertex3)
    {
        surfaces.Add(new SM64Surface
        {
            type = (short)surfaceType,
            terrain = (ushort)terrainType,
            
            v0x = (int)vertex1.x,
            v0y = (int)vertex1.y,
            v0z = (int)vertex1.z,
            v1x = (int)vertex2.x,
            v1y = (int)vertex2.y,
            v1z = (int)vertex2.z,
            v2x = (int)vertex3.x,
            v2y = (int)vertex3.y,
            v2z = (int)vertex3.z,
        });

        return this;
    }

    public CollisionMeshBuilder AddQuad(
        SM64SurfaceType surfaceType,
        SM64TerrainType terrainType,
        SM64Vector3f vertex1,
        SM64Vector3f vertex2,
        SM64Vector3f vertex3,
        SM64Vector3f vertex4)
    {
        AddTriangle(
            surfaceType,
            terrainType,
            vertex1,
            vertex2,
            vertex3);
        AddTriangle(
            surfaceType,
            terrainType,
            vertex4,
            vertex3,
            vertex2);

        return this;
    }
}