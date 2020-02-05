using System;
using System.Linq;
using System.Collections.Generic;

using pEngine.Utils.Threading;

namespace pEngine.Framework.Geometry
{
	using Geometry = Graphics.Resources.Geometry;

	/// <summary>
	/// This class handles the representation of eny type of
	/// 2D geometric shape.
	/// </summary>
	public class Shape : Resource
	{
		/// <summary>
		/// Creates a new instance of <see cref="Shape"/> class.
		/// </summary>
		public Shape()
		{
			// - Initialize collections
			Points = new List<Point>();
			Edges = new List<HalfEdge>();
			Faces = new List<Face>();
		}

		/// <summary>
		/// Shape points.
		/// </summary>
		public List<Point> Points { get; }

		/// <summary>
		/// Shape edges.
		/// </summary>
		public List<HalfEdge> Edges { get; }

		/// <summary>
		/// Shape faces.
		/// </summary>
		public List<Face> Faces { get; }

		/// <summary>
		/// Shape attachment -> logic pointer to the GPU resource.
		/// </summary>
		public Geometry Attachment { get; private set; }


		#region Resource descriptor

		/// <summary>
		/// Generates a resource descriptor for this resource.
		/// </summary>
		/// <returns>A resource descriptor.</returns>
		internal override IDescriptor GetDescriptor(Scheduler scheduler)
		{
			return new Descriptor
			{
				SetState = (state) => SetLoadingState(state),
				SetResource = (resource) => Attachment = resource,
				Points = Points.Select(x => x.GetDescriptor()).ToArray(),
				Edges = Edges.Select(x => x.GetDescriptor(Points, Edges)).ToArray(),
				Faces = Faces.Select(x => x.GetDescriptor(Edges)).ToArray(),
				SourceScheduler = scheduler
			};
		}

		/// <summary>
		/// Descriptor for a generic DCEL implemented shape.
		/// </summary>
		public new struct Descriptor : IDescriptor
		{
			/// <summary>
			/// Sets the resource state.
			/// </summary>
			public Action<State> SetState { get; set; }

			/// <summary>
			/// Gets the scheduler that will handles the callback.
			/// </summary>
			public Scheduler SourceScheduler { get; set; }

			/// <summary>
			/// Sets the pointer to the loaded resource.
			/// </summary>
			public Action<Geometry> SetResource { get; set; }

			/// <summary>
			/// List of points.
			/// </summary>
			public PointDescriptor[] Points { get; set; }

			/// <summary>
			/// List of edges.
			/// </summary>
			public HalfEdgeDescriptor[] Edges { get; set; }

			/// <summary>
			/// All shape faces.
			/// </summary>
			public FaceDescriptor[] Faces { get; set; }
		}

		#endregion
	}
}
