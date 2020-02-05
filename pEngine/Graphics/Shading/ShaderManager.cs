using System;
using System.Linq;
using System.Collections.Generic;

using ShaderGen;
using System.Collections;

namespace pEngine.Graphics.Shading
{
	/// <summary>
	/// Store all running shaders and manage the compilation.
	/// </summary>
	public class ShaderManager : IReadOnlyDictionary<Type, ShaderInstance>
	{
		/// <summary>
		/// Creates a new instance of <see cref="ShaderManager"/> class.
		/// </summary>
		public ShaderManager()
		{
			// - Initialize the store
			ShaderStore = new Dictionary<Type, ShaderInstance>();
		}

		/// <summary>
		/// Internal shader store.
		/// </summary>
		protected Dictionary<Type, ShaderInstance> ShaderStore { get; }

		/// <summary>
		/// Loads a shader by specifying the class which contains the shader.
		/// </summary>
		public void LoadShader<ShaderType>() => LoadShader(typeof(ShaderType));

		/// <summary>
		/// Loads a shader by specifying the class which contains the shader.
		/// </summary>
		/// <param name="shader">Shader C# type.</param>
		public void LoadShader(Type shader)
		{
			// - Return if this shader is already loaded
			if (ShaderStore.ContainsKey(shader)) return;

			// - Check if the specified type is a shader
			if (shader.BaseType != typeof(Shader))
				throw new FormatException("The shader parameter must inherith from pEngine.Graphics.Shading.Shader");

			// - Prepare shader instance information struct
			ShaderInstance instance = new ShaderInstance();

			instance.Type = shader;

			// - Create an instance for this shader
			Shader sh = Activator.CreateInstance(shader) as Shader;

			// - Get shader functions information
			var methods = shader.GetMethods();
			var vertexFunctions = methods.Where(x => x.CustomAttributes.Any(attr => attr.AttributeType == typeof(VertexShaderAttribute)));
			var fragmentFunctions = methods.Where(x => x.CustomAttributes.Any(attr => attr.AttributeType == typeof(FragmentShaderAttribute)));
			var computeFunctions = methods.Where(x => x.CustomAttributes.Any(attr => attr.AttributeType == typeof(ComputeShaderAttribute)));
			instance.HasVertexShader = vertexFunctions.Count() > 0;
			instance.HasFragmentShader = fragmentFunctions.Count() > 0;
			instance.HasComputeShader = computeFunctions.Count() > 0;
			instance.VertexFunctionName = instance.HasVertexShader ? vertexFunctions.First().Name : "";
			instance.FragmentFunctionName = instance.HasFragmentShader ? fragmentFunctions.First().Name : "";
			instance.ComputeFunctionName = instance.HasComputeShader ? computeFunctions.First().Name : "";

			// - Gets CS code
			instance.CSourceCode = sh.GetCSCode();

			// - Perform shader compilation
			instance = CompileShader(instance);

			// - Adds the shader instance to the store
			ShaderStore.Add(shader, instance);
		}


		/// <summary>
		/// Compilation function (must be overrided for a specific graphic library implementation).
		/// </summary>
		/// <param name="shader">Shader to compile.</param>
		/// <returns>Shader instance with compiled binary.</returns>
		protected virtual ShaderInstance CompileShader(ShaderInstance shader)
		{
			return shader;
		}

		/// <summary>
		/// Reload shaders after a device disposition.
		/// </summary>
		public virtual void CreateShaders()
		{
			
		}

		#region IReadonlyDictionary implementation

		public ShaderInstance this[Type key] => ShaderStore[key];

		public IEnumerable<Type> Keys => ShaderStore.Keys;

		public IEnumerable<ShaderInstance> Values => ShaderStore.Values;

		public int Count => ShaderStore.Count;

		public bool ContainsKey(Type key) => ShaderStore.ContainsKey(key);

		public IEnumerator<KeyValuePair<Type, ShaderInstance>> GetEnumerator() => ShaderStore.GetEnumerator();

		public bool TryGetValue(Type key, out ShaderInstance value) => ShaderStore.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator() => ShaderStore.GetEnumerator();

		#endregion

		#region Resources disposal

		/// <summary>
		/// Dispose logic variable.
		/// </summary>
		private bool pDisposed { get; set; }

		/// <summary>
		/// Implement IDisposable.
		/// </summary>
		public void Dispose()
		{
			// Check to see if Dispose has already been called.
			if (!pDisposed)
			{
				Dispose(true);

				// Note disposing has been done.
				pDisposed = true;
			}

			// - This object will be cleaned up by the Dispose method.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose(bool disposing) executes in two distinct scenarios.
		/// If disposing equals <see cref="true"/>, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// If disposing equals <see cref="false"/>, the method has been called by the
		/// runtime from inside the finalizer and you should not reference
		/// other objects. Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"><see cref="True"/> if called from user's code.</param>
		protected virtual void Dispose(bool disposing)
		{
			foreach (var shader in ShaderStore.Values)
			{
				shader.Dispose();
			}
		}

		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// </summary>
		~ShaderManager()
		{
			Dispose(false);
		}

		#endregion
	}

	/// <summary>
	/// Contains a shader instance.
	/// </summary>
	public class ShaderInstance : IDisposable
	{
		/// <summary>
		/// Makes a new instance of <see cref="ShaderInstance"/> class.
		/// </summary>
		public ShaderInstance()
		{

		}

		/// <summary>
		/// Copy contructor.
		/// </summary>
		/// <param name="copy">Source object.</param>
		public ShaderInstance(ShaderInstance copy)
		{
			Type = copy.Type;
			HasVertexShader = copy.HasVertexShader;
			HasFragmentShader = copy.HasFragmentShader;
			HasComputeShader = copy.HasComputeShader;
			VertexBinary = copy.VertexBinary;
			FragmentBinary = copy.FragmentBinary;
			ComputeBinary = copy.ComputeBinary;
			VertexSource = copy.VertexSource;
			FragmentSource = copy.FragmentSource;
			ComputeSource = copy.ComputeSource;
			VertexFunctionName = copy.VertexFunctionName;
			FragmentFunctionName = copy.FragmentFunctionName;
			ComputeFunctionName = copy.ComputeFunctionName;
			CompileOutput = copy.FragmentSource;
			CSourceCode = copy.CSourceCode;
			Loaded = copy.Loaded;
		}

		/// <summary>
		/// Shader class type.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// <see cref="true"/> if this shaders contains a vertex shader function.
		/// </summary>
		public bool HasVertexShader { get; set; }

		/// <summary>
		/// <see cref="true"/> if this shaders contains a fragment shader function.
		/// </summary>
		public bool HasFragmentShader { get; set; }

		/// <summary>
		/// <see cref="true"/> if this shaders contains a compute shader function.
		/// </summary>
		public bool HasComputeShader { get; set; }

		/// <summary>
		/// Gets the compiled vertex shader binary for Vulkan.
		/// </summary>
		public uint[] VertexBinary { get; set; }

		/// <summary>
		/// Gets the compiled fragment shader binary for Vulkan.
		/// </summary>
		public uint[] FragmentBinary { get; set; }

		/// <summary>
		/// Gets the compiled compute shader binary for Vulkan.
		/// </summary>
		public uint[] ComputeBinary { get; set; }

		/// <summary>
		/// Gets the compiled vertex shader binary for Vulkan.
		/// </summary>
		public string VertexSource { get; set; }

		/// <summary>
		/// Gets the compiled fragment shader binary for Vulkan.
		/// </summary>
		public string FragmentSource { get; set; }

		/// <summary>
		/// Gets the compiled compute shader binary for Vulkan.
		/// </summary>
		public string ComputeSource { get; set; }

		/// <summary>
		/// Gets the vertex shader function name.
		/// </summary>
		public string VertexFunctionName { get; set; }

		/// <summary>
		/// Gets the fragment shader function name.
		/// </summary>
		public string FragmentFunctionName { get; set; }

		/// <summary>
		/// Gets the compute function name.
		/// </summary>
		public string ComputeFunctionName { get; set; }

		/// <summary>
		/// Contains all compilation errors and warnings.
		/// </summary>
		public string CompileOutput { get; set; }

		/// <summary>
		/// Decompiled CSharp code.
		/// </summary>
		public string CSourceCode { get; set; }

		/// <summary>
		/// True if the shader is correctly loaded.
		/// </summary>
		public bool Loaded { get; set; }

		#region Resources disposal

		/// <summary>
		/// Dispose logic variable.
		/// </summary>
		private bool pDisposed { get; set; }

		/// <summary>
		/// Implement IDisposable.
		/// </summary>
		public void Dispose()
		{
			// Check to see if Dispose has already been called.
			if (!pDisposed)
			{
				Dispose(true);

				// Note disposing has been done.
				pDisposed = true;
			}

			// - This object will be cleaned up by the Dispose method.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose(bool disposing) executes in two distinct scenarios.
		/// If disposing equals <see cref="true"/>, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// If disposing equals <see cref="false"/>, the method has been called by the
		/// runtime from inside the finalizer and you should not reference
		/// other objects. Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"><see cref="True"/> if called from user's code.</param>
		protected virtual void Dispose(bool disposing)
		{

		}

		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// </summary>
		~ShaderInstance()
		{
			Dispose(false);
		}

		#endregion
	}
}
