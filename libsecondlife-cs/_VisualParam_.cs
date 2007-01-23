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
        private static Dictionary<int, VisualParam> paramsDict = new Dictionary<int,VisualParam>();

        public static Dictionary<int, VisualParam> Params
        {
            get
            {
                if (paramsDict.Count == 0)
                {
                    paramsDict.Add(32, new VisualParam(32, "Male_Skeleton", 1, "shape", "", "Female", "Male", 0f, 0f, 1f));
                    paramsDict.Add(33, new VisualParam(33, "Height", 0, "shape", "Height", "Short", "Tall", -2.3f, -2.3f, 2f));
                    paramsDict.Add(34, new VisualParam(34, "Thickness", 0, "shape", "Body Thickness", "Body Thin", "Body Thick", -0.7f, -0.7f, 1.5f));
                    paramsDict.Add(36, new VisualParam(36, "Shoulders", 0, "shape", "Shoulders", "Narrow", "Broad", -0.5f, -1.8f, 1.4f));
                    paramsDict.Add(37, new VisualParam(37, "Hip Width", 0, "shape", "Hip Width", "Narrow", "Wide", -3.2f, -3.2f, 2.8f));
                    paramsDict.Add(842, new VisualParam(842, "Hip Length", 0, "shape", "", "Short hips", "Long Hips", -1f, -1f, 1f));
                    paramsDict.Add(38, new VisualParam(38, "Torso Length", 0, "shape", "", "Short Torso", "Long Torso", -1f, -1f, 1f));
                    paramsDict.Add(195, new VisualParam(195, "EyeBone_Spread", 1, "shape", "", "Eyes Together", "Eyes Spread", -1f, -1f, 1f));
                    paramsDict.Add(661, new VisualParam(661, "EyeBone_Head_Shear", 1, "shape", "", "Eyes Shear Left Up", "Eyes Shear Right Up", -2f, -2f, 2f));
                    paramsDict.Add(772, new VisualParam(772, "EyeBone_Head_Elongate", 1, "shape", "", "Eyes Short Head", "Eyes Long Head", -1f, -1f, 1f));
                    paramsDict.Add(768, new VisualParam(768, "EyeBone_Bug", 1, "shape", "", "Eyes Sunken", "Eyes Bugged", -2f, -2f, 2f));
                    paramsDict.Add(655, new VisualParam(655, "Head Size", 1, "shape", "Head Size", "Small Head", "Big Head", -0.25f, -0.25f, 0.1f));
                    paramsDict.Add(197, new VisualParam(197, "Shoe_Heels", 1, "shoes", "", "No Heels", "High Heels", 0f, 0f, 1f));
                    paramsDict.Add(502, new VisualParam(502, "Shoe_Platform", 1, "shoes", "", "No Heels", "High Heels", 0f, 0f, 1f));
                    paramsDict.Add(675, new VisualParam(675, "Hand Size", 0, "shape", "", "Small Hands", "Large Hands", -0.3f, -0.3f, 0.3f));
                    paramsDict.Add(683, new VisualParam(683, "Neck Thickness", 0, "shape", "", "Skinny Neck", "Thick Neck", -0.15f, -0.4f, 0.2f));
                    paramsDict.Add(689, new VisualParam(689, "EyeBone_Big_Eyes", 1, "shape", "", "Eyes Back", "Eyes Forward", -1f, -1f, 1f));
                    paramsDict.Add(692, new VisualParam(692, "Leg Length", 0, "shape", "", "Short Legs", "Long Legs", -1f, -1f, 1f));
                    paramsDict.Add(693, new VisualParam(693, "Arm Length", 0, "shape", "", "Short Arms", "Long arms", 0.6f, -1f, 1f));
                    paramsDict.Add(756, new VisualParam(756, "Neck Length", 0, "shape", "", "Short Neck", "Long Neck", 0f, -1f, 1f));
                    paramsDict.Add(180, new VisualParam(180, "Hair_Volume", 1, "hair", "Hair Volume", "Less", "More", 0f, 0f, 1.3f));
                    paramsDict.Add(761, new VisualParam(761, "Hair_Volume_Small", 1, "hair", "Hair Volume", "Less", "More", 0f, 0f, 1.3f));
                    paramsDict.Add(181, new VisualParam(181, "Hair_Big_Front", 0, "hair", "Big Hair Front", "Less", "More", 0.14f, -1f, 1f));
                    paramsDict.Add(182, new VisualParam(182, "Hair_Big_Top", 0, "hair", "Big Hair Top", "Less", "More", 0.7f, -1f, 1f));
                    paramsDict.Add(183, new VisualParam(183, "Hair_Big_Back", 0, "hair", "Big Hair Back", "Less", "More", 0.05f, -1f, 1f));
                    paramsDict.Add(184, new VisualParam(184, "Hair_Spiked", 0, "hair", "Spiked Hair", "No Spikes", "Big Spikes", 0f, 0f, 1f));
                    paramsDict.Add(140, new VisualParam(140, "Hair_Part_Middle", 0, "hair", "Middle Part", "No Part", "Part", 0f, 0f, 2f));
                    paramsDict.Add(141, new VisualParam(141, "Hair_Part_Right", 0, "hair", "Right Part", "No Part", "Part", 0f, 0f, 2f));
                    paramsDict.Add(142, new VisualParam(142, "Hair_Part_Left", 0, "hair", "Left Part", "No Part", "Part", 0f, 0f, 2f));
                    paramsDict.Add(143, new VisualParam(143, "Hair_Sides_Full", 0, "hair", "Full Hair Sides", "Mowhawk", "Full Sides", 0.125f, -4f, 1.5f));
                    paramsDict.Add(144, new VisualParam(144, "Bangs_Front_Up", 1, "hair", "Front Bangs Up", "Bangs", "Bangs Up", 0f, 0f, 1f));
                    paramsDict.Add(145, new VisualParam(145, "Bangs_Front_Down", 1, "hair", "Front Bangs Down", "Bangs", "Bangs Down", 0f, 0f, 5f));
                    paramsDict.Add(146, new VisualParam(146, "Bangs_Sides_Up", 1, "hair", "Side Bangs Up", "Side Bangs", "Side Bangs Up", 0f, 0f, 1f));
                    paramsDict.Add(147, new VisualParam(147, "Bangs_Sides_Down", 1, "hair", "Side Bangs Down", "Side Bangs", "Side Bangs Down", 0f, 0f, 2f));
                    paramsDict.Add(148, new VisualParam(148, "Bangs_Back_Up", 1, "hair", "Back Bangs Up", "Back Bangs", "Back Bangs Up", 0f, 0f, 1f));
                    paramsDict.Add(149, new VisualParam(149, "Bangs_Back_Down", 1, "hair", "Back Bangs Down", "Back Bangs", "Back Bangs Down", 0f, 0f, 2f));
                    paramsDict.Add(171, new VisualParam(171, "Hair_Front_Down", 1, "hair", "Front Hair Down", "Front Hair", "Front Hair Down", 0f, 0f, 1f));
                    paramsDict.Add(172, new VisualParam(172, "Hair_Front_Up", 1, "hair", "Front Hair Up", "Front Hair", "Front Hair Up", 0f, 0f, 1f));
                    paramsDict.Add(173, new VisualParam(173, "Hair_Sides_Down", 1, "hair", "Sides Hair Down", "Sides Hair", "Sides Hair Down", 0f, 0f, 1f));
                    paramsDict.Add(174, new VisualParam(174, "Hair_Sides_Up", 1, "hair", "Sides Hair Up", "Sides Hair", "Sides Hair Up", 0f, 0f, 1f));
                    paramsDict.Add(175, new VisualParam(175, "Hair_Back_Down", 1, "hair", "Back Hair Down", "Back Hair", "Back Hair Down", 0f, 0f, 3f));
                    paramsDict.Add(176, new VisualParam(176, "Hair_Back_Up", 1, "hair", "Back Hair Up", "Back Hair", "Back Hair Up", 0f, 0f, 1f));
                    paramsDict.Add(177, new VisualParam(177, "Hair_Rumpled", 0, "hair", "Rumpled Hair", "Smooth Hair", "Rumpled Hair", 0f, 0f, 1f));
                    paramsDict.Add(178, new VisualParam(178, "Hair_Swept_Back", 1, "hair", "Swept Back Hair", "NotHair", "Swept Back", 0f, 0f, 1f));
                    paramsDict.Add(179, new VisualParam(179, "Hair_Swept_Forward", 1, "hair", "Swept Forward Hair", "Hair", "Swept Forward", 0f, 0f, 1f));
                    paramsDict.Add(190, new VisualParam(190, "Hair_Tilt_Right", 1, "hair", "Hair Tilted Right", "Hair", "Tilt Right", 0f, 0f, 1f));
                    paramsDict.Add(191, new VisualParam(191, "Hair_Tilt_Left", 1, "hair", "Hair Tilted Left", "Hair", "Tilt Left", 0f, 0f, 1f));
                    paramsDict.Add(192, new VisualParam(192, "Bangs_Part_Middle", 0, "hair", "Part Bangs", "No Part", "Part Bangs", 0f, 0f, 1f));
                    paramsDict.Add(640, new VisualParam(640, "Hair_Egg_Head", 1, "hair", "", "", "", -1.3f, -1.3f, 1f));
                    paramsDict.Add(641, new VisualParam(641, "Hair_Squash_Stretch_Head", 1, "hair", "", "", "", -0.5f, -0.5f, 1f));
                    paramsDict.Add(642, new VisualParam(642, "Hair_Square_Head", 1, "hair", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(643, new VisualParam(643, "Hair_Round_Head", 1, "hair", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(644, new VisualParam(644, "Hair_Forehead_Round", 1, "hair", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(645, new VisualParam(645, "Hair_Forehead_Slant", 1, "hair", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(774, new VisualParam(774, "Shear_Head_Hair", 1, "hair", "", "", "", -2f, -2f, 2f));
                    paramsDict.Add(771, new VisualParam(771, "Elongate_Head_Hair", 1, "hair", "", "", "", -1f, -1f, 1f));
                    paramsDict.Add(674, new VisualParam(674, "Hair_Shear_Back", 0, "hair", "Shear Back", "Full Back", "Sheared Back", -0.3f, -1f, 2f));
                    paramsDict.Add(762, new VisualParam(762, "Hair_Shear_Front", 0, "hair", "Shear Front", "Full Front", "Sheared Front", 0f, 0f, 3f));
                    paramsDict.Add(754, new VisualParam(754, "Hair_Taper_Back", 0, "hair", "Taper Back", "Wide Back", "Narrow Back", 0f, -1f, 2f));
                    paramsDict.Add(755, new VisualParam(755, "Hair_Taper_Front", 0, "hair", "Taper Front", "Wide Front", "Narrow Front", 0.05f, -1.5f, 1.5f));
                    paramsDict.Add(782, new VisualParam(782, "Hair_Pigtails_Short", 1, "hair", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(783, new VisualParam(783, "Hair_Pigtails_Med", 1, "hair", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(790, new VisualParam(790, "Hair_Pigtails_Medlong", 1, "hair", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(784, new VisualParam(784, "Hair_Pigtails_Long", 1, "hair", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(786, new VisualParam(786, "Hair_Ponytail_Short", 1, "hair", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(787, new VisualParam(787, "Hair_Ponytail_Med", 1, "hair", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(788, new VisualParam(788, "Hair_Ponytail_Long", 1, "hair", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(1, new VisualParam(1, "Big_Brow", 0, "shape", "Brow Size", "Small", "Large", -0.3f, -0.3f, 2f));
                    paramsDict.Add(2, new VisualParam(2, "Nose_Big_Out", 0, "shape", "Nose Size", "Small", "Large", -0.8f, -0.8f, 2.5f));
                    paramsDict.Add(4, new VisualParam(4, "Broad_Nostrils", 0, "shape", "Nostril Width", "Narrow", "Broad", -0.5f, -0.5f, 1f));
                    paramsDict.Add(759, new VisualParam(759, "Low_Septum_Nose", 0, "shape", "Nostril Division", "High", "Low", 0.5f, -1f, 1.5f));
                    paramsDict.Add(517, new VisualParam(517, "Wide_Nose", 0, "shape", "Nose Width", "Narrow", "Wide", -0.5f, -0.5f, 1f));
                    paramsDict.Add(5, new VisualParam(5, "Cleft_Chin", 0, "shape", "Chin Cleft", "Round", "Cleft", -0.1f, -0.1f, 1f));
                    paramsDict.Add(6, new VisualParam(6, "Bulbous_Nose_Tip", 0, "shape", "Nose Tip Shape", "Pointy", "Bulbous", -0.3f, -0.3f, 1f));
                    paramsDict.Add(7, new VisualParam(7, "Weak_Chin", 0, "shape", "Chin Angle", "Chin Out", "Chin In", -0.5f, -0.5f, 0.5f));
                    paramsDict.Add(8, new VisualParam(8, "Double_Chin", 0, "shape", "Chin-Neck", "Tight Chin", "Double Chin", -0.5f, -0.5f, 1.5f));
                    paramsDict.Add(10, new VisualParam(10, "Sunken_Cheeks", 0, "shape", "Lower Cheeks", "Well-Fed", "Sunken", -1.5f, -1.5f, 3f));
                    paramsDict.Add(11, new VisualParam(11, "Noble_Nose_Bridge", 0, "shape", "Upper Bridge", "Low", "High", -0.5f, -0.5f, 1.5f));
                    paramsDict.Add(758, new VisualParam(758, "Lower_Bridge_Nose", 0, "shape", "Lower Bridge", "Low", "High", -1.5f, -1.5f, 1.5f));
                    paramsDict.Add(12, new VisualParam(12, "Jowls", 0, "shape", "", "Less", "More", -0.5f, -0.5f, 2.5f));
                    paramsDict.Add(13, new VisualParam(13, "Cleft_Chin_Upper", 0, "shape", "Upper Chin Cleft", "Round", "Cleft", 0f, 0f, 1.5f));
                    paramsDict.Add(14, new VisualParam(14, "High_Cheek_Bones", 0, "shape", "Cheek Bones", "Low", "High", -0.5f, -0.5f, 1f));
                    paramsDict.Add(15, new VisualParam(15, "Ears_Out", 0, "shape", "Ear Angle", "In", "Out", -0.5f, -0.5f, 1.5f));
                    paramsDict.Add(870, new VisualParam(870, "Pointy_Eyebrows", 1, "hair", "Eyebrow Points", "Smooth", "Pointy", -0.5f, -0.5f, 1f));
                    paramsDict.Add(17, new VisualParam(17, "Square_Jaw", 0, "shape", "Jaw Shape", "Pointy", "Square", -0.5f, -0.5f, 1f));
                    paramsDict.Add(18, new VisualParam(18, "Puffy_Upper_Cheeks", 0, "shape", "Upper Cheeks", "Thin", "Puffy", -1.5f, -1.5f, 2.5f));
                    paramsDict.Add(19, new VisualParam(19, "Upturned_Nose_Tip", 0, "shape", "Nose Tip Angle", "Downturned", "Upturned", -1.5f, -1.5f, 1f));
                    paramsDict.Add(20, new VisualParam(20, "Bulbous_Nose", 0, "shape", "Nose Thickness", "Thin Nose", "Bulbous Nose", -0.5f, -0.5f, 1.5f));
                    paramsDict.Add(21, new VisualParam(21, "Upper_Eyelid_Fold", 0, "shape", "Upper Eyelid Fold", "Uncreased", "Creased", -0.2f, -0.2f, 1.3f));
                    paramsDict.Add(22, new VisualParam(22, "Attached_Earlobes", 0, "shape", "Attached Earlobes", "Unattached", "Attached", 0f, 0f, 1f));
                    paramsDict.Add(23, new VisualParam(23, "Baggy_Eyes", 0, "shape", "Eye Bags", "Smooth", "Baggy", -0.5f, -0.5f, 1.5f));
                    paramsDict.Add(765, new VisualParam(765, "Puffy_Lower_Lids", 0, "shape", "Puffy Eyelids", "Flat", "Puffy", -0.3f, -0.3f, 2.5f));
                    paramsDict.Add(24, new VisualParam(24, "Wide_Eyes", 0, "shape", "Eye Opening", "Narrow", "Wide", -1.5f, -1.5f, 2f));
                    paramsDict.Add(25, new VisualParam(25, "Wide_Lip_Cleft", 0, "shape", "Lip Cleft", "Narrow", "Wide", -0.8f, -0.8f, 1.5f));
                    paramsDict.Add(764, new VisualParam(764, "Lip_Cleft_Deep", 0, "shape", "Lip Cleft Depth", "Shallow", "Deep", -0.5f, -0.5f, 1.2f));
                    paramsDict.Add(27, new VisualParam(27, "Wide_Nose_Bridge", 0, "shape", "Bridge Width", "Narrow", "Wide", -1.3f, -1.3f, 1.2f));
                    paramsDict.Add(872, new VisualParam(872, "Arced_Eyebrows", 1, "hair", "Eyebrow Arc", "Flat", "Arced", 0f, 0f, 1f));
                    paramsDict.Add(871, new VisualParam(871, "Lower_Eyebrows", 1, "hair", "Eyebrow Height", "Higher", "Lower", -2f, -2f, 2f));
                    paramsDict.Add(35, new VisualParam(35, "Big_Ears", 0, "shape", "Ear Size", "Small", "Large", -1f, -1f, 2f));
                    paramsDict.Add(796, new VisualParam(796, "Pointy_Ears", 0, "shape", "Ear Tips", "Flat", "Pointy", -0.4f, -0.4f, 3f));
                    paramsDict.Add(185, new VisualParam(185, "Deep_Chin", 0, "shape", "Chin Depth", "Shallow", "Deep", -1f, -1f, 1f));
                    paramsDict.Add(186, new VisualParam(186, "Egg_Head", 1, "shape", "Egg Head", "Chin Heavy", "Forehead Heavy", -1.3f, -1.3f, 1f));
                    paramsDict.Add(187, new VisualParam(187, "Squash_Stretch_Head", 1, "shape", "Squash/Stretch Head", "Squash Head", "Stretch Head", -0.5f, -0.5f, 1f));
                    paramsDict.Add(188, new VisualParam(188, "Square_Head", 1, "shape", "", "Less Square", "More Square", 0f, 0f, 0.7f));
                    paramsDict.Add(189, new VisualParam(189, "Round_Head", 1, "shape", "", "Less Round", "More Round", 0f, 0f, 1f));
                    paramsDict.Add(194, new VisualParam(194, "Eye_Spread", 1, "shape", "", "Eyes Together", "Eyes Spread", -2f, -2f, 2f));
                    paramsDict.Add(400, new VisualParam(400, "Displace_Hair_Facial", 1, "hair", "Hair Thickess", "Cropped Hair", "Bushy Hair", 0f, 0f, 2f));
                    paramsDict.Add(506, new VisualParam(506, "Mouth_Height", 0, "shape", "Mouth Position", "High", "Low", -2f, -2f, 2f));
                    paramsDict.Add(633, new VisualParam(633, "Fat_Head", 1, "shape", "Fat Head", "Skinny", "Fat", 0f, 0f, 1f));
                    paramsDict.Add(630, new VisualParam(630, "Forehead_Round", 1, "shape", "Round Forehead", "Less", "More", 0f, 0f, 1f));
                    paramsDict.Add(631, new VisualParam(631, "Forehead_Slant", 1, "shape", "Slanted Forehead", "Less", "More", 0f, 0f, 1f));
                    paramsDict.Add(650, new VisualParam(650, "Eyelid_Corner_Up", 0, "shape", "Outer Eye Corner", "Corner Down", "Corner Up", -1.3f, -1.3f, 1.2f));
                    paramsDict.Add(880, new VisualParam(880, "Eyelid_Inner_Corner_Up", 0, "shape", "Inner Eye Corner", "Corner Down", "Corner Up", -1.3f, -1.3f, 1.2f));
                    paramsDict.Add(653, new VisualParam(653, "Tall_Lips", 0, "shape", "Lip Fullness", "Less Full", "More Full", -1f, -1f, 2f));
                    paramsDict.Add(656, new VisualParam(656, "Crooked_Nose", 0, "shape", "Crooked Nose", "Nose Left", "Nose Right", -2f, -2f, 2f));
                    paramsDict.Add(657, new VisualParam(657, "Smile_Mouth", 1, "shape", "Mouth Corner", "Corner Normal", "Corner Up", 0f, 0f, 1.4f));
                    paramsDict.Add(658, new VisualParam(658, "Frown_Mouth", 1, "shape", "Mouth Corner", "Corner Normal", "Corner Down", 0f, 0f, 1.2f));
                    paramsDict.Add(797, new VisualParam(797, "Fat_Upper_Lip", 1, "shape", "Fat Upper Lip", "Normal Upper", "Fat Upper", 0f, 0f, 1.5f));
                    paramsDict.Add(798, new VisualParam(798, "Fat_Lower_Lip", 1, "shape", "Fat Lower Lip", "Normal Lower", "Fat Lower", 0f, 0f, 1.5f));
                    paramsDict.Add(660, new VisualParam(660, "Shear_Head", 1, "shape", "Shear Face", "Shear Left", "Shear Right", 0f, -2f, 2f));
                    paramsDict.Add(770, new VisualParam(770, "Elongate_Head", 1, "shape", "Shear Face", "Flat Head", "Long Head", 0f, -1f, 1f));
                    paramsDict.Add(663, new VisualParam(663, "Shift_Mouth", 0, "shape", "Shift Mouth", "Shift Left", "Shift Right", 0f, -2f, 2f));
                    paramsDict.Add(664, new VisualParam(664, "Pop_Eye", 0, "shape", "Eye Pop", "Pop Right Eye", "Pop Left Eye", 0f, -1.3f, 1.3f));
                    paramsDict.Add(760, new VisualParam(760, "Jaw_Angle", 0, "shape", "Jaw Angle", "Low Jaw", "High Jaw", 0f, -1.2f, 2f));
                    paramsDict.Add(665, new VisualParam(665, "Jaw_Jut", 0, "shape", "Jaw Jut", "Overbite", "Underbite", 0f, -2f, 2f));
                    paramsDict.Add(686, new VisualParam(686, "Head_Eyes_Big", 1, "shape", "Eye Size", "Beady Eyes", "Anime Eyes", 0f, -2f, 2f));
                    paramsDict.Add(767, new VisualParam(767, "Bug_Eyed_Head", 1, "shape", "Eye Depth", "Sunken Eyes", "Bug Eyes", 0f, -2f, 2f));
                    paramsDict.Add(518, new VisualParam(518, "Eyelashes_Long", 0, "shape", "Eyelash Length", "Short", "Long", -0.3f, -0.3f, 1.5f));
                    paramsDict.Add(626, new VisualParam(626, "Big_Chest", 1, "shape", "Chest Size", "Small", "Large", 0f, 0f, 1f));
                    paramsDict.Add(627, new VisualParam(627, "Small_Chest", 1, "shape", "Chest Size", "Large", "Small", 0f, 0f, 1f));
                    paramsDict.Add(843, new VisualParam(843, "No_Chest", 1, "shape", "Chest Size", "Some", "None", 0f, 0f, 1f));
                    paramsDict.Add(106, new VisualParam(106, "Muscular_Torso", 1, "shape", "Torso Muscles", "Regular", "Muscular", 0f, 0f, 1.4f));
                    paramsDict.Add(648, new VisualParam(648, "Scrawny_Torso", 1, "shape", "Torso Muscles", "Regular", "Scrawny", 0f, 0f, 1.3f));
                    paramsDict.Add(677, new VisualParam(677, "Scrawny_Torso_Male", 1, "shape", "Torso Scrawny", "Regular", "Scrawny", 0f, 0f, 1.3f));
                    paramsDict.Add(634, new VisualParam(634, "Fat_Torso", 1, "shape", "Fat Torso", "skinny", "fat", 0f, 0f, 1f));
                    paramsDict.Add(507, new VisualParam(507, "Breast_Gravity", 0, "shape", "Breast Buoyancy", "Less Gravity", "More Gravity", 0f, -1.5f, 2f));
                    paramsDict.Add(840, new VisualParam(840, "Shirtsleeve_flair", 0, "shirt", "Sleeve Looseness", "Tight Sleeves", "Loose Sleeves", 0f, 0f, 1.5f));
                    paramsDict.Add(684, new VisualParam(684, "Breast_Female_Cleavage", 0, "shape", "Breast Cleavage", "Separate", "Join", 0f, -0.3f, 1.3f));
                    paramsDict.Add(685, new VisualParam(685, "Chest_Male_No_Pecs", 0, "shape", "Pectorals", "Big Pectorals", "Sunken Chest", 0f, -0.5f, 1.1f));
                    paramsDict.Add(151, new VisualParam(151, "Big_Butt_Legs", 1, "shape", "Butt Size", "Regular", "Large", 0f, 0f, 1f));
                    paramsDict.Add(794, new VisualParam(794, "Small_Butt", 1, "shape", "Butt Size", "Regular", "Small", 0f, 0f, 1f));
                    paramsDict.Add(152, new VisualParam(152, "Muscular_Legs", 1, "shape", "Leg Muscles", "Regular Muscles", "More Muscles", 0f, 0f, 1.5f));
                    paramsDict.Add(651, new VisualParam(651, "Scrawny_Legs", 1, "shape", "Scrawny Leg", "Regular Muscles", "Less Muscles", 0f, 0f, 1.5f));
                    paramsDict.Add(853, new VisualParam(853, "Bowed_Legs", 1, "shape", "Knee Angle", "", "", -1f, -1f, 1f));
                    paramsDict.Add(500, new VisualParam(500, "Shoe_Heel_Height", 1, "shoes", "Heel Height", "Low Heels", "High Heels", 0f, 0f, 1f));
                    paramsDict.Add(501, new VisualParam(501, "Shoe_Platform_Height", 1, "shoes", "Platform Height", "Low Platforms", "High Platforms", 0f, 0f, 1f));
                    paramsDict.Add(508, new VisualParam(508, "Shoe_Platform_Width", 0, "shoes", "Platform Width", "Narrow", "Wide", -1f, -1f, 2f));
                    paramsDict.Add(509, new VisualParam(509, "Shoe_Heel_Point", 1, "shoes", "Heel Shape", "Default Heels", "Pointy Heels", 0f, 0f, 1f));
                    paramsDict.Add(510, new VisualParam(510, "Shoe_Heel_Thick", 1, "shoes", "Heel Shape", "default Heels", "Thick Heels", 0f, 0f, 1f));
                    paramsDict.Add(511, new VisualParam(511, "Shoe_Toe_Point", 1, "shoes", "Toe Shape", "Default Toe", "Pointy Toe", 0f, 0f, 1f));
                    paramsDict.Add(512, new VisualParam(512, "Shoe_Toe_Square", 1, "shoes", "Toe Shape", "Default Toe", "Square Toe", 0f, 0f, 1f));
                    paramsDict.Add(654, new VisualParam(654, "Shoe_Toe_Thick", 0, "shoes", "Toe Thickness", "Flat Toe", "Thick Toe", 0f, 0f, 2f));
                    paramsDict.Add(515, new VisualParam(515, "Foot_Size", 0, "shape", "Foot Size", "Small", "Big", -1f, -1f, 3f));
                    paramsDict.Add(625, new VisualParam(625, "Leg_Pantflair", 0, "pants", "Cuff Flare", "Tight Cuffs", "Flared Cuffs", 0f, 0f, 1.5f));
                    paramsDict.Add(638, new VisualParam(638, "Low_Crotch", 0, "pants", "Pants Crotch", "High and Tight", "Low and Loose", 0f, 0f, 1.3f));
                    paramsDict.Add(635, new VisualParam(635, "Fat_Legs", 1, "shape", "Fat Torso", "skinny", "fat", 0f, 0f, 1f));
                    paramsDict.Add(879, new VisualParam(879, "Male_Package", 0, "shape", "Package", "Coin Purse", "Duffle Bag", 0f, -0.5f, 2f));
                    paramsDict.Add(679, new VisualParam(679, "Eyeball_Size", 1, "shape", "Eyeball Size", "small eye", "big eye", -0.25f, -0.25f, 0.1f));
                    paramsDict.Add(687, new VisualParam(687, "Eyeball_Size", 1, "shape", "Big Eyeball", "small eye", "big eye", -0.25f, -0.25f, 0.25f));
                    paramsDict.Add(694, new VisualParam(694, "Eyeball_Size", 1, "shape", "Eyeball Size", "small eye", "big eye", -0.25f, -0.25f, 0.1f));
                    paramsDict.Add(695, new VisualParam(695, "Eyeball_Size", 1, "shape", "Big Eyeball", "small eye", "big eye", -0.25f, -0.25f, 0.25f));
                    paramsDict.Add(680, new VisualParam(680, "Eyeball_Size", 1, "shape", "Eyeball Size", "small eye", "big eye", -0.25f, -0.25f, 0.1f));
                    paramsDict.Add(688, new VisualParam(688, "Eyeball_Size", 1, "shape", "Big Eyeball", "small eye", "big eye", -0.25f, -0.25f, 0.25f));
                    paramsDict.Add(681, new VisualParam(681, "Eyeball_Size", 1, "shape", "Eyeball Size", "small eye", "big eye", -0.25f, -0.25f, 0.1f));
                    paramsDict.Add(691, new VisualParam(691, "Eyeball_Size", 1, "shape", "Big Eyeball", "small eye", "big eye", -0.25f, -0.25f, 0.25f));
                    paramsDict.Add(845, new VisualParam(845, "skirt_poofy", 1, "skirt", "poofy skirt", "less poofy", "more poofy", 0f, 0f, 1.5f));
                    paramsDict.Add(846, new VisualParam(846, "skirt_loose", 1, "skirt", "loose skirt", "form fitting", "loose", 0f, 0f, 1f));
                    paramsDict.Add(866, new VisualParam(866, "skirt_tight", 1, "skirt", "tight skirt", "form fitting", "loose", 0f, 0f, 1f));
                    paramsDict.Add(867, new VisualParam(867, "skirt_smallbutt", 1, "skirt", "tight skirt", "form fitting", "loose", 0f, 0f, 1f));
                    paramsDict.Add(848, new VisualParam(848, "skirt_bustle", 0, "skirt", "bustle skirt", "no bustle", "more bustle", 0.2f, 0f, 2f));
                    paramsDict.Add(847, new VisualParam(847, "skirt_bowlegs", 1, "skirt", "legs skirt", "", "", 0f, -1f, 1f));
                    paramsDict.Add(111, new VisualParam(111, "Pigment", 0, "skin", "", "Light", "Dark", 0.5f, 0f, 1f));
                    paramsDict.Add(110, new VisualParam(110, "Red Skin", 0, "skin", "Ruddiness", "Pale", "Ruddy", 0f, 0f, 0.1f));
                    paramsDict.Add(108, new VisualParam(108, "Rainbow Color", 0, "skin", "", "None", "Wild", 0f, 0f, 1f));
                    paramsDict.Add(114, new VisualParam(114, "Blonde Hair", 0, "hair", "", "Black", "Blonde", 0.5f, 0f, 1f));
                    paramsDict.Add(113, new VisualParam(113, "Red Hair", 0, "hair", "", "No Red", "Very Red", 0f, 0f, 1f));
                    paramsDict.Add(115, new VisualParam(115, "White Hair", 0, "hair", "", "No White", "All White", 0f, 0f, 1f));
                    paramsDict.Add(112, new VisualParam(112, "Rainbow Color", 0, "hair", "", "None", "Wild", 0f, 0f, 1f));
                    paramsDict.Add(99, new VisualParam(99, "Eye Color", 0, "eyes", "", "Natural", "Unnatural", 0f, 0f, 1f));
                    paramsDict.Add(98, new VisualParam(98, "Eye Lightness", 0, "eyes", "", "Darker", "Lighter", 0f, 0f, 1f));
                    paramsDict.Add(158, new VisualParam(158, "Shading", 1, "skin", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(159, new VisualParam(159, "Shading", 1, "skin", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(116, new VisualParam(116, "Rosy Complexion", 0, "skin", "", "Less Rosy", "More Rosy", 0f, 0f, 1f));
                    paramsDict.Add(117, new VisualParam(117, "Lip Pinkness", 0, "skin", "", "Darker", "Pinker", 0f, 0f, 1f));
                    paramsDict.Add(118, new VisualParam(118, "Wrinkles", 1, "skin", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(165, new VisualParam(165, "Freckles", 0, "skin", "", "Less", "More", 0f, 0f, 1f));
                    paramsDict.Add(1001, new VisualParam(1001, "Eyebrow Size", 1, "hair", "", "", "", 0.5f, 0f, 1f));
                    paramsDict.Add(700, new VisualParam(700, "Lipstick Color", 0, "skin", "", "Pink", "Black", 0.25f, 0f, 1f));
                    paramsDict.Add(701, new VisualParam(701, "Lipstick", 0, "skin", "", "No Lipstick", "More Lipstick", 0f, 0f, 0.9f));
                    paramsDict.Add(702, new VisualParam(702, "Lipgloss", 0, "skin", "", "No Lipgloss", "Glossy", 0f, 0f, 1f));
                    paramsDict.Add(704, new VisualParam(704, "Blush", 0, "skin", "", "No Blush", "More Blush", 0f, 0f, 0.9f));
                    paramsDict.Add(705, new VisualParam(705, "Blush Color", 0, "skin", "", "Pink", "Orange", 0.5f, 0f, 1f));
                    paramsDict.Add(711, new VisualParam(711, "Blush Opacity", 0, "skin", "", "Clear", "Opaque", 0.5f, 0f, 1f));
                    paramsDict.Add(708, new VisualParam(708, "Out Shdw Color", 0, "skin", "", "Light", "Dark", 0f, 0f, 1f));
                    paramsDict.Add(706, new VisualParam(706, "Out Shdw Opacity", 0, "skin", "", "Clear", "Opaque", 0.6f, 0.2f, 1f));
                    paramsDict.Add(707, new VisualParam(707, "Outer Shadow", 0, "skin", "", "No Eyeshadow", "More Eyeshadow", 0f, 0f, 0.7f));
                    paramsDict.Add(712, new VisualParam(712, "In Shdw Color", 0, "skin", "", "Light", "Dark", 0f, 0f, 1f));
                    paramsDict.Add(713, new VisualParam(713, "In Shdw Opacity", 0, "skin", "", "Clear", "Opaque", 0.7f, 0.2f, 1f));
                    paramsDict.Add(709, new VisualParam(709, "Inner Shadow", 0, "skin", "", "No Eyeshadow", "More Eyeshadow", 0f, 0f, 1f));
                    paramsDict.Add(703, new VisualParam(703, "Eyeliner", 0, "skin", "", "No Eyeliner", "Full Eyeliner", 0f, 0f, 1f));
                    paramsDict.Add(714, new VisualParam(714, "Eyeliner Color", 0, "skin", "", "Dark Green", "Black", 0f, 0f, 1f));
                    paramsDict.Add(751, new VisualParam(751, "5 O'Clock Shadow", 1, "hair", "", "Dense hair", "Shadow hair", 0.7f, 0f, 1f));
                    paramsDict.Add(125, new VisualParam(125, "Shading", 1, "skin", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(126, new VisualParam(126, "Shading", 1, "skin", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(776, new VisualParam(776, "freckles upper", 1, "skin", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(1045, new VisualParam(1045, "Bottom bump", 1, "undershirt", "", "", "", 0.8f, 0f, 1f));
                    paramsDict.Add(821, new VisualParam(821, "undershirt_red", 0, "undershirt", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(822, new VisualParam(822, "undershirt_green", 0, "undershirt", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(823, new VisualParam(823, "undershirt_blue", 0, "undershirt", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(710, new VisualParam(710, "Nail Polish", 0, "skin", "", "No Polish", "Painted Nails", 0f, 0f, 1f));
                    paramsDict.Add(715, new VisualParam(715, "Nail Polish Color", 0, "skin", "", "Pink", "Black", 0f, 0f, 1f));
                    paramsDict.Add(827, new VisualParam(827, "gloves_red", 0, "gloves", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(829, new VisualParam(829, "gloves_green", 0, "gloves", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(830, new VisualParam(830, "gloves_blue", 0, "gloves", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(868, new VisualParam(868, "Shirt Wrinkles", 0, "shirt", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(803, new VisualParam(803, "shirt_red", 0, "shirt", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(804, new VisualParam(804, "shirt_green", 0, "shirt", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(805, new VisualParam(805, "shirt_blue", 0, "shirt", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(875, new VisualParam(875, "jacket upper Wrinkles", 1, "jacket", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(831, new VisualParam(831, "upper_jacket_red", 1, "jacket", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(832, new VisualParam(832, "upper_jacket_green", 1, "jacket", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(833, new VisualParam(833, "upper_jacket_blue", 1, "jacket", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(620, new VisualParam(620, "bottom length upper", 1, "jacket", "", "hi cut", "low cut", 0.8f, 0f, 1f));
                    paramsDict.Add(622, new VisualParam(622, "open upper", 1, "jacket", "", "closed", "open", 0.8f, 0f, 1f));
                    paramsDict.Add(160, new VisualParam(160, "Shading", 1, "pants", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(161, new VisualParam(161, "Shading", 1, "skin", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(777, new VisualParam(777, "freckles lower", 1, "skin", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(1055, new VisualParam(1055, "Pants Length", 1, "underpants", "", "", "", 0.3f, 0f, 1f));
                    paramsDict.Add(1057, new VisualParam(1057, "Pants Waist", 1, "underpants", "", "", "", 0.8f, 0f, 1f));
                    paramsDict.Add(824, new VisualParam(824, "underpants_red", 0, "underpants", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(825, new VisualParam(825, "underpants_green", 0, "underpants", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(826, new VisualParam(826, "underpants_blue", 0, "underpants", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(818, new VisualParam(818, "socks_red", 0, "socks", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(819, new VisualParam(819, "socks_green", 0, "socks", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(820, new VisualParam(820, "socks_blue", 0, "socks", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(812, new VisualParam(812, "shoes_red", 0, "shoes", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(813, new VisualParam(813, "shoes_green", 0, "shoes", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(817, new VisualParam(817, "shoes_blue", 0, "shoes", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(869, new VisualParam(869, "Pants Wrinkles", 0, "pants", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(806, new VisualParam(806, "pants_red", 0, "pants", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(807, new VisualParam(807, "pants_green", 0, "pants", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(808, new VisualParam(808, "pants_blue", 0, "pants", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(876, new VisualParam(876, "jacket upper Wrinkles", 1, "jacket", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(809, new VisualParam(809, "lower_jacket_red", 1, "jacket", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(810, new VisualParam(810, "lower_jacket_green", 1, "jacket", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(811, new VisualParam(811, "lower_jacket_blue", 1, "jacket", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(621, new VisualParam(621, "bottom length lower", 1, "jacket", "", "hi cut", "low cut", 0.8f, 0f, 1f));
                    paramsDict.Add(623, new VisualParam(623, "open lower", 1, "jacket", "", "open", "closed", 0.8f, 0f, 1f));
                    paramsDict.Add(921, new VisualParam(921, "skirt_red", 0, "skirt", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(922, new VisualParam(922, "skirt_green", 0, "skirt", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(923, new VisualParam(923, "skirt_blue", 0, "skirt", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(858, new VisualParam(858, "Skirt Length", 0, "skirt", "", "Short", "Long", 0.4f, 0.01f, 1f));
                    paramsDict.Add(859, new VisualParam(859, "Slit Front", 0, "skirt", "", "Open Front", "Closed Front", 1f, 0f, 1f));
                    paramsDict.Add(860, new VisualParam(860, "Slit Back", 0, "skirt", "", "Open Back", "Closed Back", 1f, 0f, 1f));
                    paramsDict.Add(861, new VisualParam(861, "Slit Left", 0, "skirt", "", "Open Left", "Closed Left", 1f, 0f, 1f));
                    paramsDict.Add(862, new VisualParam(862, "Slit Right", 0, "skirt", "", "Open Right", "Closed Right", 1f, 0f, 1f));
                    paramsDict.Add(828, new VisualParam(828, "Loose Upper Clothing", 0, "shirt", "Shirt Fit", "Tight Shirt", "Loose Shirt", 0f, 0f, 1f));
                    paramsDict.Add(816, new VisualParam(816, "Loose Lower Clothing", 0, "pants", "Pants Fit", "Tight Pants", "Loose Pants", 0f, 0f, 1f));
                    paramsDict.Add(814, new VisualParam(814, "Waist Height", 0, "pants", "", "Low", "High", 1f, 0f, 1f));
                    paramsDict.Add(815, new VisualParam(815, "Pants Length", 0, "pants", "", "Short", "Long", 0.8f, 0f, 1f));
                    paramsDict.Add(800, new VisualParam(800, "Sleeve Length", 0, "shirt", "", "Short", "Long", 0.89f, 0f, 1f));
                    paramsDict.Add(801, new VisualParam(801, "Shirt Bottom", 0, "shirt", "", "Short", "Long", 1f, 0f, 1f));
                    paramsDict.Add(802, new VisualParam(802, "Collar Front", 0, "shirt", "", "Low", "High", 0.78f, 0f, 1f));
                    paramsDict.Add(781, new VisualParam(781, "Collar Back", 0, "shirt", "", "Low", "High", 0.78f, 0f, 1f));
                    paramsDict.Add(150, new VisualParam(150, "Body Definition", 0, "skin", "", "Less", "More", 0f, 0f, 1f));
                    paramsDict.Add(775, new VisualParam(775, "Body Freckles", 0, "skin", "", "Less Freckles", "More Freckles", 0f, 0f, 1f));
                    paramsDict.Add(162, new VisualParam(162, "Facial Definition", 0, "skin", "", "Less", "More", 0f, 0f, 1f));
                    paramsDict.Add(163, new VisualParam(163, "wrinkles", 0, "skin", "", "Less", "More", 0f, 0f, 1f));
                    paramsDict.Add(505, new VisualParam(505, "Lip Thickness", 0, "shape", "", "Thin Lips", "Fat Lips", 0.5f, 0f, 1f));
                    paramsDict.Add(799, new VisualParam(799, "Lip Ratio", 0, "shape", "Lip Ratio", "More Upper Lip", "More Lower Lip", 0.5f, 0f, 1f));
                    paramsDict.Add(155, new VisualParam(155, "Lip Width", 0, "shape", "Lip Width", "Narrow Lips", "Wide Lips", 0f, -0.9f, 1.3f));
                    paramsDict.Add(196, new VisualParam(196, "Eye Spacing", 0, "shape", "Eye Spacing", "Close Set Eyes", "Far Set Eyes", 0f, -2f, 1f));
                    paramsDict.Add(769, new VisualParam(769, "Eye Depth", 0, "shape", "", "Sunken Eyes", "Bugged Eyes", 0.5f, 0f, 1f));
                    paramsDict.Add(198, new VisualParam(198, "Heel Height", 0, "shoes", "", "Low Heels", "High Heels", 0f, 0f, 1f));
                    paramsDict.Add(513, new VisualParam(513, "Heel Shape", 0, "shoes", "", "Pointy Heels", "Thick Heels", 0.5f, 0f, 1f));
                    paramsDict.Add(514, new VisualParam(514, "Toe Shape", 0, "shoes", "", "Pointy", "Square", 0.5f, 0f, 1f));
                    paramsDict.Add(503, new VisualParam(503, "Platform Height", 0, "shoes", "", "Low Platforms", "High Platforms", 0f, 0f, 1f));
                    paramsDict.Add(193, new VisualParam(193, "Head Shape", 0, "shape", "Head Shape", "More Square", "More Round", 0.5f, 0f, 1f));
                    paramsDict.Add(157, new VisualParam(157, "Belly Size", 0, "shape", "", "Small", "Big", 0f, 0f, 1f));
                    paramsDict.Add(637, new VisualParam(637, "Body Fat", 0, "shape", "", "Less Body Fat", "More Body Fat", 0f, 0f, 1f));
                    paramsDict.Add(130, new VisualParam(130, "Front Fringe", 0, "hair", "", "Short", "Long", 0.45f, 0f, 1f));
                    paramsDict.Add(131, new VisualParam(131, "Side Fringe", 0, "hair", "", "Short", "Long", 0.5f, 0f, 1f));
                    paramsDict.Add(132, new VisualParam(132, "Back Fringe", 0, "hair", "", "Short", "Long", 0.39f, 0f, 1f));
                    paramsDict.Add(133, new VisualParam(133, "Hair Front", 0, "hair", "", "Short", "Long", 0.25f, 0f, 1f));
                    paramsDict.Add(134, new VisualParam(134, "Hair Sides", 0, "hair", "", "Short", "Long", 0.5f, 0f, 1f));
                    paramsDict.Add(135, new VisualParam(135, "Hair Back", 0, "hair", "", "Short", "Long", 0.55f, 0f, 1f));
                    paramsDict.Add(136, new VisualParam(136, "Hair Sweep", 0, "hair", "", "Sweep Forward", "Sweep Back", 0.5f, 0f, 1f));
                    paramsDict.Add(137, new VisualParam(137, "Hair Tilt", 0, "hair", "", "Left", "Right", 0.5f, 0f, 1f));
                    paramsDict.Add(608, new VisualParam(608, "bottom length lower", 0, "jacket", "Jacket Length", "Short", "Long", 0.8f, 0f, 1f));
                    paramsDict.Add(609, new VisualParam(609, "open jacket", 0, "jacket", "Open Front", "Open", "Closed", 0.2f, 0f, 1f));
                    paramsDict.Add(105, new VisualParam(105, "Breast Size", 0, "shape", "", "Small", "Large", 0.5f, 0f, 1f));
                    paramsDict.Add(629, new VisualParam(629, "Forehead Angle", 0, "shape", "", "More Vertical", "More Sloped", 0.5f, 0f, 1f));
                    paramsDict.Add(646, new VisualParam(646, "Egg_Head", 0, "shape", "Egg Head", "Chin Heavy", "Forehead Heavy", 0f, -1.3f, 1f));
                    paramsDict.Add(647, new VisualParam(647, "Squash_Stretch_Head", 0, "shape", "Head Stretch", "Squash Head", "Stretch Head", 0f, -0.5f, 1f));
                    paramsDict.Add(649, new VisualParam(649, "Torso Muscles", 0, "shape", "Torso Muscles", "Less Muscular", "More Muscular", 0.5f, 0f, 1f));
                    paramsDict.Add(678, new VisualParam(678, "Torso Muscles", 0, "shape", "", "Less Muscular", "More Muscular", 0.5f, 0f, 1f));
                    paramsDict.Add(652, new VisualParam(652, "Leg Muscles", 0, "shape", "", "Less Muscular", "More Muscular", 0.5f, 0f, 1f));
                    paramsDict.Add(80, new VisualParam(80, "male", 0, "shape", "", "", "", 0f, 0f, 1f));
                    paramsDict.Add(659, new VisualParam(659, "Mouth Corner", 0, "shape", "", "Corner Down", "Corner Up", 0.5f, 0f, 1f));
                    paramsDict.Add(662, new VisualParam(662, "Face Shear", 0, "shape", "", "Shear Right Up", "Shear Left Up", 0.5f, 0f, 1f));
                    paramsDict.Add(773, new VisualParam(773, "Head Length", 0, "shape", "", "Flat Head", "Long Head", 0.5f, 0f, 1f));
                    paramsDict.Add(682, new VisualParam(682, "Head Size", 0, "shape", "Head Size", "Small Head", "Big Head", 0.5f, 0f, 1f));
                    paramsDict.Add(690, new VisualParam(690, "Eye Size", 0, "shape", "Eye Size", "Beady Eyes", "Anime Eyes", 0.5f, 0f, 1f));
                    paramsDict.Add(752, new VisualParam(752, "Hair Thickness", 0, "hair", "", "5 O'Clock Shadow", "Bushy Hair", 0.5f, 0f, 1f));
                    paramsDict.Add(763, new VisualParam(763, "Hair Volume", 0, "hair", "", "Less Volume", "More Volume", 0.55f, 0f, 1f));
                    paramsDict.Add(834, new VisualParam(834, "jacket_red", 0, "jacket", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(835, new VisualParam(835, "jacket_green", 0, "jacket", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(836, new VisualParam(836, "jacket_blue", 0, "jacket", "", "", "", 1f, 0f, 1f));
                    paramsDict.Add(785, new VisualParam(785, "Pigtails", 0, "hair", "", "Short Pigtails", "Long Pigtails", 0f, 0f, 1f));
                    paramsDict.Add(789, new VisualParam(789, "Ponytail", 0, "hair", "", "Short Ponytail", "Long Ponytail", 0f, 0f, 1f));
                    paramsDict.Add(795, new VisualParam(795, "Butt Size", 0, "shape", "Butt Size", "Flat Butt", "Big Butt", 0.25f, 0f, 1f));
                    paramsDict.Add(841, new VisualParam(841, "Bowed_Legs", 0, "shape", "Knee Angle", "Knock Kneed", "Bow Legged", 0f, -1f, 1f));
                    paramsDict.Add(753, new VisualParam(753, "Saddlebags", 0, "shape", "Saddle Bags", "Less Saddle", "More Saddle", 0f, -0.5f, 3f));
                    paramsDict.Add(676, new VisualParam(676, "Love_Handles", 0, "shape", "Love Handles", "Less Love", "More Love", 0f, -1f, 2f));
                    paramsDict.Add(863, new VisualParam(863, "skirt_looseness", 0, "skirt", "Skirt Fit", "Tight Skirt", "Poofy Skirt", 0.333f, 0f, 1f));
                    paramsDict.Add(119, new VisualParam(119, "Eyebrow Size", 0, "hair", "", "Thin Eyebrows", "Bushy Eyebrows", 0.5f, 0f, 1f));
                    paramsDict.Add(750, new VisualParam(750, "Eyebrow Density", 0, "hair", "", "Sparse", "Dense", 0.7f, 0f, 1f));
                    paramsDict.Add(166, new VisualParam(166, "Sideburns", 0, "hair", "", "Short Sideburns", "Mutton Chops", 0f, 0f, 1f));
                    paramsDict.Add(167, new VisualParam(167, "Moustache", 0, "hair", "", "Chaplin", "Handlebars", 0f, 0f, 1f));
                    paramsDict.Add(168, new VisualParam(168, "Soulpatch", 0, "hair", "", "Less soul", "More soul", 0f, 0f, 1f));
                    paramsDict.Add(169, new VisualParam(169, "Chin Curtains", 0, "hair", "", "Less Curtains", "More Curtains", 0f, 0f, 1f));
                    paramsDict.Add(606, new VisualParam(606, "Sleeve Length", 0, "jacket", "", "Short", "Long", 0.8f, 0f, 1f));
                    paramsDict.Add(607, new VisualParam(607, "Collar Front", 0, "jacket", "", "Low", "High", 0.8f, 0f, 1f));
                    paramsDict.Add(780, new VisualParam(780, "Collar Back", 0, "jacket", "", "Low", "High", 0.8f, 0f, 1f));
                    paramsDict.Add(603, new VisualParam(603, "Sleeve Length", 0, "undershirt", "", "Short", "Long", 0.4f, 0.01f, 1f));
                    paramsDict.Add(604, new VisualParam(604, "Bottom", 0, "undershirt", "", "Short", "Long", 0.85f, 0f, 1f));
                    paramsDict.Add(605, new VisualParam(605, "Collar Front", 0, "undershirt", "", "Low", "High", 0.84f, 0f, 1f));
                    paramsDict.Add(779, new VisualParam(779, "Collar Back", 0, "undershirt", "", "Low", "High", 0.84f, 0f, 1f));
                    paramsDict.Add(617, new VisualParam(617, "Socks Length", 0, "socks", "", "Short", "Long", 0.35f, 0f, 1f));
                    paramsDict.Add(616, new VisualParam(616, "Shoe Height", 0, "shoes", "", "Short", "Tall", 0.1f, 0f, 1f));
                    paramsDict.Add(619, new VisualParam(619, "Pants Length", 0, "underpants", "", "Short", "Long", 0.3f, 0f, 1f));
                    paramsDict.Add(624, new VisualParam(624, "Pants Waist", 0, "underpants", "", "Low", "High", 0.8f, 0f, 1f));
                    paramsDict.Add(93, new VisualParam(93, "Glove Length", 0, "gloves", "", "Short", "Long", 0.8f, 0.01f, 1f));
                    paramsDict.Add(844, new VisualParam(844, "Glove Fingers", 0, "gloves", "", "Fingerless", "Fingers", 1f, 0.01f, 1f));
                    paramsDict.Add(16, new VisualParam(16, "Pointy_Eyebrows", 0, "hair", "Eyebrow Points", "Smooth", "Pointy", -0.5f, -0.5f, 3f));
                    paramsDict.Add(757, new VisualParam(757, "Lower_Eyebrows", 0, "hair", "Eyebrow Height", "Higher", "Lower", -1f, -4f, 2f));
                    paramsDict.Add(31, new VisualParam(31, "Arced_Eyebrows", 0, "hair", "Eyebrow Arc", "Flat", "Arced", 0.5f, 0f, 2f));
                    paramsDict.Add(877, new VisualParam(877, "Jacket Wrinkles", 0, "jacket", "Jacket Wrinkles", "No Wrinkles", "Wrinkles", 0f, 0f, 1f));
                }
                return paramsDict;
            }
        }
    }
}
