using System;

using SharpVk;

using pEngine.Graphics.FrameBuffers;
using pEngine.Graphics.Vulkan.Devices;
using pEngine.Graphics.Devices;

namespace pEngine.Graphics.Vulkan.FrameBuffers
{
	/// <summary>
	/// 
	/// </summary>
	public class VKFrameBuffer : FrameBuffer
	{
		/// <summary>
		/// Makes a new instance of <see cref="VKFrameBuffer"/> class.
		/// </summary>
		/// <param name="device"></param>
		public VKFrameBuffer()
		{

		}

		/// <summary>
		/// Makes a new instance of <see cref="VKFrameBuffer"/> class.
		/// </summary>
		/// <param name="device"></param>
		public VKFrameBuffer(Framebuffer device)
		{
			Handler = device;
		}

		public Framebuffer Handler { get; private set; }

		public override bool Initialized => base.Initialized;

		/// <summary>
		/// Initializes the framebuffer.
		/// </summary>
		public override void Initialize(GraphicDevice device)
		{
			//GraphicDevice.Handle.CreateFramebuffer();

			base.Initialize(device);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) Handler.Destroy();

			base.Dispose(disposing);
		}
	}
}
