using System;
using OpenMetaverse.StructuredData;

namespace Simian
{
    public interface IPersistable
    {
        LLSD Serialize();
        void Deserialize(LLSD serialized);
    }
}
