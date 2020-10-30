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
using pEngine.Framework.Geometry;

using pEngine.Graphics.Shading;
using pEngine.Graphics.Vulkan.Shading;
using pEngine.Graphics.Vulkan.Vertexs;

using pEngine.Graphics.Vulkan.Devices;
using pEngine.Graphics.Vulkan.Pipelines;
using pEngine.Graphics.Resources;
using pEngine.Graphics.Data;

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

			// - Istantiate modules
			ShaderManager = new VKShaderManager(GraphicDevice);

			// - Initialize the vertex manager
			base.VertexManager = new VKVertexBuffer(GraphicDevice, 100, true, 0.2);

			// - Initialize the index manager
			base.IndexManager = new VKIndexBuffer(GraphicDevice, 100, true, 0.2);

			// - Initialize the geometry dispatcher
			base.GeometryDispatcher = new VKGeometryDispatcher(VertexManager, IndexManager);

			// - Create a new swapchain
			Swapchain = new VKSwapChain(PhysicalDevice, GraphicDevice);

			// - Create the default render pass
			DefaultRenderPass = new VKRenderPass(GraphicDevice);

			// - Create the default pipeline
			Pipeline = new VKPipeline(GraphicDevice, DefaultRenderPass, false);

			// - Number of frames processing at the same time allowed
			MaxFrameInFlight = 2;

			// - Rendering and syncronization
			Commands = new List<CommandPool>();
			CommandBuffers = new List<CommandBuffer[]>();
			RenderFinishedSemaphores = new List<Semaphore>();
			ImageAvailableSemaphores = new List<Semaphore>();
			InFlightFences = new List<Fence>();
			ImagesInFlight = new List<Fence>();
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
		/// Geometry resource store.
		/// </summary>
		public new VKGeometryDispatcher GeometryDispatcher => base.GeometryDispatcher as VKGeometryDispatcher;

		/// <summary>
		/// Window or area where the library will render all images.
		/// </summary>
		protected Surface DrawingSurface { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		protected VKSwapChain Swapchain { get; private set; }

		/// <summary>
		/// Current renderer drawing pipeline.
		/// </summary>
		protected VKPipeline Pipeline { get; private set; }

		/// <summary>
		/// Configure the default graphic render pass.
		/// A render pass is like a descriptor that tells to the graphics
		/// card which resources are needed from a rendering step.
		/// </summary>
		protected VKRenderPass DefaultRenderPass { get; private set; }

		/// <summary>
		/// Command pool, makes all the command buffers.
		/// </summary>
		protected List<CommandPool> Commands { get; private set; }

		/// <summary>
		/// Command buffers.
		/// </summary>
		protected List<CommandBuffer[]> CommandBuffers { get; private set; }

		/// <summary>
		/// Swapchain presentation semaphore.
		/// </summary>
		protected List<Semaphore> ImageAvailableSemaphores { get; private set; }

		/// <summary>
		/// Swapchain render finish semaphore.
		/// </summary>
		protected List<Semaphore> RenderFinishedSemaphores { get; private set; }

		/// <summary>
		/// Syncronization fences.
		/// </summary>
		protected List<Fence> InFlightFences { get; private set; }

		/// <summary>
		/// Fences for all images that are actually in elaboration status.
		/// </summary>
		protected List<Fence> ImagesInFlight { get; private set; }

		/// <summary>
		/// Maximum pending frames per CPU clock tick.
		/// </summary>
		public int MaxFrameInFlight { get; private set; }

		/// <summary>
		/// Current frame in the pending frame window.
		/// </summary>
		private int CurrentFrame { get; set; } = 0;

		/// <summary>
		/// Initialize the graphic library.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			// - Create a Vulkan instance, this instance must be alive during all the game execution
			Library.Initialize(TargetSurface);

			// - Makes a new device manager
			PhysicalDevice.Initialize(TargetSurface);

			// - Initialize the physical device on this drawing area
			GraphicDevice.Initialize();

			// - Initialize index and vert manager
			VertexManager.Setup();
			IndexManager.Setup();

			// - Makes syncronization semaphores and fances CPU<->GPU
			for (int i = 0; i < MaxFrameInFlight; ++i)
			{
				CommandBuffers.Add(null);
				Commands.Add(GraphicDevice.Handle.CreateCommandPool(GraphicDevice.GraphicQueueIndex, CommandPoolCreateFlags.ResetCommandBuffer));
				ImageAvailableSemaphores.Add(GraphicDevice.Handle.CreateSemaphore());
				RenderFinishedSemaphores.Add(GraphicDevice.Handle.CreateSemaphore());
				InFlightFences.Add(GraphicDevice.Handle.CreateFence(FenceCreateFlags.Signaled));
			}
		}

		/// <summary>
		/// Configure the render for rendering.
		/// </summary>
		public override void ConfigureRendering()
		{
			base.ConfigureRendering();

			// - Initialize the swap chain
			Swapchain.Initialize(PhysicalDevice.DrawingSurface, TargetSurface.SurfaceSize);

			// - Initialzie the swapchain images
			Swapchain.CreateImageViews();

			// - Configure the default render pass
			DefaultRenderPass.Initialize((Format)Swapchain.CurrentFormat.Format);

			// - Load shaders
			ShaderManager.CreateShaders();

			// - Gets default shaders
			var defaultShader = ShaderManager[typeof(DefaultShader)] as VKShaderInstance;
			Pipeline.Shader = defaultShader;

			// - Initialize pipeline
			Pipeline.Initialize(TargetSurface.SurfaceSize);

			// - Build swapchain framebuffers
			Swapchain.CreateFrameBuffers(DefaultRenderPass);

			for (int i = 0; i < Swapchain.Images.Count(); ++i)
				ImagesInFlight.Add(null);
		}

		/// <summary>
		/// 
		/// </summary>
		public override void LoadAttachments(IEnumerable<Resource.IDescriptor> resources)
		{
			// - Skip buffer creation if no resources to load
			if (resources.Count() <= 0) return;

			// - Creates a command buffer for data transfer
			var commands = GraphicDevice.Handle.AllocateCommandBuffer(Commands[0], CommandBufferLevel.Primary);

			commands.Begin(CommandBufferUsageFlags.OneTimeSubmit);

			foreach (Resource.IDescriptor res in resources)
			{
				switch (res)
				{
					case Shape.Descriptor shape:

						// - Loads the geometry
						Geometry pointer = GeometryDispatcher.Load(shape);

						// - Set resource loaded in the source thread
						shape.SourceScheduler.Add(() =>
						{
							shape.SetResource(pointer);
							shape.SetState(Resource.State.Loaded);
						});

						break;

					default:
						throw new Exception($"Resource {res.GetType().Name} does not has a loader.");
				}

			}

			{
				// - Upload invalidated vertexs
				VertexManager.Upload(commands, 1000);

				// - Upload invalidated indexes
				IndexManager.Upload(commands, 1000);

			}
			commands.End();

			// - Perform all attachment operations
			GraphicDevice.GraphicQueue.Submit
			(
				new SubmitInfo
				{ 
					CommandBuffers = new[] { commands } 
				},
				null
			);

			// - Free attachment command buffer
			Commands[0].FreeCommandBuffers(commands);

			// - Wait the GPU (we must operate only when GPU is idle)
			GraphicDevice.Handle.WaitIdle();
		}

		/// <summary>
		/// Render a set of asssets on the screen.
		/// </summary>
		public override void Render(Asset[] assets)
		{
			base.Render(assets);


			CommandBuffers[CurrentFrame] = GraphicDevice.Handle.AllocateCommandBuffers(Commands[CurrentFrame], CommandBufferLevel.Primary, (uint)Swapchain.Images.Count());	


			for (int index = 0; index < Swapchain.Images.Count(); index++)
			{
				var commandBuffer = CommandBuffers[CurrentFrame][index];

				commandBuffer.Begin(CommandBufferUsageFlags.SimultaneousUse);

				commandBuffer.BeginRenderPass(DefaultRenderPass.Handle, Swapchain.VideoBuffers[index].Handler, new Rect2D(Swapchain.SurfaceSize), (ClearValue)(0F, 1F, 0F, 1F), SubpassContents.Inline);

				if (assets != null)
				{
					foreach (Asset asset in assets)
					{
						// - Gets the mesh from the mesh store
						var mesh = GeometryDispatcher.Get(asset.Mesh);

						VertexManager.Bind(commandBuffer);

						IndexManager.Bind(commandBuffer, (int)mesh.Indexes.Offset);

						commandBuffer.BindPipeline(PipelineBindPoint.Graphics, Pipeline.PipelineInstance);

						commandBuffer.DrawIndexed((uint)mesh.Indexes.Count, (uint)mesh.Indexes.Count / 3, 0, 0, 0);
					}
				}

				commandBuffer.EndRenderPass();

				commandBuffer.End();
			}


			uint nextImage = Swapchain.Handle.AcquireNextImage(uint.MaxValue, ImageAvailableSemaphores[CurrentFrame], null);

			if (ImagesInFlight[(int)nextImage] != null)
			{
				GraphicDevice.Handle.WaitForFences(ImagesInFlight[(int)nextImage], true, UInt64.MaxValue);
			}

			// - Mark the image as now being in use by this frame
			ImagesInFlight[(int)nextImage] = InFlightFences[CurrentFrame];

			try
			{
				GraphicDevice.Handle.ResetFences(InFlightFences[CurrentFrame]);

				GraphicDevice.GraphicQueue.Submit
				(
					new SubmitInfo
					{
						CommandBuffers = new[] { CommandBuffers[CurrentFrame][nextImage] },
						SignalSemaphores = new[] { RenderFinishedSemaphores[CurrentFrame] },
						WaitDestinationStageMask = new[] { PipelineStageFlags.ColorAttachmentOutput },
						WaitSemaphores = new[] { ImageAvailableSemaphores[CurrentFrame] }
					},
					InFlightFences[CurrentFrame]
				);
			}
			catch (DeviceLostException)
			{
				InvalidateDevice();
			}

			try
			{
				var present = GraphicDevice.PresentQueue.Present(RenderFinishedSemaphores[CurrentFrame], Swapchain.Handle, nextImage, new Result[1]);

				switch (present)
				{
					case Result.Suboptimal:
						InvalidateGraphics();
						break;
				}

				if (present == Result.Success)
				{
					GraphicDevice.Handle.WaitForFences(InFlightFences[CurrentFrame], true, UInt64.MaxValue);
					Commands[(CurrentFrame + 1) % MaxFrameInFlight].FreeCommandBuffers(CommandBuffers[(CurrentFrame + 1) % MaxFrameInFlight]);
					CommandBuffers[(CurrentFrame + 1) % MaxFrameInFlight] = null;
				}
			}
			catch
			{
				InvalidateGraphics();
			}
			finally
			{
				
			}
			
			CurrentFrame = (CurrentFrame + 1) % MaxFrameInFlight;
		}

		/// <summary>
		/// Do all syncronization things to stop graphic loop
		/// </summary>
		public virtual void CloseRendering()
		{

		}

		/// <summary>
		/// Configure the graphic library to work with a new surface size.
		/// </summary>
		/// <param name="videoSurface">Target surface.</param>
		public override void InvalidateGraphics()
		{
			base.InvalidateGraphics();

			// - Wait the GPU (we must operate only when GPU is idle)
			GraphicDevice.Handle.WaitIdle();

			// - Dispose render pass
			DefaultRenderPass.Dispose();

			// - Dispose the pipeline
			Pipeline.Dispose();

			// - Dipose current swapchain
			Swapchain.Dispose();

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
			GraphicDevice.Handle.WaitIdle();

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

			for (int i = 0; i < MaxFrameInFlight; ++i)
			{
				if (CommandBuffers[i] != null)
				{
					foreach (var buffer in CommandBuffers[i]) buffer.Reset();
					Commands[i].FreeCommandBuffers(CommandBuffers[i]);
				}
			}

			foreach (Fence fence in InFlightFences) fence.Destroy();
			foreach (Semaphore sem in ImageAvailableSemaphores) sem.Destroy();
			foreach (Semaphore sem in RenderFinishedSemaphores) sem.Destroy();

			// - Dispose device
			GraphicDevice.Dispose();

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

			GraphicDevice.Handle.WaitForFences(InFlightFences.ToArray(), true, UInt64.MaxValue);

			// - Wait the GPU (we must operate only when GPU is idle)
			GraphicDevice.Handle.WaitIdle();

			// - Dispose render pass
			DefaultRenderPass.Dispose();

			// - Dispose the pipeline
			Pipeline.Dispose();

			// - Dipose current swapchain
			Swapchain.Dispose();

			// - Dispose vertex and index managers
			VertexManager.Dispose();
			IndexManager.Dispose();

			// - Release all VK shaders
			ShaderManager.Dispose();

			for (int i = 0; i < MaxFrameInFlight; ++i)
			{
				if (CommandBuffers[i] != null)
				{
					foreach (var buffer in CommandBuffers[i]) buffer.Reset();
				}
			}

			foreach (CommandPool pool in Commands) {  pool?.Destroy(); }
			foreach (Fence fence in InFlightFences) fence?.Destroy();
			foreach (Semaphore sem in ImageAvailableSemaphores) sem?.Destroy();
			foreach (Semaphore sem in RenderFinishedSemaphores) sem?.Destroy();

			// - Dispose device
			GraphicDevice.Dispose();

			// - Dispose vulkan
			Library.Dispose();
		}

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
