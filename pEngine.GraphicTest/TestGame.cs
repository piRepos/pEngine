using System;

using pEngine.Timing.Base;

using pEngine.Framework;
using pEngine.Framework.Geometry;

using pEngine.Utils.Threading;

namespace pEngine.GraphicTest
{
	public class TestGame : Game
	{

		/// <summary>
		/// Makes a new instance of <see cref="TestGame"/> class.
		/// </summary>
		public TestGame() : base()
		{

		}

		Shape shape;

		public override void Initialize(Runtime pEngine)
		{
			base.Initialize(pEngine);

			shape = new Shape();
			shape.Points.Add(new Point(new Vector2(1, 1)));
			shape.Points.Add(new Point(new Vector2(-1, 1)));
			shape.Points.Add(new Point(new Vector2(1, -1)));

			shape.Edges.Add(new HalfEdge(shape.Points[0], shape.Points[1]));
			shape.Edges.Add(new HalfEdge(shape.Points[1], shape.Points[2]));
			shape.Edges.Add(new HalfEdge(shape.Points[2], shape.Points[0]));

			shape.Edges[0].Next = shape.Edges[1];
			shape.Edges[1].Next = shape.Edges[2];
			shape.Edges[2].Next = shape.Edges[0];

			shape.Edges[0].Prev = shape.Edges[2];
			shape.Edges[1].Prev = shape.Edges[0];
			shape.Edges[2].Prev = shape.Edges[1];

			shape.Faces.Add(new Face(shape.Edges[0]));
			shape.Loaded += Shape_Loaded;
			LoadResource(shape);




		}

		private void Shape_Loaded(object sender, EventArgs e)
		{
			//throw new NotImplementedException();
		}

		public override void Update(IFrameBasedClock clock)
		{
			base.Update(clock);
		}

		public override Descriptor GetDescriptor(Scheduler scheduler)
		{
			var descriptor = base.GetDescriptor(scheduler); return descriptor;

			if (shape.LoadingState.Value == Resource.State.Loaded)
			{
				Graphics.Asset ass = new Graphics.Asset();
				ass.Mesh = shape.Attachment;

				descriptor.Assets = new Graphics.Asset[]
				{
				ass
				};
			}

			return descriptor;
		}
	}
}
