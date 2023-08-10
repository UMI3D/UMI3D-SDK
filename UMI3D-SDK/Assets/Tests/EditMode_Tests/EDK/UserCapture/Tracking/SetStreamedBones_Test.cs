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
using System.Linq;
using System.Reflection;
using umi3d.common.userCapture;
using umi3d.common.userCapture.tracking;
using umi3d.edk;
using umi3d.edk.userCapture.tracking;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Tracking.EDK
{
    #region ToOperationDto

    [TestFixture, TestOf(typeof(SetStreamedBones))]
    public class SetStreamedBones_Test
    {
        List<uint> bonetypesValues;

        [SetUp]
        public void SetUp()
        {
            bonetypesValues = new List<uint>();
            IEnumerable<FieldInfo> val = typeof(BoneType).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                           .Where(fi => fi.IsLiteral && !fi.IsInitOnly);
            bonetypesValues = val.Select(fi => (uint)fi.GetValue(null)).ToList();
        }

        [Test]
        public void SetTrackingTargetFPSToOperationDto([Random(1, 58, 3)] int nbr)
        {
            // GIVEN
            HashSet<UMI3DUser> users = new() { new UMI3DUser(), new UMI3DUser(), new UMI3DUser() };

            List<uint> bonetypes = new List<uint>();

            for (int i = 0; i < nbr; i++)
            {
                bonetypes.Add(GetRandomBonetype());
            }

            SetStreamedBones op = new SetStreamedBones()
            {
                streamedBones = bonetypes,
                users = users
            };

            // WHEN
            SetStreamedBonesDto dto = (SetStreamedBonesDto)op.ToOperationDto(new UMI3DUser());

            // THEN
            Assert.AreEqual(op.streamedBones.Count, dto.streamedBones.Count);
            for (int i = 0; i < nbr; i++)
            {
                Assert.AreEqual(op.streamedBones[i], dto.streamedBones[i]);
            }
        }

        protected uint GetRandomBonetype()
        {
            uint id = bonetypesValues[0];
            bonetypesValues.RemoveAt(0);
            return id;
        }
    }

    #endregion ToOperationDto
}
