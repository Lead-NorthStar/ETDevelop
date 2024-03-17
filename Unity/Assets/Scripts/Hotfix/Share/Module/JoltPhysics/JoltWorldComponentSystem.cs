using Jolt;
using Unity.Mathematics;

namespace ET
{
    [FriendOf(typeof(JoltWorldComponent))]
    [EntitySystemOf(typeof(JoltWorldComponent))]
    public static partial class JoltWorldComponentSystem
    {
        [EntitySystem]
        private static void Awake(this JoltWorldComponent self)
        {
            JoltAutoInitialization.Initialize();
            
            PhysicsSystemSettings settings = new()
            {
                MaxBodies = JoltWorldComponent.MaxBodies,
                MaxBodyPairs = JoltWorldComponent.MaxBodyPairs,
                MaxContactConstraints = JoltWorldComponent.MaxContactConstraints
            };

            self.System = new PhysicsSystem(settings);
            self.Bodies = self.System.GetBodyInterface();

            // TODO: 传入地图数据，读取后创建实体
            // foreach (var authoring in FindObjectsByType<PhysicsBody>(FindObjectsSortMode.None))
            // {
            //     var bodyID = PhysicsHelpers.CreateBodyFromGameObject(bodies, authoring.gameObject);
            //     managedGameObjects.Add((bodyID, authoring.gameObject));
            // }
            // self.system.OptimizeBroadPhase();
        }

        [EntitySystem]
        private static void Destroy(this JoltWorldComponent self)
        {
            self.System.Dispose();
        }

        public static void FixedUpdate(this JoltWorldComponent self, int fixedDeltaTime)
        {
            if (!self.System.Step(fixedDeltaTime, JoltWorldComponent.CollisionSteps, out PhysicsUpdateError error))
            {
                Log.Error(error.ToString());
            }
        }

        public static Body CreateBody(this JoltWorldComponent self, BodyComponent bodyComponent, BodyCreationSettings settings)
        {
            return self.Bodies.CreateBody(settings);
        }
        
        public static void DestroyBody(this JoltWorldComponent self, BodyID bodyID)
        {
            self.Bodies.DestroyBody(bodyID);
        }
    }
}