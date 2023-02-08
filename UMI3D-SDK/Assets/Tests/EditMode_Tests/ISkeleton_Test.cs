using System.Collections.Generic;
using NUnit.Framework;
using umi3d.cdk.userCapture;
using umi3d.cdk.collaboration;
using Moq;
using umi3d.common.userCapture;
using UnityEngine;
using System.Linq;

public class ISkeleton_Test
{
    List<AnimatedSkeleton> animatedSkeletons = null;
    ISkeleton fakeSkeleton = null;

    public class FakeSkeleton : ISkeleton
    {
        public Dictionary<uint, ISkeleton.s_Transform> Bones { get => bones; set => bones = value; }
        private Dictionary<uint, ISkeleton.s_Transform> bones;
        public List<ISubSkeleton> Skeletons { get => skeletons; set => skeletons = value; }
        private List<ISubSkeleton> skeletons;

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
    //#region public methods
    //[Test]
    //public void TestAddNullBindingNullList()
    //{
    //    ISkeleton skeleton = (collaborativeSkeleton as ISkeleton);
    //    skeleton.AddBinding(0, null);

    //    Assert.IsTrue(skeleton.userBindings == null);
    //}

    //[Test]
    //public void TestAddBinding()
    //{
    //    ISkeleton skeleton = (collaborativeSkeleton as ISkeleton);
    //    BoneBindingDto dto = new BoneBindingDto();
    //    skeleton.AddBinding(0, dto);

    //    Assert.IsTrue(skeleton.userBindings.Count == 1);
    //}

    //[Test]
    //public void TestAddMultipleSameBindings()
    //{
    //    ISkeleton skeleton = (collaborativeSkeleton as ISkeleton);
    //    BoneBindingDto dto = new BoneBindingDto();
    //    dto.bindingId = "123";
    //    skeleton.AddBinding(0, dto);
    //    BoneBindingDto dto2 = new BoneBindingDto();
    //    dto2.bindingId = "1";
    //    skeleton.AddBinding(0, dto2);

    //    Assert.IsTrue(skeleton.userBindings.Count == 1);
    //}

    //[Test]
    //public void TestAddMultipleDifferentBindings()
    //{
    //    ISkeleton skeleton = (collaborativeSkeleton as ISkeleton);
    //    BoneBindingDto dto = new BoneBindingDto();
    //    dto.bindingId = "5648";
    //    skeleton.AddBinding(0, dto);
    //    BoneBindingDto dto2 = new BoneBindingDto();
    //    dto2.bindingId = "94856";
    //    skeleton.AddBinding(0, dto2);

    //    Assert.IsTrue(skeleton.userBindings.Count == 2);
    //}

    //[Test]
    //public void TestRemoveBindingAtIndex() 
    //{
    //    ISkeleton skeleton = (collaborativeSkeleton as ISkeleton);
    //    BoneBindingDto dto = new BoneBindingDto();
    //    dto.bindingId = "5648";
    //    skeleton.AddBinding(0, dto);
    //    BoneBindingDto dto2 = new BoneBindingDto();
    //    dto2.bindingId = "94856";
    //    skeleton.AddBinding(1, dto2);


    //    Debug.Log("hehhehe");
    //    skeleton.RemoveBinding(0);
    //    Debug.Log("hehhehe");

    //    Assert.IsTrue(skeleton.userBindings.Contains(dto2));
    //    Assert.IsTrue(skeleton.userBindings.Count == 1);
    //}
    //#endregion
    #region private methods

    #endregion
    #endregion
    #endregion
}
