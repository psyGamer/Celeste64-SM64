using System.Reflection;
using LibSM64.Util;
using static LibSM64.Util.Native;

namespace LibSM64;

public class StaticCollisionMesh
{
    public class Builder
    {
        private readonly List<SM64Surface> surfaces = [];
        
        // Access the lists internal array
        private static readonly FieldInfo f_items = typeof(List<SM64Surface>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)!;
        public StaticCollisionMesh Build() => new((SM64Surface[])f_items.GetValue(surfaces)!);
        
        public Builder AddTriangle(
            SM64SurfaceType surfaceType,
            SM64TerrainType terrainType,
            SM64Vector3f vertex1,
            SM64Vector3f vertex2,
            SM64Vector3f vertex3)
        {
            surfaces.Add(new SM64Surface
            {
                type = surfaceType,
                terrain = terrainType,
                
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

        public Builder AddQuad(
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
    
    public StaticCollisionMesh(SM64Surface[] surfaces)
    {
        sm64_static_surfaces_load(surfaces, (uint)surfaces.Length);
    }
}