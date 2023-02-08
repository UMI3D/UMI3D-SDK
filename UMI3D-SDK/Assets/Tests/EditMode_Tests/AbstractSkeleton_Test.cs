using System.Collections.Generic;
using NUnit.Framework;
using umi3d.cdk.userCapture;
using umi3d.cdk.collaboration;
using Moq;
using umi3d.common.userCapture;
using UnityEngine;
using System.Linq;


public class AbstractSkeleton_Test : MonoBehaviour
{
    public class FakeSkeleton : AbstractSkeleton
    {
        public override void UpdateFrame(UserTrackingFrameDto frame)
        {
            throw new System.NotImplementedException();
        }
    }

    AbstractSkeleton fakeSkeleton = null;

    [SetUp]
    public void SetUp()
    {
        fakeSkeleton = new FakeSkeleton();  
    }

    [TearDown]
    public void TearDown()
    {

    }

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
