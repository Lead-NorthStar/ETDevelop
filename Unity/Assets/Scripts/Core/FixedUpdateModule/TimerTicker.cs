/**
 * Code from Xenko TimerTick
 */

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

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerTick"/> class.
        /// </summary>
        public TimerTick()
        {
            speedFactor = 1.0m;
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerTick" /> class.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        public TimerTick(TimeSpan startTime)
        {
            speedFactor = 1.0m;
            Reset(startTime);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 获取创建此计时器时的开始时间。
        /// </summary>
        public TimeSpan StartTime { get; private set; }

        /// <summary>
        /// 获取自上次重置或创建此计时器以来经过的总时间。
        /// </summary>
        public TimeSpan TotalTime { get; private set; }

        /// <summary>
        /// 获取上次重置或创建此定时器后所用的总时间，包括 <see cref="Pause"/>
        /// </summary>
        public TimeSpan TotalTimeWithPause { get; private set; }

        /// <summary>
        /// 获取自上次调用 <see cref="Tick"/> 后经过的时间。
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }

        /// <summary>
        /// 获取自上次调用 <see cref="Tick"/> 包括 <see cref="Pause"/> 后所用的时间
        /// </summary>
        public TimeSpan ElapsedTimeWithPause { get; private set; }

        /// <summary>
        /// 获取或设置速度因子。默认为 1.0
        /// </summary>
        /// <value>The speed factor.</value>
        public double SpeedFactor
        {
            get { return (double) speedFactor; }
            set { speedFactor = (decimal) value; }
        }

        /// <summary>
        /// 获取表示该实例是否暂停的值。
        /// </summary>
        /// <value><c>true</c> if this instance is paused; otherwise, <c>false</c>.</value>
        public bool IsPaused
        {
            get { return pauseCount > 0; }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Resets this instance. <see cref="TotalTime"/> is set to zero.
        /// </summary>
        public void Reset()
        {
            Reset(TimeSpan.Zero);
        }

        /// <summary>
        /// Resets this instance. <see cref="TotalTime" /> is set to startTime.
        /// </summary>
        /// <param name="startTime">The start time.</param>
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
        /// Resumes this instance, only if a call to <see cref="Pause"/> has been already issued.
        /// </summary>
        public void Resume()
        {
            pauseCount--;
            if (pauseCount <= 0)
            {
                timePaused += Stopwatch.GetTimestamp() - pauseStartTime;
                pauseStartTime = 0L;
            }
        }

        /// <summary>
        /// 更新 <see cref="TotalTime"/> 和 <see cref="ElapsedTime"/>
        /// </summary>
        /// <remarks>
        /// This method must be called on a regular basis at every *tick*.
        /// </remarks>
        public void Tick()
        {
            // Don't tick when this instance is paused.
            if (IsPaused)
            {
                ElapsedTime = TimeSpan.Zero;
                return;
            }

            long rawTime = Stopwatch.GetTimestamp();
            TotalTime = StartTime + new TimeSpan((long) Math.Round(ConvertRawToTimestamp(rawTime - timePaused - startRawTime).Ticks * speedFactor));
            TotalTimeWithPause = StartTime + new TimeSpan((long) Math.Round(ConvertRawToTimestamp(rawTime - startRawTime).Ticks * speedFactor));

            ElapsedTime = ConvertRawToTimestamp(rawTime - timePaused - lastRawTime);
            ElapsedTimeWithPause = ConvertRawToTimestamp(rawTime - lastRawTime);

            if (ElapsedTime < TimeSpan.Zero)
            {
                ElapsedTime = TimeSpan.Zero;
            }

            lastRawTime = rawTime;
        }

        /// <summary>
        /// Pauses this instance.
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