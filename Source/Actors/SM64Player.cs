using libsm64sharp;
using libsm64sharp.lowlevel;

namespace Celeste64;

public class SM64Player : Player
{
    public SM64Player()
    {
       
    }

    ISm64Context Context;
    ISm64Mario Mario;
    
    public override void Added()
    {
        base.Added();
        
        var romBytes = File.ReadAllBytes("sm64.z64");
        
        Context = Sm64Context.InitFromRom(romBytes);
        
        LibSm64Interop.sm64_static_surfaces_load(Data.surfaces, (ulong)Data.surfaces.Length);
        // int marioId = LibSm64Interop.sm64_mario_create(0, 1000, 0);
        Mario = Context.CreateMario(0, 1000, 0);
        
        Log.Info($"Mario ID: {Mario}");
        
        // const float Size = 1000.0f;
        // const float ZOffset = -100.0f; 
        // var builder = Context.CreateStaticCollisionMesh();
        // builder.AddQuad(Sm64SurfaceType.SURFACE_DEFAULT, Sm64TerrainType.TERRAIN_GRASS, 
        //     ((int x, int y, int z))(Position.X - Size, Position.Y + ZOffset, Position.Z - Size),
        //     ((int x, int y, int z))(Position.X + Size, Position.Y + ZOffset, Position.Z - Size),
        //     ((int x, int y, int z))(Position.X - Size, Position.Y + ZOffset, Position.Z + Size),
        //     ((int x, int y, int z))(Position.X + Size, Position.Y + ZOffset, Position.Z + Size));
        // builder.Build();

        // Mario = Context.CreateMario(Position.X, Position.Y, Position.Z);
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
     
        Mario.Gamepad.AnalogStick.X = Controls.Move.ValueNoDeadzone.X;
        Mario.Gamepad.AnalogStick.Y = Controls.Move.ValueNoDeadzone.Y;
        Mario.Gamepad.IsAButtonDown = Controls.Jump.Down;
        Mario.Gamepad.IsBButtonDown = Controls.Dash.Down;
        Mario.Gamepad.IsZButtonDown = Controls.Climb.Down;
        
        Mario.Tick();
        
        Log.Info($"Pos: {Mario.Position.X} {Mario.Position.Y} {Mario.Position.Z}");
        Log.Info($"Vel: {Mario.Velocity.X} {Mario.Velocity.Y} {Mario.Velocity.Z}");

        Position = Position with
        {
            X = Mario.Position.X,
            Y = Mario.Position.Z,

            Z = Mario.Position.Y,
        };
    }

    public override void Kill()
    {
        // no.
    }
}