﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using pEngine.Graphics.Data;

using SharpVk;

using Buffer = SharpVk.Buffer;
using pEngine.Graphics.Vulkan.Devices;

namespace pEngine.Graphics.Vulkan.Vertexs
{
	/// <summary>
	/// Manages vertex allocation and store using Vulkan library.
	/// </summary>
	public class VKIndexBuffer : IndexBuffer
	{

		/// <summary>
		/// Makes a new instance of <see cref="VKIndexBuffer"/> class.
		/// </summary>
		public VKIndexBuffer(VKGraphicDevice device, uint initialSize, bool canGrow, double growFactor) : 
			base(initialSize, canGrow, growFactor)
		{
			RenderDevice = device;
			IndexBuffer = null;
		}

		/// <summary>
		/// Stored working device.
		/// </summary>
		protected VKGraphicDevice RenderDevice { get; private set; }

		/// <summary>
		/// Buffer inside the VRAM visible only to the GPU.
		/// </summary>
		public Buffer IndexBuffer { get; private set; }

		/// <summary>
		/// Ausiliary buffer for data transfer between CPU and GPU.
		/// </summary>
		public Buffer StagingBuffer { get; private set; }

		/// <summary>
		/// CPU visible VRAM memory slice.
		/// </summary>
		protected DeviceMemory StagingVRAM { get; private set; }

		/// <summary>
		/// GPU VRAM not acessible from CPU.
		/// </summary>
		protected DeviceMemory VertexVRAM { get; private set; }

		/// <summary>
		/// Setups all the GPU beheviors.
		/// </summary>
		public void Setup()
		{
			uint GetSuitableMemory(PhysicalDeviceMemoryProperties props, MemoryRequirements reqs, MemoryPropertyFlags flags)
			{
				int memIndex = -1;
				for (int i = 0; i < props.MemoryTypes.Length; ++i)
					if ((reqs.MemoryTypeBits & (1 << i)) != 0 && (props.MemoryTypes[i].PropertyFlags & flags) == flags)
						memIndex = i;

				if (memIndex < 0) throw new NotSupportedException("Failed to allocate vertex buffer memory.");

				return (uint)memIndex;
			}

			// - Gets the physical memory properties
			var memory = RenderDevice.Physical.Handle.GetMemoryProperties();

			try
			{
				// - Create the vertex buffer visible from the CPU
				StagingBuffer = RenderDevice.Handle.CreateBuffer
				(
					HeapSize,
					BufferUsageFlags.TransferSource,
					SharingMode.Exclusive,
					null
				);

				// - Create the vertex buffer inside the GPU private memory
				IndexBuffer = RenderDevice.Handle.CreateBuffer
				(
					HeapSize,
					BufferUsageFlags.IndexBuffer | BufferUsageFlags.TransferDestination,
					SharingMode.Exclusive,
					null
				);

				// - Gets all requirements
				var vertReq = IndexBuffer.GetMemoryRequirements();
				var stagReq = StagingBuffer.GetMemoryRequirements();

				var vertMemoryIndex = GetSuitableMemory(memory, vertReq, MemoryPropertyFlags.DeviceLocal);
				var stagMemoryIndex = GetSuitableMemory(memory, stagReq, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent);

				// - Allocate the required memory on the VRAM
				VertexVRAM = RenderDevice.Handle.AllocateMemory(vertReq.Size, vertMemoryIndex);
				StagingVRAM = RenderDevice.Handle.AllocateMemory(stagReq.Size, stagMemoryIndex);

				// - Bind memory to the buffers
				IndexBuffer.BindMemory(VertexVRAM, 0);
				StagingBuffer.BindMemory(StagingVRAM, 0);
			}
			catch
			{
				// TODO: Add exception management
			}
		}

		/// <summary>
		/// Release the memory from the VRAM.
		/// </summary>
		public void ReleaseMemory()
		{
			StagingBuffer?.Dispose();
			StagingBuffer = null;

			IndexBuffer?.Dispose();
			IndexBuffer = null;

			StagingVRAM?.Free();
			VertexVRAM?.Free();
		}


		public override bool Grow(uint size, bool noOvergrowth = false)
		{
			if (!base.Grow(size, noOvergrowth)) return false;

			ReleaseMemory();

			Setup();

			foreach (BufferResource res in UsedSpace)
				res.Invalidate(true);

			return true;
		}

		/// <summary>
		/// Upload the vertexs to the VRAM.
		/// </summary>
		public void Upload(CommandBuffer commands, int maxBytes)
		{
			if (StagingVRAM != null && VertexVRAM != null && IndexBuffer != null && StagingBuffer != null)
			{

				List<BufferCopy> uploadQueue = new List<BufferCopy>();

				// - Order invalidated chunks by priority 
				var invalidated = UsedSpace.Where(x => (x as BufferResource).Invalidated)
					.OrderByDescending(x => (x as BufferResource).UploadPriority);

				// - Keep track of the upload zone
				var uploadRange = (start: 0L, end: 0L);

				foreach (BufferResource chunk in invalidated)
				{
					if (chunk.UploadPriority == int.MaxValue || maxBytes > 0)
					{
						uploadQueue.Add(new BufferCopy(chunk.Offset, chunk.Offset, chunk.Size));
						maxBytes -= (int)chunk.Size;
						chunk.Invalidated = false;
						chunk.UploadPriority = 0;

						uploadRange =
						(
							start: Math.Min(uploadRange.start, chunk.Offset),
							end: Math.Max(uploadRange.end, chunk.Offset + chunk.Size)
						);
					}
				}

				// - No invalidated chunks
				if (uploadQueue.Count <= 0) return;

				// - Calculate upload range
				int uploadOffset = (int)uploadRange.start;
				int uploadSize = (int)(uploadRange.end - uploadRange.start);

				// TODO: This can be improved by splitting large upload areas

				// - Gets the pointer to the staging memory
				IntPtr vram = StagingVRAM.Map(uploadOffset, uploadSize, MemoryMapFlags.None);

				unsafe
				{
					// - Perform the copy
					System.Buffer.MemoryCopy
					(
						IntPtr.Add(RawMemory, uploadOffset).ToPointer(), 
						vram.ToPointer(),
						uploadSize, uploadSize
					);
				}

				// - Unmap the staging memory pointer
				StagingVRAM.Unmap();

				// - Send the upload message to the GPU
				commands.CopyBuffer(StagingBuffer, IndexBuffer, uploadQueue.ToArray());
			}
		}

		/// <summary>
		/// Bind this vertex buffer.
		/// </summary>
		public void Bind(CommandBuffer commands, int offset)
		{
			commands.BindIndexBuffer(IndexBuffer, offset, IndexType.Uint32);
		}

		/// <summary>
		/// Implement IDisposable.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// - Delete buffer resources
			ReleaseMemory();
		}

	}
}
