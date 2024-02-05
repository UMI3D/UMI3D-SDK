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
using umi3d.edk.binding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.Core.Binding.EDK
{
    public class BindingManager_Test
    {
        protected BindingManager bindingManager;

        private Mock<IUMI3DServer> umi3dServerMock;
        private Mock<IUMI3DEnvironmentManager> umi3dEnvironmentServiceMock;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.TEST_SCENE_EDK_BASE);
            SceneManager.LoadScene(PlayModeTestHelper.TEST_SCENE_EDK_BINDINGS, LoadSceneMode.Additive);
            // Destroy used singletons preventively here
            ClearSingletons();
        }

        [SetUp]
        public void SetUp()
        {
            umi3dServerMock = new();
            umi3dServerMock.Setup(x => x.OnUserActive).Returns(new UMI3DUserEvent());
            umi3dServerMock.Setup(x => x.OnUserLeave).Returns(new UMI3DUserEvent());
            umi3dServerMock.Setup(x => x.OnUserMissing).Returns(new UMI3DUserEvent());

            umi3dEnvironmentServiceMock = new();

            bindingManager = new BindingManager(umi3dServerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            ClearSingletons();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            SceneManager.UnloadSceneAsync(PlayModeTestHelper.TEST_SCENE_EDK_BINDINGS);
        }

        private void ClearSingletons()
        {
            if (BindingManager.Exists)
                BindingManager.Destroy();
        }

        #endregion Test SetUp

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
            var gameObject = new GameObject();
            var node = gameObject.AddComponent<UMI3DNode>();
            ulong boundNodeId = node.Id();
            umi3dEnvironmentServiceMock.Setup(x => x._GetEntityInstance<UMI3DAbstractNode>(boundNodeId)).Returns(node);

            int initialCount = bindingManager.bindings.GetValue().Count;

            var binding = new NodeBinding(boundNodeId, 10008uL);

            // WHEN
            var resultOperation = bindingManager.AddBinding(binding);

            // THEN
            Assert.GreaterOrEqual(resultOperation.Count, 1);
            Assert.AreEqual(initialCount + 1, bindingManager.bindings.GetValue().Count);

            // teardown
            UnityEngine.Object.Destroy(gameObject);
        }

        [Test]
        public void AddBinding_Upgrade()
        {
            // GIVEN
            var gameObject = new GameObject();
            var node = gameObject.AddComponent<UMI3DNode>();
            ulong boundNodeId = node.Id();
            umi3dEnvironmentServiceMock.Setup(x => x._GetEntityInstance<UMI3DAbstractNode>(boundNodeId)).Returns(node);

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

            // teardown
            UnityEngine.Object.Destroy(gameObject);
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

        #endregion AddBinding

        #region RemoveBinding

        [Test]
        public void RemoveBinding()
        {
            // GIVEN
            var gameObject = new GameObject();
            var node = gameObject.AddComponent<UMI3DNode>();
            ulong boundNodeId = node.Id();
            umi3dEnvironmentServiceMock.Setup(x => x._GetEntityInstance<UMI3DAbstractNode>(boundNodeId)).Returns(node);

            var binding = new NodeBinding(boundNodeId, 10008uL);
            bindingManager.AddBinding(binding);
            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN
            var resultOperation = bindingManager.RemoveBinding(binding);

            // THEN
            Assert.GreaterOrEqual(resultOperation.Count, 1);
            Assert.AreEqual(initialCount - 1, bindingManager.bindings.GetValue().Count);

            // teardown
            UnityEngine.Object.Destroy(gameObject);
        }

        [Test]
        public void RemoveBinding_Downgrade()
        {
            // GIVEN
            var gameObject = new GameObject();
            var node = gameObject.AddComponent<UMI3DNode>();
            ulong boundNodeId = node.Id();
            umi3dEnvironmentServiceMock.Setup(x => x._GetEntityInstance<UMI3DAbstractNode>(boundNodeId)).Returns(node);

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

            // teardown
            UnityEngine.Object.Destroy(gameObject);
        }

        [Test]
        public void RemoveBinding_Null()
        {
            // GIVEN
            var gameObject = new GameObject();
            var node = gameObject.AddComponent<UMI3DNode>();
            ulong boundNodeId = node.Id();
            umi3dEnvironmentServiceMock.Setup(x => x._GetEntityInstance<UMI3DAbstractNode>(boundNodeId)).Returns(node);

            var binding = new NodeBinding(boundNodeId, 10008uL);
            bindingManager.AddBinding(binding);
            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN
            var resultOperation = bindingManager.RemoveBinding(null);

            // THEN
            Assert.IsNull(resultOperation);
            Assert.AreEqual(initialCount, bindingManager.bindings.GetValue().Count);

            // teardown
            UnityEngine.Object.Destroy(gameObject);
        }

        #endregion RemoveBinding

        #region RemoveAllBindings

        [Test]
        public void RemoveAllBindings()
        {
            // GIVEN
            var gameObject = new GameObject();
            var node = gameObject.AddComponent<UMI3DNode>();
            ulong boundNodeId = node.Id();
            umi3dEnvironmentServiceMock.Setup(x => x._GetEntityInstance<UMI3DAbstractNode>(boundNodeId)).Returns(node);

            var binding = new NodeBinding(boundNodeId, 10008uL);
            bindingManager.AddBinding(binding);
            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN

            var resultOperation = bindingManager.RemoveAllBindings(boundNodeId);

            // THEN
            Assert.GreaterOrEqual(resultOperation.Count, 1);
            Assert.AreEqual(initialCount - 1, bindingManager.bindings.GetValue().Count);

            // teardown
            UnityEngine.Object.Destroy(gameObject);
        }

        [Test]
        public void RemoveAllBindings_Multibinding()
        {
            // GIVEN
            var gameObject = new GameObject();
            var node = gameObject.AddComponent<UMI3DNode>();
            ulong boundNodeId = node.Id();
            umi3dEnvironmentServiceMock.Setup(x => x._GetEntityInstance<UMI3DAbstractNode>(boundNodeId)).Returns(node);

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

            // teardown
            UnityEngine.Object.Destroy(gameObject);
        }

        [Test]
        public void RemoveAllBindings_InvalidKey()
        {
            // GIVEN
            var gameObject = new GameObject();
            var node = gameObject.AddComponent<UMI3DNode>();
            ulong boundNodeId = node.Id();
            umi3dEnvironmentServiceMock.Setup(x => x._GetEntityInstance<UMI3DAbstractNode>(boundNodeId)).Returns(node);

            var binding = new NodeBinding(boundNodeId, 10008uL);
            bindingManager.AddBinding(binding);
            int initialCount = bindingManager.bindings.GetValue().Count;

            // WHEN
            var resultOperation = bindingManager.RemoveAllBindings(boundNodeId + 5);

            // THEN
            Assert.IsNull(resultOperation);
            Assert.AreEqual(initialCount, bindingManager.bindings.GetValue().Count);

            // teardown
            UnityEngine.Object.Destroy(gameObject);
        }

        #endregion RemoveAllBindings
    }
}