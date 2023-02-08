using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using NUnit.Framework;
using umi3d.cdk.userCapture;
using umi3d.cdk.collaboration;
using Moq;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ISkeleton_Test
{
    List<AnimatedSkeleton> animatedSkeletons = null;
    PersonalSkeleton personalSkeleton;
    ISkeleton skeleton = null;

    [UnitySetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("Assets/Tests/PlayMode_Tests/TestScenes/TESTSCENE_Bindings");

        GameObject go = new GameObject();
        personalSkeleton = go.AddComponent<PersonalSkeleton>();
        skeleton = (personalSkeleton as ISkeleton);
        BoneBindingDto dto = new BoneBindingDto();
        dto.bindingId = "5648";
        skeleton.AddBinding(0, dto);
        BoneBindingDto dto2 = new BoneBindingDto();
        dto2.bindingId = "94856";
        skeleton.AddBinding(0, dto2);

        Debug.Log(skeleton.userId);

    }

    [UnityTearDown]
    public void TearDown()
    {

    }

    #region Bindings
    [UnityTest]
    public IEnumerator TestUpdateBinding()
    {
        Debug.Log(skeleton.userId);

        yield return null;
    }
    #endregion
}
