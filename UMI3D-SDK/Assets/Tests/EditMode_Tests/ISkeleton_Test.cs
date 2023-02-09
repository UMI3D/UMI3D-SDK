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

    public class FakeSkeleton : ISkeleton
    {
        #region fields
        #region interface Fields
        public Dictionary<uint, ISkeleton.s_Transform> Bones { get => bones; set => bones = value; }
        public List<ISubSkeleton> Skeletons { get => skeletons; set => skeletons = value; }
        bool ISkeleton.activeUserBindings { get => activeUserBindings; set => activeUserBindings = value; }
        ulong ISkeleton.userId { get => userId; set => userId = value; }
        Vector3LinearDelayedExtrapolator ISkeleton.nodePositionExtrapolator { get => nodePositionExtrapolator; set => nodePositionExtrapolator = value; }
        QuaternionLinearDelayedExtrapolator ISkeleton.nodeRotationExtrapolator { get => nodeRotationExtrapolator; set => nodeRotationExtrapolator = value; }
        List<ISkeleton.Bound> ISkeleton.bounds { get => bounds; set => bounds = value; }
        List<Transform> ISkeleton.boundRigs { get => boundRigs; set => boundRigs = value; }
        List<BoneBindingDto> ISkeleton.userBindings { get => userBindings; set => userBindings = value; }
        Dictionary<ISkeleton.BoundObject, ISkeleton.SavedTransform> ISkeleton.savedTransforms { get => savedTransforms; set => savedTransforms = value; }
        #endregion
        protected Dictionary<uint, ISkeleton.s_Transform> bones;
        protected List<ISubSkeleton> skeletons;
        protected bool activeUserBindings;
        protected ulong userId;
        protected Vector3LinearDelayedExtrapolator nodePositionExtrapolator;
        protected QuaternionLinearDelayedExtrapolator nodeRotationExtrapolator;
        protected List<ISkeleton.Bound> bounds;
        protected List<Transform> boundRigs;
        protected List<BoneBindingDto> userBindings;
        protected Dictionary<ISkeleton.BoundObject, ISkeleton.SavedTransform> savedTransforms;
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

    [Test]
    public void Test_Compute_OneAnimatedSkeletonWithhBones()
    {
        Mock<AnimatedSkeleton> mock = new Mock<AnimatedSkeleton>();
        PoseDto poseDto= new PoseDto();
        poseDto.bones = new BonePoseDto[]
        {
            new BonePoseDto(BoneType.CenterFeet, Vector3.zero, Quaternion.identity),
        };

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
    [Test]
    public void TestAddNullBindingNullList()
    {
        fakeSkeleton.AddBinding(0, null);

        Assert.IsTrue(fakeSkeleton.userBindings == null);
    }

    [Test]
    public void TestAddBinding()
    {
        BoneBindingDto dto = new BoneBindingDto();
        fakeSkeleton.AddBinding(0, dto);

        Assert.IsTrue(fakeSkeleton.userBindings.Count == 1);
    }

    [Test]
    public void TestAddMultipleSameBindings()
    {
        BoneBindingDto dto = new BoneBindingDto();
        dto.bindingId = "123";
        fakeSkeleton.AddBinding(0, dto);
        BoneBindingDto dto2 = new BoneBindingDto();
        dto2.bindingId = "1";
        fakeSkeleton.AddBinding(0, dto2);

        Assert.IsTrue(fakeSkeleton.userBindings.Count == 1);
    }

    [Test]
    public void TestAddMultipleDifferentBindings()
    {
        BoneBindingDto dto = new BoneBindingDto();
        dto.bindingId = "5648";
        fakeSkeleton.AddBinding(0, dto);
        BoneBindingDto dto2 = new BoneBindingDto();
        dto2.bindingId = "94856";
        fakeSkeleton.AddBinding(0, dto2);

        Assert.IsTrue(fakeSkeleton.userBindings.Count == 2);
    }

    [Test]
    public void TestRemoveBindingAtIndex()
    {
        BoneBindingDto dto = new BoneBindingDto();
        dto.bindingId = "5648";
        fakeSkeleton.AddBinding(0, dto);
        BoneBindingDto dto2 = new BoneBindingDto();
        dto2.bindingId = "94856";
        fakeSkeleton.AddBinding(1, dto2);


        Debug.Log("hehhehe");
        fakeSkeleton.RemoveBinding(0);
        Debug.Log("hehhehe");

        Assert.IsTrue(fakeSkeleton.userBindings.Contains(dto2));
        Assert.IsTrue(fakeSkeleton.userBindings.Count == 1);
    }
    #endregion
    #region private methods

    #endregion
    #endregion
    #endregion
}
