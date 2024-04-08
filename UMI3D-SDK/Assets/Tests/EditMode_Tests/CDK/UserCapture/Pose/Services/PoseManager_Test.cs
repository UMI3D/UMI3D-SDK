using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;

namespace EditMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(PoseService))]
    public class PoseManager_Test
    {
        private PoseService poseManager;
        private Mock<IEnvironmentManager> environmentServiceMock;
        private Mock<ISkeletonManager> skeletonManagerServiceMock;
        private Mock<IUMI3DClientServer> clientServerServiceMock;

        private Mock<IUMI3DUserCaptureLoadingParameters> userCaptureLoadingParametersMock;

        #region SetUp

        [SetUp]
        public void Setup()
        {
            ClearSingletons();

            environmentServiceMock = new ();
            skeletonManagerServiceMock = new ();
            userCaptureLoadingParametersMock = new();
            clientServerServiceMock = new();
            clientServerServiceMock.Setup(x => x.OnLeaving).Returns(new UnityEngine.Events.UnityEvent());
            clientServerServiceMock.Setup(x => x.OnRedirection).Returns(new UnityEngine.Events.UnityEvent());

            poseManager = new PoseService(skeletonManagerServiceMock.Object, environmentServiceMock.Object, clientServerServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            poseManager = null;
            
            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (PoseService.Exists)
                PoseService.Destroy();
        }

        #endregion SetUp

        #region TryActivatePoseAnimator

        [Test, TestOf(nameof(PoseService.TryActivatePoseAnimator))]
        public void TryActivatePoseAnimator()
        {
            // Given
            PoseAnimatorDto dto = new()
            {
                id = 1005uL,
                activationMode = (ushort)PoseAnimatorActivationMode.ON_REQUEST,
                relatedNodeId = 1006uL
            };

            Mock<PoseAnimator> poseAnimatorMock = new (dto, new PoseClip(new()), new IPoseCondition[0], null);

            poseAnimatorMock.Setup(x=>x.TryActivate()).Returns(true);
            IPoseAnimator pA = poseAnimatorMock.Object;
            environmentServiceMock.Setup(x => x.TryGetEntity(UMI3DGlobalID.EnvironmentId, dto.id, out pA)).Returns(true);
            
            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            var poseSubskeletonMock = new Mock<IPoseSubskeleton>();
            skeletonManagerServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);
            personalSkeletonMock.Setup(x => x.PoseSubskeleton).Returns(poseSubskeletonMock.Object);
            poseSubskeletonMock.Setup(x => x.StartPose(It.IsAny<PoseClip>(), false, It.IsAny<ISubskeletonDescriptionInterpolationPlayer.PlayingParameters>()));
            // When
            bool result = false;
            TestDelegate action = () => result = poseManager.TryActivatePoseAnimator(UMI3DGlobalID.EnvironmentId, dto.id);

            // Then
            Assert.DoesNotThrow(() => action());
            Assert.IsTrue(result);
        }

        #endregion TryActivatePoseAnimator
    }
}