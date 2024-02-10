using System.Runtime.InteropServices;
using LibSM64Sharp.LowLevel;

namespace LibSM64Sharp.Impl;

public partial class Sm64Context
{
    public ISm64Mario CreateMario(float x, float y, float z) => new Sm64Mario(marioTextureImage_, x, y, z);

    private class Sm64Mario : ISm64Mario
    {
        private readonly int id_;
        private LowLevelSm64MarioOutState outState_;
        private readonly Sm64MarioMesh mesh_;

        public Sm64Mario(Texture marioTextureImage, float x, float y, float z)
        {
            id_ = LibSm64Interop.sm64_mario_create(x, y, z);
            if (id_ == -1)
            {
                throw new NullReferenceException("Failed to create Mario. Have you created a floor for him to stand on yet?");
            }

            mesh_ = new Sm64MarioMesh(marioTextureImage);
        }

        ~Sm64Mario()
        {
            ReleaseUnmanagedResources_();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources_();
            GC.SuppressFinalize(this);
        }

        private void ReleaseUnmanagedResources_() => LibSm64Interop.sm64_mario_delete(id_);

        public ISm64Gamepad Gamepad { get; } = new Sm64Gamepad();
        public ISm64MarioMesh Mesh => mesh_;

        public IReadOnlySm64Vector3<float> Position => outState_.position;
        public IReadOnlySm64Vector3<float> Velocity => outState_.velocity;
        public float FaceAngle => outState_.faceAngle;
        public short Health => outState_.health;

        public void Tick()
        {
            var inputs = new LowLevelSm64MarioInputs
            {
                buttonA = (byte)(Gamepad.IsAButtonDown ? 1 : 0),
                buttonB = (byte)(Gamepad.IsBButtonDown ? 1 : 0),
                buttonZ = (byte)(Gamepad.IsZButtonDown ? 1 : 0),
                stickX = Gamepad.AnalogStick.X,
                stickY = Gamepad.AnalogStick.Y,
                camLookX = Gamepad.CameraNormal.X,
                camLookZ = Gamepad.CameraNormal.Y,
            };

            var outState = outState_;

            var marioMesh = mesh_;
            var posHandle = GCHandle.Alloc(marioMesh.PositionsBuffer, GCHandleType.Pinned);
            var normHandle = GCHandle.Alloc(marioMesh.NormalsBuffer, GCHandleType.Pinned);
            var colorHandle = GCHandle.Alloc(marioMesh.ColorsBuffer, GCHandleType.Pinned);
            var uvHandle = GCHandle.Alloc(marioMesh.UvsBuffer, GCHandleType.Pinned);
            var outBuffers = new LowLevelSm64MarioGeometryBuffers
            {
                position = posHandle.AddrOfPinnedObject(),
                normal = normHandle.AddrOfPinnedObject(),
                color = colorHandle.AddrOfPinnedObject(),
                uv = uvHandle.AddrOfPinnedObject()
            };

            // TODO: Crashes here when sliding, need to investigate.
            LibSm64Interop.sm64_mario_tick(id_, ref inputs, ref outState, ref outBuffers);

            outState_ = outState;

            mesh_.UpdateTriangleDataFromBuffers(
                outBuffers.numTrianglesUsed);

            posHandle.Free();
            normHandle.Free();
            colorHandle.Free();
            uvHandle.Free();
        }
        
        #region Native Interop
        // TODO: Refactor this all
        
        public void SetState(uint flags) => sm64_set_mario_state(id_, flags);
        
        public void SetPosition(float x, float y, float z) => sm64_set_mario_position(id_, x, y, z);
        public void SetVelocity(float x, float y, float z) => sm64_set_mario_velocity(id_, x, y, z);
        public void SetForwardVelocity(float vel) => sm64_set_mario_forward_velocity(id_, vel);

        public void SetAngle(float x, float y, float z) => sm64_set_mario_angle(id_, x, y, z);
        public void SetFaceAngle(float y) => sm64_set_mario_faceangle(id_, y);
        
        public void SetInvincible(float y, short timer) => sm64_set_mario_invincibility(id_, timer);
        
        public void SetHealth(ushort health) => sm64_set_mario_health(id_, health);
        public void TakeDamage(uint damage, uint subtype, float x, float y, float z) => sm64_mario_take_damage(id_, damage, subtype, x, y, z);
        public void Heal(byte healCounter) => sm64_mario_heal(id_, healCounter);
        
        public void Kill() => sm64_mario_kill(id_);

        [DllImport("sm64")]
        private static extern void sm64_set_mario_action(int marioId, uint action);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_action_arg(int marioId, uint action, uint actionArg);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_animation(int marioId, int animID);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_anim_frame(int marioId, short animFrame);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_state(int marioId, uint flags);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_position(int marioId, float x, float y, float z);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_angle(int marioId, float x, float y, float z);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_faceangle(int marioId, float y);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_velocity(int marioId, float x, float y, float z);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_forward_velocity(int marioId, float vel);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_invincibility(int marioId, short timer);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_water_level(int marioId, int level);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_gas_level(int marioId, int level);
        [DllImport("sm64")]
        private static extern void sm64_set_mario_health(int marioId, ushort health);
        [DllImport("sm64")]
        private static extern void sm64_mario_take_damage(int marioId, uint damage, uint subtype, float x, float y, float z);
        [DllImport("sm64")]
        private static extern void sm64_mario_heal(int marioId, byte healCounter);
        [DllImport("sm64")]
        private static extern void sm64_mario_kill(int marioId);
        [DllImport("sm64")]
        private static extern void sm64_mario_interact_cap(int marioId, uint capFlag, ushort capTime, byte playMusic);
        [DllImport("sm64")]
        private static extern void sm64_mario_extend_cap(int marioId, ushort capTime);
        [DllImport("sm64")]
        private static extern bool sm64_mario_attack(int marioId, float x, float y, float z, float hitboxHeight);
        
        #endregion
    }
}