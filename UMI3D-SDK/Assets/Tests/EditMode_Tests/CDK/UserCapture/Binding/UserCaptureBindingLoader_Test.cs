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

using EditMode_Tests.Core.Binding.CDK;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.cdk;
using umi3d.cdk.binding;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.binding;
using umi3d.common.binding;
using umi3d.common.dto.binding;
using umi3d.common.userCapture;
using umi3d.common.userCapture.binding;

namespace EditMode_Tests.UserCapture.Binding.CDK
{
    public class UserCaptureBindingLoader_Test : BindingLoader_Test
    {
        protected Mock<ISkeletonManager> skeletonServiceMock;

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
            environmentManagerMock = new();
            loadingManagerMock = new();
            skeletonServiceMock = new();
            bindingLoader = new UserCaptureBindingLoader(bindingManagementServiceMock.Object,
                                                         environmentManagerMock.Object,
                                                         loadingManagerMock.Object,
                                                         skeletonServiceMock.Object);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (PersonalSkeletonManager.Exists)
                PersonalSkeletonManager.Destroy();
        }

        #endregion Test SetUp

        #region ReadUMI3DExtension

        [Test]
        public override async void ReadUMI3DExtension_MultiBinding()
        {
            // GIVEN
            uint targetBoneType = BoneType.Chest;
            var dto = new BindingDto()
            {
                id = 1005uL,
                boundNodeId = 1008uL,
                data = new MultiBindingDataDto() { Bindings = new AbstractSimpleBindingDataDto[] { new NodeBindingDataDto() { parentNodeId = 1008uL }, 
                                                                                                   new BoneBindingDataDto() { userId = 2005uL, boneType = targetBoneType } } }
            };

            var extensionData = new ReadUMI3DExtensionData(dto);

            var entityFake = new UMI3DEntityInstance(() => { });
            var nodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));

            var personalSkeletonMock = new Mock<PersonalSkeleton>();
            personalSkeletonMock.Setup(x => x.Bones).Returns(new Dictionary<uint, ISkeleton.s_Transform>() { { targetBoneType, new() } });

            nodeMock.Setup(x => x.transform).Returns(default(UnityEngine.Transform));

            loadingManagerMock.Setup(x => x.WaitUntilEntityLoaded(dto.id, null)).Returns(Task.FromResult(entityFake));
            environmentManagerMock.Setup(x => x.RegisterEntity(dto.id, dto, null, It.IsAny<System.Action>())).Returns(entityFake);
            environmentManagerMock.Setup(x => x.GetNodeInstance(dto.boundNodeId)).Returns(nodeMock.Object);

            bindingManagementServiceMock.Setup(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));

            skeletonServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);

            // WHEN
            await bindingLoader.ReadUMI3DExtension(extensionData);

            // THEN
            environmentManagerMock.Verify(x => x.RegisterEntity(dto.id, dto, null, It.IsAny<System.Action>()));
            bindingManagementServiceMock.Verify(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));
        }

        [Test]
        public virtual async void ReadUMI3DExtension_BoneBinding()
        {
            // GIVEN
            uint targetBoneType = BoneType.Chest;
            var dto = new BindingDto()
            {
                id = 1005uL,
                boundNodeId = 1008uL,
                data = new BoneBindingDataDto() { userId = 2005uL, boneType = targetBoneType }
            };

            var extensionData = new ReadUMI3DExtensionData(dto);

            var entityFake = new UMI3DEntityInstance(() => { });
            var nodeMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));

            var personalSkeletonMock = new Mock<PersonalSkeleton>();
            personalSkeletonMock.Setup(x => x.Bones).Returns(new Dictionary<uint, ISkeleton.s_Transform>() { { targetBoneType, new()} });

            nodeMock.Setup(x => x.transform).Returns(default(UnityEngine.Transform));

            loadingManagerMock.Setup(x => x.WaitUntilEntityLoaded(dto.id, null)).Returns(Task.FromResult(entityFake));
            environmentManagerMock.Setup(x => x.RegisterEntity(dto.id, dto, null, It.IsAny<System.Action>())).Returns(entityFake);
            environmentManagerMock.Setup(x => x.GetNodeInstance(dto.boundNodeId)).Returns(nodeMock.Object);

            skeletonServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);

            bindingManagementServiceMock.Setup(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));

            // WHEN
            await bindingLoader.ReadUMI3DExtension(extensionData);

            // THEN
            environmentManagerMock.Verify(x => x.RegisterEntity(dto.id, dto, null, It.IsAny<System.Action>()));
            bindingManagementServiceMock.Verify(x => x.AddBinding(dto.boundNodeId, It.IsAny<AbstractBinding>()));
        }

        // require play mode test because of rig checking in scene graph


        #endregion ReadUMI3DExtension
    }
}