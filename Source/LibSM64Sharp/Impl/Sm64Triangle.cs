namespace LibSM64Sharp.Impl;

public partial class Sm64Context
{
    private class Sm64Triangle : ISm64Triangle
    {
        public required Sm64SurfaceType SurfaceType { get; init; }
        public required Sm64TerrainType TerrainType { get; init; }
        public required IReadOnlyList<IReadOnlySm64Vector3<int>> Vertices { get; init; }
    }
}