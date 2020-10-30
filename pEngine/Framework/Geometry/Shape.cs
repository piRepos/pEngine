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

			Tesselation = TesselationType.Auto;
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
		/// Gets or sets the required tessellation type.
		/// </summary>
		public TesselationType Tesselation { get; set; }

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
				TesselationType = Tesselation,
				SourceScheduler = scheduler
			};
		}

		/// <summary>
		/// All supported types of tesselation (triangulation system).
		/// </summary>
		public enum TesselationType
		{
			/// <summary>
			/// No tesselation applied.
			/// </summary>
			None = 0x00,

			/// <summary>
			/// The engine will choose the best algorithm based on the avaiability.
			/// </summary>
			Auto = 0xFF,

			/// <summary>
			/// Single CPU (single core) Ear-Cut algorithm.
			/// </summary>
			CPU1EarCut = 0x01,

			/// <summary>
			/// Single CPU (single core) Delaunay algorithm.
			/// </summary>
			CPU1Delaunay = 0x02,

			/// <summary>
			/// Single GPU (CUDA core multithreading) Ear-Cut algorithm.
			/// </summary>
			GPU1EarCut = 0x11,

			/// <summary>
			/// Single GPU (CUDA core multithreading) Delaunay algorithm.
			/// </summary>
			GPU1Delaunay = 0x12
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
			/// Gets or sets the required tessellation type.
			/// </summary>
			public TesselationType TesselationType { get; set; }

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
