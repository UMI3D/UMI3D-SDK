using Moq;
using NUnit.Framework;
using System;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.common.userCapture.pose;

namespace EditMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(PoseManager))]
    public class PoseManager_Test
    {
        private PoseManager poseManager;
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

            poseManager = new PoseManager(skeletonManagerServiceMock.Object, environmentServiceMock.Object, clientServerServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            poseManager = null;
            
            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (PoseManager.Exists)
                PoseManager.Destroy();
        }

        #endregion SetUp

        #region TryActivatePoseAnimator

        [Test]
        public void TryActivatePoseAnimator()
        {
            // Given
            PoseAnimatorDto dto = new()
            {
                id = 1005uL,
                activationMode = (ushort)PoseAnimatorActivationMode.ON_REQUEST,
                relatedNodeId = 1006uL
            };

            Mock<PoseAnimator> poseAnimatorMock = new (dto, new PoseClip(new()), new IPoseCondition[0]);

            poseAnimatorMock.Setup(x=>x.TryActivate()).Returns(true);
            environmentServiceMock.Setup(x => x.GetEntityObject<PoseAnimator>(dto.id)).Returns(poseAnimatorMock.Object);

            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            var poseSubskeletonMock = new Mock<IPoseSubskeleton>();
            skeletonManagerServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);
            personalSkeletonMock.Setup(x => x.PoseSubskeleton).Returns(poseSubskeletonMock.Object);
            poseSubskeletonMock.Setup(x => x.StartPose(It.IsAny<PoseClip>(), false));
            // When
            bool result = false;
            TestDelegate action = () => result = poseManager.TryActivatePoseAnimator(dto.id);

            // Then
            Assert.DoesNotThrow(() => action());
            Assert.IsTrue(result);
        }

        #endregion TryActivatePoseAnimator

        #region PlayPoseClip

        [Test]
        public void PlayPoseClip_Null()
        {
            // Given
            PoseClip poseClip = null;

            // When
            TestDelegate action = () => poseManager.PlayPoseClip(poseClip);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void PlayPoseClip()
        {
            // Given
            PoseClipDto dto = new()
            {
                id = 1005uL,
                pose = new PoseDto() { }
            };
            PoseClip poseClip = new PoseClip(dto);

            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            var poseSubskeletonMock = new Mock<IPoseSubskeleton>();
            skeletonManagerServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);
            personalSkeletonMock.Setup(x => x.PoseSubskeleton).Returns(poseSubskeletonMock.Object);
            poseSubskeletonMock.Setup(x => x.StartPose(It.IsAny<PoseClip>(), false));

            // When
            poseManager.PlayPoseClip(poseClip);

            // Then
            poseSubskeletonMock.Verify(x => x.StartPose(It.IsAny<PoseClip>(), false), Times.Once());
        }

        #endregion PlayPoseClip

        #region StopPoseClip

        [Test]
        public void StopPoseClip_Null()
        {
            // Given
            PoseClip poseClip = null;

            // When
            TestDelegate action = () => poseManager.StopPoseClip(poseClip);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void StopPoseClip()
        {
            // Given
            PoseClipDto dto = new()
            {
                id = 1005uL,
                pose = new PoseDto() { }
            };
            PoseClip poseClip = new PoseClip(dto);

            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            var poseSubskeletonMock = new Mock<IPoseSubskeleton>();
            skeletonManagerServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);
            personalSkeletonMock.Setup(x => x.PoseSubskeleton).Returns(poseSubskeletonMock.Object);
            poseSubskeletonMock.Setup(x => x.StopPose(It.IsAny<PoseClip>()));

            // When
            poseManager.StopPoseClip(poseClip);

            // Then
            poseSubskeletonMock.Verify(x => x.StopPose(It.IsAny<PoseClip>()), Times.Once());
        }

        #endregion StopPoseClip
    }
}