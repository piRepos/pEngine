using System.Numerics;

using ShaderGen;

using pEngine.Framework.Geometry;

using static ShaderGen.ShaderBuiltins;

using SVector3 = System.Numerics.Vector3;
using SVector4 = System.Numerics.Vector4;

namespace pEngine.Graphics.Shading
{

	public class DefaultShader : Shader
	{
		#region Compute shader / Geometry translation

		public struct PointDescriptor
		{
			/// <summary>
			/// Point position.
			/// </summary>
			public SVector3 Position;

			/// <summary>
			/// True if this point is a control point for a
			/// bezier curve.
			/// </summary>
			public PointType Type;
		}

		/// <summary>
		/// Compute shader input data (points information).
		/// </summary>
		public StructuredBuffer<PointDescriptor> InputPoints;

		/// <summary>
		/// Compute shader output data (vertex struct for the vertex shader).
		/// </summary>
		public RWStructuredBuffer<SVector3> OutputVertexs;

		/// <summary>
		/// This shader takes a set of points and compute the geometry by adding borders,
		/// calculating curves and other geometry informations.
		/// </summary>
		[ComputeShader(1U, 1U, 1U)]
		public void ComputeShader()
		{
			OutputVertexs[DispatchThreadID.X] = InputPoints[DispatchThreadID.X].Position;
		}

		#endregion

		#region Vertex shader

		/// <summary>
		/// Projection matrix.
		/// </summary>
		public Matrix4x4 Projection;
		
		/// <summary>
		/// View matrix.
		/// </summary>
		public Matrix4x4 View;

		/// <summary>
		/// World transformation matrix.
		/// </summary>
		public Matrix4x4 World;

		public struct VertexData
		{
			/// <summary>
			/// Vertex position.
			/// </summary>
			[SystemPositionSemantic]
			public SVector4 Position;

			/// <summary>
			/// Vertex position.
			/// </summary>
			[ColorSemantic]
			public SVector4 Color;
		}

		/// <summary>
		/// Takes a vertex and send it directly to the rasterizer.
		/// </summary>
		[VertexShader]
		public FragmentData VertexShader(VertexData input)
		{
			FragmentData outValue = new FragmentData();

			outValue.Position = input.Position;
			outValue.Color = input.Color;

			// - Passtrough
			return outValue;
		}

		#endregion

		#region Fragment shader

		public struct FragmentData
		{
			/// <summary>
			/// Vertex position.
			/// </summary>
			[SystemPositionSemantic]
			public SVector4 Position;

			/// <summary>
			/// Pixel color.
			/// </summary>
			[ColorSemantic]
			public SVector4 Color;
		}

		/// <summary>
		/// Takes each pixel and do stuff.
		/// </summary>
		[FragmentShader]
		public SVector4 FragmentShader(FragmentData input)
		{
			// - Passtrough
			return input.Color;
		}

		#endregion
	}
}
