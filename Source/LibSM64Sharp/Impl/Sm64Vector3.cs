﻿namespace LibSM64Sharp.Impl;

public partial class Sm64Context
{
    private class Sm64Vector3<TNumber> : ISm64Vector3<TNumber>
        where TNumber : INumber<TNumber>
    {
        public TNumber X { get; set; }
        public TNumber Y { get; set; }
        public TNumber Z { get; set; }
    }
}