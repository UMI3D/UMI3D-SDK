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
using System;
using umi3d.edk;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


namespace PlayMode_Tests.Core.Bindings.EDK
{
    public class BindingManager_Test
    {
        protected BindingManager bindingManager;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // Destroy used singletons preventively here
            ClearSingletons();
        }

        [SetUp]
        public void SetUp()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);

            bindingManager = new BindingManager(UMI3DServer.Instance);
        }

        [TearDown]
        public void TearDown()
        {
            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (UMI3DServer.Exists)
                UMI3DServer.Destroy();

            if (BindingManager.Exists)
                BindingManager.Destroy();
        }

        #endregion Test SetUp

        // TODO: Replace the region name by the tested method name
        #region AreBindingsEnabled

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void AreBindingsEnabled(bool bindingsEnabled)
        {
            // GIVEN
            bindingManager.areBindingsEnabled.SetValue(bindingsEnabled);

            // WHEN
            var enabledResult = bindingManager.AreBindingsEnabled();

            // THEN
            Assert.AreEqual(bindingsEnabled, enabledResult);
        }


        #endregion AreBindingsEnabled

        #region SetBindingsActivation

        [Test]
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void SetBindingsActivation(bool bindingsEnabled, bool shouldEnable)
        {
            // GIVEN
            bindingManager.areBindingsEnabled.SetValue(bindingsEnabled);

            // WHEN
            bindingManager.SetBindingsActivation(shouldEnable);

            // THEN
            Assert.AreEqual(shouldEnable, bindingManager.areBindingsEnabled.GetValue());
        }

        #endregion SetBindingsActivation
    }
}