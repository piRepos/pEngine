using System;
using System.Collections.Generic;
using System.Text;

namespace pEngine.Framework.Geometry
{
	/// <summary>
	/// 2D planar face delimited by <see cref="HalfEdge"/>.
	/// </summary>
	public class Face
	{
		/// <summary>
		/// Creates a new instance of <see cref="Face"/> class.
		/// </summary>
		/// <param name="startingEdge">Face's starting edge.</param>
		public Face(HalfEdge startingEdge)
		{
			FirstEdge = startingEdge;
		}

		/// <summary>
		/// First face's edge.
		/// </summary>
		public HalfEdge FirstEdge { get; set; }

		/// <summary>
		/// Gets a struct descriptor for this face.
		/// </summary>
		/// <returns>A <see cref="FaceDescriptor"/> struct value.</returns>
		public FaceDescriptor GetDescriptor(IList<HalfEdge> edges)
		{
			return new FaceDescriptor
			{
				FirstEdge = edges.IndexOf(FirstEdge)
			};
		}
	}

	public struct FaceDescriptor
	{
		/// <summary>
		/// Half edge starting point.
		/// </summary>
		public int FirstEdge { get; set; }
	}
}
