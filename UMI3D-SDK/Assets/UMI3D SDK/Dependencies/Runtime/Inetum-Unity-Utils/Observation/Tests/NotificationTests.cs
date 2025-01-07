/*
Copyright 2019 - 2024 Inetum

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

using AutoFixture;
using inetum.unityUtils.observation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class NotificationTests
{
    public struct FooStruct: IFooA { }
    
    public class BaseFooClass { }
    public class FooClass: BaseFooClass, IFooA { }

    public enum FooEnum { }

    public interface IFooA { }
    public interface IFooB { }

    public class ConstructorTest
    {
        [Test]
        public void GivenNullNullNull_WhenConstructor_ThenLogError()
        {
            string id = null;
            object publisher = null;
            Dictionary<string, object> info = null;

            string message1 =
            $"[Notification.Notification] Error: create a new notification with id null or empty.";
            string message2 = 
            $"[Notification.Notification] Error: create a new notification with a null publisher.\n" +
            $"Having a null publisher is a bad practice because it increase complexity while debugging.";
            Notification notification = new(id, publisher, info);

            LogAssert.Expect(LogType.Error, message1);
            LogAssert.Expect(LogType.Error, message2);
            Assert.AreEqual(notification.ID, id);
            Assert.AreEqual(notification.Publisher, publisher);
            Assert.AreEqual(notification.Info, info);
        }

        [Test]
        public void GivenEmptyNullNull_WhenConstructor_ThenLogError()
        {
            string id = "";
            object publisher = null;
            Dictionary<string, object> info = null;

            string message1 =
            $"[Notification.Notification] Error: create a new notification with id null or empty.";
            string message2 = 
            $"[Notification.Notification] Error: create a new notification with a null publisher.\n" +
            $"Having a null publisher is a bad practice because it increase complexity while debugging.";
            Notification notification = new(id, publisher, info);

            LogAssert.Expect(LogType.Error, message1);
            LogAssert.Expect(LogType.Error, message2);
            Assert.AreEqual(notification.ID, id);
            Assert.AreEqual(notification.Publisher, publisher);
            Assert.AreEqual(notification.Info, info);
        }

        [Test]
        public void GivenNullPublisherNull_WhenConstructor_ThenLogError()
        {
            string id = null;
            object publisher = this;
            Dictionary<string, object> info = null;

            Notification notification = new(id, publisher, info);

            LogAssert.Expect(LogType.Error, $"[Notification.Notification] Error: create a new notification with id null or empty.");
            Assert.AreEqual(notification.ID, id);
            Assert.AreEqual(notification.Publisher, publisher);
            Assert.AreEqual(notification.Info, info);
        }

        [Test]
        public void GivenEmptyPublisherNull_WhenConstructor_ThenLogError()
        {
            string id = "";
            object publisher = this;
            Dictionary<string, object> info = null;

            Notification notification = new(id, publisher, info);

            LogAssert.Expect(LogType.Error, $"[Notification.Notification] Error: create a new notification with id null or empty.");
            Assert.AreEqual(notification.ID, id);
            Assert.AreEqual(notification.Publisher, publisher);
            Assert.AreEqual(notification.Info, info);
        }

        [Test]
        public void GivenIdNullNull_WhenConstructor_ThenLogError()
        {
            string id = "My new id";
            object publisher = null;
            Dictionary<string, object> info = null;

            string message = 
            $"[Notification.Notification] Error: create a new notification with a null publisher.\n" +
            $"Having a null publisher is a bad practice because it increase complexity while debugging.";
            Notification notification = new(id, publisher, info);

            LogAssert.Expect(LogType.Error, message);
            Assert.AreEqual(notification.ID, id);
            Assert.AreEqual(notification.Publisher, publisher);
            Assert.AreEqual(notification.Info, info);
        }

        [Test]
        public void GivenIdPublisherNull_WhenConstructor_ThenGoodNotification()
        {
            string id = "My new id";
            object publisher = this;
            Dictionary<string, object> info = null;

            Notification notification = new(id, publisher, info);

            Assert.AreEqual(notification.ID, id);
            Assert.AreEqual(notification.Publisher, publisher);
            Assert.AreEqual(notification.Info, info);
        }

        [Test]
        public void GivenIdPublisherInfo_WhenConstructor_ThenGoodNotification()
        {
            string id = "My new id";
            object publisher = this;
            Dictionary<string, object> info = new() { { "An info", true } };

            Notification notification = new(id, publisher, info);

            Assert.AreEqual(notification.ID, id);
            Assert.AreEqual(notification.Publisher, publisher);
            Assert.AreEqual(notification.Info, info);
        }
    }

    public class TryGetInfoTest
    {
        Fixture fixture;

        [SetUp]
        public void Setup()
        {
            fixture = new Fixture();
        }

        [TearDown]
        public void Teardown()
        {
            fixture = null;
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoWithNullKey_ThenLogErrorAndFalseAndNull()
        {
            string id = fixture.Create<string>();
            string key = fixture.Create<string>();
            Notification notificationNullInfo = new(id, this, null);
            Notification notificationEmptyInfo = new(id, this, new());
            Notification notificationInfo = new(id, this, new() { { key, true } });

            bool result1 = notificationNullInfo.TryGetInfo(null, out object info1);
            bool result2 = notificationEmptyInfo.TryGetInfo(null, out object info2);
            bool result3 = notificationInfo.TryGetInfo(null, out object info3);

            LogAssert.Expect(LogType.Error, $"[Notification.TryGetInfo] Error: key is null for notification '{id}'.");
            LogAssert.Expect(LogType.Error, $"[Notification.TryGetInfo] Error: key is null for notification '{id}'.");
            LogAssert.Expect(LogType.Error, $"[Notification.TryGetInfo] Error: key is null for notification '{id}'.");
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.Null(info1);
            Assert.Null(info2);
            Assert.Null(info3);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoKeyWithUnmatchedKey_ThenLogErrorAndFalseAndNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            Notification notificationNullInfo = new(id, this, null);
            Notification notificationEmptyInfo = new(id, this, new());
            Notification notificationInfo = new(id, this, new() { { infoKey, true } });
            string unmatchedKey = fixture.Create<string>();

            string errorMessage1 = 
            $"[Notification.TryGetInfo] Error: key '{unmatchedKey}' not found for notification '{id}'.\n" +
            $"Reason: info is null.";
            bool result1 = notificationNullInfo.TryGetInfo(unmatchedKey, out object info1);

            string errorMessage2 = 
            $"[Notification.TryGetInfo] Error: key '{unmatchedKey}' not found for notification '{id}'.\n" +
            $"Reason: info is empty.";
            bool result2 = notificationEmptyInfo.TryGetInfo(unmatchedKey, out object info2);

            string errorMessage3 = 
            $"[Notification.TryGetInfo] Error: key '{unmatchedKey}' not found for notification '{id}'.\n" +
            $"Reason: info does not contain '{unmatchedKey}'.";
            bool result3 = notificationInfo.TryGetInfo(unmatchedKey, out object info3);

            LogAssert.Expect(LogType.Error, errorMessage1);
            LogAssert.Expect(LogType.Error, errorMessage2);
            LogAssert.Expect(LogType.Error, errorMessage3);
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.Null(info1);
            Assert.Null(info2);
            Assert.Null(info3);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoWithNullKeyAndNoLogError_ThenFalseAndNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            Notification notificationNullInfo = new(id, this, null);
            Notification notificationEmptyInfo = new(id, this, new());
            Notification notificationInfo = new(id, this, new() { { infoKey, true } });

            bool result1 = notificationNullInfo.TryGetInfo(null, out object info1, false);
            bool result2 = notificationEmptyInfo.TryGetInfo(null, out object info2, false);
            bool result3 = notificationInfo.TryGetInfo(null, out object info3, false);

            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.Null(info1);
            Assert.Null(info2);
            Assert.Null(info3);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoKeyWithUnmatchedKeyAndNoLogError_ThenFalseAndNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            Notification notificationNullInfo = new(id, this, null);
            Notification notificationEmptyInfo = new(id, this, new());
            Notification notificationInfo = new(id, this, new() { { infoKey, true } });
            string unmatchedKey = fixture.Create<string>();

            bool result1 = notificationNullInfo.TryGetInfo(unmatchedKey, out object info1, false);
            bool result2 = notificationEmptyInfo.TryGetInfo(unmatchedKey, out object info2, false);
            bool result3 = notificationInfo.TryGetInfo(unmatchedKey, out object info3, false);

            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.Null(info1);
            Assert.Null(info2);
            Assert.Null(info3);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoWithClass_ThenTrueAndValue()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            FooClass value = new();
            Notification notification = new(id, this, new() { { infoKey, value } });

            bool result = notification.TryGetInfo(infoKey, out object info);

            Assert.True(result);
            Assert.AreEqual(info, value);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoWithStruct_ThenTrueAndValue()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            FooStruct value = new();
            Notification notification = new(id, this, new() { { infoKey, value } });

            bool result = notification.TryGetInfo(infoKey, out object info);

            Assert.True(result);
            Assert.AreEqual(info, value);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoWithEnum_ThenTrueAndValue()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            FooEnum value = new();
            Notification notification = new(id, this, new() { { infoKey, value } });

            bool result = notification.TryGetInfo(infoKey, out object info);

            Assert.True(result);
            Assert.AreEqual(info, value);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoWithInterface_ThenTrueAndValue()
        {
            string id = fixture.Create<string>();
            string infoKey1 = fixture.Create<string>();
            string infoKey2 = fixture.Create<string>();
            IFooA value1 = new FooStruct();
            IFooA value2 = new FooClass();
            Notification notification = new(id, this, new() { { infoKey1, value1 }, { infoKey2, value2 } });

            bool result1 = notification.TryGetInfo(infoKey1, out object info1);
            bool result2 = notification.TryGetInfo(infoKey2, out object info2);

            Assert.True(result1);
            Assert.AreEqual(info1, value1);
            Assert.True(result2);
            Assert.AreEqual(info2, value2);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoWithValueNull_ThenTrueAndValueNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            string value = null;
            Notification notification = new(id, this, new() { { infoKey, value } });

            bool result = notification.TryGetInfo(infoKey, out object info);

            Assert.True(result);
            Assert.AreEqual(info, value);
        }
    }

    public class TryGetInfoTTest
    {
        Fixture fixture;

        [SetUp]
        public void Setup()
        {
            fixture = new Fixture();
        }

        [TearDown]
        public void Teardown()
        {
            fixture = null;
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithNullKey_ThenLogErrorAndFalseAndNull()
        {
            string id = fixture.Create<string>();
            string key = fixture.Create<string>();
            Notification notificationNullInfo = new(id, this, null);
            Notification notificationEmptyInfo = new(id, this, new());
            Notification notificationInfo = new(id, this, new() { { key, true } });

            string errorMessage = $"[Notification.TryGetInfo] Error: key is null for notification '{id}'.";
            bool result1 = notificationNullInfo.TryGetInfoT(null, out object info1);
            bool result2 = notificationEmptyInfo.TryGetInfoT(null, out object info2);
            bool result3 = notificationInfo.TryGetInfoT(null, out object info3);

            LogAssert.Expect(LogType.Error, errorMessage);
            LogAssert.Expect(LogType.Error, errorMessage);
            LogAssert.Expect(LogType.Error, errorMessage);
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.Null(info1);
            Assert.Null(info2);
            Assert.Null(info3);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithUnmatchedKey_ThenLogErrorAndFalseAndNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            Notification notificationNullInfo = new(id, this, null);
            Notification notificationEmptyInfo = new(id, this, new());
            Notification notificationInfo = new(id, this, new() { { infoKey, true } });
            string unmatchedKey = fixture.Create<string>();

            string errorMessage1 = 
            $"[Notification.TryGetInfo] Error: key '{unmatchedKey}' not found for notification '{id}'.\n" +
            $"Reason: info is null.";
            bool result1 = notificationNullInfo.TryGetInfoT(unmatchedKey, out object info1);

            string errorMessage2 = 
            $"[Notification.TryGetInfo] Error: key '{unmatchedKey}' not found for notification '{id}'.\n" +
            $"Reason: info is empty.";
            bool result2 = notificationEmptyInfo.TryGetInfoT(unmatchedKey, out object info2);

            string errorMessage3 = 
            $"[Notification.TryGetInfo] Error: key '{unmatchedKey}' not found for notification '{id}'.\n" +
            $"Reason: info does not contain '{unmatchedKey}'.";
            bool result3 = notificationInfo.TryGetInfoT(unmatchedKey, out object info3);

            LogAssert.Expect(LogType.Error, errorMessage1);
            LogAssert.Expect(LogType.Error, errorMessage2);
            LogAssert.Expect(LogType.Error, errorMessage3);
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.Null(info1);
            Assert.Null(info2);
            Assert.Null(info3);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithNullKeyNoLogError_ThenFalseAndNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            Notification notificationNullInfo = new(id, this, null);
            Notification notificationEmptyInfo = new(id, this, new());
            Notification notificationInfo = new(id, this, new() { { infoKey, true } });

            bool result1 = notificationNullInfo.TryGetInfo(null, out object info1, false);
            bool result2 = notificationEmptyInfo.TryGetInfo(null, out object info2, false);
            bool result3 = notificationInfo.TryGetInfo(null, out object info3, false);

            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.Null(info1);
            Assert.Null(info2);
            Assert.Null(info3);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithUnmatchedKeyAndNoLogError_ThenFalseAndNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            Notification notificationNullInfo = new(id, this, null);
            Notification notificationEmptyInfo = new(id, this, new());
            Notification notificationInfo = new(id, this, new() { { infoKey, true } });
            string unmatchedKey = fixture.Create<string>();

            bool result1 = notificationNullInfo.TryGetInfo(unmatchedKey, out object info1, false);
            bool result2 = notificationEmptyInfo.TryGetInfo(unmatchedKey, out object info2, false);
            bool result3 = notificationInfo.TryGetInfo(unmatchedKey, out object info3, false);

            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
            Assert.Null(info1);
            Assert.Null(info2);
            Assert.Null(info3);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithStructValueButWrongType_ThenLogErrorAndFalseAndNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            FooStruct value = new();
            Notification notification = new(id, this, new() { { infoKey, value } });

            string errorMessage1 = 
            $"[Notification.TryGetInfoT] Error: notification '{id}' does not contain key '{infoKey}' of type {typeof(FooClass)}.\n" +
            $"Type of the object is {typeof(FooStruct)}.";
            bool result1 = notification.TryGetInfoT(infoKey, out FooClass info1);

            string errorMessage2 =
            $"[Notification.TryGetInfoT] Error: notification '{id}' does not contain key '{infoKey}' of type {typeof(FooEnum)}.\n" +
            $"Type of the object is {typeof(FooStruct)}.";
            bool result2 = notification.TryGetInfoT(infoKey, out FooEnum info2);

            string errorMessage3 =
            $"[Notification.TryGetInfoT] Error: notification '{id}' does not contain key '{infoKey}' of type {typeof(bool)}.\n" +
            $"Type of the object is {typeof(FooStruct)}.";
            bool result3 = notification.TryGetInfoT(infoKey, out bool info3);

            LogAssert.Expect(LogType.Error, errorMessage1);
            LogAssert.Expect(LogType.Error, errorMessage2);
            LogAssert.Expect(LogType.Error, errorMessage3);
            Assert.False(result1);
            Assert.Null(info1);
            Assert.False(result2);
            Assert.AreEqual(info2, default(FooEnum));
            Assert.False(result3);
            Assert.AreEqual(info3, default(bool));
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithClassValueButWrongType_ThenLogErrorAndFalseAndNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            FooClass value = new();
            Notification notification = new(id, this, new() { { infoKey, value } });

            string errorMessage1 = 
            $"[Notification.TryGetInfoT] Error: notification '{id}' does not contain key '{infoKey}' of type {typeof(FooStruct)}.\n" +
            $"Type of the object is {typeof(FooClass)}.";
            bool result1 = notification.TryGetInfoT(infoKey, out FooStruct info1);

            string errorMessage2 =
            $"[Notification.TryGetInfoT] Error: notification '{id}' does not contain key '{infoKey}' of type {typeof(FooEnum)}.\n" +
            $"Type of the object is {typeof(FooClass)}.";
            bool result2 = notification.TryGetInfoT(infoKey, out FooEnum info2);
            
            string errorMessage3 =
            $"[Notification.TryGetInfoT] Error: notification '{id}' does not contain key '{infoKey}' of type {typeof(string)}.\n" +
            $"Type of the object is {typeof(FooClass)}.";
            bool result3 = notification.TryGetInfoT(infoKey, out string info3);

            LogAssert.Expect(LogType.Error, errorMessage1);
            LogAssert.Expect(LogType.Error, errorMessage2);
            LogAssert.Expect(LogType.Error, errorMessage3);
            Assert.False(result1);
            Assert.AreEqual(info1, default(FooStruct));
            Assert.False(result2);
            Assert.AreEqual(info2, default(FooEnum));
            Assert.False(result3);
            Assert.Null(info3);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithEnumValueButWrongType_ThenLogErrorAndFalseAndNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            FooEnum value = new();
            Notification notification = new(id, this, new() { { infoKey, value } });

            string errorMessage1 =
            $"[Notification.TryGetInfoT] Error: notification '{id}' does not contain key '{infoKey}' of type {typeof(FooStruct)}.\n" +
            $"Type of the object is {typeof(FooEnum)}.";
            bool result1 = notification.TryGetInfoT(infoKey, out FooStruct info1);

            string errorMessage2 =
            $"[Notification.TryGetInfoT] Error: notification '{id}' does not contain key '{infoKey}' of type {typeof(FooClass)}.\n" +
            $"Type of the object is {typeof(FooEnum)}.";
            bool result2 = notification.TryGetInfoT(infoKey, out FooClass info2);

            string errorMessage3 =
            $"[Notification.TryGetInfoT] Error: notification '{id}' does not contain key '{infoKey}' of type {typeof(LogOption)}.\n" +
            $"Type of the object is {typeof(FooEnum)}.";
            bool result3 = notification.TryGetInfoT(infoKey, out LogOption info3);

            LogAssert.Expect(LogType.Error, errorMessage1);
            LogAssert.Expect(LogType.Error, errorMessage2);
            LogAssert.Expect(LogType.Error, errorMessage3);
            Assert.False(result1);
            Assert.AreEqual(info1, default(FooStruct));
            Assert.False(result2);
            Assert.Null(info2);
            Assert.False(result3);
            Assert.AreEqual(info3, default(LogOption));
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithValueNullButWrongValueType_TheLogErrorAndFalseAndNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            FooClass value = null;
            Notification notification = new(id, this, new() { { infoKey, value } });

            string errorMessage1 =
            $"[Notification.TryGetInfoT] Error: notification '{id}' does not contain key '{infoKey}' of type {typeof(FooStruct)}.\n" +
            $"Type of the object is Unknown because the value is null.";
            bool result1 = notification.TryGetInfoT(infoKey, out FooStruct info1);
            string errorMessage2 =
            $"[Notification.TryGetInfoT] Error: notification '{id}' does not contain key '{infoKey}' of type {typeof(FooEnum)}.\n" +
            $"Type of the object is Unknown because the value is null.";
            bool result2 = notification.TryGetInfoT(infoKey, out FooEnum info2);

            LogAssert.Expect(LogType.Error, errorMessage1);
            LogAssert.Expect(LogType.Error, errorMessage2);
            Assert.False(result1);
            Assert.AreEqual(info1, default(FooStruct));
            Assert.False(result2);
            Assert.AreEqual(info2, default(FooEnum));
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithValueNullAndRefType_ThenLogWarningAndTrueAndNull()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            FooClass value = null;
            Notification notification = new(id, this, new() { { infoKey, value } });

            string errorMessage1 =
            $"[Notification.TryGetInfoT] Warning: notification '{id}' has a null value for key '{infoKey}'.\n" +
            $"The initial type of a null value is Unknown but you are trying to cast it in '{typeof(string)}'.";
            bool result1 = notification.TryGetInfoT(infoKey, out string info1);

            string errorMessage2 =
            $"[Notification.TryGetInfoT] Warning: notification '{id}' has a null value for key '{infoKey}'.\n" +
            $"The initial type of a null value is Unknown but you are trying to cast it in '{typeof(FooClass)}'.";
            bool result2 = notification.TryGetInfoT(infoKey, out FooClass info2);

            string errorMessage3 =
            $"[Notification.TryGetInfoT] Warning: notification '{id}' has a null value for key '{infoKey}'.\n" +
            $"The initial type of a null value is Unknown but you are trying to cast it in '{typeof(IFooA)}'.";
            bool result3 = notification.TryGetInfoT(infoKey, out IFooA info3);

            LogAssert.Expect(LogType.Warning, errorMessage1);
            LogAssert.Expect(LogType.Warning, errorMessage2);
            LogAssert.Expect(LogType.Warning, errorMessage3);
            Assert.True(result1);
            Assert.Null(info1);
            Assert.True(result2);
            Assert.Null(info2);
            Assert.True(result3);
            Assert.Null(info3);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithClass_ThenTruAndValue()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            FooClass value = new();
            Notification notification = new(id, this, new() { { infoKey, value } });

            bool result1 = notification.TryGetInfoT(infoKey, out BaseFooClass info1);
            bool result2 = notification.TryGetInfoT(infoKey, out FooClass info2);
            bool result3 = notification.TryGetInfoT(infoKey, out IFooA info3);

            Assert.True(result1);
            Assert.AreEqual(info1, value);
            Assert.True(result2);
            Assert.AreEqual(info2, value);
            Assert.True(result3);
            Assert.AreEqual(info3, value);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithEnum_ThenTruAndValue()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            FooEnum value = new();
            Notification notification = new(id, this, new() { { infoKey, value } });

            bool result = notification.TryGetInfoT(infoKey, out FooEnum info);

            Assert.True(result);
            Assert.AreEqual(info, value);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithStruct_ThenTruAndValue()
        {
            string id = fixture.Create<string>();
            string infoKey = fixture.Create<string>();
            FooStruct value = new();
            Notification notification = new(id, this, new() { { infoKey, value } });

            bool result = notification.TryGetInfoT(infoKey, out FooStruct info);

            Assert.True(result);
            Assert.AreEqual(info, value);
        }

        [Test]
        public void GivenNotification_WhenTryGetInfoTWithNullable_ThenTruAndValue()
        {
            string id = fixture.Create<string>();
            string infoKey1 = fixture.Create<string>();
            string infoKey2 = fixture.Create<string>();
            string infoKey3 = fixture.Create<string>();
            FooStruct? value1 = new();
            FooStruct value2 = new();
            Notification notification = new(
                id, 
                this, 
                new() { { infoKey1, value1 }, { infoKey2, value2 }, { infoKey3, null } }
            );

            bool result1 = notification.TryGetInfoT(infoKey1, out FooStruct? info1);
            bool result2 = notification.TryGetInfoT(infoKey2, out FooStruct? info2);
            bool result3 = notification.TryGetInfoT(infoKey3, out FooStruct? info3);

            Assert.True(result1);
            Assert.True(info1.HasValue);
            Assert.AreEqual(info1.Value, value1.Value);

            Assert.True(result2);
            Assert.True(info2.HasValue);
            Assert.AreEqual(info2.Value, value2);

            Assert.True(result3);
            Assert.False(info3.HasValue);
        }
    }

    public class LogErrorTest
    {
        [Test]
        public void GivenNullNullNull_WhenConstructor_ThenLogError()
        {
            string id = new Fixture().Create<string>();
            Notification notification = new(id, this, null);
            string infoKey = new Fixture().Create<string>();
            string message = "This is an error message";

            string erroMessage1 =
            $"[NULL] notification: '{id}' does not contain key: 'NULL'.";
            notification.LogError(null, null, null);

            string erroMessage2 =
            $"[EMPTY] notification: '{id}' does not contain key: 'EMPTY'.";
            notification.LogError("", "", "");

            string erroMessage3 =
            $"[{GetType().FullName}] notification: '{id}' does not contain key: '{infoKey}'.\n" + message;
            notification.LogError(GetType().FullName, infoKey, message);

            LogAssert.Expect(LogType.Error, erroMessage1);
            LogAssert.Expect(LogType.Error, erroMessage2);
            LogAssert.Expect(LogType.Error, erroMessage3);
        }
    }
}
