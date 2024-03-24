using System.Runtime.InteropServices;
using static LibSM64.Native;

namespace LibSM64;

public class MarioMesh
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct MarioVertex(SM64Vector3f position, SM64Vector3f normal, SM64Vector3f color, SM64Vector2f texcoord) : IVertex
    {
        public SM64Vector3f Pos = position;
        public SM64Vector3f Normal = normal;
        public SM64Vector3f Col = color;
        public SM64Vector2f Tex = texcoord;

        public VertexFormat Format => VertexFormat;

        private static readonly VertexFormat VertexFormat = VertexFormat.Create<MarioVertex>(
        [
            new(0, VertexType.Float3, normalized: false),
            new(1, VertexType.Float3, normalized: true),
            new(2, VertexType.Float3, normalized: false),
            new(3, VertexType.Float2, normalized: false),
        ]);
    }
    
    private const int SM64_GEO_MAX_TRIANGLES = 1024;
    private const int SM64_GEO_MAX_VERTICES = 3 * SM64_GEO_MAX_TRIANGLES;

    public readonly Mesh Mesh = new();
    public int TriangleCount { get; private set; }
    
    internal readonly SM64Vector3f[] PositionsBuffer = new SM64Vector3f[SM64_GEO_MAX_VERTICES];
    internal readonly SM64Vector3f[] NormalsBuffer = new SM64Vector3f[SM64_GEO_MAX_VERTICES];
    internal readonly SM64Vector3f[] ColorsBuffer = new SM64Vector3f[SM64_GEO_MAX_VERTICES];
    internal readonly SM64Vector2f[] UvsBuffer = new SM64Vector2f[SM64_GEO_MAX_VERTICES];
    
    private readonly MarioVertex[] vertices = new MarioVertex[SM64_GEO_MAX_VERTICES];
    
    public MarioMesh()
    {
        // SM64 doesn't use indexing, but Foster requires it..
        var indices = new int[SM64_GEO_MAX_VERTICES];
        for (int i = 0; i < SM64_GEO_MAX_VERTICES; i++)
        {
            indices[i] = i;
            
            // Pre-allocate all vertices
            vertices[i] = new();
        }
        Mesh.SetIndices<int>(indices);
    }
    
    public void UpdateBuffers(int triangleCount)
    {
        for (int i = 0; i < triangleCount * 3; i++)
        {
            vertices[i].Pos = PositionsBuffer[i];
            vertices[i].Normal = NormalsBuffer[i];
            vertices[i].Col = ColorsBuffer[i];
            vertices[i].Tex = UvsBuffer[i];
        }
        Mesh.SetVertices<MarioVertex>(vertices);
        
        TriangleCount = triangleCount;
    }
}