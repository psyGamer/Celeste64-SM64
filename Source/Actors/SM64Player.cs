using System.Runtime.InteropServices;
using FMOD;
using LibSM64;
using LibSM64.Util;
using LibSM64Sharp.LowLevel;
using static LibSM64.Util.Native;

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
        private readonly Mario mario;
        private readonly Texture marioTexture;
        
        private readonly DefaultMaterial material = new();
        
        public MarioModel(Mario mario, Texture marioTexture)
        {
            this.mario = mario;
            this.marioTexture = marioTexture;
            
            material.SetShader(Assets.Shaders["Mario"]);
            if (material.Shader?.Has("u_color") ?? false)
                material.Set("u_color", material.Color);
            if (material.Shader?.Has("u_effects") ?? false)
                material.Set("u_effects", material.Effects);
            
            // Mario is animated through updated vertices from libsm64
            if (material.Shader != null && material.Shader.Has("u_jointMult"))
                material.Set("u_jointMult", 0.0f);

            Flags = ModelFlags.Default;
        }
        
        public override void Render(ref RenderState state)
        {
            state.ApplyToMaterial(material, Matrix.Identity);
        
            material.Texture = marioTexture;
            material.Model = Matrix.CreateTranslation(-mario.Position.ToVec3()) * Matrix.CreateScale(SM64_To_C64) * Matrix.CreateTranslation(mario.Position.ToVec3() * SM64_To_C64);
            material.MVP = material.Model * state.Camera.ViewProjection;
            
            var call = new DrawCommand(state.Camera.Target, mario.Mesh.Mesh, material)
            {
                DepthCompare = state.DepthCompare,
                DepthMask = state.DepthMask,
                CullMode = CullMode.None,
                MeshIndexStart = 0,
                MeshIndexCount = mario.Mesh.TriangleCount * 3,
            };
            call.Submit();
            state.Calls++;
            state.Triangles += mario.Mesh.TriangleCount;
        }
    }
    
    // private ISm64Context Context = null!;
    private SM64Context Context = null!;
    private Mario? Mario = null;
    
    private MarioModel MarioPlayerModel = null!;
    
    private const int NumChannels = 2;
    private const int SampleRate = 32000;
    private const int AudioBufferSize = 544 * 2;
    private const int QueueSize = 16384;
    
    private static readonly short[] audioBuffer = new short[AudioBufferSize * NumChannels];
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
        
        var romBytes = File.ReadAllBytes("sm64.z64");
        Context = new SM64Context(romBytes);
        // Context = Sm64Context.InitFromRom(romBytes);
        
        var builder = new StaticCollisionMesh.Builder();

        // Bounding box of all solids combined
        float minX = 0, maxX = 0, minZ = 0, maxZ = 0;
        
        foreach (var solid in World.All<Solid>().Cast<Solid>())
        {
            var verts = solid.WorldVertices;

            foreach (var face in solid.WorldFaces)
            {
                // Triangulate the mesh
                for (int i = 0; i < face.VertexCount - 2; i ++)
                {
                    builder.AddTriangle(SM64SurfaceType.DEFAULT, SM64TerrainType.GRASS,
                        (verts[face.VertexStart + 0] * C64_To_SM64).ToSM64Vec3(),
                        (verts[face.VertexStart + 2 + i] * C64_To_SM64).ToSM64Vec3(),
                        (verts[face.VertexStart + 1 + i] * C64_To_SM64).ToSM64Vec3());
                }
            }
            
            minX = Math.Min(minX, solid.WorldBounds.Min.X);
            maxX = Math.Max(maxX, solid.WorldBounds.Max.X);
            minZ = Math.Min(minZ, solid.WorldBounds.Min.Y);
            maxZ = Math.Max(maxZ, solid.WorldBounds.Max.Y);
        }

        // Add death plane
        const int DeathPlaneInflate = 10;
        builder.AddQuad(SM64SurfaceType.DEATH_PLANE, SM64TerrainType.GRASS, 
            new SM64Vector3f(minX * C64_To_SM64 - DeathPlaneInflate, World.DeathPlane * C64_To_SM64, maxZ * C64_To_SM64 + DeathPlaneInflate),
            new SM64Vector3f(maxX * C64_To_SM64 + DeathPlaneInflate, World.DeathPlane * C64_To_SM64, maxZ * C64_To_SM64 + DeathPlaneInflate),
            new SM64Vector3f(minX * C64_To_SM64 - DeathPlaneInflate, World.DeathPlane * C64_To_SM64, minZ * C64_To_SM64 - DeathPlaneInflate),
            new SM64Vector3f(maxX * C64_To_SM64 + DeathPlaneInflate, World.DeathPlane * C64_To_SM64, minZ * C64_To_SM64 - DeathPlaneInflate));
        
        builder.Build();

        Mario = new Mario(Position.X * C64_To_SM64, Position.Z * C64_To_SM64, Position.Y * C64_To_SM64);
        
        // Initial tick to set everything up
        Mario.Tick();
        
        MarioPlayerModel = new MarioModel(Mario, Context.MarioTexture);
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
        
        SuperMario64Mod.Instance.OnUnloadedCleanup += Dispose;
    }
    
    private static unsafe RESULT LibSM64Playback(IntPtr _, IntPtr data, uint length)
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
        base.Destroyed();
        Dispose();
    }

    public override unsafe void Update()
    {
        base.Update();
        
        if (Mario == null)
            return;
        
        Mario.Gamepad.Stick.X = -Controls.Move.Value.X;
        Mario.Gamepad.Stick.Y = Controls.Move.Value.Y;
        Mario.Gamepad.AButtonDown = Controls.Jump.Down;
        Mario.Gamepad.BButtonDown = Controls.Dash.Down;
        Mario.Gamepad.ZButtonDown = Controls.Climb.Down;
        
        GetCameraTarget(out var cameraLookAt, out var cameraPosition, out _);
        Mario.Gamepad.CameraLook.X = cameraLookAt.X - cameraPosition.X;
        Mario.Gamepad.CameraLook.Y = cameraLookAt.Y - cameraPosition.Y;
        
        if (IsOddFrame)
        {
            Mario.Tick();
            
            fixed (short* pBuf = audioBuffer)
            {
                uint writtenSamples = LibSm64Interop.sm64_audio_tick(QueueSize, (uint)audioBuffer.Length, (IntPtr)pBuf);
                audioQueue.Enqueue(pBuf, writtenSamples * 2 * 2);
            }
        } 
        IsOddFrame = !IsOddFrame;
    }

    public override void CollectModels(List<(Actor Actor, Model Model)> populate)
    {
        populate.Add((this, MarioPlayerModel));
    }
    
    private bool disposed = false;
    private void Dispose()
    {
        if (disposed) return;
        disposed = true;
        
        Audio.Check(sound.release());
        
        Mario?.Dispose();
        Context.Dispose();
    }
}