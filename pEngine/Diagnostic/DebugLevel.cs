using System;
using System.Collections.Generic;
using System.Text;

namespace pEngine.Diagnostic
{
	/// <summary>
	/// Generic debug level flags.
	/// </summary>
	[Flags]
	public enum DebugLevel
	{
		/// <summary>
		/// No debug informations.
		/// </summary>
		None = 0,

		/// <summary>
		/// Checks only critical errors.
		/// </summary>
		Critical = 1,

		/// <summary>
		/// Logs inconsistent states that may cause errors.
		/// </summary>
		Warning = 2,

		/// <summary>
		/// Checks for performance issues.
		/// </summary>
		Performance = 4,

		/// <summary>
		/// Handles all debug messages.
		/// </summary>
		Debug = 8,

		/// <summary>
		/// Verbose mode, may cause performance issues due to debug overhead.
		/// </summary>
		Info = 16,

		/// <summary>
		/// Enables all debug flags.
		/// </summary>
		All = Critical | Warning | Performance | Debug | Info
	}
}
