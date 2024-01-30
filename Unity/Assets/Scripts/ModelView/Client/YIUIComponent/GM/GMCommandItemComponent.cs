using YIUIFramework;
#pragma warning disable ET0020
namespace ET.Client
{
    public partial class GMCommandItemComponent: Entity
    {
        public EntityRef<GMCommandComponent>                     m_CommandComponent;
        public GMCommandComponent                                CommandComponent => m_CommandComponent;
        public GMCommandInfo                                     Info;
        public YIUILoopScroll<GMParamInfo, GMParamItemComponent> GMParamLoop;
    }
}