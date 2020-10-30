using System;
using System.Collections.Generic;
using System.Text;

namespace pEngine.Graphics.Resources
{
	/// <summary>
	/// Represent an external resource.
	/// </summary>
	public interface IResource
	{
		/// <summary>
		/// Resource unique identifier.
		/// </summary>
		uint ID { get; set; }
	}

	/// <summary>
	/// Generic interface for internal rendering resources.
	/// </summary>
	public interface IInternalResource
	{

	}
}
