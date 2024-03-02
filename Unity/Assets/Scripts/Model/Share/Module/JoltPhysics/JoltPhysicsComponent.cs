using Jolt;

namespace ET
{
    [ComponentOf]
    public class JoltPhysicsComponent: Entity, IAwake, IDestroy
    {
        public const uint MaxBodies = 1024;
        public const uint MaxBodyPairs = 1024;
        public const uint MaxContactConstraints = 1024;

        public const int CollisionSteps = 1;

        public PhysicsSystem system;
        public BodyInterface bodies;

        // private List<(BodyID, GameObject)> managedGameObjects = new();
    }
}