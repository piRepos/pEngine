using System;
using System.Collections.Generic;

using SharpVk;

using pEngine.Graphics.Vulkan.Vertexs;
using pEngine.Graphics.Vulkan.Shading;
using pEngine.Graphics.Vulkan.Devices;

namespace pEngine.Graphics.Vulkan
{
	/// <summary>
	/// Manage the Vulkan graphic pipeline initialization.
	/// </summary>
	public class VKPipeline : Pipelines.Pipeline
	{
		/// <summary>
		/// Makes a new instance of <see cref="RenderPipeline"/> class.
		/// </summary>
		public VKPipeline(VKGraphicDevice device, bool compute) : base(device , compute)
		{
		}

		/// <summary>
		/// The device on which this pipline will be binded to.
		/// </summary>
		protected new VKGraphicDevice GraphicDevice => base.GraphicDevice as VKGraphicDevice;

		/// <summary>
		/// Contains the pipeline layout.
		/// </summary>
		public PipelineLayout Layout { get; private set; }

		/// <summary>
		/// Vulkan render pipeline.
		/// </summary>
		public Pipeline PipelineInstance { get; private set; }

		/// <summary>
		/// Initializes the render pipeline.
		/// </summary>
		public void Initialize(RenderPass renderPass)
		{
			Disposed = false;
			
			// - Prepare the pipeline by creating the layout
			Layout = GraphicDevice.Handle.CreatePipelineLayout(null, null);

			// - Shader pipeline attachment
			var shaders = new List<PipelineShaderStageCreateInfo>();

			if (Shader.HasVertexShader && !IsCompute)
			{
				var vkShader = Shader as VKShaderInstance;
				shaders.Add(new PipelineShaderStageCreateInfo
				{
					Stage = ShaderStageFlags.Vertex,
					Module = vkShader.VKVertexShader,
					Name = "main"
				});
			}

			if (Shader.HasFragmentShader && !IsCompute)
			{
				var vkShader = Shader as VKShaderInstance;
				shaders.Add(new PipelineShaderStageCreateInfo
				{
					Stage = ShaderStageFlags.Fragment,
					Module = vkShader.VKFragmentShader,
					Name = "main"
				});
			}

			if (Shader.HasComputeShader && IsCompute)
			{
				var vkShader = Shader as VKShaderInstance;
				shaders.Add(new PipelineShaderStageCreateInfo
				{
					Stage = ShaderStageFlags.Compute,
					Module = vkShader.VKComputeShader,
					Name = "main"
				});
			}

			PipelineInstance = GraphicDevice.Handle.CreateGraphicsPipeline
			(
				null,
				shaders.ToArray(),
				new PipelineVertexInputStateCreateInfo
				{
					VertexBindingDescriptions = new[] { VKVertexBuffer.BindingDescriptor },
					VertexAttributeDescriptions = new[] { VKVertexBuffer.PositionDescriptor, VKVertexBuffer.ColorDescriptor }
				},
				new PipelineInputAssemblyStateCreateInfo
				{
					PrimitiveRestartEnable = false,
					Topology = PrimitiveTopology.TriangleList
				},
				new PipelineRasterizationStateCreateInfo
				{
					DepthClampEnable = false,
					RasterizerDiscardEnable = false,
					PolygonMode = PolygonMode.Fill,
					LineWidth = 1,
					CullMode = CullModeFlags.Front,
					FrontFace = FrontFace.Clockwise,
					DepthBiasEnable = false,
				},

				Layout, renderPass, 0, null, -1,

				viewportState: new PipelineViewportStateCreateInfo
				{
					Viewports = new[]
					{
						new Viewport(0f, 0f, BufferSize.Width, BufferSize.Height, 0, 1)
					},
					Scissors = new[]
					{
						new Rect2D(new Extent2D((uint)BufferSize.Width, (uint)BufferSize.Height))
					}
				},

				colorBlendState: new PipelineColorBlendStateCreateInfo
				{
					LogicOpEnable = false,
					Attachments = new[]
					{
						new PipelineColorBlendAttachmentState
						{
							ColorWriteMask = ColorComponentFlags.R | ColorComponentFlags.G | ColorComponentFlags.B | ColorComponentFlags.A,
							BlendEnable = false
						}
					}
				},

				multisampleState: new PipelineMultisampleStateCreateInfo
				{
					SampleShadingEnable = true,
					RasterizationSamples = SampleCountFlags.SampleCount1,
					MinSampleShading = 1
				}
			);
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
			if (!Disposed)
			{
				// - Destroy layout
				Layout?.Dispose();
				Layout = null;

				// - Free the pipeline
				PipelineInstance?.Dispose();
				PipelineInstance = null;
			}

			base.Dispose(disposing);
		}
	}
}
