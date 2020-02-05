using System;
using System.Collections.Generic;
using System.Text;

namespace pEngine.Utils.Properties
{
	/// <summary>
	/// Allows both mono directional and bidirectional 
	/// connection between two properties.
	/// </summary>
	public class Bindable<T> : Notify<T>
	{
		/// <summary>
		/// Makes a new instance of <see cref="Bindable{T}"/> class.
		/// </summary>
		/// <param name="cached">If <see cref="true"/> getter wont be called until the next invalidation.</param>
		/// <param name="getter">Getter function, calculates the property's value.</param>
		public Bindable(bool cached, Func<T> getter) : base(cached, getter)
		{
			AllowedDirections = BindingDirection.Read;
			Bindings = new Dictionary<object, Action>();
		}

		/// <summary>
		/// Makes a new instance of <see cref="Bindable{T}"/> class.
		/// </summary>
		/// <param name="initialValue">Initial value.</param>
		/// <param name="readOnly">Sets readonly mode for this property.</param>
		/// <param name="allowedDirections">Describes the allowed binding directions for this property.</param>
		public Bindable(T initialValue, bool readOnly = false, BindingDirection allowedDirections = BindingDirection.Both) 
			: base(initialValue, readOnly)
		{
			// - Exclude write permission if the property is readonly
			AllowedDirections = readOnly ? allowedDirections & ~BindingDirection.Write : allowedDirections;

			Bindings = new Dictionary<object, Action>();
		}

		/// <summary>
		/// Makes a new instance of <see cref="Bindable{T}"/> class.
		/// </summary>
		/// <param name="initialValue">Initial value.</param>
		public Bindable(BindingDirection allowedDirections = BindingDirection.Both) : base()
		{
			AllowedDirections = allowedDirections;
			Bindings = new Dictionary<object, Action>();
		}

		/// <summary>
		/// Describes the allowed binding directions for this property.
		/// </summary>
		public BindingDirection AllowedDirections { get; }

		/// <summary>
		/// Contains all bindings.
		/// </summary>
		protected Dictionary<object, Action> Bindings { get; }

		/// <summary>
		/// Creates a data binding between this property and the specified one.
		/// </summary>
		/// <param name="property">Binding target property.</param>
		/// <param name="direction">Binding direction.</param>
		public void Bind(Bindable<T> property, BindingDirection direction = BindingDirection.Both)
		{
			bool invalidOperation = false;

			invalidOperation = invalidOperation || direction.HasFlag(BindingDirection.Read) && !AllowedDirections.HasFlag(BindingDirection.Write);
			invalidOperation = invalidOperation || direction.HasFlag(BindingDirection.Write) && !AllowedDirections.HasFlag(BindingDirection.Read);

			if (invalidOperation) throw new InvalidOperationException("This binding direction is not allowed");

			// - Get a weak reference of the property
			var propRef = new WeakReference<Bindable<T>>(property);

			if (direction.HasFlag(BindingDirection.Write))
			{
				var bindingAction = new Action(() =>
				{
					// - Check if the target property is still alive
					if (!propRef.TryGetTarget(out Bindable<T> prop))
					{
						Bindings.Remove(propRef);
						return;
					}

					// - Update value
					prop.Value = InternalValue;
				});

				Bindings.Add(propRef, bindingAction);
			}

			// - If we want to read the target property, delegate binding process
			if (direction.HasFlag(BindingDirection.Read))
				property.Bind(this, BindingDirection.Write);
		}

		/// <summary>
		/// Creates a data binding between this property and another which
		/// have a different type.
		/// </summary>
		/// <typeparam name="T2">Target property type.</typeparam>
		/// <param name="property">Binding target property.</param>
		/// <param name="outAdapter">Data conversion function.</param>
		public void Bind<T2>(Bindable<T2> property, Func<T, T2> outAdapter)
		{
			if (outAdapter == null)
				throw new InvalidOperationException("No binding adapter provided.");

			if (!AllowedDirections.HasFlag(BindingDirection.Read))
				throw new InvalidOperationException("This binding direction is not allowed.");

			// - Get a weak reference of the property
			var propRef = new WeakReference<Bindable<T2>>(property);

			var bindingAction = new Action(() =>
			{
				// - Check if the target property is still alive
				if (!propRef.TryGetTarget(out Bindable<T2> prop))
				{
					Bindings.Remove(propRef);
					return;
				}

				// - Update value
				prop.Value = outAdapter.Invoke(InternalValue);
			});

			Bindings.Add(propRef, bindingAction);
		}

		/// <summary>
		/// Creates a data binding between this property and another which
		/// have a different type.
		/// </summary>
		/// <typeparam name="T2">Target property type.</typeparam>
		/// <param name="property">Binding target property.</param>
		/// <param name="outAdapter">Data conversion function.</param>
		public void Bind<T2>(Bindable<T2> property, Func<T2, T> inAdapter)
		{
			// - Delegate binding
			property.Bind(this, inAdapter);
		}

		/// <summary>
		/// Creates a data binding between this property and another which
		/// have a different type.
		/// </summary>
		/// <typeparam name="T2">Target property type.</typeparam>
		/// <param name="property">Binding target property.</param>
		/// <param name="inAdapter">Data conversion read function.</param>
		/// <param name="outAdapter">Data conversion write function.</param>
		public void Bind<T2>(Bindable<T2> property, Func<T2, T> inAdapter, Func<T, T2> outAdapter)
		{
			Bind(property, outAdapter);
			Bind(property, inAdapter);
		}

		protected override T GetValue()
		{
			// - If one of this condition is true, the value will change
			bool changed = GetterFunction != null && (Invalidated || !Cached);

			var res = base.GetValue();

			if (changed)
			{
				// - Update remote value
				foreach (var binding in Bindings.Values)
					binding?.Invoke();
			}

			return res;
		}

		protected override void SetValue(T value)
		{
			base.SetValue(value);

			// - Update remote value
			foreach (var binding in Bindings.Values)
				binding?.Invoke();
		}

		/// <summary>
		/// Base interface for binding descriptor implementation.
		/// </summary>
		protected interface IBindingDescriptor { }

		/// <summary>
		/// Describes a binding between two properties.
		/// </summary>
		/// <typeparam name="T1">Source type.</typeparam>
		/// <typeparam name="T2">Destination type.</typeparam>
		protected struct BindingDescriptor<T1, T2> : IBindingDescriptor
		{
			/// <summary>
			/// Target bindable property.
			/// </summary>
			public WeakReference<Bindable<T2>> Target { get; set; }

			/// <summary>
			/// Binding direction.
			/// </summary>
			public BindingDirection Direction { get; set; }

			/// <summary>
			/// Apply a function to the binding transfer.
			/// </summary>
			public Func<T1, T2> Adapter { get; set; }
		}
	}

	/// <summary>
	/// Binding directions.
	/// </summary>
	[Flags]
	public enum BindingDirection
	{
		/// <summary>
		/// No binding direction (binding not allowed).
		/// </summary>
		None = 0,

		/// <summary>
		/// This binding can only be readed from other properties.
		/// </summary>
		Read = 1,

		/// <summary>
		/// This binding can be modified from other properties.
		/// </summary>
		Write = 2,

		/// <summary>
		/// Bidirectional binding allowed.
		/// </summary>
		Both = Read | Write
	}
}
