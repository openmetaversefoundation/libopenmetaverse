using System;
using System.Collections.Generic;

namespace OpenMetaverse
{
    /// <summary>
    /// Operation to apply when applying color to texture
    /// </summary>
    public enum VisualColorOperation
    {
        Add,
        Blend,
        Multiply
    }

    /// <summary>
    /// Information needed to translate visual param value to RGBA color
    /// </summary>
    public struct VisualColorParam
    {
        public VisualColorOperation Operation;
        public Color4[] Colors;

        /// <summary>
        /// Construct VisualColorParam
        /// </summary>
        /// <param name="operation">Operation to apply when applying color to texture</param>
        /// <param name="colors">Colors</param>
        public VisualColorParam(VisualColorOperation operation, Color4[] colors)
        {
            Operation = operation;
            Colors = colors;
        }
    }

    /// <summary>
    /// Represents alpha blending and bump infor for a visual parameter
    /// such as sleive length
    /// </summary>
    public struct VisualAlphaParam
    {
        /// <summary>Stregth of the alpha to apply</summary>
        public float Domain;

        /// <summary>File containing the alpha channel</summary>
        public string TGAFile;

        /// <summary>Skip blending if parameter value is 0</summary>
        public bool SkipIfZero;

        /// <summary>Use miltiply insted of alpha blending</summary>
        public bool MultiplyBlend;

        /// <summary>
        /// Create new alhpa information for a visual param
        /// </summary>
        /// <param name="domain">Stregth of the alpha to apply</param>
        /// <param name="tgaFile">File containing the alpha channel</param>
        /// <param name="skipIfZero">Skip blending if parameter value is 0</param>
        /// <param name="multiplyBlend">Use miltiply insted of alpha blending</param>
        public VisualAlphaParam(float domain, string tgaFile, bool skipIfZero, bool multiplyBlend)
        {
            Domain = domain;
            TGAFile = tgaFile;
            SkipIfZero = skipIfZero;
            MultiplyBlend = multiplyBlend;
        }
    }
    /// <summary>
    /// A single visual characteristic of an avatar mesh, such as eyebrow height
    /// </summary>
    public struct VisualParam
    {
        /// <summary>Index of this visual param</summary>
        public int ParamID;
        /// <summary>Internal name</summary>
        public string Name;
        /// <summary>Group ID this parameter belongs to</summary>
        public int Group;
        /// <summary>Name of the wearable this parameter belongs to</summary>
        public string Wearable;
        /// <summary>Displayable label of this characteristic</summary>
        public string Label;
        /// <summary>Displayable label for the minimum value of this characteristic</summary>
        public string LabelMin;
        /// <summary>Displayable label for the maximum value of this characteristic</summary>
        public string LabelMax;
        /// <summary>Default value</summary>
        public float DefaultValue;
        /// <summary>Minimum value</summary>
        public float MinValue;
        /// <summary>Maximum value</summary>
        public float MaxValue;
        /// <summary>Is this param used for creation of bump layer?</summary>
        public bool IsBumpAttribute;
        /// <summary>Alpha blending/bump info</summary>
        public VisualAlphaParam? AlphaParams;
        /// <summary>Color information</summary>
        public VisualColorParam? ColorParams;
        /// <summary>Array of param IDs that are drivers for this parameter</summary>
        public int[] Drivers;
        /// <summary>
        /// Set all the values through the constructor
        /// </summary>
        /// <param name="paramID">Index of this visual param</param>
        /// <param name="name">Internal name</param>
        /// <param name="group"></param>
        /// <param name="wearable"></param>
        /// <param name="label">Displayable label of this characteristic</param>
        /// <param name="labelMin">Displayable label for the minimum value of this characteristic</param>
        /// <param name="labelMax">Displayable label for the maximum value of this characteristic</param>
        /// <param name="def">Default value</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="isBumpAttribute">Is this param used for creation of bump layer?</param>
        /// <param name="drivers">Array of param IDs that are drivers for this parameter</param>
        /// <param name="alpha">Alpha blending/bump info</param>
        /// <param name="colorParams">Color information</param>
        public VisualParam(int paramID, string name, int group, string wearable, string label, string labelMin, string labelMax, float def, float min, float max, bool isBumpAttribute, int[] drivers, VisualAlphaParam? alpha, VisualColorParam? colorParams)
        {
            ParamID = paramID;
            Name = name;
            Group = group;
            Wearable = wearable;
            Label = label;
            LabelMin = labelMin;
            LabelMax = labelMax;
            DefaultValue = def;
            MaxValue = max;
            MinValue = min;
            IsBumpAttribute = isBumpAttribute;
            Drivers = drivers;
            AlphaParams = alpha;
            ColorParams = colorParams;
        }
    }

    /// <summary>
    /// Holds the Params array of all the avatar appearance parameters
    /// </summary>
    public static class VisualParams
    {
        public static SortedList<int, VisualParam> Params = new SortedList<int, VisualParam>();

        public static VisualParam Find(string name, string wearable)
        {
            foreach (KeyValuePair<int, VisualParam> param in Params)
                if (param.Value.Name == name && param.Value.Wearable == wearable)
                    return param.Value;

            return new VisualParam();
        }

        static VisualParams()
        {
            Params[1] = new VisualParam(1, "Big_Brow", 0, "shape", "Brow Size", "Small", "Large", -0.3f, -0.3f, 2f, false, null, null, null);
            Params[2] = new VisualParam(2, "Nose_Big_Out", 0, "shape", "Nose Size", "Small", "Large", -0.8f, -0.8f, 2.5f, false, null, null, null);
            Params[4] = new VisualParam(4, "Broad_Nostrils", 0, "shape", "Nostril Width", "Narrow", "Broad", -0.5f, -0.5f, 1f, false, null, null, null);
            Params[5] = new VisualParam(5, "Cleft_Chin", 0, "shape", "Chin Cleft", "Round", "Cleft", -0.1f, -0.1f, 1f, false, null, null, null);
            Params[6] = new VisualParam(6, "Bulbous_Nose_Tip", 0, "shape", "Nose Tip Shape", "Pointy", "Bulbous", -0.3f, -0.3f, 1f, false, null, null, null);
            Params[7] = new VisualParam(7, "Weak_Chin", 0, "shape", "Chin Angle", "Chin Out", "Chin In", -0.5f, -0.5f, 0.5f, false, null, null, null);
            Params[8] = new VisualParam(8, "Double_Chin", 0, "shape", "Chin-Neck", "Tight Chin", "Double Chin", -0.5f, -0.5f, 1.5f, false, null, null, null);
            Params[10] = new VisualParam(10, "Sunken_Cheeks", 0, "shape", "Lower Cheeks", "Well-Fed", "Sunken", -1.5f, -1.5f, 3f, false, null, null, null);
            Params[11] = new VisualParam(11, "Noble_Nose_Bridge", 0, "shape", "Upper Bridge", "Low", "High", -0.5f, -0.5f, 1.5f, false, null, null, null);
            Params[12] = new VisualParam(12, "Jowls", 0, "shape", String.Empty, "Less", "More", -0.5f, -0.5f, 2.5f, false, null, null, null);
            Params[13] = new VisualParam(13, "Cleft_Chin_Upper", 0, "shape", "Upper Chin Cleft", "Round", "Cleft", 0f, 0f, 1.5f, false, null, null, null);
            Params[14] = new VisualParam(14, "High_Cheek_Bones", 0, "shape", "Cheek Bones", "Low", "High", -0.5f, -0.5f, 1f, false, null, null, null);
            Params[15] = new VisualParam(15, "Ears_Out", 0, "shape", "Ear Angle", "In", "Out", -0.5f, -0.5f, 1.5f, false, null, null, null);
            Params[16] = new VisualParam(16, "Pointy_Eyebrows", 0, "hair", "Eyebrow Points", "Smooth", "Pointy", -0.5f, -0.5f, 3f, false, new int[] { 870 }, null, null);
            Params[17] = new VisualParam(17, "Square_Jaw", 0, "shape", "Jaw Shape", "Pointy", "Square", -0.5f, -0.5f, 1f, false, null, null, null);
            Params[18] = new VisualParam(18, "Puffy_Upper_Cheeks", 0, "shape", "Upper Cheeks", "Thin", "Puffy", -1.5f, -1.5f, 2.5f, false, null, null, null);
            Params[19] = new VisualParam(19, "Upturned_Nose_Tip", 0, "shape", "Nose Tip Angle", "Downturned", "Upturned", -1.5f, -1.5f, 1f, false, null, null, null);
            Params[20] = new VisualParam(20, "Bulbous_Nose", 0, "shape", "Nose Thickness", "Thin Nose", "Bulbous Nose", -0.5f, -0.5f, 1.5f, false, null, null, null);
            Params[21] = new VisualParam(21, "Upper_Eyelid_Fold", 0, "shape", "Upper Eyelid Fold", "Uncreased", "Creased", -0.2f, -0.2f, 1.3f, false, null, null, null);
            Params[22] = new VisualParam(22, "Attached_Earlobes", 0, "shape", "Attached Earlobes", "Unattached", "Attached", 0f, 0f, 1f, false, null, null, null);
            Params[23] = new VisualParam(23, "Baggy_Eyes", 0, "shape", "Eye Bags", "Smooth", "Baggy", -0.5f, -0.5f, 1.5f, false, null, null, null);
            Params[24] = new VisualParam(24, "Wide_Eyes", 0, "shape", "Eye Opening", "Narrow", "Wide", -1.5f, -1.5f, 2f, false, null, null, null);
            Params[25] = new VisualParam(25, "Wide_Lip_Cleft", 0, "shape", "Lip Cleft", "Narrow", "Wide", -0.8f, -0.8f, 1.5f, false, null, null, null);
            Params[26] = new VisualParam(26, "Lips_Thin", 1, "shape", String.Empty, String.Empty, String.Empty, 0f, 0f, 0.7f, false, null, null, null);
            Params[27] = new VisualParam(27, "Wide_Nose_Bridge", 0, "shape", "Bridge Width", "Narrow", "Wide", -1.3f, -1.3f, 1.2f, false, null, null, null);
            Params[28] = new VisualParam(28, "Lips_Fat", 1, "shape", String.Empty, String.Empty, String.Empty, 0f, 0f, 2f, false, null, null, null);
            Params[29] = new VisualParam(29, "Wide_Upper_Lip", 1, "shape", String.Empty, String.Empty, String.Empty, -0.7f, -0.7f, 1.3f, false, null, null, null);
            Params[30] = new VisualParam(30, "Wide_Lower_Lip", 1, "shape", String.Empty, String.Empty, String.Empty, -0.7f, -0.7f, 1.3f, false, null, null, null);
            Params[31] = new VisualParam(31, "Arced_Eyebrows", 0, "hair", "Eyebrow Arc", "Flat", "Arced", 0.5f, 0f, 2f, false, new int[] { 872 }, null, null);
            Params[33] = new VisualParam(33, "Height", 0, "shape", "Height", "Short", "Tall", -2.3f, -2.3f, 2f, false, null, null, null);
            Params[34] = new VisualParam(34, "Thickness", 0, "shape", "Body Thickness", "Body Thin", "Body Thick", -0.7f, -0.7f, 1.5f, false, null, null, null);
            Params[35] = new VisualParam(35, "Big_Ears", 0, "shape", "Ear Size", "Small", "Large", -1f, -1f, 2f, false, null, null, null);
            Params[36] = new VisualParam(36, "Shoulders", 0, "shape", "Shoulders", "Narrow", "Broad", -0.5f, -1.8f, 1.4f, false, null, null, null);
            Params[37] = new VisualParam(37, "Hip Width", 0, "shape", "Hip Width", "Narrow", "Wide", -3.2f, -3.2f, 2.8f, false, null, null, null);
            Params[38] = new VisualParam(38, "Torso Length", 0, "shape", String.Empty, "Short Torso", "Long Torso", -1f, -1f, 1f, false, null, null, null);
            Params[80] = new VisualParam(80, "male", 0, "shape", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, new int[] { 32, 153, 40, 100, 857 }, null, null);
            Params[93] = new VisualParam(93, "Glove Length", 0, "gloves", String.Empty, "Short", "Long", 0.8f, 0.01f, 1f, false, new int[] { 1058, 1059 }, null, null);
            Params[98] = new VisualParam(98, "Eye Lightness", 0, "eyes", String.Empty, "Darker", "Lighter", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(0, 0, 0, 0), new Color4(255, 255, 255, 255) }));
            Params[99] = new VisualParam(99, "Eye Color", 0, "eyes", String.Empty, "Natural", "Unnatural", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 50, 25, 5), new Color4(255, 109, 55, 15), new Color4(255, 150, 93, 49), new Color4(255, 152, 118, 25), new Color4(255, 95, 179, 107), new Color4(255, 87, 192, 191), new Color4(255, 95, 172, 179), new Color4(255, 128, 128, 128), new Color4(255, 0, 0, 0), new Color4(255, 255, 255, 0), new Color4(255, 0, 255, 0), new Color4(255, 0, 255, 255), new Color4(255, 0, 0, 255), new Color4(255, 255, 0, 255), new Color4(255, 255, 0, 0) }));
            Params[105] = new VisualParam(105, "Breast Size", 0, "shape", String.Empty, "Small", "Large", 0.5f, 0f, 1f, false, new int[] { 843, 627, 626 }, null, null);
            Params[106] = new VisualParam(106, "Muscular_Torso", 1, "shape", "Torso Muscles", "Regular", "Muscular", 0f, 0f, 1.4f, false, null, null, null);
            Params[108] = new VisualParam(108, "Rainbow Color", 0, "skin", String.Empty, "None", "Wild", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 255), new Color4(255, 255, 0, 0), new Color4(255, 255, 255, 0), new Color4(255, 0, 255, 0), new Color4(255, 0, 255, 255), new Color4(255, 0, 0, 255), new Color4(255, 255, 0, 255) }));
            Params[110] = new VisualParam(110, "Red Skin", 0, "skin", "Ruddiness", "Pale", "Ruddy", 0f, 0f, 0.1f, false, null, null, new VisualColorParam(VisualColorOperation.Blend, new Color4[] { new Color4(255, 218, 41, 37) }));
            Params[111] = new VisualParam(111, "Pigment", 0, "skin", String.Empty, "Light", "Dark", 0.5f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 252, 215, 200), new Color4(255, 240, 177, 112), new Color4(255, 90, 40, 16), new Color4(255, 29, 9, 6) }));
            Params[112] = new VisualParam(112, "Rainbow Color", 0, "hair", String.Empty, "None", "Wild", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 255), new Color4(255, 255, 0, 0), new Color4(255, 255, 255, 0), new Color4(255, 0, 255, 0), new Color4(255, 0, 255, 255), new Color4(255, 0, 0, 255), new Color4(255, 255, 0, 255) }));
            Params[113] = new VisualParam(113, "Red Hair", 0, "hair", String.Empty, "No Red", "Very Red", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 118, 47, 19) }));
            Params[114] = new VisualParam(114, "Blonde Hair", 0, "hair", String.Empty, "Black", "Blonde", 0.5f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 22, 6, 6), new Color4(255, 29, 9, 6), new Color4(255, 45, 21, 11), new Color4(255, 78, 39, 11), new Color4(255, 90, 53, 16), new Color4(255, 136, 92, 21), new Color4(255, 150, 106, 33), new Color4(255, 198, 156, 74), new Color4(255, 233, 192, 103), new Color4(255, 238, 205, 136) }));
            Params[115] = new VisualParam(115, "White Hair", 0, "hair", String.Empty, "No White", "All White", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 255, 255) }));
            Params[116] = new VisualParam(116, "Rosy Complexion", 0, "skin", String.Empty, "Less Rosy", "More Rosy", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(0, 198, 71, 71), new Color4(255, 198, 71, 71) }));
            Params[117] = new VisualParam(117, "Lip Pinkness", 0, "skin", String.Empty, "Darker", "Pinker", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(0, 220, 115, 115), new Color4(128, 220, 115, 115) }));
            Params[119] = new VisualParam(119, "Eyebrow Size", 0, "hair", String.Empty, "Thin Eyebrows", "Bushy Eyebrows", 0.5f, 0f, 1f, false, new int[] { 1000, 1001 }, null, null);
            Params[130] = new VisualParam(130, "Front Fringe", 0, "hair", String.Empty, "Short", "Long", 0.45f, 0f, 1f, false, new int[] { 144, 145 }, null, null);
            Params[131] = new VisualParam(131, "Side Fringe", 0, "hair", String.Empty, "Short", "Long", 0.5f, 0f, 1f, false, new int[] { 146, 147 }, null, null);
            Params[132] = new VisualParam(132, "Back Fringe", 0, "hair", String.Empty, "Short", "Long", 0.39f, 0f, 1f, false, new int[] { 148, 149 }, null, null);
            Params[133] = new VisualParam(133, "Hair Front", 0, "hair", String.Empty, "Short", "Long", 0.25f, 0f, 1f, false, new int[] { 172, 171 }, null, null);
            Params[134] = new VisualParam(134, "Hair Sides", 0, "hair", String.Empty, "Short", "Long", 0.5f, 0f, 1f, false, new int[] { 174, 173 }, null, null);
            Params[135] = new VisualParam(135, "Hair Back", 0, "hair", String.Empty, "Short", "Long", 0.55f, 0f, 1f, false, new int[] { 176, 175 }, null, null);
            Params[136] = new VisualParam(136, "Hair Sweep", 0, "hair", String.Empty, "Sweep Forward", "Sweep Back", 0.5f, 0f, 1f, false, new int[] { 179, 178 }, null, null);
            Params[137] = new VisualParam(137, "Hair Tilt", 0, "hair", String.Empty, "Left", "Right", 0.5f, 0f, 1f, false, new int[] { 190, 191 }, null, null);
            Params[140] = new VisualParam(140, "Hair_Part_Middle", 0, "hair", "Middle Part", "No Part", "Part", 0f, 0f, 2f, false, null, null, null);
            Params[141] = new VisualParam(141, "Hair_Part_Right", 0, "hair", "Right Part", "No Part", "Part", 0f, 0f, 2f, false, null, null, null);
            Params[142] = new VisualParam(142, "Hair_Part_Left", 0, "hair", "Left Part", "No Part", "Part", 0f, 0f, 2f, false, null, null, null);
            Params[143] = new VisualParam(143, "Hair_Sides_Full", 0, "hair", "Full Hair Sides", "Mowhawk", "Full Sides", 0.125f, -4f, 1.5f, false, null, null, null);
            Params[144] = new VisualParam(144, "Bangs_Front_Up", 1, "hair", "Front Bangs Up", "Bangs", "Bangs Up", 0f, 0f, 1f, false, null, null, null);
            Params[145] = new VisualParam(145, "Bangs_Front_Down", 1, "hair", "Front Bangs Down", "Bangs", "Bangs Down", 0f, 0f, 5f, false, null, null, null);
            Params[146] = new VisualParam(146, "Bangs_Sides_Up", 1, "hair", "Side Bangs Up", "Side Bangs", "Side Bangs Up", 0f, 0f, 1f, false, null, null, null);
            Params[147] = new VisualParam(147, "Bangs_Sides_Down", 1, "hair", "Side Bangs Down", "Side Bangs", "Side Bangs Down", 0f, 0f, 2f, false, null, null, null);
            Params[148] = new VisualParam(148, "Bangs_Back_Up", 1, "hair", "Back Bangs Up", "Back Bangs", "Back Bangs Up", 0f, 0f, 1f, false, null, null, null);
            Params[149] = new VisualParam(149, "Bangs_Back_Down", 1, "hair", "Back Bangs Down", "Back Bangs", "Back Bangs Down", 0f, 0f, 2f, false, null, null, null);
            Params[150] = new VisualParam(150, "Body Definition", 0, "skin", String.Empty, "Less", "More", 0f, 0f, 1f, false, new int[] { 125, 126, 160, 161, 874, 878 }, null, null);
            Params[151] = new VisualParam(151, "Big_Butt_Legs", 1, "shape", "Butt Size", "Regular", "Large", 0f, 0f, 1f, false, null, null, null);
            Params[152] = new VisualParam(152, "Muscular_Legs", 1, "shape", "Leg Muscles", "Regular Muscles", "More Muscles", 0f, 0f, 1.5f, false, null, null, null);
            Params[155] = new VisualParam(155, "Lip Width", 0, "shape", "Lip Width", "Narrow Lips", "Wide Lips", 0f, -0.9f, 1.3f, false, new int[] { 29, 30 }, null, null);
            Params[157] = new VisualParam(157, "Belly Size", 0, "shape", String.Empty, "Small", "Big", 0f, 0f, 1f, false, new int[] { 104, 156, 849 }, null, null);
            Params[162] = new VisualParam(162, "Facial Definition", 0, "skin", String.Empty, "Less", "More", 0f, 0f, 1f, false, new int[] { 158, 159, 873 }, null, null);
            Params[163] = new VisualParam(163, "wrinkles", 0, "skin", String.Empty, "Less", "More", 0f, 0f, 1f, false, new int[] { 118 }, null, null);
            Params[165] = new VisualParam(165, "Freckles", 0, "skin", String.Empty, "Less", "More", 0f, 0f, 1f, false, null, new VisualAlphaParam(0.5f, "freckles_alpha.tga", true, false), null);
            Params[166] = new VisualParam(166, "Sideburns", 0, "hair", String.Empty, "Short Sideburns", "Mutton Chops", 0f, 0f, 1f, false, new int[] { 1004, 1005 }, null, null);
            Params[167] = new VisualParam(167, "Moustache", 0, "hair", String.Empty, "Chaplin", "Handlebars", 0f, 0f, 1f, false, new int[] { 1006, 1007 }, null, null);
            Params[168] = new VisualParam(168, "Soulpatch", 0, "hair", String.Empty, "Less soul", "More soul", 0f, 0f, 1f, false, new int[] { 1008, 1009 }, null, null);
            Params[169] = new VisualParam(169, "Chin Curtains", 0, "hair", String.Empty, "Less Curtains", "More Curtains", 0f, 0f, 1f, false, new int[] { 1010, 1011 }, null, null);
            Params[171] = new VisualParam(171, "Hair_Front_Down", 1, "hair", "Front Hair Down", "Front Hair", "Front Hair Down", 0f, 0f, 1f, false, null, null, null);
            Params[172] = new VisualParam(172, "Hair_Front_Up", 1, "hair", "Front Hair Up", "Front Hair", "Front Hair Up", 0f, 0f, 1f, false, null, null, null);
            Params[173] = new VisualParam(173, "Hair_Sides_Down", 1, "hair", "Sides Hair Down", "Sides Hair", "Sides Hair Down", 0f, 0f, 1f, false, null, null, null);
            Params[174] = new VisualParam(174, "Hair_Sides_Up", 1, "hair", "Sides Hair Up", "Sides Hair", "Sides Hair Up", 0f, 0f, 1f, false, null, null, null);
            Params[175] = new VisualParam(175, "Hair_Back_Down", 1, "hair", "Back Hair Down", "Back Hair", "Back Hair Down", 0f, 0f, 3f, false, null, null, null);
            Params[176] = new VisualParam(176, "Hair_Back_Up", 1, "hair", "Back Hair Up", "Back Hair", "Back Hair Up", 0f, 0f, 1f, false, null, null, null);
            Params[177] = new VisualParam(177, "Hair_Rumpled", 0, "hair", "Rumpled Hair", "Smooth Hair", "Rumpled Hair", 0f, 0f, 1f, false, null, null, null);
            Params[178] = new VisualParam(178, "Hair_Swept_Back", 1, "hair", "Swept Back Hair", "NotHair", "Swept Back", 0f, 0f, 1f, false, null, null, null);
            Params[179] = new VisualParam(179, "Hair_Swept_Forward", 1, "hair", "Swept Forward Hair", "Hair", "Swept Forward", 0f, 0f, 1f, false, null, null, null);
            Params[180] = new VisualParam(180, "Hair_Volume", 1, "hair", "Hair Volume", "Less", "More", 0f, 0f, 1.3f, false, null, null, null);
            Params[181] = new VisualParam(181, "Hair_Big_Front", 0, "hair", "Big Hair Front", "Less", "More", 0.14f, -1f, 1f, false, null, null, null);
            Params[182] = new VisualParam(182, "Hair_Big_Top", 0, "hair", "Big Hair Top", "Less", "More", 0.7f, -1f, 1f, false, null, null, null);
            Params[183] = new VisualParam(183, "Hair_Big_Back", 0, "hair", "Big Hair Back", "Less", "More", 0.05f, -1f, 1f, false, null, null, null);
            Params[184] = new VisualParam(184, "Hair_Spiked", 0, "hair", "Spiked Hair", "No Spikes", "Big Spikes", 0f, 0f, 1f, false, null, null, null);
            Params[185] = new VisualParam(185, "Deep_Chin", 0, "shape", "Chin Depth", "Shallow", "Deep", -1f, -1f, 1f, false, null, null, null);
            Params[186] = new VisualParam(186, "Egg_Head", 1, "shape", "Egg Head", "Chin Heavy", "Forehead Heavy", -1.3f, -1.3f, 1f, false, null, null, null);
            Params[187] = new VisualParam(187, "Squash_Stretch_Head", 1, "shape", "Squash/Stretch Head", "Squash Head", "Stretch Head", -0.5f, -0.5f, 1f, false, null, null, null);
            Params[190] = new VisualParam(190, "Hair_Tilt_Right", 1, "hair", "Hair Tilted Right", "Hair", "Tilt Right", 0f, 0f, 1f, false, null, null, null);
            Params[191] = new VisualParam(191, "Hair_Tilt_Left", 1, "hair", "Hair Tilted Left", "Hair", "Tilt Left", 0f, 0f, 1f, false, null, null, null);
            Params[192] = new VisualParam(192, "Bangs_Part_Middle", 0, "hair", "Part Bangs", "No Part", "Part Bangs", 0f, 0f, 1f, false, null, null, null);
            Params[193] = new VisualParam(193, "Head Shape", 0, "shape", "Head Shape", "More Square", "More Round", 0.5f, 0f, 1f, false, new int[] { 188, 642, 189, 643 }, null, null);
            Params[194] = new VisualParam(194, "Eye_Spread", 1, "shape", String.Empty, "Eyes Together", "Eyes Spread", -2f, -2f, 2f, false, null, null, null);
            Params[195] = new VisualParam(195, "EyeBone_Spread", 1, "shape", String.Empty, "Eyes Together", "Eyes Spread", -1f, -1f, 1f, false, null, null, null);
            Params[196] = new VisualParam(196, "Eye Spacing", 0, "shape", "Eye Spacing", "Close Set Eyes", "Far Set Eyes", 0f, -2f, 1f, false, new int[] { 194, 195 }, null, null);
            Params[197] = new VisualParam(197, "Shoe_Heels", 1, "shoes", String.Empty, "No Heels", "High Heels", 0f, 0f, 1f, false, null, null, null);
            Params[198] = new VisualParam(198, "Heel Height", 0, "shoes", String.Empty, "Low Heels", "High Heels", 0f, 0f, 1f, false, new int[] { 197, 500 }, null, null);
            Params[400] = new VisualParam(400, "Displace_Hair_Facial", 1, "hair", "Hair Thickess", "Cropped Hair", "Bushy Hair", 0f, 0f, 2f, false, null, null, null);
            Params[500] = new VisualParam(500, "Shoe_Heel_Height", 1, "shoes", "Heel Height", "Low Heels", "High Heels", 0f, 0f, 1f, false, null, null, null);
            Params[501] = new VisualParam(501, "Shoe_Platform_Height", 1, "shoes", "Platform Height", "Low Platforms", "High Platforms", 0f, 0f, 1f, false, null, null, null);
            Params[502] = new VisualParam(502, "Shoe_Platform", 1, "shoes", String.Empty, "No Heels", "High Heels", 0f, 0f, 1f, false, null, null, null);
            Params[503] = new VisualParam(503, "Platform Height", 0, "shoes", String.Empty, "Low Platforms", "High Platforms", 0f, 0f, 1f, false, new int[] { 501, 502 }, null, null);
            Params[505] = new VisualParam(505, "Lip Thickness", 0, "shape", String.Empty, "Thin Lips", "Fat Lips", 0.5f, 0f, 1f, false, new int[] { 26, 28 }, null, null);
            Params[506] = new VisualParam(506, "Mouth_Height", 0, "shape", "Mouth Position", "High", "Low", -2f, -2f, 2f, false, null, null, null);
            Params[507] = new VisualParam(507, "Breast_Gravity", 0, "shape", "Breast Buoyancy", "Less Gravity", "More Gravity", 0f, -1.5f, 2f, false, null, null, null);
            Params[508] = new VisualParam(508, "Shoe_Platform_Width", 0, "shoes", "Platform Width", "Narrow", "Wide", -1f, -1f, 2f, false, null, null, null);
            Params[509] = new VisualParam(509, "Shoe_Heel_Point", 1, "shoes", "Heel Shape", "Default Heels", "Pointy Heels", 0f, 0f, 1f, false, null, null, null);
            Params[510] = new VisualParam(510, "Shoe_Heel_Thick", 1, "shoes", "Heel Shape", "default Heels", "Thick Heels", 0f, 0f, 1f, false, null, null, null);
            Params[511] = new VisualParam(511, "Shoe_Toe_Point", 1, "shoes", "Toe Shape", "Default Toe", "Pointy Toe", 0f, 0f, 1f, false, null, null, null);
            Params[512] = new VisualParam(512, "Shoe_Toe_Square", 1, "shoes", "Toe Shape", "Default Toe", "Square Toe", 0f, 0f, 1f, false, null, null, null);
            Params[513] = new VisualParam(513, "Heel Shape", 0, "shoes", String.Empty, "Pointy Heels", "Thick Heels", 0.5f, 0f, 1f, false, new int[] { 509, 510 }, null, null);
            Params[514] = new VisualParam(514, "Toe Shape", 0, "shoes", String.Empty, "Pointy", "Square", 0.5f, 0f, 1f, false, new int[] { 511, 512 }, null, null);
            Params[515] = new VisualParam(515, "Foot_Size", 0, "shape", "Foot Size", "Small", "Big", -1f, -1f, 3f, false, null, null, null);
            Params[516] = new VisualParam(516, "Displace_Loose_Lowerbody", 1, "pants", "Pants Fit", String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[517] = new VisualParam(517, "Wide_Nose", 0, "shape", "Nose Width", "Narrow", "Wide", -0.5f, -0.5f, 1f, false, null, null, null);
            Params[518] = new VisualParam(518, "Eyelashes_Long", 0, "shape", "Eyelash Length", "Short", "Long", -0.3f, -0.3f, 1.5f, false, null, null, null);
            Params[600] = new VisualParam(600, "Sleeve Length Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.7f, 0f, 0.85f, false, null, new VisualAlphaParam(0.01f, "shirt_sleeve_alpha.tga", false, false), null);
            Params[601] = new VisualParam(601, "Shirt Bottom Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "shirt_bottom_alpha.tga", false, true), null);
            Params[602] = new VisualParam(602, "Collar Front Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "shirt_collar_alpha.tga", false, true), null);
            Params[603] = new VisualParam(603, "Sleeve Length", 0, "undershirt", String.Empty, "Short", "Long", 0.4f, 0.01f, 1f, false, new int[] { 1042, 1043 }, null, null);
            Params[604] = new VisualParam(604, "Bottom", 0, "undershirt", String.Empty, "Short", "Long", 0.85f, 0f, 1f, false, new int[] { 1044, 1045 }, null, null);
            Params[605] = new VisualParam(605, "Collar Front", 0, "undershirt", String.Empty, "Low", "High", 0.84f, 0f, 1f, false, new int[] { 1046, 1047 }, null, null);
            Params[606] = new VisualParam(606, "Sleeve Length", 0, "jacket", String.Empty, "Short", "Long", 0.8f, 0f, 1f, false, new int[] { 1019, 1039, 1020 }, null, null);
            Params[607] = new VisualParam(607, "Collar Front", 0, "jacket", String.Empty, "Low", "High", 0.8f, 0f, 1f, false, new int[] { 1021, 1040, 1022 }, null, null);
            Params[608] = new VisualParam(608, "bottom length lower", 0, "jacket", "Jacket Length", "Short", "Long", 0.8f, 0f, 1f, false, new int[] { 620, 1025, 1037, 621, 1027, 1033 }, null, null);
            Params[609] = new VisualParam(609, "open jacket", 0, "jacket", "Open Front", "Open", "Closed", 0.2f, 0f, 1f, false, new int[] { 622, 1026, 1038, 623, 1028, 1034 }, null, null);
            Params[614] = new VisualParam(614, "Waist Height Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "pants_waist_alpha.tga", false, false), null);
            Params[615] = new VisualParam(615, "Pants Length Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.01f, "pants_length_alpha.tga", false, false), null);
            Params[616] = new VisualParam(616, "Shoe Height", 0, "shoes", String.Empty, "Short", "Tall", 0.1f, 0f, 1f, false, new int[] { 1052, 1053 }, null, null);
            Params[617] = new VisualParam(617, "Socks Length", 0, "socks", String.Empty, "Short", "Long", 0.35f, 0f, 1f, false, new int[] { 1050, 1051 }, null, null);
            Params[619] = new VisualParam(619, "Pants Length", 0, "underpants", String.Empty, "Short", "Long", 0.3f, 0f, 1f, false, new int[] { 1054, 1055 }, null, null);
            Params[620] = new VisualParam(620, "bottom length upper", 1, "jacket", String.Empty, "hi cut", "low cut", 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.01f, "jacket_length_upper_alpha.tga", false, true), null);
            Params[621] = new VisualParam(621, "bottom length lower", 1, "jacket", String.Empty, "hi cut", "low cut", 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.01f, "jacket_length_lower_alpha.tga", false, false), null);
            Params[622] = new VisualParam(622, "open upper", 1, "jacket", String.Empty, "closed", "open", 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.01f, "jacket_open_upper_alpha.tga", false, true), null);
            Params[623] = new VisualParam(623, "open lower", 1, "jacket", String.Empty, "open", "closed", 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.01f, "jacket_open_lower_alpha.tga", false, true), null);
            Params[624] = new VisualParam(624, "Pants Waist", 0, "underpants", String.Empty, "Low", "High", 0.8f, 0f, 1f, false, new int[] { 1056, 1057 }, null, null);
            Params[625] = new VisualParam(625, "Leg_Pantflair", 0, "pants", "Cuff Flare", "Tight Cuffs", "Flared Cuffs", 0f, 0f, 1.5f, false, null, null, null);
            Params[626] = new VisualParam(626, "Big_Chest", 1, "shape", "Chest Size", "Small", "Large", 0f, 0f, 1f, false, null, null, null);
            Params[627] = new VisualParam(627, "Small_Chest", 1, "shape", "Chest Size", "Large", "Small", 0f, 0f, 1f, false, null, null, null);
            Params[628] = new VisualParam(628, "Displace_Loose_Upperbody", 1, "shirt", "Shirt Fit", String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[629] = new VisualParam(629, "Forehead Angle", 0, "shape", String.Empty, "More Vertical", "More Sloped", 0.5f, 0f, 1f, false, new int[] { 630, 644, 631, 645 }, null, null);
            Params[633] = new VisualParam(633, "Fat_Head", 1, "shape", "Fat Head", "Skinny", "Fat", 0f, 0f, 1f, false, null, null, null);
            Params[634] = new VisualParam(634, "Fat_Torso", 1, "shape", "Fat Torso", "skinny", "fat", 0f, 0f, 1f, false, null, null, null);
            Params[635] = new VisualParam(635, "Fat_Legs", 1, "shape", "Fat Torso", "skinny", "fat", 0f, 0f, 1f, false, null, null, null);
            Params[637] = new VisualParam(637, "Body Fat", 0, "shape", String.Empty, "Less Body Fat", "More Body Fat", 0f, 0f, 1f, false, new int[] { 633, 634, 635, 851 }, null, null);
            Params[638] = new VisualParam(638, "Low_Crotch", 0, "pants", "Pants Crotch", "High and Tight", "Low and Loose", 0f, 0f, 1.3f, false, null, null, null);
            Params[640] = new VisualParam(640, "Hair_Egg_Head", 1, "hair", String.Empty, String.Empty, String.Empty, -1.3f, -1.3f, 1f, false, null, null, null);
            Params[641] = new VisualParam(641, "Hair_Squash_Stretch_Head", 1, "hair", String.Empty, String.Empty, String.Empty, -0.5f, -0.5f, 1f, false, null, null, null);
            Params[642] = new VisualParam(642, "Hair_Square_Head", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[643] = new VisualParam(643, "Hair_Round_Head", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[644] = new VisualParam(644, "Hair_Forehead_Round", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[645] = new VisualParam(645, "Hair_Forehead_Slant", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[646] = new VisualParam(646, "Egg_Head", 0, "shape", "Egg Head", "Chin Heavy", "Forehead Heavy", 0f, -1.3f, 1f, false, new int[] { 640, 186 }, null, null);
            Params[647] = new VisualParam(647, "Squash_Stretch_Head", 0, "shape", "Head Stretch", "Squash Head", "Stretch Head", 0f, -0.5f, 1f, false, new int[] { 641, 187 }, null, null);
            Params[648] = new VisualParam(648, "Scrawny_Torso", 1, "shape", "Torso Muscles", "Regular", "Scrawny", 0f, 0f, 1.3f, false, null, null, null);
            Params[649] = new VisualParam(649, "Torso Muscles", 0, "shape", "Torso Muscles", "Less Muscular", "More Muscular", 0.5f, 0f, 1f, false, new int[] { 648, 106 }, null, null);
            Params[650] = new VisualParam(650, "Eyelid_Corner_Up", 0, "shape", "Outer Eye Corner", "Corner Down", "Corner Up", -1.3f, -1.3f, 1.2f, false, null, null, null);
            Params[651] = new VisualParam(651, "Scrawny_Legs", 1, "shape", "Scrawny Leg", "Regular Muscles", "Less Muscles", 0f, 0f, 1.5f, false, null, null, null);
            Params[652] = new VisualParam(652, "Leg Muscles", 0, "shape", String.Empty, "Less Muscular", "More Muscular", 0.5f, 0f, 1f, false, new int[] { 651, 152 }, null, null);
            Params[653] = new VisualParam(653, "Tall_Lips", 0, "shape", "Lip Fullness", "Less Full", "More Full", -1f, -1f, 2f, false, null, null, null);
            Params[654] = new VisualParam(654, "Shoe_Toe_Thick", 0, "shoes", "Toe Thickness", "Flat Toe", "Thick Toe", 0f, 0f, 2f, false, null, null, null);
            Params[655] = new VisualParam(655, "Head Size", 1, "shape", "Head Size", "Small Head", "Big Head", -0.25f, -0.25f, 0.1f, false, null, null, null);
            Params[656] = new VisualParam(656, "Crooked_Nose", 0, "shape", "Crooked Nose", "Nose Left", "Nose Right", -2f, -2f, 2f, false, null, null, null);
            Params[657] = new VisualParam(657, "Smile_Mouth", 1, "shape", "Mouth Corner", "Corner Normal", "Corner Up", 0f, 0f, 1.4f, false, null, null, null);
            Params[658] = new VisualParam(658, "Frown_Mouth", 1, "shape", "Mouth Corner", "Corner Normal", "Corner Down", 0f, 0f, 1.2f, false, null, null, null);
            Params[659] = new VisualParam(659, "Mouth Corner", 0, "shape", String.Empty, "Corner Down", "Corner Up", 0.5f, 0f, 1f, false, new int[] { 658, 657 }, null, null);
            Params[660] = new VisualParam(660, "Shear_Head", 1, "shape", "Shear Face", "Shear Left", "Shear Right", 0f, -2f, 2f, false, null, null, null);
            Params[661] = new VisualParam(661, "EyeBone_Head_Shear", 1, "shape", String.Empty, "Eyes Shear Left Up", "Eyes Shear Right Up", -2f, -2f, 2f, false, null, null, null);
            Params[662] = new VisualParam(662, "Face Shear", 0, "shape", String.Empty, "Shear Right Up", "Shear Left Up", 0.5f, 0f, 1f, false, new int[] { 660, 661, 774 }, null, null);
            Params[663] = new VisualParam(663, "Shift_Mouth", 0, "shape", "Shift Mouth", "Shift Left", "Shift Right", 0f, -2f, 2f, false, null, null, null);
            Params[664] = new VisualParam(664, "Pop_Eye", 0, "shape", "Eye Pop", "Pop Right Eye", "Pop Left Eye", 0f, -1.3f, 1.3f, false, null, null, null);
            Params[665] = new VisualParam(665, "Jaw_Jut", 0, "shape", "Jaw Jut", "Overbite", "Underbite", 0f, -2f, 2f, false, null, null, null);
            Params[674] = new VisualParam(674, "Hair_Shear_Back", 0, "hair", "Shear Back", "Full Back", "Sheared Back", -0.3f, -1f, 2f, false, null, null, null);
            Params[675] = new VisualParam(675, "Hand Size", 0, "shape", String.Empty, "Small Hands", "Large Hands", -0.3f, -0.3f, 0.3f, false, null, null, null);
            Params[676] = new VisualParam(676, "Love_Handles", 0, "shape", "Love Handles", "Less Love", "More Love", 0f, -1f, 2f, false, new int[] { 855, 856 }, null, null);
            Params[677] = new VisualParam(677, "Scrawny_Torso_Male", 1, "shape", "Torso Scrawny", "Regular", "Scrawny", 0f, 0f, 1.3f, false, null, null, null);
            Params[678] = new VisualParam(678, "Torso Muscles", 0, "shape", String.Empty, "Less Muscular", "More Muscular", 0.5f, 0f, 1f, false, new int[] { 677, 106 }, null, null);
            Params[679] = new VisualParam(679, "Eyeball_Size", 1, "shape", "Eyeball Size", "small eye", "big eye", -0.25f, -0.25f, 0.1f, false, null, null, null);
            Params[681] = new VisualParam(681, "Eyeball_Size", 1, "shape", "Eyeball Size", "small eye", "big eye", -0.25f, -0.25f, 0.1f, false, null, null, null);
            Params[682] = new VisualParam(682, "Head Size", 0, "shape", "Head Size", "Small Head", "Big Head", 0.5f, 0f, 1f, false, new int[] { 679, 694, 680, 681, 655 }, null, null);
            Params[683] = new VisualParam(683, "Neck Thickness", 0, "shape", String.Empty, "Skinny Neck", "Thick Neck", -0.15f, -0.4f, 0.2f, false, null, null, null);
            Params[684] = new VisualParam(684, "Breast_Female_Cleavage", 0, "shape", "Breast Cleavage", "Separate", "Join", 0f, -0.3f, 1.3f, false, null, null, null);
            Params[685] = new VisualParam(685, "Chest_Male_No_Pecs", 0, "shape", "Pectorals", "Big Pectorals", "Sunken Chest", 0f, -0.5f, 1.1f, false, null, null, null);
            Params[686] = new VisualParam(686, "Head_Eyes_Big", 1, "shape", "Eye Size", "Beady Eyes", "Anime Eyes", 0f, -2f, 2f, false, null, null, null);
            Params[687] = new VisualParam(687, "Eyeball_Size", 1, "shape", "Big Eyeball", "small eye", "big eye", -0.25f, -0.25f, 0.25f, false, null, null, null);
            Params[689] = new VisualParam(689, "EyeBone_Big_Eyes", 1, "shape", String.Empty, "Eyes Back", "Eyes Forward", -1f, -1f, 1f, false, null, null, null);
            Params[690] = new VisualParam(690, "Eye Size", 0, "shape", "Eye Size", "Beady Eyes", "Anime Eyes", 0.5f, 0f, 1f, false, new int[] { 686, 687, 695, 688, 691, 689 }, null, null);
            Params[691] = new VisualParam(691, "Eyeball_Size", 1, "shape", "Big Eyeball", "small eye", "big eye", -0.25f, -0.25f, 0.25f, false, null, null, null);
            Params[692] = new VisualParam(692, "Leg Length", 0, "shape", String.Empty, "Short Legs", "Long Legs", -1f, -1f, 1f, false, null, null, null);
            Params[693] = new VisualParam(693, "Arm Length", 0, "shape", String.Empty, "Short Arms", "Long arms", 0.6f, -1f, 1f, false, null, null, null);
            Params[694] = new VisualParam(694, "Eyeball_Size", 1, "shape", "Eyeball Size", "small eye", "big eye", -0.25f, -0.25f, 0.1f, false, null, null, null);
            Params[695] = new VisualParam(695, "Eyeball_Size", 1, "shape", "Big Eyeball", "small eye", "big eye", -0.25f, -0.25f, 0.25f, false, null, null, null);
            Params[700] = new VisualParam(700, "Lipstick Color", 0, "skin", String.Empty, "Pink", "Black", 0.25f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(200, 245, 161, 177), new Color4(200, 216, 37, 67), new Color4(200, 178, 48, 76), new Color4(200, 68, 0, 11), new Color4(200, 252, 207, 184), new Color4(200, 241, 136, 106), new Color4(200, 208, 110, 85), new Color4(200, 106, 28, 18), new Color4(200, 58, 26, 49), new Color4(200, 14, 14, 14) }));
            Params[701] = new VisualParam(701, "Lipstick", 0, "skin", String.Empty, "No Lipstick", "More Lipstick", 0f, 0f, 0.9f, false, null, new VisualAlphaParam(0.05f, "lipstick_alpha.tga", true, false), null);
            Params[702] = new VisualParam(702, "Lipgloss", 0, "skin", String.Empty, "No Lipgloss", "Glossy", 0f, 0f, 1f, false, null, new VisualAlphaParam(0.2f, "lipgloss_alpha.tga", true, false), null);
            Params[703] = new VisualParam(703, "Eyeliner", 0, "skin", String.Empty, "No Eyeliner", "Full Eyeliner", 0f, 0f, 1f, false, null, new VisualAlphaParam(0.1f, "eyeliner_alpha.tga", true, false), null);
            Params[704] = new VisualParam(704, "Blush", 0, "skin", String.Empty, "No Blush", "More Blush", 0f, 0f, 0.9f, false, null, new VisualAlphaParam(0.3f, "blush_alpha.tga", true, false), null);
            Params[705] = new VisualParam(705, "Blush Color", 0, "skin", String.Empty, "Pink", "Orange", 0.5f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(200, 253, 162, 193), new Color4(200, 247, 131, 152), new Color4(200, 213, 122, 140), new Color4(200, 253, 152, 144), new Color4(200, 236, 138, 103), new Color4(200, 195, 128, 122), new Color4(200, 148, 103, 100), new Color4(200, 168, 95, 62) }));
            Params[706] = new VisualParam(706, "Out Shdw Opacity", 0, "skin", String.Empty, "Clear", "Opaque", 0.6f, 0.2f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Blend, new Color4[] { new Color4(0, 255, 255, 255), new Color4(255, 255, 255, 255) }));
            Params[707] = new VisualParam(707, "Outer Shadow", 0, "skin", String.Empty, "No Eyeshadow", "More Eyeshadow", 0f, 0f, 0.7f, false, null, new VisualAlphaParam(0.05f, "eyeshadow_outer_alpha.tga", true, false), null);
            Params[708] = new VisualParam(708, "Out Shdw Color", 0, "skin", String.Empty, "Light", "Dark", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 252, 247, 246), new Color4(255, 255, 206, 206), new Color4(255, 233, 135, 149), new Color4(255, 220, 168, 192), new Color4(255, 228, 203, 232), new Color4(255, 255, 234, 195), new Color4(255, 230, 157, 101), new Color4(255, 255, 147, 86), new Color4(255, 228, 110, 89), new Color4(255, 228, 150, 120), new Color4(255, 223, 227, 213), new Color4(255, 96, 116, 87), new Color4(255, 88, 143, 107), new Color4(255, 194, 231, 223), new Color4(255, 207, 227, 234), new Color4(255, 41, 171, 212), new Color4(255, 180, 137, 130), new Color4(255, 173, 125, 105), new Color4(255, 144, 95, 98), new Color4(255, 115, 70, 77), new Color4(255, 155, 78, 47), new Color4(255, 239, 239, 239), new Color4(255, 194, 194, 194), new Color4(255, 120, 120, 120), new Color4(255, 10, 10, 10) }));
            Params[709] = new VisualParam(709, "Inner Shadow", 0, "skin", String.Empty, "No Eyeshadow", "More Eyeshadow", 0f, 0f, 1f, false, null, new VisualAlphaParam(0.2f, "eyeshadow_inner_alpha.tga", true, false), null);
            Params[710] = new VisualParam(710, "Nail Polish", 0, "skin", String.Empty, "No Polish", "Painted Nails", 0f, 0f, 1f, false, null, new VisualAlphaParam(0.1f, "nailpolish_alpha.tga", true, false), null);
            Params[711] = new VisualParam(711, "Blush Opacity", 0, "skin", String.Empty, "Clear", "Opaque", 0.5f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Blend, new Color4[] { new Color4(0, 255, 255, 255), new Color4(255, 255, 255, 255) }));
            Params[712] = new VisualParam(712, "In Shdw Color", 0, "skin", String.Empty, "Light", "Dark", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 252, 247, 246), new Color4(255, 255, 206, 206), new Color4(255, 233, 135, 149), new Color4(255, 220, 168, 192), new Color4(255, 228, 203, 232), new Color4(255, 255, 234, 195), new Color4(255, 230, 157, 101), new Color4(255, 255, 147, 86), new Color4(255, 228, 110, 89), new Color4(255, 228, 150, 120), new Color4(255, 223, 227, 213), new Color4(255, 96, 116, 87), new Color4(255, 88, 143, 107), new Color4(255, 194, 231, 223), new Color4(255, 207, 227, 234), new Color4(255, 41, 171, 212), new Color4(255, 180, 137, 130), new Color4(255, 173, 125, 105), new Color4(255, 144, 95, 98), new Color4(255, 115, 70, 77), new Color4(255, 155, 78, 47), new Color4(255, 239, 239, 239), new Color4(255, 194, 194, 194), new Color4(255, 120, 120, 120), new Color4(255, 10, 10, 10) }));
            Params[713] = new VisualParam(713, "In Shdw Opacity", 0, "skin", String.Empty, "Clear", "Opaque", 0.7f, 0.2f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Blend, new Color4[] { new Color4(0, 255, 255, 255), new Color4(255, 255, 255, 255) }));
            Params[714] = new VisualParam(714, "Eyeliner Color", 0, "skin", String.Empty, "Dark Green", "Black", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(250, 24, 98, 40), new Color4(250, 9, 100, 127), new Color4(250, 61, 93, 134), new Color4(250, 70, 29, 27), new Color4(250, 115, 75, 65), new Color4(250, 100, 100, 100), new Color4(250, 91, 80, 74), new Color4(250, 112, 42, 76), new Color4(250, 14, 14, 14) }));
            Params[715] = new VisualParam(715, "Nail Polish Color", 0, "skin", String.Empty, "Pink", "Black", 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 255, 187, 200), new Color4(255, 194, 102, 127), new Color4(255, 227, 34, 99), new Color4(255, 168, 41, 60), new Color4(255, 97, 28, 59), new Color4(255, 234, 115, 93), new Color4(255, 142, 58, 47), new Color4(255, 114, 30, 46), new Color4(255, 14, 14, 14) }));
            Params[750] = new VisualParam(750, "Eyebrow Density", 0, "hair", String.Empty, "Sparse", "Dense", 0.7f, 0f, 1f, false, new int[] { 1002, 1003 }, null, null);
            Params[751] = new VisualParam(751, "5 O'Clock Shadow", 1, "hair", String.Empty, "Dense hair", "Shadow hair", 0.7f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Blend, new Color4[] { new Color4(255, 255, 255, 255), new Color4(30, 255, 255, 255) }));
            Params[752] = new VisualParam(752, "Hair Thickness", 0, "hair", String.Empty, "5 O'Clock Shadow", "Bushy Hair", 0.5f, 0f, 1f, false, new int[] { 751, 1012, 400 }, null, null);
            Params[753] = new VisualParam(753, "Saddlebags", 0, "shape", "Saddle Bags", "Less Saddle", "More Saddle", 0f, -0.5f, 3f, false, new int[] { 850, 854 }, null, null);
            Params[754] = new VisualParam(754, "Hair_Taper_Back", 0, "hair", "Taper Back", "Wide Back", "Narrow Back", 0f, -1f, 2f, false, null, null, null);
            Params[755] = new VisualParam(755, "Hair_Taper_Front", 0, "hair", "Taper Front", "Wide Front", "Narrow Front", 0.05f, -1.5f, 1.5f, false, null, null, null);
            Params[756] = new VisualParam(756, "Neck Length", 0, "shape", String.Empty, "Short Neck", "Long Neck", 0f, -1f, 1f, false, null, null, null);
            Params[757] = new VisualParam(757, "Lower_Eyebrows", 0, "hair", "Eyebrow Height", "Higher", "Lower", -1f, -4f, 2f, false, new int[] { 871 }, null, null);
            Params[758] = new VisualParam(758, "Lower_Bridge_Nose", 0, "shape", "Lower Bridge", "Low", "High", -1.5f, -1.5f, 1.5f, false, null, null, null);
            Params[759] = new VisualParam(759, "Low_Septum_Nose", 0, "shape", "Nostril Division", "High", "Low", 0.5f, -1f, 1.5f, false, null, null, null);
            Params[760] = new VisualParam(760, "Jaw_Angle", 0, "shape", "Jaw Angle", "Low Jaw", "High Jaw", 0f, -1.2f, 2f, false, null, null, null);
            Params[761] = new VisualParam(761, "Hair_Volume_Small", 1, "hair", "Hair Volume", "Less", "More", 0f, 0f, 1.3f, false, null, null, null);
            Params[762] = new VisualParam(762, "Hair_Shear_Front", 0, "hair", "Shear Front", "Full Front", "Sheared Front", 0f, 0f, 3f, false, null, null, null);
            Params[763] = new VisualParam(763, "Hair Volume", 0, "hair", String.Empty, "Less Volume", "More Volume", 0.55f, 0f, 1f, false, new int[] { 761, 180 }, null, null);
            Params[764] = new VisualParam(764, "Lip_Cleft_Deep", 0, "shape", "Lip Cleft Depth", "Shallow", "Deep", -0.5f, -0.5f, 1.2f, false, null, null, null);
            Params[765] = new VisualParam(765, "Puffy_Lower_Lids", 0, "shape", "Puffy Eyelids", "Flat", "Puffy", -0.3f, -0.3f, 2.5f, false, null, null, null);
            Params[767] = new VisualParam(767, "Bug_Eyed_Head", 1, "shape", "Eye Depth", "Sunken Eyes", "Bug Eyes", 0f, -2f, 2f, false, null, null, null);
            Params[768] = new VisualParam(768, "EyeBone_Bug", 1, "shape", String.Empty, "Eyes Sunken", "Eyes Bugged", -2f, -2f, 2f, false, null, null, null);
            Params[769] = new VisualParam(769, "Eye Depth", 0, "shape", String.Empty, "Sunken Eyes", "Bugged Eyes", 0.5f, 0f, 1f, false, new int[] { 767, 768 }, null, null);
            Params[770] = new VisualParam(770, "Elongate_Head", 1, "shape", "Shear Face", "Flat Head", "Long Head", 0f, -1f, 1f, false, null, null, null);
            Params[771] = new VisualParam(771, "Elongate_Head_Hair", 1, "hair", String.Empty, String.Empty, String.Empty, -1f, -1f, 1f, false, null, null, null);
            Params[772] = new VisualParam(772, "EyeBone_Head_Elongate", 1, "shape", String.Empty, "Eyes Short Head", "Eyes Long Head", -1f, -1f, 1f, false, null, null, null);
            Params[773] = new VisualParam(773, "Head Length", 0, "shape", String.Empty, "Flat Head", "Long Head", 0.5f, 0f, 1f, false, new int[] { 770, 771, 772 }, null, null);
            Params[774] = new VisualParam(774, "Shear_Head_Hair", 1, "hair", String.Empty, String.Empty, String.Empty, -2f, -2f, 2f, false, null, null, null);
            Params[775] = new VisualParam(775, "Body Freckles", 0, "skin", String.Empty, "Less Freckles", "More Freckles", 0f, 0f, 1f, false, new int[] { 776, 777 }, null, null);
            Params[778] = new VisualParam(778, "Collar Back Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "shirt_collar_back_alpha.tga", false, true), null);
            Params[779] = new VisualParam(779, "Collar Back", 0, "undershirt", String.Empty, "Low", "High", 0.84f, 0f, 1f, false, new int[] { 1048, 1049 }, null, null);
            Params[780] = new VisualParam(780, "Collar Back", 0, "jacket", String.Empty, "Low", "High", 0.8f, 0f, 1f, false, new int[] { 1023, 1041, 1024 }, null, null);
            Params[781] = new VisualParam(781, "Collar Back", 0, "shirt", String.Empty, "Low", "High", 0.78f, 0f, 1f, false, new int[] { 778, 1016, 1032, 903 }, null, null);
            Params[782] = new VisualParam(782, "Hair_Pigtails_Short", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[783] = new VisualParam(783, "Hair_Pigtails_Med", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[784] = new VisualParam(784, "Hair_Pigtails_Long", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[785] = new VisualParam(785, "Pigtails", 0, "hair", String.Empty, "Short Pigtails", "Long Pigtails", 0f, 0f, 1f, false, new int[] { 782, 783, 790, 784 }, null, null);
            Params[786] = new VisualParam(786, "Hair_Ponytail_Short", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[787] = new VisualParam(787, "Hair_Ponytail_Med", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[788] = new VisualParam(788, "Hair_Ponytail_Long", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[789] = new VisualParam(789, "Ponytail", 0, "hair", String.Empty, "Short Ponytail", "Long Ponytail", 0f, 0f, 1f, false, new int[] { 786, 787, 788 }, null, null);
            Params[790] = new VisualParam(790, "Hair_Pigtails_Medlong", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, null);
            Params[793] = new VisualParam(793, "Leg_Longcuffs", 1, "pants", "Longcuffs", String.Empty, String.Empty, 0f, 0f, 3f, false, null, null, null);
            Params[794] = new VisualParam(794, "Small_Butt", 1, "shape", "Butt Size", "Regular", "Small", 0f, 0f, 1f, false, null, null, null);
            Params[795] = new VisualParam(795, "Butt Size", 0, "shape", "Butt Size", "Flat Butt", "Big Butt", 0.25f, 0f, 1f, false, new int[] { 867, 794, 151, 852 }, null, null);
            Params[796] = new VisualParam(796, "Pointy_Ears", 0, "shape", "Ear Tips", "Flat", "Pointy", -0.4f, -0.4f, 3f, false, null, null, null);
            Params[797] = new VisualParam(797, "Fat_Upper_Lip", 1, "shape", "Fat Upper Lip", "Normal Upper", "Fat Upper", 0f, 0f, 1.5f, false, null, null, null);
            Params[798] = new VisualParam(798, "Fat_Lower_Lip", 1, "shape", "Fat Lower Lip", "Normal Lower", "Fat Lower", 0f, 0f, 1.5f, false, null, null, null);
            Params[799] = new VisualParam(799, "Lip Ratio", 0, "shape", "Lip Ratio", "More Upper Lip", "More Lower Lip", 0.5f, 0f, 1f, false, new int[] { 797, 798 }, null, null);
            Params[800] = new VisualParam(800, "Sleeve Length", 0, "shirt", String.Empty, "Short", "Long", 0.89f, 0f, 1f, false, new int[] { 600, 1013, 1029, 900 }, null, null);
            Params[801] = new VisualParam(801, "Shirt Bottom", 0, "shirt", String.Empty, "Short", "Long", 1f, 0f, 1f, false, new int[] { 601, 1014, 1030, 901 }, null, null);
            Params[802] = new VisualParam(802, "Collar Front", 0, "shirt", String.Empty, "Low", "High", 0.78f, 0f, 1f, false, new int[] { 602, 1015, 1031, 902 }, null, null);
            Params[803] = new VisualParam(803, "shirt_red", 0, "shirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 0) }));
            Params[804] = new VisualParam(804, "shirt_green", 0, "shirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 255, 0) }));
            Params[805] = new VisualParam(805, "shirt_blue", 0, "shirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 0, 255) }));
            Params[806] = new VisualParam(806, "pants_red", 0, "pants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 0) }));
            Params[807] = new VisualParam(807, "pants_green", 0, "pants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 255, 0) }));
            Params[808] = new VisualParam(808, "pants_blue", 0, "pants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 0, 255) }));
            Params[809] = new VisualParam(809, "lower_jacket_red", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 0) }));
            Params[810] = new VisualParam(810, "lower_jacket_green", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 255, 0) }));
            Params[811] = new VisualParam(811, "lower_jacket_blue", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 0, 255) }));
            Params[812] = new VisualParam(812, "shoes_red", 0, "shoes", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 0) }));
            Params[813] = new VisualParam(813, "shoes_green", 0, "shoes", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 255, 0) }));
            Params[814] = new VisualParam(814, "Waist Height", 0, "pants", String.Empty, "Low", "High", 1f, 0f, 1f, false, new int[] { 614, 1017, 1033, 914 }, null, null);
            Params[815] = new VisualParam(815, "Pants Length", 0, "pants", String.Empty, "Short", "Long", 0.8f, 0f, 1f, false, new int[] { 615, 1018, 1036, 793, 915 }, null, null);
            Params[816] = new VisualParam(816, "Loose Lower Clothing", 0, "pants", "Pants Fit", "Tight Pants", "Loose Pants", 0f, 0f, 1f, false, new int[] { 516, 913 }, null, null);
            Params[817] = new VisualParam(817, "shoes_blue", 0, "shoes", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 0, 255) }));
            Params[818] = new VisualParam(818, "socks_red", 0, "socks", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 0) }));
            Params[819] = new VisualParam(819, "socks_green", 0, "socks", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 255, 0) }));
            Params[820] = new VisualParam(820, "socks_blue", 0, "socks", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 0, 255) }));
            Params[821] = new VisualParam(821, "undershirt_red", 0, "undershirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 0) }));
            Params[822] = new VisualParam(822, "undershirt_green", 0, "undershirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 255, 0) }));
            Params[823] = new VisualParam(823, "undershirt_blue", 0, "undershirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 0, 255) }));
            Params[824] = new VisualParam(824, "underpants_red", 0, "underpants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 0) }));
            Params[825] = new VisualParam(825, "underpants_green", 0, "underpants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 255, 0) }));
            Params[826] = new VisualParam(826, "underpants_blue", 0, "underpants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 0, 255) }));
            Params[827] = new VisualParam(827, "gloves_red", 0, "gloves", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 0) }));
            Params[828] = new VisualParam(828, "Loose Upper Clothing", 0, "shirt", "Shirt Fit", "Tight Shirt", "Loose Shirt", 0f, 0f, 1f, false, new int[] { 628, 899 }, null, null);
            Params[829] = new VisualParam(829, "gloves_green", 0, "gloves", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 255, 0) }));
            Params[830] = new VisualParam(830, "gloves_blue", 0, "gloves", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 0, 255) }));
            Params[831] = new VisualParam(831, "upper_jacket_red", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 0) }));
            Params[832] = new VisualParam(832, "upper_jacket_green", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 255, 0) }));
            Params[833] = new VisualParam(833, "upper_jacket_blue", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 0, 255) }));
            Params[834] = new VisualParam(834, "jacket_red", 0, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, new int[] { 809, 831 }, null, null);
            Params[835] = new VisualParam(835, "jacket_green", 0, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, new int[] { 810, 832 }, null, null);
            Params[836] = new VisualParam(836, "jacket_blue", 0, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, new int[] { 811, 833 }, null, null);
            Params[840] = new VisualParam(840, "Shirtsleeve_flair", 0, "shirt", "Sleeve Looseness", "Tight Sleeves", "Loose Sleeves", 0f, 0f, 1.5f, false, null, null, null);
            Params[841] = new VisualParam(841, "Bowed_Legs", 0, "shape", "Knee Angle", "Knock Kneed", "Bow Legged", 0f, -1f, 1f, false, new int[] { 853, 847 }, null, null);
            Params[842] = new VisualParam(842, "Hip Length", 0, "shape", String.Empty, "Short hips", "Long Hips", -1f, -1f, 1f, false, null, null, null);
            Params[843] = new VisualParam(843, "No_Chest", 1, "shape", "Chest Size", "Some", "None", 0f, 0f, 1f, false, null, null, null);
            Params[844] = new VisualParam(844, "Glove Fingers", 0, "gloves", String.Empty, "Fingerless", "Fingers", 1f, 0.01f, 1f, false, new int[] { 1060, 1061 }, null, null);
            Params[845] = new VisualParam(845, "skirt_poofy", 1, "skirt", "poofy skirt", "less poofy", "more poofy", 0f, 0f, 1.5f, false, null, null, null);
            Params[846] = new VisualParam(846, "skirt_loose", 1, "skirt", "loose skirt", "form fitting", "loose", 0f, 0f, 1f, false, null, null, null);
            Params[848] = new VisualParam(848, "skirt_bustle", 0, "skirt", "bustle skirt", "no bustle", "more bustle", 0.2f, 0f, 2f, false, null, null, null);
            Params[858] = new VisualParam(858, "Skirt Length", 0, "skirt", String.Empty, "Short", "Long", 0.4f, 0.01f, 1f, false, null, new VisualAlphaParam(0f, "skirt_length_alpha.tga", false, true), null);
            Params[859] = new VisualParam(859, "Slit Front", 0, "skirt", String.Empty, "Open Front", "Closed Front", 1f, 0f, 1f, false, null, new VisualAlphaParam(0f, "skirt_slit_front_alpha.tga", false, true), null);
            Params[860] = new VisualParam(860, "Slit Back", 0, "skirt", String.Empty, "Open Back", "Closed Back", 1f, 0f, 1f, false, null, new VisualAlphaParam(0f, "skirt_slit_back_alpha.tga", false, true), null);
            Params[861] = new VisualParam(861, "Slit Left", 0, "skirt", String.Empty, "Open Left", "Closed Left", 1f, 0f, 1f, false, null, new VisualAlphaParam(0f, "skirt_slit_left_alpha.tga", false, true), null);
            Params[862] = new VisualParam(862, "Slit Right", 0, "skirt", String.Empty, "Open Right", "Closed Right", 1f, 0f, 1f, false, null, new VisualAlphaParam(0f, "skirt_slit_right_alpha.tga", false, true), null);
            Params[863] = new VisualParam(863, "skirt_looseness", 0, "skirt", "Skirt Fit", "Tight Skirt", "Poofy Skirt", 0.333f, 0f, 1f, false, new int[] { 866, 846, 845 }, null, null);
            Params[866] = new VisualParam(866, "skirt_tight", 1, "skirt", "tight skirt", "form fitting", "loose", 0f, 0f, 1f, false, null, null, null);
            Params[867] = new VisualParam(867, "skirt_smallbutt", 1, "skirt", "tight skirt", "form fitting", "loose", 0f, 0f, 1f, false, null, null, null);
            Params[868] = new VisualParam(868, "Shirt Wrinkles", 0, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, null, null);
            Params[869] = new VisualParam(869, "Pants Wrinkles", 0, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, null, null);
            Params[870] = new VisualParam(870, "Pointy_Eyebrows", 1, "hair", "Eyebrow Points", "Smooth", "Pointy", -0.5f, -0.5f, 1f, false, null, null, null);
            Params[871] = new VisualParam(871, "Lower_Eyebrows", 1, "hair", "Eyebrow Height", "Higher", "Lower", -2f, -2f, 2f, false, null, null, null);
            Params[872] = new VisualParam(872, "Arced_Eyebrows", 1, "hair", "Eyebrow Arc", "Flat", "Arced", 0f, 0f, 1f, false, null, null, null);
            Params[873] = new VisualParam(873, "Bump base", 1, "skin", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0f, string.Empty, false, false), null);
            Params[874] = new VisualParam(874, "Bump upperdef", 1, "skin", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0f, string.Empty, false, false), null);
            Params[877] = new VisualParam(877, "Jacket Wrinkles", 0, "jacket", "Jacket Wrinkles", "No Wrinkles", "Wrinkles", 0f, 0f, 1f, false, new int[] { 875, 876 }, null, null);
            Params[878] = new VisualParam(878, "Bump upperdef", 1, "skin", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0f, string.Empty, false, false), null);
            Params[879] = new VisualParam(879, "Male_Package", 0, "shape", "Package", "Coin Purse", "Duffle Bag", 0f, -0.5f, 2f, false, null, null, null);
            Params[880] = new VisualParam(880, "Eyelid_Inner_Corner_Up", 0, "shape", "Inner Eye Corner", "Corner Down", "Corner Up", -1.3f, -1.3f, 1.2f, false, null, null, null);
            Params[899] = new VisualParam(899, "Upper Clothes Shading", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(0, 0, 0, 0), new Color4(80, 0, 0, 0) }));
            Params[900] = new VisualParam(900, "Sleeve Length Shadow", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 0.87f, false, null, new VisualAlphaParam(0.03f, "shirt_sleeve_alpha.tga", true, false), null);
            Params[901] = new VisualParam(901, "Shirt Shadow Bottom", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 1f, false, null, new VisualAlphaParam(0.05f, "shirt_bottom_alpha.tga", true, true), null);
            Params[902] = new VisualParam(902, "Collar Front Shadow Height", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 1f, false, null, new VisualAlphaParam(0.02f, "shirt_collar_alpha.tga", true, true), null);
            Params[903] = new VisualParam(903, "Collar Back Shadow Height", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 1f, false, null, new VisualAlphaParam(0.02f, "shirt_collar_back_alpha.tga", true, true), null);
            Params[913] = new VisualParam(913, "Lower Clothes Shading", 1, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(0, 0, 0, 0), new Color4(80, 0, 0, 0) }));
            Params[914] = new VisualParam(914, "Waist Height Shadow", 1, "pants", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 1f, false, null, new VisualAlphaParam(0.04f, "pants_waist_alpha.tga", true, false), null);
            Params[915] = new VisualParam(915, "Pants Length Shadow", 1, "pants", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 1f, false, null, new VisualAlphaParam(0.03f, "pants_length_alpha.tga", true, false), null);
            Params[921] = new VisualParam(921, "skirt_red", 0, "skirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 255, 0, 0) }));
            Params[922] = new VisualParam(922, "skirt_green", 0, "skirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 255, 0) }));
            Params[923] = new VisualParam(923, "skirt_blue", 0, "skirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 0, 0, 0), new Color4(255, 0, 0, 255) }));
            Params[1000] = new VisualParam(1000, "Eyebrow Size Bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.1f, "eyebrows_alpha.tga", false, false), null);
            Params[1001] = new VisualParam(1001, "Eyebrow Size", 1, "hair", String.Empty, String.Empty, String.Empty, 0.5f, 0f, 1f, false, null, new VisualAlphaParam(0.1f, "eyebrows_alpha.tga", false, false), null);
            Params[1002] = new VisualParam(1002, "Eyebrow Density Bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(0, 255, 255, 255), new Color4(255, 255, 255, 255) }));
            Params[1003] = new VisualParam(1003, "Eyebrow Density", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, null, new VisualColorParam(VisualColorOperation.Blend, new Color4[] { new Color4(0, 255, 255, 255), new Color4(255, 255, 255, 255) }));
            Params[1004] = new VisualParam(1004, "Sideburns bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "facehair_sideburns_alpha.tga", true, false), null);
            Params[1005] = new VisualParam(1005, "Sideburns", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "facehair_sideburns_alpha.tga", true, false), null);
            Params[1006] = new VisualParam(1006, "Moustache bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "facehair_moustache_alpha.tga", true, false), null);
            Params[1007] = new VisualParam(1007, "Moustache", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "facehair_moustache_alpha.tga", true, false), null);
            Params[1008] = new VisualParam(1008, "Soulpatch bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.1f, "facehair_soulpatch_alpha.tga", true, false), null);
            Params[1009] = new VisualParam(1009, "Soulpatch", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, new VisualAlphaParam(0.1f, "facehair_soulpatch_alpha.tga", true, false), null);
            Params[1010] = new VisualParam(1010, "Chin Curtains bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.03f, "facehair_chincurtains_alpha.tga", true, false), null);
            Params[1011] = new VisualParam(1011, "Chin Curtains", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, new VisualAlphaParam(0.03f, "facehair_chincurtains_alpha.tga", true, false), null);
            Params[1012] = new VisualParam(1012, "5 O'Clock Shadow bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, null, new VisualColorParam(VisualColorOperation.Add, new Color4[] { new Color4(255, 255, 255, 255), new Color4(0, 255, 255, 255) }));
            Params[1013] = new VisualParam(1013, "Sleeve Length Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 0.85f, true, null, new VisualAlphaParam(0.01f, "shirt_sleeve_alpha.tga", false, false), null);
            Params[1014] = new VisualParam(1014, "Shirt Bottom Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_bottom_alpha.tga", false, true), null);
            Params[1015] = new VisualParam(1015, "Collar Front Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_collar_alpha.tga", false, true), null);
            Params[1016] = new VisualParam(1016, "Collar Back Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_collar_back_alpha.tga", false, true), null);
            Params[1017] = new VisualParam(1017, "Waist Height Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "pants_waist_alpha.tga", false, false), null);
            Params[1018] = new VisualParam(1018, "Pants Length Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "pants_length_alpha.tga", false, false), null);
            Params[1019] = new VisualParam(1019, "Jacket Sleeve Length bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "shirt_sleeve_alpha.tga", false, false), null);
            Params[1020] = new VisualParam(1020, "jacket Sleeve Length", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, new VisualAlphaParam(0.01f, "shirt_sleeve_alpha.tga", false, false), null);
            Params[1021] = new VisualParam(1021, "Jacket Collar Front bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_collar_alpha.tga", false, true), null);
            Params[1022] = new VisualParam(1022, "jacket Collar Front", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "shirt_collar_alpha.tga", false, true), null);
            Params[1023] = new VisualParam(1023, "Jacket Collar Back bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_collar_back_alpha.tga", false, true), null);
            Params[1024] = new VisualParam(1024, "jacket Collar Back", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "shirt_collar_back_alpha.tga", false, true), null);
            Params[1025] = new VisualParam(1025, "jacket bottom length upper bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "jacket_length_upper_alpha.tga", false, true), null);
            Params[1026] = new VisualParam(1026, "jacket open upper bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "jacket_open_upper_alpha.tga", false, true), null);
            Params[1027] = new VisualParam(1027, "jacket bottom length lower bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "jacket_length_lower_alpha.tga", false, false), null);
            Params[1028] = new VisualParam(1028, "jacket open lower bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "jacket_open_lower_alpha.tga", false, true), null);
            Params[1029] = new VisualParam(1029, "Sleeve Length Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 0.85f, true, null, new VisualAlphaParam(0.01f, "shirt_sleeve_alpha.tga", false, false), null);
            Params[1030] = new VisualParam(1030, "Shirt Bottom Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_bottom_alpha.tga", false, true), null);
            Params[1031] = new VisualParam(1031, "Collar Front Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_collar_alpha.tga", false, true), null);
            Params[1032] = new VisualParam(1032, "Collar Back Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_collar_back_alpha.tga", false, true), null);
            Params[1033] = new VisualParam(1033, "jacket bottom length lower bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "jacket_length_lower_alpha.tga", false, false), null);
            Params[1034] = new VisualParam(1034, "jacket open lower bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "jacket_open_lower_alpha.tga", false, true), null);
            Params[1035] = new VisualParam(1035, "Waist Height Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "pants_waist_alpha.tga", false, false), null);
            Params[1036] = new VisualParam(1036, "Pants Length Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "pants_length_alpha.tga", false, false), null);
            Params[1037] = new VisualParam(1037, "jacket bottom length upper bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "jacket_length_upper_alpha.tga", false, true), null);
            Params[1038] = new VisualParam(1038, "jacket open upper bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "jacket_open_upper_alpha.tga", false, true), null);
            Params[1039] = new VisualParam(1039, "Jacket Sleeve Length bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "shirt_sleeve_alpha.tga", false, false), null);
            Params[1040] = new VisualParam(1040, "Jacket Collar Front bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_collar_alpha.tga", false, true), null);
            Params[1041] = new VisualParam(1041, "Jacket Collar Back bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_collar_back_alpha.tga", false, true), null);
            Params[1042] = new VisualParam(1042, "Sleeve Length", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.4f, 0.01f, 1f, false, null, new VisualAlphaParam(0.01f, "shirt_sleeve_alpha.tga", false, false), null);
            Params[1043] = new VisualParam(1043, "Sleeve Length bump", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.4f, 0.01f, 1f, true, null, new VisualAlphaParam(0.01f, "shirt_sleeve_alpha.tga", false, false), null);
            Params[1044] = new VisualParam(1044, "Bottom", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "shirt_bottom_alpha.tga", false, true), null);
            Params[1045] = new VisualParam(1045, "Bottom bump", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_bottom_alpha.tga", false, true), null);
            Params[1046] = new VisualParam(1046, "Collar Front", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "shirt_collar_alpha.tga", false, true), null);
            Params[1047] = new VisualParam(1047, "Collar Front bump", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_collar_alpha.tga", false, true), null);
            Params[1048] = new VisualParam(1048, "Collar Back", 1, "undershirt", String.Empty, "Low", "High", 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "shirt_collar_back_alpha.tga", false, true), null);
            Params[1049] = new VisualParam(1049, "Collar Back bump", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "shirt_collar_back_alpha.tga", false, true), null);
            Params[1050] = new VisualParam(1050, "Socks Length bump", 1, "socks", String.Empty, String.Empty, String.Empty, 0.35f, 0f, 1f, false, null, new VisualAlphaParam(0.01f, "shoe_height_alpha.tga", false, false), null);
            Params[1051] = new VisualParam(1051, "Socks Length bump", 1, "socks", String.Empty, String.Empty, String.Empty, 0.35f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "shoe_height_alpha.tga", false, false), null);
            Params[1052] = new VisualParam(1052, "Shoe Height", 1, "shoes", String.Empty, String.Empty, String.Empty, 0.1f, 0f, 1f, false, null, new VisualAlphaParam(0.01f, "shoe_height_alpha.tga", false, false), null);
            Params[1053] = new VisualParam(1053, "Shoe Height bump", 1, "shoes", String.Empty, String.Empty, String.Empty, 0.1f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "shoe_height_alpha.tga", false, false), null);
            Params[1054] = new VisualParam(1054, "Pants Length", 1, "underpants", String.Empty, String.Empty, String.Empty, 0.3f, 0f, 1f, false, null, new VisualAlphaParam(0.01f, "pants_length_alpha.tga", false, false), null);
            Params[1055] = new VisualParam(1055, "Pants Length", 1, "underpants", String.Empty, String.Empty, String.Empty, 0.3f, 0f, 1f, true, null, new VisualAlphaParam(0.01f, "pants_length_alpha.tga", false, false), null);
            Params[1056] = new VisualParam(1056, "Pants Waist", 1, "underpants", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, false, null, new VisualAlphaParam(0.05f, "pants_waist_alpha.tga", false, false), null);
            Params[1057] = new VisualParam(1057, "Pants Waist", 1, "underpants", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f, true, null, new VisualAlphaParam(0.05f, "pants_waist_alpha.tga", false, false), null);
            Params[1058] = new VisualParam(1058, "Glove Length", 1, "gloves", String.Empty, String.Empty, String.Empty, 0.8f, 0.01f, 1f, false, null, new VisualAlphaParam(0.01f, "glove_length_alpha.tga", false, false), null);
            Params[1059] = new VisualParam(1059, "Glove Length bump", 1, "gloves", String.Empty, String.Empty, String.Empty, 0.8f, 0.01f, 1f, true, null, new VisualAlphaParam(0.01f, "glove_length_alpha.tga", false, false), null);
            Params[1060] = new VisualParam(1060, "Glove Fingers", 1, "gloves", String.Empty, String.Empty, String.Empty, 1f, 0.01f, 1f, false, null, new VisualAlphaParam(0.01f, "gloves_fingers_alpha.tga", false, true), null);
            Params[1061] = new VisualParam(1061, "Glove Fingers bump", 1, "gloves", String.Empty, String.Empty, String.Empty, 1f, 0.01f, 1f, true, null, new VisualAlphaParam(0.01f, "gloves_fingers_alpha.tga", false, true), null);
        }
    }
}
