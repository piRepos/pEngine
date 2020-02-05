using System;
using System.Threading;

using pEngine.Timing.Base;
using pEngine.Timing.Clocks;

using pEngine.Utils.Threading;

using pEngine.Diagnostic.Timing;

namespace pEngine.Timing
{
	public class GameLoop
    {
		
		/// <summary>
		/// Makes a new instance of <see cref="GameLoop"/> class.
		/// </summary>
		/// <param name="action">Delegate that will be executed on each frame.</param>
		/// <param name="threadName">Name of the associated thread.</param>
		public GameLoop(Action<IFrameBasedClock> action, string threadName)
		{
			pLoopAction = action;
			ExitRequest = false;
			ThreadName = threadName;

			// - Initialize performance collector
			Performance = new PerformanceCollector(threadName);

			// - Initialize scheduler
			Scheduler = new Scheduler();

			// - Initialize clock
			Clock = new ThrottledFramedClock();
		}

		/// <summary>
		/// Makes a new instance of <see cref="GameLoop"/> class.
		/// </summary>
		/// <param name="action">Delegate that will be executed on each frame.</param>
		/// <param name="initialization">Initialization function (executed before loop start).</param>
		/// <param name="threadName">Name of the associated thread.</param>
		public GameLoop(Action<IFrameBasedClock> action, Action initialization, string threadName)
			: this(action, threadName)
		{
			pInitAction = initialization;
		}

		// - Initialization function
		private Action pInitAction = null;

		// - Action to be executed each frame
		Action<IFrameBasedClock> pLoopAction;
		
		/// <summary>
		/// <see cref="GameLoop"/> thread name.
		/// </summary>
		public string ThreadName { get; }

		/// <summary>
		/// Game loop object.
		/// </summary>
		public ThrottledFramedClock Clock { get; }

		/// <summary>
		/// Performance collector for this loop.
		/// </summary>
		public PerformanceCollector Performance { get; }

		/// <summary>
		/// Scheduler for this thread, this is usefull to invoke function
		/// on this thread from others threads.
		/// </summary>
		public Scheduler Scheduler { get; }

		/// <summary>
		/// Game loop thread.
		/// </summary>
		public Thread CurrentThread { get; protected set; }

		/// <summary>
		/// Current frame id.
		/// </summary>
		public long FrameId { get; private set; }

		/// <summary>
		/// Current loop must close?
		/// </summary>
		public bool ExitRequest { get; private set; }

		/// <summary>
		/// Check if actually i'm on the loop thread.
		/// </summary>
		public bool ImOnThisThread => Thread.CurrentThread == CurrentThread;

		/// <summary>
		/// <see cref="true"/> if this loop is running.
		/// </summary>
		public bool IsRunning => Clock.IsRunning;

		/// <summary>
		/// Triggered on loop initialization.
		/// </summary>
		public event Action OnInitialize;

		/// <summary>
		/// Start this gameloop.
		/// </summary>
		public virtual void Run()
		{
			Thread.CurrentThread.Name = ThreadName;
			CurrentThread = Thread.CurrentThread;

			Scheduler.SetCurrentThread();

			pInitAction?.Invoke();

			OnInitialize?.Invoke();

			while (!ExitRequest)
				ProcessFrame();
		}

		/// <summary>
		/// Begin loop end.
		/// </summary>
		public void Stop()
		{
			ExitRequest = true;
		}

		/// <summary>
		/// Runs all the utilities and main functions for each frame.
		/// </summary>
		protected void ProcessFrame()
		{
			using (Performance.StartCollect("Scheduler"))
				Scheduler.Update();

			using (Performance.StartCollect("Task"))
				pLoopAction?.Invoke(Clock);

			using (Performance.StartCollect("Idle"))
				Clock.ProcessFrame();

			FrameId = (FrameId + 1) % long.MaxValue;
		}
    }
}
