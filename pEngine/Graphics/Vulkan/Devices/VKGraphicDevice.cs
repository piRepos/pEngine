using System;
using System.Linq;
using System.Collections.Generic;

using pEngine.Environment.Video;
using pEngine.Graphics.Devices;

using SharpVk;
using SharpVk.Khronos;

namespace pEngine.Graphics.Vulkan.Devices
{
	/// <summary>
	/// A Vulkan's virtual device which allows rendering.
	/// </summary>
	public class VKGraphicDevice : GraphicDevice
	{
		/// <summary>
		/// Makes a new instance of <see cref="VKGraphicDevice"/> class.
		/// </summary>
		public VKGraphicDevice(VKPhysicalDevice device) : base(device)
		{
			
		}

		/// <summary>
		/// Vulkan graphic device's handle.
		/// </summary>
		public Device Handle { get; private set; }

		/// <summary>
		/// This queue is able to send logic commands to the GPU.
		/// </summary>
		public Queue GraphicQueue { get; private set; }

		/// <summary>
		/// This queue is able to send drawing commands to the GPU.
		/// </summary>
		public Queue PresentQueue { get; private set; }

		/// <summary>
		/// This queue is able to send commands for general purpose computing.
		/// </summary>
		public Queue ComputeQueue { get; private set; }

		/// <summary>
		/// Graphics queue index.
		/// </summary>
		public uint GraphicQueueIndex { get; private set; }

		/// <summary>
		/// Present queue index.
		/// </summary>
		public uint PresentQueueIndex { get; private set; }

		/// <summary>
		/// Compute queue index.
		/// </summary>
		public uint ComputeQueueIndex { get; private set; }

		/// <summary>
		/// Associated Vulkan's physical device.
		/// </summary>
		public new VKPhysicalDevice Physical => base.Physical as VKPhysicalDevice;

		/// <summary>
		/// Initialize the graphic device.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			// - Take presentation and graphic queue
			GraphicQueueIndex = GetSuitableQueues(Physical.Handle, QueueFlags.Graphics).First();
			PresentQueueIndex = GetPresentQueues(Physical.Handle, Physical.DrawingSurface.Handle).First();
			ComputeQueueIndex = GetSuitableQueues(Physical.Handle, QueueFlags.Compute).First();

			var queues = new DeviceQueueCreateInfo[]
			{
				new DeviceQueueCreateInfo { QueueFamilyIndex = GraphicQueueIndex, QueuePriorities = new[] { 1f } },
				new DeviceQueueCreateInfo { QueueFamilyIndex = PresentQueueIndex, QueuePriorities = new[] { 1f } },
				new DeviceQueueCreateInfo { QueueFamilyIndex = ComputeQueueIndex, QueuePriorities = new[] { 1f } }
			};

			// - Create logical device
			Handle = Physical.Handle.CreateDevice(queues, null, new[] { KhrExtensions.Swapchain });

			// - Gets the logical queues
			GraphicQueue = Handle.GetQueue(GraphicQueueIndex, 0);
			PresentQueue = Handle.GetQueue(PresentQueueIndex, 0);
			ComputeQueue = Handle.GetQueue(ComputeQueueIndex, 0);
		}

		#region Device utilities

		public static implicit operator Device(VKGraphicDevice device)
		{
			return device.Handle;
		}

		private IEnumerable<uint> GetSuitableQueues(SharpVk.PhysicalDevice device, QueueFlags type)
		{
			// - Gets GPU command queues
			var queues = device.GetQueueFamilyProperties();

			for (uint i = 0; i < queues.Length; ++i)
			{
				var queue = queues[i];

				if (queue.QueueFlags.HasFlag(type))
				{
					yield return i;
				}
			}
		}

		private IEnumerable<uint> GetPresentQueues(SharpVk.PhysicalDevice device, SharpVk.Khronos.Surface surface)
		{
			// - Gets GPU command queues
			var queues = device.GetQueueFamilyProperties();

			for (uint i = 0; i < queues.Length; ++i)
			{
				var queue = queues[i];

				if (device.GetSurfaceSupport(i, surface))
				{
					yield return i;
				}
			}
		}

		#endregion

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
				Handle.Destroy();
			}

			base.Dispose(disposing);
		}
	}
}
