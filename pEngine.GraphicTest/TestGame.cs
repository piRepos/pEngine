using System;

using pEngine.Timing.Base;

using pEngine.Framework;
using pEngine.Framework.Geometry;

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

		public override void Initialize(Runtime pEngine)
		{
			base.Initialize(pEngine);

			var shape = new Shape();
			shape.Points.Add(new Point(new Vector2(1, 1)));
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
	}
}
