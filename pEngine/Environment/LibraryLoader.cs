using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace pEngine.Windows.Runtime
{
	/// <summary>
	/// Helper that provides a platform indipendent way
	/// to manually load dynamic libraries.
	/// </summary>
	public static class LibraryLoader
	{
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        /// <summary>
        /// Sets the searching paths for external libraries.
        /// </summary>
        public static void Initialize()
		{
            SetDllDirectory("Binaries");
		}

    }
}
