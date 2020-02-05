using System;
using System.Collections.Generic;
using System.Text;

namespace pEngine.Framework.Geometry
{
	/// <summary>
	/// A directed edge between two point.
	/// </summary>
	public class HalfEdge
	{
		/// <summary>
		/// Makes a new instance of <see cref="HalfEdge"/> class.
		/// </summary>
		/// <param name="p1">Edge starting point.</param>
		/// <param name="p2">Edge end point.</param>
		public HalfEdge(Point start, Point end)
		{
			Start = start;
			End = end;
		}


		/// <summary>
		/// Edge starting point.
		/// </summary>
		public Point Start { get; set; }

		/// <summary>
		/// Edge end point.
		/// </summary>
		public Point End { get; set; }

		/// <summary>
		/// Gets the edge in the opposite direction.
		/// </summary>
		public HalfEdge Twin { get; set; }

		/// <summary>
		/// Gets the next connected edge.
		/// </summary>
		public HalfEdge Next { get; set; }

		/// <summary>
		/// Gets the previous edge in the current direction.
		/// </summary>
		public HalfEdge Prev { get; set; }


		/// <summary>
		/// Gets a struct descriptor for this half edge.
		/// </summary>
		/// <returns>A <see cref="HalfEdgeDescriptor"/> struct value.</returns>
		public HalfEdgeDescriptor GetDescriptor(IList<Point> points, IList<HalfEdge> edges)
		{
			int start = -1, end = -1, i = 0;
			foreach (var point in points)
			{
				if (Equals(point, Start)) start = i;
				if (Equals(point, End)) end = i;

				i++;
			}

			// - Validation check
			if (start < 0 || end < 0) 
				throw new Exception("Inconsistent data struct.");

			int twin = -1, next = -1, prev = -1, j = 0;
			foreach (var edge in edges)
			{
				if (Equals(edge, Twin)) twin = j;
				if (Equals(edge, Next)) next = j;
				if (Equals(edge, Prev)) prev = j;

				j++;
			}

			return new HalfEdgeDescriptor
			{
				Start = (uint)start,
				End = (uint)end,
				Twin = twin,
				Next = next,
				Prev = prev
			};
		}
	}

	public struct HalfEdgeDescriptor
	{
		/// <summary>
		/// Half edge starting point.
		/// </summary>
		public uint Start { get; set; }

		/// <summary>
		/// Half edge end point.
		/// </summary>
		public uint End { get; set; }

		/// <summary>
		/// Half edge in the opposite direction.
		/// </summary>
		public int Twin { get; set; }

		/// <summary>
		/// Next connected half edge in the current direction.
		/// </summary>
		public int Next { get; set; }

		/// <summary>
		/// Previous connected half edge in the current direction.
		/// </summary>
		public int Prev { get; set; }
	}
}
