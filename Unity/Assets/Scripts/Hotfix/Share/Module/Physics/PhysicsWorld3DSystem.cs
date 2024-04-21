using System;
using Jolt;
using Unity.Mathematics;

namespace ET
{
    [FriendOf(typeof(PhysicsWorld3D))]
    [EntitySystemOf(typeof(PhysicsWorld3D))]
    public static partial class PhysicsWorld3DSystem
    {
        [EntitySystem]
        private static void Awake(this PhysicsWorld3D self)
        {
            JoltAutoInitialization.Initialize();
            
            PhysicsSystemSettings settings = new()
            {
                MaxBodies = PhysicsWorld3D.MaxBodies,
                MaxBodyPairs = PhysicsWorld3D.MaxBodyPairs,
                MaxContactConstraints = PhysicsWorld3D.MaxContactConstraints
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
        private static void Destroy(this PhysicsWorld3D self)
        {
            self.System.Dispose();
        }

        [EntitySystem]
        private static void FixedUpdate(this PhysicsWorld3D self, GameTime gameTime)
        {
            if (!self.System.Step((float)gameTime.Elapsed.TotalSeconds, PhysicsWorld3D.CollisionSteps, out PhysicsUpdateError error))
            {
                Log.Error(error.ToString());
            }
        }

        public static Body CreateBody(this PhysicsWorld3D self, BodyCreationSettings settings)
        {
            return self.Bodies.CreateBody(settings);
        }
        
        public static BodyID CreateAndAddBody(this PhysicsWorld3D self,BodyCreationSettings settings, Activation activation)
        {
            return self.Bodies.CreateAndAddBody(settings, activation);
        }
        
        public static void AddBody(this PhysicsWorld3D self, BodyID bodyID, Activation activation)
        {
            self.Bodies.AddBody(bodyID, activation);
        }
        
        public static void DestroyBody(this PhysicsWorld3D self, BodyID bodyID)
        {
            self.Bodies.DestroyBody(bodyID);
        }
        
        public static float4x4 GetWorldTransform(this PhysicsWorld3D self, BodyID bodyID)
        {
            return self.Bodies.GetWorldTransform(bodyID);
        }
    }
}