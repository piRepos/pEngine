using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using pEngine.Utils.Collections;

namespace pEngine.Graphics.Data
{
	/// <summary>
	/// Manages vertex allocation and store.
	/// </summary>
	public class DataBuffer : ManagedHeap
	{

		/// <summary>
		/// Creates a new instance of <see cref="DataBuffer"/> class.
		/// </summary>
		/// <param name="initialSize">Preallocated heap size in bytes.</param>
		/// <param name="canGrow">If <see cref="true"/> will expand the maximum heap size when necessary.</param>
		/// <param name="growFactor">Determines the amount of memory to alloc when the heap grows as percentile of the current size.</param>
		/// <param name="maxSize">Limits the maximum size the heap can reach.</param>
		public DataBuffer(uint initialSize = 1, bool canGrow = true, double growFactor = 0.1, uint maxSize = uint.MaxValue)
			: base(initialSize, canGrow, growFactor, maxSize)
		{

		}

		/// <summary>
		/// Reserve a heap's chunk and returns a pointer to this area.
		/// </summary>
		/// <param name="size">Number of bytes to reserve.</param>
		/// <returns>An handle to the reserved area.</returns>
		public override MemoryPointer Alloc(uint size)
		{
			BufferResource resource = new BufferResource(this, size);

			return base.Alloc(resource);
		}

		/// <summary>
		/// This method will re-arrange the array in a way that 
		/// all the used memory is contiguous when possible.
		/// </summary>
		/// <param name="limitSwaps">Limits the number of chunk swaps for a partial defragmentation.</param>
		/// <param name="movedCallback">Callback invoked on each swap.</param>
		public override void Defrag(int limitSwaps = int.MaxValue, Action<MemoryPointer> movedCallback = null)
		{
			Action<MemoryPointer> callback = (chunk) =>
			{
				BufferResource resource = chunk as BufferResource;

				// - Invalidate the moved resource
				resource.Invalidate(true);

				movedCallback?.Invoke(chunk);
			};

			base.Defrag(limitSwaps, callback);
		}

		/// <summary>
		/// Implement IDisposable.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		#region Buffer resource

		/// <summary>
		/// Manage a buffer chunk that can be invalidated.
		/// </summary>
		public class BufferResource : MemoryPointer
		{

			/// <summary>
			/// Creates a new insance of <see cref="BufferResource"/> class.
			/// </summary>
			/// <param name="provider">Heap manager which contains this memory.</param>
			/// <param name="size">Memory block size.</param>
			public BufferResource(DataBuffer provider, uint size)
				: base(provider, size, 0)
			{
				Invalidated = true;
				UploadPriority = 0;
			}

			/// <summary>
			/// <see cref="true"/> if this chunk has changed and needs an upload.
			/// </summary>
			public bool Invalidated { get; internal set; }

			/// <summary>
			/// Higher values means this chunk will be uploaded before others.
			/// </summary>
			public int UploadPriority { get; internal set; }

			/// <summary>
			/// Invalidate this chunk of buffer.
			/// </summary>
			/// <param name="forceUpload">Makes sure that this chunk will be uploaded at the next update.</param>
			public void Invalidate(bool forceUpload = false)
			{
				Invalidated = true;
				UploadPriority = forceUpload ? int.MaxValue : 0;
			}
		}

		#endregion
	}
}
