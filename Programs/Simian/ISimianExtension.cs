namespace Simian
{
    /// <summary>
    /// Abstract base for rendering plugins
    /// </summary>
    public interface ISimianExtension
    {
        /// <summary>
        /// Called when the simulator is initializing
        /// </summary>
        void Start();

        /// <summary>
        /// Called when the simulator is shutting down
        /// </summary>
        void Stop();
    }
}
