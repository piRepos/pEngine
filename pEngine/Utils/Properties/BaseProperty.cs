using System;
using System.Collections.Generic;
using System.Text;

namespace pEngine.Utils.Properties
{
	/// <summary>
	/// Interface for a generic framework property.
	/// </summary>
	public abstract class BaseProperty<T>
	{
		/// <summary>
		/// Property (external) current value.
		/// </summary>
		public abstract T Value { get; set; }

		/// <summary>
		/// Sintatic sugar for assignment.
		/// </summary>
		/// <param name="p">Target property.</param>
		/// <param name="b">The new value.</param>
		/// <returns>The assigned value (for chaining).</returns>
		public static T operator |(BaseProperty<T> p, T b)
		{
			p.Value = b;
			return b;
		}

		/// <summary>
		/// Implicit conversion to the property type <see cref="T"/>.
		/// </summary>
		/// <param name="p">The source property.</param>
		public static implicit operator T(BaseProperty<T> p)
		{
			return p.Value;
		}
	}
}
