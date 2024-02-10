﻿namespace LibSM64Sharp;

public interface ISm64StaticCollisionMeshBuilder
    : ISm64CollisionMeshBuilder<
        ISm64StaticCollisionMeshBuilder,
        ISm64StaticCollisionMesh>;

public interface ISm64StaticCollisionMesh : ISm64CollisionMesh;