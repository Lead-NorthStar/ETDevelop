using Jolt;

namespace ET
{
    public class JoltPhysicsSingleton: Singleton<NumericWatcherComponent>, ISingletonAwake
    {
        public const uint MaxBodies = 1024;
        public const uint MaxBodyPairs = 1024;
        public const uint MaxContactConstraints = 1024;

        public const int CollisionSteps = 1;

        public PhysicsSystem system;
        public BodyInterface bodies;

        // private List<(BodyID, GameObject)> managedGameObjects = new();

        public void Awake()
        {
            JoltAutoInitialization.Initialize();
            
            PhysicsSystemSettings settings = new()
            {
                MaxBodies = MaxBodies,
                MaxBodyPairs = MaxBodyPairs,
                MaxContactConstraints = MaxContactConstraints
            };

            system = new PhysicsSystem(settings);
            bodies = system.GetBodyInterface();

            // foreach (var authoring in FindObjectsByType<PhysicsBody>(FindObjectsSortMode.None))
            // {
            //     var bodyID = PhysicsHelpers.CreateBodyFromGameObject(bodies, authoring.gameObject);
            //     managedGameObjects.Add((bodyID, authoring.gameObject));
            // }

            system.OptimizeBroadPhase();
        }

        public void FixedUpdate(int fixedDeltaTime)
        {
            if (system.Step(fixedDeltaTime, CollisionSteps, out PhysicsUpdateError error))
            {
                UpdateManagedTransforms();
            }
            else
            {
                Log.Error(error.ToString());
            }
        }
        
        private void UpdateManagedTransforms()
        {
            // foreach (var (bodyID, gobj) in managedGameObjects)
            // {
            //     PhysicsHelpers.ApplyTransform(bodies, bodyID, gobj.transform);
            // }
        }

        private void OnDestroy()
        {
            system.Dispose();
        }
    }
}