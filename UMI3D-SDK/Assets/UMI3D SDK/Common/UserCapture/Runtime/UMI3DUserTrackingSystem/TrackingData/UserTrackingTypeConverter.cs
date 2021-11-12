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

using UnityEngine;

namespace umi3d.common.userCapture
{
    public static class BoneTypeConverter
    {
        public static uint Convert(this HumanBodyBones HumanBodyBones)
        {
            switch (HumanBodyBones)
            {
                case HumanBodyBones.Hips:
                    return BoneType.Hips;
                case HumanBodyBones.LeftUpperLeg:
                    return BoneType.LeftHip;
                case HumanBodyBones.RightUpperLeg:
                    return BoneType.RightHip;
                case HumanBodyBones.LeftLowerLeg:
                    return BoneType.LeftKnee;
                case HumanBodyBones.RightLowerLeg:
                    return BoneType.RightKnee;
                case HumanBodyBones.LeftFoot:
                    return BoneType.LeftAnkle;
                case HumanBodyBones.RightFoot:
                    return BoneType.RightAnkle;
                case HumanBodyBones.Spine:
                    return BoneType.Spine;
                case HumanBodyBones.Chest:
                    return BoneType.Chest;
                case HumanBodyBones.UpperChest:
                    return BoneType.UpperChest;
                case HumanBodyBones.Neck:
                    return BoneType.Neck;
                case HumanBodyBones.Head:
                    return BoneType.Head;
                case HumanBodyBones.LeftShoulder:
                    return BoneType.LeftShoulder;
                case HumanBodyBones.RightShoulder:
                    return BoneType.RightShoulder;
                case HumanBodyBones.LeftUpperArm:
                    return BoneType.LeftUpperArm;
                case HumanBodyBones.RightUpperArm:
                    return BoneType.RightUpperArm;
                case HumanBodyBones.LeftLowerArm:
                    return BoneType.LeftForearm;
                case HumanBodyBones.RightLowerArm:
                    return BoneType.RightForearm;
                case HumanBodyBones.LeftHand:
                    return BoneType.LeftHand;
                case HumanBodyBones.RightHand:
                    return BoneType.RightHand;
                case HumanBodyBones.LeftToes:
                    return BoneType.LeftToeBase;
                case HumanBodyBones.RightToes:
                    return BoneType.RightToeBase;
                case HumanBodyBones.LeftEye:
                    return BoneType.LeftEye;
                case HumanBodyBones.RightEye:
                    return BoneType.RightEye;
                case HumanBodyBones.Jaw:
                    return BoneType.Jaw;
                case HumanBodyBones.LeftThumbProximal:
                    return BoneType.LeftThumbProximal;
                case HumanBodyBones.LeftThumbIntermediate:
                    return BoneType.LeftThumbIntermediate;
                case HumanBodyBones.LeftThumbDistal:
                    return BoneType.LeftThumbDistal;
                case HumanBodyBones.LeftIndexProximal:
                    return BoneType.LeftIndexProximal;
                case HumanBodyBones.LeftIndexIntermediate:
                    return BoneType.LeftIndexIntermediate;
                case HumanBodyBones.LeftIndexDistal:
                    return BoneType.LeftIndexDistal;
                case HumanBodyBones.LeftMiddleProximal:
                    return BoneType.LeftMiddleProximal;
                case HumanBodyBones.LeftMiddleIntermediate:
                    return BoneType.LeftMiddleIntermediate;
                case HumanBodyBones.LeftMiddleDistal:
                    return BoneType.LeftMiddleDistal;
                case HumanBodyBones.LeftRingProximal:
                    return BoneType.LeftRingProximal;
                case HumanBodyBones.LeftRingIntermediate:
                    return BoneType.LeftRingIntermediate;
                case HumanBodyBones.LeftRingDistal:
                    return BoneType.LeftRingDistal;
                case HumanBodyBones.LeftLittleProximal:
                    return BoneType.LeftLittleProximal;
                case HumanBodyBones.LeftLittleIntermediate:
                    return BoneType.LeftLittleIntermediate;
                case HumanBodyBones.LeftLittleDistal:
                    return BoneType.LeftLittleDistal;
                case HumanBodyBones.RightThumbProximal:
                    return BoneType.RightThumbProximal;
                case HumanBodyBones.RightThumbIntermediate:
                    return BoneType.RightThumbIntermediate;
                case HumanBodyBones.RightThumbDistal:
                    return BoneType.RightThumbDistal;
                case HumanBodyBones.RightIndexProximal:
                    return BoneType.RightIndexProximal;
                case HumanBodyBones.RightIndexIntermediate:
                    return BoneType.RightIndexIntermediate;
                case HumanBodyBones.RightIndexDistal:
                    return BoneType.RightIndexDistal;
                case HumanBodyBones.RightMiddleProximal:
                    return BoneType.RightMiddleProximal;
                case HumanBodyBones.RightMiddleIntermediate:
                    return BoneType.RightMiddleIntermediate;
                case HumanBodyBones.RightMiddleDistal:
                    return BoneType.RightMiddleDistal;
                case HumanBodyBones.RightRingProximal:
                    return BoneType.RightRingProximal;
                case HumanBodyBones.RightRingIntermediate:
                    return BoneType.RightRingIntermediate;
                case HumanBodyBones.RightRingDistal:
                    return BoneType.RightRingDistal;
                case HumanBodyBones.RightLittleProximal:
                    return BoneType.RightLittleProximal;
                case HumanBodyBones.RightLittleIntermediate:
                    return BoneType.RightLittleIntermediate;
                case HumanBodyBones.RightLittleDistal:
                    return BoneType.RightLittleDistal;
                case HumanBodyBones.LastBone:
                    return BoneType.LastBone;
                default:
                    return BoneType.None;
            }
        }

        public static HumanBodyBones? ConvertToBoneType(this uint boneType)
        {
            switch (boneType)
            {
                case BoneType.Hips:
                    return HumanBodyBones.Hips;
                case BoneType.LeftHip:
                    return HumanBodyBones.LeftUpperLeg;
                case BoneType.RightHip:
                    return HumanBodyBones.RightUpperLeg;
                case BoneType.LeftKnee:
                    return HumanBodyBones.LeftLowerLeg;
                case BoneType.RightKnee:
                    return HumanBodyBones.RightLowerLeg;
                case BoneType.LeftAnkle:
                    return HumanBodyBones.LeftFoot;
                case BoneType.RightAnkle:
                    return HumanBodyBones.RightFoot;
                case BoneType.Spine:
                    return HumanBodyBones.Spine;
                case BoneType.Chest:
                    return HumanBodyBones.Chest;
                case BoneType.UpperChest:
                    return HumanBodyBones.UpperChest;
                case BoneType.Neck:
                    return HumanBodyBones.Neck;
                case BoneType.Head:
                    return HumanBodyBones.Head;
                case BoneType.LeftShoulder:
                    return HumanBodyBones.LeftShoulder;
                case BoneType.RightShoulder:
                    return HumanBodyBones.RightShoulder;
                case BoneType.LeftUpperArm:
                    return HumanBodyBones.LeftUpperArm;
                case BoneType.RightUpperArm:
                    return HumanBodyBones.RightUpperArm;
                case BoneType.LeftForearm:
                    return HumanBodyBones.LeftLowerArm;
                case BoneType.RightForearm:
                    return HumanBodyBones.RightLowerArm;
                case BoneType.LeftHand:
                    return HumanBodyBones.LeftHand;
                case BoneType.RightHand:
                    return HumanBodyBones.RightHand;
                case BoneType.LeftToeBase:
                    return HumanBodyBones.LeftToes;
                case BoneType.RightToeBase:
                    return HumanBodyBones.RightToes;
                case BoneType.LeftEye:
                    return HumanBodyBones.LeftEye;
                case BoneType.RightEye:
                    return HumanBodyBones.RightEye;
                case BoneType.Jaw:
                    return HumanBodyBones.Jaw;
                case BoneType.LeftThumbProximal:
                    return HumanBodyBones.LeftThumbProximal;
                case BoneType.LeftThumbIntermediate:
                    return HumanBodyBones.LeftThumbIntermediate;
                case BoneType.LeftThumbDistal:
                    return HumanBodyBones.LeftThumbDistal;
                case BoneType.LeftIndexProximal:
                    return HumanBodyBones.LeftIndexProximal;
                case BoneType.LeftIndexIntermediate:
                    return HumanBodyBones.LeftIndexIntermediate;
                case BoneType.LeftIndexDistal:
                    return HumanBodyBones.LeftIndexDistal;
                case BoneType.LeftMiddleProximal:
                    return HumanBodyBones.LeftMiddleProximal;
                case BoneType.LeftMiddleIntermediate:
                    return HumanBodyBones.LeftMiddleIntermediate;
                case BoneType.LeftMiddleDistal:
                    return HumanBodyBones.LeftMiddleDistal;
                case BoneType.LeftRingProximal:
                    return HumanBodyBones.LeftRingProximal;
                case BoneType.LeftRingIntermediate:
                    return HumanBodyBones.LeftRingIntermediate;
                case BoneType.LeftRingDistal:
                    return HumanBodyBones.LeftRingDistal;
                case BoneType.LeftLittleProximal:
                    return HumanBodyBones.LeftLittleProximal;
                case BoneType.LeftLittleIntermediate:
                    return HumanBodyBones.LeftLittleIntermediate;
                case BoneType.LeftLittleDistal:
                    return HumanBodyBones.LeftLittleDistal;
                case BoneType.RightThumbProximal:
                    return HumanBodyBones.RightThumbProximal;
                case BoneType.RightThumbIntermediate:
                    return HumanBodyBones.RightThumbIntermediate;
                case BoneType.RightThumbDistal:
                    return HumanBodyBones.RightThumbDistal;
                case BoneType.RightIndexProximal:
                    return HumanBodyBones.RightIndexProximal;
                case BoneType.RightIndexIntermediate:
                    return HumanBodyBones.RightIndexIntermediate;
                case BoneType.RightIndexDistal:
                    return HumanBodyBones.RightIndexDistal;
                case BoneType.RightMiddleProximal:
                    return HumanBodyBones.RightMiddleProximal;
                case BoneType.RightMiddleIntermediate:
                    return HumanBodyBones.RightMiddleIntermediate;
                case BoneType.RightMiddleDistal:
                    return HumanBodyBones.RightMiddleDistal;
                case BoneType.RightRingProximal:
                    return HumanBodyBones.RightRingProximal;
                case BoneType.RightRingIntermediate:
                    return HumanBodyBones.RightRingIntermediate;
                case BoneType.RightRingDistal:
                    return HumanBodyBones.RightRingDistal;
                case BoneType.RightLittleProximal:
                    return HumanBodyBones.RightLittleProximal;
                case BoneType.RightLittleIntermediate:
                    return HumanBodyBones.RightLittleIntermediate;
                case BoneType.RightLittleDistal:
                    return HumanBodyBones.RightLittleDistal;
                case BoneType.LastBone:
                    return HumanBodyBones.LastBone;
                default:
                    return null;
            }
        }

    }
}
