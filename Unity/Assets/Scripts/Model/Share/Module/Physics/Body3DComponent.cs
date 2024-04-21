using Jolt;
using Unity.Mathematics;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class Body3DComponent : Entity, IAwake<BodyCreationSettings>, IAwake<ShapeSettings, MotionType, ushort>, IDestroy, IFixedUpdate
    {
        public Body Body;
        public PhysicsWorld3D PhysicsWorld3D { get; set; }
    }
}