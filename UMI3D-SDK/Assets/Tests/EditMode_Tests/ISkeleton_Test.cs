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
        Dictionary<uint, ISkeleton.s_Transform> ISkeleton.Bones { get => bones; set => bones = value; }
        List<ISubSkeleton> ISkeleton.Skeletons { get => skeletons; set => skeletons = value; }
        bool ISkeleton.activeUserBindings { get => activeUserBindings; set => activeUserBindings = value; }
        ulong ISkeleton.userId { get => userId; set => userId = value; }
        Vector3LinearDelayedExtrapolator ISkeleton.nodePositionExtrapolator { get => nodePositionExtrapolator; set => nodePositionExtrapolator = value; }
        QuaternionLinearDelayedExtrapolator ISkeleton.nodeRotationExtrapolator { get => nodeRotationExtrapolator; set => nodeRotationExtrapolator = value; }
        List<ISkeleton.Bound> ISkeleton.bounds { get => bounds; set => bounds = value; }
        List<Transform> ISkeleton.boundRigs { get => boundRigs; set => boundRigs = value; }
        List<BindingDto> ISkeleton.userBindings { get => userBindings; set => userBindings = value; }
        Dictionary<ulong, ISkeleton.SavedTransform> ISkeleton.savedTransforms { get => savedTransforms; set => savedTransforms = value; }
        Dictionary<uint, (uint, Vector3)> ISkeleton.SkeletonHierarchy { get => skeletonHierarchy; set => skeletonHierarchy = value; }


        Transform ISkeleton.HipsAnchor { get => hipsAnchor; set => hipsAnchor = value; }

        #endregion
        protected Dictionary<uint, ISkeleton.s_Transform> bones;
        protected List<ISubSkeleton> skeletons;
        protected bool activeUserBindings;
        protected ulong userId;
        protected Vector3LinearDelayedExtrapolator nodePositionExtrapolator;
        protected QuaternionLinearDelayedExtrapolator nodeRotationExtrapolator;
        protected List<ISkeleton.Bound> bounds;
        protected List<Transform> boundRigs;
        protected List<BindingDto> userBindings;
        protected Dictionary<ulong, ISkeleton.SavedTransform> savedTransforms;
        protected Dictionary<uint, (uint, Vector3)> skeletonHierarchy = new Dictionary<uint, (uint, Vector3)>();
        [SerializeField]
        protected Transform hipsAnchor;

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
        Assert.IsTrue(results.Bones.Count == 1);
    }

    /// <summary>
    /// Test that we can compute correctly With one animated skelton with bones 
    /// </summary>
    [Test]
    public void Test_Compute_OneAnimatedSkeletonWithhBones()
    {
        Mock<AnimatedSkeleton> mock = new Mock<AnimatedSkeleton>();
        PoseDto poseDto = new PoseDto();
        poseDto.SetBonePoseDtoArray(new List<BoneDto>
        {
            new BoneDto() {boneType = BoneType.CenterFeet, rotation = Vector4.one},
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
    #region Bindings
    #region Add/remove
    #region public methods
    /// <summary>
    /// Test that adding null bindings will not break our user bindings system
    /// </summary>
    [Test]
    public void TestAddNullBindingNullList()
    {
        fakeSkeleton.AddBinding(0, null);

        Assert.IsTrue(fakeSkeleton.userBindings == null);
    }

    /// <summary>
    /// Test that we can add a binding
    /// </summary>
    [Test]
    public void TestAddBinding()
    {
        BindingDto dto = new BindingDto();
        fakeSkeleton.AddBinding(0, dto);

        Assert.IsTrue(fakeSkeleton.userBindings.Count == 1);
    }
    /// <summary>
    ///     testc that we can add multiple bindings at once
    /// </summary>
    [Test]
    public void TestAddMultipleSameBindings()
    {
        BindingDto dto = new BindingDto(1 , true, null);
        fakeSkeleton.AddBinding(0, dto);
        BindingDto dto2 = new BindingDto(1, true, null);
        fakeSkeleton.AddBinding(0, dto2);

        Assert.IsTrue(fakeSkeleton.userBindings.Count == 1);
    }
    /// <summary>
    /// test taht we can add multiple different bindings at once
    /// </summary>
    [Test]
    public void TestAddMultipleDifferentBindings()
    {
        BindingDto dto = new BindingDto(5648, true, null);
        fakeSkeleton.AddBinding(0, dto);
        BindingDto dto2 = new BindingDto(94856, true, null);
        fakeSkeleton.AddBinding(1, dto2);

        Assert.IsTrue(fakeSkeleton.userBindings.Count == 2);
    }

    /// <summary>
    /// test that we can add a binding at a specific index 
    /// </summary>
    [Test]
    public void TestRemoveBindingAtIndex()
    {
        BindingDto dto = new BindingDto(5648, true, null);
        fakeSkeleton.AddBinding(0, dto);
        BindingDto dto2 = new BindingDto(94856, true, null);
        fakeSkeleton.AddBinding(1, dto2);

        fakeSkeleton.RemoveBinding(0);

        Assert.IsTrue(fakeSkeleton.userBindings.Contains(dto2));
        Assert.IsTrue(fakeSkeleton.userBindings.Count == 1);
    }
    #endregion
    #region private methods

    #endregion
    #endregion
    #endregion
}
