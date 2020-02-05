using System;
using System.Linq;
using System.Reflection;

using System.Collections.Generic;

using pEngine.Utils.Invocation;

namespace pEngine.Diagnostic.Timing
{


	public class PerformanceCollector : IExecutionTimeData, IEnumerable<IExecutionTimeData>
	{
		/// <summary>
		/// Makes a new instance of <see cref="PerformanceCollector"/> class.
		/// </summary>
		/// <param name="name">Collector name.</param>
		public PerformanceCollector(string name)
		{
			Name = name;
			Created = DateTime.Now;
			pCollections = new List<IExecutionTimeData>();
		}

		/// <summary>
		/// Number of samples per collection.
		/// </summary>
		static public int SamplesCount { get; private set; } = 20;

		/// <summary>
		/// Occurs when a new performance is created.
		/// </summary>
		public event EventHandler<IExecutionTimeData> OnNewPerformance;

		/// <summary>
		/// Occurs when a collector starts to collect.
		/// </summary>
		public event EventHandler<IExecutionTimeData> OnStartCollect;

		#region Collections

		// - This list contains all data collectors
		private List<IExecutionTimeData> pCollections;

		/// <summary>
		/// Search an existent collection.
		/// </summary>
		/// <returns>The collection found (null if not found).</returns>
		/// <param name="name">Name of the collection to search.</param>
		public IExecutionTimeData GetCollection(string name) =>
			pCollections.Find(X => X.Name == name);


		/// <summary>
		/// Starts the collecting a time for the specified performance.
		/// !!!! To use in a using block !!!!
		/// </summary>
		/// <returns>An object to dispose for stop the collection.</returns>
		/// <param name="Performance">Performance to collect.</param>
		public InvokeOnDisposal StartCollect(string performance)
		{
			// - Create a new collector
			var collector = CreateCollection(performance);

			// - Invoke the "new collection" event
			OnStartCollect?.Invoke(this, collector);

			// - Start collecting data
			collector.Start();

			// - Stop collecting data once the scope delete this object
			return new InvokeOnDisposal(() => collector.Stop());
		}

		/// <summary>
		/// Creates a new collection.
		/// </summary>
		/// <returns>The collection created.</returns>
		/// <param name="performance">Performance collection name.</param>
		public ExecutionTimeDataCollector CreateCollection(string performance)
		{
			// - Check if the collector already exists
			var collector = GetCollection(performance);

			// - Otherwise we need to create a new collector
			if (collector == null)
			{
				// - Create a new collector
				collector = new ExecutionTimeDataCollector(performance, SamplesCount);

				// - Store this collector
				pCollections.Add(collector);

				// - Raise the collector creation event
				OnNewPerformance?.Invoke(this, collector);
			}

			return collector as ExecutionTimeDataCollector;
		}

		#endregion

		#region Collectors

		/// <summary>
		/// Creates a new collection.
		/// </summary>
		/// <returns>The collection created.</returns>
		/// <param name="performance">Performance collection name.</param>
		/// <param name="collectOnRelease">If set to <c>true</c> collect on release build else not.</param>
		public PerformanceCollector CreateCollector(string performance)
		{
			// - Check if the collection already exists
			var collection = GetCollection(performance);

			// - If it doesn't exists, create a new one
			if (collection == null)
			{
				// - Create a new collection
				collection = new PerformanceCollector(performance);

				// - Add it in the local storage
				pCollections.Add(collection);

				// - Raise collector event creation
				OnNewPerformance?.Invoke(this, collection);
			}

			return collection as PerformanceCollector;
		}

		/// <summary>
		/// Add an existing collector to this collector.
		/// </summary>
		/// <param name="collector">Collector to add.</param>
		public void AddCollector(PerformanceCollector collector)
		{
			if (!pCollections.Contains(collector))
			{
				pCollections.Add(collector);
			}
		}

		#endregion

		#region Collector

		/// <summary>
		/// The collection name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Collection creation time.
		/// </summary>
		public DateTime Created { get; }

		/// <summary>
		/// Gets the average time for this performance.
		/// </summary>
		public TimeSpan Average => new TimeSpan((long)pCollections.Average(p => p.Average.Ticks));

		/// <summary>
		/// Gets the minimum time.
		/// </summary>
		public TimeSpan Min => new TimeSpan(pCollections.Min(p => p.Average.Ticks));

		/// <summary>
		/// Gets the maximum time.
		/// </summary>
		public TimeSpan Max => new TimeSpan(pCollections.Max(p => p.Average.Ticks));

		/// <summary>
		/// Gets the sum time.
		/// </summary>
		public TimeSpan Sum => new TimeSpan(pCollections.Sum(p => p.Average.Ticks));

		#endregion

		#region IEnumerable

		/// <summary>
		/// Access to a collection by key
		/// </summary>
		/// <param name="key">Key.</param>
		/// <returns>Performance collection.</returns>
		public IExecutionTimeData this[string key]
		{
			get
			{
				return pCollections.Find(x => x.Name == key);
			}
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<IExecutionTimeData> GetEnumerator()
		{
			return pCollections.GetEnumerator();
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion
	}
}
