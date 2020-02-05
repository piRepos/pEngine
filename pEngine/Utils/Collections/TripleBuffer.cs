using System;

using pEngine.Utils.Invocation;

namespace pEngine.Utils.Collections
{
	/// <summary>
	/// A thread safe triple buffer implementation.
	/// </summary>
	/// <typeparam name="T">Colection type.</typeparam>
	public class TripleBuffer<T>
	{
		/// <summary>
		/// Makes a new instance of <see cref="TripleBuffer{T}"/>.
		/// </summary>
		public TripleBuffer()
		{
			pCloseDelegate = Close;
		}

		/// <summary>
		/// Internal buffer.
		/// </summary>
		private ObjectUsage<T>[] pBuffer { get; } = new ObjectUsage<T>[3];

		/// <summary>
		/// Finish delegate cache for performance improvement.
		/// </summary>
		private Action<ObjectUsage<T>> pCloseDelegate { get; }

		/// <summary>
		/// Logical pointer to the object that is being used for writing.
		/// </summary>
		private int pWritePointer { get; set; }

		/// <summary>
		/// Logical pointer to the object that is being used for reading.
		/// </summary>
		private int pReadPointer { get; set; }

		/// <summary>
		/// Logical pointer to the object which is not used.
		/// </summary>
		private int pFreePointer { get; set; }

		/// <summary>
		/// Get an access to the buffer.
		/// </summary>
		/// <param name="usage">Specifies the access usage.</param>
		/// <returns>An usage instance.</returns>
		public ObjectUsage<T> Get(UsageType usage)
		{
			switch (usage)
			{
				case UsageType.Write:

					lock (pBuffer)
					{
						// - Find the free buffer for write
						while (pBuffer[pWritePointer]?.Usage == UsageType.Read || pWritePointer == pFreePointer)
							pWritePointer = (pWritePointer + 1) % 3;
					}

					// - If buffer is null instance it and set it on Write
					if (pBuffer[pWritePointer] == null)
					{
						ObjectUsage<T> obj = new ObjectUsage<T>
						{
							Usage = UsageType.Write
						};

						obj.Action = () => Close(obj);

						pBuffer[pWritePointer] = obj;
					}
					else
					{
						pBuffer[pWritePointer].Usage = UsageType.Write;
					}

					return pBuffer[pWritePointer];

				case UsageType.Read:

					if (pFreePointer < 0) return null;

					lock (pBuffer)
					{
						pReadPointer = pFreePointer;
						pBuffer[pReadPointer].Usage = UsageType.Read;
					}

					return pBuffer[pReadPointer];
			}

			return null;
		}

		/// <summary>
		/// Called on usage disposition
		/// </summary>
		private void Close(ObjectUsage<T> obj)
		{
			switch (obj.Usage)
			{
				case UsageType.Read:

					lock (pBuffer)
						pBuffer[pReadPointer].Usage = UsageType.None;

					break;

				case UsageType.Write:

					lock (pBuffer)
					{
						pBuffer[pWritePointer].Usage = UsageType.None;
						pFreePointer = pWritePointer;
					}

					break;
			}
		}

	}

	public class ObjectUsage<T> : InvokeOnDisposal
	{

		public ObjectUsage()
		{

		}

		/// <summary>
		/// Current usage.
		/// </summary>
		public UsageType Usage { get; set; }

		/// <summary>
		/// Current object.
		/// </summary>
		public T Value { get; set; }

		/// <summary>
		/// Pointer index
		/// </summary>
		public int Index { get; set; }

	}

	/// <summary>
	/// Describe the object usage in memory, write or read.
	/// </summary>
	public enum UsageType
	{
		None,
		Read,
		Write
	}
}
