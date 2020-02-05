using System;

using pEngine.Timing.Base;

namespace pEngine.Timing.Clocks
{
    using Math = System.Math;

    /// <summary>
    /// A clock which uses an internal stopwatch to interpolate (smooth out) a source.
    /// Note that this will NOT function unless a source has been set.
    /// </summary>
    public class InterpolatedFramedClock : IFrameBasedClock
    {
        /// <summary>
        /// Makes a new hinstance of <see cref="InterpolatedFramedClock"/>.
        /// </summary>
        /// <param name="source">Source clock.</param>
        public InterpolatedFramedClock(IClock source = null)
        {
            pClock = new FramedClock(new StopwatchClock(true));
            ChangeSource(source);
        }

        // - Internal clock
        private FramedClock pClock;

        /// <summary>
        /// Source clock.
        /// </summary>
        protected FramedClock SourceClock { get; set; }

        /// <summary>
        /// Changes che clock source.
        /// </summary>
        /// <param name="source">New source clock.</param>
        public void ChangeSource(IClock source)
        {
            SourceClock = new FramedClock(source);
            LastInterpolatedTime = 0;
            CurrentInterpolatedTime = 0;
        }

        /// <summary>
        /// Last interpolation time
        /// </summary>
        protected double LastInterpolatedTime { get; set; }

        /// <summary>
        /// Current time
        /// </summary>
        protected double CurrentInterpolatedTime { get; set; }

        /// <summary>
        /// The current time of this clock, in milliseconds.
        /// </summary>
        public virtual double CurrentTime => SourceClock.IsRunning ? CurrentInterpolatedTime : SourceClock.CurrentTime;

        /// <summary>
        /// Error treshold.
        /// </summary>
        public double AllowableErrorMilliseconds = 1000.0 / 60 * 2;

        /// <summary>
        /// The rate this clock is running at.
        /// </summary>
        public double Rate => SourceClock.Rate;

        /// <summary>
        /// Whether this clock is running.
        /// </summary>
        public virtual bool IsRunning => SourceClock.IsRunning;

        /// <summary>
        /// Difference between source clock and actual clock.
        /// </summary>
        public virtual double Drift => CurrentTime - SourceClock.CurrentTime;

        /// <summary>
        /// Elapsed time from last process frame.
        /// </summary>
        public virtual double ElapsedFrameTime => CurrentInterpolatedTime - LastInterpolatedTime;

        /// <summary>
        /// Processes one frame. Generally should be run once per update loop.
        /// </summary>
        public virtual void ProcessFrame()
        {
            if (SourceClock == null) return;

            pClock.ProcessFrame();
            SourceClock.ProcessFrame();

            LastInterpolatedTime = CurrentTime;

            if (!SourceClock.IsRunning)
                return;

            CurrentInterpolatedTime += pClock.ElapsedFrameTime * Rate;

            if (Math.Abs(SourceClock.CurrentTime - CurrentInterpolatedTime) > AllowableErrorMilliseconds)
            {
                // -If we've exceeded the allowable error, we should use the source clock's time value.
                CurrentInterpolatedTime = SourceClock.CurrentTime;
            }
            else
            {
                // - If we differ from the elapsed time of the source, let's adjust for the difference.
                CurrentInterpolatedTime += (SourceClock.CurrentTime - CurrentInterpolatedTime) / 8;
            }
        }
    }
}
