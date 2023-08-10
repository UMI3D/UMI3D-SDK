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
    [TestFixture, TestOf(typeof(BoneTypeHelper))]
    public class BoneTypeHelper_Test
    {
        #region GetBoneName

        [Test]
        public void GetBoneName_Test([Random(1, 70, 100)] int bonetype)
        {
            // GIVEN

            // WHEN
            string name = BoneTypeHelper.GetBoneName((uint)bonetype);

            // THEN
            switch ((uint)bonetype)
            {
                case BoneType.Hips:
                    Assert.IsTrue(name.Equals("Hips"));
                    break;

                case BoneType.LeftHip:
                    Assert.IsTrue(name.Equals("LeftHip"));
                    break;

                case BoneType.RightHip:
                    Assert.IsTrue(name.Equals("RightHip"));
                    break;

                case BoneType.LeftKnee:
                    Assert.IsTrue(name.Equals("LeftKnee"));
                    break;

                case BoneType.RightKnee:
                    Assert.IsTrue(name.Equals("RightKnee"));
                    break;

                case BoneType.LeftAnkle:
                    Assert.IsTrue(name.Equals("LeftAnkle"));
                    break;

                case BoneType.RightAnkle:
                    Assert.IsTrue(name.Equals("RightAnkle"));
                    break;

                case BoneType.Spine:
                    Assert.IsTrue(name.Equals("Spine"));
                    break;

                case BoneType.Chest:
                    Assert.IsTrue(name.Equals("Chest"));
                    break;

                case BoneType.UpperChest:
                    Assert.IsTrue(name.Equals("UpperChest"));
                    break;

                case BoneType.Neck:
                    Assert.IsTrue(name.Equals("Neck"));
                    break;

                case BoneType.Head:
                    Assert.IsTrue(name.Equals("Head"));
                    break;

                case BoneType.LeftShoulder:
                    Assert.IsTrue(name.Equals("LeftShoulder"));
                    break;

                case BoneType.RightShoulder:
                    Assert.IsTrue(name.Equals("RightShoulder"));
                    break;

                case BoneType.LeftUpperArm:
                    Assert.IsTrue(name.Equals("LeftUpperArm"));
                    break;

                case BoneType.RightUpperArm:
                    Assert.IsTrue(name.Equals("RightUpperArm"));
                    break;

                case BoneType.LeftForearm:
                    Assert.IsTrue(name.Equals("LeftForearm"));
                    break;

                case BoneType.RightForearm:
                    Assert.IsTrue(name.Equals("RightForearm"));
                    break;

                case BoneType.LeftHand:
                    Assert.IsTrue(name.Equals("LeftHand"));
                    break;

                case BoneType.RightHand:
                    Assert.IsTrue(name.Equals("RightHand"));
                    break;

                case BoneType.LeftToeBase:
                    Assert.IsTrue(name.Equals("LeftToeBase"));
                    break;

                case BoneType.RightToeBase:
                    Assert.IsTrue(name.Equals("RightToeBase"));
                    break;

                case BoneType.LeftEye:
                    Assert.IsTrue(name.Equals("LeftEye"));
                    break;

                case BoneType.RightEye:
                    Assert.IsTrue(name.Equals("RightEye"));
                    break;

                case BoneType.Jaw:
                    Assert.IsTrue(name.Equals("Jaw"));
                    break;

                case BoneType.LeftThumbProximal:
                    Assert.IsTrue(name.Equals("LeftThumbProximal"));
                    break;

                case BoneType.LeftThumbIntermediate:
                    Assert.IsTrue(name.Equals("LeftThumbIntermediate"));
                    break;

                case BoneType.LeftThumbDistal:
                    Assert.IsTrue(name.Equals("LeftThumbDistal"));
                    break;

                case BoneType.LeftIndexProximal:
                    Assert.IsTrue(name.Equals("LeftIndexProximal"));
                    break;

                case BoneType.LeftIndexIntermediate:
                    Assert.IsTrue(name.Equals("LeftIndexIntermediate"));
                    break;

                case BoneType.LeftIndexDistal:
                    Assert.IsTrue(name.Equals("LeftIndexDistal"));
                    break;

                case BoneType.LeftMiddleProximal:
                    Assert.IsTrue(name.Equals("LeftMiddleProximal"));
                    break;

                case BoneType.LeftMiddleIntermediate:
                    Assert.IsTrue(name.Equals("LeftMiddleIntermediate"));
                    break;

                case BoneType.LeftMiddleDistal:
                    Assert.IsTrue(name.Equals("LeftMiddleDistal"));
                    break;

                case BoneType.LeftRingProximal:
                    Assert.IsTrue(name.Equals("LeftRingProximal"));
                    break;

                case BoneType.LeftRingIntermediate:
                    Assert.IsTrue(name.Equals("LeftRingIntermediate"));
                    break;

                case BoneType.LeftRingDistal:
                    Assert.IsTrue(name.Equals("LeftRingDistal"));
                    break;

                case BoneType.LeftLittleProximal:
                    Assert.IsTrue(name.Equals("LeftLittleProximal"));
                    break;

                case BoneType.LeftLittleIntermediate:
                    Assert.IsTrue(name.Equals("LeftLittleIntermediate"));
                    break;

                case BoneType.LeftLittleDistal:
                    Assert.IsTrue(name.Equals("LeftLittleDistal"));
                    break;

                case BoneType.RightThumbProximal:
                    Assert.IsTrue(name.Equals("RightThumbProximal"));
                    break;

                case BoneType.RightThumbIntermediate:
                    Assert.IsTrue(name.Equals("RightThumbIntermediate"));
                    break;

                case BoneType.RightThumbDistal:
                    Assert.IsTrue(name.Equals("RightThumbDistal"));
                    break;

                case BoneType.RightIndexProximal:
                    Assert.IsTrue(name.Equals("RightIndexProximal"));
                    break;

                case BoneType.RightIndexIntermediate:
                    Assert.IsTrue(name.Equals("RightIndexIntermediate"));
                    break;

                case BoneType.RightIndexDistal:
                    Assert.IsTrue(name.Equals("RightIndexDistal"));
                    break;

                case BoneType.RightMiddleProximal:
                    Assert.IsTrue(name.Equals("RightMiddleProximal"));
                    break;

                case BoneType.RightMiddleIntermediate:
                    Assert.IsTrue(name.Equals("RightMiddleIntermediate"));
                    break;

                case BoneType.RightMiddleDistal:
                    Assert.IsTrue(name.Equals("RightMiddleDistal"));
                    break;

                case BoneType.RightRingProximal:
                    Assert.IsTrue(name.Equals("RightRingProximal"));
                    break;

                case BoneType.RightRingIntermediate:
                    Assert.IsTrue(name.Equals("RightRingIntermediate"));
                    break;

                case BoneType.RightRingDistal:
                    Assert.IsTrue(name.Equals("RightRingDistal"));
                    break;

                case BoneType.RightLittleProximal:
                    Assert.IsTrue(name.Equals("RightLittleProximal"));
                    break;

                case BoneType.RightLittleIntermediate:
                    Assert.IsTrue(name.Equals("RightLittleIntermediate"));
                    break;

                case BoneType.RightLittleDistal:
                    Assert.IsTrue(name.Equals("RightLittleDistal"));
                    break;

                case BoneType.LastBone:
                    Assert.IsTrue(name.Equals("LastBone"));
                    break;

                case BoneType.Viewpoint:
                    Assert.IsTrue(name.Equals("Viewpoint"));
                    break;

                case BoneType.CenterFeet:
                    Assert.IsTrue(name.Equals("CenterFeet"));
                    break;

                default:
                    Assert.IsTrue(name.Equals("Custom bone"));
                    break;
            }
        }

        #endregion GetBoneName
    }
}