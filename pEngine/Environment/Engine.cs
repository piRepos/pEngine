using System;
using System.Reflection;

namespace pEngine.Environment
{
	/// <summary>
	/// Contains all current game / engine metadata and static
	/// information.
	/// </summary>
	public static class Engine
	{
		/// <summary>
		/// Gets the pEngine version.
		/// </summary>
		public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		/// <summary>
		/// Gets the running game version.
		/// </summary>
		public static Version GameVersion => Assembly.GetEntryAssembly().GetName().Version;

		/// <summary>
		/// The game engine name.
		/// </summary>
		public static string Name => "pEngine";

		/// <summary>
		/// The running game name.
		/// </summary>
		public static string GameName { get; internal set; }

	}
}
