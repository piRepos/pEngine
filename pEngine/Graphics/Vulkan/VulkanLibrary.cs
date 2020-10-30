using System;
using System.Linq;
using System.Collections.Generic;

using pEngine.Diagnostic;
using pEngine.Environment;
using pEngine.Environment.Video;

using SharpVk;
using SharpVk.Multivendor;

namespace pEngine.Graphics.Vulkan
{
	/// <summary>
	/// An implementation of vulkan library for pEngine.
	/// </summary>
	public class VulkanLibrary : GraphicLibrary
	{
		/// <summary>
		/// Makes a new instance of <see cref="VulkanLibrary"/> class.
		/// </summary>
		public VulkanLibrary()
		{

		}

		/// <summary>
		/// Vulkan library instance.
		/// </summary>
		public Instance Handle { get; private set; }

		/// <summary>
		/// Initializes the graphic library using the specified surface target.
		/// </summary>
		/// <param name="targetSurface">The surface which the library will target.</param>
		public override void Initialize(ISurface targetSurface)
		{
			base.Initialize(targetSurface);

			SharpVk.Version engineVersion = new SharpVk.Version
			(
				Engine.Version.Major,
				Engine.Version.Minor,
				Engine.Version.Revision
			);

			SharpVk.Version gameVersion = new SharpVk.Version
			(
				Engine.GameVersion.Major,
				Engine.GameVersion.Minor,
				Engine.GameVersion.Revision
			);

			ApplicationInfo appInfo = new ApplicationInfo
			{
				ApplicationName = Engine.GameName,
				EngineName = Engine.Name,
				EngineVersion = engineVersion,
				ApplicationVersion = gameVersion
			};

			DebugReportCallbackCreateInfo debugInfo = new DebugReportCallbackCreateInfo
			{
				Callback = DebugCallback,
				Flags = DebugFlags(),
				UserData = IntPtr.Zero
			};

			List<string> validationLayers = new List<string>();
			var layers = Instance.EnumerateLayerProperties();

			if (layers.Any(x => x.LayerName == "VK_LAYER_LUNARG_standard_validation"))
				validationLayers.Add("VK_LAYER_LUNARG_standard_validation");

			if (layers.Any(x => x.LayerName == "VK_LAYER_LUNARG_monitor"))
				validationLayers.Add("VK_LAYER_LUNARG_monitor");

			if (layers.Any(x => x.LayerName == "VK_LAYER_KHRONOS_validation"))
				validationLayers.Add("VK_LAYER_KHRONOS_validation");

			// - Creates the vulkan instance
			Handle = Instance.Create(validationLayers.ToArray(), targetSurface.Extensions, null, appInfo, debugInfo);
		}

		/// <summary>
		/// Converts pEngine debug flags to Vulkan debug flags.
		/// </summary>
		/// <returns>Vulkan debug flags.</returns>
		private DebugReportFlags DebugFlags()
		{
			DebugReportFlags debug = DebugReportFlags.Debug | DebugReportFlags.Error | DebugReportFlags.Information | DebugReportFlags.PerformanceWarning | DebugReportFlags.Warning;

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

		/// <summary>
		/// Vulkan debug callback.
		/// </summary>
		private Bool32 DebugCallback(DebugReportFlags flags, DebugReportObjectType objectType, ulong @object, HostSize location, int messageCode, string pLayerPrefix, string pMessage, IntPtr pUserData)
		{
			Console.WriteLine(pMessage);
			return false;
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
				Handle.Destroy();
			}

			base.Dispose(disposing);
		}
	}
}
