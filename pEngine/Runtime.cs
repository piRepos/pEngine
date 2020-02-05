using System;
using System.Linq;
using System.Collections.Generic;

using pEngine.Environment.Video;
using pEngine.Diagnostic;

using pEngine.Timing.Base;
using pEngine.Timing;

using pEngine.Graphics;
using pEngine.Graphics.Vulkan;

using pEngine.Utils.Collections;
using pEngine.Utils.Extensions;

using pEngine.Framework;
using pEngine.Framework.Geometry;


namespace pEngine
{
	public class Runtime : IDisposable
	{
		/// <summary>
		/// Initialize the pEngine runtime.
		/// </summary>
		public Runtime()
		{
			// - Initialize debug manager
			Debug.Initialize();

			// - Create a game loop on the main thread because this thread is going to talk with the OS
			InputGameLoop = new GameLoop(InputFunction, InputInit, "InputGameLoop");

			// - Create a threaded update gameloop, we want to run the update in a separate thread
			UpdateGameLoop = new ThreadedGameLoop(UpdateFunction, UpdateInit, "UpdateGameLoop");

			// - Create a threaded render loop, this thread will manage all GPU settings and actions
			GraphicGameLoop = new ThreadedGameLoop(RenderFunction, GraphicInit, "RenderGameLoop");

			// - Initialize resource queues
			ResourceStash = new List<Resource.IDescriptor>();

			// - Initialize graphic buffer
			GraphicRenderBuffer = new TripleBuffer<Asset[]>();
		}

		/// <summary>
		/// <see cref="true"/> it the game engine is running.
		/// </summary>
		public bool Running => UpdateGameLoop.IsRunning || InputGameLoop.IsRunning;

		/// <summary>
		/// Gets the instance of the game that is actually running.
		/// </summary>
		public Game RunningGame { get; private set; }

		/// <summary>
		/// Start the game loop, this function will block the executing
		/// context.
		/// </summary>
		public virtual void Run(Game game)
		{
			// - Cannot run many games at the same time
			if (RunningGame != null) return;

			// - Set the game
			RunningGame = game;

			// - Start update loop
			UpdateGameLoop.Run();

			// - Start render loop
			GraphicGameLoop.Run();

			// - Start input loop (blocking call)
			InputGameLoop.Run();

			// - Wait for loop finish
			UpdateGameLoop.CurrentThread.Join();
			GraphicGameLoop.CurrentThread.Join();
		}

		/// <summary>
		/// Closes the running game, if there's no game running do nothing.
		/// </summary>
		public virtual void Close()
		{
			if (Running)
			{
				// - Stop all loops
				InputGameLoop.Stop();
				UpdateGameLoop.Stop();
				GraphicGameLoop.Stop();
			}
		}

		#region Input

		/// <summary>
		/// Manages any physical input from input devices and talks with the OS.
		/// </summary>
		protected GameLoop InputGameLoop { get; }

		/// <summary>
		/// Initialize the input modules and logic.
		/// </summary>
		protected virtual void InputInit()
		{

		}

		/// <summary>
		/// Main input function; all game input and OS messages must be handled here.
		/// </summary>
		/// <param name="clock">Frame clock: gives all timing informations.</param>
		protected virtual void InputFunction(IFrameBasedClock clock)
		{

		}

		/// <summary>
		/// Destroy all the resources allocated from this thread.
		/// </summary>
		protected virtual void InputDispose()
		{

		}

		#endregion

		#region Update

		/// <summary>
		/// Manages all the physics updates and logical interactions cycles.
		/// </summary>
		protected GameLoop UpdateGameLoop { get; }

		/// <summary>
		/// Share resource data between this gameloop and the others.
		/// </summary>
		protected List<Resource.IDescriptor> ResourceStash { get; }

		/// <summary>
		/// Initialize the game update modules and logic.
		/// </summary>
		protected virtual void UpdateInit()
		{
			// - Initialize the game
			RunningGame.Initialize(this);
		}

		/// <summary>
		/// Main update function; all game logic MUST be executed in this
		/// context, keep passing the clock on each update call.
		/// </summary>
		/// <param name="clock">Frame clock: gives all timing informations.</param>
		protected virtual void UpdateFunction(IFrameBasedClock clock)
		{
			// - Update the game tree state
			RunningGame.Update(clock);

			// - Gets the game tree resources
			var descriptor = RunningGame.GetDescriptor(UpdateGameLoop.Scheduler);

			// - Dispatch resource loading
			lock (ResourceStash)
			{
				foreach (var resource in descriptor.Resources)
				{
					ResourceStash.Add(resource);
				}
			}

			// - Send the render assets to the graphic renderer
			using (var lk = GraphicRenderBuffer.Get(UsageType.Write))
				lk.Value = descriptor.Assets;
		}

		/// <summary>
		/// Destroy all the resources allocated from this thread.
		/// </summary>
		protected virtual void UpdateDispose()
		{

		}

		#endregion

		// TODO: Data exchange between Update and Rendering: make a thread safe circular array passing a list of assets

		#region Rendering

		/// <summary>
		/// Manages the video buffer drawing and the graphic context.
		/// </summary>
		protected GameLoop GraphicGameLoop { get; }

		/// <summary>
		/// Share all the information to render the scene with the update thread.
		/// </summary>
		protected TripleBuffer<Asset[]> GraphicRenderBuffer { get; }

		/// <summary>
		/// Graphic drawing surface.
		/// </summary>
		protected ISurface Surface { get; set; }

		/// <summary>
		/// Perform video rendering from a set of assets.
		/// </summary>
		protected Renderer Renderer { get; private set; }

		/// <summary>
		/// Initialize the graphic modules and GPU settings.
		/// </summary>
		protected virtual void GraphicInit()
		{
			// - Create the renderer module
			Renderer = new VKRenderer(Surface);

			// - Initialize the renderer
			Renderer.Initialize();

			// - Load initial resources
			Renderer.LoadResources();

			// - Configure the renderer rendering process
			Renderer.ConfigureRendering();
		}

		/// <summary>
		/// Main render function; draws all the assets on the screen and refresh
		/// the screen with the speicified framerate.
		/// </summary>
		/// <param name="clock">Frame clock: gives all timing informations.</param>
		protected virtual void RenderFunction(IFrameBasedClock clock)
		{
			Resource.IDescriptor[] graphicResources;

			lock (ResourceStash)
			{
				// - Gets geometry resources which needs to be loaded
				var res = ResourceStash.TakeAll(x => x is Shape.Descriptor);

				graphicResources = res.ToArray();
			}

			// - Prepare all attachments
			Renderer.LoadAttachments(graphicResources);

			// - Render a frame
			Renderer.Render();
		}

		/// <summary>
		/// Destroy all the resources allocated from this thread.
		/// </summary>
		protected virtual void GraphicDispose()
		{
			// - Dispose renderer
			Renderer.Dispose();
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
			if (disposing)
			{
				// - Dispose resources
				InputDispose();
				UpdateDispose();
				GraphicDispose();
			}
		}

		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// </summary>
		~Runtime()
		{
			Dispose(false);
		}

		#endregion
	}
}
