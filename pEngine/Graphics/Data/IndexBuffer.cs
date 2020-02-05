using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace pEngine.Graphics.Data
{
	/// <summary>
	/// Manages a vertex buffer implemented with a <see cref="Utils.Collections.ManagedHeap"/>.
	/// </summary>
	public class IndexBuffer : DataBuffer
	{

		/// <summary>
		/// Make a new instance of <see cref="IndexBuffer"/> class.
		/// </summary>
		/// <param name="initialSize">Preallocated heap size in bytes.</param>
		/// <param name="canGrow">If <see cref="true"/> will expand the maximum heap size when necessary.</param>
		/// <param name="growFactor">Determines the amount of memory to alloc when the heap grows as percentile of the current size.</param>
		/// <param name="maxSize">Limits the maximum size the heap can reach.</param>
		public IndexBuffer(uint initialSize = 1, bool canGrow = true, double growFactor = 0.1, uint maxSize = uint.MaxValue) 
			: base(initialSize, canGrow, growFactor, maxSize)
		{

		}

		/// <summary>
		/// Reserve a chunk of indexs inside the buffer's heap.
		/// </summary>
		/// <param name="count">Number of vertexs to reserve.</param>
		/// <returns>An handle to the vertex array resource.</returns>
		public new IndexChunk Alloc(uint count)
		{
			IndexChunk newChunk = new IndexChunk(this, count);

			return base.Alloc(newChunk) as IndexChunk;
		}

		#region Buffer resource

		/// <summary>
		/// Manage a buffer chunk that can be invalidated.
		/// </summary>
		public class IndexChunk : BufferResource, IReadOnlyList<int>
		{

			/// <summary>
			/// Creates a new insance of <see cref="BufferResource"/> class.
			/// </summary>
			/// <param name="provider">Heap manager which contains this memory.</param>
			/// <param name="size">Number of vertexs.</param>
			public IndexChunk(IndexBuffer provider, uint count)
				: base(provider, count * sizeof(int))
			{

			}

			/// <summary>
			/// Gets the number of elements in the collection.
			/// </summary>
			public int Count => (int)(Size / VertexData.SizeInBytes);

			#region List implementation

			public unsafe int this[int index]
			{
				get => Marshal.ReadInt32(IntPtr.Add(Handler, index));
				set => *(int*)IntPtr.Add(Handler, index * sizeof(int)).ToPointer() = value;
			}

			private IEnumerable<int> Enumerator()
			{
				for (int i = 0; i < Count; ++i)
				{
					yield return this[i];
				}
			}

			public IEnumerator<int> GetEnumerator()
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
