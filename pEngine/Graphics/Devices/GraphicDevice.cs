﻿using System;

using pEngine.Environment.Video;

namespace pEngine.Graphics.Devices
{
	/// <summary>
	/// A virtual device which allows rendering.
	/// </summary>
	public class GraphicDevice : IDisposable
	{
		/// <summary>
		/// Makes a new instance of <see cref="GraphicDevice"/> class.
		/// </summary>
		public GraphicDevice(PhysicalDevice device)
		{
			Physical = device;
		}

		/// <summary>
		/// Associated physical device.
		/// </summary>
		public PhysicalDevice Physical { get; }

		/// <summary>
		/// Initialize the graphic device.
		/// </summary>
		public virtual void Initialize()
		{
			Disposed = false;
		}

		#region Dispose

		/// <summary>
		/// Dispose logic variable.
		/// </summary>
		public bool Disposed { get; protected set; }

		/// <summary>
		/// Implement IDisposable.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			// - This object will be cleaned up by the Dispose method.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose(bool disposing) executes in two distinct scenarios.
		/// If disposing equals <see cref="true"/>, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// If disposing equals <see cref="false"/>, the method has been called by the
		/// runtime from inside the finalizer and you should not reference
		/// other objects. Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"><see cref="True"/> if called from user's code.</param>
		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!Disposed)
			{
				// Note disposing has been done.
				Disposed = true;
			}
		}

		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// </summary>
		~GraphicDevice()
		{
			Dispose(false);
		}

		#endregion
	}
}
