using System;
using System.Collections.Generic;
using System.Text;

namespace pEngine.Graphics.FrameBuffers
{
	/// <summary>
	/// Structure describing a supported swapchain format-color space pair.
	/// </summary>
	public struct SurfaceFormat
	{
		/// <summary>
		/// A Format that is compatible with the specified surface.
		/// </summary>
		public Format Format;

		/// <summary>
		/// A presentation ColorSpaceKHR that is compatible with the surface.
		/// </summary>
		public ColorSpace ColorSpace;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="format"></param>
		/// <param name="colorSpace"></param>
		public SurfaceFormat(Format format, ColorSpace colorSpace)
		{
			Format = format;
			ColorSpace = colorSpace;
		}

		#region Conversion

		public static implicit operator SurfaceFormat(SharpVk.Khronos.SurfaceFormat format)
		{
			return new SurfaceFormat((Format)format.Format, (ColorSpace)format.ColorSpace);
		}

		#endregion
	}
}
