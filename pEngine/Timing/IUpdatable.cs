using pEngine.Timing.Base;

namespace pEngine.Timing
{
    public interface IUpdatable
    {
		/// <summary>
		/// Update the state of this object.
		/// </summary>
		/// <param name="clock">Update clock.</param>
		void Update(IFrameBasedClock clock);
    }
}
