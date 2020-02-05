using System;
using System.Collections.Generic;

using pEngine.Framework;
using pEngine.Framework.Geometry;

using pEngine.Environment.Video;
using pEngine.Graphics.Shading;
using pEngine.Graphics.Devices;
using pEngine.Graphics.Data;

namespace pEngine.Graphics
{
	public abstract class Renderer : IDisposable
	{
		/// <summary>
		/// Makes a new instance of <see cref="Renderer"/> class.
		/// </summary>
		public Renderer(ISurface videoSurface)
		{
			TargetSurface = videoSurface;
		}

		/// <summary>
		/// Window or surface where the renderer must work.
		/// </summary>
		protected ISurface TargetSurface { get; }

		/// <summary>
		/// Contains the instance of graphic library.
		/// </summary>
		protected GraphicLibrary Library { get; set; }

		/// <summary>
		/// Graphics card or device that allows drawings on a screen.
		/// </summary>
		protected PhysicalDevice PhysicalDevice { get; set; }

		/// <summary>
		/// An abstraction layer from the physical video device.
		/// </summary>
		protected GraphicDevice GraphicDevice { get; set; }

		/// <summary>
		/// Loads and store all game's shaders.
		/// </summary>
		public ShaderManager ShaderManager { get; protected set; }

		/// <summary>
		/// Manages raw vertex allocation and the GPU buffer transfer.
		/// </summary>
		public VertexBuffer VertexManager { get; protected set; }

		/// <summary>
		/// Manages raw index allocation and the GPU buffer transfer.
		/// </summary>
		public IndexBuffer IndexManager { get; protected set; }

		/// <summary>
		/// Initialize the graphic library.
		/// </summary>
		public virtual void Initialize()
		{

		}

		/// <summary>
		/// Load initial resources.
		/// </summary>
		public virtual void LoadResources()
		{
			// - Loads essential shaders
			ShaderManager.LoadShader<DefaultShader>();

			var Rectangle = VertexManager.Alloc(4);

			Rectangle[0] = new VertexData(new Vector3(-0.5F, -0.5F, 0F), Color4.Black);
			Rectangle[1] = new VertexData(new Vector3( 0.5F, -0.5F, 0F), Color4.Black);
			Rectangle[2] = new VertexData(new Vector3( 0.5F,  0.5F, 0F), Color4.Black);
			Rectangle[3] = new VertexData(new Vector3(-0.5F,  0.5F, 0F), Color4.Black);

			Rectangle.Invalidate();

			var Indexes = IndexManager.Alloc(6);

			Indexes[0] = 0;
			Indexes[1] = 1;
			Indexes[2] = 2;
			Indexes[3] = 2;
			Indexes[4] = 3;
			Indexes[5] = 0;

			Indexes.Invalidate();
		}

		/// <summary>
		/// Configure the render for rendering.
		/// </summary>
		public virtual void ConfigureRendering()
		{

		}

		#region Invalidation

		/// <summary>
		/// This call configure the graphic library for the new device instance.
		/// </summary>
		public virtual void InvalidateDevice()
		{

		}

		/// <summary>
		/// Configure the graphic library to work with a new surface size.
		/// </summary>
		public virtual void InvalidateGraphics()
		{

		}

		#endregion

		#region Pipeline

		/// <summary>
		/// Prepare and loads all graphics resources on the GPU.
		/// </summary>
		public virtual void LoadAttachments(IEnumerable<Resource.IDescriptor> resources)
		{
			foreach (Shape.Descriptor shape in resources)
			{
				// - Alloc the needed vertexs for this shape
				var vertexs = VertexManager.Alloc((uint)shape.Points.Length);


				shape.SourceScheduler.Add(() =>
				{
					shape.SetResource(new Resources.Geometry
					{
						
					});

					shape.SetState(Resource.State.Loaded);
				});
			}
		}

		/// <summary>
		/// Render a set of asssets on the screen.
		/// </summary>
		public virtual void Render()
		{
			
		}

		#endregion

		#region Resources disposal

		/// <summary>
		/// Dispose logic variable.
		/// </summary>
		private bool pDisposed { get; set; }

		/// <summary>
		/// Implement IDisposable.
		/// </summary>
		public void Dispose()
		{
			// Check to see if Dispose has already been called.
			if (!pDisposed)
			{
				Dispose(true);

				// Note disposing has been done.
				pDisposed = true;
			}

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
		}

		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// </summary>
		~Renderer()
		{
			Dispose(false);
		}

		#endregion
	}
}
