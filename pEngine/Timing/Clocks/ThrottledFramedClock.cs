using System;
using System.Threading;

namespace pEngine.Timing.Clocks
{
    using Math = System.Math;

    /// <summary>
    /// A FrameClock which will limit the number of frames processed by adding Thread.Sleep calls on each ProcessFrame.
    /// </summary>
    public class ThrottledFramedClock : FramedClock
    {
		/// <summary>
		/// The number of updated per second which is permitted.
		/// </summary>
		public double MaximumUpdateHz { get; set; } = 1000.0;

		/// <summary>
		/// If true, we will perform a Thread.Sleep even if the period is absolute zero.
		/// Allows other threads to process.
		/// </summary>
		public bool AlwaysSleep { get; set; } = true;

		/// <summary>
		/// Processes one frame. Generally should be run once per update loop.
		/// </summary>
		public override void ProcessFrame()
		{
			base.ProcessFrame();
			throttle();
		}

		#region Logic

		// - Minimum frame time based on maximum update frequency
		private double pMinimumFrameTime => 1000.0 / MaximumUpdateHz;

		// - Sleep error summ (used for time correction)
		private double pAccumulatedSleepError;

		private void throttle()
		{
			double targetMilliseconds = pMinimumFrameTime;
			int timeToSleepFloored = 0;

			//If we are limiting to a specific rate, and not enough time has passed for the next frame to be accepted we should pause here.
			if (targetMilliseconds > 0)
			{
				if (ElapsedFrameTime < targetMilliseconds)
				{
					// Using ticks for sleeping is pointless due to them being rounded to milliseconds internally anyways (in windows at least).
					double timeToSleep = targetMilliseconds - ElapsedFrameTime;
					timeToSleepFloored = (int)Math.Floor(timeToSleep);

					pAccumulatedSleepError += timeToSleep - timeToSleepFloored;
					int accumulatedSleepErrorCompensation = (int)Math.Round(pAccumulatedSleepError);

					// Can't sleep a negative amount of time
					accumulatedSleepErrorCompensation = Math.Max(accumulatedSleepErrorCompensation, -timeToSleepFloored);

					pAccumulatedSleepError -= accumulatedSleepErrorCompensation;
					timeToSleepFloored += accumulatedSleepErrorCompensation;

					// We don't want re-schedules with Thread.Sleep(0). We already have that case down below.
					if (timeToSleepFloored > 0)
						Thread.Sleep(timeToSleepFloored);

					// Sleep is not guaranteed to be an exact time. It only guaranteed to sleep AT LEAST the specified time. We also used some time to compute the above things, so this is also factored in here.
					double afterSleepTime = SourceTime;
					pAccumulatedSleepError += timeToSleepFloored - (afterSleepTime - CurrentTime);
					CurrentTime = afterSleepTime;
				}
				else
				{
					// We use the negative spareTime to compensate for framerate jitter slightly.
					double spareTime = ElapsedFrameTime - targetMilliseconds;
					pAccumulatedSleepError = -spareTime;
				}
			}

			// Call the scheduler to give lower-priority background processes a chance to do stuff.
			if (timeToSleepFloored == 0)
				Thread.Sleep(0);
		}

		#endregion
	}
}
