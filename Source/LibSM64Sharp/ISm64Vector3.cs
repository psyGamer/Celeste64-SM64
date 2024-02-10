namespace LibSM64Sharp;

public interface IReadOnlySm64Vector3<out TNumber>
    where TNumber : INumber<TNumber>
{
    TNumber X { get; }
    TNumber Y { get; }
    TNumber Z { get; }
}

public interface ISm64Vector3<TNumber> : IReadOnlySm64Vector3<TNumber>
    where TNumber : INumber<TNumber>
{
    new TNumber X { get; set; }
    new TNumber Y { get; set; }
    new TNumber Z { get; set; }
}

public static partial class Extensions
{
    public static Vec2 ToVec2(this IReadOnlySm64Vector2<float> vec) => new(vec.X, vec.Y);
    public static Vec3 ToVec3(this IReadOnlySm64Vector3<float> vec) => new(vec.X, vec.Z, vec.Y); // !! IMPORTANT !! In SM64 Y and Z are flipped compared to C64.
}