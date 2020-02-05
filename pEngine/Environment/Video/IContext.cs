using System;

using SharpVk;
using SharpVk.Khronos;

namespace pEngine.Environment.Video
{
	/// <summary>
	/// Base for window context implementations, this interface
	/// provide a generic attachment point for any type of platform's window.
	/// </summary>
	public interface IContext
	{
		/// <summary>
		/// Gets the graphic context handle.
		/// </summary>
		IntPtr Handle { get; }


		/// <summary>
		/// Contains all required device extensions.
		/// </summary>
		string[] Extensions { get; }

		/// <summary>
		/// Makes a new vulkan <see cref="Surface"/> instance.
		/// </summary>
		/// <param name="vulkan">Vulcan source instance.</param>
		/// <returns>A new <see cref="Surface"/> attached to this context.</returns>
		Surface CreateVKSurface(Instance vulkan);

		/// <summary>
		/// Bind this graphic context (for OpenGL or statefull libraries).
		/// </summary>
		void BindContext();
	}
}
