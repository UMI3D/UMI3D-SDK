using Moq;
using NUnit.Framework;
using PlayMode_Tests;
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoseOverriderHandlerUnit_Tests
{
    private List<AnimatedSkeleton> animatedSkeletons = new();
    private PersonalSkeleton personalSkeleton;

    private GameObject root;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
    }

    [SetUp]
    public void SetUp()
    {
        root = new GameObject();
        animatedSkeletons = new();
        personalSkeleton = root.AddComponent<PersonalSkeleton>();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.Destroy(root);
    }

    /// <summary>
    /// test that we can handle an empty list of sub skeleton i the compute méthod
    /// </summary>
    [Test]
    public void Compute_EmptyList()
    {
        //Given
        

        //When


        //Then

    }
}