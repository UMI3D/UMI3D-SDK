using System.Collections.Generic;
using NUnit.Framework;
using umi3d.cdk.userCapture;
using umi3d.cdk.collaboration;
using Moq;
using umi3d.common.userCapture;

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

    [Test]
    public void Test_Compute_ListNotSet()
    {
        //Given
        ISkeleton iskeletton = (collaborativeSkeleton as ISkeleton);
        animatedSkeletons = null;

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
        iskeletton.Skeletons = new();
        iskeletton.Skeletons.AddRange(animatedSkeletons);

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
        Mock<SkeletonMapper> mockSkeletonMapper = new Mock<SkeletonMapper>();
        animatedSkeletons.Add(new AnimatedSkeleton(mockSkeletonMapper.Object));
        animatedSkeletons.Add(new AnimatedSkeleton(mockSkeletonMapper.Object));

        iskeletton.Skeletons = new();
        iskeletton.Skeletons.AddRange(animatedSkeletons);

        //When
        ISkeleton results = iskeletton.Compute();

        //Then
        Assert.IsTrue(results.Bones.Count == 0);
    }

    [Test]
    public void Test_Compute_OneAnimatedSkeletonWithhBones()
    {

        Mock<AnimatedSkeleton> mock = new Mock<AnimatedSkeleton>();
        //Lets mock the method of interest
        mock.Setup(x => x.GetPose());
        //Given
        ISkeleton iskeletton = (collaborativeSkeleton as ISkeleton);
        Mock<SkeletonMapper> mockSkeletonMapper = new Mock<SkeletonMapper>();
        animatedSkeletons.Add(new AnimatedSkeleton(mockSkeletonMapper.Object));
        animatedSkeletons.Add(new AnimatedSkeleton(mockSkeletonMapper.Object));

        iskeletton.Skeletons = new();
        iskeletton.Skeletons.AddRange(animatedSkeletons);

        //When
        ISkeleton results = iskeletton.Compute();

        //Then
        Assert.IsTrue(results.Bones.Count == 0);
    }
}
