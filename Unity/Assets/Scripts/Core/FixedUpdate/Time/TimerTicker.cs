using System;
using System.Diagnostics;

namespace ET
{
    public class TimerTick
    {
        #region Fields

        private long startRawTime;
        private long lastRawTime;
        private int pauseCount;
        private long pauseStartTime;
        private long timePaused;
        private decimal speedFactor;

        #endregion

        #region 构造和析构函数

        public TimerTick()
        {
            speedFactor = 1.0m;
            Reset();
        }

        /// <param name="startTime">开始时间</param>
        public TimerTick(TimeSpan startTime)
        {
            speedFactor = 1.0m;
            Reset(startTime);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 创建此计时器时的开始时间。
        /// </summary>
        public TimeSpan StartTime { get; private set; }

        /// <summary>
        /// 自上次重置或创建此计时器以来经过的总时间。
        /// </summary>
        public TimeSpan TotalTime { get; private set; }

        /// <summary>
        /// 自上次重置或创建此定时器后所经过的总时间，包括 <see cref="Pause"/>
        /// </summary>
        public TimeSpan TotalTimeWithPause { get; private set; }

        /// <summary>
        /// 自上次调用 <see cref="Tick"/> 后经过了的时间。
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }

        /// <summary>
        /// 自上次调用 <see cref="Tick"/> 后所经过了的时间，包括 <see cref="Pause"/> 
        /// </summary>
        public TimeSpan ElapsedTimeWithPause { get; private set; }

        /// <summary>
        /// 速度系数，默认为 1.0
        /// </summary>
        public double SpeedFactor
        {
            get { return (double)speedFactor; }
            set { speedFactor = (decimal)value; }
        }

        /// <summary>
        /// 实例是否暂停。
        /// </summary>
        public bool IsPaused
        {
            get { return pauseCount > 0; }
        }

        #endregion

        #region 公共方法和运算符

        /// <summary>
        /// 重置此实例。<see cref="TotalTime"/> 设置为零。
        /// </summary>
        public void Reset()
        {
            Reset(TimeSpan.Zero);
        }

        /// <summary>
        /// 重置此实例。<see cref="TotalTime" /> 设置为 startTime。
        /// </summary>
        public void Reset(TimeSpan startTime)
        {
            StartTime = startTime;
            TotalTime = startTime;
            startRawTime = Stopwatch.GetTimestamp();
            lastRawTime = startRawTime;
            timePaused = 0;
            pauseStartTime = 0;
            pauseCount = 0;
        }

        /// <summary>
        /// 只有在已调用 <see cref="Pause"/> 的情况下，才能重新开始该实例。
        /// </summary>
        public void Resume()
        {
            pauseCount--;
            if (this.pauseCount > 0)
                return;

            this.timePaused += Stopwatch.GetTimestamp() - this.pauseStartTime;
            this.pauseStartTime = 0L;
        }

        /// <summary>
        /// 更新 <see cref="TotalTime"/> 和 <see cref="ElapsedTime"/>
        /// </summary>
        public void Tick()
        {
            // 当暂停时，不进行 tick
            if (IsPaused)
            {
                ElapsedTime = TimeSpan.Zero;
                return;
            }

            // 获取定时器的刻度
            long rawTime = Stopwatch.GetTimestamp();
            TotalTime = StartTime + new TimeSpan((long)Math.Round(ConvertRawToTimestamp(rawTime - timePaused - startRawTime).Ticks * speedFactor));
            TotalTimeWithPause = StartTime + new TimeSpan((long)Math.Round(ConvertRawToTimestamp(rawTime - startRawTime).Ticks * speedFactor));

            ElapsedTime = ConvertRawToTimestamp(rawTime - timePaused - lastRawTime);
            ElapsedTimeWithPause = ConvertRawToTimestamp(rawTime - lastRawTime);

            if (ElapsedTime < TimeSpan.Zero)
            {
                ElapsedTime = TimeSpan.Zero;
            }

            lastRawTime = rawTime;
        }

        /// <summary>
        /// 暂停此实例。
        /// </summary>
        public void Pause()
        {
            pauseCount++;
            if (pauseCount == 1)
            {
                pauseStartTime = Stopwatch.GetTimestamp();
            }
        }

        public static TimeSpan ConvertRawToTimestamp(long delta)
        {
            return new TimeSpan(delta == 0 ? 0 : (delta * TimeSpan.TicksPerSecond) / Stopwatch.Frequency);
        }

        #endregion
    }
}