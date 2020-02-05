using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

using ShaderGen;
using ShaderGen.Glsl;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

namespace pEngine.Graphics.Shading
{
	public class Shader
	{ 
		/// <summary>
		/// Compile this shader from the C# code.
		/// </summary>
		public string GetCSCode()
		{
			// - Prepare the decompiler by passing the target assembly
			CSharpDecompiler d = new CSharpDecompiler(GetType().Assembly.Location, new DecompilerSettings
			{
				MakeAssignmentExpressions = false
			});

			var tree = d.DecompileModuleAndAssemblyAttributes();


			// - We want to get only the shader's code
			var name = new FullTypeName(this.GetType().FullName);

			// - Decompile shader's binary in order to get the source code
			var shaderCs = d.DecompileTypeAsString(name);

			Regex defaultRe = new Regex(@"=.+default\(.+\)");
			Regex computeRe = new Regex(@"(?<=\[ComputeShader\().+(?=\)\])");
			Regex uintRe = new Regex(@"[Uu]");

			shaderCs = defaultRe.Replace(shaderCs, "");

			var match = computeRe.Match(shaderCs);
			if (match.Success)
			{
				do
				{
					shaderCs = shaderCs.Replace(match.Value, uintRe.Replace(match.Value, ""));

					match = match.NextMatch();

				} while (match.Success);
			}

			return shaderCs;
		}
	}
}
