using LibSM64Sharp.LowLevel;

namespace LibSM64Sharp;

public interface ISm64MarioMesh
{
    Texture Texture { get; }

    ISm64MarioMeshTrianglesData? TriangleData { get; }
}

public interface ISm64MarioMeshTrianglesData
{
    int TriangleCount { get; }

    IReadOnlyList<LowLevelSm64Vector3f> Positions { get; }
    IReadOnlyList<LowLevelSm64Vector3f> Normals { get; }
    IReadOnlyList<LowLevelSm64Vector3f> Colors { get; }
    IReadOnlyList<LowLevelSm64Vector2f> Uvs { get; }
}