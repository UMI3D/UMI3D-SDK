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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TestUtils;
using umi3d.cdk;
using umi3d.cdk.binding;
using umi3d.common;
using umi3d.common.dto.binding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PlayMode_Tests.Core.Binding.CDK
{
    public class MultiBinding_Test
    {
        protected GameObject go;
        protected GameObject parentGo;
        protected GameObject parentSecondGo;
        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public void SetUp()
        {
            parentGo = new GameObject("Parent Node");
            Object.Instantiate(parentGo);
            parentGo.transform.position = Vector3.zero;
            parentGo.transform.rotation = Quaternion.identity;
            parentGo.transform.localScale = Vector3.one;

            parentSecondGo = new GameObject("Parent 2 Node");
            Object.Instantiate(parentSecondGo);
            parentSecondGo.transform.position = Vector3.zero;
            parentSecondGo.transform.rotation = Quaternion.identity;
            parentSecondGo.transform.localScale = Vector3.one;

            go = new GameObject("Bound Node");
            Object.Instantiate(go);
            go.transform.position = Vector3.one;
            go.transform.rotation = Quaternion.Euler(90, 90, 90);
            go.transform.localScale = Vector3.one * 0.5f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(parentGo);
            Object.Destroy(go);
        }

        #endregion Test SetUp

        #region Constuctor

        [Test]
        public void Constuctor_OneBinding()
        {
            // GIVEN
            var dto = new MultiBindingDataDto()
            {
                Bindings = new AbstractSimpleBindingDataDto[]
                {
                    new NodeBindingDataDto()
                    {
                        parentNodeId = 1008uL,
                        syncPosition = true,
                        offSetPosition = Vector3.zero.Dto(),
                        partialFit = false,
                        priority = 10
                    }
                }
            };

            var parentNodeMock = new Mock<UMI3DNodeInstance>(MockBehavior.Default, 0UL, new System.Action(() => { }), 0uL);

            Stack<NodeBinding> bindings = new();
            
            foreach (var bindingDataDto in dto.Bindings)
            {
                bindings.Push(new NodeBinding(bindingDataDto as NodeBindingDataDto, go.transform, parentNodeMock.Object));
            }

            NodeBinding[] simpleBindings = bindings.ToArray();

            // WHEN
            MultiBinding binding = new(dto, simpleBindings, go.transform);

            // THEN
            Assert.AreEqual(1, binding.Bindings.Count);
        }

        [Test]
        public void Constuctor_OrderedBindings()
        {
            // GIVEN
            var dto = new MultiBindingDataDto()
            {
                Bindings = new AbstractSimpleBindingDataDto[]
                {
                    new NodeBindingDataDto()
                    {
                        parentNodeId = 1008uL,
                        syncPosition = true,
                        offSetPosition = Vector3.zero.Dto(),
                        partialFit = false,
                        priority = 10
                    },
                    new NodeBindingDataDto()
                    {
                        parentNodeId = 1009uL,
                        syncPosition = true,
                        offSetPosition = Vector3.zero.Dto(),
                        partialFit = false,
                        priority = 5
                    },
                    new NodeBindingDataDto()
                    {
                        parentNodeId = 1010uL,
                        syncPosition = true,
                        offSetPosition = Vector3.zero.Dto(),
                        partialFit = false,
                        priority = 2
                    }
                }
            };

            var parentNodeMock = new Mock<UMI3DNodeInstance>(MockBehavior.Default, 0UL, new System.Action(() => { }), 0uL);

            Stack<NodeBinding> bindings = new();

            foreach (var bindingDataDto in dto.Bindings)
            {
                bindings.Push(new NodeBinding(bindingDataDto as NodeBindingDataDto, go.transform, parentNodeMock.Object));
            }

            NodeBinding[] simpleBindings = bindings.ToArray();

            // WHEN
            MultiBinding binding = new(dto, simpleBindings, go.transform, isOrdered: true);

            // THEN
            Assert.AreEqual(dto.Bindings.Length, binding.Bindings.Count);

            for (int i= 0; i < dto.Bindings.Length; i++)
            {
                Assert.AreEqual(simpleBindings[i].Priority,       binding.Bindings[i].Priority);
                Assert.AreEqual(simpleBindings[i].IsPartiallyFit, binding.Bindings[i].IsPartiallyFit);
                Assert.AreEqual(simpleBindings[i].ParentNodeId,   (binding.Bindings[i] as NodeBinding).ParentNodeId);
            }
        }

        [Test]
        public void Constuctor_UnorderedBindings()
        {
            // GIVEN
            var dto = new MultiBindingDataDto()
            {
                Bindings = new AbstractSimpleBindingDataDto[]
                {
                    new NodeBindingDataDto()
                    {
                        parentNodeId = 1008uL,
                        syncPosition = true,
                        offSetPosition = Vector3.zero.Dto(),
                        partialFit = false,
                        priority = 8
                    },
                    new NodeBindingDataDto()
                    {
                        parentNodeId = 1009uL,
                        syncPosition = true,
                        offSetPosition = Vector3.zero.Dto(),
                        partialFit = false,
                        priority = 10
                    },
                    new NodeBindingDataDto()
                    {
                        parentNodeId = 1010uL,
                        syncPosition = true,
                        offSetPosition = Vector3.zero.Dto(),
                        partialFit = false,
                        priority = 2
                    }
                }
            };

            var parentNodeMock = new Mock<UMI3DNodeInstance>(MockBehavior.Default, 0UL, new System.Action(() => { }), 0uL);

            Stack<NodeBinding> bindings = new();

            foreach (var bindingDataDto in dto.Bindings)
            {
                bindings.Push(new NodeBinding(bindingDataDto as NodeBindingDataDto, go.transform, parentNodeMock.Object));
            }

            NodeBinding[] simpleBindings = bindings.ToArray();

            // WHEN
            MultiBinding binding = new(dto, simpleBindings, go.transform, isOrdered: false);

            // THEN
            Assert.AreEqual(dto.Bindings.Length, binding.Bindings.Count);

            var orderedBindings = simpleBindings.OrderByDescending(x => x.Priority).ToArray();

            for (int i = 0; i < dto.Bindings.Length; i++)
            {
                Assert.AreEqual(orderedBindings[i].Priority, binding.Bindings[i].Priority);
                Assert.AreEqual(orderedBindings[i].IsPartiallyFit, binding.Bindings[i].IsPartiallyFit);
                Assert.AreEqual(orderedBindings[i].ParentNodeId, (binding.Bindings[i] as NodeBinding).ParentNodeId);
            }
        }

        #endregion Constuctor

        #region Apply

        private static (ulong id, int priority, bool partialFit)[][] simpleBindingParameters = new (ulong id, int priority, bool partialFit)[][] 
            {
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 10, true), (1009uL, 5, true), (1010uL, 1, true)  },
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 10, true), (1009uL, 5, true), (1010uL, 1, false) },
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 1, true),  (1009uL, 1, true), (1010uL, 1, true)  },
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 1, true),  (1009uL, 1, true), (1010uL, 1, false) },
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 1, true), (1009uL, 5, true), (1010uL, 10, true)  },
                new (ulong id, int priority, bool partialFit)[] { (1008uL, 1, true), (1009uL, 5, true), (1010uL, 10, false) },
            };


        [Test]
        public void Apply([ValueSource(nameof(simpleBindingParameters))] (ulong id, int priority, bool partialFit)[] testValue)
        {
            // GIVEN
            Queue<AbstractSimpleBindingDataDto> bindingDataDtos = new();
            int i = 0;
            foreach (var v in testValue)
            {
                var bindingDataDto = new NodeBindingDataDto()
                {
                    parentNodeId = v.id,
                    syncPosition = true,
                    offSetPosition = Vector3.one.Dto(),
                    syncRotation = true,
                    offSetRotation = Quaternion.Euler(0, 90, 0).Dto(),
                    syncScale = true,
                    offSetScale = (Vector3.one * 2).Dto(),
                    partialFit = v.partialFit,
                    priority = v.priority
                };
                bindingDataDtos.Enqueue(bindingDataDto);
                i++;
            }

            var dto = new MultiBindingDataDto() { Bindings = bindingDataDtos.ToArray() };

            var parentNodeMock = new Mock<UMI3DNodeInstance>(MockBehavior.Default, UMI3DGlobalID.EnvironmentId, new System.Action(() => { }), 0uL);

            Queue<NodeBinding> bindings = new();
            List<NodeBindingDataDto> callBackCache = new();

            foreach (var bindingDataDto in dto.Bindings.OrderByDescending(x=>x.priority))
            {
                var bindingMock = new Mock<NodeBinding>( bindingDataDto as NodeBindingDataDto, go.transform, parentNodeMock.Object);
                bool successLocal = true; // is lazy evaluated and put as the result of the mocked method
                bindingMock
                    .Setup(x => x.Apply(out successLocal))
                    .Callback(() => callBackCache.Add(bindingDataDto as NodeBindingDataDto));
                bindingMock.Setup(x => x.IsPartiallyFit).Returns(bindingDataDto.partialFit);

                bindings.Enqueue(bindingMock.Object);
            }

            MultiBinding binding = new(dto, bindings.ToArray(), go.transform, isOrdered: false);

            // WHEN
            binding.Apply(out bool success);

            // THEN
            Assert.IsTrue(success);

            var orderedtestValues = testValue.OrderByDescending(x => x.priority).ToArray();
            int numberOfValueToApply = orderedtestValues.TakeWhile(x => x.partialFit).Count();
            if (numberOfValueToApply == 0) //when first binding cannot be applied totally, it is the only one applied
                numberOfValueToApply = 1;

            Assert.AreEqual(numberOfValueToApply, callBackCache.Count , "Not all bindings were applied.");
            
            for (int j = 0; j < callBackCache.Count; j++)
            {
                Assert.AreEqual(orderedtestValues[0].id, callBackCache[0].parentNodeId, $"Order of binding application is not right. Problem at {j}");
            }
        }

        #endregion

        #region Reset

        [Test, TestOf(nameof(MultiBinding.Reset))]
        public void Reset([ValueSource(nameof(simpleBindingParameters))] (ulong id, int priority, bool partialFit)[] testValue)
        {
            // GIVEN
            Queue<AbstractSimpleBindingDataDto> bindingDataDtos = new();
            int i = 0;
            foreach (var v in testValue)
            {
                var bindingDataDto = new NodeBindingDataDto()
                {
                    parentNodeId = v.id,
                    syncPosition = true,
                    offSetPosition = Vector3.one.Dto(),
                    syncRotation = true,
                    offSetRotation = Quaternion.Euler(0, 90, 0).Dto(),
                    syncScale = true,
                    offSetScale = (Vector3.one * 2).Dto(),
                    partialFit = v.partialFit,
                    priority = v.priority
                };
                bindingDataDtos.Enqueue(bindingDataDto);
                i++;
            }

            var dto = new MultiBindingDataDto() { Bindings = bindingDataDtos.ToArray() };

            var parentNodeMock = new Mock<UMI3DNodeInstance>(MockBehavior.Default, UMI3DGlobalID.EnvironmentId, new System.Action(() => { }), 0uL);

            Queue<NodeBinding> bindings = new();
            List<NodeBindingDataDto> callBackCache = new();

            foreach (var bindingDataDto in dto.Bindings.OrderByDescending(x => x.priority))
            {
                var bindingMock = new Mock<NodeBinding>(bindingDataDto as NodeBindingDataDto, go.transform, parentNodeMock.Object);
                bool successLocal = true; // is lazy evaluated and put as the result of the mocked method
                bindingMock
                    .Setup(x => x.Apply(out successLocal))
                    .Callback(() => callBackCache.Add(bindingDataDto as NodeBindingDataDto));
                bindingMock.Setup(x => x.IsPartiallyFit).Returns(bindingDataDto.partialFit);

                bindings.Enqueue(bindingMock.Object);
            }

            MultiBinding binding = new(dto, bindings.ToArray(), go.transform, isOrdered: false);

            Vector3 previousPosition = go.transform.position;
            Quaternion previousRotation = go.transform.rotation;
            Vector3 previousScale = go.transform.localScale;

            binding.Apply(out bool succes);

            // WHEN
            binding.Reset();

            // THEN
            Assert.IsTrue(succes);
            AssertUnityStruct.AreEqual(previousPosition, go.transform.position, message: "Positions are not equal");
            AssertUnityStruct.AreEqual(previousRotation, go.transform.rotation, message: "Rotations are not the same.");
            AssertUnityStruct.AreEqual(previousScale, go.transform.localScale, message: "Scales are not the same.");
        }

        #endregion Reset
    }
}