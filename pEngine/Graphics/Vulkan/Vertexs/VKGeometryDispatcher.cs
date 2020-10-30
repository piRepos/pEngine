using System;
using System.Collections.Generic;

using pEngine.Framework.Geometry;
using pEngine.Graphics.Data;
using pEngine.Graphics.Resources;

namespace pEngine.Graphics.Vulkan.Vertexs
{
	public class VKGeometryDispatcher : GeometryDispatcher
	{
		/// <summary>
		/// Makes a new instance of <see cref="VKGeometryDispatcher"/> class.
		/// </summary>
		/// <param name="vb">Vertex buffer.</param>
		/// <param name="ib">Index buffer.</param>
		public VKGeometryDispatcher(VKVertexBuffer vb, VKIndexBuffer ib)
			: base(vb, ib)
		{

		}

		/// <summary>
		/// Manages raw vertex allocation and the GPU buffer transfer.
		/// </summary>
		protected new VKVertexBuffer VertexManager => base.VertexManager as VKVertexBuffer;

		/// <summary>
		/// Manages raw index allocation and the GPU buffer transfer.
		/// </summary>
		protected new VKIndexBuffer IndexManager => base.IndexManager as VKIndexBuffer;

		/// <summary>
		/// This method must implement the loading business in order to
		/// provide a rendering cached structure.
		/// </summary>
		/// <param name="data">Metadata and data for the loading process.</param>
		/// <returns>A result's descriptor.</returns>
		protected override GeometryResource InternalLoad(Shape.Descriptor data)
		{
			GeometryResource resource = new GeometryResource
			{
				Vertexs = VertexManager.Alloc((uint)data.Points.Length),
				Indexes = IndexManager.Alloc((uint)data.Edges.Length)
			};

			switch (data.TesselationType)
			{
				case Shape.TesselationType.Auto:
				// - Manage automatic tessellation algorithm selection
				case Shape.TesselationType.CPU1EarCut:
					break;
				case Shape.TesselationType.CPU1Delaunay:
					break;
				case Shape.TesselationType.GPU1EarCut:
					break;
				case Shape.TesselationType.GPU1Delaunay:
					break;
			}

			return CopyToGPUBuffer(data, resource);
		}

		/// <summary>
		/// Copy the specified geometric data to an index and a vertex buffer.
		/// </summary>
		/// <param name="data">Data to load inside the GPU.</param>
		/// <param name="resource">Resource to return.</param>
		/// <returns>The updated resource with the new vertex/index values.</returns>
		private GeometryResource CopyToGPUBuffer(Shape.Descriptor data, GeometryResource resource)
		{
			int index = 0;

			// - Loads all processed vertexs
			foreach (PointDescriptor point in data.Points)
				resource.Vertexs[index++] = new VertexData(new Vector3(point.Position), Color4.Black);

			index = 0;

			// - Loads all processed indexes
			foreach (FaceDescriptor face in data.Faces)
			{
				HalfEdgeDescriptor firstEdge = data.Edges[face.FirstEdge];
				HalfEdgeDescriptor edge = firstEdge;

				do
				{
					resource.Indexes[index++] = (int)(edge.Start + resource.Vertexs.Offset / VertexData.SizeInBytes);
					edge = data.Edges[edge.Next];
				} while (edge.Start != face.FirstEdge);
			}

			return resource;
		}
	}
}
