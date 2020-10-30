using System;
using System.Runtime.InteropServices;

using GLFW;
using GLFW.Game;

using SharpVk;
using SharpVk.Khronos;

using pEngine.Environment.Video;

using pEngine.Utils.Properties;

namespace pEngine.Windows.Context
{
    /// <summary>
    /// This class manage a GLFW crossplatform window.
    /// </summary>
    public class GlfwWindow : ISurface
    {
        /// <summary>
        /// Makes a new instance of <see cref="GlfwWindow"/> class.
		/// This class manage a crossplatform window.
        /// </summary>
		/// <param name="size">Initial window size.</param>
        /// <param name="title">Initial window title.</param>
		public GlfwWindow(Vector2i size, string title)
        {
            // - Initialize properties
            Size = new Notify<Vector2i>(size);
            Position = new Notify<Vector2i>();
            Title = new Notify<string>(title);

            // - Bind properties events
            Size.Changing += SizeChanging;
            Position.Changing += PositionChanging;
            Title.Changing += TitleChanging;

            // - Initialize callback store
            pSizeCallback = new SizeCallback((hnd, w, h) => Resized?.Invoke(this, new Vector2(w, h)));
        }

        #region Instance

        /// <summary>
        /// GLFW Wrapped window pointer.
        /// </summary>
        protected Window GLFWHAndle { get; private set; }

        /// <summary>
        /// <see cref="false"/> if the windows handle hasn't been initialized yet.
        /// </summary>
        public bool IsValid => GLFWHAndle != Window.None;

        /// <summary>
		/// Raised on surface invalidation.
		/// </summary>
		public event EventHandler Invalidating;

        /// <summary>
        /// Raised on surface initialization complete.
        /// </summary>
        public event EventHandler Initialized;

        /// <summary>
        /// Initialize this windows instance.
        /// </summary>
        public void Initialize()
        {
            // - Reset window hints
            Glfw.DefaultWindowHints();

            // - Let the user choose whenever show the window
            Glfw.WindowHint(Hint.ClientApi, 0);
            Glfw.WindowHint(Hint.Visible, false);

            // - Make the new window saving the GLFW pointer
            GLFWHAndle = Glfw.CreateWindow
            (
                Size.Value.Width, 
                Size.Value.Height, 
                Title, 
                Monitor.None, 
                Window.None
            );

            // - Manage events handlers
            Glfw.SetWindowSizeCallback(GLFWHAndle, pSizeCallback);

            // - Send initialization event
            Initialized?.Invoke(this, EventArgs.Empty);
        }
        
        public void Destroy()
        {
            // - Warn extern resource before destroying the window
            Invalidating?.Invoke(this, EventArgs.Empty);

            // - Destroy the window
            Glfw.DestroyWindow(GLFWHAndle);

            // - Set the pointer to null
            GLFWHAndle = Window.None;
        }

        #endregion

        #region Management

        /// <summary>
        /// Gets if the window should close.
        /// </summary>
        public bool ShouldClose => Glfw.WindowShouldClose(GLFWHAndle);

        /// <summary>
        /// Show the window
        /// </summary>
        public void Show()
        {
            // - If not initialized do nothing
            if (!IsValid) throw new InvalidOperationException("Window not initialized");

            // - Show the window
            Glfw.ShowWindow(GLFWHAndle);
        }

		#endregion

		#region Context

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// GLFW window pointer.
        /// </summary>
        public IntPtr Handle => GLFWHAndle;

        /// <summary>
        /// Gets the video buffer current size. (this call may be slow)
        /// </summary>
        public Vector2i SurfaceSize => GetSurfaceSize();

        /// <summary>
        /// Gets the scaling ratio between the window size and the buffer size, (pixel dentisy).
        /// </summary>
        public Vector2 Scaling => GetSurfaceScaling();

        /// <summary>
        /// Gets the required extensions.
        /// </summary>
        public string[] Extensions => new[] { "VK_KHR_surface", "VK_KHR_win32_surface" };

        /// <summary>
        /// Makes a new vulkan <see cref="Surface"/> instance.
        /// </summary>
        /// <param name="vulkan">Vulcan source instance.</param>
        /// <returns>A new <see cref="Surface"/> attached to this context.</returns>
        public Graphics.Vulkan.Devices.VKSurface CreateVKSurface(Instance vulkan)
        {
            // - If not initialized do nothing
            if (!IsValid) throw new InvalidOperationException("Window not initialized");

            IntPtr hinstance = GetModuleHandle("Kernel32.dll");
            IntPtr hwnd = Native.GetWin32Window(GLFWHAndle);

            return new Graphics.Vulkan.Devices.VKSurface(vulkan.CreateWin32Surface(hinstance, hwnd));
        }

        /// <summary>
		/// Bind this graphic context (for OpenGL or statefull libraries).
		/// </summary>
        public void BindContext()
        {
            // - If not initialized do nothing
            if (!IsValid) throw new InvalidOperationException("Window not initialized");

            // - Set the context on this thread
            Glfw.MakeContextCurrent(GLFWHAndle);
        }

        private Vector2i GetSurfaceSize()
        {
            // - If not initialized do nothing
            if (!IsValid) throw new InvalidOperationException("Window not initialized");

            Glfw.GetFramebufferSize(GLFWHAndle, out int w, out int h);

            return new Vector2i(w, h);
        }

        private Vector2 GetSurfaceScaling()
        {
            // - If not initialized do nothing
            if (!IsValid) throw new InvalidOperationException("Window not initialized");

            Glfw.GetWindowContentScale(GLFWHAndle, out float w, out float h);

            return new Vector2(w, h);
        }

        #endregion

        #region Geometry

        // - Store all calback mantaining the reference
        SizeCallback pSizeCallback { get; }

        /// <summary>
        /// Gets or sets the window size.
        /// </summary>
        public Notify<Vector2i> Size { get; }

        /// <summary>
        /// Gets or sets the window position.
        /// </summary>
        public Notify<Vector2i> Position { get; }

        /// <summary>
		/// Raised after this surface is resized.
		/// </summary>
        public event EventHandler<Vector2> Resized;


        private void SizeChanging(object sender, (Vector2i newValue, Vector2i oldValue) e)
        {
            // - If not initialized do nothing
            if (!IsValid) return;

            // - Sets the new window size to the current window if initialized
            Glfw.SetWindowSize(GLFWHAndle, e.newValue.Width, e.newValue.Height);
        }

        private void PositionChanging(object sender, (Vector2i newValue, Vector2i oldValue) e)
        {
            // - If not initialized do nothing
            if (!IsValid) return;

            // - Sets the new window position to the current window if initialized
            Glfw.SetWindowPosition(GLFWHAndle, e.newValue.X, e.newValue.Y);
        }

        #endregion

        #region Metadata

        /// <summary>
		/// Gets or sets the window title.
		/// </summary>
		public Notify<string> Title { get; }


        private void TitleChanging(object sender, (string newValue, string oldValue) e)
        {
            // - If not initialized do nothing
            if (!IsValid) return;

            // - Sets the new window position to the current window if initialized
            Glfw.SetWindowTitle(GLFWHAndle, e.newValue);
        }

        #endregion
    }
}
