using LibSM64;
using static LibSM64.Native;

namespace Celeste64.Mod.SuperMario64;

public static class SM64VectorExtensions
{
    // !! IMPORTANT !! In SM64 Y and Z are flipped compared to C64.
    
    public static Vec2 AsVec2(this SM64Vector2f vec) => new(vec.x, vec.y);
    public static Vec3 AsVec3(this SM64Vector3f vec) => new(vec.x, vec.z, vec.y);
    
    public static Vec2 ToC64Vec2(this SM64Vector2f vec) => new(vec.x * SM64Player.SM64_To_C64_Pos, vec.y * SM64Player.SM64_To_C64_Pos);
    public static Vec3 ToC64Vec3(this SM64Vector3f vec) => new(vec.x * SM64Player.SM64_To_C64_Pos, vec.z * SM64Player.SM64_To_C64_Pos, vec.y * SM64Player.SM64_To_C64_Pos); 
    
    public static SM64Vector2f ToSM64Vec2(this Vec2 vec) => new(vec.X * SM64Player.C64_To_SM64_Pos, vec.Y * SM64Player.C64_To_SM64_Pos);
    public static SM64Vector3f ToSM64Vec3(this Vec3 vec) => new(vec.X * SM64Player.C64_To_SM64_Pos, vec.Z * SM64Player.C64_To_SM64_Pos, vec.Y * SM64Player.C64_To_SM64_Pos);
    
    public static SM64Vector2f ToSM64VelocityVec2(this Vec2 vec) => new(vec.X * SM64Player.C64_To_SM64_Vel, vec.Y * SM64Player.C64_To_SM64_Vel);
    public static SM64Vector3f ToSM64VelocityVec3(this Vec3 vec) => new(vec.X * SM64Player.C64_To_SM64_Vel, vec.Z * SM64Player.C64_To_SM64_Vel, vec.Y * SM64Player.C64_To_SM64_Vel);
}

public class SM64Player : Player
{
    /// <summary>
    /// A single unit in SM64 and C64 are different sizes.
    /// These constants transform a SM64 unit into a C64 one or vice versa.
    /// </summary>
    public const float SM64_To_C64_Pos = 0.075f;
    public const float C64_To_SM64_Pos = 1.0f / SM64_To_C64_Pos;
    
    // C64 uses px/s, SM64 uses px/f (30 FPS)
    public const float SM64_To_C64_Vel = SM64_To_C64_Pos * 30.0f;
    public const float C64_To_SM64_Vel = C64_To_SM64_Pos / 30.0f;
    
    private class MarioModel : Model
    {
        private readonly Mario mario;
        
        private readonly DefaultMaterial material = new();
        
        public MarioModel(Mario mario)
        {
            this.mario = mario;
            
            material.SetShader(Assets.Shaders["Mario"]);
            if (material.Shader?.Has("u_color") ?? false)
                material.Set("u_color", material.Color);
            if (material.Shader?.Has("u_effects") ?? false)
                material.Set("u_effects", material.Effects);
            
            // Mario is animated through updated vertices from libsm64
            if (material.Shader != null && material.Shader.Has("u_jointMult"))
                material.Set("u_jointMult", 0.0f);
            
            // Use linear filtering, like SM64
            if (material.Shader != null && material.Shader.Has("u_texture_sampler"))
                material.Set("u_texture_sampler", new TextureSampler(TextureFilter.Linear, TextureWrap.ClampToEdge, TextureWrap.ClampToEdge));
            
            Flags = ModelFlags.Default;
        }
        
        public override void Render(ref RenderState state)
        {
            state.ApplyToMaterial(material, Matrix.Identity);
        
            material.Texture = SM64Context.MarioTexture;
            
            material.Model = Matrix.CreateTranslation(-mario.Position.AsVec3()) * Matrix.CreateScale(SM64_To_C64_Pos) * Matrix.CreateTranslation(mario.Position.ToC64Vec3());
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
    
    private Mario Mario = null!;
    private MarioModel MarioPlayerModel = null!;
    
    /// <summary>
    /// SM64 runs at 30FPS but C64 at 60FPS, so we need to skip every odd frame.
    /// </summary>
    private bool IsOddFrame = false;

    // Mario is technically null, until the player is Added
    public override Vec3 Position
    {
        get => Mario != null ? Mario.Position.ToC64Vec3() : position;
        set
        {
            if (position == value) 
                return;

            position = value;
            dirty = true;
                
            if (Mario != null)
                Mario.Position = value.ToSM64Vec3();
        }
    }

    public override Vec3 Velocity => Mario != null ? Mario.Velocity.ToC64Vec3() : velocity;

    public override Vec2 Facing
    {
        get => Calc.AngleToVector(Mario.FaceAngle);
        set
        {
            if (facing == value)
                return;

            facing = value;
            dirty = true;
            Mario.FaceAngle = value.Angle();
        }
    }

    public override void Added()
    {
        Mario = new Mario(Position.X * C64_To_SM64_Pos, Position.Z * C64_To_SM64_Pos, Position.Y * C64_To_SM64_Pos);
        
        // Initial tick to set everything up
        Mario.Tick();
        
        MarioPlayerModel = new MarioModel(Mario);
        MarioPlayerModel.Flags |= ModelFlags.Silhouette; 
        
        // Setup camera
        CameraOriginPos = Position;
        GetCameraTarget(out var orig, out var target, out _);
        World.Camera.LookAt = target;
        World.Camera.Position = orig;
    }
    
    public override void Destroyed()
    {
        base.Destroyed();
        Dispose();
    }
    
    public static void GenerateSolids(World world)
    {
        var builder = new StaticCollisionMesh.Builder();

        // Bounding box of all solids combined
        float minX = 0, maxX = 0, minZ = 0, maxZ = 0;
        
        // Gather all solids, even if they aren't part of the World yet
        var solids = world.All<Solid>().Cast<Solid>()
                    .Concat(world.adding.OfType<Solid>());
        
        foreach (var solid in solids)
        {
            var verts = solid.WorldVertices;

            foreach (var face in solid.WorldFaces)
            {
                // Triangulate the mesh
                for (int i = 0; i < face.VertexCount - 2; i ++)
                {
                    builder.AddTriangle(SM64SurfaceType.DEFAULT, SM64TerrainType.GRASS,
                        verts[face.VertexStart + 0].ToSM64Vec3(),
                        verts[face.VertexStart + 2 + i].ToSM64Vec3(),
                        verts[face.VertexStart + 1 + i].ToSM64Vec3());
                }
            }
            
            minX = Math.Min(minX, solid.WorldBounds.Min.X);
            maxX = Math.Max(maxX, solid.WorldBounds.Max.X);
            minZ = Math.Min(minZ, solid.WorldBounds.Min.Y);
            maxZ = Math.Max(maxZ, solid.WorldBounds.Max.Y);
        }

        // Add death plane
        const int DeathPlaneInflate = (int)(10 * C64_To_SM64_Pos);
        const int DeathPlaneOffset = (int)(100 * C64_To_SM64_Pos);
        builder.AddQuad(SM64SurfaceType.DEATH_PLANE, SM64TerrainType.GRASS, 
            new SM64Vector3f(minX * C64_To_SM64_Pos - DeathPlaneInflate, world.DeathPlane * C64_To_SM64_Pos - DeathPlaneOffset, maxZ * C64_To_SM64_Pos + DeathPlaneInflate),
            new SM64Vector3f(maxX * C64_To_SM64_Pos + DeathPlaneInflate, world.DeathPlane * C64_To_SM64_Pos - DeathPlaneOffset, maxZ * C64_To_SM64_Pos + DeathPlaneInflate),
            new SM64Vector3f(minX * C64_To_SM64_Pos - DeathPlaneInflate, world.DeathPlane * C64_To_SM64_Pos - DeathPlaneOffset, minZ * C64_To_SM64_Pos - DeathPlaneInflate),
            new SM64Vector3f(maxX * C64_To_SM64_Pos + DeathPlaneInflate, world.DeathPlane * C64_To_SM64_Pos - DeathPlaneOffset, minZ * C64_To_SM64_Pos - DeathPlaneInflate));
        
        builder.Build();
    }
    
    private bool inCutscene = false;

    public override void Update()
    {
        bool cutsceneActive = World.All<Cutscene>().Count() != 0;
        if (cutsceneActive && !inCutscene)
        {
            // Start cutscene
            Mario.Action = SM64Action.WAITING_FOR_DIALOG;
        }
        else if (!cutsceneActive && inCutscene)
        {
            // End cutscene
            Mario.Action = SM64Action.IDLE;
        }
        
        Mario.Gamepad.Stick.X = -Controls.Move.Value.X;
        Mario.Gamepad.Stick.Y = Controls.Move.Value.Y;
        Mario.Gamepad.AButtonDown = Controls.Jump.Down;
        Mario.Gamepad.BButtonDown = Controls.Dash.Down;
        Mario.Gamepad.ZButtonDown = Controls.Climb.Down;
        
        // Rotate Camera
        {
            var invertX = Save.Instance.InvertCamera == Save.InvertCameraOptions.X || Save.Instance.InvertCamera == Save.InvertCameraOptions.Both;
            var rot = new Vec2(CameraTargetForward.X, CameraTargetForward.Y).Angle();
            rot -= Controls.Camera.Value.X * Time.Delta * 4 * (invertX ? -1 : 1);

            var angle = Calc.AngleToVector(rot);
            CameraTargetForward = new(angle, 0);
        }

        // Move Camera in / out
        if (Controls.Camera.Value.Y != 0)
        {
            var invertY = Save.Instance.InvertCamera == Save.InvertCameraOptions.Y || Save.Instance.InvertCamera == Save.InvertCameraOptions.Both;
            CameraTargetDistance += Controls.Camera.Value.Y * Time.Delta * (invertY ? -1 : 1);
            CameraTargetDistance = Calc.Clamp(CameraTargetDistance, 0, 1);
        }
        else
        {
            const float interval = 1f / 3;
            const float threshold = .1f;
            if (CameraTargetDistance % interval < threshold || CameraTargetDistance % interval > interval - threshold)
                Calc.Approach(ref CameraTargetDistance, Calc.Snap(CameraTargetDistance, interval), Time.Delta / 2);
        }
        
        GetCameraTarget(out var cameraLookAt, out var cameraPosition, out _);
        Mario.Gamepad.CameraLook.X = cameraLookAt.X - cameraPosition.X;
        Mario.Gamepad.CameraLook.Y = cameraLookAt.Y - cameraPosition.Y;

        // Check for NPC interaction
        if (Mario.ReadyToSpeak)
        {
            foreach (var actor in World.All<NPC>())
            {
                if (actor is NPC { InteractEnabled: true } npc)
                {
                    if ((Position - npc.Position).LengthSquared() < npc.InteractRadius * npc.InteractRadius &&
                        Vec2.Dot((npc.Position - Position).XY(), TargetFacing) > 0 &&
                        MathF.Abs(npc.Position.Z - Position.Z) < 2)
                    {
                        npc.IsPlayerOver = true;

                        if (Controls.Dash.ConsumePress())
                        {
                            npc.Interact(this);
                        }

                        break;
                    }
                }
            }
        }
        
        // Check for pickups
        if (IsAbleToPickup)
        {
            foreach (var actor in World.All<IPickup>())
            {
                if (actor is IPickup pickup)
                {
                    if ((SolidWaistTestPos - actor.Position).LengthSquared() < pickup.PickupRadius * pickup.PickupRadius)
                    {
                        pickup.Pickup(this);
                        ModManager.Instance.OnItemPickup(this, pickup);
                    }
                }
            }
        }
        
        // Only update specific states
        if (StateMachine.State == States.Cassette)
        {
            StateMachine.Update();
        }
        
        if (IsOddFrame)
        {
            if (StateMachine.State != States.Cassette)
                Mario.Tick();
            AudioPlayer.ShouldTick = true;
        } 
        IsOddFrame = !IsOddFrame;
        
        // Death plane
        if (Position.Z < World.DeathPlane)
            Kill();
        
        // Respawning
        if (Dead && !Game.Instance.IsMidTransition)
        {
            SM64Context.PlaySoundGlobal(SM64Sound.MENU_BOWSER_LAUGH);
            
            var entry = World.Entry with { Reason = World.EntryReasons.Respawned };
            Game.Instance.Goto(new Transition()
            {
                Mode = Transition.Modes.Replace,
                Scene = () => new World(entry),
                ToBlack = new BowserWipe(),
                FromBlack = new AngledWipe(),
                HoldOnBlackFor = 1.0f,
            });
        }
    }

    public override void LateUpdate()
    {
        // Update camera origin position
        {
            float ZPad = StateMachine.State == States.Climbing ? 0 : 8;
            CameraOriginPos.X = Position.X;
            CameraOriginPos.Y = Position.Y;

            float targetZ;
            if (OnGround)
                targetZ = Position.Z;
            else if (Position.Z < CameraOriginPos.Z)
                targetZ = Position.Z;
            else if (Position.Z > CameraOriginPos.Z + ZPad)
                targetZ = Position.Z - ZPad;
            else
                targetZ = CameraOriginPos.Z;

            if (CameraOriginPos.Z != targetZ)
                CameraOriginPos.Z += (targetZ - CameraOriginPos.Z) * (1 - MathF.Pow(.001f, Time.Delta));
        }

        // Update camera position
        {
            Vec3 lookAt, cameraPos;

            if (CameraOverride.HasValue)
            {
                lookAt = CameraOverride.Value.LookAt;
                cameraPos = CameraOverride.Value.Position;
            }
            else
            {
                GetCameraTarget(out lookAt, out cameraPos, out _);
            }

            World.Camera.Position += (cameraPos - World.Camera.Position) * (1 - MathF.Pow(0.01f, Time.Delta));
            World.Camera.LookAt = lookAt;

            float targetFOV = Calc.ClampedMap(velocity.XY().Length(), MaxSpeed * 1.2f, 120, 1, 1.2f);

            World.Camera.FOVMultiplier = Calc.Approach(World.Camera.FOVMultiplier, targetFOV, Time.Delta / 4);
        }
    }

    public override void Spring(Spring spring)
    {
        Mario.Action = SM64Action.TWIRLING;
        Mario.Velocity = Mario.Velocity with { y = SpringJumpSpeed * C64_To_SM64_Vel };
    }

    public override void StCassetteExit()
    {
        base.StCassetteExit();
        // Mario.Action = SM64Action.FREEFALL;
        // Mario.Velocity = velocity.ToSM64VelocityVec3();
        // Mario.ForwardVelocity = 0.0f;
    }

    public override void Kill()
    {
        Mario.Kill();
        
        Dead = true;
        Save.CurrentRecord.Deaths++;
    }
    
    public override void SetTargetFacing(Vector2 facing) => Facing = facing;
    public override void Stop()
    {
        Mario.Action = SM64Action.FREEFALL;
        Mario.Velocity = new SM64Vector3f(0.0f, 0.0f, 0.0f);
        Mario.ForwardVelocity = 0.0f;
    }

    public override void ValidateTransformations()
    {
        // We don't know if something changed, so we always update
        matrix = Matrix.CreateTranslation(Position);
        worldBounds = BoundingBox.Transform(localBounds, matrix);
        forward = Vec3.TransformNormal(-Vec3.UnitY, matrix);

        Transformed();
    }

    public override void CollectModels(List<(Actor Actor, Model Model)> populate)
    {
        if (StateMachine.State != States.Cassette)
            populate.Add((this, MarioPlayerModel));
    }
    
    private bool disposed = false;
    private void Dispose()
    {
        if (disposed) return;
        disposed = true;
        
        Mario?.Dispose();
    }
}