using System;
using System.Collections.Generic;
using System.Text;

namespace pEngine.Diagnostic
{
	/// <summary>
	/// Manages pEngine debug and diagnostic information.
	/// </summary>
	public static class Debug
	{
		/// <summary>
		/// If the current assembly is in debug mode.
		/// </summary>
		public static bool DebugMode { get; private set; }

		/// <summary>
		/// General debug level option.
		/// </summary>
		public static DebugLevel DebugLevel { get; private set; }

		/// <summary>
		/// Game debug level option (game assembly).
		/// </summary>
		public static DebugLevel GameDebugLevel { get; private set; }

		/// <summary>
		/// Audio thread debug level option.
		/// </summary>
		public static DebugLevel AudioDebugLevel { get; private set; }

		/// <summary>
		/// Input thread debug level option.
		/// </summary>
		public static DebugLevel InputDebugLevel { get; private set; }

		/// <summary>
		/// Update thread debug level option.
		/// </summary>
		public static DebugLevel FrameworkDebugLevel { get; private set; }

		/// <summary>
		/// Renderer debug level option.
		/// </summary>
		public static DebugLevel RendererDebugLevel { get; private set; } = DebugLevel.Critical;

		#region Module initialization

		/// <summary>
		/// Debugger initialization state.
		/// </summary>
		public static bool Initialized { get; private set; }

		/// <summary>
		/// Initialize the debugger.
		/// </summary>
		public static void Initialize()
		{
			// - Nothing to do here
			if (Initialized) return;

			#if DEBUG
				DebugMode = true;
			#else
				DebugMode = false;
			#endif


			// - We finished initialization
			Initialized = true;
		}

		#endregion

	}
}
