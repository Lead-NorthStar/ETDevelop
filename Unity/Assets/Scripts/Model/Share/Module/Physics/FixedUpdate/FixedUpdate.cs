using System;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class FixedUpdate : Entity, IAwake, IUpdate
    {
        public GameTime UpdateTime;
        public TimerTick PlayTimer;
        public TimerTick UpdateTimer;
        public int[] LastUpdateCount;
        public float UpdateCountAverageSlowLimit;
        public TimeSpan SingleFrameUpdateTime;
        public TimeSpan TotalUpdateTime;
        public TimeSpan MaximumElapsedTime;
        public TimeSpan AccumulatedElapsedGameTime;
        public TimeSpan LastFrameElapsedGameTime;
        public int NextLastUpdateCountIndex;
        public bool DrawRunningSlowly;
        public bool ForceElapsedTimeToZero;
        public TimerTick Timer;
        public object TickLock = new();
        /// <summary>
        /// 每次更新的时间间隔,默认30Hz<br/>
        /// 获取或设置目标运行时间，即每次 tick/update 的持续时间
        /// </summary>
        /// <value>目标运行时间.</value>
        public TimeSpan TargetElapsedTime { get; set; }
    }
}