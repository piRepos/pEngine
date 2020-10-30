using System;
using System.Linq;
using System.Collections.Generic;

using SharpVk;
using SharpVk.Khronos;
using SharpVk.Multivendor;

using pEngine.Graphics.Vulkan.Devices;
using pEngine.Graphics.Devices;
using pEngine.Graphics.Pipelines;
using pEngine.Graphics.Vulkan.FrameBuffers;
using pEngine.Graphics.FrameBuffers;

namespace pEngine.Graphics.Vulkan
{
	using SurfaceFormat = Graphics.FrameBuffers.SurfaceFormat;
	using Surface = Graphics.Devices.Surface;
	using Format = SharpVk.Format;
	using ColorSpace = SharpVk.Khronos.ColorSpace;

	/// <summary>
	/// Handles a Vulkan swap chain settings.
	/// </summary>
	public class VKSwapChain : SwapChain
	{
		/// <summary>
		/// Makes a new instance of <see cref="VKSwapChain"/> class.
		/// </summary>
		/// <param name="device">Source device.</param>
		public VKSwapChain(VKPhysicalDevice card, VKGraphicDevice device)
			: base(card, device)
		{
			
		}

		/// <summary>
		/// Contains the parent graphic device.
		/// </summary>
		protected new VKGraphicDevice RenderDevice => base.RenderDevice as VKGraphicDevice;

		/// <summary>
		/// Contains the parent graphic card.
		/// </summary>
		protected new VKPhysicalDevice GraphicsCard => base.GraphicsCard as VKPhysicalDevice;

		/// <summary>
		/// Video source format.
		/// </summary>
		public override IEnumerable<SurfaceFormat> Formats => formats.Cast<SurfaceFormat>();

		/// <summary>
		/// Presentation mode.
		/// </summary>
		public IEnumerable<PresentMode> PresentModes => presentModes;

		/// <summary>
		/// Active swap chain video format.
		/// </summary>
		public override SurfaceFormat CurrentFormat => surfaceFormat;

		/// <summary>
		/// Video surface size.
		/// </summary>
		public override Vector2i SurfaceSize => new Vector2i((int)extent.Width, (int)extent.Height);

		/// <summary>
		/// Swap chain image.
		/// </summary>
		public IEnumerable<Image> Images => images;

		/// <summary>
		/// Image views.
		/// </summary>
		public IEnumerable<ImageView> ImageViews => imageViews;

		/// <summary>
		/// Vulkan swap chain.
		/// </summary>
		public new IList<VKFrameBuffer> VideoBuffers => videoBuffers.Select(x => new VKFrameBuffer(x)).ToList();

		/// <summary>
		/// Vulkan swap chain.
		/// </summary>
		public Swapchain Handle { get; private set; }


		#region Initialization and settings

		private SharpVk.Khronos.SurfaceFormat surfaceFormat;
		private SharpVk.Khronos.SurfaceFormat[] formats;
		private SharpVk.Khronos.PresentMode[] presentModes;
		private SharpVk.Framebuffer[] videoBuffers;
		private SharpVk.ImageView[] imageViews;
		private SharpVk.Image[] images;
		private SharpVk.Extent2D extent;

		/// <summary>
		/// Initialize this swap chain on a specified surface.
		/// </summary>
		/// <param name="surface">Target video surface.</param>
		/// <param name="surfaceSize">Swap queue target size.</param>
		public void Initialize(Surface surface, Vector2i surfaceSize)
		{
			base.Initialize(surface, surfaceSize);

			VKSurface vkSurface = surface as VKSurface;

			// - Get capabilites and hardware information
			SurfaceCapabilities Capabilities = GraphicsCard.Handle.GetSurfaceCapabilities(vkSurface.Handle);
			formats = GraphicsCard.Handle.GetSurfaceFormats(vkSurface.Handle);
			presentModes = GraphicsCard.Handle.GetSurfacePresentModes(vkSurface.Handle);

			uint imageCount = Capabilities.MinImageCount + 1;
			if (Capabilities.MaxImageCount > 0 && imageCount > Capabilities.MaxImageCount)
				imageCount = Capabilities.MaxImageCount;

			// - Default video format -> the first one
			surfaceFormat = formats[0];

			// - Checks for a BGRA32 video format
			foreach (var format in formats)
			{
				if (format.Format == Format.B8G8R8A8UNorm && format.ColorSpace == ColorSpace.SrgbNonlinear)
				{
					surfaceFormat = format;
					break;
				}
			}

			// - Checks if ther're no avaiable formats and we create a new one
			if (formats.Length == 1 && formats[0].Format == Format.Undefined)
			{
				surfaceFormat = new SharpVk.Khronos.SurfaceFormat
				{
					Format = Format.B8G8R8A8UNorm,
					ColorSpace = ColorSpace.SrgbNonlinear
				};
			}

			// - Computes the swap chain drawing surface size
			Extent2D extent = Capabilities.CurrentExtent;
			if (extent.Width != uint.MaxValue)
			{
				long Width = Math.Max(Capabilities.MinImageExtent.Width, Math.Min(Capabilities.MaxImageExtent.Width, surfaceSize.Width));
				long Height = Math.Max(Capabilities.MinImageExtent.Height, Math.Min(Capabilities.MaxImageExtent.Height, surfaceSize.Height));
				extent = new Extent2D((uint)Width, (uint)Height);
			}

			// - Shortcut (to avoid long parameters on the next call)
			var queues = new[] { RenderDevice.PresentQueueIndex, RenderDevice.GraphicQueueIndex };
			bool exclusive = RenderDevice.GraphicQueueIndex == RenderDevice.PresentQueueIndex;

			Handle = RenderDevice.Handle.CreateSwapchain
			(
				vkSurface.Handle, imageCount,
				surfaceFormat.Format,
				surfaceFormat.ColorSpace,
				extent, 1, ImageUsageFlags.ColorAttachment,
				exclusive ? SharingMode.Exclusive : SharingMode.Concurrent, exclusive ? null : queues, 
				Capabilities.CurrentTransform, CompositeAlphaFlags.Opaque, 
				PresentModes.Contains(PresentMode.Mailbox) ? PresentMode.Mailbox : PresentMode.Fifo,
				true, Handle
			);

			images = Handle.GetImages();
			this.extent = extent;
		}

		/// <summary>
		/// Create the image views for the swapchain.
		/// This is made because they allow us to access to the color target of each image.
		/// </summary>
		public void CreateImageViews()
		{
			imageViews = Images.Select(img => RenderDevice.Handle.CreateImageView
			(
				img, ImageViewType.ImageView2d,
				surfaceFormat.Format, ComponentMapping.Identity,
				new ImageSubresourceRange(ImageAspectFlags.Color, 0, 1, 0, 1)
			)).ToArray();
		}

		/// <summary>
		/// Creates all framebuffers from the swapchain images.
		/// </summary>
		/// <param name="pass">Render pass.</param>
		public void CreateFrameBuffers(VKRenderPass pass)
		{
			Framebuffer Create(ImageView imageView) => 
				RenderDevice.Handle.CreateFramebuffer(pass.Handle, new[] { imageView }, extent.Width, extent.Height, 1);

			videoBuffers = ImageViews.Select(Create).ToArray();
		}

		#endregion

		#region Disposable

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
			// Check to see if Dispose has already been called.
			if (!Disposed)
			{
				// - Destroy all images
				foreach (var img in Images) img?.Destroy();
				foreach (var img in ImageViews) img?.Destroy();
				foreach (var fb in VideoBuffers) fb?.Handler.Destroy();

				images = new Image[0];
				imageViews = new ImageView[0];
				videoBuffers = new Framebuffer[0];

				// - Destroy swap chain
				Handle?.Dispose();
				Handle = null;

				// Note disposing has been done.
				Disposed = true;
			}
		}

		#endregion

	}
}
