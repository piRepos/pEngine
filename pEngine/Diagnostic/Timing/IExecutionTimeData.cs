using System;
using System.Collections.Generic;
using System.Text;

namespace pEngine.Diagnostic.Timing
{
	/// <summary>
	/// Execution time summary. 
	/// </summary>
	public interface IExecutionTimeData
	{
		/// <summary>
		/// The collection name.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Collection creation time.
		/// </summary>
		DateTime Created { get; }

		/// <summary>
		/// Gets the average time for this performance.
		/// </summary>
		TimeSpan Average { get; }

		/// <summary>
		/// Gets the minimum time.
		/// </summary>
		TimeSpan Min { get; }

		/// <summary>
		/// Gets the maximum time.
		/// </summary>
		TimeSpan Max { get; }

		/// <summary>
		/// Gets the sum time.
		/// </summary>
		TimeSpan Sum { get; }
	}
}
