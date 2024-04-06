/**
 * Code from Xenko GameTime
 */

using System;

namespace ET
{
    /// <summary>
    /// 用于可变步长(实时)或固定步长(游戏时间)游戏的当前计时。
    /// Current timing used for variable-step (real time) or fixed-step (game time) games.
    /// </summary>
    public class GameTime
    {
        // 累计流逝时间
        private TimeSpan _accumulatedElapsedTime;

        // 每秒累计帧计数
        private int _accumulatedFrameCountPerSecond;

        #region 构造函数和析构函数

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTime" /> class.
        /// </summary>
        public GameTime()
        {
            _accumulatedElapsedTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTime" /> class.
        /// </summary>
        /// <param name="totalTime">自游戏开始以来的总游戏时间。</param>
        /// <param name="elapsedTime">自上次更新以来经过的游戏时间。</param>
        public GameTime(TimeSpan totalTime, TimeSpan elapsedTime)
        {
            Total = totalTime;
            Elapsed = elapsedTime;
            _accumulatedElapsedTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTime" /> class.
        /// </summary>
        /// <param name=“totalTime”>自游戏开始以来的总游戏时间。</param>。
        /// <param name=“elapsedTime”>自上次更新以来经过的游戏时间。</param>。
        /// <param name=“isRunningSlowly”>如果游戏运行异常缓慢，则为True。</param>
        public GameTime(TimeSpan totalTime, TimeSpan elapsedTime, bool isRunningSlowly)
        {
            Total = totalTime;
            Elapsed = elapsedTime;
            IsRunningSlowly = isRunningSlowly;
            _accumulatedElapsedTime = TimeSpan.Zero;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the elapsed game time since the last update
        /// </summary>
        /// <value>The elapsed game time.</value>
        public TimeSpan Elapsed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the game is running slowly than its TargetElapsedTime. This can be used for example to render less details...etc.
        /// </summary>
        /// <value><c>true</c> if this instance is running slowly; otherwise, <c>false</c>.</value>
        public bool IsRunningSlowly { get; private set; }

        /// <summary>
        /// Gets the amount of game time since the start of the game.
        /// </summary>
        /// <value>The total game time.</value>
        public TimeSpan Total { get; private set; }

        /// <summary>
        /// Gets the current frame count since the start of the game.
        /// </summary>
        public int FrameCount { get; private set; }

        /// <summary>
        /// Gets the number of frame per second (FPS) for the current running game.
        /// </summary>
        /// <value>The frame per second.</value>
        public float FramePerSecond { get; private set; }

        /// <summary>
        /// Gets the time per frame.
        /// </summary>
        /// <value>The time per frame.</value>
        public TimeSpan TimePerFrame { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="FramePerSecond"/> and <see cref="TimePerFrame"/> were updated for this frame.
        /// </summary>
        /// <value><c>true</c> if the <see cref="FramePerSecond"/> and <see cref="TimePerFrame"/> were updated for this frame; otherwise, <c>false</c>.</value>
        public bool FramePerSecondUpdated { get; private set; }

        internal void Update(TimeSpan totalGameTime, TimeSpan elapsedGameTime, TimeSpan elapsedUpdateTime, bool isRunningSlowly, bool incrementFrameCount)
        {
            Total = totalGameTime;
            Elapsed = elapsedGameTime;
            IsRunningSlowly = isRunningSlowly;
            FramePerSecondUpdated = false;

            if (!incrementFrameCount)
                return;

            this._accumulatedElapsedTime += elapsedGameTime;
            double accumulatedElapsedGameTimeInSecond = this._accumulatedElapsedTime.TotalSeconds;
            if (this._accumulatedFrameCountPerSecond > 0 && accumulatedElapsedGameTimeInSecond > 1.0)
            {
                this.TimePerFrame = TimeSpan.FromTicks(this._accumulatedElapsedTime.Ticks / this._accumulatedFrameCountPerSecond);
                this.FramePerSecond = (float) (this._accumulatedFrameCountPerSecond / accumulatedElapsedGameTimeInSecond);
                this._accumulatedFrameCountPerSecond = 0;
                this._accumulatedElapsedTime = TimeSpan.Zero;
                this.FramePerSecondUpdated = true;
            }

            this._accumulatedFrameCountPerSecond++;
            this.FrameCount++;
        }

        internal void Reset(TimeSpan totalGameTime)
        {
            Update(totalGameTime, TimeSpan.Zero, TimeSpan.Zero, false, false);
            _accumulatedElapsedTime = TimeSpan.Zero;
            _accumulatedFrameCountPerSecond = 0;
            FrameCount = 0;
        }

        #endregion
    }
}