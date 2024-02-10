using System.Runtime.InteropServices;
using FMOD;
using LibSM64Sharp;
using LibSM64Sharp.Impl;
using LibSM64Sharp.LowLevel;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Thread = System.Threading.Thread;

namespace Celeste64.Mod.SuperMario64;

public class SM64Player : Player
{
    /// <summary>
    /// A single unit in SM64 and C64 are different sizes.
    /// This constant transforms a SM64 unit into a C64 one.
    /// </summary>
    private const float UnitScaleFactor = 0.075f;
    
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
            
            queue.Clear();
            
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
    
    private static Thread? threadHandle;
    private static bool runThread;
    
    internal static void Load()
    {
        // il_Player_LateUpdate = new ILHook(typeof(Player).GetMethod("LateUpdate")!, IL_Player_LateUpdate);
        runThread = true;
        threadHandle = new Thread(CaptureThread);
        threadHandle.Start();
    }
    internal static void Unload()
    {
        // il_Player_LateUpdate?.Dispose();
        runThread = false;
        threadHandle?.Join();
        threadHandle = null;

    }
    
    private static unsafe void CaptureThread()
    {
        while (runThread)
        {
            if (instance is not { } p || !p.active) continue;

            var buffer = new short[544*2*2];
            fixed (short* pBuf = buffer)
            {
                if (queue.Count < 12000)
                {
                    uint writtenSamples = LibSm64Interop.sm64_audio_tick((uint)queue.Count, (uint)buffer.Length, (IntPtr)pBuf);
                    for (uint i = 0; i < writtenSamples*2*2; i += 1)
                    {
                        queue.Enqueue(buffer[i]);
                    }
                }
            }
            
            Thread.Sleep(33);
        }
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

    private static SM64Player? instance = null;
    
    private bool active;
    
    private ISm64Context Context = null!;
    private ISm64Mario? Mario = null;
    
    private MarioModel MarioPlayerModel = null!;
    
    private const int NumChannels = 2;
    private const int AudioBufferSize = 512 * NumChannels;
    private const int SampleRate = 32000;
    private byte[] AudioBuffer = new byte[544*2*2];
    
    /// <summary>
    /// SM64 runs at 30FPS but C64 at 60FPS, so we need to skip every odd frame.
    /// </summary>
    private bool IsOddFrame = false;

    public override Vec3 Position => Mario != null ? Mario.Position.ToVec3() * UnitScaleFactor : position;
    public override Vec3 Velocity => Mario != null ? Mario.Velocity.ToVec3() * UnitScaleFactor : velocity;

    private FMOD.Sound sn;
    private Channel ch;
    
    public override void Added()
    {
        base.Added();
        instance = this;
        
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
        
        // unsafe
        // {
        //     fixed (byte* pBuffer = AudioBuffer)
        //     {
        //         generatePCMData((short*)pBuffer, NumChannels);
        //     }
        // }
        
        Log.Info("A");
        Audio.Check(Audio.system.getCoreSystem(out var coreSystem));
        CREATESOUNDEXINFO exinfo = default;
        exinfo.cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO));
        exinfo.numchannels = NumChannels;
        exinfo.decodebuffersize = SampleRate/8;//AudioBufferSize;
        exinfo.decodebuffersize = (uint)(544*2*Marshal.SizeOf<short>());
        exinfo.format = SOUND_FORMAT.PCM16;
        exinfo.defaultfrequency = SampleRate;
        exinfo.pcmreadcallback = pcmreadcallback;
        exinfo.length = (uint)(SampleRate * NumChannels * Marshal.SizeOf<short>());
        exinfo.length = (uint)(544*2*2*Marshal.SizeOf<short>());
        
        Log.Info("B");
        Audio.Check(coreSystem.createStream("haiii", MODE.OPENUSER | MODE.LOOP_NORMAL, ref exinfo, out sn));
        // coreSystem.getMasterChannelGroup(out var channelGroup);
        // coreSystem.setStreamBufferSize(65536, TIMEUNIT.RAWBYTES);
        
        Log.Info("C");
        Audio.Check(coreSystem.playSound(sn, new ChannelGroup(0), false, out ch));
        
        active = true;
    }
    
    // Function to generate PCM data
    private static unsafe void generatePCMData(short *buffer, int numSamples) {
        for (int i = 0; i < numSamples; i++) {
            // Generate some audio data (e.g., sine wave)
            float sample = 32767.0f * MathF.Sin(2.0f * 3.14159265f * 440.0f * i / SampleRate);
            buffer[i * NumChannels] = (short)sample; // Left channel
            buffer[i * NumChannels + 1] = (short)sample; // Right channel (for stereo)
        }
    }
    
    static Queue<short> queue = new();
    
    static int samplesElapsed = 0;
    static float  t1 = 0, t2 = 0;        // time
    static float  v1 = 0, v2 = 0;        // velocity
    private static unsafe RESULT pcmreadcallback(IntPtr sound, IntPtr data, uint length)
    {
        //Log.Info($"CALLBACK: {(nint)sound} {(nint)data} {length} | Queue: {queue.Count}");

        // if (instance is not { } p || !p.active || length < 544*2*2*Marshal.SizeOf<short>()) return RESULT.OK;
        
        // LibSm64Interop.sm64_audio_tick((uint)(544*2*2*Marshal.SizeOf<short>()-length), length, data);
        
        // short *stereo16bitbuffer = (short*)data;

        // for (uint count = 0; count < (datalen >> 2); count++)     // >>2 = 16bit stereo (4 bytes per sample)
        // {
        //     *stereo16bitbuffer++ = (short)(MathF.Sin(t1) * 32767.0f);    // left channel
        //     *stereo16bitbuffer++ = (short)(MathF.Sin(t2) * 32767.0f);    // right channel
        //
        //     t1 += 0.01f   + v1;
        //     t2 += 0.0142f + v2;
        //     v1 += (float)(MathF.Sin(t1) * 0.002f);
        //     v2 += (float)(MathF.Sin(t2) * 0.002f);
        // }
	
        // A 2-channel 16-bit stereo stream uses 4 bytes per sample
        // for (uint sample = 0; sample < length / 4; sample++)
        // {
        //     // Get the position in the sample
        //     double pos = (float)(800 * samplesElapsed) / SampleRate;
        //
        //     // The generator function returns a value from -1 to 1 so we multiply this by the
        //     // maximum possible volume of a 16-bit PCM sample (32767) to get the true volume to store
        //
        //     // Generate a sample for the left channel
        //     *stereo16bitbuffer++ = (short)(Math.Sin(pos * Math.PI*2) * 32767.0f * 0.3f);
        //
        //     // Generate a sample for the right channel
        //     *stereo16bitbuffer++ = (short)(Math.Sin(pos * Math.PI*2) * 32767.0f * 0.3f);
        //
        //     // Increment number of samples generated
        //     samplesElapsed++;
        // }
        
        // uint position = 0;
        // if (instance is { } p)
        //     p.ch.getPosition(out position, TIMEUNIT.PCM);
        // Log.Info($"New: {position}");
        // if (instance is { } p2)
        //     p2.ch.getPosition(out position, TIMEUNIT.MS);
        // Log.Info($"New:MS {position}");
        
        // if (instance is {} p)
        // {
        //     NativeMemory.Fill((void*)data, length, 0);
        //     
        //     var buffer = new short[544*2*2];
        //     fixed (short* pBuf = buffer)
        //     {
        //         
        //         if (queue.Count < 1000)
        //         {
        //         uint writtenSamples = LibSm64Interop.sm64_audio_tick((uint)queue.Count, (uint)buffer.Length, (IntPtr)pBuf);
        //             for (uint i = 0; i < writtenSamples*2*2; i += 1)
        //             {
        //                 queue.Enqueue(buffer[i]);
        //             }
        //         }
        //     } 
        // }
        
        for (int i = 0; i < length; i += Marshal.SizeOf<short>())
        {
            if (!queue.TryDequeue(out short sample))
                sample = 0;
            *(short*)(data + i) = sample;
        }
        
        //
        // NativeMemory.Fill((void*)data, datalen, 0);
        // LibSm64Interop.sm64_audio_tick(0, datalen, data);

        return RESULT.OK;
    }


    public override void Destroyed()
    {
        active = false;
        
        base.Destroyed();
        
        Log.Info("DESTROYED!!!");

        Audio.Check(sn.release());
        // Audio.Check(ch.stop());
        
        Mario?.Dispose();
        Context.Dispose();
        
        if (instance == this)
            instance = null;
    }

    public override void Update()
    {
        base.Update();
        
        Audio.Check(ch.getPosition(out uint fpos, TIMEUNIT.MS));
        Audio.Check(sn.getLength(out uint flen, TIMEUNIT.MS));
        // Log.Info($"Channel: {fpos} / {flen} | ({ch.handle}) ({sn.handle})");
        
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