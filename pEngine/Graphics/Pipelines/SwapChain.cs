using System;
using System.Collections.Generic;

using pEngine.Utils.Math;
using pEngine.Graphics.Shading;
using pEngine.Graphics.Devices;
using pEngine.Graphics.FrameBuffers;

namespace pEngine.Graphics.Pipelines
{
	/// <summary>
	/// A virtual representation of a GPU's swapchain
	/// </summary>
	public class SwapChain : IDisposable
	{
		/// <summary>
		/// Makes a new instance of <see cref="SwapChain"/> class.
		/// </summary>
		/// <param name="device">Source device.</param>
		public SwapChain(PhysicalDevice card, GraphicDevice device)
		{
			RenderDevice = device;
			GraphicsCard = card;
		}

		/// <summary>
		/// Stored physical device.
		/// </summary>
		protected PhysicalDevice GraphicsCard { get; private set; }

		/// <summary>
		/// Stored working device.
		/// </summary>
		protected GraphicDevice RenderDevice { get; private set; }

		/// <summary>
		/// Active swap chain video format.
		/// </summary>
		public virtual SurfaceFormat CurrentFormat { get; }

		/// <summary>
		/// Video source formats.
		/// </summary>
		public virtual IEnumerable<SurfaceFormat> Formats { get; }

		/// <summary>
		/// Video surface size.
		/// </summary>
		public virtual Vector2i SurfaceSize { get; }

		/// <summary>
		/// Swapchain frame buffers.
		/// </summary>
		public virtual IList<FrameBuffer> VideoBuffers { get; }

		/// <summary>
		/// Initialize this swap chain on a specified surface.
		/// </summary>
		/// <param name="surface">Target video surface.</param>
		/// <param name="surfaceSize">Swap queue target size.</param>
		public virtual void Initialize(Surface surface, Vector2i surfaceSize)
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
		~SwapChain()
		{
			Dispose(false);
		}

		#endregion
	}
}
