using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;

namespace Simian
{
    public struct Animation
    {
        public UUID ID;
        public int SequenceNum;

        public Animation(UUID id, int sequenceNum)
        {
            ID = id;
            SequenceNum = sequenceNum;
        }
    }

    public class AnimationSet
    {
        private Animation defaultAnimation;
        private List<Animation> animations = new List<Animation>();

        public AnimationSet()
        {
            ResetDefaultAnimation();
        }

        public bool HasAnimation(UUID animID)
        {
            if (defaultAnimation.ID == animID)
                return true;

            lock (animations)
            {
                for (int i = 0; i < animations.Count; ++i)
                {
                    if (animations[i].ID == animID)
                        return true;
                }
            }

            return false;
        }

        public bool Add(UUID animID, ref int sequenceCounter)
        {
            lock (animations)
            {
                if (!HasAnimation(animID))
                {
                    int sequenceNum = Interlocked.Increment(ref sequenceCounter);
                    animations.Add(new Animation(animID, sequenceNum));
                    return true;
                }
            }

            return false;
        }

        public bool Add(UUID animID, int sequenceNum)
        {
            lock (animations)
            {
                if (!HasAnimation(animID))
                {
                    animations.Add(new Animation(animID, sequenceNum));
                    return true;
                }
            }

            return false;
        }

        public bool Remove(UUID animID)
        {
            if (defaultAnimation.ID == animID)
            {
                ResetDefaultAnimation();
                return true;
            }
            else if (HasAnimation(animID))
            {
                lock (animations)
                {
                    for (int i = 0; i < animations.Count; i++)
                    {
                        if (animations[i].ID == animID)
                        {
                            animations.RemoveAt(i);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Clear()
        {
            ResetDefaultAnimation();
            lock (animations) animations.Clear();
        }

        public bool SetDefaultAnimation(UUID animID, ref int sequenceCounter)
        {
            if (defaultAnimation.ID != animID)
            {
                int sequenceNum = Interlocked.Increment(ref sequenceCounter);
                defaultAnimation = new Animation(animID, sequenceNum);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SetDefaultAnimation(UUID animID, int sequenceNum)
        {
            if (defaultAnimation.ID != animID)
            {
                defaultAnimation = new Animation(animID, sequenceNum);
                return true;
            }
            else
            {
                return false;
            }
        }

        public AnimationTrigger[] GetAnimations()
        {
            lock (animations)
            {
                AnimationTrigger[] triggers = new AnimationTrigger[animations.Count + 1];

                triggers[0] = new AnimationTrigger(defaultAnimation.ID, defaultAnimation.SequenceNum);

                for (int i = 0; i < animations.Count; i++)
                    triggers[i + 1] = new AnimationTrigger(animations[i].ID, animations[i].SequenceNum);

                return triggers;
            }
        }

        protected bool ResetDefaultAnimation()
        {
            return SetDefaultAnimation(Animations.STAND, 1);
        }
    }
}
