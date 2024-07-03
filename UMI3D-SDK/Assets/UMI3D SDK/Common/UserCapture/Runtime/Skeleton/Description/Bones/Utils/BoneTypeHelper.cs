/*
Copyright 2019 - 2023 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// Utils methods for UMI3D bone-related operations.
    /// </summary>
    public static class BoneTypeHelper
    {
        private static readonly Dictionary<uint, string> BoneNames = new();

        /// <summary>
        /// Get all UMI3D bone names indexed by UMI3D <see cref="BoneType"/> index.
        /// </summary>
        /// <returns>UMI3D bone names indexed by UMI3D <see cref="BoneType"/> index</returns>
        public static Dictionary<uint, string> GetBoneNames()
        {
            if (BoneNames.Count > 0)
                return BoneNames;

            var keyValues = typeof(BoneType).GetFields().Select(f => (f.Name, boneType: (uint)f.GetRawConstantValue()));
            foreach (var keyValue in keyValues)
            {
                BoneNames.Add(keyValue.boneType, keyValue.Name);
            }
            return BoneNames;
        }

        /// <summary>
        /// Get the name of a UMI3D bone.
        /// </summary>
        /// <param name="bone">Bone key in <see cref="BoneType"/></param>
        /// <returns>UMI3D bone name. "Custom bone" if no correspondign bone is found.</returns>
        public static string GetBoneName(uint bone)
        {
            return GetBoneNames().ContainsKey(bone) ? BoneNames[bone] : "Custom bone";
        }

        public static IReadOnlyDictionary<uint, uint> Symmetries => symmetries;

        private static readonly Dictionary<uint, uint> symmetries = new Dictionary<uint, uint>()
        {
                { BoneType.LeftEye , BoneType.RightEye },

                { BoneType.LeftHip , BoneType.RightHip },
                { BoneType.LeftKnee , BoneType.RightKnee },
                { BoneType.LeftAnkle , BoneType.RightAnkle },
                { BoneType.LeftToeBase , BoneType.RightToeBase },

                { BoneType.LeftShoulder , BoneType.RightShoulder },
                { BoneType.LeftUpperArm , BoneType.RightUpperArm },
                { BoneType.LeftForearm , BoneType.RightForearm },
                { BoneType.LeftHand , BoneType.RightHand },

                { BoneType.LeftThumbProximal , BoneType.RightThumbProximal },
                { BoneType.LeftThumbIntermediate , BoneType.RightThumbIntermediate },
                { BoneType.LeftThumbDistal , BoneType.RightThumbDistal },

                { BoneType.LeftIndexProximal , BoneType.RightIndexProximal },
                { BoneType.LeftIndexIntermediate , BoneType.RightIndexIntermediate },
                { BoneType.LeftIndexDistal , BoneType.RightIndexDistal },

                { BoneType.LeftMiddleProximal , BoneType.RightMiddleProximal },
                { BoneType.LeftMiddleIntermediate , BoneType.RightMiddleIntermediate },
                { BoneType.LeftMiddleDistal , BoneType.RightLittleDistal },

                { BoneType.LeftRingProximal , BoneType.RightRingProximal },
                { BoneType.LeftRingIntermediate , BoneType.RightRingIntermediate },
                { BoneType.LeftRingDistal , BoneType.RightRingDistal },

                { BoneType.LeftLittleProximal , BoneType.RightLittleProximal },
                { BoneType.LeftLittleIntermediate , BoneType.RightLittleIntermediate },
                { BoneType.LeftLittleDistal , BoneType.RightLittleDistal },

                // RIGHT -> LEFT
                { BoneType.RightEye , BoneType.LeftHip },

                { BoneType.RightHip , BoneType.LeftHip },
                { BoneType.RightKnee , BoneType.LeftKnee },
                { BoneType.RightAnkle , BoneType.LeftAnkle },
                { BoneType.RightToeBase , BoneType.LeftToeBase },

                { BoneType.RightShoulder , BoneType.LeftShoulder },
                { BoneType.RightUpperArm , BoneType.LeftUpperArm },
                { BoneType.RightForearm , BoneType.LeftForearm },
                { BoneType.RightHand , BoneType.LeftHand },

                { BoneType.RightThumbProximal , BoneType.LeftThumbProximal },
                { BoneType.RightThumbIntermediate , BoneType.LeftThumbIntermediate },
                { BoneType.RightThumbDistal , BoneType.LeftThumbDistal },

                { BoneType.RightIndexProximal , BoneType.LeftIndexProximal },
                { BoneType.RightIndexIntermediate , BoneType.LeftIndexIntermediate },
                { BoneType.RightIndexDistal , BoneType.LeftIndexDistal },

                { BoneType.RightMiddleProximal , BoneType.LeftMiddleProximal },
                { BoneType.RightMiddleIntermediate , BoneType.LeftMiddleIntermediate },
                { BoneType.RightMiddleDistal , BoneType.LeftLittleDistal },

                { BoneType.RightRingProximal , BoneType.LeftRingProximal },
                { BoneType.RightRingIntermediate , BoneType.LeftRingIntermediate },
                { BoneType.RightRingDistal , BoneType.LeftRingDistal },

                { BoneType.RightLittleProximal , BoneType.LeftLittleProximal },
                { BoneType.RightLittleIntermediate , BoneType.LeftLittleIntermediate },
                { BoneType.RightLittleDistal , BoneType.LeftLittleDistal },
        };

        public static uint GetSymmetricBoneType(uint boneType)
        {
            if (!symmetries.ContainsKey(boneType))
                return boneType;

            else return symmetries[boneType];
        }

        /// <summary>
        /// Give the depth of each bone in a standard hierarchy.
        /// </summary>
        public static uint StandardPriority(uint boneType)
        {
            if (standardPriority.TryGetValue(boneType, out uint priority))
                return priority;

            else
                return standardPriority[BoneType.LastBone] + 1;
        }

        /// <summary>
        /// Depth of each bone in a standard hierarchy
        /// </summary>
        private static readonly Dictionary<uint, uint> standardPriority = new Dictionary<uint, uint>()
        {
            { BoneType.None , 0},

            { BoneType.Hips , 1},

            { BoneType.Spine , 2},
            { BoneType.Chest , 3},
            { BoneType.UpperChest , 4},


            { BoneType.Neck , 5},
            { BoneType.Head , 6},

            { BoneType.Jaw , 7},

            { BoneType.LeftEye ,7},
            { BoneType.RightEye , 7},

            { BoneType.LeftShoulder , 5},
            { BoneType.LeftUpperArm , 6},
            { BoneType.LeftForearm , 7},
            { BoneType.LeftHand , 8},

            { BoneType.RightShoulder , 5},
            { BoneType.RightUpperArm , 6},
            { BoneType.RightForearm , 7},
            { BoneType.RightHand , 8},

            { BoneType.LeftHip , 2},
            { BoneType.LeftKnee , 3},
            { BoneType.LeftAnkle , 4},
            { BoneType.LeftToeBase , 5},

            { BoneType.RightHip , 2},
            { BoneType.RightKnee , 3},
            { BoneType.RightAnkle , 4},
            { BoneType.RightToeBase , 5},


            { BoneType.LeftThumbProximal , 9},
            { BoneType.LeftThumbIntermediate , 10},
            { BoneType.LeftThumbDistal , 11},
            { BoneType.LeftIndexProximal , 9},
            { BoneType.LeftIndexIntermediate , 10},
            { BoneType.LeftIndexDistal , 11},
            { BoneType.LeftMiddleProximal , 9},
            { BoneType.LeftMiddleIntermediate , 10},
            { BoneType.LeftMiddleDistal , 11},
            { BoneType.LeftRingProximal , 9},
            { BoneType.LeftRingIntermediate , 10},
            { BoneType.LeftRingDistal , 11},
            { BoneType.LeftLittleProximal , 9},
            { BoneType.LeftLittleIntermediate , 10},
            { BoneType.LeftLittleDistal , 11},

            { BoneType.RightThumbProximal , 9},
            { BoneType.RightThumbIntermediate , 10},
            { BoneType.RightThumbDistal , 11},
            { BoneType.RightIndexProximal , 9},
            { BoneType.RightIndexIntermediate , 10},
            { BoneType.RightIndexDistal , 11},
            { BoneType.RightMiddleProximal , 9},
            { BoneType.RightMiddleIntermediate , 10},
            { BoneType.RightMiddleDistal , 11},
            { BoneType.RightRingProximal , 9},
            { BoneType.RightRingIntermediate , 10},
            { BoneType.RightRingDistal , 11},
            { BoneType.RightLittleProximal , 9},
            { BoneType.RightLittleIntermediate , 10},
            { BoneType.RightLittleDistal , 11},

            { BoneType.CenterFeet , 15},
            { BoneType.Viewpoint , 15},
            { BoneType.LastBone , 16},
        };
    }
}