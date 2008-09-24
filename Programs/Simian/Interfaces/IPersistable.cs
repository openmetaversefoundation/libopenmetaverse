using System;
using OpenMetaverse.StructuredData;

namespace Simian
{
    public interface IPersistable
    {
        string StoreName { get; }

        LLSD Serialize();
        void Deserialize(LLSD serialized);
    }
}
