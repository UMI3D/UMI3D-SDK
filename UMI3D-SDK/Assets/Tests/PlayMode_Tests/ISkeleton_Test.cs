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


    }

    [UnityTearDown]
    public void TearDown()
    {

    }

    #region Bindings
    [UnityTest]
    public IEnumerator TestUpdateBinding()
    {

        yield return null;
    }
    #endregion
}
