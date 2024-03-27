using LibSM64;

namespace Celeste64.Mod.SuperMario64;

public class SuperMario64Mod : GameMod
{
    public static SuperMario64Mod Instance { get; private set; } = null!;
    
    /// <summary>
    /// SM64 runs at 30FPS but C64 at 60FPS, so we need to skip every odd frame.
    /// </summary>
    public static bool IsOddFrame { get; private set; } = false;
    
    public SuperMario64Mod()
    {
        Instance = this;
    }

    public override void OnModLoaded()
    {
        var romBytes = File.ReadAllBytes("sm64.z64");
        SM64Context.InitializeFromROM(romBytes);
        AudioPlayer.Create();
        MeshGenerator.Load();
        
        AddActorFactory("PlayerSpawn", new Map.ActorFactory((map, entity) =>
        {
            var name = entity.GetStringProperty("name", Map.StartCheckpoint);

            // spawns ther player if the world entry is this checkpoint
            // OR the world entry has no checkpoint and we're the start
            // OR the world entry checkpoint is misconfigured and we're the start
            var spawnsPlayer = 
                (map.LoadWorld!.Entry.CheckPoint == name) ||
                (string.IsNullOrEmpty(map.LoadWorld.Entry.CheckPoint) && name == Map.StartCheckpoint) ||
                (!map.Checkpoints.Contains(map.LoadWorld.Entry.CheckPoint) && name == Map.StartCheckpoint);

            if (spawnsPlayer)
                map.HandleActorCreation(map.LoadWorld, entity, new MarioPlayer(), null);

            if (name != Map.StartCheckpoint)
                map.HandleActorCreation(map.LoadWorld, entity, new Checkpoint(name), null);
            
            return null;
        }));
    }
    public override void OnModUnloaded()
    {
        AudioPlayer.Dispose();
        SM64Context.Terminate();
    }
    public override void Update(float deltaTime)
    {
        IsOddFrame = !IsOddFrame;
        AudioPlayer.Update();
    }

    public override void OnSceneEntered(Scene scene)
    {
        if (scene is not World world)
        {
            SM64Context.StopAllBackgroundMusic();
            return;
        }
        
        MeshGenerator.GenerateSolids(world);
    }
}
