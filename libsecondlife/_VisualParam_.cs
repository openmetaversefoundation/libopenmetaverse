using System;
using System.Collections.Generic;

namespace OpenMetaverse
{
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
        public VisualParam(int paramID, string name, int group, string wearable, string label, string labelMin, string labelMax, float def, float min, float max)
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
        }
    }

    /// <summary>
    /// Holds the Params array of all the avatar appearance parameters
    /// </summary>
    public static class VisualParams
    {
        public static VisualParam Find(string name, string wearable)
        {
            foreach (KeyValuePair<int, VisualParam> param in Params)
                if (param.Value.Name == name && param.Value.Wearable == wearable)
                    return param.Value;

            return new VisualParam();
        }

        public static SortedList<int, VisualParam> Params = new SortedList<int, VisualParam>();

        static VisualParams()
        {
            Params[1] = new VisualParam(1, "Big_Brow", 0, "shape", "Brow Size", "Small", "Large", -0.3f, -0.3f, 2f);
            Params[2] = new VisualParam(2, "Nose_Big_Out", 0, "shape", "Nose Size", "Small", "Large", -0.8f, -0.8f, 2.5f);
            Params[4] = new VisualParam(4, "Broad_Nostrils", 0, "shape", "Nostril Width", "Narrow", "Broad", -0.5f, -0.5f, 1f);
            Params[5] = new VisualParam(5, "Cleft_Chin", 0, "shape", "Chin Cleft", "Round", "Cleft", -0.1f, -0.1f, 1f);
            Params[6] = new VisualParam(6, "Bulbous_Nose_Tip", 0, "shape", "Nose Tip Shape", "Pointy", "Bulbous", -0.3f, -0.3f, 1f);
            Params[7] = new VisualParam(7, "Weak_Chin", 0, "shape", "Chin Angle", "Chin Out", "Chin In", -0.5f, -0.5f, 0.5f);
            Params[8] = new VisualParam(8, "Double_Chin", 0, "shape", "Chin-Neck", "Tight Chin", "Double Chin", -0.5f, -0.5f, 1.5f);
            Params[10] = new VisualParam(10, "Sunken_Cheeks", 0, "shape", "Lower Cheeks", "Well-Fed", "Sunken", -1.5f, -1.5f, 3f);
            Params[11] = new VisualParam(11, "Noble_Nose_Bridge", 0, "shape", "Upper Bridge", "Low", "High", -0.5f, -0.5f, 1.5f);
            Params[12] = new VisualParam(12, "Jowls", 0, "shape", String.Empty, "Less", "More", -0.5f, -0.5f, 2.5f);
            Params[13] = new VisualParam(13, "Cleft_Chin_Upper", 0, "shape", "Upper Chin Cleft", "Round", "Cleft", 0f, 0f, 1.5f);
            Params[14] = new VisualParam(14, "High_Cheek_Bones", 0, "shape", "Cheek Bones", "Low", "High", -0.5f, -0.5f, 1f);
            Params[15] = new VisualParam(15, "Ears_Out", 0, "shape", "Ear Angle", "In", "Out", -0.5f, -0.5f, 1.5f);
            Params[16] = new VisualParam(16, "Pointy_Eyebrows", 0, "hair", "Eyebrow Points", "Smooth", "Pointy", -0.5f, -0.5f, 3f);
            Params[17] = new VisualParam(17, "Square_Jaw", 0, "shape", "Jaw Shape", "Pointy", "Square", -0.5f, -0.5f, 1f);
            Params[18] = new VisualParam(18, "Puffy_Upper_Cheeks", 0, "shape", "Upper Cheeks", "Thin", "Puffy", -1.5f, -1.5f, 2.5f);
            Params[19] = new VisualParam(19, "Upturned_Nose_Tip", 0, "shape", "Nose Tip Angle", "Downturned", "Upturned", -1.5f, -1.5f, 1f);
            Params[20] = new VisualParam(20, "Bulbous_Nose", 0, "shape", "Nose Thickness", "Thin Nose", "Bulbous Nose", -0.5f, -0.5f, 1.5f);
            Params[21] = new VisualParam(21, "Upper_Eyelid_Fold", 0, "shape", "Upper Eyelid Fold", "Uncreased", "Creased", -0.2f, -0.2f, 1.3f);
            Params[22] = new VisualParam(22, "Attached_Earlobes", 0, "shape", "Attached Earlobes", "Unattached", "Attached", 0f, 0f, 1f);
            Params[23] = new VisualParam(23, "Baggy_Eyes", 0, "shape", "Eye Bags", "Smooth", "Baggy", -0.5f, -0.5f, 1.5f);
            Params[24] = new VisualParam(24, "Wide_Eyes", 0, "shape", "Eye Opening", "Narrow", "Wide", -1.5f, -1.5f, 2f);
            Params[25] = new VisualParam(25, "Wide_Lip_Cleft", 0, "shape", "Lip Cleft", "Narrow", "Wide", -0.8f, -0.8f, 1.5f);
            Params[26] = new VisualParam(26, "Lips_Thin", 1, "shape", String.Empty, String.Empty, String.Empty, 0f, 0f, 0.7f);
            Params[27] = new VisualParam(27, "Wide_Nose_Bridge", 0, "shape", "Bridge Width", "Narrow", "Wide", -1.3f, -1.3f, 1.2f);
            Params[28] = new VisualParam(28, "Lips_Fat", 1, "shape", String.Empty, String.Empty, String.Empty, 0f, 0f, 2f);
            Params[29] = new VisualParam(29, "Wide_Upper_Lip", 1, "shape", String.Empty, String.Empty, String.Empty, -0.7f, -0.7f, 1.3f);
            Params[30] = new VisualParam(30, "Wide_Lower_Lip", 1, "shape", String.Empty, String.Empty, String.Empty, -0.7f, -0.7f, 1.3f);
            Params[31] = new VisualParam(31, "Arced_Eyebrows", 0, "hair", "Eyebrow Arc", "Flat", "Arced", 0.5f, 0f, 2f);
            Params[33] = new VisualParam(33, "Height", 0, "shape", "Height", "Short", "Tall", -2.3f, -2.3f, 2f);
            Params[34] = new VisualParam(34, "Thickness", 0, "shape", "Body Thickness", "Body Thin", "Body Thick", -0.7f, -0.7f, 1.5f);
            Params[35] = new VisualParam(35, "Big_Ears", 0, "shape", "Ear Size", "Small", "Large", -1f, -1f, 2f);
            Params[36] = new VisualParam(36, "Shoulders", 0, "shape", "Shoulders", "Narrow", "Broad", -0.5f, -1.8f, 1.4f);
            Params[37] = new VisualParam(37, "Hip Width", 0, "shape", "Hip Width", "Narrow", "Wide", -3.2f, -3.2f, 2.8f);
            Params[38] = new VisualParam(38, "Torso Length", 0, "shape", String.Empty, "Short Torso", "Long Torso", -1f, -1f, 1f);
            Params[80] = new VisualParam(80, "male", 0, "shape", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[93] = new VisualParam(93, "Glove Length", 0, "gloves", String.Empty, "Short", "Long", 0.8f, 0.01f, 1f);
            Params[98] = new VisualParam(98, "Eye Lightness", 0, "eyes", String.Empty, "Darker", "Lighter", 0f, 0f, 1f);
            Params[99] = new VisualParam(99, "Eye Color", 0, "eyes", String.Empty, "Natural", "Unnatural", 0f, 0f, 1f);
            Params[105] = new VisualParam(105, "Breast Size", 0, "shape", String.Empty, "Small", "Large", 0.5f, 0f, 1f);
            Params[106] = new VisualParam(106, "Muscular_Torso", 1, "shape", "Torso Muscles", "Regular", "Muscular", 0f, 0f, 1.4f);
            Params[108] = new VisualParam(108, "Rainbow Color", 0, "skin", String.Empty, "None", "Wild", 0f, 0f, 1f);
            Params[110] = new VisualParam(110, "Red Skin", 0, "skin", "Ruddiness", "Pale", "Ruddy", 0f, 0f, 0.1f);
            Params[111] = new VisualParam(111, "Pigment", 0, "skin", String.Empty, "Light", "Dark", 0.5f, 0f, 1f);
            Params[112] = new VisualParam(112, "Rainbow Color", 0, "hair", String.Empty, "None", "Wild", 0f, 0f, 1f);
            Params[113] = new VisualParam(113, "Red Hair", 0, "hair", String.Empty, "No Red", "Very Red", 0f, 0f, 1f);
            Params[114] = new VisualParam(114, "Blonde Hair", 0, "hair", String.Empty, "Black", "Blonde", 0.5f, 0f, 1f);
            Params[115] = new VisualParam(115, "White Hair", 0, "hair", String.Empty, "No White", "All White", 0f, 0f, 1f);
            Params[116] = new VisualParam(116, "Rosy Complexion", 0, "skin", String.Empty, "Less Rosy", "More Rosy", 0f, 0f, 1f);
            Params[117] = new VisualParam(117, "Lip Pinkness", 0, "skin", String.Empty, "Darker", "Pinker", 0f, 0f, 1f);
            Params[119] = new VisualParam(119, "Eyebrow Size", 0, "hair", String.Empty, "Thin Eyebrows", "Bushy Eyebrows", 0.5f, 0f, 1f);
            Params[130] = new VisualParam(130, "Front Fringe", 0, "hair", String.Empty, "Short", "Long", 0.45f, 0f, 1f);
            Params[131] = new VisualParam(131, "Side Fringe", 0, "hair", String.Empty, "Short", "Long", 0.5f, 0f, 1f);
            Params[132] = new VisualParam(132, "Back Fringe", 0, "hair", String.Empty, "Short", "Long", 0.39f, 0f, 1f);
            Params[133] = new VisualParam(133, "Hair Front", 0, "hair", String.Empty, "Short", "Long", 0.25f, 0f, 1f);
            Params[134] = new VisualParam(134, "Hair Sides", 0, "hair", String.Empty, "Short", "Long", 0.5f, 0f, 1f);
            Params[135] = new VisualParam(135, "Hair Back", 0, "hair", String.Empty, "Short", "Long", 0.55f, 0f, 1f);
            Params[136] = new VisualParam(136, "Hair Sweep", 0, "hair", String.Empty, "Sweep Forward", "Sweep Back", 0.5f, 0f, 1f);
            Params[137] = new VisualParam(137, "Hair Tilt", 0, "hair", String.Empty, "Left", "Right", 0.5f, 0f, 1f);
            Params[140] = new VisualParam(140, "Hair_Part_Middle", 0, "hair", "Middle Part", "No Part", "Part", 0f, 0f, 2f);
            Params[141] = new VisualParam(141, "Hair_Part_Right", 0, "hair", "Right Part", "No Part", "Part", 0f, 0f, 2f);
            Params[142] = new VisualParam(142, "Hair_Part_Left", 0, "hair", "Left Part", "No Part", "Part", 0f, 0f, 2f);
            Params[143] = new VisualParam(143, "Hair_Sides_Full", 0, "hair", "Full Hair Sides", "Mowhawk", "Full Sides", 0.125f, -4f, 1.5f);
            Params[144] = new VisualParam(144, "Bangs_Front_Up", 1, "hair", "Front Bangs Up", "Bangs", "Bangs Up", 0f, 0f, 1f);
            Params[145] = new VisualParam(145, "Bangs_Front_Down", 1, "hair", "Front Bangs Down", "Bangs", "Bangs Down", 0f, 0f, 5f);
            Params[146] = new VisualParam(146, "Bangs_Sides_Up", 1, "hair", "Side Bangs Up", "Side Bangs", "Side Bangs Up", 0f, 0f, 1f);
            Params[147] = new VisualParam(147, "Bangs_Sides_Down", 1, "hair", "Side Bangs Down", "Side Bangs", "Side Bangs Down", 0f, 0f, 2f);
            Params[148] = new VisualParam(148, "Bangs_Back_Up", 1, "hair", "Back Bangs Up", "Back Bangs", "Back Bangs Up", 0f, 0f, 1f);
            Params[149] = new VisualParam(149, "Bangs_Back_Down", 1, "hair", "Back Bangs Down", "Back Bangs", "Back Bangs Down", 0f, 0f, 2f);
            Params[150] = new VisualParam(150, "Body Definition", 0, "skin", String.Empty, "Less", "More", 0f, 0f, 1f);
            Params[151] = new VisualParam(151, "Big_Butt_Legs", 1, "shape", "Butt Size", "Regular", "Large", 0f, 0f, 1f);
            Params[152] = new VisualParam(152, "Muscular_Legs", 1, "shape", "Leg Muscles", "Regular Muscles", "More Muscles", 0f, 0f, 1.5f);
            Params[155] = new VisualParam(155, "Lip Width", 0, "shape", "Lip Width", "Narrow Lips", "Wide Lips", 0f, -0.9f, 1.3f);
            Params[157] = new VisualParam(157, "Belly Size", 0, "shape", String.Empty, "Small", "Big", 0f, 0f, 1f);
            Params[162] = new VisualParam(162, "Facial Definition", 0, "skin", String.Empty, "Less", "More", 0f, 0f, 1f);
            Params[163] = new VisualParam(163, "wrinkles", 0, "skin", String.Empty, "Less", "More", 0f, 0f, 1f);
            Params[165] = new VisualParam(165, "Freckles", 0, "skin", String.Empty, "Less", "More", 0f, 0f, 1f);
            Params[166] = new VisualParam(166, "Sideburns", 0, "hair", String.Empty, "Short Sideburns", "Mutton Chops", 0f, 0f, 1f);
            Params[167] = new VisualParam(167, "Moustache", 0, "hair", String.Empty, "Chaplin", "Handlebars", 0f, 0f, 1f);
            Params[168] = new VisualParam(168, "Soulpatch", 0, "hair", String.Empty, "Less soul", "More soul", 0f, 0f, 1f);
            Params[169] = new VisualParam(169, "Chin Curtains", 0, "hair", String.Empty, "Less Curtains", "More Curtains", 0f, 0f, 1f);
            Params[171] = new VisualParam(171, "Hair_Front_Down", 1, "hair", "Front Hair Down", "Front Hair", "Front Hair Down", 0f, 0f, 1f);
            Params[172] = new VisualParam(172, "Hair_Front_Up", 1, "hair", "Front Hair Up", "Front Hair", "Front Hair Up", 0f, 0f, 1f);
            Params[173] = new VisualParam(173, "Hair_Sides_Down", 1, "hair", "Sides Hair Down", "Sides Hair", "Sides Hair Down", 0f, 0f, 1f);
            Params[174] = new VisualParam(174, "Hair_Sides_Up", 1, "hair", "Sides Hair Up", "Sides Hair", "Sides Hair Up", 0f, 0f, 1f);
            Params[175] = new VisualParam(175, "Hair_Back_Down", 1, "hair", "Back Hair Down", "Back Hair", "Back Hair Down", 0f, 0f, 3f);
            Params[176] = new VisualParam(176, "Hair_Back_Up", 1, "hair", "Back Hair Up", "Back Hair", "Back Hair Up", 0f, 0f, 1f);
            Params[177] = new VisualParam(177, "Hair_Rumpled", 0, "hair", "Rumpled Hair", "Smooth Hair", "Rumpled Hair", 0f, 0f, 1f);
            Params[178] = new VisualParam(178, "Hair_Swept_Back", 1, "hair", "Swept Back Hair", "NotHair", "Swept Back", 0f, 0f, 1f);
            Params[179] = new VisualParam(179, "Hair_Swept_Forward", 1, "hair", "Swept Forward Hair", "Hair", "Swept Forward", 0f, 0f, 1f);
            Params[180] = new VisualParam(180, "Hair_Volume", 1, "hair", "Hair Volume", "Less", "More", 0f, 0f, 1.3f);
            Params[181] = new VisualParam(181, "Hair_Big_Front", 0, "hair", "Big Hair Front", "Less", "More", 0.14f, -1f, 1f);
            Params[182] = new VisualParam(182, "Hair_Big_Top", 0, "hair", "Big Hair Top", "Less", "More", 0.7f, -1f, 1f);
            Params[183] = new VisualParam(183, "Hair_Big_Back", 0, "hair", "Big Hair Back", "Less", "More", 0.05f, -1f, 1f);
            Params[184] = new VisualParam(184, "Hair_Spiked", 0, "hair", "Spiked Hair", "No Spikes", "Big Spikes", 0f, 0f, 1f);
            Params[185] = new VisualParam(185, "Deep_Chin", 0, "shape", "Chin Depth", "Shallow", "Deep", -1f, -1f, 1f);
            Params[186] = new VisualParam(186, "Egg_Head", 1, "shape", "Egg Head", "Chin Heavy", "Forehead Heavy", -1.3f, -1.3f, 1f);
            Params[187] = new VisualParam(187, "Squash_Stretch_Head", 1, "shape", "Squash/Stretch Head", "Squash Head", "Stretch Head", -0.5f, -0.5f, 1f);
            Params[190] = new VisualParam(190, "Hair_Tilt_Right", 1, "hair", "Hair Tilted Right", "Hair", "Tilt Right", 0f, 0f, 1f);
            Params[191] = new VisualParam(191, "Hair_Tilt_Left", 1, "hair", "Hair Tilted Left", "Hair", "Tilt Left", 0f, 0f, 1f);
            Params[192] = new VisualParam(192, "Bangs_Part_Middle", 0, "hair", "Part Bangs", "No Part", "Part Bangs", 0f, 0f, 1f);
            Params[193] = new VisualParam(193, "Head Shape", 0, "shape", "Head Shape", "More Square", "More Round", 0.5f, 0f, 1f);
            Params[194] = new VisualParam(194, "Eye_Spread", 1, "shape", String.Empty, "Eyes Together", "Eyes Spread", -2f, -2f, 2f);
            Params[195] = new VisualParam(195, "EyeBone_Spread", 1, "shape", String.Empty, "Eyes Together", "Eyes Spread", -1f, -1f, 1f);
            Params[196] = new VisualParam(196, "Eye Spacing", 0, "shape", "Eye Spacing", "Close Set Eyes", "Far Set Eyes", 0f, -2f, 1f);
            Params[197] = new VisualParam(197, "Shoe_Heels", 1, "shoes", String.Empty, "No Heels", "High Heels", 0f, 0f, 1f);
            Params[198] = new VisualParam(198, "Heel Height", 0, "shoes", String.Empty, "Low Heels", "High Heels", 0f, 0f, 1f);
            Params[400] = new VisualParam(400, "Displace_Hair_Facial", 1, "hair", "Hair Thickess", "Cropped Hair", "Bushy Hair", 0f, 0f, 2f);
            Params[500] = new VisualParam(500, "Shoe_Heel_Height", 1, "shoes", "Heel Height", "Low Heels", "High Heels", 0f, 0f, 1f);
            Params[501] = new VisualParam(501, "Shoe_Platform_Height", 1, "shoes", "Platform Height", "Low Platforms", "High Platforms", 0f, 0f, 1f);
            Params[502] = new VisualParam(502, "Shoe_Platform", 1, "shoes", String.Empty, "No Heels", "High Heels", 0f, 0f, 1f);
            Params[503] = new VisualParam(503, "Platform Height", 0, "shoes", String.Empty, "Low Platforms", "High Platforms", 0f, 0f, 1f);
            Params[505] = new VisualParam(505, "Lip Thickness", 0, "shape", String.Empty, "Thin Lips", "Fat Lips", 0.5f, 0f, 1f);
            Params[506] = new VisualParam(506, "Mouth_Height", 0, "shape", "Mouth Position", "High", "Low", -2f, -2f, 2f);
            Params[507] = new VisualParam(507, "Breast_Gravity", 0, "shape", "Breast Buoyancy", "Less Gravity", "More Gravity", 0f, -1.5f, 2f);
            Params[508] = new VisualParam(508, "Shoe_Platform_Width", 0, "shoes", "Platform Width", "Narrow", "Wide", -1f, -1f, 2f);
            Params[509] = new VisualParam(509, "Shoe_Heel_Point", 1, "shoes", "Heel Shape", "Default Heels", "Pointy Heels", 0f, 0f, 1f);
            Params[510] = new VisualParam(510, "Shoe_Heel_Thick", 1, "shoes", "Heel Shape", "default Heels", "Thick Heels", 0f, 0f, 1f);
            Params[511] = new VisualParam(511, "Shoe_Toe_Point", 1, "shoes", "Toe Shape", "Default Toe", "Pointy Toe", 0f, 0f, 1f);
            Params[512] = new VisualParam(512, "Shoe_Toe_Square", 1, "shoes", "Toe Shape", "Default Toe", "Square Toe", 0f, 0f, 1f);
            Params[513] = new VisualParam(513, "Heel Shape", 0, "shoes", String.Empty, "Pointy Heels", "Thick Heels", 0.5f, 0f, 1f);
            Params[514] = new VisualParam(514, "Toe Shape", 0, "shoes", String.Empty, "Pointy", "Square", 0.5f, 0f, 1f);
            Params[515] = new VisualParam(515, "Foot_Size", 0, "shape", "Foot Size", "Small", "Big", -1f, -1f, 3f);
            Params[516] = new VisualParam(516, "Displace_Loose_Lowerbody", 1, "pants", "Pants Fit", String.Empty, String.Empty, 0f, 0f, 1f);
            Params[517] = new VisualParam(517, "Wide_Nose", 0, "shape", "Nose Width", "Narrow", "Wide", -0.5f, -0.5f, 1f);
            Params[518] = new VisualParam(518, "Eyelashes_Long", 0, "shape", "Eyelash Length", "Short", "Long", -0.3f, -0.3f, 1.5f);
            Params[600] = new VisualParam(600, "Sleeve Length Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.7f, 0f, 0.85f);
            Params[601] = new VisualParam(601, "Shirt Bottom Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[602] = new VisualParam(602, "Collar Front Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[603] = new VisualParam(603, "Sleeve Length", 0, "undershirt", String.Empty, "Short", "Long", 0.4f, 0.01f, 1f);
            Params[604] = new VisualParam(604, "Bottom", 0, "undershirt", String.Empty, "Short", "Long", 0.85f, 0f, 1f);
            Params[605] = new VisualParam(605, "Collar Front", 0, "undershirt", String.Empty, "Low", "High", 0.84f, 0f, 1f);
            Params[606] = new VisualParam(606, "Sleeve Length", 0, "jacket", String.Empty, "Short", "Long", 0.8f, 0f, 1f);
            Params[607] = new VisualParam(607, "Collar Front", 0, "jacket", String.Empty, "Low", "High", 0.8f, 0f, 1f);
            Params[608] = new VisualParam(608, "bottom length lower", 0, "jacket", "Jacket Length", "Short", "Long", 0.8f, 0f, 1f);
            Params[609] = new VisualParam(609, "open jacket", 0, "jacket", "Open Front", "Open", "Closed", 0.2f, 0f, 1f);
            Params[614] = new VisualParam(614, "Waist Height Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[615] = new VisualParam(615, "Pants Length Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[616] = new VisualParam(616, "Shoe Height", 0, "shoes", String.Empty, "Short", "Tall", 0.1f, 0f, 1f);
            Params[617] = new VisualParam(617, "Socks Length", 0, "socks", String.Empty, "Short", "Long", 0.35f, 0f, 1f);
            Params[619] = new VisualParam(619, "Pants Length", 0, "underpants", String.Empty, "Short", "Long", 0.3f, 0f, 1f);
            Params[620] = new VisualParam(620, "bottom length upper", 1, "jacket", String.Empty, "hi cut", "low cut", 0.8f, 0f, 1f);
            Params[621] = new VisualParam(621, "bottom length lower", 1, "jacket", String.Empty, "hi cut", "low cut", 0.8f, 0f, 1f);
            Params[622] = new VisualParam(622, "open upper", 1, "jacket", String.Empty, "closed", "open", 0.8f, 0f, 1f);
            Params[623] = new VisualParam(623, "open lower", 1, "jacket", String.Empty, "open", "closed", 0.8f, 0f, 1f);
            Params[624] = new VisualParam(624, "Pants Waist", 0, "underpants", String.Empty, "Low", "High", 0.8f, 0f, 1f);
            Params[625] = new VisualParam(625, "Leg_Pantflair", 0, "pants", "Cuff Flare", "Tight Cuffs", "Flared Cuffs", 0f, 0f, 1.5f);
            Params[626] = new VisualParam(626, "Big_Chest", 1, "shape", "Chest Size", "Small", "Large", 0f, 0f, 1f);
            Params[627] = new VisualParam(627, "Small_Chest", 1, "shape", "Chest Size", "Large", "Small", 0f, 0f, 1f);
            Params[628] = new VisualParam(628, "Displace_Loose_Upperbody", 1, "shirt", "Shirt Fit", String.Empty, String.Empty, 0f, 0f, 1f);
            Params[629] = new VisualParam(629, "Forehead Angle", 0, "shape", String.Empty, "More Vertical", "More Sloped", 0.5f, 0f, 1f);
            Params[633] = new VisualParam(633, "Fat_Head", 1, "shape", "Fat Head", "Skinny", "Fat", 0f, 0f, 1f);
            Params[634] = new VisualParam(634, "Fat_Torso", 1, "shape", "Fat Torso", "skinny", "fat", 0f, 0f, 1f);
            Params[635] = new VisualParam(635, "Fat_Legs", 1, "shape", "Fat Torso", "skinny", "fat", 0f, 0f, 1f);
            Params[637] = new VisualParam(637, "Body Fat", 0, "shape", String.Empty, "Less Body Fat", "More Body Fat", 0f, 0f, 1f);
            Params[638] = new VisualParam(638, "Low_Crotch", 0, "pants", "Pants Crotch", "High and Tight", "Low and Loose", 0f, 0f, 1.3f);
            Params[640] = new VisualParam(640, "Hair_Egg_Head", 1, "hair", String.Empty, String.Empty, String.Empty, -1.3f, -1.3f, 1f);
            Params[641] = new VisualParam(641, "Hair_Squash_Stretch_Head", 1, "hair", String.Empty, String.Empty, String.Empty, -0.5f, -0.5f, 1f);
            Params[642] = new VisualParam(642, "Hair_Square_Head", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[643] = new VisualParam(643, "Hair_Round_Head", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[644] = new VisualParam(644, "Hair_Forehead_Round", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[645] = new VisualParam(645, "Hair_Forehead_Slant", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[646] = new VisualParam(646, "Egg_Head", 0, "shape", "Egg Head", "Chin Heavy", "Forehead Heavy", 0f, -1.3f, 1f);
            Params[647] = new VisualParam(647, "Squash_Stretch_Head", 0, "shape", "Head Stretch", "Squash Head", "Stretch Head", 0f, -0.5f, 1f);
            Params[648] = new VisualParam(648, "Scrawny_Torso", 1, "shape", "Torso Muscles", "Regular", "Scrawny", 0f, 0f, 1.3f);
            Params[649] = new VisualParam(649, "Torso Muscles", 0, "shape", "Torso Muscles", "Less Muscular", "More Muscular", 0.5f, 0f, 1f);
            Params[650] = new VisualParam(650, "Eyelid_Corner_Up", 0, "shape", "Outer Eye Corner", "Corner Down", "Corner Up", -1.3f, -1.3f, 1.2f);
            Params[651] = new VisualParam(651, "Scrawny_Legs", 1, "shape", "Scrawny Leg", "Regular Muscles", "Less Muscles", 0f, 0f, 1.5f);
            Params[652] = new VisualParam(652, "Leg Muscles", 0, "shape", String.Empty, "Less Muscular", "More Muscular", 0.5f, 0f, 1f);
            Params[653] = new VisualParam(653, "Tall_Lips", 0, "shape", "Lip Fullness", "Less Full", "More Full", -1f, -1f, 2f);
            Params[654] = new VisualParam(654, "Shoe_Toe_Thick", 0, "shoes", "Toe Thickness", "Flat Toe", "Thick Toe", 0f, 0f, 2f);
            Params[655] = new VisualParam(655, "Head Size", 1, "shape", "Head Size", "Small Head", "Big Head", -0.25f, -0.25f, 0.1f);
            Params[656] = new VisualParam(656, "Crooked_Nose", 0, "shape", "Crooked Nose", "Nose Left", "Nose Right", -2f, -2f, 2f);
            Params[657] = new VisualParam(657, "Smile_Mouth", 1, "shape", "Mouth Corner", "Corner Normal", "Corner Up", 0f, 0f, 1.4f);
            Params[658] = new VisualParam(658, "Frown_Mouth", 1, "shape", "Mouth Corner", "Corner Normal", "Corner Down", 0f, 0f, 1.2f);
            Params[659] = new VisualParam(659, "Mouth Corner", 0, "shape", String.Empty, "Corner Down", "Corner Up", 0.5f, 0f, 1f);
            Params[660] = new VisualParam(660, "Shear_Head", 1, "shape", "Shear Face", "Shear Left", "Shear Right", 0f, -2f, 2f);
            Params[661] = new VisualParam(661, "EyeBone_Head_Shear", 1, "shape", String.Empty, "Eyes Shear Left Up", "Eyes Shear Right Up", -2f, -2f, 2f);
            Params[662] = new VisualParam(662, "Face Shear", 0, "shape", String.Empty, "Shear Right Up", "Shear Left Up", 0.5f, 0f, 1f);
            Params[663] = new VisualParam(663, "Shift_Mouth", 0, "shape", "Shift Mouth", "Shift Left", "Shift Right", 0f, -2f, 2f);
            Params[664] = new VisualParam(664, "Pop_Eye", 0, "shape", "Eye Pop", "Pop Right Eye", "Pop Left Eye", 0f, -1.3f, 1.3f);
            Params[665] = new VisualParam(665, "Jaw_Jut", 0, "shape", "Jaw Jut", "Overbite", "Underbite", 0f, -2f, 2f);
            Params[674] = new VisualParam(674, "Hair_Shear_Back", 0, "hair", "Shear Back", "Full Back", "Sheared Back", -0.3f, -1f, 2f);
            Params[675] = new VisualParam(675, "Hand Size", 0, "shape", String.Empty, "Small Hands", "Large Hands", -0.3f, -0.3f, 0.3f);
            Params[676] = new VisualParam(676, "Love_Handles", 0, "shape", "Love Handles", "Less Love", "More Love", 0f, -1f, 2f);
            Params[677] = new VisualParam(677, "Scrawny_Torso_Male", 1, "shape", "Torso Scrawny", "Regular", "Scrawny", 0f, 0f, 1.3f);
            Params[678] = new VisualParam(678, "Torso Muscles", 0, "shape", String.Empty, "Less Muscular", "More Muscular", 0.5f, 0f, 1f);
            Params[679] = new VisualParam(679, "Eyeball_Size", 1, "shape", "Eyeball Size", "small eye", "big eye", -0.25f, -0.25f, 0.1f);
            Params[681] = new VisualParam(681, "Eyeball_Size", 1, "shape", "Eyeball Size", "small eye", "big eye", -0.25f, -0.25f, 0.1f);
            Params[682] = new VisualParam(682, "Head Size", 0, "shape", "Head Size", "Small Head", "Big Head", 0.5f, 0f, 1f);
            Params[683] = new VisualParam(683, "Neck Thickness", 0, "shape", String.Empty, "Skinny Neck", "Thick Neck", -0.15f, -0.4f, 0.2f);
            Params[684] = new VisualParam(684, "Breast_Female_Cleavage", 0, "shape", "Breast Cleavage", "Separate", "Join", 0f, -0.3f, 1.3f);
            Params[685] = new VisualParam(685, "Chest_Male_No_Pecs", 0, "shape", "Pectorals", "Big Pectorals", "Sunken Chest", 0f, -0.5f, 1.1f);
            Params[686] = new VisualParam(686, "Head_Eyes_Big", 1, "shape", "Eye Size", "Beady Eyes", "Anime Eyes", 0f, -2f, 2f);
            Params[687] = new VisualParam(687, "Eyeball_Size", 1, "shape", "Big Eyeball", "small eye", "big eye", -0.25f, -0.25f, 0.25f);
            Params[689] = new VisualParam(689, "EyeBone_Big_Eyes", 1, "shape", String.Empty, "Eyes Back", "Eyes Forward", -1f, -1f, 1f);
            Params[690] = new VisualParam(690, "Eye Size", 0, "shape", "Eye Size", "Beady Eyes", "Anime Eyes", 0.5f, 0f, 1f);
            Params[691] = new VisualParam(691, "Eyeball_Size", 1, "shape", "Big Eyeball", "small eye", "big eye", -0.25f, -0.25f, 0.25f);
            Params[692] = new VisualParam(692, "Leg Length", 0, "shape", String.Empty, "Short Legs", "Long Legs", -1f, -1f, 1f);
            Params[693] = new VisualParam(693, "Arm Length", 0, "shape", String.Empty, "Short Arms", "Long arms", 0.6f, -1f, 1f);
            Params[694] = new VisualParam(694, "Eyeball_Size", 1, "shape", "Eyeball Size", "small eye", "big eye", -0.25f, -0.25f, 0.1f);
            Params[695] = new VisualParam(695, "Eyeball_Size", 1, "shape", "Big Eyeball", "small eye", "big eye", -0.25f, -0.25f, 0.25f);
            Params[700] = new VisualParam(700, "Lipstick Color", 0, "skin", String.Empty, "Pink", "Black", 0.25f, 0f, 1f);
            Params[701] = new VisualParam(701, "Lipstick", 0, "skin", String.Empty, "No Lipstick", "More Lipstick", 0f, 0f, 0.9f);
            Params[702] = new VisualParam(702, "Lipgloss", 0, "skin", String.Empty, "No Lipgloss", "Glossy", 0f, 0f, 1f);
            Params[703] = new VisualParam(703, "Eyeliner", 0, "skin", String.Empty, "No Eyeliner", "Full Eyeliner", 0f, 0f, 1f);
            Params[704] = new VisualParam(704, "Blush", 0, "skin", String.Empty, "No Blush", "More Blush", 0f, 0f, 0.9f);
            Params[705] = new VisualParam(705, "Blush Color", 0, "skin", String.Empty, "Pink", "Orange", 0.5f, 0f, 1f);
            Params[706] = new VisualParam(706, "Out Shdw Opacity", 0, "skin", String.Empty, "Clear", "Opaque", 0.6f, 0.2f, 1f);
            Params[707] = new VisualParam(707, "Outer Shadow", 0, "skin", String.Empty, "No Eyeshadow", "More Eyeshadow", 0f, 0f, 0.7f);
            Params[708] = new VisualParam(708, "Out Shdw Color", 0, "skin", String.Empty, "Light", "Dark", 0f, 0f, 1f);
            Params[709] = new VisualParam(709, "Inner Shadow", 0, "skin", String.Empty, "No Eyeshadow", "More Eyeshadow", 0f, 0f, 1f);
            Params[710] = new VisualParam(710, "Nail Polish", 0, "skin", String.Empty, "No Polish", "Painted Nails", 0f, 0f, 1f);
            Params[711] = new VisualParam(711, "Blush Opacity", 0, "skin", String.Empty, "Clear", "Opaque", 0.5f, 0f, 1f);
            Params[712] = new VisualParam(712, "In Shdw Color", 0, "skin", String.Empty, "Light", "Dark", 0f, 0f, 1f);
            Params[713] = new VisualParam(713, "In Shdw Opacity", 0, "skin", String.Empty, "Clear", "Opaque", 0.7f, 0.2f, 1f);
            Params[714] = new VisualParam(714, "Eyeliner Color", 0, "skin", String.Empty, "Dark Green", "Black", 0f, 0f, 1f);
            Params[715] = new VisualParam(715, "Nail Polish Color", 0, "skin", String.Empty, "Pink", "Black", 0f, 0f, 1f);
            Params[750] = new VisualParam(750, "Eyebrow Density", 0, "hair", String.Empty, "Sparse", "Dense", 0.7f, 0f, 1f);
            Params[751] = new VisualParam(751, "5 O'Clock Shadow", 1, "hair", String.Empty, "Dense hair", "Shadow hair", 0.7f, 0f, 1f);
            Params[752] = new VisualParam(752, "Hair Thickness", 0, "hair", String.Empty, "5 O'Clock Shadow", "Bushy Hair", 0.5f, 0f, 1f);
            Params[753] = new VisualParam(753, "Saddlebags", 0, "shape", "Saddle Bags", "Less Saddle", "More Saddle", 0f, -0.5f, 3f);
            Params[754] = new VisualParam(754, "Hair_Taper_Back", 0, "hair", "Taper Back", "Wide Back", "Narrow Back", 0f, -1f, 2f);
            Params[755] = new VisualParam(755, "Hair_Taper_Front", 0, "hair", "Taper Front", "Wide Front", "Narrow Front", 0.05f, -1.5f, 1.5f);
            Params[756] = new VisualParam(756, "Neck Length", 0, "shape", String.Empty, "Short Neck", "Long Neck", 0f, -1f, 1f);
            Params[757] = new VisualParam(757, "Lower_Eyebrows", 0, "hair", "Eyebrow Height", "Higher", "Lower", -1f, -4f, 2f);
            Params[758] = new VisualParam(758, "Lower_Bridge_Nose", 0, "shape", "Lower Bridge", "Low", "High", -1.5f, -1.5f, 1.5f);
            Params[759] = new VisualParam(759, "Low_Septum_Nose", 0, "shape", "Nostril Division", "High", "Low", 0.5f, -1f, 1.5f);
            Params[760] = new VisualParam(760, "Jaw_Angle", 0, "shape", "Jaw Angle", "Low Jaw", "High Jaw", 0f, -1.2f, 2f);
            Params[761] = new VisualParam(761, "Hair_Volume_Small", 1, "hair", "Hair Volume", "Less", "More", 0f, 0f, 1.3f);
            Params[762] = new VisualParam(762, "Hair_Shear_Front", 0, "hair", "Shear Front", "Full Front", "Sheared Front", 0f, 0f, 3f);
            Params[763] = new VisualParam(763, "Hair Volume", 0, "hair", String.Empty, "Less Volume", "More Volume", 0.55f, 0f, 1f);
            Params[764] = new VisualParam(764, "Lip_Cleft_Deep", 0, "shape", "Lip Cleft Depth", "Shallow", "Deep", -0.5f, -0.5f, 1.2f);
            Params[765] = new VisualParam(765, "Puffy_Lower_Lids", 0, "shape", "Puffy Eyelids", "Flat", "Puffy", -0.3f, -0.3f, 2.5f);
            Params[767] = new VisualParam(767, "Bug_Eyed_Head", 1, "shape", "Eye Depth", "Sunken Eyes", "Bug Eyes", 0f, -2f, 2f);
            Params[768] = new VisualParam(768, "EyeBone_Bug", 1, "shape", String.Empty, "Eyes Sunken", "Eyes Bugged", -2f, -2f, 2f);
            Params[769] = new VisualParam(769, "Eye Depth", 0, "shape", String.Empty, "Sunken Eyes", "Bugged Eyes", 0.5f, 0f, 1f);
            Params[770] = new VisualParam(770, "Elongate_Head", 1, "shape", "Shear Face", "Flat Head", "Long Head", 0f, -1f, 1f);
            Params[771] = new VisualParam(771, "Elongate_Head_Hair", 1, "hair", String.Empty, String.Empty, String.Empty, -1f, -1f, 1f);
            Params[772] = new VisualParam(772, "EyeBone_Head_Elongate", 1, "shape", String.Empty, "Eyes Short Head", "Eyes Long Head", -1f, -1f, 1f);
            Params[773] = new VisualParam(773, "Head Length", 0, "shape", String.Empty, "Flat Head", "Long Head", 0.5f, 0f, 1f);
            Params[774] = new VisualParam(774, "Shear_Head_Hair", 1, "hair", String.Empty, String.Empty, String.Empty, -2f, -2f, 2f);
            Params[775] = new VisualParam(775, "Body Freckles", 0, "skin", String.Empty, "Less Freckles", "More Freckles", 0f, 0f, 1f);
            Params[778] = new VisualParam(778, "Collar Back Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[779] = new VisualParam(779, "Collar Back", 0, "undershirt", String.Empty, "Low", "High", 0.84f, 0f, 1f);
            Params[780] = new VisualParam(780, "Collar Back", 0, "jacket", String.Empty, "Low", "High", 0.8f, 0f, 1f);
            Params[781] = new VisualParam(781, "Collar Back", 0, "shirt", String.Empty, "Low", "High", 0.78f, 0f, 1f);
            Params[782] = new VisualParam(782, "Hair_Pigtails_Short", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[783] = new VisualParam(783, "Hair_Pigtails_Med", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[784] = new VisualParam(784, "Hair_Pigtails_Long", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[785] = new VisualParam(785, "Pigtails", 0, "hair", String.Empty, "Short Pigtails", "Long Pigtails", 0f, 0f, 1f);
            Params[786] = new VisualParam(786, "Hair_Ponytail_Short", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[787] = new VisualParam(787, "Hair_Ponytail_Med", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[788] = new VisualParam(788, "Hair_Ponytail_Long", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[789] = new VisualParam(789, "Ponytail", 0, "hair", String.Empty, "Short Ponytail", "Long Ponytail", 0f, 0f, 1f);
            Params[790] = new VisualParam(790, "Hair_Pigtails_Medlong", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[793] = new VisualParam(793, "Leg_Longcuffs", 1, "pants", "Longcuffs", String.Empty, String.Empty, 0f, 0f, 3f);
            Params[794] = new VisualParam(794, "Small_Butt", 1, "shape", "Butt Size", "Regular", "Small", 0f, 0f, 1f);
            Params[795] = new VisualParam(795, "Butt Size", 0, "shape", "Butt Size", "Flat Butt", "Big Butt", 0.25f, 0f, 1f);
            Params[796] = new VisualParam(796, "Pointy_Ears", 0, "shape", "Ear Tips", "Flat", "Pointy", -0.4f, -0.4f, 3f);
            Params[797] = new VisualParam(797, "Fat_Upper_Lip", 1, "shape", "Fat Upper Lip", "Normal Upper", "Fat Upper", 0f, 0f, 1.5f);
            Params[798] = new VisualParam(798, "Fat_Lower_Lip", 1, "shape", "Fat Lower Lip", "Normal Lower", "Fat Lower", 0f, 0f, 1.5f);
            Params[799] = new VisualParam(799, "Lip Ratio", 0, "shape", "Lip Ratio", "More Upper Lip", "More Lower Lip", 0.5f, 0f, 1f);
            Params[800] = new VisualParam(800, "Sleeve Length", 0, "shirt", String.Empty, "Short", "Long", 0.89f, 0f, 1f);
            Params[801] = new VisualParam(801, "Shirt Bottom", 0, "shirt", String.Empty, "Short", "Long", 1f, 0f, 1f);
            Params[802] = new VisualParam(802, "Collar Front", 0, "shirt", String.Empty, "Low", "High", 0.78f, 0f, 1f);
            Params[803] = new VisualParam(803, "shirt_red", 0, "shirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[804] = new VisualParam(804, "shirt_green", 0, "shirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[805] = new VisualParam(805, "shirt_blue", 0, "shirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[806] = new VisualParam(806, "pants_red", 0, "pants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[807] = new VisualParam(807, "pants_green", 0, "pants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[808] = new VisualParam(808, "pants_blue", 0, "pants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[809] = new VisualParam(809, "lower_jacket_red", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[810] = new VisualParam(810, "lower_jacket_green", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[811] = new VisualParam(811, "lower_jacket_blue", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[812] = new VisualParam(812, "shoes_red", 0, "shoes", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[813] = new VisualParam(813, "shoes_green", 0, "shoes", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[814] = new VisualParam(814, "Waist Height", 0, "pants", String.Empty, "Low", "High", 1f, 0f, 1f);
            Params[815] = new VisualParam(815, "Pants Length", 0, "pants", String.Empty, "Short", "Long", 0.8f, 0f, 1f);
            Params[816] = new VisualParam(816, "Loose Lower Clothing", 0, "pants", "Pants Fit", "Tight Pants", "Loose Pants", 0f, 0f, 1f);
            Params[817] = new VisualParam(817, "shoes_blue", 0, "shoes", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[818] = new VisualParam(818, "socks_red", 0, "socks", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[819] = new VisualParam(819, "socks_green", 0, "socks", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[820] = new VisualParam(820, "socks_blue", 0, "socks", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[821] = new VisualParam(821, "undershirt_red", 0, "undershirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[822] = new VisualParam(822, "undershirt_green", 0, "undershirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[823] = new VisualParam(823, "undershirt_blue", 0, "undershirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[824] = new VisualParam(824, "underpants_red", 0, "underpants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[825] = new VisualParam(825, "underpants_green", 0, "underpants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[826] = new VisualParam(826, "underpants_blue", 0, "underpants", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[827] = new VisualParam(827, "gloves_red", 0, "gloves", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[828] = new VisualParam(828, "Loose Upper Clothing", 0, "shirt", "Shirt Fit", "Tight Shirt", "Loose Shirt", 0f, 0f, 1f);
            Params[829] = new VisualParam(829, "gloves_green", 0, "gloves", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[830] = new VisualParam(830, "gloves_blue", 0, "gloves", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[831] = new VisualParam(831, "upper_jacket_red", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[832] = new VisualParam(832, "upper_jacket_green", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[833] = new VisualParam(833, "upper_jacket_blue", 1, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[834] = new VisualParam(834, "jacket_red", 0, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[835] = new VisualParam(835, "jacket_green", 0, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[836] = new VisualParam(836, "jacket_blue", 0, "jacket", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[840] = new VisualParam(840, "Shirtsleeve_flair", 0, "shirt", "Sleeve Looseness", "Tight Sleeves", "Loose Sleeves", 0f, 0f, 1.5f);
            Params[841] = new VisualParam(841, "Bowed_Legs", 0, "shape", "Knee Angle", "Knock Kneed", "Bow Legged", 0f, -1f, 1f);
            Params[842] = new VisualParam(842, "Hip Length", 0, "shape", String.Empty, "Short hips", "Long Hips", -1f, -1f, 1f);
            Params[843] = new VisualParam(843, "No_Chest", 1, "shape", "Chest Size", "Some", "None", 0f, 0f, 1f);
            Params[844] = new VisualParam(844, "Glove Fingers", 0, "gloves", String.Empty, "Fingerless", "Fingers", 1f, 0.01f, 1f);
            Params[845] = new VisualParam(845, "skirt_poofy", 1, "skirt", "poofy skirt", "less poofy", "more poofy", 0f, 0f, 1.5f);
            Params[846] = new VisualParam(846, "skirt_loose", 1, "skirt", "loose skirt", "form fitting", "loose", 0f, 0f, 1f);
            Params[848] = new VisualParam(848, "skirt_bustle", 0, "skirt", "bustle skirt", "no bustle", "more bustle", 0.2f, 0f, 2f);
            Params[858] = new VisualParam(858, "Skirt Length", 0, "skirt", String.Empty, "Short", "Long", 0.4f, 0.01f, 1f);
            Params[859] = new VisualParam(859, "Slit Front", 0, "skirt", String.Empty, "Open Front", "Closed Front", 1f, 0f, 1f);
            Params[860] = new VisualParam(860, "Slit Back", 0, "skirt", String.Empty, "Open Back", "Closed Back", 1f, 0f, 1f);
            Params[861] = new VisualParam(861, "Slit Left", 0, "skirt", String.Empty, "Open Left", "Closed Left", 1f, 0f, 1f);
            Params[862] = new VisualParam(862, "Slit Right", 0, "skirt", String.Empty, "Open Right", "Closed Right", 1f, 0f, 1f);
            Params[863] = new VisualParam(863, "skirt_looseness", 0, "skirt", "Skirt Fit", "Tight Skirt", "Poofy Skirt", 0.333f, 0f, 1f);
            Params[866] = new VisualParam(866, "skirt_tight", 1, "skirt", "tight skirt", "form fitting", "loose", 0f, 0f, 1f);
            Params[867] = new VisualParam(867, "skirt_smallbutt", 1, "skirt", "tight skirt", "form fitting", "loose", 0f, 0f, 1f);
            Params[868] = new VisualParam(868, "Shirt Wrinkles", 0, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[869] = new VisualParam(869, "Pants Wrinkles", 0, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[870] = new VisualParam(870, "Pointy_Eyebrows", 1, "hair", "Eyebrow Points", "Smooth", "Pointy", -0.5f, -0.5f, 1f);
            Params[871] = new VisualParam(871, "Lower_Eyebrows", 1, "hair", "Eyebrow Height", "Higher", "Lower", -2f, -2f, 2f);
            Params[872] = new VisualParam(872, "Arced_Eyebrows", 1, "hair", "Eyebrow Arc", "Flat", "Arced", 0f, 0f, 1f);
            Params[873] = new VisualParam(873, "Bump base", 1, "skin", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[874] = new VisualParam(874, "Bump upperdef", 1, "skin", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[877] = new VisualParam(877, "Jacket Wrinkles", 0, "jacket", "Jacket Wrinkles", "No Wrinkles", "Wrinkles", 0f, 0f, 1f);
            Params[878] = new VisualParam(878, "Bump upperdef", 1, "skin", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[879] = new VisualParam(879, "Male_Package", 0, "shape", "Package", "Coin Purse", "Duffle Bag", 0f, -0.5f, 2f);
            Params[880] = new VisualParam(880, "Eyelid_Inner_Corner_Up", 0, "shape", "Inner Eye Corner", "Corner Down", "Corner Up", -1.3f, -1.3f, 1.2f);
            Params[899] = new VisualParam(899, "Upper Clothes Shading", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[900] = new VisualParam(900, "Sleeve Length Shadow", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 0.87f);
            Params[901] = new VisualParam(901, "Shirt Shadow Bottom", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 1f);
            Params[902] = new VisualParam(902, "Collar Front Shadow Height", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 1f);
            Params[903] = new VisualParam(903, "Collar Back Shadow Height", 1, "shirt", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 1f);
            Params[913] = new VisualParam(913, "Lower Clothes Shading", 1, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[914] = new VisualParam(914, "Waist Height Shadow", 1, "pants", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 1f);
            Params[915] = new VisualParam(915, "Pants Length Shadow", 1, "pants", String.Empty, String.Empty, String.Empty, 0.02f, 0.02f, 1f);
            Params[921] = new VisualParam(921, "skirt_red", 0, "skirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[922] = new VisualParam(922, "skirt_green", 0, "skirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[923] = new VisualParam(923, "skirt_blue", 0, "skirt", String.Empty, String.Empty, String.Empty, 1f, 0f, 1f);
            Params[1000] = new VisualParam(1000, "Eyebrow Size Bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1001] = new VisualParam(1001, "Eyebrow Size", 1, "hair", String.Empty, String.Empty, String.Empty, 0.5f, 0f, 1f);
            Params[1002] = new VisualParam(1002, "Eyebrow Density Bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1003] = new VisualParam(1003, "Eyebrow Density", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1004] = new VisualParam(1004, "Sideburns bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1005] = new VisualParam(1005, "Sideburns", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1006] = new VisualParam(1006, "Moustache bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1007] = new VisualParam(1007, "Moustache", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1008] = new VisualParam(1008, "Soulpatch bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1009] = new VisualParam(1009, "Soulpatch", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1010] = new VisualParam(1010, "Chin Curtains bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1011] = new VisualParam(1011, "Chin Curtains", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1012] = new VisualParam(1012, "5 O'Clock Shadow bump", 1, "hair", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1013] = new VisualParam(1013, "Sleeve Length Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 0.85f);
            Params[1014] = new VisualParam(1014, "Shirt Bottom Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1015] = new VisualParam(1015, "Collar Front Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1016] = new VisualParam(1016, "Collar Back Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1017] = new VisualParam(1017, "Waist Height Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1018] = new VisualParam(1018, "Pants Length Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1019] = new VisualParam(1019, "Jacket Sleeve Length bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1020] = new VisualParam(1020, "jacket Sleeve Length", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1021] = new VisualParam(1021, "Jacket Collar Front bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1022] = new VisualParam(1022, "jacket Collar Front", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1023] = new VisualParam(1023, "Jacket Collar Back bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1024] = new VisualParam(1024, "jacket Collar Back", 1, null, String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1025] = new VisualParam(1025, "jacket bottom length upper bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1026] = new VisualParam(1026, "jacket open upper bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1027] = new VisualParam(1027, "jacket bottom length lower bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1028] = new VisualParam(1028, "jacket open lower bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1029] = new VisualParam(1029, "Sleeve Length Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 0.85f);
            Params[1030] = new VisualParam(1030, "Shirt Bottom Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1031] = new VisualParam(1031, "Collar Front Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1032] = new VisualParam(1032, "Collar Back Height Cloth", 1, "shirt", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1033] = new VisualParam(1033, "jacket bottom length lower bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1034] = new VisualParam(1034, "jacket open lower bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1035] = new VisualParam(1035, "Waist Height Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1036] = new VisualParam(1036, "Pants Length Cloth", 1, "pants", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1037] = new VisualParam(1037, "jacket bottom length upper bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1038] = new VisualParam(1038, "jacket open upper bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1039] = new VisualParam(1039, "Jacket Sleeve Length bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1040] = new VisualParam(1040, "Jacket Collar Front bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1041] = new VisualParam(1041, "Jacket Collar Back bump", 1, "jacket", String.Empty, String.Empty, String.Empty, 0f, 0f, 1f);
            Params[1042] = new VisualParam(1042, "Sleeve Length", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.4f, 0.01f, 1f);
            Params[1043] = new VisualParam(1043, "Sleeve Length bump", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.4f, 0.01f, 1f);
            Params[1044] = new VisualParam(1044, "Bottom", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[1045] = new VisualParam(1045, "Bottom bump", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[1046] = new VisualParam(1046, "Collar Front", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[1047] = new VisualParam(1047, "Collar Front bump", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[1048] = new VisualParam(1048, "Collar Back", 1, "undershirt", String.Empty, "Low", "High", 0.8f, 0f, 1f);
            Params[1049] = new VisualParam(1049, "Collar Back bump", 1, "undershirt", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[1050] = new VisualParam(1050, "Socks Length bump", 1, "socks", String.Empty, String.Empty, String.Empty, 0.35f, 0f, 1f);
            Params[1051] = new VisualParam(1051, "Socks Length bump", 1, "socks", String.Empty, String.Empty, String.Empty, 0.35f, 0f, 1f);
            Params[1052] = new VisualParam(1052, "Shoe Height", 1, "shoes", String.Empty, String.Empty, String.Empty, 0.1f, 0f, 1f);
            Params[1053] = new VisualParam(1053, "Shoe Height bump", 1, "shoes", String.Empty, String.Empty, String.Empty, 0.1f, 0f, 1f);
            Params[1054] = new VisualParam(1054, "Pants Length", 1, "underpants", String.Empty, String.Empty, String.Empty, 0.3f, 0f, 1f);
            Params[1055] = new VisualParam(1055, "Pants Length", 1, "underpants", String.Empty, String.Empty, String.Empty, 0.3f, 0f, 1f);
            Params[1056] = new VisualParam(1056, "Pants Waist", 1, "underpants", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[1057] = new VisualParam(1057, "Pants Waist", 1, "underpants", String.Empty, String.Empty, String.Empty, 0.8f, 0f, 1f);
            Params[1058] = new VisualParam(1058, "Glove Length", 1, "gloves", String.Empty, String.Empty, String.Empty, 0.8f, 0.01f, 1f);
            Params[1059] = new VisualParam(1059, "Glove Length bump", 1, "gloves", String.Empty, String.Empty, String.Empty, 0.8f, 0.01f, 1f);
            Params[1060] = new VisualParam(1060, "Glove Fingers", 1, "gloves", String.Empty, String.Empty, String.Empty, 1f, 0.01f, 1f);
            Params[1061] = new VisualParam(1061, "Glove Fingers bump", 1, "gloves", String.Empty, String.Empty, String.Empty, 1f, 0.01f, 1f);
        }
    }
}
