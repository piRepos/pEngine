using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace pEngine.Diagnostic.Timing
{
	/// <summary>
	/// This class works as a stopwatch for data performance collection.
	/// </summary>
	public class ExecutionTimeDataCollector : Stopwatch, IExecutionTimeData
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="ExecutionTimeDataCollector"/> class.
		/// </summary>
		/// <param name="name">Performance name.</param>
		/// <param name="cacheSize">How many values must be stored.</param>
		public ExecutionTimeDataCollector(string name, int cacheSize)
		{
			Name = name;

			pCacheSize = cacheSize;

			Created = DateTime.Now;

			Min = TimeSpan.MaxValue;
			Max = TimeSpan.MinValue;

			pCollectedValues = new List<TimeSpan>();
		}

		/// <summary>
		/// The collection name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Number of samples.
		/// </summary>
		public int SampleCount => pCollectedValues.Count;

		/// <summary>
		/// Collection creation time.
		/// </summary>
		public DateTime Created { get; }

		/// <summary>
		/// Gets the average time for this performance.
		/// </summary>
		public TimeSpan Average => new TimeSpan(pCollectedValues.Sum(X => X.Ticks) / pCollectedValues.Count);

		/// <summary>
		/// Gets the minimum time.
		/// </summary>
		public TimeSpan Min { get; private set; }

		/// <summary>
		/// Gets the maximum time.
		/// </summary>
		public TimeSpan Max { get; private set; }

		/// <summary>
		/// Gets the sum time.
		/// </summary>
		public TimeSpan Sum { get; private set; }

		#region Management

		// - Maximum collection size
		private int pCacheSize;

		// - Time slices collection
		private List<TimeSpan> pCollectedValues;

		/// <summary>
		/// Start the performance collection.
		/// </summary>
		internal new void Start()
		{
			base.Start();
		}

		/// <summary>
		/// Stop the performance collection.
		/// </summary>
		internal new void Stop()
		{
			base.Stop();

			Min = Elapsed > Min ? Min : Elapsed;
			Max = Elapsed > Max ? Elapsed : Max;
			Sum = Elapsed;

			pCollectedValues.Add(Elapsed);

			if (pCollectedValues.Count > pCacheSize)
				pCollectedValues.RemoveAt(0);

			base.Reset();
		}

		/// <summary>
		/// Restart the data collection and reset all values.
		/// </summary>
		internal new void Reset()
		{
			Stop();
			Start();

			pCollectedValues.Clear();

			Min = TimeSpan.MaxValue;
			Max = TimeSpan.MinValue;
			Sum = TimeSpan.Zero;
		}

		#endregion
	}
}
