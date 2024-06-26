﻿/*
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
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.userCapture.pose;
using umi3d.edk.userCapture.pose;

namespace EditMode_Tests.UserCapture.Pose.EDK
{
    [TestFixture, TestOf(nameof(UMI3DPoseAnimator))]
    public class UMI3DPoseAnimator_Test
    {
        // todo : move in PlayMode
        //#region Init

        //[Test]
        //public void Init()
        //{
        //    // GIVEN
        //    UMI3DPoseAnimator container = new(0, new List<IUMI3DPoseOverriderData>());

        //    // WHEN
        //    container.Init();

        //    // THEN
        //    Assert.IsNotNull(container.PoseOverriders);
        //    Assert.AreEqual(UMI3DPropertyKeys.ActivePoseOverrider, container.PoseOverriders.propertyId);
        //}

        //#endregion Init

        //#region ToEntityDto

        //[Test]
        //public void ToEntityDto()
        //{
        //    // GIVEN
        //    UMI3DPoseAnimator container = new(0, new List<IUMI3DPoseOverriderData>());

        //    // WHEN
        //    UMI3DPoseAnimator dto = (UMI3DPoseAnimator)container.ToEntityDto();

        //    // THEN
        //    Assert.AreEqual(container.Id(), dto.id);
        //    Assert.AreEqual(container.PoseOverriders.GetValue().Count, dto.poseOverriderDtos.Length);
        //    Assert.AreEqual(container.NodeId, dto.relatedNodeId);
        //}

        //#endregion ToEntityDto
    }
}