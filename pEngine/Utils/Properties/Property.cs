using System;
using System.Collections.Generic;
using System.Text;

namespace pEngine.Utils.Properties
{
	/// <summary>
	/// Wrap a type providing an underlying layer that allows to extend
	/// functionalities like change notification, caching, bindings, etc...
	/// </summary>
	/// <typeparam name="T">Property type</typeparam>
	public class Property<T> : BaseProperty<T>
	{
		/// <summary>
		/// Makes a new instance of <see cref="Property{T}"/> class.
		/// </summary>
		/// <param name="cached">If <see cref="true"/> getter wont be called until the next invalidation.</param>
		/// <param name="getter">Getter function, calculates the property's value.</param>
		public Property(bool cached, Func<T> getter)
		{
			ReadOnly = true;
			GetterFunction = getter;
		}

		/// <summary>
		/// Makes a new instance of <see cref="Property{T}"/> class.
		/// </summary>
		/// <param name="defaultValue">Property default value.</param>
		public Property(T initialValue, bool readOnly = false)
		{
			InternalValue = initialValue;
			ReadOnly = readOnly;
			GetterFunction = null;
		}

		/// <summary>
		/// Makes a new instance of <see cref="Property{T}"/> class.
		/// </summary>
		public Property()
		{
			InternalValue = default;
			ReadOnly = false;
			GetterFunction = null;
		}

		/// <summary>
		/// Current real value.
		/// </summary>
		protected T InternalValue { get; set; }

		/// <summary>
		/// Getter function, calculates the property's value.
		/// </summary>
		protected Func<T> GetterFunction { get; }

		/// <summary>
		/// If <see cref="true"/> force internal value reloading by calling getter function.
		/// </summary>
		protected bool Invalidated { get; private set; }

		/// <summary>
		/// If true any write attempt to this property will raise an exception.
		/// </summary>
		public bool ReadOnly { get; }

		/// <summary>
		/// If a getter is provided this property enables value caching.
		/// </summary>
		public bool Cached { get; set; }

		/// <summary>
		/// Invalidate the calculated value is a getter is provided.
		/// </summary>
		public void Invalidate()
		{
			Invalidated = true;
		}

		/// <summary>
		/// If readonly from a getter, force the value refresh.
		/// </summary>
		public void ForceRefresh()
		{
			GetValue();
		}

		/// <summary>
		/// Property (external) current value.
		/// </summary>
		public override T Value
		{
			get => GetValue();
			set 
			{ 
				if (ReadOnly) throw new InvalidOperationException("Assignment to a readonly property"); 
				SetValue(value); 
			}
		}

		/// <summary>
		/// Get the current saved value.
		/// </summary>
		/// <returns>The <see cref="T"/> saved value.</returns>
		protected virtual T GetValue()
		{
			if (GetterFunction != null)
			{
				if (Cached && !Invalidated) return InternalValue;

				Invalidated = false;

				return InternalValue = GetterFunction.Invoke();
			}

			return InternalValue;
		}

		/// <summary>
		/// Sets the internal value.
		/// </summary>
		/// <param name="value">The new value to set.</param>
		protected virtual void SetValue(T value)
		{
			InternalValue = value;
		}
	}
}
