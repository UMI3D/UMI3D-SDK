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
using umi3d.edk.binding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


namespace PlayMode_Tests.Core.Binding.EDK
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

        #region AddBinding

        [Test]
        public void AddBinding()
        {
            // GIVEN
            int initialCount = bindingManager.bindings.GetValue().Count;

            var binding = new NodeBinding(10005uL, 10008uL);

            // WHEN
            var resultOperation = bindingManager.AddBinding(binding);

            // THEN
            Assert.GreaterOrEqual(resultOperation.Count, 1);
            Assert.AreEqual(initialCount + 1, bindingManager.bindings.GetValue().Count);
        }

        [Test]
        public void AddBinding_Upgrade()
        {
            // GIVEN
            ulong boundNodeId = 10005uL;
            var firstBinding = new NodeBinding(boundNodeId, 10008uL);
            var secondBinding = new NodeBinding(boundNodeId, 10009uL);
            bindingManager.AddBinding(firstBinding);

            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN
            var resultOperation = bindingManager.AddBinding(secondBinding);

            // THEN
            Assert.GreaterOrEqual(resultOperation.Count, 1);
            Assert.AreEqual(initialCount, bindingManager.bindings.GetValue().Count);
            Assert.IsAssignableFrom(typeof(MultiBinding), bindingManager.bindings[boundNodeId]);
        }

        [Test]
        public void AddBinding_Null()
        {
            // GIVEN
            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN
            var resultOperation = bindingManager.AddBinding(null);

            // THEN
            Assert.IsNull(resultOperation);
            Assert.AreEqual(initialCount, bindingManager.bindings.GetValue().Count);
        }

        #endregion

        #region RemoveBinding

        [Test]
        public void RemoveBinding()
        {
            // GIVEN
            var binding = new NodeBinding(10005uL, 10008uL);
            bindingManager.AddBinding(binding);
            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN
            var resultOperation = bindingManager.RemoveBinding(binding);

            // THEN
            Assert.GreaterOrEqual(resultOperation.Count, 1);
            Assert.AreEqual(initialCount - 1, bindingManager.bindings.GetValue().Count);
        }

        [Test]
        public void RemoveBinding_Downgrade()
        {
            // GIVEN
            ulong boundNodeId = 10005uL;
            var firstBinding = new NodeBinding(boundNodeId, 10008uL);
            var secondBinding = new NodeBinding(boundNodeId, 10009uL);
            var binding = new MultiBinding(boundNodeId)
            {
                bindings = new() { firstBinding, secondBinding }
            };
            bindingManager.AddBinding(binding);
            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN
            var resultOperation = bindingManager.RemoveBinding(secondBinding);

            // THEN
            Assert.GreaterOrEqual(resultOperation.Count, 1);
            Assert.AreEqual(initialCount, bindingManager.bindings.GetValue().Count);
            Assert.IsAssignableFrom(typeof(NodeBinding), bindingManager.bindings[boundNodeId]);
        }

        [Test]
        public void RemoveBinding_Null()
        {
            // GIVEN
            var binding = new NodeBinding(10005uL, 10008uL);
            bindingManager.AddBinding(binding);
            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN
            var resultOperation = bindingManager.RemoveBinding(null);

            // THEN
            Assert.IsNull(resultOperation);
            Assert.AreEqual(initialCount, bindingManager.bindings.GetValue().Count);
        }


        #endregion

        #region RemoveAllBindings

        [Test]
        public void RemoveAllBindings()
        {
            // GIVEN
            ulong boundNodeId = 10005uL;
            var binding = new NodeBinding(boundNodeId, 10008uL);
            bindingManager.AddBinding(binding);
            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN
            var resultOperation = bindingManager.RemoveAllBindings(boundNodeId);

            // THEN
            Assert.GreaterOrEqual(resultOperation.Count, 1);
            Assert.AreEqual(initialCount - 1, bindingManager.bindings.GetValue().Count);
        }

        [Test]
        public void RemoveAllBindings_Multibinding()
        {
            // GIVEN
            ulong boundNodeId = 10005uL;
            var firstBinding = new NodeBinding(boundNodeId, 10008uL);
            var secondBinding = new NodeBinding(boundNodeId, 10009uL);
            var binding = new MultiBinding(boundNodeId)
            {
                bindings = new() { firstBinding, secondBinding }
            };
            bindingManager.AddBinding(binding);
            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN
            var resultOperation = bindingManager.RemoveAllBindings(boundNodeId);

            // THEN
            Assert.GreaterOrEqual(resultOperation.Count, 1);
            Assert.AreEqual(initialCount - 1, bindingManager.bindings.GetValue().Count);
        }

        [Test]
        public void RemoveAllBindings_InvalidKey()
        {
            // GIVEN
            ulong boundNodeId = 10005uL;
            var binding = new NodeBinding(boundNodeId, 10008uL);
            bindingManager.AddBinding(binding);
            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN
            var resultOperation = bindingManager.RemoveAllBindings(boundNodeId+5);

            // THEN
            Assert.IsNull(resultOperation);
            Assert.AreEqual(initialCount, bindingManager.bindings.GetValue().Count);
        }

        #endregion
    }
}