using System;

namespace ET
{
    /// <summary>
    /// 用于可变步长(实时)或固定步长(游戏时间)游戏的当前计时。
    /// </summary>
    public class GameTime
    {
        // 累计流逝时间
        private TimeSpan AccumulatedElapsedTime;

        // 每秒累计帧计数
        private int AccumulatedFrameCountPerSecond;

        #region 构造函数和析构函数

        public GameTime()
        {
            this.AccumulatedElapsedTime = TimeSpan.Zero;
        }

        /// <param name="totalTime">自游戏开始以来的总游戏时间。</param>
        /// <param name="elapsedTime">自上次更新以来经过的游戏时间。</param>
        public GameTime(TimeSpan totalTime, TimeSpan elapsedTime)
        {
            Total = totalTime;
            Elapsed = elapsedTime;
            this.AccumulatedElapsedTime = TimeSpan.Zero;
        }

        /// <param name="totalTime">自游戏开始以来的总游戏时间。</param>
        /// <param name="elapsedTime">自上次更新以来经过的游戏时间。</param>
        /// <param name="isRunningSlowly">如果游戏运行异常缓慢，则为True。</param>
        public GameTime(TimeSpan totalTime, TimeSpan elapsedTime, bool isRunningSlowly)
        {
            Total = totalTime;
            Elapsed = elapsedTime;
            IsRunningSlowly = isRunningSlowly;
            this.AccumulatedElapsedTime = TimeSpan.Zero;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 获取自上次更新后的游戏时间
        /// </summary>
        public TimeSpan Elapsed { get; private set; }

        /// <summary>
        /// 获取一个值，该值指示游戏的运行速度是否慢于其TargetElapsedTime。例如，这可以用来渲染更少的细节……等等。
        /// </summary>
        /// <value> 如果此实例运行缓慢，则为 <c>true</c>，否则为<c>false</c>。</value>
        public bool IsRunningSlowly { get; private set; }

        /// <summary>
        /// 获取自游戏开始以来的总游戏时间。
        /// </summary>
        public TimeSpan Total { get; private set; }

        /// <summary>
        /// 获取自游戏开始以来的帧数。
        /// </summary>
        public int FrameCount { get; private set; }

        /// <summary>
        ///获取当前运行游戏的每秒帧数 (FPS)。
        /// </summary>
        /// <value>每秒帧数。</value>
        public float FramePerSecond { get; private set; }

        /// <summary>
        /// 获取每帧的时间。
        /// </summary>
        public TimeSpan TimePerFrame { get; private set; }

        /// <summary>
        /// 是否更新了当前帧的 <see cref="FramePerSecond"/> 和 <see cref="TimePerFrame"/>。
        /// </summary>
        /// <value>如果该帧的 <see cref="FramePerSecond"/> 和 <see cref="TimePerFrame"/> 已更新，则为 <c>true</c>，否则为 <c>false</c></value>
        public bool FramePerSecondUpdated { get; private set; }

        public void Update(TimeSpan totalGameTime, TimeSpan elapsedGameTime, TimeSpan elapsedUpdateTime, bool isRunningSlowly,
        bool incrementFrameCount)
        {
            Total = totalGameTime;
            Elapsed = elapsedGameTime;
            IsRunningSlowly = isRunningSlowly;
            FramePerSecondUpdated = false;

            if (!incrementFrameCount)
                return;

            this.AccumulatedElapsedTime += elapsedGameTime;
            double accumulatedElapsedGameTimeInSecond = this.AccumulatedElapsedTime.TotalSeconds;
            if (this.AccumulatedFrameCountPerSecond > 0 && accumulatedElapsedGameTimeInSecond > 1.0)
            {
                this.TimePerFrame = TimeSpan.FromTicks(this.AccumulatedElapsedTime.Ticks / this.AccumulatedFrameCountPerSecond);
                this.FramePerSecond = (float)(this.AccumulatedFrameCountPerSecond / accumulatedElapsedGameTimeInSecond);
                this.AccumulatedElapsedTime = TimeSpan.Zero;
                this.AccumulatedFrameCountPerSecond = 0;
                this.FramePerSecondUpdated = true;
            }

            this.AccumulatedFrameCountPerSecond++;
            this.FrameCount++;
        }

        public void Reset(TimeSpan totalGameTime)
        {
            Update(totalGameTime, TimeSpan.Zero, TimeSpan.Zero, false, false);
            this.AccumulatedElapsedTime = TimeSpan.Zero;
            this.AccumulatedFrameCountPerSecond = 0;
            FrameCount = 0;
        }

        #endregion
    }
}