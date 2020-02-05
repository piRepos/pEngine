using System;

using pEngine.Windows.Context;
using pEngine.Windows.Runtime;
using pEngine.Timing.Base;
using pEngine.Framework;

namespace pEngine.Windows
{
	public class WindowsRuntime : pEngine.Runtime
	{
		/// <summary>
		/// Makes a new instance of <see cref="WindowsRuntime"/>, a specific
		/// implementation of pEngine for windows platform.
		/// </summary>
		public WindowsRuntime() : base()
		{
			// - Add Binaries folder to the search path
			LibraryLoader.Initialize();

			// - Initialize GLFW
			GLFW.Glfw.Init();

			// - Create the game window instance
			Surface = new GlfwWindow(new Vector2i(500, 500), Environment.Engine.Name);
		}

		public override void Run(Game game)
		{
			var window = Surface as GlfwWindow;

			// TODO: Set default window size from game

			// - Initialize the game window
			window.Initialize();

			// - Open the window
			window.Show();

			base.Run(game);
		}

		protected override void InputFunction(IFrameBasedClock clock)
		{
			var window = Surface as GlfwWindow;

			// - Process OS messages
			GLFW.Glfw.PollEvents();

			// - Close the engine runtime when closing the window
			if (window.ShouldClose) Close();

			base.InputFunction(clock);
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
		protected override void Dispose(bool disposing)
		{
			var window = Surface as GlfwWindow;

			base.Dispose(disposing);

			if (disposing)
			{
				// - Destroy the active window
				window.Destroy();
			}

			// - Dispose GLFW library
			GLFW.Glfw.Terminate();
		}
	}
}
