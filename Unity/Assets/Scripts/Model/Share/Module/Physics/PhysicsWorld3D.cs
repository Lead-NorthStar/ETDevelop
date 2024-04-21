using Jolt;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class PhysicsWorld3D: Entity, IAwake, IDestroy, IFixedUpdate<GameTime>
    {
        public const uint MaxBodies = 1024;
        public const uint MaxBodyPairs = 1024;
        public const uint MaxContactConstraints = 1024;

        public const int CollisionSteps = 1;

        public PhysicsSystem System;
        public BodyInterface Bodies;
    }
}