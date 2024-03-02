using Jolt;

namespace ET
{
    [FriendOf(typeof(JoltPhysicsComponent))]
    [EntitySystemOf(typeof(JoltPhysicsComponent))]
    public static partial class JoltPhysicsComponentSystem
    {
        [EntitySystem]
        private static void Awake(this JoltPhysicsComponent self)
        {
            JoltAutoInitialization.Initialize();
            
            PhysicsSystemSettings settings = new()
            {
                MaxBodies = JoltPhysicsComponent.MaxBodies,
                MaxBodyPairs = JoltPhysicsComponent.MaxBodyPairs,
                MaxContactConstraints = JoltPhysicsComponent.MaxContactConstraints
            };

            self.system = new PhysicsSystem(settings);
            self.bodies = self.system.GetBodyInterface();

            // TODO: 传入地图数据，读取后创建实体
            // foreach (var authoring in FindObjectsByType<PhysicsBody>(FindObjectsSortMode.None))
            // {
            //     var bodyID = PhysicsHelpers.CreateBodyFromGameObject(bodies, authoring.gameObject);
            //     managedGameObjects.Add((bodyID, authoring.gameObject));
            // }
            // self.system.OptimizeBroadPhase();
        }

        [EntitySystem]
        private static void Destroy(this JoltPhysicsComponent self)
        {
            self.system.Dispose();
        }
        
        public static void FixedUpdate(this JoltPhysicsComponent self, int fixedDeltaTime)
        {
            if (self.system.Step(fixedDeltaTime, JoltPhysicsComponent.CollisionSteps, out PhysicsUpdateError error))
            {
                self.UpdateManagedTransforms();
            }
            else
            {
                Log.Error(error.ToString());
            }
        }
        
        private static void UpdateManagedTransforms(this JoltPhysicsComponent self)
        {
            // foreach (var (bodyID, gobj) in managedGameObjects)
            // {
            //     PhysicsHelpers.ApplyTransform(bodies, bodyID, gobj.transform);
            // }
        }
    }
}