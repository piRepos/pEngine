using System;

using pEngine.Utils.Properties;
using pEngine.Utils.Threading;

namespace pEngine.Framework
{
	/// <summary>
	/// Generic resource which can be sent or received
	/// from other threads.
	/// </summary>
	public abstract class Resource
	{
		/// <summary>
		/// Makes a new instance of <see cref="Resource"/> class.
		/// </summary>
		public Resource()
		{
			// - Setup properties
			LoadingState = new Bindable<State>(false, () => InternalLoadingState);
			Dispatched = new Property<bool>(false, () => InternalDispatched);

			// - Setup events
			LoadingState.Changed += (obj, e) => 
			{ 
				if (e.newValue == State.Loaded) 
					Loaded?.Invoke(this, EventArgs.Empty);

				if (e.newValue == State.Disposing)
					Disposing?.Invoke(this, EventArgs.Empty);
			};
		}

		/// <summary>
		/// Describes the current loading state.
		/// </summary>
		public Bindable<State> LoadingState { get; }

		/// <summary>
		/// 
		/// </summary>
		public Property<bool> Dispatched { get; }

		/// <summary>
		/// Raised when the resource loading is completed.
		/// </summary>
		public event EventHandler Loaded;

		/// <summary>
		/// Raised on disposing process start.
		/// </summary>
		public event EventHandler Disposing;

		#region Loading state

		/// <summary>
		/// Internal loading state variable.
		/// </summary>
		protected State InternalLoadingState { get; set; }

		/// <summary>
		/// 
		/// </summary>
		protected bool InternalDispatched { get; set; }

		/// <summary>
		/// Sets the loading state (must be used from the loader).
		/// </summary>
		/// <param name="state">New loading state.</param>
		internal void SetLoadingState(State state)
		{
			InternalDispatched = false;
			InternalLoadingState = state;
			LoadingState.ForceRefresh();
		}

		/// <summary>
		/// 
		/// </summary>
		internal void Dispatch()
		{
			InternalDispatched = true;
		}

		/// <summary>
		/// All possible <see cref="Resource"/> loading states.
		/// </summary>
		public enum State
		{
			/// <summary>
			/// The resource loadeing process 
			/// has not been requested yet.
			/// </summary>
			NotLoaded = 0,

			/// <summary>
			/// The resource is currently loading.
			/// </summary>
			Loading = 1,

			/// <summary>
			/// The resource is loaded.
			/// </summary>
			Loaded = 2,

			/// <summary>
			/// Error during resource loading.
			/// </summary>
			Aborted = 3,

			/// <summary>
			/// Sends a request for the resource deletion.
			/// </summary>
			Disposing = 4
		}

		#endregion

		#region Descriptor

		/// <summary>
		/// Generates a resource descriptor for this resource.
		/// </summary>
		/// <returns>A resource descriptor.</returns>
		internal virtual IDescriptor GetDescriptor(Scheduler scheduler)
		{
			return new Descriptor
			{
				SetState = (state) => SetLoadingState(state),
				SourceScheduler = scheduler
			};
		}

		/// <summary>
		/// Generic interface for all resource descriptors.
		/// </summary>
		public interface IDescriptor
		{
			/// <summary>
			/// Gets the remote resource state.
			/// </summary>
			Action<State> SetState { get; }

			/// <summary>
			/// Gets the scheduler that will handles the callback.
			/// </summary>
			Scheduler SourceScheduler { get; }
		}

		/// <summary>
		/// Generic descriptors.
		/// </summary>
		public struct Descriptor : IDescriptor
		{
			/// <summary>
			/// Sets the remote resource state.
			/// </summary>
			public Action<State> SetState { get; set; }

			/// <summary>
			/// Gets the scheduler that will handles the callback.
			/// </summary>
			public Scheduler SourceScheduler { get; set; }
		}

		#endregion
	}
}
