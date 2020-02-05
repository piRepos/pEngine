using System;

using pEngine.Timing.Base;

namespace pEngine.Timing.Clocks
{
	/// <summary>
	/// Takes a clock source and separates time reading on a per-frame level.
	/// The CurrentTime value will only change on initial construction and whenever ProcessFrame is run.
	/// </summary>
	public class FramedClock : IFrameBasedClock
	{

		/// <summary>
		/// Construct a new FramedClock with an optional source clock.
		/// </summary>
		/// <param name="source">
		///		A source clock which will be used as the backing time source. If null, a StopwatchClock will be created. 
		///		When provided, the CurrentTime of <see cref="Source" /> will be transferred instantly.
		///	</param>
		public FramedClock(IClock source = null)
		{
			if (source != null)
			{
				CurrentTime = LastFrameTime = source.CurrentTime;
				Source = source;
			}
			else Source = new StopwatchClock(true);

			FpsCalculationInterval = 250;
		}


		/// <summary>
		/// Source clock.
		/// </summary>
		public IClock Source { get; }

		/// <summary>
		/// Current average execution time for the frames.
		/// </summary>
		public double AverageFrameTime { get; private set; }

		/// <summary>
		/// Frames per seconds.
		/// </summary>
		public double FramesPerSecond { get; private set; }

		/// <summary>
		/// Elapsed time from clock start.
		/// </summary>
		public virtual double CurrentTime { get; protected set; }

		/// <summary>
		/// Last frame time executiom.
		/// </summary>
		protected virtual double LastFrameTime { get; set; }

		/// <summary>
		/// The rate this clock is running at.
		/// </summary>
		public double Rate => Source.Rate;

		/// <summary>
		/// Time elapsed from the last frame execution.
		/// </summary>
		public double ElapsedFrameTime => CurrentTime - LastFrameTime;

		/// <summary>
		/// <see cref="true"/> if this clock is running.
		/// </summary>
		public bool IsRunning => Source?.IsRunning ?? false;

		/// <summary>
		/// Whether we should run <see cref="ProcessFrame"/> on the underlying <see cref="Source"/> 
		/// (in the case it is an <see cref="IFrameBasedClock"/>).
		/// </summary>
		public bool ProcessSourceClockFrames { get; set; }

		/// <summary>
		/// Source time from <see cref="Source"/> clock.
		/// </summary>
		protected double SourceTime => Source.CurrentTime;

		/// <summary>
		/// Time between each FPS calculation.
		/// </summary>
		public int FpsCalculationInterval { get; set; }

		#region Calculation

		// - Remaining time for the next FPS calculation
		private double pTimeUntilNextCalculation;

		// - Time since last FPS calculation
		private double pTimeSinceLastCalculation;

		// - Frame count (FPS counter)
		private int pFramesSinceLastCalculation;

		/// <summary>
		/// Processes one frame. Generally should be run once per update loop.
		/// </summary>
		public virtual void ProcessFrame()
		{
			if (ProcessSourceClockFrames)
				(Source as IFrameBasedClock)?.ProcessFrame();

			if (pTimeUntilNextCalculation <= 0)
			{
				pTimeUntilNextCalculation += FpsCalculationInterval;

				if (pFramesSinceLastCalculation == 0)
					FramesPerSecond = 0;
				else
					FramesPerSecond = Math.Ceiling(pFramesSinceLastCalculation * 1000f / pTimeSinceLastCalculation);
				pTimeSinceLastCalculation = pFramesSinceLastCalculation = 0;
			}

			pFramesSinceLastCalculation++;
			pTimeUntilNextCalculation -= ElapsedFrameTime;
			pTimeSinceLastCalculation += ElapsedFrameTime;

			AverageFrameTime = Utils.Math.Interpolation.Damp(AverageFrameTime, ElapsedFrameTime, 0.01, Math.Abs(ElapsedFrameTime) / 1000);

			LastFrameTime = CurrentTime;
			CurrentTime = SourceTime;
		}

		#endregion
	}

}
