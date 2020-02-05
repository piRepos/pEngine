using System;

namespace pEngine.Environment.Video
{
	/// <summary>
	/// Provide an interface for a generic drawing surface,
	/// crossplatform surface like desktop windows or smartphone applications.
	/// </summary>
	public interface ISurface : IContext
	{
		/// <summary>
		/// Drawing surface area size.
		/// </summary>
		Vector2i SurfaceSize { get; }

		/// <summary>
		/// Pixel ratio scaling.
		/// </summary>
		Vector2 Scaling { get; }

		/// <summary>
		/// Raised on surface invalidation.
		/// </summary>
		event EventHandler Invalidating;

		/// <summary>
		/// Raised on surface initialization complete.
		/// </summary>
		event EventHandler Initialized;

		/// <summary>
		/// Raised after this surface is resized.
		/// </summary>
		event EventHandler<Vector2> Resized;
	}
}
