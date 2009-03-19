using System;
using OpenMetaverse.StructuredData;

namespace Simian
{
    public interface IPersistable
    {
        OSD Serialize();
        void Deserialize(OSD serialized);
    }
}
