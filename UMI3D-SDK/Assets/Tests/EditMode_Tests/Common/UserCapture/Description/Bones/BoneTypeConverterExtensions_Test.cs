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

using NUnit.Framework;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Description.Common
{
    [TestFixture, TestOf(typeof(BoneTypeConvertingExtensions))]
    public class BoneTypeConverterExtensions_Test
    {
        #region Convert

        [Test]
        public void Convert_Test([Random(1, 70, 100)] int humanBodyBone)
        {
            // GIVEN

            // WHEN
            var bonetype = BoneTypeConvertingExtensions.Convert((HumanBodyBones)humanBodyBone);

            // THEN
            switch ((HumanBodyBones)humanBodyBone)
            {
                case HumanBodyBones.Hips:
                    Assert.IsTrue(bonetype == BoneType.Hips);
                    break;

                case HumanBodyBones.LeftUpperLeg:
                    Assert.IsTrue(bonetype == BoneType.LeftHip);
                    break;

                case HumanBodyBones.RightUpperLeg:
                    Assert.IsTrue(bonetype == BoneType.RightHip);
                    break;

                case HumanBodyBones.LeftLowerLeg:
                    Assert.IsTrue(bonetype == BoneType.LeftKnee);
                    break;

                case HumanBodyBones.RightLowerLeg:
                    Assert.IsTrue(bonetype == BoneType.RightKnee);
                    break;

                case HumanBodyBones.LeftFoot:
                    Assert.IsTrue(bonetype == BoneType.LeftAnkle);
                    break;

                case HumanBodyBones.RightFoot:
                    Assert.IsTrue(bonetype == BoneType.RightAnkle);
                    break;

                case HumanBodyBones.Spine:
                    Assert.IsTrue(bonetype == BoneType.Spine);
                    break;

                case HumanBodyBones.Chest:
                    Assert.IsTrue(bonetype == BoneType.Chest);
                    break;

                case HumanBodyBones.UpperChest:
                    Assert.IsTrue(bonetype == BoneType.UpperChest);
                    break;

                case HumanBodyBones.Neck:
                    Assert.IsTrue(bonetype == BoneType.Neck);
                    break;

                case HumanBodyBones.Head:
                    Assert.IsTrue(bonetype == BoneType.Head);
                    break;

                case HumanBodyBones.LeftShoulder:
                    Assert.IsTrue(bonetype == BoneType.LeftShoulder);
                    break;

                case HumanBodyBones.RightShoulder:
                    Assert.IsTrue(bonetype == BoneType.RightShoulder);
                    break;

                case HumanBodyBones.LeftUpperArm:
                    Assert.IsTrue(bonetype == BoneType.LeftUpperArm);
                    break;

                case HumanBodyBones.RightUpperArm:
                    Assert.IsTrue(bonetype == BoneType.RightUpperArm);
                    break;

                case HumanBodyBones.LeftLowerArm:
                    Assert.IsTrue(bonetype == BoneType.LeftForearm);
                    break;

                case HumanBodyBones.RightLowerArm:
                    Assert.IsTrue(bonetype == BoneType.RightForearm);
                    break;

                case HumanBodyBones.LeftHand:
                    Assert.IsTrue(bonetype == BoneType.LeftHand);
                    break;

                case HumanBodyBones.RightHand:
                    Assert.IsTrue(bonetype == BoneType.RightHand);
                    break;

                case HumanBodyBones.LeftToes:
                    Assert.IsTrue(bonetype == BoneType.LeftToeBase);
                    break;

                case HumanBodyBones.RightToes:
                    Assert.IsTrue(bonetype == BoneType.RightToeBase);
                    break;

                case HumanBodyBones.LeftEye:
                    Assert.IsTrue(bonetype == BoneType.LeftEye);
                    break;

                case HumanBodyBones.RightEye:
                    Assert.IsTrue(bonetype == BoneType.RightEye);
                    break;

                case HumanBodyBones.Jaw:
                    Assert.IsTrue(bonetype == BoneType.Jaw);
                    break;

                case HumanBodyBones.LeftThumbProximal:
                    Assert.IsTrue(bonetype == BoneType.LeftThumbProximal);
                    break;

                case HumanBodyBones.LeftThumbIntermediate:
                    Assert.IsTrue(bonetype == BoneType.LeftThumbIntermediate);
                    break;

                case HumanBodyBones.LeftThumbDistal:
                    Assert.IsTrue(bonetype == BoneType.LeftThumbDistal);
                    break;

                case HumanBodyBones.LeftIndexProximal:
                    Assert.IsTrue(bonetype == BoneType.LeftIndexProximal);
                    break;

                case HumanBodyBones.LeftIndexIntermediate:
                    Assert.IsTrue(bonetype == BoneType.LeftIndexIntermediate);
                    break;

                case HumanBodyBones.LeftIndexDistal:
                    Assert.IsTrue(bonetype == BoneType.LeftIndexDistal);
                    break;

                case HumanBodyBones.LeftMiddleProximal:
                    Assert.IsTrue(bonetype == BoneType.LeftMiddleProximal);
                    break;

                case HumanBodyBones.LeftMiddleIntermediate:
                    Assert.IsTrue(bonetype == BoneType.LeftMiddleIntermediate);
                    break;

                case HumanBodyBones.LeftMiddleDistal:
                    Assert.IsTrue(bonetype == BoneType.LeftMiddleDistal);
                    break;

                case HumanBodyBones.LeftRingProximal:
                    Assert.IsTrue(bonetype == BoneType.LeftRingProximal);
                    break;

                case HumanBodyBones.LeftRingIntermediate:
                    Assert.IsTrue(bonetype == BoneType.LeftRingIntermediate);
                    break;

                case HumanBodyBones.LeftRingDistal:
                    Assert.IsTrue(bonetype == BoneType.LeftRingDistal);
                    break;

                case HumanBodyBones.LeftLittleProximal:
                    Assert.IsTrue(bonetype == BoneType.LeftLittleProximal);
                    break;

                case HumanBodyBones.LeftLittleIntermediate:
                    Assert.IsTrue(bonetype == BoneType.LeftLittleIntermediate);
                    break;

                case HumanBodyBones.LeftLittleDistal:
                    Assert.IsTrue(bonetype == BoneType.LeftLittleDistal);
                    break;

                case HumanBodyBones.RightThumbProximal:
                    Assert.IsTrue(bonetype == BoneType.RightThumbProximal);
                    break;

                case HumanBodyBones.RightThumbIntermediate:
                    Assert.IsTrue(bonetype == BoneType.RightThumbIntermediate);
                    break;

                case HumanBodyBones.RightThumbDistal:
                    Assert.IsTrue(bonetype == BoneType.RightThumbDistal);
                    break;

                case HumanBodyBones.RightIndexProximal:
                    Assert.IsTrue(bonetype == BoneType.RightIndexProximal);
                    break;

                case HumanBodyBones.RightIndexIntermediate:
                    Assert.IsTrue(bonetype == BoneType.RightIndexIntermediate);
                    break;

                case HumanBodyBones.RightIndexDistal:
                    Assert.IsTrue(bonetype == BoneType.RightIndexDistal);
                    break;

                case HumanBodyBones.RightMiddleProximal:
                    Assert.IsTrue(bonetype == BoneType.RightMiddleProximal);
                    break;

                case HumanBodyBones.RightMiddleIntermediate:
                    Assert.IsTrue(bonetype == BoneType.RightMiddleIntermediate);
                    break;

                case HumanBodyBones.RightMiddleDistal:
                    Assert.IsTrue(bonetype == BoneType.RightMiddleDistal);
                    break;

                case HumanBodyBones.RightRingProximal:
                    Assert.IsTrue(bonetype == BoneType.RightRingProximal);
                    break;

                case HumanBodyBones.RightRingIntermediate:
                    Assert.IsTrue(bonetype == BoneType.RightRingIntermediate);
                    break;

                case HumanBodyBones.RightRingDistal:
                    Assert.IsTrue(bonetype == BoneType.RightRingDistal);
                    break;

                case HumanBodyBones.RightLittleProximal:
                    Assert.IsTrue(bonetype == BoneType.RightLittleProximal);
                    break;

                case HumanBodyBones.RightLittleIntermediate:
                    Assert.IsTrue(bonetype == BoneType.RightLittleIntermediate);
                    break;

                case HumanBodyBones.RightLittleDistal:
                    Assert.IsTrue(bonetype == BoneType.RightLittleDistal);
                    break;

                case HumanBodyBones.LastBone:
                    Assert.IsTrue(bonetype == BoneType.LastBone);
                    break;

                default:
                    Assert.IsTrue(bonetype == BoneType.None);
                    break;
            }
        }

        #endregion Convert

        #region ConvertToBonetype

        [Test]
        public void ConvertToBonetype_Test([Random(1, 70, 100)] int bonetype)
        {
            // GIVEN

            // WHEN
            var bone = BoneTypeConvertingExtensions.ConvertToBoneType((uint)bonetype);

            // THEN
            switch ((uint)bonetype)
            {
                case BoneType.Hips:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.Hips));
                    break;

                case BoneType.LeftHip:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftUpperLeg));
                    break;

                case BoneType.RightHip:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightUpperLeg));
                    break;

                case BoneType.LeftKnee:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftLowerLeg));
                    break;

                case BoneType.RightKnee:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightLowerLeg));
                    break;

                case BoneType.LeftAnkle:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftFoot));
                    break;

                case BoneType.RightAnkle:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightFoot));
                    break;

                case BoneType.Spine:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.Spine));
                    break;

                case BoneType.Chest:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.Chest));
                    break;

                case BoneType.UpperChest:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.UpperChest));
                    break;

                case BoneType.Neck:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.Neck));
                    break;

                case BoneType.Head:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.Head));
                    break;

                case BoneType.LeftShoulder:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftShoulder));
                    break;

                case BoneType.RightShoulder:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightShoulder));
                    break;

                case BoneType.LeftUpperArm:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftUpperArm));
                    break;

                case BoneType.RightUpperArm:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightUpperArm));
                    break;

                case BoneType.LeftForearm:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftLowerArm));
                    break;

                case BoneType.RightForearm:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightLowerArm));
                    break;

                case BoneType.LeftHand:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftHand));
                    break;

                case BoneType.RightHand:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightHand));
                    break;

                case BoneType.LeftToeBase:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftToes));
                    break;

                case BoneType.RightToeBase:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightToes));
                    break;

                case BoneType.LeftEye:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftEye));
                    break;

                case BoneType.RightEye:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightEye));
                    break;

                case BoneType.Jaw:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.Jaw));
                    break;

                case BoneType.LeftThumbProximal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftThumbProximal));
                    break;

                case BoneType.LeftThumbIntermediate:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftThumbIntermediate));
                    break;

                case BoneType.LeftThumbDistal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftThumbDistal));
                    break;

                case BoneType.LeftIndexProximal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftIndexProximal));
                    break;

                case BoneType.LeftIndexIntermediate:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftIndexIntermediate));
                    break;

                case BoneType.LeftIndexDistal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftIndexDistal));
                    break;

                case BoneType.LeftMiddleProximal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftMiddleProximal));
                    break;

                case BoneType.LeftMiddleIntermediate:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftMiddleIntermediate));
                    break;

                case BoneType.LeftMiddleDistal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftMiddleDistal));
                    break;

                case BoneType.LeftRingProximal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftRingProximal));
                    break;

                case BoneType.LeftRingIntermediate:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftRingIntermediate));
                    break;

                case BoneType.LeftRingDistal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftRingDistal));
                    break;

                case BoneType.LeftLittleProximal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftLittleProximal));
                    break;

                case BoneType.LeftLittleIntermediate:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftLittleIntermediate));
                    break;

                case BoneType.LeftLittleDistal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LeftLittleDistal));
                    break;

                case BoneType.RightThumbProximal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightThumbProximal));
                    break;

                case BoneType.RightThumbIntermediate:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightThumbIntermediate));
                    break;

                case BoneType.RightThumbDistal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightThumbDistal));
                    break;

                case BoneType.RightIndexProximal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightIndexProximal));
                    break;

                case BoneType.RightIndexIntermediate:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightIndexIntermediate));
                    break;

                case BoneType.RightIndexDistal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightIndexDistal));
                    break;

                case BoneType.RightMiddleProximal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightMiddleProximal));
                    break;

                case BoneType.RightMiddleIntermediate:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightMiddleIntermediate));
                    break;

                case BoneType.RightMiddleDistal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightMiddleDistal));
                    break;

                case BoneType.RightRingProximal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightRingProximal));
                    break;

                case BoneType.RightRingIntermediate:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightRingIntermediate));
                    break;

                case BoneType.RightRingDistal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightRingDistal));
                    break;

                case BoneType.RightLittleProximal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightLittleProximal));
                    break;

                case BoneType.RightLittleIntermediate:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightLittleIntermediate));
                    break;

                case BoneType.RightLittleDistal:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.RightLittleDistal));
                    break;

                case BoneType.LastBone:
                    Assert.IsTrue(bone.Equals(HumanBodyBones.LastBone));
                    break;

                default:
                    Assert.IsTrue(bone == null);
                    break;
            }
        }

        #endregion ConvertToBonetype
    }
}
