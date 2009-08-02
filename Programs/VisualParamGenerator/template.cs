using System;
using System.Collections.Generic;

namespace OpenMetaverse
{
    /// <summary>
    /// Operation to apply when applying color to texture
    /// </summary>
    public enum VisualColorOperation
    {
        None,
        Blend,
        Multiply
    }

    /// <summary>
    /// Information needed to translate visual param value to RGBA color
    /// </summary>
    public struct VisualColorParam
    {
        public VisualColorOperation Operation;
        public System.Drawing.Color[] Colors;

        /// <summary>
        /// Construct VisualColorParam
        /// </summary>
        /// <param name="operation">Operation to apply when applying color to texture</param>
        /// <param name="colors">Colors</param>
        public VisualColorParam(VisualColorOperation operation, System.Drawing.Color[] colors)
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
        /// <summary>Alpha blending/bump info</summary>
        public VisualAlphaParam? AlphaParams;
        /// <summary>Color information</summary>
        public VisualColorParam? ColorParams;
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
        public VisualParam(int paramID, string name, int group, string wearable, string label, string labelMin, string labelMax, float def, float min, float max, VisualAlphaParam? alpha, VisualColorParam? colorParams)
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