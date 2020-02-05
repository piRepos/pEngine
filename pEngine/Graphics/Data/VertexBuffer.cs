using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace pEngine.Graphics.Data
{
	/// <summary>
	/// Manages a vertex buffer implemented with a <see cref="Utils.Collections.ManagedHeap"/>.
	/// </summary>
	public class VertexBuffer : DataBuffer
	{

		/// <summary>
		/// Make a new instance of <see cref="VertexBuffer"/> class.
		/// </summary>
		/// <param name="initialSize">Preallocated heap size in bytes.</param>
		/// <param name="canGrow">If <see cref="true"/> will expand the maximum heap size when necessary.</param>
		/// <param name="growFactor">Determines the amount of memory to alloc when the heap grows as percentile of the current size.</param>
		/// <param name="maxSize">Limits the maximum size the heap can reach.</param>
		public VertexBuffer(uint initialSize = 1, bool canGrow = true, double growFactor = 0.1, uint maxSize = uint.MaxValue) 
			: base(initialSize, canGrow, growFactor, maxSize)
		{

		}

		/// <summary>
		/// Reserve a chunk of vertexs inside the buffer's heap.
		/// </summary>
		/// <param name="count">Number of vertexs to reserve.</param>
		/// <returns>An handle to the vertex array resource.</returns>
		public new VertexChunk Alloc(uint count)
		{
			VertexChunk newChunk = new VertexChunk(this, count);

			return base.Alloc(newChunk) as VertexChunk;
		}

		#region Buffer resource

		/// <summary>
		/// Manage a buffer chunk that can be invalidated.
		/// </summary>
		public class VertexChunk : BufferResource, IReadOnlyList<VertexData>
		{

			/// <summary>
			/// Creates a new insance of <see cref="BufferResource"/> class.
			/// </summary>
			/// <param name="provider">Heap manager which contains this memory.</param>
			/// <param name="size">Number of vertexs.</param>
			public VertexChunk(VertexBuffer provider, uint count)
				: base(provider, count * VertexData.SizeInBytes)
			{

			}

			/// <summary>
			/// Gets the number of elements in the collection.
			/// </summary>
			public int Count => (int)(Size / VertexData.SizeInBytes);

			#region List implementation

			public unsafe VertexData this[int index]
			{
				get => Marshal.PtrToStructure<VertexData>(IntPtr.Add(Handler, index * (int)VertexData.SizeInBytes));
				set => *(VertexData*)IntPtr.Add(Handler, index * (int)VertexData.SizeInBytes).ToPointer() = value;
			}

			private IEnumerable<VertexData> Enumerator()
			{
				for (int i = 0; i < Count; ++i)
				{
					yield return this[i];
				}
			}

			public IEnumerator<VertexData> GetEnumerator()
			{
				return Enumerator().GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return Enumerator().GetEnumerator();
			}

			#endregion
		}

		#endregion

	}
}
