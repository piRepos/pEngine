using System;
using System.Collections.Generic;

using pEngine.Framework.Geometry;
using pEngine.Graphics.Data;

namespace pEngine.Graphics.Resources
{
	/// <summary>
	/// Loads any type of geometry from DECL structures.
	/// </summary>
	public abstract class GeometryDispatcher : Dispatcher<Geometry, GeometryResource, Shape.Descriptor>
	{
		/// <summary>
		/// Makes a new instance of <see cref="GeometryDispatcher"/> class.
		/// </summary>
		/// <param name="vb">Vertex buffer.</param>
		/// <param name="ib">Index buffer.</param>
		public GeometryDispatcher(VertexBuffer vb, IndexBuffer ib)
		{
			VertexManager = vb;
			IndexManager = ib;
		}

		/// <summary>
		/// Manages raw vertex allocation and the GPU buffer transfer.
		/// </summary>
		protected VertexBuffer VertexManager { get;  }

		/// <summary>
		/// Manages raw index allocation and the GPU buffer transfer.
		/// </summary>
		protected IndexBuffer IndexManager { get; }
	}


	/// <summary>
	/// Describes a geometric graphic resource, so
	/// a logical pointer to the GPU resources which contains
	/// the vertexs and the indexes for a mesh.
	/// </summary>
	public struct Geometry : IResource
	{
		/// <summary>
		/// Resource ID.
		/// </summary>
		public uint ID { get; set; }
	}

	/// <summary>
	/// Contains the pointers to the rendering entities.
	/// </summary>
	public struct GeometryResource : IInternalResource
	{
		/// <summary>
		/// Vertex buffer pointing to the GPU's data.
		/// </summary>
		public VertexBuffer.VertexChunk Vertexs { get; set; }

		/// <summary>
		/// Index buffer pointing to the GPU's data.
		/// </summary>
		public IndexBuffer.IndexChunk Indexes { get; set; }
	}
}
