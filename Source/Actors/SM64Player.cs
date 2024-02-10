using System.Runtime.InteropServices;
using LibSM64Sharp;
using LibSM64Sharp.Impl;
using LibSM64Sharp.LowLevel;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste64.Mod.SuperMario64;

public class SM64Player : Player
{
    /// <summary>
    /// A single unit in SM64 and C64 are different sizes.
    /// This constant transforms a SM64 unit into a C64 one.
    /// </summary>
    private const float UnitScaleFactor = 0.1f;
    
    private const float SM64_To_C64 = UnitScaleFactor;
    private const float C64_To_SM64 = 1.0f / UnitScaleFactor;
    
    private class MarioModel : Model
    {
        private readonly Mesh Mesh = new();
        private readonly DefaultMaterial Material;
        
        private readonly ISm64Mario Mario;
        
        public MarioModel(ISm64Mario mario)
        {
            Mario = mario;
            
            Material = new DefaultMaterial();
            Material.SetShader(Assets.Shaders["Mario"]);
            if (Material.Shader?.Has("u_color") ?? false)
                Material.Set("u_color", Material.Color);
            if (Material.Shader?.Has("u_effects") ?? false)
                Material.Set("u_effects", Material.Effects);
            
            // Mario is animated through updated vertices from libsm64
            if (Material.Shader != null && Material.Shader.Has("u_jointMult"))
                Material.Set("u_jointMult", 0.0f);

            Flags = ModelFlags.Default;             
            
            // unsafe
            // {
            //     MarioMesh.Texture.DangerousTryGetSinglePixelMemory(out var textureMemory);
            //     using var pinHandle = textureMemory.Pin();
            //     Texture = new Texture((IntPtr)pinHandle.Pointer, MarioMesh.Texture.Width, MarioMesh.Texture.Height, TextureFormat.R8G8B8A8);
            // }
            // Texture = Assets.Textures["white"];
        }
        
        ~MarioModel()
        {
            // Mesh.Dispose();
            // Materials.ForEach(mat => mat.Texture?.Dispose());
        }
        
        public override void Render(ref RenderState state)
        {
            // Log.Info(Mario.Mesh.TriangleData);
            // Log.Info(Mario.Mesh.TriangleData == null);
            // Log.Info(Mario.Mesh.TriangleData.TriangleCount.ToString());
            
            if (Mario.Mesh.TriangleData is not { } data) return;
        
            List<Vertex> vertices = [];
            List<int> indices = [];
            
            for (int i = 0; i < data.TriangleCount * 3; i++)
            {
                vertices.Add(new Vertex(data.Positions[i].ToVec3(), data.Uvs[i].ToVec2(), data.Colors[i].ToVec3(), data.Normals[i].ToVec3()));
                indices.Add(i);
            }

            Mesh.SetVertices<Vertex>(CollectionsMarshal.AsSpan(vertices));
            Mesh.SetIndices<int>(CollectionsMarshal.AsSpan(indices));
            
            
            //
            
            state.ApplyToMaterial(Material, Matrix.Identity);
        
            Material.Texture = Mario.Mesh.Texture;
            Material.Model = Matrix.CreateTranslation(-Mario.Position.ToVec3()) * Matrix.CreateScale(UnitScaleFactor) * Matrix.CreateTranslation(Mario.Position.ToVec3() * UnitScaleFactor);
            Material.MVP = Material.Model * state.Camera.ViewProjection;
            
            var call = new DrawCommand(state.Camera.Target, Mesh, Material)
            {
                DepthCompare = state.DepthCompare,
                DepthMask = state.DepthMask,
                CullMode = CullMode.None,
                MeshIndexStart = 0,
                MeshIndexCount = data.TriangleCount * 3,
            };
            call.Submit();
            state.Calls++;
            state.Triangles += data.TriangleCount * 3;
        }
    }
    
    // private static ILHook? il_Player_LateUpdate;
    
    internal static void Load()
    {
        // il_Player_LateUpdate = new ILHook(typeof(Player).GetMethod("LateUpdate")!, IL_Player_LateUpdate);
    }
    internal static void Unload()
    {
        // il_Player_LateUpdate?.Dispose();
    }
    
    /// <summary>
    /// Removes the easing from the camera
    /// </summary>
    private static void IL_Player_LateUpdate(ILContext il)
    {
        var cur = new ILCursor(il);
        // Goto: GetCameraTarget()
        cur.GotoNext(instr => instr.MatchCallvirt<Player>("GetCameraTarget"));
        // Get the cameraPosition out variable
        int cameraPositionIdx = -1;
        cur.GotoPrev(instr => instr.MatchLdloca(out _)); // bool snapRequested
        cur.GotoPrev(instr => instr.MatchLdloca(out cameraPositionIdx));
        
        // Goto before: this.World.Camera.LookAt = cameraLookAt;
        cur.GotoNext(instr => instr.MatchCall<Actor>("get_World"));
        cur.GotoNext(instr => instr.MatchCall<Actor>("get_World"));
        cur.GotoNext(instr => instr.MatchCall<Actor>("get_World"));
        
        cur.EmitLdarg0();
        cur.EmitLdloc(il.Body.Variables[cameraPositionIdx]);
        cur.EmitDelegate(SetCamera);
        
        static void SetCamera(Player self, Vec3 cameraPosition)
        {
            self.World.Camera.Position = cameraPosition;
        }

        Console.WriteLine(il);
    }

    
    private ISm64Context Context = null!;
    private ISm64Mario? Mario = null;
    
    private MarioModel MarioPlayerModel = null!;
    
    /// <summary>
    /// SM64 runs at 30FPS but C64 at 60FPS, so we need to skip every odd frame.
    /// </summary>
    private bool IsOddFrame = false;

    public override Vec3 Position => Mario != null ? Mario.Position.ToVec3() * UnitScaleFactor : position;
    public override Vec3 Velocity => Mario != null ? Mario.Velocity.ToVec3() * UnitScaleFactor : velocity;

    public override void Added()
    {
        base.Added();
        
        var romBytes = File.ReadAllBytes("sm64.z64");
        
        Context = Sm64Context.InitFromRom(romBytes);
        
        // LibSm64Interop.sm64_static_surfaces_load(Data.surfaces, (ulong)Data.surfaces.Length);
        // // int marioId = LibSm64Interop.sm64_mario_create(0, 1000, 0);
        // Mario = Context.CreateMario(0, 1000, 0);
        
        var builder = Context.CreateStaticCollisionMesh();
        
        // Bounding box of all solids combined
        int minX = 0, maxX = 0, minZ = 0, maxZ = 0;
        
        foreach (var solid in World.All<Solid>().Cast<Solid>())
        {
            var verts = solid.WorldVertices;

            foreach (var face in solid.WorldFaces)
            {
                // Triangulate the mesh
                for (int i = 0; i < face.VertexCount - 2; i ++)
                {
                    builder.AddTriangle(Sm64SurfaceType.SURFACE_DEFAULT, Sm64TerrainType.TERRAIN_GRASS,
                        Deconstruct(verts[face.VertexStart + 0]),
                        Deconstruct(verts[face.VertexStart + 2 + i]),
                        Deconstruct(verts[face.VertexStart + 1 + i]));
                }
            }

            continue;

            (int x, int y, int z) Deconstruct(Vec3 vec)
            {
                int x = (int)(vec.X / UnitScaleFactor);
                // !! IMPORTANT !! In SM64 Y and Z are flipped compared to C64.
                int z = (int)(vec.Y / UnitScaleFactor);
                int y = (int)(vec.Z / UnitScaleFactor);
                
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minZ = Math.Min(minZ, z);
                maxZ = Math.Max(maxZ, z);
                
                return (x, y, z);
            }
        }

        // Add death plane
        const int DeathPlaneInflate = 10;
        builder.AddQuad(Sm64SurfaceType.SURFACE_DEATH_PLANE, Sm64TerrainType.TERRAIN_GRASS, 
            ((int x, int y, int z))(minX - DeathPlaneInflate, World.DeathPlane / UnitScaleFactor, maxZ + DeathPlaneInflate),
            ((int x, int y, int z))(maxX + DeathPlaneInflate, World.DeathPlane / UnitScaleFactor, maxZ + DeathPlaneInflate),
            ((int x, int y, int z))(minX - DeathPlaneInflate, World.DeathPlane / UnitScaleFactor, minZ - DeathPlaneInflate),
            ((int x, int y, int z))(maxX + DeathPlaneInflate, World.DeathPlane / UnitScaleFactor, minZ - DeathPlaneInflate));
        
        builder.Build();

        Mario = Context.CreateMario(Position.X / UnitScaleFactor, Position.Z / UnitScaleFactor, Position.Y / UnitScaleFactor);
        
        // Initial tick to set everything up
        Mario.Tick();
        
        MarioPlayerModel = new MarioModel(Mario);
        MarioPlayerModel.Flags |= ModelFlags.Silhouette; 
        Log.Info($"Mario ID: {Mario}");
    }

    public override void Destroyed()
    {
        base.Destroyed();

        Mario?.Dispose();
        Context.Dispose();
    }

    public override void Update()
    {
        base.Update();
        
        Mario.Gamepad.AnalogStick.X = -Controls.Move.Value.X;
        Mario.Gamepad.AnalogStick.Y = Controls.Move.Value.Y;
        Mario.Gamepad.IsAButtonDown = Controls.Jump.Down;
        Mario.Gamepad.IsBButtonDown = Controls.Dash.Down;
        Mario.Gamepad.IsZButtonDown = Controls.Climb.Down;
        
        GetCameraTarget(out var cameraLookAt, out var cameraPosition, out bool snapRequested);
        Mario.Gamepad.CameraNormal.X = cameraLookAt.X - cameraPosition.X;
        Mario.Gamepad.CameraNormal.Y = cameraLookAt.Y - cameraPosition.Y;
        
        if (IsOddFrame)
        {
            Mario.Tick();
        } 
        IsOddFrame = !IsOddFrame;
        
        // Log.Info($"Pos: {Mario.Position.X} {Mario.Position.Y} {Mario.Position.Z}");
        // Log.Info($"Vel: {Mario.Velocity.X} {Mario.Velocity.Y} {Mario.Velocity.Z}");
    }

    // public override void GetCameraTarget(out Vector3 cameraLookAt, out Vector3 cameraPosition, out bool snapRequested)
    // {
    //     cameraLookAt = Position;
    //     cameraPosition = Position + new Vec3(10, 10, 10);
    //     snapRequested = false;
    // }

    // public override void Kill()
    // {
    //     // no.
    // }

    public override void CollectModels(List<(Actor Actor, Model Model)> populate)
    {
        populate.Add((this, MarioPlayerModel));
    }
}