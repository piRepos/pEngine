using System;

using pEngine.Environment;
using pEngine.Environment.Video;

namespace pEngine.Graphics
{
	/// <summary>
	/// A generic base class for a graphic library implementation.
	/// </summary>
	public class GraphicLibrary : IDisposable
	{
		/// <summary>
		/// Makes an instance of <see cref="GraphicLibrary"/> class.
		/// </summary>
		public GraphicLibrary()
		{

		}

		/// <summary>
		/// Initializes the graphic library using the specified surface target.
		/// </summary>
		/// <param name="targetSurface">The surface which the library will target.</param>
		public virtual void Initialize(ISurface targetSurface)
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
		~GraphicLibrary()
		{
			Dispose(false);
		}

		#endregion
	}
}
