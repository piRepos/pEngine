using System;

using pEngine.Timing.Base;
using pEngine.Utils.Math;

namespace pEngine.Timing.Clocks
{
    using Math = System.Math;

    public class BeatClock : InterpolatedFramedClock, IBeatClock
	{
		/// <summary>
		/// Makes a new <see cref="BeatClock"/>.
		/// </summary>
		/// <param name="source">Source clock.</param>
		public BeatClock(IClock source)
			: base(source)
		{
			pLastTime = double.MaxValue;
		}

		/// <summary>
		/// Gets the current time.
		/// </summary>
		public override double CurrentTime => base.CurrentTime + Offset;

		/// <summary>
		/// Gets or sets the time offset in ms.
		/// </summary>
		public double Offset { get; set; }

		/// <summary>
		/// Gets or sets the beats per minute.
		/// </summary>
		public double BeatsPerMinute { get; set; }

		/// <summary>
		/// Gets or sets the signature.
		/// </summary>
		public TimeSignature Signature { get; set; }

		/// <summary>
		/// Gets or sets the time snap divisor.
		/// </summary>
		public int Divisor { get; set; }

		/// <summary>
		/// Occurs when there's a normal beat in the signature.
		/// </summary>
		public event Action OnLowBeat;

		/// <summary>
		/// Occurs when there's an accented beat in the signature.
		/// </summary>
		public event Action OnUpBeat;

		#region Update

		// - Last registered click
		private double pLastTime;

		/// <summary>
		/// Processes one frame. Generally should be run once per update loop.
		/// </summary>
		public override void ProcessFrame()
		{
			base.ProcessFrame();

			double MeasureDuration = (60D / BeatsPerMinute) * 4 / Signature.BeatSize;

			if (pLastTime > MathHelpers.Mod(CurrentTime, MeasureDuration * 4 / Divisor))
			{
				if (Math.Floor(MathHelpers.Mod(CurrentTime / MeasureDuration, Signature.BeatSize)) == 0)
					OnUpBeat?.Invoke();
				else OnLowBeat?.Invoke();
			}

			pLastTime = MathHelpers.Mod(CurrentTime, MeasureDuration * 4 / Divisor);

		}

		

		#endregion
	}
}
