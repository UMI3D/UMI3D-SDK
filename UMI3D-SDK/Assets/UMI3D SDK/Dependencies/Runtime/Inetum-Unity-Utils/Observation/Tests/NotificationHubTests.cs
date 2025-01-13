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

using AutoFixture;
using inetum.unityUtils.observation;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

public class NotificationHubTests
{
    public class GetSubscribersForTest
    {
        class FooClass { }

        [TearDown]
        public void TearDown()
        {
            NotificationHub.Default.Clear();
        }

        [Test]
        public void GivenIdNull_WhenGettingSubscribersForId_ThenLogError()
        {
            IEnumerable<object> subscribers1 = NotificationHub.Default.GetSubscribersFor(null);
            IEnumerable<object> subscribers2 = NotificationHub.Default.GetSubscribersFor("");

            LogAssert.Expect(LogType.Error, "[NotificationHub.GetSubscribersFor] Error: id is null or empty.");
            Assert.NotNull(subscribers1);
            Assert.AreEqual(subscribers1.Count(), 0);
            LogAssert.Expect(LogType.Error, "[NotificationHub.GetSubscribersFor] Error: id is null or empty.");
            Assert.NotNull(subscribers2);
            Assert.AreEqual(subscribers2.Count(), 0);
        }

        [Test]
        public void GivenIdAndASubscriptions_WhenGettingSubscribersForId_ThenSubscribers()
        {
            Fixture fixture = new();
            object subscriber1 = this;
            object subscriber2 = new FooClass();
            ID id = fixture.Create<string>();
            NotificationHub.Default.Subscribe(subscriber1, id, (Callback)(() => { }));
            NotificationHub.Default.Subscribe(subscriber2, id, (Callback)(() => { }));

            IEnumerable<object> subscribers = NotificationHub.Default.GetSubscribersFor(id);

            Assert.NotNull(subscribers);
            Assert.AreEqual(subscribers.Count(), 2);
            IEnumerator<object> subscribersEnumerator = subscribers.GetEnumerator();
            Assert.True(subscribersEnumerator.MoveNext());
            Assert.AreEqual(subscribersEnumerator.Current, subscriber1);
            Assert.True(subscribersEnumerator.MoveNext());
            Assert.AreEqual(subscribersEnumerator.Current, subscriber2);
        }

        [Test]
        public void GivenTwoSubscriptionsWithSameIDAndSubscriber_WhenGettingSubscribersForId_ThenOneSubscriber()
        {
            Fixture fixture = new();
            object subscriber = this;
            ID id = fixture.Create<string>();

            NotificationHub.Default.Subscribe(subscriber, id, (Callback)(() => { }));
            NotificationHub.Default.Subscribe(subscriber, id, (Callback)(() => { }));

            IEnumerable<object> subscribers = NotificationHub.Default.GetSubscribersFor(id);
            IEnumerator<object> subscribersEnumerator = subscribers.GetEnumerator();
            Assert.AreEqual(1, subscribers.Count());
            Assert.True(subscribersEnumerator.MoveNext());
            Assert.AreEqual(subscribersEnumerator.Current, subscriber);
        }
    }

    public class GetIdsForTest
    {
        [TearDown]
        public void TearDown()
        {
            NotificationHub.Default.Clear();
        }

        [Test]
        public void GivenIdNull_WhenGettingIdsForSubscriber_ThenLogError()
        {
            IEnumerable<string> ids = NotificationHub.Default.GetIdsFor(null);

            LogAssert.Expect(LogType.Error, "[NotificationHub.GetIdsFor] Error: subscriber is null.");
            Assert.NotNull(ids);
            Assert.AreEqual(ids.Count(), 0);
        }

        [Test]
        public void GivenIdAndASubscriptions_WhenGettingIdsForSubscriber_ThenIds()
        {
            Fixture fixture = new();
            object subscriber = this;
            ID id1 = fixture.Create<string>();
            ID id2 = fixture.Create<string>();
            NotificationHub.Default.Subscribe(subscriber, id1, (Callback)(() => { }));
            NotificationHub.Default.Subscribe(subscriber, id2, (Callback)(() => { }));

            IEnumerable<string> ids = NotificationHub.Default.GetIdsFor(subscriber);

            Assert.NotNull(ids);
            Assert.AreEqual(ids.Count(), 2);
            IEnumerator<object> idsEnumerator = ids.GetEnumerator();
            Assert.True(idsEnumerator.MoveNext());
            Assert.AreEqual(idsEnumerator.Current, id1.id);
            Assert.True(idsEnumerator.MoveNext());
            Assert.AreEqual(idsEnumerator.Current, id2.id);
        }

        [Test]
        public void GivenTwoSubscriptionsWithSameIDAndSubscriber_WhenGettingIdsForSubscriber_ThenOneId()
        {
            Fixture fixture = new();
            object subscriber = this;
            ID id = fixture.Create<string>();

            NotificationHub.Default.Subscribe(subscriber, id, (Callback)(() => { }));
            NotificationHub.Default.Subscribe(subscriber, id, (Callback)(() => { }));

            IEnumerable<string> ids = NotificationHub.Default.GetIdsFor(subscriber);
            IEnumerator<string> idsEnumerator = ids.GetEnumerator();
            Assert.AreEqual(1, ids.Count());
            Assert.True(idsEnumerator.MoveNext());
            Assert.AreEqual(idsEnumerator.Current, id.id);
        }
    }

    public class SubscribeTest
    {
        Fixture fixture;

        [SetUp]
        public void SetUp()
        {
            fixture = new Fixture();
        }

        [TearDown]
        public void TearDown()
        {
            fixture = null;
            NotificationHub.Default.Clear();
        }

        [Test]
        public void GivenIdNull_WhenSubscribing_ThenLogError()
        {
            NotificationHub.Default.Subscribe(null, null, (Callback)(() => { }));
            NotificationHub.Default.Subscribe(this, null, (Callback)(() => { }));

            LogAssert.Expect(LogType.Error, "[NotificationHub.Subscribe] Error: id is null or empty.");
            LogAssert.Expect(LogType.Error, "[NotificationHub.Subscribe] Error: id is null or empty.");
        }

        [Test]
        public void GivenSubscriberNull_WhenSubscribing_ThenLogError()
        {
            ID id = fixture.Create<string>();

            NotificationHub.Default.Subscribe(null, id, (Callback)(() => { }));

            LogAssert.Expect(LogType.Error, $"[NotificationHub.Subscribe] Error: subscriber is null for id '{id}'.");
        }

        [Test]
        public void GivenSubscriberAndId_WhenSubscribing_ThenASubscriptionHasBeenAdded()
        {
            object subscriber = this;
            ID id = fixture.Create<string>();

            NotificationHub.Default.Subscribe(subscriber, id, (Callback)(() => { }));

            IEnumerable<object> subscribers = NotificationHub.Default.GetSubscribersFor(id);
            Assert.AreEqual(1, subscribers.Count());
            IEnumerator<object> subscribersEnumerator = subscribers.GetEnumerator();
            Assert.True(subscribersEnumerator.MoveNext());
            Assert.AreEqual(subscribersEnumerator.Current, subscriber);

            IEnumerable<string> ids = NotificationHub.Default.GetIdsFor(subscriber);
            Assert.AreEqual(1, ids.Count());
            IEnumerator<string> idsEnumerator = ids.GetEnumerator();
            Assert.True(idsEnumerator.MoveNext());
            Assert.AreEqual(idsEnumerator.Current, id.id);
        }

        [Test]
        public void GivenSubscriberAndTwoIds_WhenSubscribingThreeTimes_ThenTwoSubscriptionsHaveBeenMandeOneForId1AndOneForId2()
        {
            object subscriber = this;
            ID id1 = fixture.Create<string>();
            ID id2 = fixture.Create<string>();

            NotificationHub.Default.Subscribe(subscriber, id1, (Callback)(() => { }));
            NotificationHub.Default.Subscribe(subscriber, id1, (Callback)(() => { }));
            NotificationHub.Default.Subscribe(subscriber, id2, (Callback)(() => { }));

            IEnumerable<object> subscribersForId1 = NotificationHub.Default.GetSubscribersFor(id1);
            Assert.AreEqual(1, subscribersForId1.Count());
            IEnumerable<object> subscribersForId2 = NotificationHub.Default.GetSubscribersFor(id2);
            Assert.AreEqual(1, subscribersForId2.Count());

            IEnumerator<object> subscribersForId1Enumerator = subscribersForId1.GetEnumerator();
            Assert.True(subscribersForId1Enumerator.MoveNext());
            Assert.AreEqual(subscriber, subscribersForId1Enumerator.Current);

            IEnumerator<object> subscribersForId2Enumerator = subscribersForId2.GetEnumerator();
            Assert.True(subscribersForId2Enumerator.MoveNext());
            Assert.AreEqual(subscriber, subscribersForId2Enumerator.Current);

            IEnumerable<string> ids = NotificationHub.Default.GetIdsFor(subscriber);
            IEnumerator<string> idsEnumerator = ids.GetEnumerator();
            Assert.AreEqual(2, ids.Count());
            Assert.True(idsEnumerator.MoveNext());
            Assert.AreEqual(id1, idsEnumerator.Current);
            Assert.True(idsEnumerator.MoveNext());
            Assert.AreEqual(id2, idsEnumerator.Current);
        }

        [Test]
        public void GivenASubscriberAndId_WhenSubscribing100TimesInDifferentThread_ThenOnlyOneSubscription()
        {
            object subscriber = this;
            ID id = fixture.Create<string>();
            var tasks = new List<Task>();
            int numberOfThreads = 100;

            for (int i = 0; i < numberOfThreads; i++)
            {
                tasks.Add(Task.Run(() => NotificationHub.Default.Subscribe(subscriber, id, (Callback)(() => { }))));
            }
            Task.WaitAll(tasks.ToArray());

            IEnumerable<string> ids = NotificationHub.Default.GetIdsFor(subscriber);
            Assert.AreEqual(1, ids.Count());
            IEnumerable<object> subscribers = NotificationHub.Default.GetSubscribersFor(id);
            Assert.AreEqual(1, subscribers.Count());
        }
    }

    public class UnsubscribeTest
    {
        class FooClass { }

        Fixture fixture;

        [SetUp]
        public void SetUp()
        {
            fixture = new Fixture();
        }

        [TearDown]
        public void TearDown()
        {
            fixture = null;
            NotificationHub.Default.Clear();
        }

        [Test]
        public void GivenNoSubscriptionAndNullSubscriber_WhenUnsubscribing_ThenLogError()
        {
            ID id = fixture.Create<string>();

            NotificationHub.Default.Unsubscribe(null);
            NotificationHub.Default.Unsubscribe(null, id);

            LogAssert.Expect(LogType.Error, "[NotificationHub.Unsubscribe] Error: subscriber is null.");
            LogAssert.Expect(LogType.Error, "[NotificationHub.Unsubscribe] Error: subscriber is null.");
        }

        [Test]
        public void GivenSubscription_WhenUnsubscribingWithAnIdThatIsNotRegistered_ThenLogWarning()
        {
            ID id1 = fixture.Create<string>();
            ID id2 = fixture.Create<string>();
            object subscriber = this;
            NotificationHub.Default.Subscribe(subscriber, id1, (Callback)(() => { }));

            NotificationHub.Default.Unsubscribe(subscriber, id2);

            LogAssert.Expect(LogType.Warning, $"[NotificationHub.Unsubscribe] Warning: no subscription for '{subscriber.GetType().FullName}' with id '{id2}'");
        }

        [Test]
        public void GivenNoSubscription_WhenUnsubscribing_ThenLogWarning()
        {
            ID id = fixture.Create<string>();

            NotificationHub.Default.Unsubscribe(this);
            NotificationHub.Default.Unsubscribe(this, id);

            LogAssert.Expect(LogType.Warning, $"[NotificationHub.Unsubscribe] Warning: no subscription for '{this.GetType().FullName}'.");
            LogAssert.Expect(LogType.Warning, $"[NotificationHub.Unsubscribe] Warning: no subscription for '{this.GetType().FullName}'.");
        }

        [Test]
        public void GivenSubscription_WhenUnsubscribing_ThenSubscriptionIsRemoved()
        {
            object subscriber1 = this;
            object subscriber2 = new FooClass();
            ID id1 = fixture.Create<string>();
            ID id2 = fixture.Create<string>();
            NotificationHub.Default.Subscribe(subscriber1, id1, (Callback)(() => { }));
            NotificationHub.Default.Subscribe(subscriber1, id2, (Callback)(() => { }));
            NotificationHub.Default.Subscribe(subscriber2, id1, (Callback)(() => { }));

            // --- New Test ---
            NotificationHub.Default.Unsubscribe(subscriber2);

            IEnumerable<string> idsForSubscriber2 = NotificationHub.Default.GetIdsFor(subscriber2);
            Assert.AreEqual(0, idsForSubscriber2.Count());

            IEnumerable<string> idsForSubscriber1 = NotificationHub.Default.GetIdsFor(subscriber1);
            Assert.AreEqual(2, idsForSubscriber1.Count());
            IEnumerator<string> idsForSubscriber1Enumerator = idsForSubscriber1.GetEnumerator();
            Assert.True(idsForSubscriber1Enumerator.MoveNext());
            Assert.AreEqual(id1, idsForSubscriber1Enumerator.Current);
            Assert.True(idsForSubscriber1Enumerator.MoveNext());
            Assert.AreEqual(id2, idsForSubscriber1Enumerator.Current);

            // --- New Test ---
            NotificationHub.Default.Unsubscribe(subscriber1, id1);

            idsForSubscriber2 = NotificationHub.Default.GetIdsFor(subscriber2);
            Assert.AreEqual(0, idsForSubscriber2.Count());

            idsForSubscriber1 = NotificationHub.Default.GetIdsFor(subscriber1);
            Assert.AreEqual(1, idsForSubscriber1.Count());
            idsForSubscriber1Enumerator = idsForSubscriber1.GetEnumerator();
            Assert.True(idsForSubscriber1Enumerator.MoveNext());
            Assert.AreEqual(id2, idsForSubscriber1Enumerator.Current);
        }

        [Test]
        public void Given100Subscriptions_WhenUnsubscribing100TimesInDifferentThread_ThenNoSubscriptions()
        {
            object subscriber = this;
            ID id = fixture.Create<string>();
            var tasks = new List<Task>();
            int numberOfThreads = 2;
            for (int i = 0; i < numberOfThreads; i++)
            {
                NotificationHub.Default.Subscribe(subscriber, id + i, (Callback)(() => { }));
            }
            IEnumerable<string> ids = NotificationHub.Default.GetIdsFor(subscriber);
            Assert.AreEqual(2, ids.Count());

            for (int i = 0; i < numberOfThreads; i++)
            {
                string currentId = id + i;
                tasks.Add(Task.Run(() => NotificationHub.Default.Unsubscribe(subscriber, currentId)));
            }
            Task.WaitAll(tasks.ToArray());

            ids = NotificationHub.Default.GetIdsFor(subscriber);
            Assert.AreEqual(0, ids.Count());
        }
    }

    public class NotifyTest
    {
        class FooClass { }

        Fixture fixture;

        [SetUp]
        public void SetUp()
        {
            fixture = new Fixture();
        }

        [TearDown]
        public void TearDown()
        {
            fixture = null;
            NotificationHub.Default.Clear();
        }

        [Test]
        public void GivenNoSubscriptionAndNullPublisherOrId_WhenNotifying_ThenLogErrorAndWarning()
        {
            ID id = fixture.Create<string>();

            // --- New Test ---
            NotificationHub.Default.Notify(null, null);

            LogAssert.Expect(LogType.Warning, "[NotificationHub.Notify] Warning: publisher is null. A null publisher makes debugging difficult.");
            LogAssert.Expect(LogType.Error, $"[NotificationHub.Notify] Error: id is null or empty when publisher 'Unknown' try to notify.");

            // --- New Test ---
            NotificationHub.Default.Notify(this, null);

            LogAssert.Expect(LogType.Error, $"[NotificationHub.Notify] Error: id is null or empty when publisher '{this.GetType().FullName}' try to notify.");

            // --- New Test ---
            NotificationHub.Default.Notify(null, id);

            LogAssert.Expect(LogType.Warning, "[NotificationHub.Notify] Warning: publisher is null. A null publisher makes debugging difficult.");
            LogAssert.Expect(LogType.Warning, $"[NotificationHub.Notify] Warning: 'Unknown' try to notify with id '{id}' but no one is listening.");

            // --- New Test ---
            NotificationHub.Default.Notify(this, id);

            LogAssert.Expect(LogType.Warning, $"[NotificationHub.Notify] Warning: '{this.GetType().FullName}' try to notify with id '{id}' but no one is listening.");
        }

        [Test]
        public void GivenSubscription_WhenNotifyingForAnotherID_ThenLogWarning()
        {
            object subscriber1 = this;
            ID id1 = fixture.Create<string>();
            ID id2 = fixture.Create<string>();
            NotificationHub.Default.Subscribe(subscriber1, id1, (Callback)(() => { }));

            NotificationHub.Default.Notify(subscriber1, id2);
            object publisher = new FooClass();
            NotificationHub.Default.Notify(publisher, id2);

            LogAssert.Expect(LogType.Warning, $"[NotificationHub.Notify] Warning: '{subscriber1.GetType().FullName}' try to notify with id '{id2}' but no one is listening.");
            LogAssert.Expect(LogType.Warning, $"[NotificationHub.Notify] Warning: '{publisher.GetType().FullName}' try to notify with id '{id2}' but no one is listening.");
        }

        [Test]
        public void GivenSubscriptions_WhenNotifying_ThenReturnTheNumberOfNotificationSent()
        {
            object subscriber1 = this;
            object subscriber2 = new FooClass();
            ID id1 = fixture.Create<string>();
            ID id2 = fixture.Create<string>();
            object publisher = new FooClass();

            // --- New Test ---
            NotificationHub.Default.Subscribe(subscriber1, id1, (Callback)(() => { }));
            int nbOfNotificationSent = NotificationHub.Default.Notify(publisher, id1);
            Assert.AreEqual(1, nbOfNotificationSent);

            // --- New Test ---
            NotificationHub.Default.Subscribe(subscriber1, id2, (Callback)(() => { }));
            nbOfNotificationSent = NotificationHub.Default.Notify(publisher, id1);
            Assert.AreEqual(1, nbOfNotificationSent);
            nbOfNotificationSent = NotificationHub.Default.Notify(publisher, id2);
            Assert.AreEqual(1, nbOfNotificationSent);

            // --- New Test ---
            NotificationHub.Default.Subscribe(subscriber2, id1, (Callback)(() => { }));
            nbOfNotificationSent = NotificationHub.Default.Notify(publisher, id1);
            Assert.AreEqual(2, nbOfNotificationSent);
            nbOfNotificationSent = NotificationHub.Default.Notify(publisher, id2);
            Assert.AreEqual(1, nbOfNotificationSent);

            // --- New Test ---
            NotificationHub.Default.Subscribe(subscriber2, id2, (Callback)(() => { }));
            nbOfNotificationSent = NotificationHub.Default.Notify(publisher, id1);
            Assert.AreEqual(2, nbOfNotificationSent);
            nbOfNotificationSent = NotificationHub.Default.Notify(publisher, id2);
            Assert.AreEqual(2, nbOfNotificationSent);
        }

        [Test]
        public void GivenASubscription_WhenNotifyingWithInfo_ThenCallbackHasInfo()
        {
            object subscriber = this;
            ID id = fixture.Create<string>();
            object publisher = new FooClass();
            string key = null;
            string value = null;
            void NotificationAction(Notification notification)
            {
                if (!notification.TryGetInfoT("key", out string _value))
                {
                    Assert.Fail();
                    return;
                }

                key = "key";
                value = _value;
            }
            NotificationHub.Default.Subscribe(subscriber, id, (Callback)(NotificationAction));

            Dictionary<string, object> info = new() { { "key", "value" } };
            NotificationHub.Default.Notify(publisher, id, info);

            Assert.AreEqual("key", key);
            Assert.AreEqual("value", value);
        }

        [Test]
        public void GivenTwoSubscriptionsAndASubscriptionInAnotherSubscription_WhenNotifying_ThenOnlyTwoCallbackAndANewSubscription()
        {
            object subscriber = this;
            object publisher = new FooClass();
            ID id1 = fixture.Create<string>();
            ID id2 = fixture.Create<string>();
            ID id3 = fixture.Create<string>();
            int count = 0;
            NotificationHub.Default.Subscribe(subscriber, id1, (Callback)(() =>
            {
                count++;
                NotificationHub.Default.Subscribe(subscriber, id3, (Callback)(() => 
                {
                    Assert.Fail();
                }));
            }));
            NotificationHub.Default.Subscribe(subscriber, id2, (Callback)(() => 
            {
                count++;
            }));
            IEnumerable<string> idsForSubscriber = NotificationHub.Default.GetIdsFor(subscriber);
            Assert.AreEqual(2, idsForSubscriber.Count());

            NotificationHub.Default.Notify(publisher, id1);
            NotificationHub.Default.Notify(publisher, id2);

            Assert.AreEqual(2, count);
            idsForSubscriber = NotificationHub.Default.GetIdsFor(subscriber);
            Assert.AreEqual(3, idsForSubscriber.Count());
        }

        [Test]
        public void GivenASubscriptionsAndASubscriptionInAnotherSubscription_WhenNotifyingTwoTimes_ThenThreeCallbackAndTwoNewSubscriptions()
        {
            object subscriber = this;
            object publisher = new FooClass();
            ID id1 = fixture.Create<string>();
            ID id2 = fixture.Create<string>();
            int count = 0;
            NotificationHub.Default.Subscribe(subscriber, id1, (Callback)(() =>
            {
                count++;
                NotificationHub.Default.Subscribe(subscriber, id2, (Callback)(() =>
                {
                    count++;
                }));
            }));
            IEnumerable<string> idsForSubscriber = NotificationHub.Default.GetIdsFor(subscriber);
            Assert.AreEqual(1, idsForSubscriber.Count());

            NotificationHub.Default.Notify(publisher, id1);
            Assert.AreEqual(1, count);
            NotificationHub.Default.Notify(publisher, id2);
            Assert.AreEqual(2, count);
            idsForSubscriber = NotificationHub.Default.GetIdsFor(subscriber);
            Assert.AreEqual(2, idsForSubscriber.Count());
        }

        [Test]
        public void GivenASubscriptions_WhenNotifyingFromASpecificThread_ThenTheCallbackIsCalledFromThatThread()
        {
            object subscriber = this;
            object publisher = new FooClass();
            ID id = fixture.Create<string>();
            int mainThreadID = Thread.CurrentThread.ManagedThreadId;
            int newThreadId = 0;
            int callbackThreadId = 0;
            NotificationHub.Default.Subscribe(subscriber, id, (Callback)(() => {
                callbackThreadId = Thread.CurrentThread.ManagedThreadId;
            }));

            Thread thread = new Thread(() =>
            {
                newThreadId = Thread.CurrentThread.ManagedThreadId;
                NotificationHub.Default.Notify(subscriber, id);
            });

            thread.Start();
            thread.Join();

            Assert.AreNotEqual(mainThreadID, callbackThreadId);
            Assert.AreEqual(newThreadId, callbackThreadId);
        }
    }
}
