using System;
using System.Linq;
using System.Collections.Generic;

using pEngine.Graphics.Devices;
using pEngine.Environment.Video;

using SharpVk;
using SharpVk.Khronos;

namespace pEngine.Graphics.Vulkan.Devices
{
	/// <summary>
	/// Implements a vulkan physical device.
	/// </summary>
	public class VKPhysicalDevice : Graphics.Devices.PhysicalDevice
	{
		/// <summary>
		/// Makes a new instance of <see cref="VKPhysicalDevice"/> class.
		/// </summary>
		public VKPhysicalDevice(VulkanLibrary library) : base(library)
		{

		}

		/// <summary>
		/// Target drawing surface.
		/// </summary>
		public Surface DrawingSurface { get; private set; }

		/// <summary>
		/// Vulkan physical device.
		/// </summary>
		public SharpVk.PhysicalDevice Handle { get; private set; }

		/// <summary>
		/// Initializes the physical device.
		/// </summary>
		public override void Initialize(ISurface surface)
		{
			base.Initialize(surface);

			// - Get the vulkan library
			var library = Library as VulkanLibrary;

			// - Makes a new drawing surface
			DrawingSurface = surface.CreateVKSurface(library.Handle);

			// - Gets all devices
			var devices = library.Handle.EnumeratePhysicalDevices();

			// - Filter all non graphic devices
			var suitableDevices = FilterSuitableDevices(devices, DrawingSurface);

			// - Order devices by priority (performance)
			var orderedDevices = OrderDeviceBySuitability(suitableDevices);

			// - Takes the first suitable device
			Handle = orderedDevices.First();
		}

		#region Device utilities

		private IEnumerable<SharpVk.PhysicalDevice> FilterSuitableDevices(IEnumerable<SharpVk.PhysicalDevice> devices, Surface surface)
		{
			bool SuitabilityCriteria(SharpVk.PhysicalDevice device)
			{
				// - Gets device features
				var features = device.GetFeatures();

				// - Check if this GPU has a graphic queue
				if (GetSuitableQueues(device, QueueFlags.Graphics).Count() <= 0)
					return false;

				// - Check if this GPU has a compatible drawing surface
				if (GetPresentQueues(device, surface).Count() <= 0)
					return false;

				// - We need at least a geometry shader support
				if (!features.GeometryShader)
					return false;

				return true;
			}

			return devices.Where(SuitabilityCriteria);
		}

		private IEnumerable<SharpVk.PhysicalDevice> OrderDeviceBySuitability(IEnumerable<SharpVk.PhysicalDevice> devices)
		{
			uint OrderCriteria(SharpVk.PhysicalDevice device)
			{
				uint score = 0;

				// - Gets device properties
				var props = device.GetProperties();

				// - Physical dedicated graphics card has an high priority
				score += props.DeviceType == PhysicalDeviceType.DiscreteGpu ? 1000U : 0U;

				// - Maximum possible size of textures affects graphics quality
				score += props.Limits.MaxImageDimension2D;

				// TODO: Take in account other performance properties 

				return score;
			}

			return devices.OrderBy(OrderCriteria);
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

		private IEnumerable<uint> GetPresentQueues(SharpVk.PhysicalDevice device, Surface surface)
		{
			// - Gets GPU command queues
			var queues = device.GetQueueFamilyProperties();

			for (uint i = 0; i < queues.Length; ++i)
			{
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
				DrawingSurface.Destroy();
			}

			base.Dispose(disposing);
		}
	}
}
