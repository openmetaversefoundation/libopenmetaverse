/*
 * Copyright (c) 2009, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using OpenMetaverse;

namespace OpenMetaverse.Assets
{
    #region Enums
    /// <summary>
    /// Type of gesture step
    /// </summary>
    public enum GestureStepType : int
    {
        Animation = 0,
        Sound,
        Chat,
        Wait,
        EOF
    }
    #endregion

    #region Gesture step classes
    /// <summary>
    /// Base class for gesture steps
    /// </summary>
    public abstract class GestureStep
    {
        /// <summary>
        /// Retururns what kind of gesture step this is
        /// </summary>
        public abstract GestureStepType GestureStepType { get; }
    }

    /// <summary>
    /// Describes animation step of a gesture
    /// </summary>
    public class GestureStepAnimation : GestureStep
    {
        /// <summary>
        /// Returns what kind of gesture step this is
        /// </summary>
        public override GestureStepType GestureStepType
        {
            get { return GestureStepType.Animation; }
        }

        /// <summary>
        /// If true, this step represents start of animation, otherwise animation stop
        /// </summary>
        public bool AnimationStart = true;

        /// <summary>
        /// Animation asset <see cref="UUID"/>
        /// </summary>
        public UUID ID;

        /// <summary>
        /// Animation inventory name
        /// </summary>
        public string Name;

        public override string ToString()
        {
            if (AnimationStart)
            {
                return "Start animation: " + Name;
            }
            else
            {
                return "Stop animation: " + Name;
            }
        }
    }

    /// <summary>
    /// Describes sound step of a gesture
    /// </summary>
    public class GestureStepSound : GestureStep
    {
        /// <summary>
        /// Returns what kind of gesture step this is
        /// </summary>
        public override GestureStepType GestureStepType
        {
            get { return GestureStepType.Sound; }
        }

        /// <summary>
        /// Sound asset <see cref="UUID"/>
        /// </summary>
        public UUID ID;

        /// <summary>
        /// Sound inventory name
        /// </summary>
        public string Name;

        public override string ToString()
        {
            return "Sound: " + Name;
        }

    }

    /// <summary>
    /// Describes sound step of a gesture
    /// </summary>
    public class GestureStepChat : GestureStep
    {
        /// <summary>
        /// Returns what kind of gesture step this is
        /// </summary>
        public override GestureStepType GestureStepType
        {
            get { return GestureStepType.Chat; }
        }

        /// <summary>
        /// Text to output in chat
        /// </summary>
        public string Text;

        public override string ToString()
        {
            return "Chat: " + Text;
        }
    }

    /// <summary>
    /// Describes sound step of a gesture
    /// </summary>
    public class GestureStepWait : GestureStep
    {
        /// <summary>
        /// Returns what kind of gesture step this is
        /// </summary>
        public override GestureStepType GestureStepType
        {
            get { return GestureStepType.Wait; }
        }

        /// <summary>
        /// If true in this step we wait for all animations to finish
        /// </summary>
        public bool WaitForAnimation;

        /// <summary>
        /// If true gesture player should wait for the specified amount of time
        /// </summary>
        public bool WaitForTime;

        /// <summary>
        /// Time in seconds to wait if WaitForAnimation is false
        /// </summary>
        public float WaitTime;

        public override string  ToString()
        {
            StringBuilder ret = new StringBuilder("-- Wait for: ");

            if (WaitForAnimation)
            {
                ret.Append("(animations to finish) ");
            }

            if (WaitForTime)
            {
                ret.AppendFormat("(time {0:0.0}s)", WaitTime);
            }

            return ret.ToString();
        }
    }

    /// <summary>
    /// Describes the final step of a gesture
    /// </summary>
    public class GestureStepEOF : GestureStep
    {
        /// <summary>
        /// Returns what kind of gesture step this is
        /// </summary>
        public override GestureStepType GestureStepType
        {
            get { return GestureStepType.EOF; }
        }

        public override string ToString()
        {
            return "End of guesture sequence";
        }
    }

    #endregion

    /// <summary>
    /// Represents a sequence of animations, sounds, and chat actions
    /// </summary>
    public class AssetGesture : Asset
    {
        /// <summary>
        /// Returns asset type
        /// </summary>
        public override AssetType AssetType
        {
            get { return AssetType.Gesture; }
        }

        /// <summary>
        /// String that triggers playing of the gesture sequence
        /// </summary>
        public string Trigger;

        /// <summary>
        /// Text that replaces trigger in chat once gesture is triggered
        /// </summary>
        public string ReplaceWith;

        /// <summary>
        /// Sequence of gesture steps
        /// </summary>
        public List<GestureStep> Sequence;

        /// <summary>
        /// Constructs guesture asset
        /// </summary>
        public AssetGesture() { }

        /// <summary>
        /// Constructs guesture asset
        /// </summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetGesture(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
            Decode();
        }

        /// <summary>
        /// Encodes gesture asset suitable for uplaod
        /// </summary>
        public override void Encode()
        {
            // TODO: implement encoder
            throw new NotImplementedException("Encoding guestire assets not supported");
        }

        /// <summary>
        /// Decodes gesture assset into play sequence
        /// </summary>
        /// <returns></returns>
        public override bool Decode()
        {
            try
            {
                string[] lines = Utils.BytesToString(AssetData).Split('\n');
                Sequence = new List<GestureStep>();

                int i = 0;
                
                // version
                int version = int.Parse(lines[i++]);
                if (version != 2)
                {
                    throw new Exception("Only know how to decode version 2 of gesture asset");
                }

                byte key = byte.Parse(lines[i++]);
                int mask = int.Parse(lines[i++]);
                Trigger = lines[i++];
                ReplaceWith = lines[i++];

                int count = int.Parse(lines[i++]);

                if (count < 0)
                {
                    throw new Exception("Wrong number of gesture steps");
                }

                for (int n = 0; n < count; n++)
                {
                    GestureStepType type = (GestureStepType)int.Parse(lines[i++]);

                    switch (type)
                    {
                        case GestureStepType.EOF:
                            goto Finish;

                        case GestureStepType.Animation:
                            {
                                GestureStepAnimation step = new GestureStepAnimation();
                                step.Name = lines[i++];
                                step.ID = new UUID(lines[i++]);
                                int flags = int.Parse(lines[i++]);

                                if (flags == 0)
                                {
                                    step.AnimationStart = true;
                                }
                                else
                                {
                                    step.AnimationStart = false;
                                }

                                Sequence.Add(step);
                                break;
                            }

                        case GestureStepType.Sound:
                            {
                                GestureStepSound step = new GestureStepSound();
                                step.Name = lines[i++];
                                step.ID = new UUID(lines[i++]);
                                int flags = int.Parse(lines[i++]);

                                Sequence.Add(step);
                                break;
                            }

                        case GestureStepType.Chat:
                            {
                                GestureStepChat step = new GestureStepChat();
                                step.Text = lines[i++];
                                int flags = int.Parse(lines[i++]);

                                Sequence.Add(step);
                                break;
                            }

                        case GestureStepType.Wait:
                            {
                                GestureStepWait step = new GestureStepWait();
                                step.WaitTime = float.Parse(lines[i++], Utils.EnUsCulture);
                                int flags = int.Parse(lines[i++]);

                                step.WaitForTime = (flags & 0x01) != 0;
                                step.WaitForAnimation = (flags & 0x02) != 0;
                                Sequence.Add(step);
                                break;
                            }

                    }
                }
                Finish:

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Decoding gestrue asset failed:" + ex.Message, Helpers.LogLevel.Error);
                return false;
            }
        }
    }
}
