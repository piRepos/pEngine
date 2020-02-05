using System;
using System.Linq;
using System.Reflection;

using pEngine.Graphics.Shading;

using SharpVk;

using ShaderGen;
using ShaderGen.Glsl;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace pEngine.Graphics.Vulkan.Shading
{
	/// <summary>
	/// Store all running shaders and manage the compilation.
	/// </summary>
	public class VKShaderManager : ShaderManager
	{
		/// <summary>
		/// Creates a new instance of <see cref="VKShaderManager"/> class.
		/// </summary>
		public VKShaderManager(GraphicDevicee device) : base()
		{
			RenderingDevice = device;
		}

		/// <summary>
		/// Vulkan rendering device.
		/// </summary>
		GraphicDevicee RenderingDevice { get; }

		/// <summary>
		/// Compilation function (must be overrided for a specific graphic library implementation).
		/// </summary>
		/// <param name="shader">Shader to compile.</param>
		/// <returns>Shader instance with compiled binary.</returns>
		protected override ShaderInstance CompileShader(ShaderInstance shader)
		{
			var outShader = new VKShaderInstance(base.CompileShader(shader));

			// - Gets all shader referenced assemblies
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var mathAsm = typeof(System.Numerics.Vector4).Assembly.GetReferencedAssemblies();
			assemblies = assemblies.Concat(mathAsm.Select(x => Assembly.Load(x.FullName))).ToArray();
			var assemblyMeta = assemblies.Select(x => MetadataReference.CreateFromFile(x.Location));

			// - Compile the shader code obtaining an IL version
			CSharpCompilation compilation = CSharpCompilation.Create
			(
				"pEngine",
				syntaxTrees: new[] { CSharpSyntaxTree.ParseText(outShader.CSourceCode) },
				references: assemblyMeta,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
			);

			// - Get the assembly class path
			string fullName = outShader.Type.FullName;

			// - We want to compile targetting GLSL450 compatible for Vulkan
			var backend = new Glsl450Backend(compilation);

			// - Prepare a GLSL compiler providing the C# IL binary
			var generator = new ShaderGenerator
			(
				compilation, backend,
				outShader.HasVertexShader ? $"{fullName}.{shader.VertexFunctionName}" : null,
				outShader.HasFragmentShader ? $"{fullName}.{shader.FragmentFunctionName}" : null,
				outShader.HasComputeShader ? $"{fullName}.{shader.ComputeFunctionName}" : null
			);

			// - Compile to the GLSL code
			var shaders = generator.GenerateShaders().GetOutput(backend);

			// - Stores all shader sources
			outShader.VertexSource = outShader.HasVertexShader ? shaders.Where(x => x.VertexShaderCode != null).First().VertexShaderCode : "";
			outShader.FragmentSource = outShader.HasFragmentShader ? shaders.Where(x => x.FragmentShaderCode != null).First().FragmentShaderCode : "";
			outShader.ComputeSource = outShader.HasComputeShader ? shaders.Where(x => x.ComputeShaderCode != null).First().ComputeShaderCode : "";

			// - This class will compile the GLSL code to the SPIR-V binary
			ShaderCompiler compiler = new ShaderCompiler();

			if (outShader.HasVertexShader)
			{
				var res = compiler.CompileToSpirV(outShader.VertexSource, ShaderCompiler.ShaderKind.VertexShader, "main");

				if (res.ErrorCount > 0)
					throw new InvalidProgramException(res.Errors);

				outShader.VertexBinary = res.Binary;
			}

			if (outShader.HasFragmentShader)
			{
				var res = compiler.CompileToSpirV(outShader.FragmentSource, ShaderCompiler.ShaderKind.FragmentShader, "main");

				if (res.ErrorCount > 0)
					throw new InvalidProgramException(res.Errors);

				outShader.FragmentBinary = res.Binary;
			}

			if (outShader.HasComputeShader)
			{
				var res = compiler.CompileToSpirV(outShader.ComputeSource, ShaderCompiler.ShaderKind.ComputeShader, "main");

				if (res.ErrorCount > 0)
					throw new InvalidProgramException(res.Errors);

				outShader.ComputeBinary = res.Binary;
			}

			return outShader;
		}

		/// <summary>
		/// Reload shaders after a device disposition.
		/// </summary>
		public override void CreateShaders()
		{
			var device = RenderingDevice.LogicalDevice;

			foreach (VKShaderInstance shader in Values)
			{
				if (shader.HasVertexShader && shader.VKVertexShader == null)
					shader.VKVertexShader = device.CreateShaderModule(shader.VertexBinary.Length * 4, shader.VertexBinary);

				if (shader.HasFragmentShader && shader.VKFragmentShader == null)
					shader.VKFragmentShader = device.CreateShaderModule(shader.FragmentBinary.Length * 4, shader.FragmentBinary);

				if (shader.HasComputeShader && shader.VKComputeShader == null)
					shader.VKComputeShader = device.CreateShaderModule(shader.ComputeBinary.Length * 4, shader.ComputeBinary);
			}
		}
	}

	/// <summary>
	/// Contains a vulkan shader instance.
	/// </summary>
	public class VKShaderInstance : ShaderInstance
	{
		/// <summary>
		/// Makes a new instance of <see cref="VKShaderInstance"/> class.
		/// </summary>
		public VKShaderInstance() : base()
		{

		}

		/// <summary>
		/// Copy constructor from base class.
		/// </summary>
		/// <param name="copy">Source class.</param>
		public VKShaderInstance(ShaderInstance copy) : base(copy)
		{

		}

		/// <summary>
		/// Vulkan vertex shader instance.
		/// </summary>
		public ShaderModule VKVertexShader { get; set; }

		/// <summary>
		/// Vulkan fragment shader instance.
		/// </summary>
		public ShaderModule VKFragmentShader { get; set; }

		/// <summary>
		/// Vulkan compute shader instance.
		/// </summary>
		public ShaderModule VKComputeShader { get; set; }

		/// <summary>
		/// Implement IDisposable.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			VKVertexShader?.Dispose();
			VKFragmentShader?.Dispose();
			VKComputeShader?.Dispose();

			VKVertexShader = null;
			VKFragmentShader = null;
			VKComputeShader = null;
		}
	}
}
