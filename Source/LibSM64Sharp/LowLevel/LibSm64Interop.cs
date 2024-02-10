using System.Runtime.InteropServices;

namespace LibSM64Sharp.LowLevel;

public static class LibSm64Interop
{
    private const string SM64_DLL = "sm64";

    [DllImport(SM64_DLL)]
    public static extern void sm64_register_debug_print_function(
        IntPtr debugPrintFunctionPtr);

    [DllImport(SM64_DLL)]
    public static extern void sm64_register_play_sound_function(
        IntPtr playSoundFunctionPtr);

    [DllImport(SM64_DLL)]
    public static extern void sm64_global_init(
        IntPtr rom,
        IntPtr outTexture);

    [DllImport(SM64_DLL)]
    public static extern void sm64_global_terminate();

    [DllImport(SM64_DLL)]
    public static extern void sm64_audio_init(IntPtr rom);

    [DllImport(SM64_DLL)]
    public static extern uint sm64_audio_tick(uint numQueuedSamples,
        uint numDesiredSamples,
        IntPtr audioBuffer);

    [DllImport(SM64_DLL)]
    public static extern void sm64_static_surfaces_load(
        LowLevelSm64Surface[] surfaces,
        ulong numSurfaces);

    #region Mario
    
    [DllImport(SM64_DLL)]
    public static extern int sm64_mario_create(
        float marioX,
        float marioY,
        float marioZ);
    
    [DllImport(SM64_DLL)]
    public static extern void sm64_mario_tick(
        int marioId, 
        ref LowLevelSm64MarioInputs inputs,
        ref LowLevelSm64MarioOutState outState,
        ref LowLevelSm64MarioGeometryBuffers outBuffers);

    [DllImport(SM64_DLL)]
    public static extern void sm64_mario_delete(int marioId);
    
    [DllImport(SM64_DLL)]
    public static extern void sm64_set_mario_action(int marioId);
    
    /*
     * extern SM64_LIB_FN void sm64_set_mario_action(int32_t marioId, uint32_t action);
       extern SM64_LIB_FN void sm64_set_mario_action_arg(int32_t marioId, uint32_t action, uint32_t actionArg);
       extern SM64_LIB_FN void sm64_set_mario_animation(int32_t marioId, int32_t animID);
       extern SM64_LIB_FN void sm64_set_mario_anim_frame(int32_t marioId, int16_t animFrame);
       extern SM64_LIB_FN void sm64_set_mario_state(int32_t marioId, uint32_t flags);
       extern SM64_LIB_FN void sm64_set_mario_position(int32_t marioId, float x, float y, float z);
       extern SM64_LIB_FN void sm64_set_mario_angle(int32_t marioId, float x, float y, float z);
       extern SM64_LIB_FN void sm64_set_mario_faceangle(int32_t marioId, float y);
       extern SM64_LIB_FN void sm64_set_mario_velocity(int32_t marioId, float x, float y, float z);
       extern SM64_LIB_FN void sm64_set_mario_forward_velocity(int32_t marioId, float vel);
       extern SM64_LIB_FN void sm64_set_mario_invincibility(int32_t marioId, int16_t timer);
       extern SM64_LIB_FN void sm64_set_mario_water_level(int32_t marioId, signed int level);
       extern SM64_LIB_FN void sm64_set_mario_gas_level(int32_t marioId, signed int level);
       extern SM64_LIB_FN void sm64_set_mario_health(int32_t marioId, uint16_t health);
       extern SM64_LIB_FN void sm64_mario_take_damage(int32_t marioId, uint32_t damage, uint32_t subtype, float x, float y, float z);
       extern SM64_LIB_FN void sm64_mario_heal(int32_t marioId, uint8_t healCounter);
       extern SM64_LIB_FN void sm64_mario_kill(int32_t marioId);
       extern SM64_LIB_FN void sm64_mario_interact_cap(int32_t marioId, uint32_t capFlag, uint16_t capTime, uint8_t playMusic);
       extern SM64_LIB_FN void sm64_mario_extend_cap(int32_t marioId, uint16_t capTime);
       extern SM64_LIB_FN bool sm64_mario_attack(int32_t marioId, float x, float y, float z, float hitboxHeight);
       
     */
    
    #endregion

    [DllImport(SM64_DLL)]
    public static extern uint sm64_surface_object_create(
        ref LowLevelSm64SurfaceObject surfaceObject);

    [DllImport(SM64_DLL)]
    public static extern void sm64_surface_object_move(
        uint objectId,
        ref LowLevelSm64ObjectTransform transform);

    [DllImport(SM64_DLL)]
    public static extern void sm64_surface_object_delete(uint objectId);


    [DllImport(SM64_DLL)]
    public static extern int sm64_surface_find_wall_collision(
        ref float xPtr,
        ref float yPtr,
        ref float zPtr,
        float offsetY,
        float radius);

    [DllImport(SM64_DLL)]
    public static extern int sm64_surface_find_wall_collisions(
        ref LowLevelSm64WallCollisionData colData);

    [DllImport(SM64_DLL)]
    public static extern unsafe float sm64_surface_find_ceil(
        float posX,
        float posY,
        float posZ,
        ref LowLevelSm64SurfaceInternal* pceil);

    [DllImport(SM64_DLL)]
    public static extern unsafe float sm64_surface_find_floor_height_and_data(
        float xPos,
        float yPos,
        float zPos,
        ref LowLevelSm64FloorGeometry* floorGeo);

    [DllImport(SM64_DLL)]
    public static extern float sm64_surface_find_floor_height(
        float x,
        float y,
        float z);

    [DllImport(SM64_DLL)]
    public static extern unsafe float sm64_surface_find_floor(
        float xPos,
        float yPos,
        float zPos,
        ref LowLevelSm64SurfaceInternal* pfloor);

    [DllImport(SM64_DLL)]
    public static extern float sm64_surface_find_water_level(float x, float z);

    [DllImport(SM64_DLL)]
    public static extern float sm64_surface_find_poison_gas_level(
        float x,
        float z);
}