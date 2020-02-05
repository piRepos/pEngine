using System;
using System.Collections.Generic;
using System.Text;

namespace pEngine.Utils.Properties
{
	/// <summary>
	/// A property that calls an event when is changed
	/// and then is changing.
	/// </summary>
	public class Notify<T> : Property<T>
	{
		/// <summary>
		/// Makes a new instance of <see cref="Notify{T}"/> class.
		/// </summary>
		/// <param name="cached">If <see cref="true"/> getter wont be called until the next invalidation.</param>
		/// <param name="getter">Getter function, calculates the property's value.</param>
		public Notify(bool cached, Func<T> getter) : base(cached, getter)
		{

		}

		/// <summary>
		/// Makes a new instance of <see cref="Notify{T}"/> class.
		/// </summary>
		/// <param name="initialValue">Initial value.</param>
		public Notify(T initialValue, bool readOnly = false) : base(initialValue, readOnly)
		{
		}

		/// <summary>
		/// Makes a new instance of <see cref="Notify{P, T}"/> class.
		/// </summary>
		public Notify() : base()
		{
		}

		/// <summary>
		/// Raised before this property change value.
		/// </summary>
		public event EventHandler<(T newValue, T oldValue)> Changing;

		/// <summary>
		/// Raised after value has been changed.
		/// </summary>
		public event EventHandler<(T newValue, T oldValue)> Changed;

		/// <summary>
		/// Raised before returning the property value.
		/// </summary>
		public event EventHandler<T> Getting;

		/// <summary>
		/// Get the current saved value.
		/// </summary>
		/// <returns>The <see cref="T"/> saved value.</returns>
		protected override T GetValue()
		{
			var oldValue = InternalValue;

			Getting?.Invoke(this, InternalValue);

			if (GetterFunction != null)
			{
				var wasInvalidated = Invalidated;

				// - Calculate new value
				var res = base.GetValue();

				if (!Cached || wasInvalidated)
					Changed?.Invoke(this, (InternalValue, oldValue));

				return res;
			}

			return base.GetValue();
		}

		/// <summary>
		/// Sets the internal value.
		/// </summary>
		/// <param name="value">The new value to set.</param>
		protected override void SetValue(T value)
		{
			Changing?.Invoke(this, (value, InternalValue));

			var oldValue = InternalValue;

			base.SetValue(value);

			Changed?.Invoke(this, (value, oldValue));
		}
	}
}
