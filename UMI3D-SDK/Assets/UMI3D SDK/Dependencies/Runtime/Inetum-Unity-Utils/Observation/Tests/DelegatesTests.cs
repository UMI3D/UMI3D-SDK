/*
Copyright 2019 - 2025 Inetum

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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using inetum.unityUtils.observation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DelegatesTests
{
    public class AddTest
    {
        interface ITestInterface { }
        class TestDelegate : ITestInterface { }

        Delegates<ITestInterface> _delegates;

        [SetUp]
        public void SetUp()
        {
            _delegates = new Delegates<ITestInterface>();
        }

        [Test]
        public void GivenEmptyDelegates_WhenAddNewDelegate_ThenDelegateIsAdded()
        {
            // Given
            ITestInterface newDelegate = new TestDelegate();

            // When
            _delegates.Add(newDelegate);

            // Then
            Assert.AreEqual(1, _delegates.Count);
            Assert.IsTrue(_delegates[0].TryGetTarget(out ITestInterface target));
            Assert.AreEqual(newDelegate, target);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenAddExistingDelegate_ThenDelegateIsNotAddedAgain()
        {
            // Given
            ITestInterface existingDelegate = new TestDelegate();
            _delegates.Add(existingDelegate);

            // When
            _delegates.Add(existingDelegate);

            // Then
            Assert.AreEqual(1, _delegates.Count);
            Assert.IsTrue(_delegates[0].TryGetTarget(out ITestInterface target));
            Assert.AreEqual(existingDelegate, target);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenAddNewDelegate_ThenDelegateIsAdded()
        {
            // Given
            ITestInterface existingDelegate = new TestDelegate();
            ITestInterface newDelegate = new TestDelegate();
            _delegates.Add(existingDelegate);

            // When
            _delegates.Add(newDelegate);

            // Then
            Assert.AreEqual(2, _delegates.Count);
            Assert.IsTrue(_delegates[0].TryGetTarget(out ITestInterface target1));
            Assert.AreEqual(existingDelegate, target1);
            Assert.IsTrue(_delegates[1].TryGetTarget(out ITestInterface target2));
            Assert.AreEqual(newDelegate, target2);
        }

        [Test]
        public void GivenEmptyDelegates_WhenAddNullDelegate_ThenDelegateIsNotAdded()
        {
            // Given
            ITestInterface nullDelegate = null;

            // When
            _delegates.Add(nullDelegate);

            // Then
            Assert.AreEqual(0, _delegates.Count);
        }

        [Test]
        public void GivenDelegatesWithNullReference_WhenAddNewDelegate_ThenNullReferenceIsReplaced()
        {
            // Given
            void AddDelegate()
            {
                ITestInterface existingDelegate = new TestDelegate();
                _delegates.Add(existingDelegate);
                existingDelegate = null;
            }

            AddDelegate();

            // Force garbage collection to simulate weak reference becoming null
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Verify that the weak reference is null
            Assert.IsFalse(_delegates[0].TryGetTarget(out _), "If this fail then you have to recompile.");

            ITestInterface newDelegate = new TestDelegate();

            // When
            _delegates.Add(newDelegate);

            // Then
            Assert.AreEqual(1, _delegates.Count);
            Assert.IsTrue(_delegates[0].TryGetTarget(out ITestInterface target));
            Assert.AreEqual(newDelegate, target);
        }
    }

    public class RemoveTest
    {
        interface ITestInterface { }
        class TestDelegate : ITestInterface { }

        Delegates<ITestInterface> _delegates;

        [SetUp]
        public void SetUp()
        {
            _delegates = new Delegates<ITestInterface>();
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenRemoveExistingDelegate_ThenDelegateIsRemoved()
        {
            // Given
            ITestInterface existingDelegate = new TestDelegate();
            _delegates.Add(existingDelegate);

            // When
            bool result = _delegates.Remove(existingDelegate);

            // Then
            Assert.IsTrue(result);
            Assert.AreEqual(0, _delegates.Count);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenRemoveNonExistingDelegate_ThenDelegateIsNotRemoved()
        {
            // Given
            ITestInterface existingDelegate = new TestDelegate();
            ITestInterface nonExistingDelegate = new TestDelegate();
            _delegates.Add(existingDelegate);

            // When
            bool result = _delegates.Remove(nonExistingDelegate);

            // Then
            Assert.IsFalse(result);
            Assert.AreEqual(1, _delegates.Count);
            Assert.IsTrue(_delegates[0].TryGetTarget(out ITestInterface target));
            Assert.AreEqual(existingDelegate, target);
        }

        [Test]
        public void GivenEmptyDelegates_WhenRemoveDelegate_ThenDelegateIsNotRemoved()
        {
            // Given
            ITestInterface nonExistingDelegate = new TestDelegate();

            // When
            bool result = _delegates.Remove(nonExistingDelegate);

            // Then
            Assert.IsFalse(result);
            Assert.AreEqual(0, _delegates.Count);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenRemoveNullDelegate_ThenDelegateIsNotRemoved()
        {
            // Given
            ITestInterface existingDelegate = new TestDelegate();
            _delegates.Add(existingDelegate);

            // When
            bool result = _delegates.Remove(null);

            // Then
            Assert.IsFalse(result);
            Assert.AreEqual(1, _delegates.Count);
            Assert.IsTrue(_delegates[0].TryGetTarget(out ITestInterface target));
            Assert.AreEqual(existingDelegate, target);
        }

        [Test]
        public void GivenDelegatesWithNullReference_WhenRemoveExistingDelegate_ThenDelegateIsRemoved()
        {
            // Given
            void AddDelegate()
            {
                ITestInterface existingDelegate = new TestDelegate();
                _delegates.Add(existingDelegate);
            }

            AddDelegate();

            // Force garbage collection to simulate weak reference becoming null
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Verify that the weak reference is null
            Assert.IsFalse(_delegates[0].TryGetTarget(out _), "If this fail then you have to recompile.");

            ITestInterface newDelegate = new TestDelegate();
            _delegates.Add(newDelegate);

            // When
            bool result = _delegates.Remove(newDelegate);

            // Then
            Assert.IsTrue(result);
            Assert.AreEqual(0, _delegates.Count);
        }
    }

    public class InsertTest
    {
        interface ITestInterface { }
        class TestDelegate : ITestInterface { }

        Delegates<ITestInterface> _delegates;

        [SetUp]
        public void SetUp()
        {
            _delegates = new Delegates<ITestInterface>();
        }

        [Test]
        public void GivenEmptyDelegates_WhenInsertNewDelegate_ThenDelegateIsInsertedAtIndex()
        {
            // Given
            ITestInterface newDelegate = new TestDelegate();

            // When
            _delegates.Insert(0, newDelegate);

            // Then
            Assert.AreEqual(1, _delegates.Count);
            Assert.IsTrue(_delegates[0].TryGetTarget(out ITestInterface target));
            Assert.AreEqual(newDelegate, target);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenInsertNewDelegate_ThenDelegateIsInsertedAtIndex()
        {
            // Given
            ITestInterface existingDelegate = new TestDelegate();
            ITestInterface newDelegate = new TestDelegate();
            _delegates.Add(existingDelegate);

            // When
            _delegates.Insert(0, newDelegate);

            // Then
            Assert.AreEqual(2, _delegates.Count);
            Assert.IsTrue(_delegates[0].TryGetTarget(out ITestInterface target1));
            Assert.AreEqual(newDelegate, target1);
            Assert.IsTrue(_delegates[1].TryGetTarget(out ITestInterface target2));
            Assert.AreEqual(existingDelegate, target2);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenInsertExistingDelegate_ThenDelegateIsMovedToIndex()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            ITestInterface delegate2 = new TestDelegate();
            _delegates.Add(delegate1);
            _delegates.Add(delegate2);

            // When
            _delegates.Insert(0, delegate2);

            // Then
            Assert.AreEqual(2, _delegates.Count);
            Assert.IsTrue(_delegates[0].TryGetTarget(out ITestInterface target1));
            Assert.AreEqual(delegate2, target1);
            Assert.IsTrue(_delegates[1].TryGetTarget(out ITestInterface target2));
            Assert.AreEqual(delegate1, target2);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenInsertExistingDelegateAtSameIndex_ThenDelegateRemainsAtIndex()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            ITestInterface delegate2 = new TestDelegate();
            _delegates.Add(delegate1);
            _delegates.Add(delegate2);

            // When
            _delegates.Insert(1, delegate2);

            // Then
            Assert.AreEqual(2, _delegates.Count);
            Assert.IsTrue(_delegates[0].TryGetTarget(out ITestInterface target1));
            Assert.AreEqual(delegate1, target1);
            Assert.IsTrue(_delegates[1].TryGetTarget(out ITestInterface target2));
            Assert.AreEqual(delegate2, target2);
        }

        [Test]
        public void GivenEmptyDelegates_WhenInsertDelegateWithInvalidIndex_ThenArgumentOutOfRangeExceptionIsThrown()
        {
            // Given
            ITestInterface newDelegate = new TestDelegate();

            // When & Then
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _delegates.Insert(1, newDelegate));
            Assert.That(ex.Message, Does.Contain("Index must be within the bounds of the List."));
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenInsertDelegateWithInvalidIndex_ThenArgumentOutOfRangeExceptionIsThrown()
        {
            // Given
            ITestInterface existingDelegate = new TestDelegate();
            _delegates.Add(existingDelegate);

            // When & Then
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _delegates.Insert(2, existingDelegate));
            Assert.That(ex.Message, Does.Contain("Index must be within the bounds of the List."));
        }

        [Test]
        public void GivenDelegatesWithNullReference_WhenInsertNewDelegate_ThenNullReferenceIsReplaced()
        {
            // Given
            void AddDelegate()
            {
                ITestInterface existingDelegate = new TestDelegate();
                _delegates.Add(existingDelegate);
            }

            AddDelegate();

            // Force garbage collection to simulate weak reference becoming null
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Verify that the weak reference is null
            Assert.IsFalse(_delegates[0].TryGetTarget(out _), "If this fail then you have to recompile.");

            ITestInterface newDelegate = new TestDelegate();

            // When
            _delegates.Insert(0, newDelegate);

            // Then
            Assert.AreEqual(1, _delegates.Count);
            Assert.IsTrue(_delegates[0].TryGetTarget(out ITestInterface target));
            Assert.AreEqual(newDelegate, target);
        }
    }

    public class RemoveAtTest
    {
        interface ITestInterface { }
        class TestDelegate : ITestInterface { }

        Delegates<ITestInterface> _delegates;

        [SetUp]
        public void SetUp()
        {
            _delegates = new Delegates<ITestInterface>();
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenRemoveAtValidIndex_ThenDelegateIsRemoved()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            ITestInterface delegate2 = new TestDelegate();
            _delegates.Add(delegate1);
            _delegates.Add(delegate2);

            // When
            _delegates.RemoveAt(0);

            // Then
            Assert.AreEqual(1, _delegates.Count);
            Assert.True(_delegates[0].TryGetTarget(out ITestInterface _delegate1));
            Assert.AreEqual(delegate2, _delegate1);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenRemoveAtLastIndex_ThenDelegateIsRemoved()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            ITestInterface delegate2 = new TestDelegate();
            _delegates.Add(delegate1);
            _delegates.Add(delegate2);

            // When
            _delegates.RemoveAt(1);

            // Then
            Assert.AreEqual(1, _delegates.Count);
            Assert.True(_delegates[0].TryGetTarget(out ITestInterface _delegate1));
            Assert.AreEqual(delegate1, _delegate1);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenRemoveAtInvalidIndex_ThenArgumentOutOfRangeExceptionIsThrown()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            _delegates.Add(delegate1);

            // When & Then
            var ex = Assert.Throws<System.ArgumentOutOfRangeException>(() => _delegates.RemoveAt(1));
            Assert.That(ex.Message, Does.Contain("Index was out of range. Must be non-negative and less than the size of the collection."));
        }

        [Test]
        public void GivenEmptyDelegates_WhenRemoveAtAnyIndex_ThenArgumentOutOfRangeExceptionIsThrown()
        {
            // Given
            // An empty instance of Delegates<ITestInterface>

            // When & Then
            var ex = Assert.Throws<System.ArgumentOutOfRangeException>(() => _delegates.RemoveAt(0));
            Assert.That(ex.Message, Does.Contain("Index was out of range. Must be non-negative and less than the size of the collection."));
        }
    }

    public class ForEachTest
    {
        interface ITestInterface { }
        class TestDelegate : ITestInterface { }

        Delegates<ITestInterface> _delegates;

        [SetUp]
        public void SetUp()
        {
            _delegates = new Delegates<ITestInterface>();
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenForEachWithContinue_ThenAllDelegatesAreProcessed()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            ITestInterface delegate2 = new TestDelegate();
            _delegates.Add(delegate1);
            _delegates.Add(delegate2);

            List<ITestInterface> processedDelegates = new List<ITestInterface>();

            // When
            _delegates.ForEach(d =>
            {
                processedDelegates.Add(d);
                return Flow.Continue;
            });

            // Then
            Assert.AreEqual(2, processedDelegates.Count);
            Assert.Contains(delegate1, processedDelegates);
            Assert.Contains(delegate2, processedDelegates);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenForEachWithBreak_ThenLoopIsInterrupted()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            ITestInterface delegate2 = new TestDelegate();
            _delegates.Add(delegate1);
            _delegates.Add(delegate2);

            List<ITestInterface> processedDelegates = new List<ITestInterface>();

            // When
            _delegates.ForEach(d =>
            {
                processedDelegates.Add(d);
                return Flow.Break;
            });

            // Then
            Assert.AreEqual(1, processedDelegates.Count);
            Assert.Contains(delegate1, processedDelegates);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenForEachWithNotImplementedException_ThenWarningIsLogged()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            _delegates.Add(delegate1);

            // When & Then
            LogAssert.Expect(LogType.Warning, $"[Delegates<ITestInterface>] Warning: a delegate raise a NotImplementedException");

            _delegates.ForEach(d =>
            {
                throw new NotImplementedException();
            });
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenForEachWithException_ThenErrorIsLogged()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            _delegates.Add(delegate1);

            // When & Then
            LogAssert.Expect(LogType.Error, $"[Delegates<ITestInterface>] Error: a delegate raise an exception");
            LogAssert.Expect(LogType.Exception, $"Exception: {new Exception().Message}");

            _delegates.ForEach(d =>
            {
                throw new Exception();
            });
        }
    }

    public class ForEachWithReturnTypeTest
    {
        interface ITestInterface { }
        class TestDelegate : ITestInterface { }

        Delegates<ITestInterface> _delegates;

        [SetUp]
        public void SetUp()
        {
            _delegates = new Delegates<ITestInterface>();
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenForEachWithAction_ThenReturnValuesAreCollected()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            ITestInterface delegate2 = new TestDelegate();
            _delegates.Add(delegate1);
            _delegates.Add(delegate2);

            // When
            List<int> result = _delegates.ForEach(d => 42);

            // Then
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(42, result[0]);
            Assert.AreEqual(42, result[1]);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenForEachWithNotImplementedException_ThenWarningIsLoggedAndDefaultValuesAreReturned()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            _delegates.Add(delegate1);

            // When & Then
            LogAssert.Expect(LogType.Warning, $"[Delegates<ITestInterface>] Warning: a delegate raise a NotImplementedException");

            List<int> result = _delegates.ForEach<int>(d =>
            {
                throw new NotImplementedException();
            });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(default(int), result[0]);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenForEachWithException_ThenErrorIsLoggedAndDefaultValuesAreReturned()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            _delegates.Add(delegate1);

            // When & Then
            LogAssert.Expect(LogType.Error, $"[Delegates<ITestInterface>] Error: a delegate raise an exception");
            LogAssert.Expect(LogType.Exception, $"Exception: {new Exception().Message}");

            List<int> result = _delegates.ForEach<int>(d =>
            {
                throw new Exception();
            });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(default(int), result[0]);
        }

        [Test]
        public void GivenNonEmptyDelegates_WhenForEachWithNullAction_ThenDefaultValuesAreReturned()
        {
            // Given
            ITestInterface delegate1 = new TestDelegate();
            _delegates.Add(delegate1);

            // When
            List<int> result = _delegates.ForEach<int>(null);

            // Then
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(default(int), result[0]);
        }
    }
}