using System.Runtime.InteropServices;
using System.IO;
using System;

namespace pEngine.Environment
{
	public static class Platform
	{
		#region Operating System

		/// <summary>
		/// Force os flag.
		/// </summary>
		private static OS pForceOS = OS.Unknow;

		/// <summary>
		/// All supported platform.
		/// </summary>
		public enum OS
		{
			/// <summary>
			/// Unknow os, when the current runtime doesn't
			/// match all the other values.
			/// </summary>
			Unknow = 0x0,

			/// <summary>
			/// Microsoft windows platform.
			/// </summary>
			Windows = 0x1,

			/// <summary>
			/// Apple mac osx platform.
			/// </summary>
			OSX = 0x2,

			/// <summary>
			/// Any linux distro platform.
			/// </summary>
			Linux = 0x3,

			/// <summary>
			/// Iphone/Ipad os platform.
			/// </summary>
			IOS = 0x4,

			/// <summary>
			/// Android smartphone/tablet platform.
			/// </summary>
			Android = 0x5
		}

		/// <summary>
		/// Force pEngine to work in a specific operaring system.
		/// </summary>
		/// <param name="target"><see cref="OS.Unknow"/> sets automatic recognition.</param>
		public static void ForceOS(OS target)
		{
			pForceOS = target;
		}

		/// <summary>
		/// Check the current operating system.
		/// </summary>
		/// <returns>An <see cref="OS"/> matching the current runtime platform.</returns>
		public static OS CurrentOS()
		{
			if (pForceOS != OS.Unknow)
				return pForceOS;

			if (IsAndroid())
				return OS.Android;

			if (IsIOS())
				return OS.IOS;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return OS.Windows;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return OS.OSX;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				return OS.Linux;

 			return OS.Unknow;
		}

		private static bool IsAndroid()
		{
			if (File.Exists(@"/proc/sys/kernel/ostype"))
			{
				string osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
				if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
				{
					var arch = RuntimeInformation.OSArchitecture;
					switch (arch)
					{
						case Architecture.Arm:
						case Architecture.Arm64:
							return true;
					}
				}
			}

			return false;
		}

		private static bool IsIOS()
		{
			if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
			{
				var arch = RuntimeInformation.OSArchitecture;
				switch (arch)
				{
					case Architecture.Arm:
					case Architecture.Arm64:
						return true;
				}

			}

			return false;
		}

		#endregion
	}
}
