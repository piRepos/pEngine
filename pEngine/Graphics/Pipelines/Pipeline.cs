using System;

using pEngine.Utils.Math;
using pEngine.Graphics.Shading;
using pEngine.Graphics.Devices;

namespace pEngine.Graphics.Pipelines
{
	/// <summary>
	/// Manage the graphic pipeline initialization.
	/// </summary>
	public class Pipeline : IDisposable
	{

		/// <summary>
		/// Makes a new instance of <see cref="Pipeline"/> class.
		/// </summary>
		public Pipeline(GraphicDevice device, RenderPass pass, bool compute = false)
		{
			GraphicDevice = device;
			IsCompute = compute;
			RenderPass = pass;
		}

		/// <summary>
		/// The device on which this pipline will be binded to.
		/// </summary>
		protected GraphicDevice GraphicDevice { get; }

		/// <summary>
		/// Pipeline render pass.
		/// </summary>
		protected RenderPass RenderPass { get; }

		/// <summary>
		/// Gets or sets the shader program.
		/// </summary>
		public ShaderInstance Shader { get; set; }

		/// <summary>
		/// Render viewport.
		/// </summary>
		public Rect Viewport { get; set; }

		/// <summary>
		/// Framebuffer render size.
		/// </summary>
		public Vector2i BufferSize { get; set; }

		/// <summary>
		/// Gets if this pipeline is a compute pipeline.
		/// </summary>
		public bool IsCompute { get; }

		/// <summary>
		/// Initializes the render pipeline.
		/// </summary>
		public virtual void Initialize(Vector2i size)
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
				RenderPass?.Dispose();

				// Note disposing has been done.
				Disposed = true;
			}
		}

		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// </summary>
		~Pipeline()
		{
			Dispose(false);
		}

		#endregion
	}
}
