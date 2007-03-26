using System;
using System.Collections.Generic;

namespace libsecondlife
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
        public static VisualParam[] Params = new VisualParam[]
        {
            new VisualParam(1, "Big_Brow", 0, "shape", "Brow Size", "Small", "Large", -0.3f, -0.3f, 2f),
            new VisualParam(2, "Nose_Big_Out", 0, "shape", "Nose Size", "Small", "Large", -0.8f, -0.8f, 2.5f),
            new VisualParam(4, "Broad_Nostrils", 0, "shape", "Nostril Width", "Narrow", "Broad", -0.5f, -0.5f, 1f),
            new VisualParam(5, "Cleft_Chin", 0, "shape", "Chin Cleft", "Round", "Cleft", -0.1f, -0.1f, 1f),
            new VisualParam(6, "Bulbous_Nose_Tip", 0, "shape", "Nose Tip Shape", "Pointy", "Bulbous", -0.3f, -0.3f, 1f),
            new VisualParam(7, "Weak_Chin", 0, "shape", "Chin Angle", "Chin Out", "Chin In", -0.5f, -0.5f, 0.5f),
            new VisualParam(8, "Double_Chin", 0, "shape", "Chin-Neck", "Tight Chin", "Double Chin", -0.5f, -0.5f, 1.5f),
            new VisualParam(10, "Sunken_Cheeks", 0, "shape", "Lower Cheeks", "Well-Fed", "Sunken", -1.5f, -1.5f, 3f),
            new VisualParam(11, "Noble_Nose_Bridge", 0, "shape", "Upper Bridge", "Low", "High", -0.5f, -0.5f, 1.5f),
            new VisualParam(12, "Jowls", 0, "shape", "", "Less", "More", -0.5f, -0.5f, 2.5f),
            new VisualParam(13, "Cleft_Chin_Upper", 0, "shape", "Upper Chin Cleft", "Round", "Cleft", 0f, 0f, 1.5f),
            new VisualParam(14, "High_Cheek_Bones", 0, "shape", "Cheek Bones", "Low", "High", -0.5f, -0.5f, 1f),
            new VisualParam(15, "Ears_Out", 0, "shape", "Ear Angle", "In", "Out", -0.5f, -0.5f, 1.5f),
            new VisualParam(16, "Pointy_Eyebrows", 0, "hair", "Eyebrow Points", "Smooth", "Pointy", -0.5f, -0.5f, 3f),
            new VisualParam(17, "Square_Jaw", 0, "shape", "Jaw Shape", "Pointy", "Square", -0.5f, -0.5f, 1f),
            new VisualParam(18, "Puffy_Upper_Cheeks", 0, "shape", "Upper Cheeks", "Thin", "Puffy", -1.5f, -1.5f, 2.5f),
            new VisualParam(19, "Upturned_Nose_Tip", 0, "shape", "Nose Tip Angle", "Downturned", "Upturned", -1.5f, -1.5f, 1f),
            new VisualParam(20, "Bulbous_Nose", 0, "shape", "Nose Thickness", "Thin Nose", "Bulbous Nose", -0.5f, -0.5f, 1.5f),
            new VisualParam(21, "Upper_Eyelid_Fold", 0, "shape", "Upper Eyelid Fold", "Uncreased", "Creased", -0.2f, -0.2f, 1.3f),
            new VisualParam(22, "Attached_Earlobes", 0, "shape", "Attached Earlobes", "Unattached", "Attached", 0f, 0f, 1f),
            new VisualParam(23, "Baggy_Eyes", 0, "shape", "Eye Bags", "Smooth", "Baggy", -0.5f, -0.5f, 1.5f),
            new VisualParam(24, "Wide_Eyes", 0, "shape", "Eye Opening", "Narrow", "Wide", -1.5f, -1.5f, 2f),
            new VisualParam(25, "Wide_Lip_Cleft", 0, "shape", "Lip Cleft", "Narrow", "Wide", -0.8f, -0.8f, 1.5f),
            new VisualParam(27, "Wide_Nose_Bridge", 0, "shape", "Bridge Width", "Narrow", "Wide", -1.3f, -1.3f, 1.2f),
            new VisualParam(31, "Arced_Eyebrows", 0, "hair", "Eyebrow Arc", "Flat", "Arced", 0.5f, 0f, 2f),
            new VisualParam(33, "Height", 0, "shape", "Height", "Short", "Tall", -2.3f, -2.3f, 2f),
            new VisualParam(34, "Thickness", 0, "shape", "Body Thickness", "Body Thin", "Body Thick", -0.7f, -0.7f, 1.5f),
            new VisualParam(35, "Big_Ears", 0, "shape", "Ear Size", "Small", "Large", -1f, -1f, 2f),
            new VisualParam(36, "Shoulders", 0, "shape", "Shoulders", "Narrow", "Broad", -0.5f, -1.8f, 1.4f),
            new VisualParam(37, "Hip Width", 0, "shape", "Hip Width", "Narrow", "Wide", -3.2f, -3.2f, 2.8f),
            new VisualParam(38, "Torso Length", 0, "shape", "", "Short Torso", "Long Torso", -1f, -1f, 1f),
            new VisualParam(80, "male", 0, "shape", "", "", "", 0f, 0f, 1f),
            new VisualParam(93, "Glove Length", 0, "gloves", "", "Short", "Long", 0.8f, 0.01f, 1f),
            new VisualParam(98, "Eye Lightness", 0, "eyes", "", "Darker", "Lighter", 0f, 0f, 1f),
            new VisualParam(99, "Eye Color", 0, "eyes", "", "Natural", "Unnatural", 0f, 0f, 1f),
            new VisualParam(105, "Breast Size", 0, "shape", "", "Small", "Large", 0.5f, 0f, 1f),
            new VisualParam(108, "Rainbow Color", 0, "skin", "", "None", "Wild", 0f, 0f, 1f),
            new VisualParam(110, "Red Skin", 0, "skin", "Ruddiness", "Pale", "Ruddy", 0f, 0f, 0.1f),
            new VisualParam(111, "Pigment", 0, "skin", "", "Light", "Dark", 0.5f, 0f, 1f),
            new VisualParam(112, "Rainbow Color", 0, "hair", "", "None", "Wild", 0f, 0f, 1f),
            new VisualParam(113, "Red Hair", 0, "hair", "", "No Red", "Very Red", 0f, 0f, 1f),
            new VisualParam(114, "Blonde Hair", 0, "hair", "", "Black", "Blonde", 0.5f, 0f, 1f),
            new VisualParam(115, "White Hair", 0, "hair", "", "No White", "All White", 0f, 0f, 1f),
            new VisualParam(116, "Rosy Complexion", 0, "skin", "", "Less Rosy", "More Rosy", 0f, 0f, 1f),
            new VisualParam(117, "Lip Pinkness", 0, "skin", "", "Darker", "Pinker", 0f, 0f, 1f),
            new VisualParam(119, "Eyebrow Size", 0, "hair", "", "Thin Eyebrows", "Bushy Eyebrows", 0.5f, 0f, 1f),
            new VisualParam(130, "Front Fringe", 0, "hair", "", "Short", "Long", 0.45f, 0f, 1f),
            new VisualParam(131, "Side Fringe", 0, "hair", "", "Short", "Long", 0.5f, 0f, 1f),
            new VisualParam(132, "Back Fringe", 0, "hair", "", "Short", "Long", 0.39f, 0f, 1f),
            new VisualParam(133, "Hair Front", 0, "hair", "", "Short", "Long", 0.25f, 0f, 1f),
            new VisualParam(134, "Hair Sides", 0, "hair", "", "Short", "Long", 0.5f, 0f, 1f),
            new VisualParam(135, "Hair Back", 0, "hair", "", "Short", "Long", 0.55f, 0f, 1f),
            new VisualParam(136, "Hair Sweep", 0, "hair", "", "Sweep Forward", "Sweep Back", 0.5f, 0f, 1f),
            new VisualParam(137, "Hair Tilt", 0, "hair", "", "Left", "Right", 0.5f, 0f, 1f),
            new VisualParam(140, "Hair_Part_Middle", 0, "hair", "Middle Part", "No Part", "Part", 0f, 0f, 2f),
            new VisualParam(141, "Hair_Part_Right", 0, "hair", "Right Part", "No Part", "Part", 0f, 0f, 2f),
            new VisualParam(142, "Hair_Part_Left", 0, "hair", "Left Part", "No Part", "Part", 0f, 0f, 2f),
            new VisualParam(143, "Hair_Sides_Full", 0, "hair", "Full Hair Sides", "Mowhawk", "Full Sides", 0.125f, -4f, 1.5f),
            new VisualParam(150, "Body Definition", 0, "skin", "", "Less", "More", 0f, 0f, 1f),
            new VisualParam(155, "Lip Width", 0, "shape", "Lip Width", "Narrow Lips", "Wide Lips", 0f, -0.9f, 1.3f),
            new VisualParam(157, "Belly Size", 0, "shape", "", "Small", "Big", 0f, 0f, 1f),
            new VisualParam(162, "Facial Definition", 0, "skin", "", "Less", "More", 0f, 0f, 1f),
            new VisualParam(163, "wrinkles", 0, "skin", "", "Less", "More", 0f, 0f, 1f),
            new VisualParam(165, "Freckles", 0, "skin", "", "Less", "More", 0f, 0f, 1f),
            new VisualParam(166, "Sideburns", 0, "hair", "", "Short Sideburns", "Mutton Chops", 0f, 0f, 1f),
            new VisualParam(167, "Moustache", 0, "hair", "", "Chaplin", "Handlebars", 0f, 0f, 1f),
            new VisualParam(168, "Soulpatch", 0, "hair", "", "Less soul", "More soul", 0f, 0f, 1f),
            new VisualParam(169, "Chin Curtains", 0, "hair", "", "Less Curtains", "More Curtains", 0f, 0f, 1f),
            new VisualParam(177, "Hair_Rumpled", 0, "hair", "Rumpled Hair", "Smooth Hair", "Rumpled Hair", 0f, 0f, 1f),
            new VisualParam(181, "Hair_Big_Front", 0, "hair", "Big Hair Front", "Less", "More", 0.14f, -1f, 1f),
            new VisualParam(182, "Hair_Big_Top", 0, "hair", "Big Hair Top", "Less", "More", 0.7f, -1f, 1f),
            new VisualParam(183, "Hair_Big_Back", 0, "hair", "Big Hair Back", "Less", "More", 0.05f, -1f, 1f),
            new VisualParam(184, "Hair_Spiked", 0, "hair", "Spiked Hair", "No Spikes", "Big Spikes", 0f, 0f, 1f),
            new VisualParam(185, "Deep_Chin", 0, "shape", "Chin Depth", "Shallow", "Deep", -1f, -1f, 1f),
            new VisualParam(192, "Bangs_Part_Middle", 0, "hair", "Part Bangs", "No Part", "Part Bangs", 0f, 0f, 1f),
            new VisualParam(193, "Head Shape", 0, "shape", "Head Shape", "More Square", "More Round", 0.5f, 0f, 1f),
            new VisualParam(196, "Eye Spacing", 0, "shape", "Eye Spacing", "Close Set Eyes", "Far Set Eyes", 0f, -2f, 1f),
            new VisualParam(198, "Heel Height", 0, "shoes", "", "Low Heels", "High Heels", 0f, 0f, 1f),
            new VisualParam(503, "Platform Height", 0, "shoes", "", "Low Platforms", "High Platforms", 0f, 0f, 1f),
            new VisualParam(505, "Lip Thickness", 0, "shape", "", "Thin Lips", "Fat Lips", 0.5f, 0f, 1f),
            new VisualParam(506, "Mouth_Height", 0, "shape", "Mouth Position", "High", "Low", -2f, -2f, 2f),
            new VisualParam(507, "Breast_Gravity", 0, "shape", "Breast Buoyancy", "Less Gravity", "More Gravity", 0f, -1.5f, 2f),
            new VisualParam(508, "Shoe_Platform_Width", 0, "shoes", "Platform Width", "Narrow", "Wide", -1f, -1f, 2f),
            new VisualParam(513, "Heel Shape", 0, "shoes", "", "Pointy Heels", "Thick Heels", 0.5f, 0f, 1f),
            new VisualParam(514, "Toe Shape", 0, "shoes", "", "Pointy", "Square", 0.5f, 0f, 1f),
            new VisualParam(515, "Foot_Size", 0, "shape", "Foot Size", "Small", "Big", -1f, -1f, 3f),
            new VisualParam(517, "Wide_Nose", 0, "shape", "Nose Width", "Narrow", "Wide", -0.5f, -0.5f, 1f),
            new VisualParam(518, "Eyelashes_Long", 0, "shape", "Eyelash Length", "Short", "Long", -0.3f, -0.3f, 1.5f),
            new VisualParam(603, "Sleeve Length", 0, "undershirt", "", "Short", "Long", 0.4f, 0.01f, 1f),
            new VisualParam(604, "Bottom", 0, "undershirt", "", "Short", "Long", 0.85f, 0f, 1f),
            new VisualParam(605, "Collar Front", 0, "undershirt", "", "Low", "High", 0.84f, 0f, 1f),
            new VisualParam(606, "Sleeve Length", 0, "jacket", "", "Short", "Long", 0.8f, 0f, 1f),
            new VisualParam(607, "Collar Front", 0, "jacket", "", "Low", "High", 0.8f, 0f, 1f),
            new VisualParam(608, "bottom length lower", 0, "jacket", "Jacket Length", "Short", "Long", 0.8f, 0f, 1f),
            new VisualParam(609, "open jacket", 0, "jacket", "Open Front", "Open", "Closed", 0.2f, 0f, 1f),
            new VisualParam(616, "Shoe Height", 0, "shoes", "", "Short", "Tall", 0.1f, 0f, 1f),
            new VisualParam(617, "Socks Length", 0, "socks", "", "Short", "Long", 0.35f, 0f, 1f),
            new VisualParam(619, "Pants Length", 0, "underpants", "", "Short", "Long", 0.3f, 0f, 1f),
            new VisualParam(624, "Pants Waist", 0, "underpants", "", "Low", "High", 0.8f, 0f, 1f),
            new VisualParam(625, "Leg_Pantflair", 0, "pants", "Cuff Flare", "Tight Cuffs", "Flared Cuffs", 0f, 0f, 1.5f),
            new VisualParam(629, "Forehead Angle", 0, "shape", "", "More Vertical", "More Sloped", 0.5f, 0f, 1f),
            new VisualParam(637, "Body Fat", 0, "shape", "", "Less Body Fat", "More Body Fat", 0f, 0f, 1f),
            new VisualParam(638, "Low_Crotch", 0, "pants", "Pants Crotch", "High and Tight", "Low and Loose", 0f, 0f, 1.3f),
            new VisualParam(646, "Egg_Head", 0, "shape", "Egg Head", "Chin Heavy", "Forehead Heavy", 0f, -1.3f, 1f),
            new VisualParam(647, "Squash_Stretch_Head", 0, "shape", "Head Stretch", "Squash Head", "Stretch Head", 0f, -0.5f, 1f),
            new VisualParam(649, "Torso Muscles", 0, "shape", "Torso Muscles", "Less Muscular", "More Muscular", 0.5f, 0f, 1f),
            new VisualParam(650, "Eyelid_Corner_Up", 0, "shape", "Outer Eye Corner", "Corner Down", "Corner Up", -1.3f, -1.3f, 1.2f),
            new VisualParam(652, "Leg Muscles", 0, "shape", "", "Less Muscular", "More Muscular", 0.5f, 0f, 1f),
            new VisualParam(653, "Tall_Lips", 0, "shape", "Lip Fullness", "Less Full", "More Full", -1f, -1f, 2f),
            new VisualParam(654, "Shoe_Toe_Thick", 0, "shoes", "Toe Thickness", "Flat Toe", "Thick Toe", 0f, 0f, 2f),
            new VisualParam(656, "Crooked_Nose", 0, "shape", "Crooked Nose", "Nose Left", "Nose Right", -2f, -2f, 2f),
            new VisualParam(659, "Mouth Corner", 0, "shape", "", "Corner Down", "Corner Up", 0.5f, 0f, 1f),
            new VisualParam(662, "Face Shear", 0, "shape", "", "Shear Right Up", "Shear Left Up", 0.5f, 0f, 1f),
            new VisualParam(663, "Shift_Mouth", 0, "shape", "Shift Mouth", "Shift Left", "Shift Right", 0f, -2f, 2f),
            new VisualParam(664, "Pop_Eye", 0, "shape", "Eye Pop", "Pop Right Eye", "Pop Left Eye", 0f, -1.3f, 1.3f),
            new VisualParam(665, "Jaw_Jut", 0, "shape", "Jaw Jut", "Overbite", "Underbite", 0f, -2f, 2f),
            new VisualParam(674, "Hair_Shear_Back", 0, "hair", "Shear Back", "Full Back", "Sheared Back", -0.3f, -1f, 2f),
            new VisualParam(675, "Hand Size", 0, "shape", "", "Small Hands", "Large Hands", -0.3f, -0.3f, 0.3f),
            new VisualParam(676, "Love_Handles", 0, "shape", "Love Handles", "Less Love", "More Love", 0f, -1f, 2f),
            new VisualParam(678, "Torso Muscles", 0, "shape", "", "Less Muscular", "More Muscular", 0.5f, 0f, 1f),
            new VisualParam(682, "Head Size", 0, "shape", "Head Size", "Small Head", "Big Head", 0.5f, 0f, 1f),
            new VisualParam(683, "Neck Thickness", 0, "shape", "", "Skinny Neck", "Thick Neck", -0.15f, -0.4f, 0.2f),
            new VisualParam(684, "Breast_Female_Cleavage", 0, "shape", "Breast Cleavage", "Separate", "Join", 0f, -0.3f, 1.3f),
            new VisualParam(685, "Chest_Male_No_Pecs", 0, "shape", "Pectorals", "Big Pectorals", "Sunken Chest", 0f, -0.5f, 1.1f),
            new VisualParam(690, "Eye Size", 0, "shape", "Eye Size", "Beady Eyes", "Anime Eyes", 0.5f, 0f, 1f),
            new VisualParam(692, "Leg Length", 0, "shape", "", "Short Legs", "Long Legs", -1f, -1f, 1f),
            new VisualParam(693, "Arm Length", 0, "shape", "", "Short Arms", "Long arms", 0.6f, -1f, 1f),
            new VisualParam(700, "Lipstick Color", 0, "skin", "", "Pink", "Black", 0.25f, 0f, 1f),
            new VisualParam(701, "Lipstick", 0, "skin", "", "No Lipstick", "More Lipstick", 0f, 0f, 0.9f),
            new VisualParam(702, "Lipgloss", 0, "skin", "", "No Lipgloss", "Glossy", 0f, 0f, 1f),
            new VisualParam(703, "Eyeliner", 0, "skin", "", "No Eyeliner", "Full Eyeliner", 0f, 0f, 1f),
            new VisualParam(704, "Blush", 0, "skin", "", "No Blush", "More Blush", 0f, 0f, 0.9f),
            new VisualParam(705, "Blush Color", 0, "skin", "", "Pink", "Orange", 0.5f, 0f, 1f),
            new VisualParam(706, "Out Shdw Opacity", 0, "skin", "", "Clear", "Opaque", 0.6f, 0.2f, 1f),
            new VisualParam(707, "Outer Shadow", 0, "skin", "", "No Eyeshadow", "More Eyeshadow", 0f, 0f, 0.7f),
            new VisualParam(708, "Out Shdw Color", 0, "skin", "", "Light", "Dark", 0f, 0f, 1f),
            new VisualParam(709, "Inner Shadow", 0, "skin", "", "No Eyeshadow", "More Eyeshadow", 0f, 0f, 1f),
            new VisualParam(710, "Nail Polish", 0, "skin", "", "No Polish", "Painted Nails", 0f, 0f, 1f),
            new VisualParam(711, "Blush Opacity", 0, "skin", "", "Clear", "Opaque", 0.5f, 0f, 1f),
            new VisualParam(712, "In Shdw Color", 0, "skin", "", "Light", "Dark", 0f, 0f, 1f),
            new VisualParam(713, "In Shdw Opacity", 0, "skin", "", "Clear", "Opaque", 0.7f, 0.2f, 1f),
            new VisualParam(714, "Eyeliner Color", 0, "skin", "", "Dark Green", "Black", 0f, 0f, 1f),
            new VisualParam(715, "Nail Polish Color", 0, "skin", "", "Pink", "Black", 0f, 0f, 1f),
            new VisualParam(750, "Eyebrow Density", 0, "hair", "", "Sparse", "Dense", 0.7f, 0f, 1f),
            new VisualParam(752, "Hair Thickness", 0, "hair", "", "5 O'Clock Shadow", "Bushy Hair", 0.5f, 0f, 1f),
            new VisualParam(753, "Saddlebags", 0, "shape", "Saddle Bags", "Less Saddle", "More Saddle", 0f, -0.5f, 3f),
            new VisualParam(754, "Hair_Taper_Back", 0, "hair", "Taper Back", "Wide Back", "Narrow Back", 0f, -1f, 2f),
            new VisualParam(755, "Hair_Taper_Front", 0, "hair", "Taper Front", "Wide Front", "Narrow Front", 0.05f, -1.5f, 1.5f),
            new VisualParam(756, "Neck Length", 0, "shape", "", "Short Neck", "Long Neck", 0f, -1f, 1f),
            new VisualParam(757, "Lower_Eyebrows", 0, "hair", "Eyebrow Height", "Higher", "Lower", -1f, -4f, 2f),
            new VisualParam(758, "Lower_Bridge_Nose", 0, "shape", "Lower Bridge", "Low", "High", -1.5f, -1.5f, 1.5f),
            new VisualParam(759, "Low_Septum_Nose", 0, "shape", "Nostril Division", "High", "Low", 0.5f, -1f, 1.5f),
            new VisualParam(760, "Jaw_Angle", 0, "shape", "Jaw Angle", "Low Jaw", "High Jaw", 0f, -1.2f, 2f),
            new VisualParam(762, "Hair_Shear_Front", 0, "hair", "Shear Front", "Full Front", "Sheared Front", 0f, 0f, 3f),
            new VisualParam(763, "Hair Volume", 0, "hair", "", "Less Volume", "More Volume", 0.55f, 0f, 1f),
            new VisualParam(764, "Lip_Cleft_Deep", 0, "shape", "Lip Cleft Depth", "Shallow", "Deep", -0.5f, -0.5f, 1.2f),
            new VisualParam(765, "Puffy_Lower_Lids", 0, "shape", "Puffy Eyelids", "Flat", "Puffy", -0.3f, -0.3f, 2.5f),
            new VisualParam(769, "Eye Depth", 0, "shape", "", "Sunken Eyes", "Bugged Eyes", 0.5f, 0f, 1f),
            new VisualParam(773, "Head Length", 0, "shape", "", "Flat Head", "Long Head", 0.5f, 0f, 1f),
            new VisualParam(775, "Body Freckles", 0, "skin", "", "Less Freckles", "More Freckles", 0f, 0f, 1f),
            new VisualParam(779, "Collar Back", 0, "undershirt", "", "Low", "High", 0.84f, 0f, 1f),
            new VisualParam(780, "Collar Back", 0, "jacket", "", "Low", "High", 0.8f, 0f, 1f),
            new VisualParam(781, "Collar Back", 0, "shirt", "", "Low", "High", 0.78f, 0f, 1f),
            new VisualParam(785, "Pigtails", 0, "hair", "", "Short Pigtails", "Long Pigtails", 0f, 0f, 1f),
            new VisualParam(789, "Ponytail", 0, "hair", "", "Short Ponytail", "Long Ponytail", 0f, 0f, 1f),
            new VisualParam(795, "Butt Size", 0, "shape", "Butt Size", "Flat Butt", "Big Butt", 0.25f, 0f, 1f),
            new VisualParam(796, "Pointy_Ears", 0, "shape", "Ear Tips", "Flat", "Pointy", -0.4f, -0.4f, 3f),
            new VisualParam(799, "Lip Ratio", 0, "shape", "Lip Ratio", "More Upper Lip", "More Lower Lip", 0.5f, 0f, 1f),
            new VisualParam(800, "Sleeve Length", 0, "shirt", "", "Short", "Long", 0.89f, 0f, 1f),
            new VisualParam(801, "Shirt Bottom", 0, "shirt", "", "Short", "Long", 1f, 0f, 1f),
            new VisualParam(802, "Collar Front", 0, "shirt", "", "Low", "High", 0.78f, 0f, 1f),
            new VisualParam(803, "shirt_red", 0, "shirt", "", "", "", 1f, 0f, 1f),
            new VisualParam(804, "shirt_green", 0, "shirt", "", "", "", 1f, 0f, 1f),
            new VisualParam(805, "shirt_blue", 0, "shirt", "", "", "", 1f, 0f, 1f),
            new VisualParam(806, "pants_red", 0, "pants", "", "", "", 1f, 0f, 1f),
            new VisualParam(807, "pants_green", 0, "pants", "", "", "", 1f, 0f, 1f),
            new VisualParam(808, "pants_blue", 0, "pants", "", "", "", 1f, 0f, 1f),
            new VisualParam(812, "shoes_red", 0, "shoes", "", "", "", 1f, 0f, 1f),
            new VisualParam(813, "shoes_green", 0, "shoes", "", "", "", 1f, 0f, 1f),
            new VisualParam(814, "Waist Height", 0, "pants", "", "Low", "High", 1f, 0f, 1f),
            new VisualParam(815, "Pants Length", 0, "pants", "", "Short", "Long", 0.8f, 0f, 1f),
            new VisualParam(816, "Loose Lower Clothing", 0, "pants", "Pants Fit", "Tight Pants", "Loose Pants", 0f, 0f, 1f),
            new VisualParam(817, "shoes_blue", 0, "shoes", "", "", "", 1f, 0f, 1f),
            new VisualParam(818, "socks_red", 0, "socks", "", "", "", 1f, 0f, 1f),
            new VisualParam(819, "socks_green", 0, "socks", "", "", "", 1f, 0f, 1f),
            new VisualParam(820, "socks_blue", 0, "socks", "", "", "", 1f, 0f, 1f),
            new VisualParam(821, "undershirt_red", 0, "undershirt", "", "", "", 1f, 0f, 1f),
            new VisualParam(822, "undershirt_green", 0, "undershirt", "", "", "", 1f, 0f, 1f),
            new VisualParam(823, "undershirt_blue", 0, "undershirt", "", "", "", 1f, 0f, 1f),
            new VisualParam(824, "underpants_red", 0, "underpants", "", "", "", 1f, 0f, 1f),
            new VisualParam(825, "underpants_green", 0, "underpants", "", "", "", 1f, 0f, 1f),
            new VisualParam(826, "underpants_blue", 0, "underpants", "", "", "", 1f, 0f, 1f),
            new VisualParam(827, "gloves_red", 0, "gloves", "", "", "", 1f, 0f, 1f),
            new VisualParam(828, "Loose Upper Clothing", 0, "shirt", "Shirt Fit", "Tight Shirt", "Loose Shirt", 0f, 0f, 1f),
            new VisualParam(829, "gloves_green", 0, "gloves", "", "", "", 1f, 0f, 1f),
            new VisualParam(830, "gloves_blue", 0, "gloves", "", "", "", 1f, 0f, 1f),
            new VisualParam(834, "jacket_red", 0, "jacket", "", "", "", 1f, 0f, 1f),
            new VisualParam(835, "jacket_green", 0, "jacket", "", "", "", 1f, 0f, 1f),
            new VisualParam(836, "jacket_blue", 0, "jacket", "", "", "", 1f, 0f, 1f),
            new VisualParam(840, "Shirtsleeve_flair", 0, "shirt", "Sleeve Looseness", "Tight Sleeves", "Loose Sleeves", 0f, 0f, 1.5f),
            new VisualParam(841, "Bowed_Legs", 0, "shape", "Knee Angle", "Knock Kneed", "Bow Legged", 0f, -1f, 1f),
            new VisualParam(842, "Hip Length", 0, "shape", "", "Short hips", "Long Hips", -1f, -1f, 1f),
            new VisualParam(844, "Glove Fingers", 0, "gloves", "", "Fingerless", "Fingers", 1f, 0.01f, 1f),
            new VisualParam(848, "skirt_bustle", 0, "skirt", "bustle skirt", "no bustle", "more bustle", 0.2f, 0f, 2f),
            new VisualParam(858, "Skirt Length", 0, "skirt", "", "Short", "Long", 0.4f, 0.01f, 1f),
            new VisualParam(859, "Slit Front", 0, "skirt", "", "Open Front", "Closed Front", 1f, 0f, 1f),
            new VisualParam(860, "Slit Back", 0, "skirt", "", "Open Back", "Closed Back", 1f, 0f, 1f),
            new VisualParam(861, "Slit Left", 0, "skirt", "", "Open Left", "Closed Left", 1f, 0f, 1f),
            new VisualParam(862, "Slit Right", 0, "skirt", "", "Open Right", "Closed Right", 1f, 0f, 1f),
            new VisualParam(863, "skirt_looseness", 0, "skirt", "Skirt Fit", "Tight Skirt", "Poofy Skirt", 0.333f, 0f, 1f),
            new VisualParam(868, "Shirt Wrinkles", 0, "shirt", "", "", "", 0f, 0f, 1f),
            new VisualParam(869, "Pants Wrinkles", 0, "pants", "", "", "", 0f, 0f, 1f),
            new VisualParam(877, "Jacket Wrinkles", 0, "jacket", "Jacket Wrinkles", "No Wrinkles", "Wrinkles", 0f, 0f, 1f),
            new VisualParam(879, "Male_Package", 0, "shape", "Package", "Coin Purse", "Duffle Bag", 0f, -0.5f, 2f),
            new VisualParam(880, "Eyelid_Inner_Corner_Up", 0, "shape", "Inner Eye Corner", "Corner Down", "Corner Up", -1.3f, -1.3f, 1.2f),
            new VisualParam(921, "skirt_red", 0, "skirt", "", "", "", 1f, 0f, 1f),
            new VisualParam(922, "skirt_green", 0, "skirt", "", "", "", 1f, 0f, 1f),
            new VisualParam(923, "skirt_blue", 0, "skirt", "", "", "", 1f, 0f, 1f),
        };
    }
}
