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

using inetum.unityUtils;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.animation;
using umi3d.common;
using umi3d.common.userCapture.animation;
using umi3d.common.userCapture.description;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.UserCapture.Animation.CDK
{
    [TestFixture, TestOf(typeof(SkeletonAnimationNodeLoader))]
    public class SkeletonAnimationNodeLoader_Test
    {
        protected SkeletonAnimationNodeLoader skeletonAnimationNodeLoader;

        protected Mock<IEnvironmentManager> environmentManagerMock;
        protected Mock<ILoadingManager> loadingManagerMock;
        protected Mock<IUMI3DResourcesManager> resourcesManagerMock;
        protected Mock<ICoroutineService> coroutineManagerMock;
        protected Mock<ISkeletonManager> personnalSkeletonServiceMock;
        protected Mock<IUMI3DClientServer> clientServerMock;
        protected GameObject personalSkeletonGo;
        protected PersonalSkeleton personalSkeleton;
        protected GameObject skeletonNodeGo;
        protected Animator animator;

        private GameObject[] boneGameObjects;

        #region Test SetUp

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);

            skeletonNodeGo = new GameObject("Root");
            animator = skeletonNodeGo.AddComponent<Animator>();

            var bones = BoneTypeHelper.GetBoneNames().ToArray();
            var bonesGos = new Queue<GameObject>(bones.Length);

            HumanDescription humanDescription = new HumanDescription()
            {
                human = new HumanBone[bones.Length]
            };

            for (var i = 0; i < bones.Length; i++)
            {
                string name = BoneTypeConvertingExtensions.ConvertToBoneType(bones[i].Key).ToString();
                humanDescription.human[i] = new HumanBone
                {
                    boneName = name,
                    humanName = name
                };
                var go = new GameObject(name);
                go.transform.SetParent(skeletonNodeGo.transform);
                bonesGos.Enqueue(go);
            }

            boneGameObjects = bonesGos.ToArray();

            //? require proper hierarchy, otherwise the builder won't work
            //animator.avatar = AvatarBuilder.BuildHumanAvatar(skeletonNodeGo, humanDescription);

            //? issue with pose skeleton instanciation
            //personalSkeletonGo = new GameObject("Personal skeleton");
            //personalSkeleton = personalSkeletonGo.AddComponent<PersonalSkeleton>();
        }

        [SetUp]
        public virtual void SetUp()
        {
            environmentManagerMock = new();
            loadingManagerMock = new();
            resourcesManagerMock = new();
            coroutineManagerMock = new();
            personnalSkeletonServiceMock = new();
            clientServerMock = new();

            skeletonAnimationNodeLoader = new SkeletonAnimationNodeLoader(environmentManagerMock.Object,
                                                                          loadingManagerMock.Object,
                                                                          resourcesManagerMock.Object,
                                                                          coroutineManagerMock.Object,
                                                                          personnalSkeletonServiceMock.Object,
                                                                          clientServerMock.Object);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            UnityEngine.Object.Destroy(skeletonNodeGo);

            foreach (var go in boneGameObjects)
                UnityEngine.Object.Destroy(go);
        }

        #endregion Test SetUp

        #region CanReadUMI3DExtension

        [Test]
        public void CanReadUMI3DExtension_IsSkeletonAnimationNodeDto()
        {
            // GIVEN
            var dto = new SkeletonAnimationNodeDto();

            var data = new ReadUMI3DExtensionData(UMI3DGlobalID.EnvironmentId, dto);

            // WHEN
            var canRead = skeletonAnimationNodeLoader.CanReadUMI3DExtension(data);

            // THEN
            Assert.IsTrue(canRead);
        }

        [Test]
        public void CanReadUMI3DExtension_IsNotSkeletonAnimationNodeDto()
        {
            // GIVEN
            var dto = new UMI3DDto();

            var data = new ReadUMI3DExtensionData(UMI3DGlobalID.EnvironmentId, dto);

            // WHEN
            bool canRead = skeletonAnimationNodeLoader.CanReadUMI3DExtension(data);

            // THEN
            Assert.IsFalse(canRead);
        }

        #endregion CanReadUMI3DExtension

        #region ReadUMI3DExtension

        [Test]
        public async void ReadUMI3DExtension_SkeletonAnimatioNode_WithoutSkeletonMapper_NoAvatar()
        {
            // GIVEN
            var dto = new SkeletonAnimationNodeDto()
            {
                id = 1005uL,
                userId = 1008uL,
                pid = 0,
                active = true,
                animatorSelfTrackedParameters = new SkeletonAnimationParameterDto[0],
                mesh = new ResourceDto()
            };

            UMI3DNodeDto nDto = new();
            GlTFNodeDto glTFNodeDto = new GlTFNodeDto();

            glTFNodeDto.extensions = new GlTFNodeExtensions();
            glTFNodeDto.extensions.umi3d = dto;

            var instance = new UMI3DNodeInstance(UMI3DGlobalID.EnvironmentId, () => { })
            {
                dto = glTFNodeDto,
                gameObject = skeletonNodeGo
            };

            environmentManagerMock.Setup(x => x.GetNodeInstance(UMI3DGlobalID.EnvironmentId, dto.id)).Returns(instance);
            personnalSkeletonServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeleton);
            var loadingParametersMock = new Mock<IUMI3DUserCaptureLoadingParameters>();
            loadingManagerMock.Setup(x => x.AbstractLoadingParameters).Returns(loadingParametersMock.Object);
            loadingParametersMock.Setup(x => x.ChooseVariant(It.IsAny<List<FileDto>>())).Returns(new FileDto());
            resourcesManagerMock.Setup(x => x._LoadFile(It.IsAny<ulong>(), It.IsAny<FileDto>(), It.IsAny<IResourcesLoader>())).Returns(Task.FromResult(skeletonNodeGo as object));
            clientServerMock.Setup(x => x.OnLeavingEnvironment).Returns(new UnityEvent());
            loadingParametersMock.Setup(x => x.SelectLoader(It.IsAny<string>())).Returns(new ObjMeshDtoLoader());

            var data = new ReadUMI3DExtensionData(UMI3DGlobalID.EnvironmentId, dto) { node = skeletonNodeGo };

            // WHEN
            await skeletonAnimationNodeLoader.ReadUMI3DExtension(data);

            // THEN
            environmentManagerMock.Verify(x => x.GetNodeInstance(UMI3DGlobalID.EnvironmentId, dto.id));
            personnalSkeletonServiceMock.Verify(x => x.PersonalSkeleton, Times.Never());
        }

        #endregion ReadUMI3DExtension
    }
}