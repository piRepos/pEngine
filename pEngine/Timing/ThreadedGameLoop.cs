using System;
using System.Threading;

using pEngine.Timing.Base;

namespace pEngine.Timing
{
    public class ThreadedGameLoop : GameLoop
    {
		/// <summary>
		/// Makes a new instance of <see cref="GameLoop"/> class.
		/// </summary>
		/// <param name="action">Delegate that will be executed on each frame.</param>
		/// <param name="threadName">Name of the associated thread.</param>
		public ThreadedGameLoop(Action<IFrameBasedClock> action, string threadName)
			: base(action, threadName)
		{
			// - Thread initialization
			CurrentThread = new Thread(base.Run);
		}

		/// <summary>
		/// Makes a new instance of <see cref="GameLoop"/> class.
		/// </summary>
		/// <param name="action">Delegate that will be executed on each frame.</param>
		/// <param name="initialization">Initialization function (executed before loop start).</param>
		/// <param name="threadName">Name of the associated thread.</param>
		public ThreadedGameLoop(Action<IFrameBasedClock> action, Action initialization, string threadName)
			: base(action, initialization, threadName)
		{
			// - Thread initialization
			CurrentThread = new Thread(base.Run);
		}

		/// <summary>
		/// Start this gameloop.
		/// </summary>
		public override void Run()
		{
			CurrentThread.Start();
		}
	}
}
