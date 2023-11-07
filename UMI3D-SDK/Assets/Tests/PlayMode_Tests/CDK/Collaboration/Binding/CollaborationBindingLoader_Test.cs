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

using Moq;
using NUnit.Framework;
using PlayMode_Tests.UserCapture.Binding.CDK;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.cdk;
using umi3d.cdk.binding;
using umi3d.cdk.collaboration;
using umi3d.cdk.collaboration.userCapture;
using umi3d.cdk.collaboration.userCapture.binding;
using umi3d.cdk.userCapture;
using umi3d.common.dto.binding;
using umi3d.common.userCapture;
using umi3d.common.userCapture.binding;
using UnityEngine;

namespace PlayMode_Tests.Collaboration.UserCapture.Binding.CDK
{
    [TestFixture, TestOf(nameof(CollaborationBindingLoader))]
    public class CollaborationBindingLoader_Test : UserCaptureBindingLoader_Test
    {
        protected Mock<ICollaborationSkeletonsManager> collaborativeSkeletonManager;


        #region Test SetUp

        [OneTimeSetUp]
        public override void OneTimeSetup()
        {
            base.OneTimeSetup();
            ClearSingletons();
        }

        [SetUp]
        public override void SetUp()
        {
            bindingManagementServiceMock = new();
            skeletonServiceMock = new();
            loadingManagerMock = new();
            environmentManagerMock = new();
            collaborativeSkeletonManager = new();
            bindingLoader = new CollaborationBindingLoader(bindingManagementServiceMock.Object,
                                                           loadingManagerMock.Object,
                                                           environmentManagerMock.Object,
                                                           collaborativeSkeletonManager.Object);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (UMI3DCollaborationEnvironmentLoader.Exists)
                UMI3DCollaborationEnvironmentLoader.Destroy();

            if (CollaborationSkeletonsManager.Exists)
                CollaborationSkeletonsManager.Destroy();
        }

        #endregion Test SetUp

        #region ReadUMI3DExtension

        [Test]
        public override async void ReadUMI3DExtension_MultiBinding()
        {
            // GIVEN
            uint targetBoneType = BoneType.Chest;
            ulong userId = 2005uL;
            var dto = new BindingDto()
            {
                id = 1005uL,
                boundNodeId = 1008uL,
                data = new MultiBindingDataDto()
                {
                    Bindings = new AbstractSimpleBindingDataDto[] { new NodeBindingDataDto() { parentNodeId = 1008uL },
                                                                                                   new BoneBindingDataDto() { userId = userId, boneType = targetBoneType } }
                }
            };

            var extensionData = new ReadUMI3DExtensionData(0, dto);

            var entityFake = new UMI3DEntityInstance(0, () => { });
            var nodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));
            var nodeGameObject = new GameObject("Node to bind");
            nodeMock.Setup(x => x.transform).Returns(nodeGameObject.transform);

            var skeletonMock = new Mock<ISkeleton>();
            skeletonMock.Setup(x => x.Bones).Returns(new Dictionary<uint, ISkeleton.Transformation>() { { targetBoneType, new() } });
            
            loadingManagerMock.Setup(x => x.WaitUntilEntityLoaded(0, dto.id, null)).Returns(Task.FromResult(entityFake));
            environmentManagerMock.Setup(x => x.RegisterEntity(0, dto.id, dto, null, It.IsAny<System.Action>())).Returns(entityFake);
            environmentManagerMock.Setup(x => x.GetNodeInstance(0, dto.boundNodeId)).Returns(nodeMock.Object);

            bindingManagementServiceMock.Setup(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));

            collaborativeSkeletonManager.Setup(x => x.Skeletons).Returns(new Dictionary<ulong, ISkeleton>() { { userId, skeletonMock.Object } });

            // WHEN
            await bindingLoader.ReadUMI3DExtension(extensionData);

            // THEN
            environmentManagerMock.Verify(x => x.RegisterEntity(0, dto.id, dto, null, It.IsAny<System.Action>()));
            bindingManagementServiceMock.Verify(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));

            // teardown
            Object.Destroy(nodeGameObject);
        }

        [Test]
        public override async void ReadUMI3DExtension_NodeBinding()
        {
            // GIVEN
            var dto = new BindingDto()
            {
                id = 1005uL,
                boundNodeId = 1008uL,
                data = new NodeBindingDataDto() { parentNodeId = 1008uL }
            };

            var extensionData = new ReadUMI3DExtensionData(0, dto);

            var entityFake = new UMI3DEntityInstance(0, () => { });
            var nodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));
            var nodeGameObject = new GameObject("Node to bind");
            nodeMock.Setup(x => x.transform).Returns(nodeGameObject.transform);

            loadingManagerMock.Setup(x => x.WaitUntilEntityLoaded(0, dto.id, null)).Returns(Task.FromResult(entityFake));
            environmentManagerMock.Setup(x => x.RegisterEntity(0, dto.id, dto, null, It.IsAny<System.Action>())).Returns(entityFake);
            environmentManagerMock.Setup(x => x.GetNodeInstance(0, dto.boundNodeId)).Returns(nodeMock.Object);

            bindingManagementServiceMock.Setup(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));

            // WHEN
            await bindingLoader.ReadUMI3DExtension(extensionData);

            // THEN
            environmentManagerMock.Verify(x => x.RegisterEntity(0, dto.id, dto, null, It.IsAny<System.Action>()));
            bindingManagementServiceMock.Verify(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));

            // teardown
            Object.Destroy(nodeGameObject);
        }

        [Test]
        public override async void ReadUMI3DExtension_BoneBinding()
        {
            // GIVEN
            uint targetBoneType = BoneType.Chest;
            ulong userId = 2005uL;
            var dto = new BindingDto()
            {
                id = 1005uL,
                boundNodeId = 1008uL,
                data = new BoneBindingDataDto() { userId = userId, boneType = targetBoneType }
            };

            var extensionData = new ReadUMI3DExtensionData(0, dto);

            var entityFake = new UMI3DEntityInstance(0, () => { });
            var nodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));
            var nodeGameObject = new GameObject("Node to bind");
            nodeMock.Setup(x => x.transform).Returns(nodeGameObject.transform);

            var skeletonMock = new Mock<ISkeleton>();
            skeletonMock.Setup(x => x.Bones).Returns(new Dictionary<uint, ISkeleton.Transformation>() { { targetBoneType, new() } });

            loadingManagerMock.Setup(x => x.WaitUntilEntityLoaded(0, dto.id, null)).Returns(Task.FromResult(entityFake));
            environmentManagerMock.Setup(x => x.RegisterEntity(0, dto.id, dto, null, It.IsAny<System.Action>())).Returns(entityFake);
            environmentManagerMock.Setup(x => x.GetNodeInstance(0, dto.boundNodeId)).Returns(nodeMock.Object); ;

            bindingManagementServiceMock.Setup(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));

            collaborativeSkeletonManager.Setup(x => x.Skeletons).Returns(new Dictionary<ulong, ISkeleton>() { { userId, skeletonMock.Object } });

            // WHEN
            await bindingLoader.ReadUMI3DExtension(extensionData);

            // THEN
            environmentManagerMock.Verify(x => x.RegisterEntity(0, dto.id, dto, null, It.IsAny<System.Action>()));
            bindingManagementServiceMock.Verify(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));

            // teardown
            Object.Destroy(nodeGameObject);
        }

        #endregion ReadUMI3DExtension
    }
}