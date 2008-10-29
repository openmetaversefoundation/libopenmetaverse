using System;

namespace ExtensionLoader
{
    /// <summary>
    /// Abstract base for extensions
    /// </summary>
    public interface IExtension<TOwner>
    {
        /// <summary>
        /// Called when the extension is starting
        /// </summary>
        void Start(TOwner owner);

        /// <summary>
        /// Called when the extension is stopping
        /// </summary>
        void Stop();
    }
}
