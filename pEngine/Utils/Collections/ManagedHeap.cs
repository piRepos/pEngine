using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace pEngine.Utils.Collections
{
	/// <summary>
	/// Works as a memory manager class which try to keep
	/// the allocated data in a contigous block of memory.
	/// </summary>
	public class ManagedHeap : IDisposable
	{

		/// <summary>
		/// Makes a new instance of <see cref="ManagedHeap"/> class.
		/// </summary>
		/// <param name="initialSize">Preallocated heap size in bytes.</param>
		/// <param name="canGrow">If <see cref="true"/> will expand the maximum heap size when necessary.</param>
		/// <param name="growFactor">Determines the amount of memory to alloc when the heap grows as percentile of the current size.</param>
		/// <param name="maxSize">Limits the maximum size the heap can reach.</param>
		public ManagedHeap(uint initialSize = 1, bool canGrow = true, double growFactor = 0.1, uint maxSize = uint.MaxValue)
		{
			CanGrow = canGrow;
			GrowFactor = growFactor;
			MaximumSize = maxSize;

			UsedSpace = new List<MemoryPointer>();
			FreeSpace = new List<MemoryPointer>();

			unsafe
			{
				RawMemory = Marshal.AllocHGlobal((int)initialSize);
				HeapSize = initialSize;
			}

			FreeSpace.Add(new MemoryPointer(this, initialSize, 0));
		}

		/// <summary>
		/// Determines the amount of memory to alloc when the heap grows as 
		/// percentile of the current size (normalized value).
		/// </summary>
		public double GrowFactor { get; set; }

		/// <summary>
		/// If <see cref="true"/> will expand the maximum heap size when necessary.
		/// </summary>
		public bool CanGrow { get; set; }

		/// <summary>
		/// Limits the maximum size the heap can reach in number of elements.
		/// </summary>
		public uint MaximumSize { get; protected set; }

		#region Raw memory allocations

		/// <summary>
		/// Managed memory pointer.
		/// </summary>
		protected IntPtr RawMemory { get; set; }

		/// <summary>
		/// Gets the current heap size in bytes.
		/// </summary>
		public uint HeapSize { get; protected set; }

		/// <summary>
		/// Resize the heap encreasing the size by the specified value mantaining all data
		/// in the same position.
		/// </summary>
		/// <param name="size">Grow size in bytes (the actual grow size could be bigger than the specified).</param>
		/// <param name="noOvergrowth">Force the allocator to grow the heap by the exact specified size.</param>
		/// <returns>Returns <see cref="true"/> if the memory allocation has been done.</returns>
		public virtual bool Grow(uint size, bool noOvergrowth = false)
		{
			// - Check if the heap can grow
			if (!CanGrow) return false;

			// - Check if we reached the memory limit
			if (HeapSize + size > MaximumSize) return false;

			// - Calculates the actual grow size
			uint growSize = noOvergrowth ? size : (uint)(size + GrowFactor * HeapSize);

			try
			{
				// - Realloc the heap memory
				RawMemory = Marshal.ReAllocHGlobal(RawMemory, new IntPtr(HeapSize + growSize));
			}
			catch (OutOfMemoryException)
			{
				return false;
			}

			// - Gets the last avaiable chunk of memory
			var lastChunk = FreeSpace.LastOrDefault();

			// - Create a new memory pointer pointing to the last block if there's no free block at the end
			if (lastChunk == null || lastChunk.Offset + lastChunk.Size < HeapSize)
				FreeSpace.Add(new MemoryPointer(this, growSize, HeapSize));

			// - Modify the existing chunk if it's at the end of the heap
			if (lastChunk != null || lastChunk.Offset + lastChunk.Size >= HeapSize)
				lastChunk.ChangeOffsetAndSize(lastChunk.Offset, lastChunk.Size + growSize);

			// - Update the heap size
			HeapSize += growSize;

			return true;
		}

		/// <summary>
		/// Shrinks the heap memory to the smallest memory that contains
		/// all the allocated blocks.
		/// </summary>
		/// <param name="keepBytes">Number of bytes to keep at the heap's tail.</param>
		/// <returns>Returns <see cref="true"/> if the shring goes well.</returns>
		public virtual bool FreeUnusedMemory(int keepBytes = -1)
		{
			// - Gets the last avaiable chunk of memory
			var lastChunk = FreeSpace.LastOrDefault();

			// - Create a new memory pointer pointing to the last block if there's no free block at the end
			if (lastChunk != null || lastChunk.Offset + lastChunk.Size >= HeapSize)
			{
				// - Automatically takes 10% of the whole size free if keepBytes is -1
				if (keepBytes < 0) keepBytes = (int)System.Math.Ceiling(HeapSize * 0.1);

				// - Calculate the amount of memory to free
				uint shrinkSize = (uint)System.Math.Max(lastChunk.Size - keepBytes, 0);

				if (shrinkSize > 0)
				{
					try
					{
						// - Realloc the heap memory
						RawMemory = Marshal.ReAllocHGlobal(RawMemory, new IntPtr(HeapSize - shrinkSize));

						// - Resize the last chunk
						if (lastChunk.Size - shrinkSize > 0)
							lastChunk.ChangeOffsetAndSize(lastChunk.Offset, lastChunk.Size - shrinkSize);
						else
							FreeSpace.Remove(lastChunk);

						HeapSize -= shrinkSize;
					}
					catch (OutOfMemoryException)
					{
						return false;
					}
				}
			}
			return true;
		}

		#endregion

		#region Memory fragmentation and management

		/// <summary>
		/// This factor is close to 1 when the reserved memory is scattered on the heap,
		/// is closer to 0 when the majority of the reserved memory is contiguous instead.
		/// </summary>
		public double FragmentationFactor { get; protected set; }

		/// <summary>
		/// All allocated handlers.
		/// </summary>
		protected List<MemoryPointer> UsedSpace { get; }

		/// <summary>
		/// All free space blocks.
		/// </summary>
		protected List<MemoryPointer> FreeSpace { get; }

		/// <summary>
		/// Reserve a heap's chunk and returns a pointer to this area.
		/// </summary>
		/// <param name="count">Number of bytes to reserve.</param>
		/// <returns>An handle to the reserved area.</returns>
		public virtual MemoryPointer Alloc(uint count)
		{
			// - Creates a new pointer without reference
			MemoryPointer newPointer = new MemoryPointer(this, count, 0);

			return Alloc(newPointer);
		}

		/// <summary>
		/// Reserves a heap's chunk and returns a pointer to this area.
		/// </summary>
		/// <param name="newPointer">Number of bytes to reserve.</param>
		/// <returns>An handle to the reserved area.</returns>
		protected virtual MemoryPointer Alloc(MemoryPointer newPointer)
		{
			// - Iterate over the free spaces
			for (int i = FreeSpace.Count - 1; i >= 0; --i)
			{
				var freeChunk = FreeSpace[i];

				// - If we found a suitable free space
				if (freeChunk.Size >= newPointer.Size)
				{
					// - Set the position to the current free chunk
					newPointer.ChangeOffsetAndSize(freeChunk.Offset, newPointer.Size);

					// - Calculates the remaining space
					uint remainingSpace = freeChunk.Size - newPointer.Size;

					// - If no memory remains in the free space pointer we must delete it
					if (remainingSpace <= 0) FreeSpace.Remove(freeChunk);
					else freeChunk.ChangeOffsetAndSize(newPointer.Offset + newPointer.Size, remainingSpace);

					// - Adds the new pointer to the used space
					UsedSpace.Add(newPointer);

					// - Sort used space
					UsedSpace.OrderBy(x => x.Offset);

					// - Update fragmentation factor
					FragmentationFactor = CalculateFragmentationRate();

					return newPointer;
				}
			}

			// - Expand the heap
			if (!Grow(newPointer.Size))
			{
				Defrag();

				if (FreeSpace.Count > 0 && FreeSpace.Last().Size > newPointer.Size)
					return Alloc(newPointer);

				return null;
			}

			return Alloc(newPointer);
		}

		/// <summary>
		/// Release a portion of heap described by the provided handler.
		/// </summary>
		/// <param name="handler">Handler to the memory chunk you want to free.</param>
		public virtual void Free(MemoryPointer handler)
		{
			// - Assure that handler is valid
			if (handler == null) return;

			// - Assure that handler is valid
			if (!UsedSpace.Contains(handler)) return;

			MemoryPointer previousFree = null;
			MemoryPointer nextFree = null;

			// - Iterate over the free spaces
			for (int i = FreeSpace.Count - 1; i >= 0; --i)
			{
				var freeChunk = FreeSpace[i];

				// - Check if there's a free block before the handler
				if (freeChunk.Offset + freeChunk.Size == handler.Offset)
					previousFree = freeChunk;

				// - Check if there's a free block after the handler
				if (handler.Offset + handler.Size == freeChunk.Offset)
					nextFree = freeChunk;
			}

			if (previousFree != null && nextFree != null)
			{
				uint newSize = previousFree.Size + handler.Size + nextFree.Size;

				// - Remove last free space
				FreeSpace.Remove(nextFree);

				// - Expand the first free space
				previousFree.ChangeOffsetAndSize(previousFree.Offset, newSize);
			}
			else if (previousFree != null)
			{
				uint newSize = previousFree.Size + handler.Size;

				// - Expand the first free space
				previousFree.ChangeOffsetAndSize(previousFree.Offset, newSize);
			}
			else if (nextFree != null)
			{
				uint newSize = nextFree.Size + handler.Size;

				// - Expand the first free space
				previousFree.ChangeOffsetAndSize(handler.Offset, newSize);
			}
			else
			{
				// - Adds the removed space to the FreeSpace list
				FreeSpace.Add(handler);

				// - Sort free space
				FreeSpace.OrderBy(x => x.Offset);
			}

			// - Remove the deleted handler
			UsedSpace.Remove(handler);
		}

		/// <summary>
		/// This method will re-arrange the array in a way that 
		/// all the used memory is contiguous when possible.
		/// </summary>
		/// <param name="limitSwaps">Limits the number of chunk swaps for a partial defragmentation.</param>
		/// <param name="movedCallback">Callback invoked on each swap.</param>
		public virtual void Defrag(int limitSwaps = int.MaxValue, Action<MemoryPointer> movedCallback = null)
		{
			// - No fragmentation
			if (FreeSpace.Count <= 1) return;

			int lastOffset = 0;
			MemoryPointer nextChunk = null;

			// - Foreach chunk we move the block to the left
			foreach (var chunk in UsedSpace)
			{
				// - Limits the number of adjustments
				if (limitSwaps <= 0)
				{
					nextChunk = chunk;
					break;
				}

				if (chunk.Offset > lastOffset)
				{
					unsafe
					{
						void* source = chunk.Handler.ToPointer();
						void* dest = IntPtr.Add(RawMemory, lastOffset).ToPointer();

						Buffer.MemoryCopy(source, dest, chunk.Size, chunk.Size);
					}

					movedCallback?.Invoke(chunk);

					// - Update handler offset
					chunk.ChangeOffsetAndSize((uint)lastOffset, chunk.Size);

					limitSwaps--;
				}

				lastOffset += (int)chunk.Size;
			}

			if (nextChunk != null)
			{
				// - Remove remaining spaces before next chunk
				FreeSpace.RemoveAll(x => x.Offset <= nextChunk.Offset);

				if (lastOffset < nextChunk.Offset)
				{
					uint newSize = (uint)(nextChunk.Offset - lastOffset);

					FreeSpace.Insert(0, new MemoryPointer(this, newSize, (uint)lastOffset));
				}
			}
			else
			{
				FreeSpace.Clear();

				var lastElement = UsedSpace.LastOrDefault();
				uint newOffset = lastElement.Offset + lastElement.Size;
				uint newSize = HeapSize - newOffset;

				FreeSpace.Add(new MemoryPointer(this, newSize, newOffset));
			}

			// - Update fragmentation factor
			FragmentationFactor = CalculateFragmentationRate();
		}

		/// <summary>
		/// Calculate the ratio beween the number of consecutive memory blocks and scattered blocks.
		/// </summary>
		/// <returns>A value between 1 and 0, close to 1 when the memory is fragmented.</returns>
		protected double CalculateFragmentationRate()
		{
			if (FreeSpace.Count <= 0) return 1;

			var freeBytes = FreeSpace.Sum(x => x.Size);

			return (freeBytes - FreeSpace.Max(x => x.Size)) / freeBytes;
		}

		#endregion

		#region Memory class

		/// <summary>
		/// Manage a portion of the provider heap.
		/// </summary>
		public class MemoryPointer
		{
			/// <summary>
			/// Creates a new instance of <see cref="ManagedHeap{T}"/> class.
			/// </summary>
			/// <param name="provider">Heap manager which contains this memory.</param>
			/// <param name="size">Memory block size.</param>
			/// <param name="offset">Offset inside the heap.</param>
			public MemoryPointer(ManagedHeap provider, uint size, uint offset)
			{
				Provider = provider;
				Size = size;
				Offset = offset;
			}

			/// <summary>
			/// Heap manager which contains this memory.
			/// </summary>
			protected ManagedHeap Provider { get; }

			/// <summary>
			/// Memory block size.
			/// </summary>
			public uint Size { get; protected set; }

			/// <summary>
			/// Offset inside the source heap.
			/// </summary>
			public uint Offset { get; protected set; }

			/// <summary>
			/// Base raw pointer for this <see cref="MemoryPointer"/>.
			/// </summary>
			public IntPtr Handler => IntPtr.Add(Provider.RawMemory, (int)Offset);

			/// <summary>
			/// !!! DO NOT CALL THIS !!!
			/// Only the <see cref="ManagedHeap"/> is allowed to call this function.
			/// </summary>
			internal void ChangeOffsetAndSize(uint offset, uint size)
			{
				Offset = offset;
				Size = size;
			}
		}

		#endregion

		#region Resources disposal

		/// <summary>
		/// Dispose logic variable.
		/// </summary>
		private bool pDisposed { get; set; }

		/// <summary>
		/// Implement IDisposable.
		/// </summary>
		public void Dispose()
		{
			// Check to see if Dispose has already been called.
			if (!pDisposed)
			{
				Dispose(true);

				// Note disposing has been done.
				pDisposed = true;
			}

			// - This object will be cleaned up by the Dispose method.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose(bool disposing) executes in two distinct scenarios.
		/// If disposing equals <see cref="true"/>, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// If disposing equals <see cref="false"/>, the method has been called by the
		/// runtime from inside the finalizer and you should not reference
		/// other objects. Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"><see cref="True"/> if called from user's code.</param>
		protected virtual void Dispose(bool disposing)
		{
			// - Free the unmanaged memory
			Marshal.FreeHGlobal(RawMemory);
		}

		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// </summary>
		~ManagedHeap()
		{
			Dispose(false);
		}

		#endregion
	}
}
