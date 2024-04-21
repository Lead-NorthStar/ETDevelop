namespace ET
{
    public static partial class FixedDefine
    {
        [StaticField]
        public static float FixedDeltaTime = 1f / LogicFrame;

        [StaticField]
        public static int LogicFrame = 60;

        [StaticField]
        public static int FixedDeltaTicks
        {
            get
            {
                return LogicFrame switch
                {
                    20 => 500000,
                    25 => 400000,
                    30 => 333333,
                    50 => 200000,
                    60 => 166666,
                    _ => 500000
                };
            }
        }
    }
}