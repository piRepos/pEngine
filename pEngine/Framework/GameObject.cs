using System;
using System.Linq;
using System.Collections.Generic;

using pEngine.Timing;
using pEngine.Graphics;
using pEngine.Timing.Base;
using pEngine.Utils.Threading;

namespace pEngine.Framework
{
	/// <summary>
	/// Every object inside the game tree is a <see cref="GameObject"/> instance.
	/// </summary>
	public abstract class GameObject : IUpdatable
	{
		/// <summary>
		/// Makes a new instance of <see cref="GameObject"/> class.
		/// </summary>
		public GameObject()
		{
			// - Initialize collections
			pResources = new HashSet<Resource>();
		}

		/// <summary>
		/// All resource initialization and dependency settings goes here.
		/// </summary>
		/// <param name="pEngine">The instance of game engine that is running the game.</param>
		public virtual void Initialize(Runtime pEngine)
		{

		}

		#region Update

		/// <summary>
		/// Updates the state of this object.
		/// </summary>
		/// <param name="clock">Gives all timing informations for each frame.</param>
		public virtual void Update(IFrameBasedClock clock)
		{

		}

		#endregion

		#region Resources

		// - Contains all loaded resources
		private HashSet<Resource> pResources;

		/// <summary>
		/// Gets all loaded resource.
		/// </summary>
		public ICollection<Resource> Resources => pResources;

		/// <summary>
		/// Loads a set of resources.
		/// </summary>
		/// <param name="res">Resources to load.</param>
		public void LoadResources(params Resource[] res)
		{
			foreach (var resource in res)
			{
				LoadResource(resource);
			}
		}

		/// <summary>
		/// Loads the specified resource.
		/// </summary>
		/// <param name="res">Resource to load.</param>
		public void LoadResource(Resource res)
		{
			res.SetLoadingState(Resource.State.Loading);

			if (!pResources.Contains(res))
				pResources.Add(res);
		}

		/// <summary>
		/// Sends a request for the resource disposal.
		/// </summary>
		/// <param name="res">Resource to remove.</param>
		public void DisposeResource(Resource res)
		{
			res.SetLoadingState(Resource.State.Disposing);
		}

		#endregion

		#region Descriptor

		/// <summary>
		/// Generates an internal descriptor for this object.
		/// </summary>
		/// <returns>A <see cref="GameObjectDescriptor"/> instance.</returns>
		public virtual Descriptor GetDescriptor(Scheduler scheduler)
		{
			var resources = new List<Resource.IDescriptor>();

			foreach (var res in Resources)
			{
				if ((res.LoadingState == Resource.State.Loading 
				||   res.LoadingState == Resource.State.Disposing) && !res.Dispatched)
				{
					res.Dispatch();
					resources.Add(res.GetDescriptor(scheduler));
				}
			}
			return new Descriptor
			{
				Assets = { },
				Resources = resources.ToArray()
			};
		}

		/// <summary>
		/// Describes all needed resources and rendering parameters.
		/// </summary>
		public struct Descriptor
		{
			/// <summary>
			/// Contains only descriptors about resources that needs to be reloaded.
			/// </summary>
			public Resource.IDescriptor[] Resources { get; set; }

			/// <summary>
			/// Describes how to render an object.
			/// </summary>
			public Asset[] Assets { get; set; }
		}

		#endregion
	}
}
