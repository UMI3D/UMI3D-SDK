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


using System.Collections.Generic;
using NUnit.Framework;
using umi3d.cdk.userCapture;
using Moq;
using umi3d.common.userCapture;
using UnityEngine;
using umi3d.cdk.utils.extrapolation;

public class ISkeleton_Test
{
    List<AnimatedSkeleton> animatedSkeletons = null;
    ISkeleton fakeSkeleton = null;

    /// <summary>
    /// Class to fake an implementation of the interface 
    /// </summary>
    public class FakeSkeleton : ISkeleton
    {
        #region fields
        #region interface Fields
        List<ISubSkeleton> ISkeleton.Skeletons { get => skeletons; set => skeletons = value; }
        ulong ISkeleton.userId { get => userId; set => userId = value; }
        Vector3LinearDelayedExtrapolator ISkeleton.nodePositionExtrapolator { get => nodePositionExtrapolator; set => nodePositionExtrapolator = value; }
        QuaternionLinearDelayedExtrapolator ISkeleton.nodeRotationExtrapolator { get => nodeRotationExtrapolator; set => nodeRotationExtrapolator = value; }
        Dictionary<ulong, ISkeleton.SavedTransform> ISkeleton.savedTransforms { get => savedTransforms; set => savedTransforms = value; }

        #endregion
        public Dictionary<uint, ISkeleton.s_Transform> Bones { get; protected set; } = new();
        protected List<ISubSkeleton> skeletons;
        protected ulong userId;
        protected Vector3LinearDelayedExtrapolator nodePositionExtrapolator;
        protected QuaternionLinearDelayedExtrapolator nodeRotationExtrapolator;
        protected Dictionary<ulong, ISkeleton.SavedTransform> savedTransforms;

        #endregion

        public void UpdateFrame(UserTrackingFrameDto frame)
        {
            throw new System.NotImplementedException();
        }
    }

    [SetUp]
    public void SetUp()
    {
        animatedSkeletons = new List<AnimatedSkeleton>();
        fakeSkeleton = new FakeSkeleton();
        fakeSkeleton.Init();
    }

    [TearDown]
    public void TearDown()
    {

    }

    #region Skeleton calculation
    /// <summary>
    /// Test that we can handle a null list of sub - skeleton in the cmpute method
    /// </summary>
    [Test]
    public void Test_Compute_NullList()
    {
        //Given
        animatedSkeletons = null;

        //When
        ISkeleton results = fakeSkeleton.Compute();

        //Then
        Assert.IsTrue(results.Bones.Count == 0);
    }

    /// <summary>
    /// test that we can handle an empty list of sub skeleton i the compute méthod
    /// </summary>
    [Test]
    public void Test_Compute_EmptyList()
    {
        //Given
        ISkeleton iskeletton = fakeSkeleton;
        iskeletton.Skeletons = new List<ISubSkeleton>();

        //When
        ISkeleton results = iskeletton.Compute();

        //Then
        Assert.IsTrue(results.Bones.Count == 0);
    }

    /// <summary>
    /// Tesst that we handle a a subskeleton  that has a list of animated skeleton composed of not initialized animated skeletons
    /// </summary>
    [Test]
    public void Test_Compute_ListWithEmpties()
    {
        //Given
        ISkeleton iskeletton = fakeSkeleton;
        animatedSkeletons.Add(new AnimatedSkeleton(new SkeletonMapper()));
        animatedSkeletons.Add(new AnimatedSkeleton(new SkeletonMapper()));

        iskeletton.Skeletons.AddRange(animatedSkeletons);
        //When
        ISkeleton results = iskeletton.Compute();
        //Then
        Assert.IsTrue(results.Bones.Count == 0);
    }

    /// <summary>
    /// Test that we can compute correctly With one animated skelton with bones 
    /// </summary>
    [Test]
    public void Test_Compute_OneAnimatedSkeletonWithhBones()
    {
        Mock<AnimatedSkeleton> mock = new Mock<AnimatedSkeleton>();
        PoseDto poseDto = new PoseDto();
        poseDto.SetBonePoseDtoArray(new BonePoseDto[]
        {
            new BonePoseDto(BoneType.CenterFeet, Vector3.zero, Vector4.one),
        });

        Mock<SkeletonMapper> mockSkeletonMapper = new Mock<SkeletonMapper>();
        animatedSkeletons.Add(new AnimatedSkeleton(mockSkeletonMapper.Object));
        animatedSkeletons.Add(new AnimatedSkeleton(mockSkeletonMapper.Object));

        //Lets mock the method of interest
        mock.Setup(x => x.GetPose()).Returns(poseDto);
        //Given
        animatedSkeletons.Add(mock.Object);

        fakeSkeleton.Skeletons.AddRange(animatedSkeletons);

        //When
        ISkeleton results = fakeSkeleton.Compute();

        //Then
        Assert.IsTrue(results.Bones.Count == 1);
    }
    #endregion
}
