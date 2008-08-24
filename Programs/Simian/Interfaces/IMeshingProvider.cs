using System;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Rendering;

namespace Simian
{
    public interface IMeshingProvider
    {
        SimpleMesh GenerateSimpleMesh(Primitive prim, DetailLevel lod);
    }
}
