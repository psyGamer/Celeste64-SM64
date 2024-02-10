using System.Runtime.InteropServices;
using LibSM64Sharp.LowLevel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LibSM64Sharp.Impl;

public partial class Sm64Context
{
    public ISm64Mario CreateMario(float x, float y, float z)
        => new Sm64Mario(marioTextureImage_, x, y, z);

    private class Sm64Mario : ISm64Mario
    {
        private readonly int id_;
        private LowLevelSm64MarioOutState outState_;
        private readonly Sm64MarioMesh mesh_;

        public Sm64Mario(Image<Rgba32> marioTextureImage, float x, float y, float z)
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
    }
}