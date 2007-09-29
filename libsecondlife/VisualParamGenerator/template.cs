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