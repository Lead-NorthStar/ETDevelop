using Jolt;
using Unity.Mathematics;

namespace ET
{
    [FriendOf(typeof(PhysicsWorld3D))]
    [FriendOf(typeof(Body3DComponent))]
    [EntitySystemOf(typeof(Body3DComponent))]
    public static partial class Body3DComponentSystem
    {
        [EntitySystem]
        private static void Awake(this Body3DComponent self, BodyCreationSettings settings)
        {
            self.PhysicsWorld3D = self.Root().GetComponent<PhysicsWorld3D>();
            if (self.PhysicsWorld3D == null)
            {
                Log.Error($"{self.Root().SceneType} 场景下没有添加 PhysicsWorld3D");
                return;
            }
            
            self.Body = self.PhysicsWorld3D.CreateBody(settings);
            self.PhysicsWorld3D.AddBody(self.GetBodyID(), Activation.Activate);
        }

        [EntitySystem]
        private static void Awake(this Body3DComponent self, ShapeSettings settings, MotionType motion, ushort layer)
        {
            self.PhysicsWorld3D = self.Root().GetComponent<PhysicsWorld3D>();
            if (self.PhysicsWorld3D == null)
            {
                Log.Error($"{self.Root().SceneType} 场景下没有添加 PhysicsWorld3D");
                return;
            }
            
            Unit unit = self.GetParent<Unit>();
            BodyCreationSettings bodySettings = BodyCreationSettings.FromShapeSettings(settings, unit.Position, unit.Rotation, motion, layer);
            self.Body = self.PhysicsWorld3D.CreateBody(bodySettings);
            self.PhysicsWorld3D.AddBody(self.GetBodyID(), Activation.Activate);
        }

        [EntitySystem]
        private static void Destroy(this Body3DComponent self)
        {
            if (self.PhysicsWorld3D == null)
                return;
            
            self.PhysicsWorld3D.DestroyBody(self.GetBodyID());
        }
        
        [EntitySystem]
        private static void Update(this Body3DComponent self)
        {
            if (self.PhysicsWorld3D == null)
                return;
            
            // Unit unit = self.GetParent<Unit>();
            // unit.Position = self.Body.GetPosition();
            // unit.Rotation = self.Body.GetRotation();
            Log.Info($"Position: {self.Body.GetPosition().x} - {self.Body.GetPosition().y} - {self.Body.GetPosition().z}");
        }

        public static BodyID GetBodyID(this Body3DComponent self)
        {
            return self.Body.GetID();
        }
    }
}