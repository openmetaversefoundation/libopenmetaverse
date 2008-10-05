using System;

namespace ExtensionLoader
{
    /// <summary>
    /// Abstract base for extensions
    /// </summary>
    public interface IExtension
    {
        /// <summary>
        /// Called when the extension is starting
        /// </summary>
        void Start();

        /// <summary>
        /// Called when the extension is stopping
        /// </summary>
        void Stop();
    }
}
