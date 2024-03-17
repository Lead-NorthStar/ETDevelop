using Jolt;
using Unity.Mathematics;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class BodyComponent : Entity, IAwake<BodyCreationSettings>, IAwake<ShapeSettings, MotionType, ushort>, IDestroy
    {
        public Body Body;
        public JoltWorldComponent WorldComponent { get; set; }
    }
}