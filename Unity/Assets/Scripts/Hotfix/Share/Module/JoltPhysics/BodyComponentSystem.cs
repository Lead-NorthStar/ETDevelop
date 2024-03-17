using Jolt;
using Unity.Mathematics;

namespace ET
{
    [FriendOf(typeof(JoltWorldComponent))]
    [FriendOf(typeof(BodyComponent))]
    [EntitySystemOf(typeof(BodyComponent))]
    public static partial class BodyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BodyComponent self, BodyCreationSettings settings)
        {
            self.WorldComponent = self.Scene<Room>().GetComponent<JoltWorldComponent>();
            self.Body = self.WorldComponent.CreateBody(self, settings);
        }

        [EntitySystem]
        private static void Awake(this BodyComponent self, ShapeSettings settings, MotionType motion, ushort layer)
        {
            self.WorldComponent = self.Scene<Room>().GetComponent<JoltWorldComponent>();
            Unit unit = self.GetParent<Unit>();
            BodyCreationSettings bodySettings = BodyCreationSettings.FromShapeSettings(settings, unit.Position, unit.Rotation, motion, layer);
            self.Body = self.WorldComponent.CreateBody(self, bodySettings);
        }

        [EntitySystem]
        private static void Destroy(this BodyComponent self)
        {
            self.WorldComponent.DestroyBody(self.Body.GetID());
        }
        
        private static void FixedUpdate(this BodyComponent self)
        {
            BodyComponent bodyComponent = self.GetParent<BodyComponent>();
            Unit unit = bodyComponent.GetParent<Unit>();
            // unit.Position = bodyComponent.Body.GetPosition();
            // unit.Rotation = bodyComponent.Body.GetRotation();
        }
    }
}