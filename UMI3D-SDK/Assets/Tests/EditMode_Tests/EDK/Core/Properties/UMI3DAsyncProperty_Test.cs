using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class UMI3DAsyncProperty_Test
{
    /// <remarks>
    /// Test To Do :
    /// Add Group [0.n users]
    /// Remove Group [0.n users]
    /// Add Property To Group [0.n users]
    /// Remove Property From Group [0.n users]
    /// Add User to group [0.n property]
    /// Remove User from group [0.n property]
    /// 
    /// Set value
    /// Set value for user only [async or not]
    /// Set value for group [async user or not]
    /// 
    /// Sync
    /// Sync user
    /// Sync group
    /// SyncAll
    /// Desync
    /// </remarks>

    // A Test behaves as an ordinary method
    [Test]
    public void UMI3DAsyncProperty_TestSimplePasses()
    {
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator UMI3DAsyncProperty_TestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
