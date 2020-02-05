using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using pEngine.Utils.Extensions;

namespace pEngine.Utils.Threading
{
	/// <summary>
	/// Marshals delegates to run from the Scheduler's base thread in a threadsafe manner
	/// </summary>
	public class Scheduler : IDisposable
	{

		/// <summary>
		/// The base thread is assumed to be the the thread on which the constructor is run.
		/// </summary>
		public Scheduler()
		{
			SetCurrentThread();
			pTimer.Start();
		}

		/// <summary>
		/// The base thread is assumed to be the the thread on which the constructor is run.
		/// </summary>
		public Scheduler(Thread mainThread)
		{
			SetCurrentThread(mainThread);
			pTimer.Start();
		}

		/// <summary>
		/// Returns whether we are on the main thread or not.
		/// </summary>
		protected virtual bool IsMainThread => Thread.CurrentThread.ManagedThreadId == pMainThreadId;

		/// <summary>
		/// Raised on scheduler overflow.
		/// </summary>
		public event EventHandler SchedulerQueueOverflow;

		#region Managing tasks

		/// <summary>
		/// Add a task to be scheduled.
		/// </summary>
		/// <param name="task">The work to be done.</param>
		/// <param name="forceScheduled">If set to false, the task will be executed immediately if we are on the main thread.</param>
		/// <returns>Whether we could run without scheduling</returns>
		public virtual bool Add(Action task, bool forceScheduled = true)
		{
			if (!forceScheduled && IsMainThread)
			{
				//We are on the main thread already - don't need to schedule.
				task.Invoke();
				return true;
			}

			pSchedulerQueue.Enqueue(task);

			return false;
		}

		/// <summary>
		/// Add a task to be scheduled.
		/// </summary>
		/// <param name="task">The work to be done.</param>
		/// <returns>Whether we could run without scheduling</returns>
		public virtual bool Add(ScheduledDelegate task)
		{
			lock (pTimedTasks)
			{
				if (task.RepeatInterval == 0)
					pPerUpdateTasks.Add(task);
				else
					pTimedTasks.AddInPlace(task);
			}
			return true;
		}

		/// <summary>
		/// Add a task which will be run after a specified delay.
		/// </summary>
		/// <param name="task">The work to be done.</param>
		/// <param name="timeUntilRun">Milliseconds until run.</param>
		/// <param name="repeat">Whether this task should repeat.</param>
		public ScheduledDelegate AddDelayed(Action task, double timeUntilRun, bool repeat = false)
		{
			ScheduledDelegate del = new ScheduledDelegate(task, pTimer.ElapsedMilliseconds + timeUntilRun, repeat ? timeUntilRun : -1);

			return Add(del) ? del : null;
		}

		/// <summary>
		/// Adds a task which will only be run once per frame, no matter how many times it was scheduled in the previous frame.
		/// </summary>
		/// <param name="task">The work to be done.</param>
		/// <returns>Whether this is the first queue attempt of this work.</returns>
		public bool AddOnce(Action task)
		{
			if (pSchedulerQueue.Contains(task))
				return false;

			pSchedulerQueue.Enqueue(task);

			return true;
		}

		#endregion

		#region Scheduler

		// - Scheduler task queue (managed by the scheduling algorithm).
		private readonly ConcurrentQueue<Action> pSchedulerQueue = new ConcurrentQueue<Action>();

		// - List of timed task in pending (executed one time).
		private readonly List<ScheduledDelegate> pTimedTasks = new List<ScheduledDelegate>();

		// - List of per update task in pending (executed periodically).
		private readonly List<ScheduledDelegate> pPerUpdateTasks = new List<ScheduledDelegate>();

		// - Scheduler timer
		private readonly Stopwatch pTimer = new Stopwatch();

		// - Current thread id
		private int pMainThreadId;

		/// <summary>
		/// Run any pending work tasks.
		/// </summary>
		/// <returns>true if any tasks were run.</returns>
		public int Update()
		{
			//purge any waiting timed tasks to the main schedulerQueue.
			lock (pTimedTasks)
			{
				long currentTime = pTimer.ElapsedMilliseconds;
				ScheduledDelegate sd;

				while (pTimedTasks.Count > 0 && (sd = pTimedTasks[0]).WaitTime <= currentTime)
				{
					pTimedTasks.RemoveAt(0);
					if (sd.Cancelled) continue;

					pSchedulerQueue.Enqueue(sd.RunTask);

					if (sd.RepeatInterval > 0)
					{
						if (pTimedTasks.Count < 1000)
							sd.WaitTime += sd.RepeatInterval;
						// This should never ever happen... but if it does, let's not overflow on queued tasks.
						else
						{
							SchedulerQueueOverflow?.Invoke(this, EventArgs.Empty);
							sd.WaitTime = pTimer.ElapsedMilliseconds + sd.RepeatInterval;
						}

						pTimedTasks.AddInPlace(sd);
					}
				}

				for (int i = 0; i < pPerUpdateTasks.Count; i++)
				{
					ScheduledDelegate task = pPerUpdateTasks[i];
					if (task.Cancelled)
					{
						pPerUpdateTasks.RemoveAt(i--);
						continue;
					}

					pSchedulerQueue.Enqueue(task.RunTask);
				}
			}

			int countRun = 0;

			Action action;
			while (pSchedulerQueue.TryDequeue(out action))
			{
				action?.Invoke();
				countRun++;
			}

			return countRun;
		}

		internal void SetCurrentThread(Thread thread)
		{
			pMainThreadId = thread?.ManagedThreadId ?? -1;
		}

		internal void SetCurrentThread()
		{
			pMainThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		#endregion

		#region IDisposable Support

		// - To detect redundant calls
		private bool pIsDisposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!pIsDisposed)
			{
				pIsDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		#endregion
	}
}
