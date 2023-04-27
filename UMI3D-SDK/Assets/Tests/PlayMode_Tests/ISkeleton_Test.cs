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

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using umi3d.cdk.userCapture;
using umi3d.cdk.collaboration;
using Moq;
using umi3d.common.userCapture;
using UnityEngine.SceneManagement;
using umi3d.cdk.utils.extrapolation;
using umi3d.common.collaboration;
using System;
using System.Reflection;
using System.Linq;
using umi3d.cdk;

public class ISkeleton_Test
{
    List<AnimatedSkeleton> animatedSkeletons = null;

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
        Dictionary<uint, (uint, Vector3)> ISkeleton.SkeletonHierarchy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Transform ISkeleton.HipsAnchor { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

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

    ISkeleton fakeSkeleton;

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("Tests/PlayMode_Tests/TestScenes/TESTSCENE_Bindings");
        animatedSkeletons = new List<AnimatedSkeleton>();
        fakeSkeleton = new FakeSkeleton();
        fakeSkeleton.Init();
    }

    [UnityTearDown]
    public void TearDown()
    {
        SceneManager.UnloadSceneAsync("Tests/PlayMode_Tests/TestScenes/TESTSCENE_Bindings");
    }

    #region Bindings
    #region SaveTranform
    /// <summary>
    /// Test if we don't save transforms if the dto received and the transfom received are null
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator TestSaveTransformWitNullInputs()
    {
        Type type = typeof(ISkeleton);
        if (type != null)
        {
            Transform obj = null;
            object[] parameters = new object[] { null, obj };
            type.GetTypeInfo().GetDeclaredMethods("SaveTransform").ToList()[1].Invoke(fakeSkeleton, parameters);

            Assert.IsTrue(fakeSkeleton.SavedTransforms.Count == 0);
        }

        yield return null;
    }

    /// <summary>
    /// Test that we save a transform when its not null
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator TestSaveTransform()
    {
        Type type = typeof(ISkeleton);
        if (type != null)
        {
            Transform obj = (new GameObject()).transform;
            object[] parameters = new object[] { null, obj };
            type.GetTypeInfo().GetDeclaredMethods("SaveTransform").ToList()[1].Invoke(fakeSkeleton, parameters);
            Assert.IsTrue(fakeSkeleton.SavedTransforms.Count == 1);
        }

        yield return null;
    }
    #endregion
    #endregion
}
