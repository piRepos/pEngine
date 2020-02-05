using System;
using System.Linq;

using SharpVk;
using SharpVk.Khronos;
using SharpVk.Multivendor;

namespace pEngine.Graphics.Vulkan
{
	/// <summary>
	/// Handles a Vulkan swap chain settings.
	/// </summary>
	public class SwapChain : IDisposable
	{
		/// <summary>
		/// Makes a new instance of <see cref="SwapChain"/> class.
		/// </summary>
		/// <param name="device">Source device.</param>
		public SwapChain(GraphicDevicee device)
		{
			RenderDevice = device;
		}

		/// <summary>
		/// Stored working device.
		/// </summary>
		protected GraphicDevicee RenderDevice { get; private set; }

		/// <summary>
		/// Swap chain capabilities.
		/// </summary>
		public SurfaceCapabilities Capabilities { get; private set; }
		
		/// <summary>
		/// Video source format.
		/// </summary>
		public SurfaceFormat[] Formats { get; private set; }

		/// <summary>
		/// Presentation mode.
		/// </summary>
		public PresentMode[] PresentModes { get; private set; }

		/// <summary>
		/// Active swap chain video format.
		/// </summary>
		public SurfaceFormat CurrentFormat { get; private set; }

		/// <summary>
		/// Swap chain image.
		/// </summary>
		public Image[] Images { get; private set; }

		/// <summary>
		/// Video surface size.
		/// </summary>
		public Extent2D Extent { get; private set; }

		/// <summary>
		/// Vulkan swap chain.
		/// </summary>
		public Swapchain Handle { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public ImageView[] ImageViews { get; private set; }

		/// <summary>
		/// Swapchain frame buffers.
		/// </summary>
		public Framebuffer[] VideoBuffers { get; private set; }

		/// <summary>
		/// Initialize this swap chain on a specified surface.
		/// </summary>
		/// <param name="surface">Target video surface.</param>
		/// <param name="surfaceSize">Swap queue target size.</param>
		public void Initialize(Surface surface, Vector2i surfaceSize)
		{
			pDisposed = false;

			// - Get capabilites and hardware information
			Capabilities = RenderDevice.PhysicalDevice.GetSurfaceCapabilities(surface);
			Formats = RenderDevice.PhysicalDevice.GetSurfaceFormats(surface);
			PresentModes = RenderDevice.PhysicalDevice.GetSurfacePresentModes(surface);

			uint imageCount = Capabilities.MinImageCount + 1;
			if (Capabilities.MaxImageCount > 0 && imageCount > Capabilities.MaxImageCount)
				imageCount = Capabilities.MaxImageCount;

			// - Default video format -> the first one
			CurrentFormat = Formats[0];

			// - Checks for a BGRA32 video format
			foreach (var format in Formats)
			{
				if (format.Format == Format.B8G8R8A8UNorm && format.ColorSpace == ColorSpace.SrgbNonlinear)
				{
					CurrentFormat = format;
					break;
				}
			}

			// - Checks if ther're no avaiable formats and we create a new one
			if (Formats.Length == 1 && Formats[0].Format == Format.Undefined)
			{
				CurrentFormat = new SurfaceFormat
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

			Handle = RenderDevice.LogicalDevice.CreateSwapchain
			(
				surface, imageCount,
				CurrentFormat.Format,
				CurrentFormat.ColorSpace,
				extent, 1, ImageUsageFlags.ColorAttachment,
				exclusive ? SharingMode.Exclusive : SharingMode.Concurrent, exclusive ? null : queues, 
				Capabilities.CurrentTransform, CompositeAlphaFlags.Opaque, 
				PresentModes.Contains(PresentMode.Mailbox) ? PresentMode.Mailbox : PresentMode.Fifo,
				true, Handle
			);

			Images = Handle.GetImages();
			Extent = extent;
		}

		/// <summary>
		/// Create the image views for the swapchain.
		/// This is made because they allow us to access to the color target of each image.
		/// </summary>
		public void CreateImageViews()
		{
			ImageViews = Images.Select(img => RenderDevice.LogicalDevice.CreateImageView
			(
				img, ImageViewType.ImageView2d,
				CurrentFormat.Format, ComponentMapping.Identity,
				new ImageSubresourceRange(ImageAspectFlags.Color, 0, 1, 0, 1)
			)).ToArray();
		}

		/// <summary>
		/// Creates all framebuffers from the swapchain images.
		/// </summary>
		/// <param name="pass">Render pass.</param>
		public void CreateFrameBuffers(RenderPass pass)
		{
			Framebuffer Create(ImageView imageView) => 
				RenderDevice.LogicalDevice.CreateFramebuffer(pass, new[] { imageView }, Extent.Width, Extent.Height, 1);

			VideoBuffers = ImageViews.Select(Create).ToArray();
		}

		/// <summary>
		/// Dispose logic variable.
		/// </summary>
		private bool pDisposed { get; set; }

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
			if (!pDisposed)
			{
				// - Destroy all images
				foreach (var img in ImageViews) img?.Dispose();

				Images = new Image[0];
				ImageViews = new ImageView[0];
				VideoBuffers = new Framebuffer[0];

				// - Destroy swap chain
				Handle?.Dispose();
				Handle = null;

				// Note disposing has been done.
				pDisposed = true;
			}
		}

		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// </summary>
		~SwapChain()
		{
			Dispose(false);
		}

	}
}
