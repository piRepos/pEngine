using System;
using System.Collections.Generic;

using pEngine.Framework;

namespace pEngine.Graphics.Resources
{
	/// <summary>
	/// Manages resource loading and dispatching for rendering.
	/// </summary>
	public abstract class Dispatcher<K, T, D> 
		where T : IInternalResource 
		where K : IResource, new() 
		where D : Resource.IDescriptor
	{
		/// <summary>
		/// Makes a new instance of <see cref="Dispatcher"/> class.
		/// </summary>
		public Dispatcher()
		{
			dataStore = new Dictionary<uint, T>();
		}

		/// <summary>
		/// Gets all resources.
		/// </summary>
		public IEnumerable<T> Resources => dataStore.Values;

		#region Business

		private Dictionary<uint, T> dataStore;
		static uint resUID = 0;

		/// <summary>
		/// Gets the resource linked to the specified pointer.
		/// </summary>
		/// <param name="key">Resource pointer.</param>
		/// <returns>The wanted rersource.</returns>
		public T Get(K key) => dataStore[key.ID];

		/// <summary>
		/// Loads the specified resource in a dispatching cache.
		/// </summary>
		/// <param name="data">Resource to load.</param>
		/// <returns>Pointer to the loaded resource.</returns>
		public K Load(D data)
		{
			K key = new K();

			key.ID = resUID++;

			dataStore[key.ID] = InternalLoad(data);

			return key;
		}

		/// <summary>
		/// This method must implement the loading business in order to
		/// provide a rendering cached structure.
		/// </summary>
		/// <param name="data">Metadata and data for the loading process.</param>
		/// <returns>A result's descriptor.</returns>
		protected abstract T InternalLoad(D data);

		#endregion
	}
}
