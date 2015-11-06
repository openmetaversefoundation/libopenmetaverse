/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace OpenMetaverse.Rendering
{
    /// <summary>
    /// load the 'avatar_skeleton.xml'
    /// </summary>
    /// <remarks>
    /// Partial class which extends the auto-generated 'LindenSkeleton.Xsd.cs'.eton.xsd
    /// </remarks>
    public partial class LindenSkeleton
   {
        /// <summary>
        /// Load a skeleton from a given file.
        /// </summary>
        /// <remarks>
        /// We use xml scema validation on top of the xml de-serializer, since the schema has
        /// some stricter checks than the de-serializer provides. E.g. the vector attributes
        /// are guaranteed to hold only 3 float values. This reduces the need for error checking
        /// while working with the loaded skeleton.
        /// </remarks>
        /// <returns>A valid recursive skeleton</returns>
        public static LindenSkeleton Load()
        {
            return Load(null);
        }

        /// <summary>
        /// Load a skeleton from a given file.
        /// </summary>
        /// <remarks>
        /// We use xml scema validation on top of the xml de-serializer, since the schema has
        /// some stricter checks than the de-serializer provides. E.g. the vector attributes
        /// are guaranteed to hold only 3 float values. This reduces the need for error checking
        /// while working with the loaded skeleton.
        /// </remarks>
        /// <param name="fileName">The path to the skeleton definition file</param>
        /// <returns>A valid recursive skeleton</returns>
        public static LindenSkeleton Load(string fileName)
        {
            if (fileName == null)
                fileName = System.IO.Path.Combine(Settings.RESOURCE_DIR, "avatar_skeleton.xml");

            LindenSkeleton result;

            using (FileStream skeletonData = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (XmlReader reader = XmlReader.Create(skeletonData))
            {
                XmlSerializer ser = new XmlSerializer(typeof(LindenSkeleton));
                result = (LindenSkeleton)ser.Deserialize(reader);
            }
            return result;
        }

        /// <summary>
        /// Build and "expanded" list of joints
        /// </summary>
        /// <remarks>
        /// The algorithm is based on this description:
        /// 
        /// >An "expanded" list of joints, not just a
        /// >linear array of the joints as defined in the skeleton file.
        /// >In particular, any joint that has more than one child will
        /// >be repeated in the list for each of its children.
        /// </remarks>
        /// <param name="jointsFilter">The list should only take these joint names in consideration</param>
        /// <returns>An "expanded" joints list as a flat list of bone names</returns>
        public List<string> BuildExpandedJointList(IEnumerable<string> jointsFilter)
        {
            List<string> expandedJointList = new List<string>();

            // not really sure about this algorithm, but it seems to fit the bill:
            // and the mesh doesn't seem to be overly distorted
            if(bone.bone != null)
                foreach (Joint child in bone.bone)
                    ExpandJoint(bone, child, expandedJointList, jointsFilter);

            return expandedJointList;
        }

        /// <summary>
        /// Expand one joint
        /// </summary>
        /// <param name="parentJoint">The parent of the joint we are operating on</param>
        /// <param name="currentJoint">The joint we are supposed to expand</param>
        /// <param name="expandedJointList">Joint list that we will extend upon</param>
        /// <param name="jointsFilter">The expanded list should only contain these joints</param>
        private void ExpandJoint(Joint parentJoint, Joint currentJoint, List<string> expandedJointList, IEnumerable<string> jointsFilter)
        {
            // does the mesh reference this joint
            if (jointsFilter.Contains(currentJoint.name))
            {
                if (expandedJointList.Count > 0 && parentJoint != null &&
                    parentJoint.name == expandedJointList[expandedJointList.Count - 1])
                    expandedJointList.Add(currentJoint.name);
                else
                {
                    if (parentJoint != null)
                        expandedJointList.Add(parentJoint.name);
                    else
                        expandedJointList.Add(currentJoint.name);        // only happens on the root joint

                    expandedJointList.Add(currentJoint.name);
                }
            }

            // recurse the joint hierarchy
            if(currentJoint.bone != null)
                foreach (Joint child in currentJoint.bone)
                    ExpandJoint(currentJoint, child, expandedJointList, jointsFilter);
        }
    }
}
