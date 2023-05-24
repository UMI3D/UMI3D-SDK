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
using umi3d.edk;
using umi3d.edk.collaboration;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PlayMode_Tests.Collaboration.Emotes.EDK
{
    public class EmoteDispatcher_Test
    {
        private EmoteDispatcher emoteDispatcher;

        private void CleanSingletons()
        {
            if (EmoteDispatcher.Exists)
                EmoteDispatcher.Destroy();

            if (UMI3DEnvironment.Exists)
                UMI3DEnvironment.Destroy();
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            CleanSingletons();
        }

        [SetUp]
        public void SetUp()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
            GameObject go = new GameObject("UMI3D Environment");
            UnityEngine.Object.Instantiate(go);

            UMI3DEnvironment umi3dEnvironment = go.AddComponent<UMI3DEnvironment>();

            emoteDispatcher = new EmoteDispatcher(umi3dEnvironment);
        }

        [TearDown]
        public void TearDown()
        {
            CleanSingletons();
        }

        [UnityTearDown]
        public void UnityTearDown()
        {
            SceneManager.UnloadSceneAsync(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

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
    }
}