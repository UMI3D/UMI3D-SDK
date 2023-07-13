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
using System.Collections.Generic;
using umi3d.common.userCapture.tracking;
using umi3d.edk;
using umi3d.edk.userCapture.tracking;

namespace EditMode_Tests.UserCapture.Tracking.EDK
{
    #region ToOperationDto

    [TestFixture, TestOf(typeof(SetTrackingBoneTargetFPS))]
    public class SetTrackingBoneTargetFPS_Test
    {
        [Test]
        public void SetTrackingBoneTargetFPSToOperationDto([Random(0f, 100f, 1)] float fps, [Random(1, 58, 1)] float bonetype)
        {
            // GIVEN
            HashSet<UMI3DUser> users = new() { new UMI3DUser(), new UMI3DUser(), new UMI3DUser() };

            SetTrackingBoneTargetFPS op = new SetTrackingBoneTargetFPS(fps, (uint)bonetype)
            {
                users = users
            };

            // WHEN
            SetTrackingBoneTargetFPSDto dto = (SetTrackingBoneTargetFPSDto)op.ToOperationDto(new UMI3DUser());

            // THEN
            Assert.AreEqual(op.targetFPS, dto.targetFPS);
            Assert.AreEqual(op.boneType, dto.boneType);
        }
    }

    #endregion
}