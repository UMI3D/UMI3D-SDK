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
using umi3d.common;
using umi3d.common.collaboration.userCapture.pose;
using umi3d.common.collaboration.userCapture.pose.dto;

namespace EditMode_Tests.Collaboration.UserCaptureExtension.Pose.Common
{
    [TestFixture, TestOf(nameof(CollaborationPoseConditionSerializerModule))]
    public class CollaborationPoseConditionSerializerModule_Test
    {
        private CollaborationPoseConditionSerializerModule serializerModule;

        protected List<UMI3DSerializerModule> serializationModules = new();

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            serializerModule = new CollaborationPoseConditionSerializerModule();
            serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();

            UMI3DSerializer.AddModule(serializationModules);
        }

        [OneTimeTearDown]
        public virtual void OnTimeTeardown()
        {
            UMI3DSerializer.RemoveModule(serializationModules);
            serializationModules.Clear();
        }

        #endregion Test SetUp

        [Test, TestOf(nameof(CollaborationPoseConditionSerializerModule.Write))]
        public void Write_ProjectedCondition()
        {
            // given
            ProjectedPoseConditionDto projectedPoseCondition = new()
            {
                interactableId = 10005uL
            };

            // when
            serializerModule.Write(projectedPoseCondition, out Bytable data);

            // then
            Assert.IsNotNull(data);
            Assert.Greater(data.size, 0);
        }

        [Test, TestOf(nameof(CollaborationPoseConditionSerializerModule.Read))]
        public void Read_ProjectedCondition()
        {
            // given
            ProjectedPoseConditionDto projectedPoseCondition = new()
            {
                interactableId = 10005uL
            };

            serializerModule.Write(projectedPoseCondition, out Bytable data);

            // when
            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());
            serializerModule.Read(byteContainer, out bool readable, out ProjectedPoseConditionDto result);

            // then
            Assert.IsTrue(readable, "Not readable");
            Assert.AreEqual(projectedPoseCondition.interactableId, result.interactableId);
        }
    }
}