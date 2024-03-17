using Jolt;
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Room))]
    public class JoltWorldComponent: Entity, IAwake, IDestroy
    {
        public const uint MaxBodies = 1024;
        public const uint MaxBodyPairs = 1024;
        public const uint MaxContactConstraints = 1024;

        public const int CollisionSteps = 1;

        public PhysicsSystem System;
        public BodyInterface Bodies;
    }
}