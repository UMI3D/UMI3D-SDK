using System.Collections.Generic;
using NUnit.Framework;
using umi3d.cdk.userCapture;
using umi3d.cdk.collaboration;
using System.Diagnostics;

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
        //Mock<ExternalStuff> mock = new Mock<ExternalStuff>();
        ////Lets mock the method of interest
        //mock.Setup(x => x.NameLengthInMyImaginaryWorld("Jean-Paul Muller")).Returns(159);
        ////Given
        //ISkeleton iskeletton = (collaborativeSkeleton as ISkeleton);
        //animatedSkeletons.Add(new AnimatedSkeleton());
        //animatedSkeletons.Add(new AnimatedSkeleton());

        //iskeletton.Skeletons = animatedSkeletons.ToArray();

        ////When
        //ISkeleton results = iskeletton.Compute();

        ////Then
        //Assert.IsTrue(results.Bones.Count == 0);
    }
}
