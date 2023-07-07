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

using Moq;
using NUnit.Framework;
using umi3d.edk;
using umi3d.edk.collaboration.emotes;
using UnityEngine;

namespace PlayMode_Tests.Collaboration.Emotes.EDK
{
    public class EmoteDispatcher_Test
    {
        private Mock<IUMI3DEnvironmentManager> umi3dEnvironmentMock;
        private EmoteDispatcher emoteDispatcher;

        private void CleanSingletons()
        {
            if (EmoteDispatcher.Exists)
                EmoteDispatcher.Destroy();
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            CleanSingletons();
        }

        [SetUp]
        public void SetUp()
        {
            umi3dEnvironmentMock = new Mock<IUMI3DEnvironmentManager>();

            emoteDispatcher = new EmoteDispatcher(umi3dEnvironmentMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CleanSingletons();
        }

        #region DispatchEmoteTrigger

        [TestCase(0uL, true)]
        [TestCase(0uL, false)]
        [Test]
        public void DispatchEmoteTrigger_NoEmoteConfig(ulong emoteId, bool trigger)
        {
            // GIVEN
            UMI3DUser user = new();

            // WHEN
            emoteDispatcher.DispatchEmoteTrigger(user, emoteId, trigger);

            // THEN
            // Nothing
            // UNDONE: Test call of mocked server to know if transaction is not sent. Require refacto of Transaction.Dispatch()
            Assert.Pass();
        }

        [TestCase(0uL, true)]
        [TestCase(0uL, false)]
        [Test]
        public void DispatchEmoteTrigger_NoEmotes(ulong emoteId, bool trigger)
        {
            // GIVEN
            UMI3DUser user = new();
            emoteDispatcher.EmotesConfigs.Add(0uL, ScriptableObject.CreateInstance<UMI3DEmotesConfig>());

            // WHEN
            emoteDispatcher.DispatchEmoteTrigger(user, emoteId, trigger);

            // THEN
            // Nothing
            // UNDONE: Test call of mocked server to know if transaction is not sent. Require refacto of Transaction.Dispatch()
            Assert.Pass();
        }

        #endregion DispatchEmoteTrigger
    }
}