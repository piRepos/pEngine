using System;
using System.Collections.Generic;
using System.Text;

using SharpVk;

using pEngine.Graphics.Vulkan.Devices;
using pEngine.Graphics.Pipelines;

namespace pEngine.Graphics.Vulkan
{
	public class VKRenderPass : Graphics.Pipelines.RenderPass
	{
		/// <summary>
		/// Makes a new instance of <see cref="VKRenderPass"/> class.
		/// </summary>
		public VKRenderPass(VKGraphicDevice device) : base(device)
		{

		}

		/// <summary>
		/// Contains the parent graphic device.
		/// </summary>
		protected new VKGraphicDevice GraphicDevice => base.GraphicDevice as VKGraphicDevice;

		/// <summary>
		/// Vulkan render pass handler.
		/// </summary>
		public SharpVk.RenderPass Handle { get; private set; }

		/// <summary>
		/// Creates render passes.
		/// </summary>
		public override void Initialize() => Initialize(null, null);

		/// <summary>
		/// Creates render passes.
		/// </summary>
		public void Initialize(Format surfaceFormat)
		{
			var attrDesc = DefaultAttachmentDescription;

			attrDesc.Format = surfaceFormat;

			Initialize(attrDesc, null);
		}

		/// <summary>
		/// Creates render passes.
		/// </summary>
		public void Initialize(AttachmentDescription? attrDesc, SubpassDescription [] subDesc)
		{
			if (Initialized) return;

			attrDesc = attrDesc ?? DefaultAttachmentDescription;

			subDesc = subDesc ?? new SubpassDescription[] { DefaultGraphicsSubpass };

			Handle = GraphicDevice.Handle.CreateRenderPass
			(
				attrDesc, subDesc, 

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

			base.Initialize();
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
			// Check to see if Dispose has already been called.
			if (!Disposed)
			{
				Handle.Destroy();

				// Note disposing has been done.
				Disposed = true;
			}
		}

		public static AttachmentDescription DefaultAttachmentDescription = new AttachmentDescription
		{
			Samples = SampleCountFlags.SampleCount1,
			LoadOp = AttachmentLoadOp.Clear,
			StoreOp = AttachmentStoreOp.Store,
			StencilLoadOp = AttachmentLoadOp.DontCare,
			StencilStoreOp = AttachmentStoreOp.DontCare,
			InitialLayout = ImageLayout.Undefined,
			FinalLayout = ImageLayout.PresentSource
		};

		public static SubpassDescription DefaultGraphicsSubpass = new SubpassDescription
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
		};



	}
}
