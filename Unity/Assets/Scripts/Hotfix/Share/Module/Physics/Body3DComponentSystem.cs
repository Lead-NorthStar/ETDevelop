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
            self.PhysicsWorld3D = self.Scene().GetComponent<PhysicsWorld3D>();
            self.Body = self.PhysicsWorld3D.CreateBody(self, settings);
        }

        [EntitySystem]
        private static void Awake(this Body3DComponent self, ShapeSettings settings, MotionType motion, ushort layer)
        {
            self.PhysicsWorld3D = self.Scene().GetComponent<PhysicsWorld3D>();
            Unit unit = self.GetParent<Unit>();
            BodyCreationSettings bodySettings = BodyCreationSettings.FromShapeSettings(settings, unit.Position, unit.Rotation, motion, layer);
            self.Body = self.PhysicsWorld3D.CreateBody(self, bodySettings);
        }

        [EntitySystem]
        private static void Destroy(this Body3DComponent self)
        {
            self.PhysicsWorld3D.DestroyBody(self.Body.GetID());
        }
        
        [EntitySystem]
        private static void Update(this Body3DComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            // unit.Position = self.Body.GetPosition();
            // unit.Rotation = self.Body.GetRotation();
        }
    }
}