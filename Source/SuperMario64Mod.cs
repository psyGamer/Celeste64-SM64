using Celeste64;
using SledgeEntity = Sledge.Formats.Map.Objects.Entity;

namespace Celeste64.Mod.SuperMario64;

public class SuperMario64Mod : GameMod
{
    public override void OnModLoaded()
    {
        SM64Player.Load();
    }

    public override void OnModUnloaded()
    {
        SM64Player.Unload();
    }
    
    public override void OnPreMapLoaded(World world, Map map)
    {
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
                map.HandleActorCreation(map.LoadWorld, entity, new SM64Player(), null);

            if (name != Map.StartCheckpoint)
                map.HandleActorCreation(map.LoadWorld, entity, new Checkpoint(name), null);
            
            return null;
        }));
    }
}
