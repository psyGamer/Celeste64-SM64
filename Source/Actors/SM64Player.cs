using System.Runtime.InteropServices;
using FMOD;
using LibSM64Sharp;
using LibSM64Sharp.Impl;
using LibSM64Sharp.LowLevel;
using SuperMario64;
using Thread = System.Threading.Thread;

namespace Celeste64.Mod.SuperMario64;

public class SM64Player : Player
{
    /// <summary>
    /// A single unit in SM64 and C64 are different sizes.
    /// These constants transform a SM64 unit into a C64 one or vice versa.
    /// </summary>
    private const float SM64_To_C64 = 0.075f;
    private const float C64_To_SM64 = 1.0f / SM64_To_C64;
    
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
            
            audioQueue.Clear();
        }
        
        public override void Render(ref RenderState state)
        {
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
            
            state.ApplyToMaterial(Material, Matrix.Identity);
        
            Material.Texture = Mario.Mesh.Texture;
            Material.Model = Matrix.CreateTranslation(-Mario.Position.ToVec3()) * Matrix.CreateScale(SM64_To_C64) * Matrix.CreateTranslation(Mario.Position.ToVec3() * SM64_To_C64);
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
    
    private static Thread? threadHandle;
    private static bool runThread;
    
    internal static void Load()
    {
        runThread = true;
        threadHandle = new Thread(CaptureThread);
        threadHandle.Start();
    }
    internal static void Unload()
    {
        runThread = false;
        threadHandle?.Join();
        threadHandle = null;
    }
    
    private static unsafe void CaptureThread()
    {
        var buffer = new short[AudioBufferSize * NumChannels];

        while (runThread)
        {
            if (instance is not { } p || !p.active) continue;

            fixed (short* pBuf = buffer)
            {
                uint writtenSamples = LibSm64Interop.sm64_audio_tick(QueueSize, (uint)buffer.Length, (IntPtr)pBuf);
                audioQueue.Enqueue(pBuf, writtenSamples * 2 * 2);
            }
            
            Thread.Sleep(33);
        }
    }
    
    private static SM64Player? instance = null;
    
    private bool active;
    
    private ISm64Context Context = null!;
    private ISm64Mario? Mario = null;
    
    private MarioModel MarioPlayerModel = null!;
    
    private const int NumChannels = 2;
    private const int SampleRate = 32000;
    private const int AudioBufferSize = 544 * 2;
    private const int QueueSize = 16384;
    
    private static readonly CircularQueue<short> audioQueue = new(QueueSize);
    
    /// <summary>
    /// SM64 runs at 30FPS but C64 at 60FPS, so we need to skip every odd frame.
    /// </summary>
    private bool IsOddFrame = false;

    public override Vec3 Position => Mario != null ? Mario.Position.ToVec3() * SM64_To_C64 : position;
    public override Vec3 Velocity => Mario != null ? Mario.Velocity.ToVec3() * SM64_To_C64 : velocity;

    private FMOD.Sound sound;

    public override void Added()
    {
        base.Added();
        instance = this;
        
        var romBytes = File.ReadAllBytes("sm64.z64");
        
        Context = Sm64Context.InitFromRom(romBytes);
        
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
                int x = (int)(vec.X * C64_To_SM64);
                // !! IMPORTANT !! In SM64 Y and Z are flipped compared to C64.
                int z = (int)(vec.Y * C64_To_SM64);
                int y = (int)(vec.Z * C64_To_SM64);
                
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
            ((int x, int y, int z))(minX - DeathPlaneInflate, World.DeathPlane * C64_To_SM64, maxZ + DeathPlaneInflate),
            ((int x, int y, int z))(maxX + DeathPlaneInflate, World.DeathPlane * C64_To_SM64, maxZ + DeathPlaneInflate),
            ((int x, int y, int z))(minX - DeathPlaneInflate, World.DeathPlane * C64_To_SM64, minZ - DeathPlaneInflate),
            ((int x, int y, int z))(maxX + DeathPlaneInflate, World.DeathPlane * C64_To_SM64, minZ - DeathPlaneInflate));
        
        builder.Build();

        Mario = Context.CreateMario(Position.X * C64_To_SM64, Position.Z * C64_To_SM64, Position.Y * C64_To_SM64);
        
        // Initial tick to set everything up
        Mario.Tick();
        
        MarioPlayerModel = new MarioModel(Mario);
        MarioPlayerModel.Flags |= ModelFlags.Silhouette; 
        
        // Create FMOD audio stream to play back libsm64 data
        Audio.Check(Audio.system.getCoreSystem(out var coreSystem));
        CREATESOUNDEXINFO exinfo = default;
        exinfo.cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO));
        exinfo.numchannels = NumChannels;
        exinfo.decodebuffersize = (uint)(AudioBufferSize * Marshal.SizeOf<short>());
        exinfo.format = SOUND_FORMAT.PCM16;
        exinfo.defaultfrequency = SampleRate;
        exinfo.pcmreadcallback = LibSM64Playback;
        exinfo.length = (uint)(SampleRate * NumChannels * Marshal.SizeOf<short>());
        exinfo.length = (uint)(AudioBufferSize * NumChannels * Marshal.SizeOf<short>());
        
        Audio.Check(coreSystem.createStream("libsm64 playback", MODE.OPENUSER | MODE.LOOP_NORMAL, ref exinfo, out sound));
        Audio.Check(coreSystem.playSound(sound, new ChannelGroup(0), false, out _));
        
        active = true;
    }
    
    private static unsafe RESULT LibSM64Playback(IntPtr sound, IntPtr data, uint length)
    {
        var len = Math.Min((int)(length / Marshal.SizeOf<short>()), audioQueue.Size);
        audioQueue.Dequeue((short*)data, (uint)len);
        
        // Fill reset with 0
        var filled = len * Marshal.SizeOf<short>();
        NativeMemory.Fill((void*)(data + filled), (UIntPtr)(length - filled), 0);
        
        return RESULT.OK;
    }

    public override void Destroyed()
    {
        active = false;
        
        base.Destroyed();

        Audio.Check(sound.release());
        
        Mario?.Dispose();
        Context.Dispose();
        
        if (instance == this)
            instance = null;
    }

    public override void Update()
    {
        base.Update();
        
        if (Mario == null)
            return;
        
        Mario.Gamepad.AnalogStick.X = -Controls.Move.Value.X;
        Mario.Gamepad.AnalogStick.Y = Controls.Move.Value.Y;
        Mario.Gamepad.IsAButtonDown = Controls.Jump.Down;
        Mario.Gamepad.IsBButtonDown = Controls.Dash.Down;
        Mario.Gamepad.IsZButtonDown = Controls.Climb.Down;
        
        GetCameraTarget(out var cameraLookAt, out var cameraPosition, out _);
        Mario.Gamepad.CameraNormal.X = cameraLookAt.X - cameraPosition.X;
        Mario.Gamepad.CameraNormal.Y = cameraLookAt.Y - cameraPosition.Y;
        
        if (IsOddFrame)
        {
            Mario.Tick();
        } 
        IsOddFrame = !IsOddFrame;
    }

    public override void CollectModels(List<(Actor Actor, Model Model)> populate)
    {
        populate.Add((this, MarioPlayerModel));
    }
}