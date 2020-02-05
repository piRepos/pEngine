using System;

using System.Reflection;

using pEngine.Windows;
using pEngine.Graphics;

namespace pEngine.GraphicTest
{
	class Program
	{
		static void Main(string[] args)
		{
			using (Runtime engine = new WindowsRuntime())
			{
				TestGame game = new TestGame();

				engine.Run(game);
			}
		}
	}
}
