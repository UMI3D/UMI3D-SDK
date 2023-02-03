using System.Collections.Generic;
using NUnit.Framework;
using umi3d.cdk.userCapture;
using umi3d.cdk.collaboration;
using Moq;
using umi3d.common.userCapture;
using UnityEngine;

public class IPoseSkeleton_Test
{
    List<AnimatedSkeleton> animatedSkeletons = null;
    CollaborativeSkeleton collaborativeSkeleton = new CollaborativeSkeleton();

    [SetUp]
    public void SetUp()
    {
        animatedSkeletons = new List<AnimatedSkeleton>();
    }

    [TearDown]
    public void TearDown()
    {

    }

    #region Skeleton calculation
    [Test]
    public void Test_Compute_ListNotSet()
    {
        //Given
        ISkeleton iskeletton = (collaborativeSkeleton as ISkeleton);
        animatedSkeletons = null;
        iskeletton.Skeletons = animatedSkeletons?.ToArray();

        //When
        ISkeleton results = iskeletton.Compute();

        //Then
        Assert.IsTrue(results.Bones.Count == 0);
    }

    [Test]
    public void Test_Compute_EmptyList()
    {
        //Given
        ISkeleton iskeletton = (collaborativeSkeleton as ISkeleton);
        iskeletton.Skeletons = animatedSkeletons.ToArray();

        //When
        ISkeleton results = iskeletton.Compute();

        //Then
        Assert.IsTrue(results.Bones.Count == 0);  
    }

    [Test]
    public void Test_Compute_ListWithEmpties()
    {
        //Given
        ISkeleton iskeletton = (collaborativeSkeleton as ISkeleton);
        animatedSkeletons.Add(new AnimatedSkeleton());
        animatedSkeletons.Add(new AnimatedSkeleton());

        iskeletton.Skeletons = animatedSkeletons.ToArray();

        //When
        ISkeleton results = iskeletton.Compute();

        //Then
        Assert.IsTrue(results.Bones.Count == 0);
    }

    [Test]
    public void Test_Compute_OneAnimatedSkeletonWithhBones()
    {
        //TODO -- finish this test
        Mock<AnimatedSkeleton> mock = new Mock<AnimatedSkeleton>();
        PoseDto poseDto= new PoseDto();
        poseDto.bones = new BonePoseDto[]
        {
            new BonePoseDto(BoneType.CenterFeet, Vector3.zero, Quaternion.identity),
        };
        //Lets mock the method of interest
        mock.Setup(x => x.GetPose()).Returns(poseDto);
        //Given
        ISkeleton iskeletton = (collaborativeSkeleton as ISkeleton);
        animatedSkeletons.Add(mock.Object);

        iskeletton.Skeletons = animatedSkeletons.ToArray();

        //When
        ISkeleton results = iskeletton.Compute();

        //Then
        Assert.IsTrue(results.Bones.Count == 1);
    }
    #endregion

    #region Bindings
    #region


    #endregion
    #endregion
}
