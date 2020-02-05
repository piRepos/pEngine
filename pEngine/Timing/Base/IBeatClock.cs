using System;

namespace pEngine.Timing.Base
{

	/// <summary>
	/// Time signatures.
	/// </summary>
	public struct TimeSignature
	{
		/// <summary>
		/// Reapresent the upper value in a time signature, so
		/// how many beats fits in a bar.
		/// </summary>
		public int BarSize;

		/// <summary>
		/// Reapresent the lower value in a time signature.
		/// </summary>
		public int BeatSize;
	}

	/// <summary>
	/// A clock that has events for music timing.
	/// </summary>
	public interface IBeatClock : IClock
	{

		/// <summary>
		/// Gets or sets the beats BPM value.
		/// </summary>
		double BeatsPerMinute { get; set; }

		/// <summary>
		/// Gets or sets the time offset in ms.
		/// </summary>
		double Offset { get; set; }

		/// <summary>
		/// Time snap division. (more low beats)
		/// </summary>
		int Divisor { get; set; }

		/// <summary>
		/// Gets or sets the time signature.
		/// </summary>
		TimeSignature Signature { get; set; }

		/// <summary>
		/// Occurs when there's a normal beat in the signature.
		/// </summary>
		event Action OnLowBeat;

		/// <summary>
		/// Occurs when there's an accented beat in the signature.
		/// </summary>
		event Action OnUpBeat;
	}
}
