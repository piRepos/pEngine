using System.Diagnostics;

using pEngine.Timing.Base;

namespace pEngine.Timing.Clocks
{
    public class StopwatchClock : Stopwatch, IAdjustableClock
    {

        /// <summary>
        /// Make a new hinstance of <see cref="StopwatchClock"/>.
        /// </summary>
        /// <param name="start"></param>
        public StopwatchClock(bool start = false)
        {
            if (start)
                Start();
        }

        // - Elapsed time with clock speed modification
        private double pStopwatchMilliseconds => (double)ElapsedTicks / Frequency * 1000;

        // - Current clock speed
        private double pInternalRate = 1;

        /// <summary>
        /// Seek offset adjust value.
        /// </summary>
        private double SeekOffset;

        /// <summary>
        /// Keep track of how much stopwatch time we have used at previous rates.
        /// </summary>
        private double RateChangeUsed;

        /// <summary>
        /// Keep track of the resultant time that was accumulated at previous rates.
        /// </summary>
        private double RateChangeAccumulated;

        /// <summary>
        /// Current clock time.
        /// </summary>
        public double CurrentTime => (pStopwatchMilliseconds - RateChangeUsed) * Rate + RateChangeAccumulated + SeekOffset;

        /// <summary>
        /// Clock speed.
        /// </summary>
        public double Rate
        {
            get { return pInternalRate; }

            set
            {
                if (pInternalRate == value) return;

                RateChangeAccumulated += (pStopwatchMilliseconds - RateChangeUsed) * pInternalRate;
                RateChangeUsed = pStopwatchMilliseconds;

                pInternalRate = value;
            }
        }

        /// <summary>
        /// Seek to a specific time position.
        /// </summary>
        /// <returns>Whether a seek was possible.</returns>
        public bool Seek(double position)
        {
            SeekOffset = 0;
            SeekOffset = position - CurrentTime;
            return true;
        }
    }
}