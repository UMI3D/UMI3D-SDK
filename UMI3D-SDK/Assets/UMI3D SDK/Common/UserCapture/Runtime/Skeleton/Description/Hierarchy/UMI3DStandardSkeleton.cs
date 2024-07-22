/*
Copyright 2019 - 2021 Inetum

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
using UnityEngine;

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// Standard skeleton hierarchy.
    /// </summary>
    public class UMI3DStandardSkeleton : MonoBehaviour
    {

        private readonly Dictionary<uint, Transform> bones = new();
        public IReadOnlyDictionary<uint, Transform> Bones => bones;

        /// <summary>
        /// Bones ordered by depth and instanciation order.
        /// </summary>
        public IReadOnlyList<uint> OrderedBones => orderedBones;

        private List<uint> orderedBones;

        [Header("Root")]
        [SerializeField]
        private Transform hips;

        [Header("Spine")]
        [SerializeField]
        private Transform spine;

        [SerializeField]
        private Transform chest;

        [SerializeField]
        private Transform upperChest;

        [Header("Head")]

        [SerializeField]
        private Transform neck;

        [SerializeField]
        private Transform head;

        [SerializeField]
        private Transform viewpoint;

        [SerializeField]
        private Transform leftEye;

        [SerializeField]
        private Transform rightEye;

        [Space(15)]
        [Header("Left Leg")]
        [SerializeField]
        private Transform leftHip;

        [SerializeField]
        private Transform leftKnee;

        [SerializeField]
        private Transform leftAnkle;

        [SerializeField]
        private Transform leftToebase;


        [Header("Left Arm")]
        [SerializeField]
        private Transform leftShoulder;

        [SerializeField]
        private Transform leftUpperArm;

        [SerializeField]
        private Transform leftForearm;


        [SerializeField]
        private Transform leftHand;

        [Header("Left Fingers")]

        [SerializeField]
        private Transform leftThumbProximal;

        [SerializeField]
        private Transform leftThumbIntermediate;

        [SerializeField]
        private Transform leftThumbDistal;

        [SerializeField]
        private Transform leftIndexProximal;

        [SerializeField]
        private Transform leftIndexIntermediate;

        [SerializeField]
        private Transform leftIndexDistal;

        [SerializeField]
        private Transform leftMiddleProximal;

        [SerializeField]
        private Transform leftMiddleIntermediate;

        [SerializeField]
        private Transform leftMiddleDistal;

        [SerializeField]
        private Transform leftRingProximal;

        [SerializeField]
        private Transform leftRingIntermediate;

        [SerializeField]
        private Transform leftRingDistal;

        [SerializeField]
        private Transform leftLittleProximal;

        [SerializeField]
        private Transform leftLittleIntermediate;

        [SerializeField]
        private Transform leftLittleDistal;

        [Space(15)]
        [Header("Right Leg")]
        [SerializeField]
        private Transform rightHip;

        [SerializeField]
        private Transform rightKnee;

        [SerializeField]
        private Transform rightAnkle;

        [SerializeField]
        private Transform rightToebase;


        [Header("Right Arm")]
        [SerializeField]
        private Transform rightShoulder;

        [SerializeField]
        private Transform rightUpperArm;

        [SerializeField]
        private Transform rightForearm;


        [SerializeField]
        private Transform rightHand;

        [Header("Right Fingers")]

        [SerializeField]
        private Transform rightThumbProximal;

        [SerializeField]
        private Transform rightThumbIntermediate;

        [SerializeField]
        private Transform rightThumbDistal;

        [SerializeField]
        private Transform rightIndexProximal;

        [SerializeField]
        private Transform rightIndexIntermediate;

        [SerializeField]
        private Transform rightIndexDistal;

        [SerializeField]
        private Transform rightMiddleProximal;

        [SerializeField]
        private Transform rightMiddleIntermediate;

        [SerializeField]
        private Transform rightMiddleDistal;

        [SerializeField]
        private Transform rightRingProximal;

        [SerializeField]
        private Transform rightRingIntermediate;

        [SerializeField]
        private Transform rightRingDistal;

        [SerializeField]
        private Transform rightLittleProximal;

        [SerializeField]
        private Transform rightLittleIntermediate;

        [SerializeField]
        private Transform rightLittleDistal;

        private bool isMapped;

        private void Start()
        {
            if (!isMapped)
                Map();
        }

        public void Map()
        {
            bones.Add(BoneType.Hips, hips);

            bones.Add(BoneType.Spine, spine);
            bones.Add(BoneType.Chest, chest);
            bones.Add(BoneType.UpperChest, upperChest);

            bones.Add(BoneType.Neck, neck);
            bones.Add(BoneType.Head, head);
            bones.Add(BoneType.Viewpoint, viewpoint);
            bones.Add(BoneType.LeftEye, leftEye);
            bones.Add(BoneType.RightEye, rightEye);

            bones.Add(BoneType.LeftHip, leftHip);
            bones.Add(BoneType.LeftKnee, leftKnee);
            bones.Add(BoneType.LeftAnkle, leftAnkle);
            bones.Add(BoneType.LeftToeBase, leftToebase);

            bones.Add(BoneType.LeftShoulder, leftShoulder);
            bones.Add(BoneType.LeftUpperArm, leftUpperArm);
            bones.Add(BoneType.LeftForearm, leftForearm);
            bones.Add(BoneType.LeftHand, leftHand);

            bones.Add(BoneType.LeftThumbProximal, leftThumbProximal);
            bones.Add(BoneType.LeftThumbIntermediate, leftThumbIntermediate);
            bones.Add(BoneType.LeftThumbDistal, leftThumbDistal);

            bones.Add(BoneType.LeftIndexProximal, leftIndexProximal);
            bones.Add(BoneType.LeftIndexIntermediate, leftIndexIntermediate);
            bones.Add(BoneType.LeftIndexDistal, leftIndexDistal);

            bones.Add(BoneType.LeftMiddleProximal, leftMiddleProximal);
            bones.Add(BoneType.LeftMiddleIntermediate, leftMiddleIntermediate);
            bones.Add(BoneType.LeftMiddleDistal, leftMiddleDistal);

            bones.Add(BoneType.LeftRingProximal, leftRingProximal);
            bones.Add(BoneType.LeftRingIntermediate, leftRingIntermediate);
            bones.Add(BoneType.LeftRingDistal, leftRingDistal);

            bones.Add(BoneType.LeftLittleProximal, leftLittleProximal);
            bones.Add(BoneType.LeftLittleIntermediate, leftLittleIntermediate);
            bones.Add(BoneType.LeftLittleDistal, leftLittleDistal);


            bones.Add(BoneType.RightHip, rightHip);
            bones.Add(BoneType.RightKnee, rightKnee);
            bones.Add(BoneType.RightAnkle, rightAnkle);
            bones.Add(BoneType.RightToeBase, rightToebase);

            bones.Add(BoneType.RightShoulder, rightShoulder);
            bones.Add(BoneType.RightUpperArm, rightUpperArm);
            bones.Add(BoneType.RightForearm, rightForearm);
            bones.Add(BoneType.RightHand, rightHand);

            bones.Add(BoneType.RightThumbProximal, rightThumbProximal);
            bones.Add(BoneType.RightThumbIntermediate, rightThumbIntermediate);
            bones.Add(BoneType.RightThumbDistal, rightThumbDistal);

            bones.Add(BoneType.RightIndexProximal, rightIndexProximal);
            bones.Add(BoneType.RightIndexIntermediate, rightIndexIntermediate);
            bones.Add(BoneType.RightIndexDistal, rightIndexDistal);

            bones.Add(BoneType.RightMiddleProximal, rightMiddleProximal);
            bones.Add(BoneType.RightMiddleIntermediate, rightMiddleIntermediate);
            bones.Add(BoneType.RightMiddleDistal, rightMiddleDistal);

            bones.Add(BoneType.RightRingProximal, rightRingProximal);
            bones.Add(BoneType.RightRingIntermediate, rightRingIntermediate);
            bones.Add(BoneType.RightRingDistal, rightRingDistal);

            bones.Add(BoneType.RightLittleProximal, rightLittleProximal);
            bones.Add(BoneType.RightLittleIntermediate, rightLittleIntermediate);
            bones.Add(BoneType.RightLittleDistal, rightLittleDistal);

            orderedBones = new()
            {
                BoneType.Hips,

                BoneType.Spine,
                BoneType.Chest,
                BoneType.UpperChest,

                BoneType.Neck,
                BoneType.Head,
                BoneType.Viewpoint,
                BoneType.LeftEye,
                BoneType.RightEye,

                BoneType.LeftHip,
                BoneType.LeftKnee,
                BoneType.LeftAnkle,
                BoneType.LeftToeBase,

                BoneType.LeftShoulder,
                BoneType.LeftUpperArm,
                BoneType.LeftForearm,
                BoneType.LeftHand,

                BoneType.LeftThumbProximal,
                BoneType.LeftThumbIntermediate,
                BoneType.LeftThumbDistal,

                BoneType.LeftIndexProximal,
                BoneType.LeftIndexIntermediate,
                BoneType.LeftIndexDistal,

                BoneType.LeftMiddleProximal,
                BoneType.LeftMiddleIntermediate,
                BoneType.LeftMiddleDistal,

                BoneType.LeftRingProximal,
                BoneType.LeftRingIntermediate,
                BoneType.LeftRingDistal,

                BoneType.LeftLittleProximal,
                BoneType.LeftLittleIntermediate,
                BoneType.LeftLittleDistal,


                BoneType.RightHip,
                BoneType.RightKnee,
                BoneType.RightAnkle,
                BoneType.RightToeBase,

                BoneType.RightShoulder,
                BoneType.RightUpperArm,
                BoneType.RightForearm,
                BoneType.RightHand,

                BoneType.RightThumbProximal,
                BoneType.RightThumbIntermediate,
                BoneType.RightThumbDistal,

                BoneType.RightIndexProximal,
                BoneType.RightIndexIntermediate,
                BoneType.RightIndexDistal,

                BoneType.RightMiddleProximal,
                BoneType.RightMiddleIntermediate,
                BoneType.RightMiddleDistal,

                BoneType.RightRingProximal,
                BoneType.RightRingIntermediate,
                BoneType.RightRingDistal,

                BoneType.RightLittleProximal,
                BoneType.RightLittleIntermediate,
                BoneType.RightLittleDistal,
        };

            isMapped = true;
        }

#if UNITY_EDITOR

        private Transform[] bonesCached;

        private const float BONE_RADIUS = 0.005f;

        private void OnDrawGizmosSelected()
        {
            if (bonesCached == null)
            {
                if (!isMapped)
                    Map();
                bonesCached = Bones.Values.ToArray();
            }

            foreach (var boneTransform in bonesCached)
            {
                Gizmos.DrawSphere(boneTransform.position, BONE_RADIUS);
            }
        }

#endif
    }
}
