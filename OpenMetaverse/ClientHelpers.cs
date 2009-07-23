using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace OpenMetaverse
{
    public static class ClientHelpers
    {
        /// <summary>
        /// Converts a list of primitives to an object that can be serialized
        /// with the LLSD system
        /// </summary>
        /// <param name="prims">Primitives to convert to a serializable object</param>
        /// <returns>An object that can be serialized with LLSD</returns>
        public static StructuredData.OSD PrimListToOSD(List<Primitive> prims)
        {
            StructuredData.OSDMap map = new OpenMetaverse.StructuredData.OSDMap(prims.Count);

            for (int i = 0; i < prims.Count; i++)
                map.Add(prims[i].LocalID.ToString(), prims[i].GetOSD());

            return map;
        }

        /// <summary>
        /// Deserializes OSD in to a list of primitives
        /// </summary>
        /// <param name="osd">Structure holding the serialized primitive list,
        /// must be of the SDMap type</param>
        /// <returns>A list of deserialized primitives</returns>
        public static List<Primitive> OSDToPrimList(StructuredData.OSD osd)
        {
            if (osd.Type != StructuredData.OSDType.Map)
                throw new ArgumentException("LLSD must be in the Map structure");

            StructuredData.OSDMap map = (StructuredData.OSDMap)osd;
            List<Primitive> prims = new List<Primitive>(map.Count);

            foreach (KeyValuePair<string, StructuredData.OSD> kvp in map)
            {
                Primitive prim = Primitive.FromOSD(kvp.Value);
                prim.LocalID = UInt32.Parse(kvp.Key);
                prims.Add(prim);
            }

            return prims;
        }        
    }
}
