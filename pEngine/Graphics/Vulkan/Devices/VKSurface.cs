using System;

using pEngine.Graphics.Devices;

namespace pEngine.Graphics.Vulkan.Devices
{
	public class VKSurface : Surface
	{

		/// <summary>
		/// Makes a new instance of <see cref="VKSurface"/> class.
		/// </summary>
		public VKSurface(SharpVk.Khronos.Surface surface) : base()
		{
			Handle = surface;
		}

		/// <summary>
		/// Vulkan surface handler.
		/// </summary>
		public SharpVk.Khronos.Surface Handle { get; }

	}
}
