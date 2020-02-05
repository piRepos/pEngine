using System;
using System.Linq;
using System.Collections.Generic;

using SharpVk;
using SharpVk.Khronos;
using SharpVk.Multivendor;

using pEngine.Environment;
using pEngine.Environment.Video;

using pEngine.Diagnostic;

using pEngine.Framework;

using pEngine.Graphics.Shading;
using pEngine.Graphics.Vulkan.Shading;
using pEngine.Graphics.Vulkan.Vertexs;

using pEngine.Graphics.Vulkan.Devices;

namespace pEngine.Graphics.Vulkan
{
	public class VKRenderer : Renderer
	{
		/// <summary>
		/// Makes a new instance of <see cref="VKRenderer"/> class,
		/// this class manage Vulkan assets rendering.
		/// </summary>
		public VKRenderer(ISurface videoSurface) : base(videoSurface)
		{
			// - Creates vulkan library instance
			base.Library = new VulkanLibrary();

			// - Creates the vulkan physical device
			base.PhysicalDevice = new VKPhysicalDevice(Library);

			// - Creates the vulkan graphical device
			base.GraphicDevice = new VKGraphicDevice(PhysicalDevice);
		}

		/// <summary>
		/// Contains the instance of graphic library.
		/// </summary>
		public new VulkanLibrary Library => base.Library as VulkanLibrary;

		/// <summary>
		/// Graphics card or device that allows drawings on a screen.
		/// </summary>
		protected new VKPhysicalDevice PhysicalDevice => base.PhysicalDevice as VKPhysicalDevice;

		/// <summary>
		/// An abstraction layer from the physical video device.
		/// </summary>
		protected new VKGraphicDevice GraphicDevice => base.GraphicDevice as VKGraphicDevice;

		/// <summary>
		/// Manages raw vertex allocation and the GPU buffer transfer.
		/// </summary>
		protected new VKIndexBuffer IndexManager => base.IndexManager as VKIndexBuffer;

		/// <summary>
		/// Manages raw vertex allocation and the GPU buffer transfer.
		/// </summary>
		public new VKVertexBuffer VertexManager => base.VertexManager as VKVertexBuffer;

		/// <summary>
		/// Window or area where the library will render all images.
		/// </summary>
		protected Surface DrawingSurface { get; private set; }

		/// <summary>
		/// Manage the comunication between the physical and the logical
		/// video device.
		/// </summary>
		protected GraphicDevicee RenderingDevice { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		protected SwapChain Swapchain { get; private set; }

		/// <summary>
		/// Current renderer drawing pipeline.
		/// </summary>
		protected RenderPipeline Pipeline { get; private set; }

		/// <summary>
		/// Configure the default graphic render pass.
		/// A render pass is like a descriptor that tells to the graphics
		/// card which resources are needed from a rendering step.
		/// </summary>
		protected RenderPass DefaultRenderPass { get; private set; }

		/// <summary>
		/// Command pool, makes all the command buffers.
		/// </summary>
		protected CommandPool Commands { get; private set; }

		/// <summary>
		/// Swapchain presentation semaphore.
		/// </summary>
		protected Semaphore ImgAvaiable { get; private set; }

		/// <summary>
		/// Swapchain render finish semaphore.
		/// </summary>
		protected Semaphore RenderFinish { get; private set; }

		/// <summary>
		/// Initialize the graphic library.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			// - Create a Vulkan instance, this instance must be alive during all the game execution
			Library.Initialize(TargetSurface);

			// - Makes a new device manager
			RenderingDevice = new GraphicDevicee(VulkanInstance);

			// - Istantiate modules
			ShaderManager = new VKShaderManager(RenderingDevice);

			// - Initialize the vertex manager
			base.VertexManager = new VKVertexBuffer(RenderingDevice, 100, true, 0.2);

			// - Initialize the index manager
			base.IndexManager = new VKIndexBuffer(RenderingDevice, 100, true, 0.2);

			// - Create a new swapchain
			Swapchain = new SwapChain(RenderingDevice);

			// - Create the default pipeline
			Pipeline = new RenderPipeline(RenderingDevice);

			// - Makes a new drawing surface
			DrawingSurface = TargetSurface.CreateVKSurface(VulkanInstance);

			// - Initialize the physical device on this drawing area
			RenderingDevice.SelectPhysicalDevice(DrawingSurface);

			// - Creates a logical device used for rendering
			RenderingDevice.CreateLogicalDevice();

			// - Initialize index and vert manager
			VertexManager.Setup();
			IndexManager.Setup();

			// - Create the command pool
			Commands = RenderingDevice.LogicalDevice.CreateCommandPool(RenderingDevice.GraphicQueueIndex);

			ImgAvaiable = RenderingDevice.LogicalDevice.CreateSemaphore();
			RenderFinish = RenderingDevice.LogicalDevice.CreateSemaphore();
		}

		/// <summary>
		/// Configure the render for rendering.
		/// </summary>
		public override void ConfigureRendering()
		{
			base.ConfigureRendering();

			// - Makes a new drawing surface
			DrawingSurface = TargetSurface.CreateVKSurface(VulkanInstance);

			// - Initialize the swap chain
			Swapchain.Initialize(DrawingSurface, TargetSurface.SurfaceSize);

			// - Initialzie the swapchain images
			Swapchain.CreateImageViews();

			// - Configure the default render pass
			ConfigureRenderPass(RenderingDevice, Swapchain.CurrentFormat.Format);

			// - Load shaders
			ShaderManager.CreateShaders();

			// - Gets default shaders
			var defaultShader = ShaderManager[typeof(DefaultShader)] as VKShaderInstance;
			var vert = defaultShader.VKVertexShader;
			var frag = defaultShader.VKFragmentShader;

			// - Initialize pipeline
			Pipeline.Initialize(DefaultRenderPass, Swapchain.Extent, vert, frag);

			// - Build swapchain framebuffers
			Swapchain.CreateFrameBuffers(DefaultRenderPass);
		}

		/// <summary>
		/// 
		/// </summary>
		public override void LoadAttachments(IEnumerable<Resource.IDescriptor> resources)
		{
			base.LoadAttachments(resources);

			var commands = RenderingDevice.LogicalDevice.AllocateCommandBuffer(Commands, CommandBufferLevel.Primary);

			commands.Begin(CommandBufferUsageFlags.OneTimeSubmit);
			{
				// - Upload invalidated vertexs
				VertexManager.Upload(commands, 1000);

				// - Upload invalidated indexes
				IndexManager.Upload(commands, 1000);

			}
			commands.End();

			// - Perform all attachment operations
			RenderingDevice.GraphicQueue.Submit
			(
				new SubmitInfo
				{ 
					CommandBuffers = new[] { commands } 
				},
				null
			);

			// - Free attachment command buffer
			Commands.FreeCommandBuffers(commands);

			// - Wait the GPU (we must operate only when GPU is idle)
			RenderingDevice.LogicalDevice.WaitIdle();
		}

		/// <summary>
		/// Render a set of asssets on the screen.
		/// </summary>
		public override void Render()
		{
			base.Render();

			CommandBuffer[] commandBuffers;

			commandBuffers = RenderingDevice.LogicalDevice.AllocateCommandBuffers(Commands, CommandBufferLevel.Primary, (uint)Swapchain.Images.Length);

			for (int index = 0; index < Swapchain.Images.Length; index++)
			{
				var commandBuffer = commandBuffers[index];

				commandBuffer.Begin(CommandBufferUsageFlags.SimultaneousUse);

				commandBuffer.BeginRenderPass(DefaultRenderPass, Swapchain.VideoBuffers[index], new Rect2D(Swapchain.Extent), (ClearValue)(1F, 0F, 0F, 1F), SubpassContents.Inline);

				commandBuffer.BindPipeline(PipelineBindPoint.Graphics, Pipeline.PipelineInstance);

				VertexManager.Bind(commandBuffer);

				IndexManager.Bind(commandBuffer, 0);

				commandBuffer.DrawIndexed(6, 1, 0, 0, 0);

				commandBuffer.EndRenderPass();

				commandBuffer.End();
			}


			uint nextImage = Swapchain.Handle.AcquireNextImage(uint.MaxValue, ImgAvaiable, null);

			RenderingDevice.GraphicQueue.Submit
			(
				new SubmitInfo
				{
					CommandBuffers = new[] { commandBuffers[nextImage] },
					SignalSemaphores = new[] { RenderFinish },
					WaitDestinationStageMask = new[] { PipelineStageFlags.ColorAttachmentOutput },
					WaitSemaphores = new[] { ImgAvaiable }
				},
				null
			);

			try
			{
				var present = RenderingDevice.PresentQueue.Present(RenderFinish, Swapchain.Handle, nextImage, new Result[1]);

				switch (present)
				{
					case Result.Suboptimal:
						InvalidateGraphics();
						break;
				}

				if (present == Result.Success)
				{
					Commands.FreeCommandBuffers(commandBuffers);
				}
			}
			catch
			{
				InvalidateGraphics();
			}
			finally
			{
				
			}
			
		}

		/// <summary>
		/// Configure the graphic library to work with a new surface size.
		/// </summary>
		/// <param name="videoSurface">Target surface.</param>
		public override void InvalidateGraphics()
		{
			base.InvalidateGraphics();

			// - Wait the GPU (we must operate only when GPU is idle)
			RenderingDevice.LogicalDevice.WaitIdle();

			// - Dispose render pass
			DefaultRenderPass.Dispose();

			// - Dispose the pipeline
			Pipeline.Dispose();

			// - Dipose current swapchain
			Swapchain.Dispose();

			// - Dispose current VK surface
			DrawingSurface.Dispose();

			// - Reconfigure the renderer
			ConfigureRendering();
		}

		/// <summary>
		/// This call configure the graphic library for the new device instance.
		/// </summary>
		public override void InvalidateDevice()
		{
			base.InvalidateDevice();

			// - Wait the GPU (we must operate only when GPU is idle)
			RenderingDevice.LogicalDevice.WaitIdle();

			// - Dispose render pass
			DefaultRenderPass.Dispose();

			// - Dispose the pipeline
			Pipeline.Dispose();

			// - Dipose current swapchain
			Swapchain.Dispose();

			// - Dispose current VK surface
			DrawingSurface.Dispose();

			// - Dispose vertex and index managers
			VertexManager.Dispose();
			IndexManager.Dispose();

			// - Release all VK shaders
			ShaderManager.Dispose();

			// - Dispose device
			RenderingDevice.Dispose();

			// - Initialize device
			Initialize();

			// - Reconfigure the renderer
			ConfigureRendering();
		}

		/// <summary>
		/// Implement IDisposable.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// - Wait the GPU (we must operate only when GPU is idle)
			RenderingDevice.LogicalDevice.WaitIdle();

			// - Dispose render pass
			DefaultRenderPass.Dispose();

			// - Dispose the pipeline
			Pipeline.Dispose();

			// - Dipose current swapchain
			Swapchain.Dispose();

			// - Dispose current VK surface
			DrawingSurface.Dispose();

			// - Dispose vertex and index managers
			VertexManager.Dispose();
			IndexManager.Dispose();

			// - Release all VK shaders
			ShaderManager.Dispose();

			// - Dispose device
			RenderingDevice.Dispose();

			// - Dispose vulkan
			VulkanInstance.Dispose();
		}

		#region Render pass

		private void ConfigureRenderPass(GraphicDevicee device, Format videoFormat)
		{
			// - Makes the default render pass
			DefaultRenderPass = device.LogicalDevice.CreateRenderPass
			(
				new AttachmentDescription
				{
					Format = videoFormat,
					Samples = SampleCountFlags.SampleCount1,
					LoadOp = AttachmentLoadOp.Clear,
					StoreOp = AttachmentStoreOp.Store,
					StencilLoadOp = AttachmentLoadOp.DontCare,
					StencilStoreOp = AttachmentStoreOp.DontCare,
					InitialLayout = ImageLayout.Undefined,
					FinalLayout = ImageLayout.PresentSource
				},
				new SubpassDescription
				{
					DepthStencilAttachment = new AttachmentReference
					{
						Attachment = Constants.AttachmentUnused
					},

					PipelineBindPoint = PipelineBindPoint.Graphics,

					ColorAttachments = new[]
					{
						new AttachmentReference
						{
							Attachment = 0,
							Layout = ImageLayout.ColorAttachmentOptimal
						}
					}
				},
				new SubpassDependency[]
				{
					new SubpassDependency
					{
						SourceSubpass = Constants.SubpassExternal,
						DestinationSubpass = 0,
						SourceStageMask = PipelineStageFlags.BottomOfPipe,
						SourceAccessMask = AccessFlags.MemoryRead,
						DestinationStageMask = PipelineStageFlags.ColorAttachmentOutput,
						DestinationAccessMask = AccessFlags.ColorAttachmentRead | AccessFlags.ColorAttachmentWrite
					},
					new SubpassDependency
					{
						SourceSubpass = 0,
						DestinationSubpass = Constants.SubpassExternal,
						SourceStageMask = PipelineStageFlags.ColorAttachmentOutput,
						SourceAccessMask = AccessFlags.ColorAttachmentRead | AccessFlags.ColorAttachmentWrite,
						DestinationStageMask = PipelineStageFlags.BottomOfPipe,
						DestinationAccessMask = AccessFlags.MemoryRead
					}
				}
			);

		}

		#endregion

		#region Debug

		private DebugReportFlags DebugFlags()
		{
			DebugReportFlags debug = DebugReportFlags.None;

			if (Debug.RendererDebugLevel.HasFlag(DebugLevel.Critical))
				debug |= DebugReportFlags.Error;

			if (Debug.RendererDebugLevel.HasFlag(DebugLevel.Performance))
				debug |= DebugReportFlags.PerformanceWarning;

			if (Debug.RendererDebugLevel.HasFlag(DebugLevel.Warning))
				debug |= DebugReportFlags.Warning;

			if (Debug.RendererDebugLevel.HasFlag(DebugLevel.Debug))
				debug |= DebugReportFlags.Debug;

			if (Debug.RendererDebugLevel.HasFlag(DebugLevel.Info))
				debug |= DebugReportFlags.Information;

			return debug;
		}

		private Bool32 DebugCallback(DebugReportFlags flags, DebugReportObjectType objectType, ulong @object, HostSize location, int messageCode, string pLayerPrefix, string pMessage, IntPtr pUserData)
		{
			Console.WriteLine(pMessage);
			return false;
		}


		#endregion
	}
}
