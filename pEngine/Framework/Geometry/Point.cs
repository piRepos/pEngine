using System;

namespace pEngine.Framework.Geometry
{
	/// <summary>
	/// Refers to a point of a gemetric shape. 
	/// </summary>
	public class Point
	{

		/// <summary>
		/// Makes a new instance of <see cref="Point"/> class.
		/// </summary>
		/// <param name="position">Point position initializer.</param>
		/// <param name="color">Point color initializer.</param>
		/// <param name="type">Point type.</param>
		public Point(Vector2 position, PointType type = PointType.Vertex)
		{
			Position = position;
			Type = type;
		}

		/// <summary>
		/// Makes a new instance of <see cref="Point"/> class.
		/// </summary>
		public Point()
		{
			Position = Vector2.Zero;
			Type = PointType.Marker;
		}

		/// <summary>
		/// Point position.
		/// </summary>
		public Vector2 Position { get; set; }

		/// <summary>
		/// Point type.
		/// </summary>
		public PointType Type { get; set; }

		/// <summary>
		/// Gets a struct descriptor for this point.
		/// </summary>
		/// <returns>A <see cref="PointDescriptor"/> struct value.</returns>
		public PointDescriptor GetDescriptor()
		{
			return new PointDescriptor
			{
				Position = Position,
				Type = Type
			};
		}
	}

	public struct PointDescriptor
	{
		/// <summary>
		/// Point position.
		/// </summary>
		public Vector2 Position { get; set; }

		/// <summary>
		/// Point type.
		/// </summary>
		public PointType Type { get; set; }
	}

	public enum PointType
	{
		/// <summary>
		/// This point will be used as a drawing anchor point.
		/// </summary>
		Vertex = 0,

		/// <summary>
		/// A bezier interpolation will generate all vertex from the last anchor
		/// to the next one.
		/// </summary>
		Curve = 1,

		/// <summary>
		/// The point does not modify the shape and wont be rendered.
		/// </summary>
		Marker = 2
	}
}
