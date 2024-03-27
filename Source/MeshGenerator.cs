using System.Reflection;
using LibSM64;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Sledge.Formats.Map.Objects;
using SledgeSolid = Sledge.Formats.Map.Objects.Solid;
using static LibSM64.Native;

namespace Celeste64.Mod.SuperMario64;

public static class MeshGenerator
{
    internal static void Load()
    {
        modelMeshes.Clear();
        solidMeshes.Clear();
        //HookManager.Instance.RegisterILHook(new ILHook(typeof(Map).GetMethod(nameof(Map.GenerateModel), BindingFlags.NonPublic | BindingFlags.Instance)!, IL_Map_GenerateModel));
        HookManager.Instance.RegisterHook(new Hook(typeof(Map).GetMethod(nameof(Map.GenerateSolid), BindingFlags.NonPublic | BindingFlags.Instance)!, On_Map_GenerateSolid));
    }
    
    private static readonly Dictionary<SimpleModel, CollisionMeshBuilder> modelMeshes = [];
    private static readonly Dictionary<Solid, CollisionMeshBuilder> solidMeshes = [];
    private static readonly Dictionary<Solid, DynamicCollisionMesh> dynamicMeshes = [];
    internal static readonly Dictionary<uint, Solid> BreakableSolids = [];

    public static void GenerateSolids(World world)
    {
        // Cleanup old data. Static geometry will just get overwritten entirely
        foreach (var dynamicMesh in dynamicMeshes.Values)
            dynamicMesh.Dispose();
        dynamicMeshes.Clear();
        BreakableSolids.Clear();
        
        var staticBuilder = new CollisionMeshBuilder();

        // Bounding box of all solids combined
        float minX = 0, maxX = 0, minZ = 0, maxZ = 0;
        
        // Gather all solids, even if they aren't part of the World yet
        var solids = world.All<Solid>().Cast<Solid>()
                    .Concat(world.adding.OfType<Solid>());
        
        foreach (var solid in solids)
        {
            // Only generate for solid level geometry
            if (solid.GetType() != typeof(Solid))
                continue;
            
            if (solidMeshes.TryGetValue(solid, out var builder))
            {
                staticBuilder.Combine(builder);
            } else
            {
                Log.Info($"builder not found: {solidMeshes.Count}");
            }
            
            minX = Math.Min(minX, solid.WorldBounds.Min.X);
            maxX = Math.Max(maxX, solid.WorldBounds.Max.X);
            minZ = Math.Min(minZ, solid.WorldBounds.Min.Y);
            maxZ = Math.Max(maxZ, solid.WorldBounds.Max.Y);
        }

        // Add death plane
        const int DeathPlaneInflate = (int)(100 * MarioPlayer.C64_To_SM64_Pos);
        const int DeathPlaneOffset = (int)(1000 * MarioPlayer.C64_To_SM64_Pos);
        staticBuilder.AddQuad(SM64SurfaceType.DEATH_PLANE, SM64TerrainType.GRASS, 
            new SM64Vector3f(minX * MarioPlayer.C64_To_SM64_Pos - DeathPlaneInflate, world.DeathPlane * MarioPlayer.C64_To_SM64_Pos - DeathPlaneOffset, maxZ * MarioPlayer.C64_To_SM64_Pos + DeathPlaneInflate),
            new SM64Vector3f(maxX * MarioPlayer.C64_To_SM64_Pos + DeathPlaneInflate, world.DeathPlane * MarioPlayer.C64_To_SM64_Pos - DeathPlaneOffset, maxZ * MarioPlayer.C64_To_SM64_Pos + DeathPlaneInflate),
            new SM64Vector3f(minX * MarioPlayer.C64_To_SM64_Pos - DeathPlaneInflate, world.DeathPlane * MarioPlayer.C64_To_SM64_Pos - DeathPlaneOffset, minZ * MarioPlayer.C64_To_SM64_Pos - DeathPlaneInflate),
            new SM64Vector3f(maxX * MarioPlayer.C64_To_SM64_Pos + DeathPlaneInflate, world.DeathPlane * MarioPlayer.C64_To_SM64_Pos - DeathPlaneOffset, minZ * MarioPlayer.C64_To_SM64_Pos - DeathPlaneInflate));
        
        staticBuilder.BuildStatic();
        
        // Only include solids which already exist. Added/Destroyed is handled inside the hooks
        foreach (var solid in world.All<Solid>().Cast<Solid>())
        {
            CreateDynamicObjectMesh(solid);
        }
    }
    
     [On.Celeste64.Actor.Added]
    private static void On_Actor_Added(On.Celeste64.Actor.orig_Added orig, Actor self)
    {
        orig(self);
        
        if (self is Solid solid)
            CreateDynamicObjectMesh(solid);
    }
    
    [On.Celeste64.Solid.Destroyed]
    private static void On_Solid_Destroyed(On.Celeste64.Solid.orig_Destroyed orig, Solid self)
    {
        if (dynamicMeshes.Remove(self, out var dynamicMesh))
            dynamicMesh.Dispose();
        orig(self);
    }
    
    private static void CreateDynamicObjectMesh(Solid solid)
    {
        // Only generate for dynamic actor geometry
        if (solid.GetType() == typeof(Solid))
            return;
            
        if (!solidMeshes.TryGetValue(solid, out var objectBuilder))
            return;
            
        var dynamicMesh = objectBuilder.BuildDynamic(new SM64ObjectTransform()
        {
            position = solid.Position.ToSM64Vec3(),
            eulerRotation = solid.RotationXYZ.AsSM64Vec3(),
        }); 
            
        if (solid is IDashTrigger)
        {
            BreakableSolids.Add(dynamicMesh.ObjectID, solid);
        }
            
        dynamicMeshes.Add(solid, dynamicMesh);
    }
    
    [On.Celeste64.Actor.ValidateTransformations]
    private static void On_Actor_ValidateTransformations(On.Celeste64.Actor.orig_ValidateTransformations orig, Actor self)
    {
        if (self.dirty && self is Solid solid && dynamicMeshes.TryGetValue(solid, out var dynamicMesh))
        {
            dynamicMesh.Move(new SM64ObjectTransform()
            {
                // Predict next position, since SM64 only runs at 30 FPS
                position = (solid.Position + solid.Velocity / 2.0f * Time.Delta).ToSM64Vec3() ,
                eulerRotation = solid.RotationXYZ.AsSM64Vec3(),
            });
        }
        
        orig(self);
    }
    
    private delegate void orig_GenerateSolid(Map self, Solid into, List<SledgeSolid> collection);
    private static void On_Map_GenerateSolid(orig_GenerateSolid orig, Map self, Solid into, List<SledgeSolid> collection)
    {
        // get bounds
        var transform = self.baseTransform;
        var bounds = self.CalculateSolidBounds(collection, transform);
        var center = bounds.Center;
        transform *= Matrix.CreateTranslation(-center);
        
        var builder = new CollisionMeshBuilder();
        
        foreach (var solid in collection)
        {
            foreach (var face in solid.Faces)
            {
                // Guess terrain type based on texture
                SM64TerrainType terrainType;
                if (face.TextureName.StartsWith("snow"))
                    terrainType = SM64TerrainType.SNOW;
                else if (face.TextureName.StartsWith("wall_ruined") || face.TextureName.StartsWith("concrete") || face.TextureName.StartsWith("girder") || face.TextureName.StartsWith("road") || face.TextureName.StartsWith("rock"))
                    terrainType = SM64TerrainType.STONE;
                else
                    terrainType = SM64TerrainType.GRASS;
        
                // Triangulate the face
                for (int i = 0; i < face.Vertices.Count - 2; i ++)
                {
                    // Use world coords for static geometry and local coords for dynamic geometry
                    if (into.GetType() == typeof(Solid))
                    {
                        builder.AddTriangle(SM64SurfaceType.DEFAULT, terrainType,
                            Vec3.Transform(face.Vertices[0], self.baseTransform).ToSM64Vec3(),
                            Vec3.Transform(face.Vertices[2 + i], self.baseTransform).ToSM64Vec3(),
                            Vec3.Transform(face.Vertices[1 + i], self.baseTransform).ToSM64Vec3());
                    }
                    else
                    {
                        builder.AddTriangle(SM64SurfaceType.DEFAULT, terrainType,
                            Vec3.Transform(face.Vertices[0], transform).ToSM64Vec3(),
                            Vec3.Transform(face.Vertices[2 + i], transform).ToSM64Vec3(),
                            Vec3.Transform(face.Vertices[1 + i], transform).ToSM64Vec3());
                    }
                }
            }
        }
        
        solidMeshes[into] = builder;
        orig(self, into, collection);
    }
    
    private static void IL_Map_GenerateModel(ILContext il)
    {
        var cur = new ILCursor(il);
        
        // 1st loop
        cur.GotoNext(instr => instr.MatchLdstr("__"));
        cur.GotoNext(instr => instr.MatchLdstr("TB_empty"));
        cur.GotoNext(instr => instr.MatchLdstr("invisible"));
        // 2nd loop
        cur.GotoNext(instr => instr.MatchLdstr("__"));
        cur.GotoNext(instr => instr.MatchLdstr("TB_empty"));
        cur.GotoNext(instr => instr.MatchLdstr("invisible"));
        cur.GotoNext(instr => instr.MatchLdstr("wall"));
        
        // Find face variable usage
        cur.GotoNext(instr => instr.MatchCallvirt<Face>("get_Plane"));
        // The variable must be loaded before this
        int faceIdx = -1;
        cur.GotoPrev(instr => instr.MatchLdloc(out faceIdx));
     
        cur.EmitLdarg1(); // SimpleModel model
        cur.EmitLdloc(faceIdx);
        cur.EmitDelegate(GenerateMeshForModel);
    }
    
    private static void GenerateMeshForModel(SimpleModel model, Face face)
    {
        if (!modelMeshes.TryGetValue(model, out var builder))
        {
            builder = new CollisionMeshBuilder();
            modelMeshes[model] = builder;
        }
        
        // Guess terrain type based on texture
        SM64TerrainType terrainType;
        if (face.TextureName.StartsWith("snow"))
            terrainType = SM64TerrainType.SNOW;
        else if (face.TextureName.StartsWith("wall_ruined") || face.TextureName.StartsWith("concrete") || face.TextureName.StartsWith("girder") || face.TextureName.StartsWith("road") || face.TextureName.StartsWith("rock"))
            terrainType = SM64TerrainType.STONE;
        else
            terrainType = SM64TerrainType.GRASS;
        
        // Triangulate the mesh
        for (int i = 0; i < face.Vertices.Count - 2; i ++)
        {
            builder.AddTriangle(SM64SurfaceType.DEFAULT, terrainType,
                face.Vertices[0].ToSM64Vec3(),
                face.Vertices[2 + i].ToSM64Vec3(),
                face.Vertices[1 + i].ToSM64Vec3());
        }
    }
}