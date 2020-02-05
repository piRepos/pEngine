using System;

namespace pEngine.Utils.Threading
{

	/// <summary>
	/// Provides an <see cref="IProgress{T}"/> that invokes callbacks for each reported
	/// progress value.
	/// </summary>
	/// <typeparam name="T">Specifies the type of the progress report value.</typeparam>
	public class Progress<T> : IProgress<T>
	{

		/// <summary>
		/// Initializes the <see cref="System.Progress{T}"/> object.
		/// </summary>
		public Progress()
		{

		}

		/// <summary>
		/// Initializes the <see cref="System.Progress{T}"/> object with the specified callback.
		/// </summary>
		/// <param name="handler">
		/// A handler to invoke for each reported progress value. This handler will be
		/// invoked in addition to any delegates registered with the System.Progress<T>.ProgressChanged
		/// event. Depending on the System.Threading.SynchronizationContext instance
		/// captured by the System.Progress<T> at construction, it is possible that this
		/// handler instance could be invoked concurrently with itself.
		/// </param>
		public Progress(Action<T> handler)
		{
			pCallback = handler;
		}

		/// <summary>
		/// Raised for each reported progress value.
		/// </summary>
		public event EventHandler ProgressChanged;

		// - Progress callback
		Action<T> pCallback;

		/// <summary>
		/// Reports a progress change.
		/// </summary>
		/// <param name="value">The value of the updated progress.</param>
		public virtual void Report(T value)
		{
			pCallback?.Invoke(value);
			ProgressChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
