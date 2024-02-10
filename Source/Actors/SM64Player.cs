using System.Runtime.InteropServices;
using LibSM64Sharp;
using LibSM64Sharp.Impl;
using LibSM64Sharp.LowLevel;

namespace Celeste64;

public class SM64Player : Player
{
    /// <summary>
    /// A single unit in SM64 and C64 are different sizes.
    /// This constant transforms a SM64 unit into a C64 one.
    /// </summary>
    private const float UnitScaleFactor = 0.1f;
    
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
    
    private ISm64Context Context;
    private ISm64Mario Mario;
    
    private MarioModel Model;
    
    /// <summary>
    /// SM64 runs at 30FPS but C64 at 60FPS, so we need to skip every odd frame.
    /// </summary>
    private bool IsOddFrame = false;
    
    public override void Added()
    {
        base.Added();
        
        var romBytes = File.ReadAllBytes("sm64.z64");
        
        Context = Sm64Context.InitFromRom(romBytes);
        
        // LibSm64Interop.sm64_static_surfaces_load(Data.surfaces, (ulong)Data.surfaces.Length);
        // // int marioId = LibSm64Interop.sm64_mario_create(0, 1000, 0);
        // Mario = Context.CreateMario(0, 1000, 0);
        
        const float Size = 1000.0f;
        const float ZOffset = -1.0f; 
        var builder = Context.CreateStaticCollisionMesh();
        builder.AddQuad(Sm64SurfaceType.SURFACE_DEFAULT, Sm64TerrainType.TERRAIN_GRASS, 
            ((int x, int y, int z))(Position.X / UnitScaleFactor - Size, Position.Z / UnitScaleFactor + ZOffset*2, Position.Y / UnitScaleFactor + Size),
            ((int x, int y, int z))(Position.X / UnitScaleFactor + Size, Position.Z / UnitScaleFactor + ZOffset*2, Position.Y / UnitScaleFactor + Size),
            ((int x, int y, int z))(Position.X / UnitScaleFactor - Size, Position.Z / UnitScaleFactor + ZOffset*2, Position.Y / UnitScaleFactor - Size),
            ((int x, int y, int z))(Position.X / UnitScaleFactor + Size, Position.Z / UnitScaleFactor + ZOffset*2, Position.Y / UnitScaleFactor - Size));
        builder.Build();

        Mario = Context.CreateMario(Position.X / UnitScaleFactor, Position.Z / UnitScaleFactor, Position.Y / UnitScaleFactor);
        
        // Initial tick to set everything up
        Mario.Tick();
        
        Model = new MarioModel(Mario);
        Model.Flags |= ModelFlags.Silhouette; 
        Log.Info($"Mario ID: {Mario}");
    }

    public override void Destroyed()
    {
        base.Destroyed();

        Mario.Dispose();
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
            Mario.Tick();
        IsOddFrame = !IsOddFrame;
        
        // Log.Info($"Pos: {Mario.Position.X} {Mario.Position.Y} {Mario.Position.Z}");
        // Log.Info($"Vel: {Mario.Velocity.X} {Mario.Velocity.Y} {Mario.Velocity.Z}");

        Position = Position with
        {
            X = Mario.Position.X * UnitScaleFactor,
            Y = Mario.Position.Z * UnitScaleFactor,

            Z = Mario.Position.Y * UnitScaleFactor,
        };
    }
    
    public override void Kill()
    {
        // no.
    }

    public override void CollectModels(List<(Actor Actor, Model Model)> populate)
    {
        populate.Add((this, Model));
    }
}