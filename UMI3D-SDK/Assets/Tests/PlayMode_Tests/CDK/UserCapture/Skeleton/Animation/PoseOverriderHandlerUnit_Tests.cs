using Moq;
using NUnit.Framework;
using PlayMode_Tests;
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PoseOverriderHandlerUnit_Tests
{
    private PoseOverriderContainerHandlerUnit unit = null;

    private Mock<UMI3DEnvironmentLoader> environmentLoaderServiceMock;

    private Mock<UMI3DCollaborationClientServer> collaborationClientServerMock;

    private Mock<TrackedSkeleton> TrackedSkeletonMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
    }

    [SetUp]
    public void SetUp()
    {
        environmentLoaderServiceMock = new Mock<UMI3DEnvironmentLoader>();
        collaborationClientServerMock = new Mock<UMI3DCollaborationClientServer>();
        TrackedSkeletonMock = new Mock<TrackedSkeleton>();

        unit = new PoseOverriderContainerHandlerUnit(environmentLoaderServiceMock.Object, TrackedSkeletonMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        unit = null;

        if (UMI3DEnvironmentLoader.Exists)
            UMI3DEnvironmentLoader.Destroy();

    }
    [Test]
    public void TestNullData()
    {
        //Given
        UMI3DPoseOverriderContainerDto container = null;

        TrackedSkeletonMock.Setup(x => x.GetBonePosition(13)).Returns(new Vector3(15, 15, 0));
        UMI3DNodeInstance i = new UMI3DNodeInstance(() => Debug.Log("loaded"));
        i.gameObject = new GameObject("hey");
        environmentLoaderServiceMock.Setup(x => x.GetNodeInstance(It.IsAny<ulong>())).Returns(i);
        environmentLoaderServiceMock.Setup(x => x.GetEntityInstance(It.IsAny<ulong>())).Returns(i);

        //When
        Assert.IsFalse(unit.SetPoseOverriderContainer(container));

        Assert.IsFalse(unit.OnTrigger());
    }

    /// <summary>
    /// test that we can handle an empty list of sub skeleton i the compute méthod
    /// </summary>
    [Test]
    public void TestPoseOverriderSorting()
    {
        //Given
        UMI3DPoseOverriderContainerDto container = GenerateASimplePoseContainer();

        //When
        unit.SetPoseOverriderContainer(container);

        //Then
        Assert.IsTrue(unit.GetEnvironmentalPoseOverriders().Count == 1);
        Assert.IsTrue(unit.GetNonEnvironmentalPoseOverriders().Count == 2);
    }

    [Test]
    public void TestOnTrigger_FalseCondtion()
    {
        //Given
        UMI3DPoseOverriderContainerDto container = GenerateASimplePoseContainer();

        TrackedSkeletonMock.Setup(x => x.GetBonePosition(13)).Returns(new Vector3(15,15,0));
        UMI3DNodeInstance i = new UMI3DNodeInstance(() => Debug.Log("loaded"));
        i.gameObject = new GameObject("hey");
        environmentLoaderServiceMock.Setup(x => x.GetNodeInstance(It.IsAny<ulong>())).Returns(i);
        environmentLoaderServiceMock.Setup(x => x.GetEntityInstance(It.IsAny<ulong>())).Returns(i);

        //When
        unit.SetPoseOverriderContainer(container);

        Assert.IsFalse(unit.OnTrigger());
    }

    [Test]
    public void TestOnTrigger_TrueCondtion()
    {
        //Given
        UMI3DPoseOverriderContainerDto container = GenerateASimplePoseContainer();

        TrackedSkeletonMock.Setup(x => x.GetBonePosition(13)).Returns(new Vector3(0, 0, 0));
        UMI3DNodeInstance i = new UMI3DNodeInstance(() => Debug.Log("loaded"));
        i.gameObject = new GameObject("hey");
        environmentLoaderServiceMock.Setup(x => x.GetNodeInstance(It.IsAny<ulong>())).Returns(i);
        environmentLoaderServiceMock.Setup(x => x.GetEntityInstance(It.IsAny<ulong>())).Returns(i);

        //When
        unit.SetPoseOverriderContainer(container);

        Assert.IsTrue(unit.OnTrigger());
    }

    #region HelperMethod
    private UMI3DPoseOverriderContainerDto GenerateASimplePoseContainer()
    {
        UMI3DPoseOverriderContainerDto container = new UMI3DPoseOverriderContainerDto()
        {
            poseOverriderDtos = new List<PoseOverriderDto>()
            {
                new PoseOverriderDto()
                {
                    poseIndexinPoseManager = 0,
                    isHoverEnter = true,
                    poseConditions = new List<PoseConditionDto>()
                    {
                        new MagnitudeConditionDto()
                        {
                            magnitude = 1,
                            boneOrigine = 13,
                            targetObjectId = 100012
                        }
                    }.ToArray()
                },
                new PoseOverriderDto()
                {
                    poseIndexinPoseManager = 0,
                    isTrigger = true,
                    poseConditions = new List<PoseConditionDto>()
                    {
                        new MagnitudeConditionDto()
                        {
                            magnitude = 1,
                            boneOrigine = 13,
                            targetObjectId = 100012
                        }
                    }.ToArray()
                },
                new PoseOverriderDto()
                {
                    poseIndexinPoseManager = 0,
                    poseConditions = new List<PoseConditionDto>()
                    {
                        new MagnitudeConditionDto()
                        {
                            magnitude = 1,
                            boneOrigine = 13,
                            targetObjectId = 100012
                        }
                    }.ToArray()
                },


            }.ToArray(),
        };

        return container;  
    }

    #endregion
}