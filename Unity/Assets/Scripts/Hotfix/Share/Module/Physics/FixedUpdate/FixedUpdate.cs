using System;
using System.Linq;

namespace ET
{
    [FriendOf(typeof(FixedUpdate))]
    [EntitySystemOf(typeof(FixedUpdate))]
    public static partial class FixedUpdateSystem
    {
        [EntitySystem]
        private static void Awake(this FixedUpdate self)
        {
            // Internals
            self.UpdateTime = new GameTime();
            self.PlayTimer = new TimerTick();
            self.UpdateTimer = new TimerTick();
            self.TotalUpdateTime = new TimeSpan();
            self.Timer = new TimerTick();
            self.MaximumElapsedTime = TimeSpan.FromMilliseconds(500.0);
            self.TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / FixedDefine.LogicFrame); 
            self.LastUpdateCount = new int[4];
            self.NextLastUpdateCountIndex = 0;

            // 计算 UpdateCountAverageSlowLimit (假如 moving 平均(average) >=3 )
            // 如果 moving 平均为 4:
            // UpdateCountAverageSlowLimit = (2 * 2 + (4 - 2)) / 4 = 1.5f
            const int BadUpdateCountTime = 2; // 坏帧数（坏帧是指至少有两次更新的帧）
            int maxLastCount = 2 * Math.Min(BadUpdateCountTime, self.LastUpdateCount.Length);
            self.UpdateCountAverageSlowLimit = (float)(maxLastCount + (self.LastUpdateCount.Length - maxLastCount)) / self.LastUpdateCount.Length;
        }

        [EntitySystem]
        private static void Update(this FixedUpdate self)
        {
            self.Tick();
        }

        /// <summary>
        /// 获取游戏开始时的当前更新时间。
        /// </summary>
        public static GameTime UpdateTime(this FixedUpdate self) { return self.UpdateTime; }

        /// <summary>
        /// 获取 play time，可更改为与当前渲染场景的时间一致。
        /// </summary>
        public static TimerTick PlayTime(this FixedUpdate self) { return self.PlayTimer; }

        /// <summary>
        /// 重置已用时间计数器。
        /// </summary>
        public static void ResetElapsedTime(this FixedUpdate self)
        {
            self.ForceElapsedTimeToZero = true;
            self.DrawRunningSlowly = false;
            Array.Clear(self.LastUpdateCount, 0, self.LastUpdateCount.Length);
            self.NextLastUpdateCountIndex = 0;
        }

        internal static void InitializeBeforeRun(this FixedUpdate self)
        {
            self.Timer.Reset();
            self.UpdateTime.Reset(self.TotalUpdateTime);

            // 首次运行更新
            self.UpdateTimer.Reset();
            self.Update(self.UpdateTime);

            self.UpdateTimer.Tick();
            self.SingleFrameUpdateTime += self.UpdateTimer.ElapsedTime;

            // 重置 PlayTime
            self.PlayTimer.Reset();
        }

        /// <summary>
        /// 更新游戏时钟，并调用 Update 和 Draw.
        /// </summary>
        public static void Tick(this FixedUpdate self)
        {
            lock (self.TickLock)
            {
                self.TickInternal();
            }
        }

        private static void TickInternal(this FixedUpdate self)
        {
            self.Timer.Tick();
            self.PlayTimer.Tick();
            // measuring update
            self.UpdateTimer.Reset();

            TimeSpan elapsedAdjustedTime = self.Timer.ElapsedTimeWithPause;

            if (self.ForceElapsedTimeToZero)
            {
                elapsedAdjustedTime = TimeSpan.Zero;
                self.ForceElapsedTimeToZero = false;
            }

            if (elapsedAdjustedTime > self.MaximumElapsedTime)
            {
                elapsedAdjustedTime = self.MaximumElapsedTime;
            }

            int updateCount = 1;

            // 如果舍入后的 TargetElapsedTime 等同于当前的 ElapsedAdjustedTime，则使 ElapsedAdjustedTime = TargetElapsedTime。
            // 我们采用与 XNA 相同的内部规则
            if (Math.Abs(elapsedAdjustedTime.Ticks - self.TargetElapsedTime.Ticks) < (self.TargetElapsedTime.Ticks >> 6))
            {
                elapsedAdjustedTime = self.TargetElapsedTime;
            }

            // 更新累计时间
            self.AccumulatedElapsedGameTime += elapsedAdjustedTime;

            // 计算要发布的更新次数

            updateCount = (int)(self.AccumulatedElapsedGameTime.Ticks / self.TargetElapsedTime.Ticks);
            if (updateCount == 0)
            {
                // 如果不需要更新，则退出
                return;
            }

            // Calculate a moving average on updateCount
            // 在 updateCount 上计算移动平均值
            self.LastUpdateCount[self.NextLastUpdateCountIndex] = updateCount;
            float updateCountMean = self.LastUpdateCount.Aggregate<int, float>(0, (current, t) => current + t);

            updateCountMean /= self.LastUpdateCount.Length;
            self.NextLastUpdateCountIndex = (self.NextLastUpdateCountIndex + 1) % self.LastUpdateCount.Length;

            // 在运行缓慢时进行测试
            self.DrawRunningSlowly = updateCountMean > self.UpdateCountAverageSlowLimit;

            // We are going to call Update updateCount times, so we can substract this from accumulated elapsed game time
            // 我们将调用 Update updateCount（更新次数），这样就可以从累积的游戏时间中减去更新次数
            self.AccumulatedElapsedGameTime = new TimeSpan(self.AccumulatedElapsedGameTime.Ticks - (updateCount * self.TargetElapsedTime.Ticks));
            TimeSpan singleFrameElapsedTime = self.TargetElapsedTime;

            // 重置下一帧的时间
            for (self.LastFrameElapsedGameTime = TimeSpan.Zero; updateCount > 0; updateCount--)
            {
                self.UpdateTime.Update(self.TotalUpdateTime, singleFrameElapsedTime, self.SingleFrameUpdateTime, self.DrawRunningSlowly, true);
                try
                {
                    self.UpdateAndProfile(self.UpdateTime);
                }
                finally
                {
                    self.LastFrameElapsedGameTime += singleFrameElapsedTime;
                    self.TotalUpdateTime += singleFrameElapsedTime;
                }
            }

            // End measuring update time
            self.UpdateTimer.Tick();
            self.SingleFrameUpdateTime = TimeSpan.Zero;
        }

        #region Methods

        private static void Update(this FixedUpdate self, GameTime gameTime)
        {
            try
            {
                self.Fiber().EntitySystem.FixedUpdate(gameTime);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            self.LastFrameElapsedGameTime = TimeSpan.Zero;
        }

        private static void UpdateAndProfile(this FixedUpdate self, GameTime gameTime)
        {
            self.UpdateTimer.Reset();
            self.Update(gameTime);
            self.UpdateTimer.Tick();
            self.SingleFrameUpdateTime += self.UpdateTimer.ElapsedTime;
            self.LastFrameElapsedGameTime = TimeSpan.Zero;
        }

        #endregion
    }
}