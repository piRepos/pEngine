namespace pEngine.Graphics.Data
{
	/// <summary>
	/// Contains all information for a single vertex.
	/// </summary>
	public struct VertexData
	{
		/// <summary>
		/// Creates a new instance of <see cref="VertexData"/> class.
		/// </summary>
		/// <param name="position">Vertex position.</param>
		public VertexData(Vector3 position)
			: this(position, Color4.Black)
		{

		}

		/// <summary>
		/// Creates a new instance of <see cref="VertexData"/> class.
		/// </summary>
		/// <param name="position">Vertex position.</param>
		/// <param name="color">Vertex color.</param>
		public VertexData(Vector3 position, Color4 color)
		{
			Position = position;
			Color = color;
		}

		/// <summary>
		/// Vertex 3D position.
		/// </summary>
		public Vector3 Position { get; set; }

		/// <summary>
		/// Vertex RGBA color.
		/// </summary>
		public Color4 Color { get; set; }

		/// <summary>
		/// Position stride.
		/// </summary>
		public static uint PositionStride => 0;

		/// <summary>
		/// Color stride.
		/// </summary>
		public static uint ColorStride => Vector3.SizeInBytes;

		/// <summary>
		/// Size for the entire struct.
		/// </summary>
		public static uint SizeInBytes => Vector3.SizeInBytes + sizeof(float) * 4;

	}
}
