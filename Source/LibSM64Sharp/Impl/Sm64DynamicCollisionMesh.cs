using System.Runtime.InteropServices;
using LibSM64Sharp.LowLevel;

namespace LibSM64Sharp.Impl;

public partial class Sm64Context
{
    public ISm64DynamicCollisionMeshBuilder CreateDynamicCollisionMesh(float scale = 1)
        => new Sm64DynamicCollisionMeshBuilder(scale);

    private class Sm64DynamicCollisionMeshBuilder(float scale) : ISm64DynamicCollisionMeshBuilder
    {
        private readonly Sm64Vector3<float> position_ = new();
        private readonly Sm64Vector3<float> eulerRotation_ = new();
        private readonly List<ISm64Triangle> triangles_ = new();

        public ISm64DynamicCollisionMesh Build()
            => new Sm64DynamicCollisionMesh(position_, eulerRotation_, triangles_);

        public ISm64DynamicCollisionMeshBuilder AddTriangle(
            Sm64SurfaceType surfaceType,
            Sm64TerrainType terrainType,
            (int x, int y, int z) vertex1,
            (int x, int y, int z) vertex2,
            (int x, int y, int z) vertex3)
        {
            triangles_.Add(new Sm64Triangle
            {
                SurfaceType = surfaceType,
                TerrainType = terrainType,
                Vertices = new[] { vertex1, vertex2, vertex3 }.Select(
                        xyz => new Sm64Vector3<int>
                        {
                            X = (int)(xyz.x * scale),
                            Y = (int)(xyz.y * scale),
                            Z = (int)(xyz.z * scale),
                        })
                    .ToArray(),
            });
            return this;
        }

        public ISm64DynamicCollisionMeshBuilder AddQuad(
            Sm64SurfaceType surfaceType,
            Sm64TerrainType terrainType,
            (int x, int y, int z) vertex1,
            (int x, int y, int z) vertex2,
            (int x, int y, int z) vertex3,
            (int x, int y, int z) vertex4)
        {
            AddTriangle(
                surfaceType,
                terrainType,
                vertex1,
                vertex2,
                vertex3);
            AddTriangle(
                surfaceType,
                terrainType,
                vertex4,
                vertex3,
                vertex2);
            return this;
        }

        public ISm64DynamicCollisionMeshBuilder SetPosition(float x, float y, float z)
        {
            position_.X = x;
            position_.Y = y;
            position_.Z = z;
            return this;
        }

        public ISm64DynamicCollisionMeshBuilder SetEulerRotation(float xDegrees, float yDegrees, float zDegrees)
        {
            eulerRotation_.X = xDegrees;
            eulerRotation_.Y = yDegrees;
            eulerRotation_.Z = zDegrees;
            return this;
        }
    }

    private class Sm64DynamicCollisionMesh : ISm64DynamicCollisionMesh
    {
        private readonly uint id_;
        private readonly Sm64Vector3<float> position_;
        private readonly Sm64Vector3<float> eulerRotation_;

        public Sm64DynamicCollisionMesh(Sm64Vector3<float> position, Sm64Vector3<float> eulerRotation, IReadOnlyList<ISm64Triangle> triangles)
        {
            position_ = position;
            eulerRotation_ = eulerRotation;
            Triangles = triangles;

            var surfaces =
                triangles.Select(triangle =>
                    {
                        var vertices = triangle.Vertices;
                        return new LowLevelSm64Surface
                        {
                            type = (short)triangle.SurfaceType,
                            terrain = (ushort)triangle.TerrainType,
                            v0x = vertices[0].X,
                            v0y = vertices[0].Y,
                            v0z = vertices[0].Z,
                            v1x = vertices[1].X,
                            v1y = vertices[1].Y,
                            v1z = vertices[1].Z,
                            v2x = vertices[2].X,
                            v2y = vertices[2].Y,
                            v2z = vertices[2].Z,
                        };
                    })
                    .ToArray();
            var surfacesHandle = GCHandle.Alloc(surfaces, GCHandleType.Pinned);

            var surfaceObject = new LowLevelSm64SurfaceObject
            {
                transform = new LowLevelSm64ObjectTransform
                {
                    position =
                    [
                        position_.X,
                        position_.Y,
                        position_.Z
                    ],
                    eulerRotation =
                    [
                        eulerRotation_.X,
                        eulerRotation_.Y,
                        eulerRotation_.Z
                    ],
                },
                surfaceCount = (uint)surfaces.Length,
                surfaces = surfacesHandle.AddrOfPinnedObject(),
            };
            id_ = LibSm64Interop.sm64_surface_object_create(ref surfaceObject);

            surfacesHandle.Free();
        }

        ~Sm64DynamicCollisionMesh()
        {
            ReleaseUnmanagedResources_();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources_();
            GC.SuppressFinalize(this);
        }

        private void ReleaseUnmanagedResources_() => LibSm64Interop.sm64_surface_object_delete(id_);

        public IReadOnlyList<ISm64Triangle> Triangles { get; }

        public IReadOnlySm64Vector3<float> Position => position_;

        public ISm64DynamicCollisionMesh SetPosition(float x, float y, float z)
        {
            position_.X = x;
            position_.Y = y;
            position_.Z = z;

            UpdateOrientation_();

            return this;
        }

        public IReadOnlySm64Vector3<float> EulerRotation => eulerRotation_;

        public ISm64DynamicCollisionMesh SetEulerRotation(
            float xDegrees,
            float yDegrees,
            float zDegrees)
        {
            eulerRotation_.X = xDegrees;
            eulerRotation_.Y = yDegrees;
            eulerRotation_.Z = zDegrees;

            UpdateOrientation_();

            return this;
        }

        private void UpdateOrientation_()
        {
            var transform = new LowLevelSm64ObjectTransform
            {
                position = new[]
                {
                    position_.X,
                    position_.Y,
                    position_.Z
                },
                eulerRotation = new[]
                {
                    eulerRotation_.X,
                    eulerRotation_.Y,
                    eulerRotation_.Z
                },
            };

            LibSm64Interop.sm64_surface_object_move(id_, ref transform);
        }
    }
}