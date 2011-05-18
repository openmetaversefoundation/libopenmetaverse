using System;
using System.IO;
using System.Xml;

namespace OpenMetaverse.Assets
{
    public class RegionSettings
    {
        public bool AllowDamage;
        public bool AllowLandResell;
        public bool AllowLandJoinDivide;
        public bool BlockFly;
        public bool BlockLandShowInSearch;
        public bool BlockTerraform;
        public bool DisableCollisions;
        public bool DisablePhysics;
        public bool DisableScripts;
        public int MaturityRating;
        public bool RestrictPushing;
        public int AgentLimit;
        public float ObjectBonus;

        public UUID TerrainDetail0;
        public UUID TerrainDetail1;
        public UUID TerrainDetail2;
        public UUID TerrainDetail3;
        public float TerrainHeightRange00;
        public float TerrainHeightRange01;
        public float TerrainHeightRange10;
        public float TerrainHeightRange11;
        public float TerrainStartHeight00;
        public float TerrainStartHeight01;
        public float TerrainStartHeight10;
        public float TerrainStartHeight11;

        public float WaterHeight;
        public float TerrainRaiseLimit;
        public float TerrainLowerLimit;
        public bool UseEstateSun;
        public bool FixedSun;

        public static RegionSettings FromStream(Stream stream)
        {
            RegionSettings settings = new RegionSettings();
            System.Globalization.NumberFormatInfo nfi = Utils.EnUsCulture.NumberFormat;

            using (XmlTextReader xtr = new XmlTextReader(stream))
            {
                xtr.ReadStartElement("RegionSettings");
                xtr.ReadStartElement("General");

                while (xtr.Read() && xtr.NodeType != XmlNodeType.EndElement)
                {
                    switch (xtr.Name)
                    {
                        case "AllowDamage":
                            settings.AllowDamage = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "AllowLandResell":
                            settings.AllowLandResell = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "AllowLandJoinDivide":
                            settings.AllowLandJoinDivide = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "BlockFly":
                            settings.BlockFly = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "BlockLandShowInSearch":
                            settings.BlockLandShowInSearch = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "BlockTerraform":
                            settings.BlockTerraform = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "DisableCollisions":
                            settings.DisableCollisions = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "DisablePhysics":
                            settings.DisablePhysics = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "DisableScripts":
                            settings.DisableScripts = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "MaturityRating":
                            settings.MaturityRating = Int32.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "RestrictPushing":
                            settings.RestrictPushing = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "AgentLimit":
                            settings.AgentLimit = Int32.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "ObjectBonus":
                            settings.ObjectBonus = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                    }
                }

                xtr.ReadEndElement();
                xtr.ReadStartElement("GroundTextures");

                while (xtr.Read() && xtr.NodeType != XmlNodeType.EndElement)
                {
                    switch (xtr.Name)
                    {
                        case "Texture1":
                            settings.TerrainDetail0 = UUID.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "Texture2":
                            settings.TerrainDetail1 = UUID.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "Texture3":
                            settings.TerrainDetail2 = UUID.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "Texture4":
                            settings.TerrainDetail3 = UUID.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "ElevationLowSW":
                            settings.TerrainStartHeight00 = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationLowNW":
                            settings.TerrainStartHeight01 = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationLowSE":
                            settings.TerrainStartHeight10 = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationLowNE":
                            settings.TerrainStartHeight11 = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationHighSW":
                            settings.TerrainHeightRange00 = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationHighNW":
                            settings.TerrainHeightRange01 = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationHighSE":
                            settings.TerrainHeightRange10 = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "ElevationHighNE":
                            settings.TerrainHeightRange11 = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                    }
                }

                xtr.ReadEndElement();
                xtr.ReadStartElement("Terrain");

                while (xtr.Read() && xtr.NodeType != XmlNodeType.EndElement)
                {
                    switch (xtr.Name)
                    {
                        case "WaterHeight":
                            settings.WaterHeight = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "TerrainRaiseLimit":
                            settings.TerrainRaiseLimit = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "TerrainLowerLimit":
                            settings.TerrainLowerLimit = Single.Parse(xtr.ReadElementContentAsString(), nfi);
                            break;
                        case "UseEstateSun":
                            settings.UseEstateSun = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                        case "FixedSun":
                            settings.FixedSun = Boolean.Parse(xtr.ReadElementContentAsString());
                            break;
                    }
                }
            }

            return settings;
        }
    }
}
