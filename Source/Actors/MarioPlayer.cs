using LibSM64;
using static LibSM64.Native;

namespace Celeste64.Mod.SuperMario64;

public static class SM64ConversionExtensions
{
    // C64 is left-handed with +Z up, SM64 is right-handed with +Y up
    public static Vec2 AsC64Vec2(this SM64Vector2f vec) => new(vec.x, vec.y);
    public static Vec3 AsC64Vec3(this SM64Vector3f vec) => new(vec.x, -vec.z, vec.y);
    public static SM64Vector2f AsSM64Vec2(this Vec2 vec) => new(vec.X, -vec.Y);
    public static SM64Vector3f AsSM64Vec3(this Vec3 vec) => new(vec.X, vec.Z, -vec.Y);
    
    public static Vec2 ToC64Vec2(this SM64Vector2f vec) => new(vec.x * MarioPlayer.SM64_To_C64, vec.y * MarioPlayer.SM64_To_C64);
    public static Vec3 ToC64Vec3(this SM64Vector3f vec) => new(vec.x * MarioPlayer.SM64_To_C64, -vec.z * MarioPlayer.SM64_To_C64, vec.y * MarioPlayer.SM64_To_C64); 
    
    public static SM64Vector2f ToSM64Vec2(this Vec2 vec) => new(vec.X * MarioPlayer.C64_To_SM64, vec.Y * MarioPlayer.C64_To_SM64);
    public static SM64Vector3f ToSM64Vec3(this Vec3 vec) => new(vec.X * MarioPlayer.C64_To_SM64, vec.Z * MarioPlayer.C64_To_SM64, -vec.Y * MarioPlayer.C64_To_SM64);
    
    // C64 uses px/s, SM64 uses px/f (30 FPS)
    public static Vec3 ToC64VelocityVec3(this SM64Vector3f vec) => new(vec.x * MarioPlayer.SM64_To_C64_Vel, -vec.z * MarioPlayer.SM64_To_C64_Vel, vec.y * MarioPlayer.SM64_To_C64_Vel);
    public static SM64Vector3f ToSM64VelocityVec3(this Vec3 vec) => new(vec.X * MarioPlayer.C64_To_SM64_Vel, vec.Z * MarioPlayer.C64_To_SM64_Vel, -vec.Y * MarioPlayer.C64_To_SM64_Vel);
    
    // :screwms: moment
    private static float Modulo(float a, float b)
    {
        return (a % b + b) % b;
    }
    
    // C64 rotates counter-clock-wise and +Y is 0, SM64 rotates clock-wise and +X is 0
    public static float ToC64Angle(this float angle) => Modulo(angle - 90.0f * Calc.DegToRad, 360.0f * Calc.DegToRad);
    public static float ToSM64Angle(this float angle) => Modulo(angle + 90.0f * Calc.DegToRad, 360.0f * Calc.DegToRad);
}

public class MarioPlayer : Player
{
    /// <summary>
    /// A single unit in SM64 and C64 are different sizes.
    /// These constants transform a SM64 unit into a C64 one or vice versa.
    /// </summary>
    public const float SM64_To_C64 = 0.075f;
    public const float C64_To_SM64 = 1.0f / SM64_To_C64;
    
    public const float SM64_To_C64_Vel = SM64_To_C64 * 30.0f;
    public const float C64_To_SM64_Vel = C64_To_SM64 / 30.0f;
    
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
            if (material.Shader?.Has("u_model_state") ?? false)
            {
                int modelState = 0;
                if (mario.ModelState == SM64ModelState.METAL)
                    modelState |= 1;
                if (mario.ModelState == SM64ModelState.NOISE_ALPHA)
                    modelState |= 2;
                material.Set("u_model_state", (float)modelState);
            }

            material.Model = Matrix.CreateTranslation(-mario.Position.AsC64Vec3()) *
                             Matrix.CreateScale(SM64_To_C64) *
                             Matrix.CreateTranslation(mario.Position.ToC64Vec3());
            material.MVP = material.Model * state.Camera.ViewProjection;
            
            var call = new DrawCommand(state.Camera.Target, mario.Mesh.Mesh, material)
            {
                BlendMode = new BlendMode(BlendOp.Add, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha),
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
        get => Calc.AngleToVector(Mario.FaceAngle.ToC64Angle());
        set
        {
            if (facing == value)
                return;

            facing = value;
            dirty = true;
            Mario.FaceAngle = value.Angle().ToSM64Angle();
        }
    }

    public override void Added()
    {
        Mario = new Mario(Position.ToSM64Vec3());
        
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
    
    private bool inCutscene = false;
    private bool startedStrawbDance = false;

    public override unsafe void Update()
    {
        bool cutsceneActive = World.All<Cutscene>().Count() != 0;
        if (cutsceneActive && !inCutscene)
        {
            // Start cutscene
            Mario.Action = SM64Action.WAITING_FOR_DIALOG;
            inCutscene = true;
        }
        else if (!cutsceneActive && inCutscene)
        {
            // End cutscene
            Mario.Action = SM64Action.IDLE;
            inCutscene = false;
        }
        
        // TODO: Probably remove these debug hotkeys lol
        if (Input.Keyboard.Pressed(Keys.P))
            Mario.InteractCap(SM64MarioFlags.WING_CAP);
        if (Input.Keyboard.Pressed(Keys.O))
            Mario.InteractCap(SM64MarioFlags.METAL_CAP);
        if (Input.Keyboard.Pressed(Keys.I))
            Mario.InteractCap(SM64MarioFlags.VANISH_CAP);
        
        if (Input.Keyboard.Pressed(Keys.K))
            SM64Context.PlayMusic(SM64SeqPlayer.LEVEL, SM64SeqId.LEVEL_BOSS_KOOPA_FINAL);
        if (Input.Keyboard.Pressed(Keys.J))
            SM64Context.PlaySound(SM64Sound.MARIO_SO_LONGA_BOWSER, Mario.Position);
        
        // Reimplemented check_kick_or_punch_wall() from libsm64 src/decomp/game/interaction.c
        // We don't set any state (since libsm64 already does that) but only check for collisions with breakable blocks
        if (Mario.Flags.Has(SM64MarioFlags.PUNCHING | SM64MarioFlags.KICKING | SM64MarioFlags.TRIPPING)) {
            var detector = new SM64Vector3f(
                Mario.Position.x + 50.0f * MathF.Sin(Mario.FaceAngle),
                Mario.Position.y,
                Mario.Position.z + 50.0f * MathF.Cos(Mario.FaceAngle));

            // Use velocity of Mario's hand for the impact
            var handVelocity = new SM64Vector3f(
                50.0f * MathF.Sin(Mario.FaceAngle), 
                0.0f, 
                50.0f * MathF.Cos(Mario.FaceAngle)).ToC64VelocityVec3();

            var walls = SM64Context.FindWallCollisions(detector, 80.0f, 5.0f);
            foreach (var wall in walls)
            {
                if (MeshGenerator.BreakableSolids.TryGetValue(wall->objId, out var solid))
                {
                    // They have been checked to be an IDashTrigger when added to the dict
                    ((IDashTrigger)solid).HandleDash(handVelocity); 
                }
            }
        }

        Mario.Gamepad.Stick.x = -Controls.Move.Value.X;
        Mario.Gamepad.Stick.y = -Controls.Move.Value.Y;
        if (Mario.Action == SM64Action.FLYING)
        {
            // For some reason the controls are inverted while flying with the wing cap???
            Mario.Gamepad.Stick.x *= -1.0f;
            Mario.Gamepad.Stick.y *= -1.0f;
        }
        
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
        Mario.Gamepad.CameraLook = (cameraPosition - cameraLookAt).XY().AsSM64Vec2();

        // Check for NPC interaction
        if (!cutsceneActive && Mario.ReadyToSpeak)
        {
            foreach (var actor in World.All<NPC>())
            {
                if (actor is NPC { InteractEnabled: true } npc)
                {
                    
                    if ((Position - npc.Position).LengthSquared() < npc.InteractRadius * npc.InteractRadius &&
                        Vec2.Dot((npc.Position - Position).XY(), Facing) > 0 &&
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
                        // TODO: Probably patch out the C64 collect SFX with an IL hook instead
                        if (pickup is Strawberry strawbPickup && !strawbPickup.IsCollected && !strawbPickup.IsCollecting && !strawbPickup.IsLocked)
                        {
                            strawbPickup.IsCollecting = true;
                            StrawbGet(strawbPickup);
                        }
                        else
                        {
                            pickup.Pickup(this);
                        }
                        
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
        
        // Push out of NPCs
        foreach (var actor in World.All<IHavePushout>())
        {
            // Reimplemented push_mario_out_of_object() from src/decomp/game/interaction.c
            const float padding = 0.0f;
            const float marioPushoutRadius = 37.0f * SM64_To_C64;
            const float marioPushoutHeight = 160.0f * SM64_To_C64; // Ignores crouching, but that really doesnt matter here
            
            var it = (actor as IHavePushout)!;
            if (it.PushoutRadius <= 0 || it.PushoutHeight <= 0)
                continue;
            if (Position.Z >= actor.Position.Z + it.PushoutHeight || actor.Position.Z >= Position.Z + marioPushoutHeight)
                continue;

            var diff = Position.XY() - actor.Position.XY();

            float distance = diff.Length();
            float minDistance = it.PushoutRadius + marioPushoutRadius + padding;
            
            if (distance > minDistance)
                continue;

            float pushAngle = distance != 0.0f 
                ? diff.Angle().ToSM64Angle()
                : Mario.FaceAngle;
            
            var objPos = actor.Position.ToSM64Vec3();
            
            float newMarioX = objPos.x + MathF.Sin(pushAngle) * minDistance * C64_To_SM64;
            float newMarioZ = objPos.z + MathF.Cos(pushAngle) * minDistance * C64_To_SM64;
            float marioY = Mario.Position.y;
            
            SM64SurfaceCollisionData* floor = null;
            
            SM64Context.FindWallCollision(ref newMarioX, ref marioY, ref newMarioZ, 60.0f, 50.0f);
            SM64Context.FindFloor(newMarioX, marioY, newMarioZ, ref floor);
            
            if (floor != null)
            {
                //! Doesn't update Mario's referenced floor (allows oob death when
                //  an object pushes you into a steep slope while in a ground action)
                Mario.Position = Mario.Position with { x = newMarioX, z = newMarioZ };
            }
        }

        
        if (!SuperMario64Mod.IsOddFrame && StateMachine.State != States.Cassette)
        {
            Mario.Tick();
            Facing = Facing;
        }
        
        // Strawb dance 
        if (LastStrawb is { } strawb)
        {
            // Always be over Mario's head
            strawb.Position = Position + Vec3.UnitZ * 17.0f;
            
            // Wait until we're in a star dance state
            if (!startedStrawbDance)
            {
                startedStrawbDance = Mario.Action is SM64Action.FALL_AFTER_STAR_GRAB or SM64Action.STAR_DANCE_EXIT or SM64Action.STAR_DANCE_NO_EXIT or SM64Action.STAR_DANCE_WATER;
            }
            else
            {
                if (CameraOverride != null && Mario.Action is not (SM64Action.FALL_AFTER_STAR_GRAB))
                {
                    // Lock camera once we finished falling
                    CameraOverride = new(cameraPosition, cameraLookAt);
                }
            
                if (Mario.Action is not (SM64Action.FALL_AFTER_STAR_GRAB or SM64Action.STAR_DANCE_EXIT or SM64Action.STAR_DANCE_NO_EXIT or SM64Action.STAR_DANCE_WATER) ||
                    World.Entry.Submap && Mario is { ActionState: 0, ActionTimer: 80 }) // In SM64 the warp is triggered at 80 of state 0
                {
                    // Animation is finished
                    LastStrawb = null;
                    CameraOverride = null;
                
                    World.Destroy(strawb);
                    Save.CurrentRecord.Strawberries.Add(strawb.ID);
                    
                    if (World.Entry.Submap)
                    {
                        Save.CurrentRecord.CompletedSubMaps.Add(World.Entry.Map);
                        Game.Instance.Goto(new Transition()
                        {
                            Mode = Transition.Modes.Pop,
                            ToPause = true,
                            ToBlack = new SpotlightWipe(),
                            StopMusic = true,
                            Saving = true
                        });
                    }
                }
            }
        }
        
        // Death plane
        if (Position.Z < World.DeathPlane ||
            World.Overlaps<DeathBlock>(SolidWaistTestPos))
        {
            Mario.Kill();
            return;
        }
        // Spikes (Mario can walk on them with the metal cap)
        if (!Mario.Flags.Has(SM64MarioFlags.METAL_CAP) && World.OverlapsFirst<SpikeBlock>(SolidWaistTestPos, SpikeBlockCheck) is { } spikes)
        {
            Mario.Damage(0xff, SM64IntSubtype.NONE, (Position + spikes.Direction * 3.0f).ToSM64Vec3());
        }
        
        if (Mario.Health <= 0x0100)
        {
            // Stop cap music
            SM64Context.StopBackgroundMusic(SM64SeqId.EVENT_POWERUP);
            SM64Context.StopBackgroundMusic(SM64SeqId.EVENT_METAL_CAP);
            Dead = true;
            Save.CurrentRecord.Deaths++;
        }
        
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

    public override void StrawbGet(Strawberry strawb)
    {
        LastStrawb = strawb;
        Position = strawb.Position + Vec3.UnitZ * -3;
        
        Mario.SetActionWithArg(SM64Action.FALL_AFTER_STAR_GRAB, World.Entry.Submap ? 0u : 1u);
    }

    public override void Spring(Spring spring)
    {
        Mario.Action = SM64Action.TWIRLING;
        Mario.Velocity = Mario.Velocity with { y = SpringJumpSpeed * C64_To_SM64_Vel };
    }

    public override void Kill() => Mario.Kill();
    
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