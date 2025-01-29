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

using System;
using System.Collections.Generic;
using System.Linq;
using inetum.unityUtils;
using Moq;
using NUnit.Framework;
using umi3d.edk;
using UnityEngine;

/// <summary>
/// Test GetSetEntityOperationForAll
/// Missing cases but ¯\_(ツ)_/¯
/// </summary>
namespace EditMode_Tests.Core.Properties
{
    public class UMI3DAsyncProperty_Test
    {
        public class Equality
        {
            UMI3DAsyncPropertyEquality equality;

            [SetUp]
            public void Setup()
            {
                equality = new();
            }

            [Test]
            public void Given2V3_WhenVector3Equality_ThenFalse()
            {
                Vector3 a = Vector3.zero;
                Assert.IsFalse(equality.Vector3Equality(a, new Vector3(1, 0, 0)));
                Assert.IsFalse(equality.Vector3Equality(a, new Vector3(0, 1, 0)));
                Assert.IsFalse(equality.Vector3Equality(a, new Vector3(0, 0, 1)));
            }


            [Test]
            public void Given1V3_WhenVector3Equality_ThenTrue()
            {
                Vector3 a = Vector3.zero;
                Vector3 b = Vector3.zero;
                Assert.IsTrue(equality.Vector3Equality(a, b));
            }

            [Test]
            public void Given2V2_WhenVector2Equality_ThenFalse()
            {
                Vector2 a = Vector2.zero;
                Assert.IsFalse(equality.Vector2Equality(a, new Vector2(1, 0)));
                Assert.IsFalse(equality.Vector2Equality(a, new Vector2(0, 1)));
            }


            [Test]
            public void Given1V2_WhenVector2Equality_ThenTrue()
            {
                Vector2 a = Vector2.zero;
                Vector2 b = Vector2.zero;
                Assert.IsTrue(equality.Vector2Equality(a, b));
            }

            [Test]
            public void Given2V4_WhenVector4Equality_ThenFalse()
            {
                Vector4 a = Vector4.zero;
                Assert.IsFalse(equality.Vector4Equality(a, new Vector4(1, 0, 0, 0)));
                Assert.IsFalse(equality.Vector4Equality(a, new Vector4(0, 1, 0, 0)));
                Assert.IsFalse(equality.Vector4Equality(a, new Vector4(0, 0, 1, 0)));
                Assert.IsFalse(equality.Vector4Equality(a, new Vector4(0, 0, 0, 1)));
            }


            [Test]
            public void Given1V4_WhenVector4Equality_ThenTrue()
            {
                Vector4 a = Vector4.zero;
                Vector4 b = Vector4.zero;
                Assert.IsTrue(equality.Vector4Equality(a, b));
            }

            [Test]
            public void Given2Color_WhenColorEquality_ThenFalse()
            {
                Color a = Color.black;
                Assert.IsFalse(equality.ColorEquality(a, Color.red));
                Assert.IsFalse(equality.ColorEquality(a, Color.blue));
                Assert.IsFalse(equality.ColorEquality(a, Color.green));
                Assert.IsFalse(equality.ColorEquality(a, new Color(0,0,0,0)));
            }


            [Test]
            public void Given1Color_WhenVector4Equality_ThenTrue()
            {
                Color a = Color.black;
                Color b = Color.black;
                Assert.IsTrue(equality.ColorEquality(a, b));
            }


            [Test]
            public void Given2Float_WhenFloatEquality_ThenFalse()
            {
                float a = 1f;
                Assert.IsFalse(equality.FloatEquality(a, 0f));
                Assert.IsFalse(equality.FloatEquality(a, 1.0001f));
            }


            [Test]
            public void Given1Float_WhenFloatEquality_ThenTrue()
            {
                float a = 1f;
                float b = 1f;
                Assert.IsTrue(equality.FloatEquality(a, b));
            }

            [Test]
            public void Given2Quaternion_WhenQuaternionEquality_ThenFalse()
            {
                Quaternion a = new();
                Quaternion b = Quaternion.Euler(35,0,0);
                Assert.IsFalse(equality.QuaternionEquality(a, b));
            }

            [Test]
            public void Given1Quaternion_WhenQuaternionEquality_ThenTrue()
            {
                Quaternion a = Quaternion.identity;
                Quaternion b = Quaternion.Euler(a.eulerAngles);

                Assert.IsTrue(equality.QuaternionEquality(a, b));
            }
        }

        public class SetGetValue
        {
            const int casesPerList = 2;
            protected Mock<IUMI3DServer> serverServiceMock;

            protected float newFPSTracking = 18f;
            private List<UMI3DUser> users;

            ulong entity = 30uL;
            uint property = 40;
            int value = 42;
            UMI3DAsyncProperty<int> asyncP;

            Dictionary<UMI3DUser, int> userExpectedValueMap;
            Dictionary<UMI3DUser, UMI3DGroupAsyncProperty> userGroupMap;
            Dictionary<UMI3DGroupAsyncProperty, int> groupExpectedValueMap;

            UMI3DGroupAsyncProperty group;

            HashSet<UMI3DUser> UserGroupSync;
            HashSet<UMI3DUser> UserGroupAsync;
            HashSet<UMI3DUser> UserNoGroupSync;
            HashSet<UMI3DUser> UserNoGroupAsync;
            HashSet<UMI3DUser> UserGroupSyncDesync;
            HashSet<UMI3DUser> UserGroupAsyncDesync;
            HashSet<UMI3DUser> UserNoGroupSyncDesync;
            HashSet<UMI3DUser> UserNoGroupAsyncDesync;

            List<HashSet<UMI3DUser>> userListList;

            //Init Mock Umi3dServer and users
            [SetUp]
            public void Setup()
            {
                UserGroupSync = new();
                UserGroupAsync = new();
                UserNoGroupSync = new();
                UserNoGroupAsync = new();
                UserGroupSyncDesync = new();
                UserGroupAsyncDesync = new();
                UserNoGroupSyncDesync = new();
                UserNoGroupAsyncDesync = new();

                userListList = new()
        {
            UserGroupSync,
            UserGroupAsync,
            UserNoGroupSync,
            UserNoGroupAsync,
            UserGroupSyncDesync,
            UserGroupAsyncDesync,
            UserNoGroupSyncDesync,
            UserNoGroupAsyncDesync,
        };


                serverServiceMock = new();

                var usersMock = new List<Mock<UMI3DUser>>();

                int cases = userListList.Count;

                for (int ui = 0; ui < cases * casesPerList; ui++)
                    usersMock.Add(new Mock<UMI3DUser>());

                ulong i = 20000uL;
                foreach (var userMock in usersMock)
                {
                    userMock.Setup(x => x.Id()).Returns(i++);
                }
                users = usersMock.Select(x => x.Object).ToList();
                serverServiceMock.Setup(x => x.Users()).Returns(users);

                serverServiceMock.Setup(x => x.OnUserLeave).Returns(new UMI3DUserEvent());

                entity = 30uL;
                property = 40;
                value = 42;
                asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);
            }

            /// <summary>
            /// Fill property with a combination of user sync async in group not in group
            /// </summary>
            public void FillGroupAndProperty()
            {
                int i = 0;
                userListList.Do(l => l = new()).ForEach(l =>
                {
                    for (int j = 0; j < casesPerList; j++)
                        l.Add(users[i++]);
                });

                userExpectedValueMap = new();
                userGroupMap = new();
                groupExpectedValueMap = new();

                group = new();

                //group in property
                groupExpectedValueMap.Add(group, 123456789);

                #region initUserMap
                //users sync not in group 
                //NOTHING TO DO

                //users sync in group
                foreach (var user in UserGroupSync)
                {
                    userGroupMap.Add(user, group);
                }

                //users async not in group
                foreach (var user in UserNoGroupAsync)
                    userExpectedValueMap.Add(user, 8544555);

                //users async in group
                foreach (var user in UserGroupAsync)
                {
                    userExpectedValueMap.Add(user, 8544555);
                    userGroupMap.Add(user, group);
                }

                //users sync not in group desync
                foreach (var user in UserGroupSyncDesync)
                {
                    asyncP.DeSync(user, true);
                }

                //users sync in group desync
                foreach (var user in UserGroupSyncDesync)
                {
                    userGroupMap.Add(user, group);
                    asyncP.DeSync(user, true);
                }

                //users async not in group desync
                foreach (var user in UserNoGroupAsyncDesync)
                {
                    userExpectedValueMap.Add(user, 8544555);
                    asyncP.DeSync(user, true);
                }

                //users async in group desync
                foreach (var user in UserGroupAsyncDesync)
                {
                    userExpectedValueMap.Add(user, 8544555);
                    userGroupMap.Add(user, group);
                    asyncP.DeSync(user, true);
                }
                #endregion

                //Set user async
                foreach (var item in userExpectedValueMap)
                    asyncP.SetValue(item.Key, item.Value);

                //Set user in group
                foreach (var item in userGroupMap)
                    (item.Value).Add(item.Key);

                //Set group value
                foreach (var item in groupExpectedValueMap)
                    asyncP.SetValue(item.Value, item.Key);
            }

            #region SetGetValue
            /// <summary>
            /// Test that getValue() return given default value
            /// Test that getvalue(user) return given default value
            /// Test that isAsync is false
            /// Test that isDeSync is false
            /// </summary>
            [Test]
            public void GivenNothing_WhenNothing_ThenDefaultValue()
            {
                Assert.AreEqual(value, asyncP.GetValue());

                foreach (var u in users)
                    Assert.AreEqual(value, asyncP.GetValue(u));

                Assert.AreEqual(false, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }

            /// <summary>
            /// Test that SetValue update default value
            /// Test that GetValue() return new default value
            /// Test that GetValue(user) return new default value
            /// Test that isAsync is false
            /// Test that isDeSync is false
            /// </summary>
            [Test]
            public void GivenNothing_WhenSetValue_ThenNewValue()
            {
                int newValue = 66;

                asyncP.SetValue(66);

                Assert.AreEqual(newValue, asyncP.GetValue());
                foreach (var u in users)
                    Assert.AreEqual(newValue, asyncP.GetValue(u));
                Assert.AreEqual(false, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }

            /// <summary>
            /// Test that SetValue(user) only set value for the specified user
            /// </summary>
            [Test]
            public void GivenNothing_WhenSetValueUser_ThenDefaultValueUserNewValue()
            {
                Dictionary<UMI3DUser, int> map = new();
                map.Add(users[1], 91120);
                foreach (var item in map)
                    asyncP.SetValue(item.Key, item.Value);

                Assert.AreEqual(value, asyncP.GetValue());

                foreach (var u in users)
                    Assert.AreEqual(map.TryGetValue(u, out int v) ? v : value, asyncP.GetValue(u));

                Assert.AreEqual(true, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }

            [Test]
            public void GivenUserAsync_WhenSetValueUser_ThenDefaultValueUserNewValue()
            {
                Dictionary<UMI3DUser, int> map = new();
                map.Add(users[1], 91120);
                foreach (var item in map)
                    asyncP.SetValue(item.Key, item.Value);

                map[users[1]] =  91300;
                foreach (var item in map)
                    asyncP.SetValue(item.Key, item.Value);

                Assert.AreEqual(value, asyncP.GetValue());

                foreach (var u in users)
                    Assert.AreEqual(map.TryGetValue(u, out int v) ? v : value, asyncP.GetValue(u));

                Assert.AreEqual(true, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }

            [Test]
            public void GivenUserAsync_WhenSetSameValueUser_ThenDefaultValueUserNewValue()
            {
                Dictionary<UMI3DUser, int> map = new();
                map.Add(users[1], 91120);
                foreach (var item in map)
                    asyncP.SetValue(item.Key, item.Value);

                foreach (var item in map)
                    Assert.AreEqual(null, asyncP.SetValue(item.Key, item.Value));

                Assert.AreEqual(value, asyncP.GetValue());

                foreach (var u in users)
                    Assert.AreEqual(map.TryGetValue(u, out int v) ? v : value, asyncP.GetValue(u));

                Assert.AreEqual(true, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }


            /// <summary>
            /// Test that SetValue(user) only set value for the specified user
            /// </summary>
            [Test]
            public void GivenNothing_WhenSetValueGroup_ThenDefaultValueUserInGroupNewValue()
            {
                Dictionary<UMI3DGroupAsyncProperty, int> map2 = new();
                UMI3DGroupAsyncProperty group = new();
                map2.Add(group, 123456789);

                foreach (var item in map2)
                    asyncP.SetValue(item.Value, item.Key);

                foreach (var item in map2)
                    Assert.AreEqual(item.Value, asyncP.TEST_GroupValueMaps[item.Key]);

                Assert.AreEqual(value, asyncP.GetValue());

                Assert.AreEqual(false, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }


            /// <summary>
            /// Test that SetValue(user) only set value for the specified user
            /// </summary>
            [Test]
            public void GivenGroupContainingUsersAndProperty_WhenSetValueGroupWithUser_ThenUserInGroupValueIsGroupValue()
            {
                ulong entity = 30uL;
                uint property = 40;
                int value = 42;

                Dictionary<UMI3DUser, UMI3DGroupAsyncProperty> map = new();
                Dictionary<UMI3DGroupAsyncProperty, int> map2 = new();

                var asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);

                Assert.AreEqual(value, asyncP.GetValue());

                foreach (var u in users)
                    Assert.AreEqual(value, asyncP.GetValue(u));

                UMI3DGroupAsyncProperty group = new();

                map.Add(users[1], group);
                map2.Add(group, 123456789);

                foreach (var item in map)
                    (item.Value).Add(item.Key);

                foreach (var item in map2)
                    asyncP.SetValue(item.Value, item.Key);

                Assert.AreEqual(value, asyncP.GetValue());

                foreach (var u in users)
                    Assert.AreEqual(map.TryGetValue(u, out var g) && map2.TryGetValue(g, out int v) ? v : value, asyncP.GetValue(u));

                Assert.AreEqual(false, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }

            /// <summary>
            /// Test that SetValue(user) only set value for the specified user
            /// </summary>
            [Test]
            public void UMI3DAsyncProperty_SetValueGroupWithAsyncUser()
            {
                ulong entity = 30uL;
                uint property = 40;
                int value = 42;

                Dictionary<UMI3DUser, int> map3 = new();
                Dictionary<UMI3DUser, UMI3DGroupAsyncProperty> map = new();
                Dictionary<UMI3DGroupAsyncProperty, int> map2 = new();

                var asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);

                UMI3DGroupAsyncProperty group = new();

                map3.Add(users[2], 8544555);
                map.Add(users[1], group);
                map3.Add(users[0], 833354555);
                map.Add(users[0], group);
                map2.Add(group, 123456789);

                foreach (var item in map3)
                    asyncP.SetValue(item.Key, item.Value);

                foreach (var item in map)
                    (item.Value).Add(item.Key);

                foreach (var item in map2)
                    asyncP.SetValue(item.Value, item.Key);

                Assert.AreEqual(value, asyncP.GetValue());

                foreach (var u in users)
                    Assert.AreEqual(
                        map3.TryGetValue(u, out var uv)
                            ? uv
                            : map.TryGetValue(u, out var g) && map2.TryGetValue(g, out int v)
                                ? v
                                : value, asyncP.GetValue(u));

                Assert.AreEqual(true, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }
            #endregion SetGetValue
        }

        public class Sync
        {
            const int casesPerList = 2;
            protected Mock<IUMI3DServer> serverServiceMock;

            protected float newFPSTracking = 18f;
            private List<UMI3DUser> users;

            ulong entity = 30uL;
            uint property = 40;
            int value = 42;
            UMI3DAsyncProperty<int> asyncP;

            Dictionary<UMI3DUser, int> userExpectedValueMap;
            Dictionary<UMI3DUser, UMI3DGroupAsyncProperty> userGroupMap;
            Dictionary<UMI3DGroupAsyncProperty, int> groupExpectedValueMap;

            UMI3DGroupAsyncProperty group;

            HashSet<UMI3DUser> UserGroupSync;
            HashSet<UMI3DUser> UserGroupAsync;
            HashSet<UMI3DUser> UserNoGroupSync;
            HashSet<UMI3DUser> UserNoGroupAsync;
            HashSet<UMI3DUser> UserGroupSyncDesync;
            HashSet<UMI3DUser> UserGroupAsyncDesync;
            HashSet<UMI3DUser> UserNoGroupSyncDesync;
            HashSet<UMI3DUser> UserNoGroupAsyncDesync;

            List<HashSet<UMI3DUser>> userListList;

            //Init Mock Umi3dServer and users
            [SetUp]
            public void Setup()
            {
                UserGroupSync = new();
                UserGroupAsync = new();
                UserNoGroupSync = new();
                UserNoGroupAsync = new();
                UserGroupSyncDesync = new();
                UserGroupAsyncDesync = new();
                UserNoGroupSyncDesync = new();
                UserNoGroupAsyncDesync = new();

                userListList = new()
        {
            UserGroupSync,
            UserGroupAsync,
            UserNoGroupSync,
            UserNoGroupAsync,
            UserGroupSyncDesync,
            UserGroupAsyncDesync,
            UserNoGroupSyncDesync,
            UserNoGroupAsyncDesync,
        };


                serverServiceMock = new();

                var usersMock = new List<Mock<UMI3DUser>>();

                int cases = userListList.Count;

                for (int ui = 0; ui < cases * casesPerList; ui++)
                    usersMock.Add(new Mock<UMI3DUser>());

                ulong i = 20000uL;
                foreach (var userMock in usersMock)
                {
                    userMock.Setup(x => x.Id()).Returns(i++);
                }
                users = usersMock.Select(x => x.Object).ToList();
                serverServiceMock.Setup(x => x.Users()).Returns(users);

                serverServiceMock.Setup(x => x.OnUserLeave).Returns(new UMI3DUserEvent());

                entity = 30uL;
                property = 40;
                value = 42;
                asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);
            }

            /// <summary>
            /// Fill property with a combination of user sync async in group not in group
            /// </summary>
            public void FillGroupAndProperty()
            {
                int i = 0;
                userListList.Do(l => l = new()).ForEach(l =>
                {
                    for (int j = 0; j < casesPerList; j++)
                        l.Add(users[i++]);
                });

                userExpectedValueMap = new();
                userGroupMap = new();
                groupExpectedValueMap = new();

                group = new();

                //group in property
                groupExpectedValueMap.Add(group, 123456789);

                #region initUserMap
                //users sync not in group 
                //NOTHING TO DO

                //users sync in group
                foreach (var user in UserGroupSync)
                {
                    userGroupMap.Add(user, group);
                }

                //users async not in group
                foreach (var user in UserNoGroupAsync)
                    userExpectedValueMap.Add(user, 8544555);

                //users async in group
                foreach (var user in UserGroupAsync)
                {
                    userExpectedValueMap.Add(user, 8544555);
                    userGroupMap.Add(user, group);
                }

                //users sync not in group desync
                foreach (var user in UserGroupSyncDesync)
                {
                    asyncP.DeSync(user, true);
                }

                //users sync in group desync
                foreach (var user in UserGroupSyncDesync)
                {
                    userGroupMap.Add(user, group);
                    asyncP.DeSync(user, true);
                }

                //users async not in group desync
                foreach (var user in UserNoGroupAsyncDesync)
                {
                    userExpectedValueMap.Add(user, 8544555);
                    asyncP.DeSync(user, true);
                }

                //users async in group desync
                foreach (var user in UserGroupAsyncDesync)
                {
                    userExpectedValueMap.Add(user, 8544555);
                    userGroupMap.Add(user, group);
                    asyncP.DeSync(user, true);
                }
                #endregion

                //Set user async
                foreach (var item in userExpectedValueMap)
                    asyncP.SetValue(item.Key, item.Value);

                //Set user in group
                foreach (var item in userGroupMap)
                    (item.Value).Add(item.Key);

                //Set group value
                foreach (var item in groupExpectedValueMap)
                    asyncP.SetValue(item.Value, item.Key);
            }


            #region Sync
            /// <summary>
            /// Sync() sync only user not in a group
            /// </summary>
            [Test]
            public void GivenFill_WhenSync_ThenOnlyNoGroupSync()
            {
                //GIVEN
                FillGroupAndProperty();

                //WHEN
                var operation = asyncP.Sync();

                Assert.AreEqual(value, asyncP.GetValue());

                foreach (var u in users)
                    Assert.AreEqual(
                        userExpectedValueMap.TryGetValue(u, out var uv)
                            ? userGroupMap.TryGetValue(u, out var g1) && groupExpectedValueMap.TryGetValue(g1, out int _)
                                ? uv // async in group not sync
                                : value // async not in group have default value
                            : userGroupMap.TryGetValue(u, out var g) && groupExpectedValueMap.TryGetValue(g, out int v)
                                ? v
                                : value, asyncP.GetValue(u));

                Assert.AreEqual(true, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }


            /// <summary>
            /// SyncAll() sync all user to their group default value or default value if not in a group
            /// </summary>
            [Test]
            public void UMI3DAsyncProperty_SyncAll()
            {
                ulong entity = 30uL;
                uint property = 40;
                int value = 42;

                Dictionary<UMI3DUser, int> map3 = new();
                Dictionary<UMI3DUser, UMI3DGroupAsyncProperty> map = new();
                Dictionary<UMI3DGroupAsyncProperty, int> map2 = new();

                var asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);

                UMI3DGroupAsyncProperty group = new();

                map3.Add(users[2], 8544555);
                map.Add(users[1], group);
                map3.Add(users[0], 833354555);
                map.Add(users[0], group);
                map2.Add(group, 123456789);

                foreach (var item in map3)
                    asyncP.SetValue(item.Key, item.Value);

                foreach (var item in map)
                    (item.Value).Add(item.Key);

                foreach (var item in map2)
                    asyncP.SetValue(item.Value, item.Key);

                asyncP.SyncAll();

                Assert.AreEqual(value, asyncP.GetValue());

                foreach (var u in users)
                    Assert.AreEqual(
                        map.TryGetValue(u, out var g) && map2.TryGetValue(g, out int v)
                                ? v
                                : value, asyncP.GetValue(u));

                Assert.AreEqual(false, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }

            /// <summary>
            /// Sync(Group) sync all user in a group to their default value
            /// </summary>
            [Test]
            public void UMI3DAsyncProperty_SyncGroup()
            {
                ulong entity = 30uL;
                uint property = 40;
                int value = 42;

                Dictionary<UMI3DUser, int> map3 = new();
                Dictionary<UMI3DUser, UMI3DGroupAsyncProperty> map = new();
                Dictionary<UMI3DGroupAsyncProperty, int> map2 = new();

                var asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);

                UMI3DGroupAsyncProperty group = new();

                map3.Add(users[2], 8544555);
                map.Add(users[1], group);
                map3.Add(users[0], 833354555);
                map.Add(users[0], group);
                map2.Add(group, 123456789);

                foreach (var item in map3)
                    asyncP.SetValue(item.Key, item.Value);

                foreach (var item in map)
                    (item.Value).Add(item.Key);

                foreach (var item in map2)
                    asyncP.SetValue(item.Value, item.Key);

                foreach (var item in map2)
                    asyncP.Sync(item.Key);

                Assert.AreEqual(value, asyncP.GetValue());

                foreach (var u in users)
                    Assert.AreEqual(
                        map3.TryGetValue(u, out var uv)
                            ? map.TryGetValue(u, out var g1) && map2.TryGetValue(g1, out int v1)
                                ? v1 // async in group have default group value
                                : uv // async not in group not sync
                            : map.TryGetValue(u, out var g) && map2.TryGetValue(g, out int v)
                                ? v
                                : value, asyncP.GetValue(u));

                Assert.AreEqual(true, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }

            /// <summary>
            /// Sync(User) sync a user in a group to their group default value or default value if not in a group
            /// </summary>
            [Test]
            public void GivenFill_WhenSyncUser_ThenUserOnDefaultValueOrGroupDefaultValue()
            {
                //GIVEN
                FillGroupAndProperty();

                //When
                List<UMI3DUser> usersToSync = new() {
            UserGroupAsync.First(),
            UserNoGroupAsync.First(),
            UserGroupAsyncDesync.First(),
            UserNoGroupAsyncDesync.First()
        };

                foreach (var user in usersToSync)
                {
                    asyncP.Sync(user, true);
                    userExpectedValueMap.Remove(user);
                }

                //THEN
                Assert.AreEqual(value, asyncP.GetValue());

                foreach (var u in users)
                    Assert.AreEqual(
                        userExpectedValueMap.TryGetValue(u, out var uv)
                            ? uv
                            : userGroupMap.TryGetValue(u, out var g) && groupExpectedValueMap.TryGetValue(g, out int v)
                                ? v
                                : value, asyncP.GetValue(u));

                Assert.AreEqual(true, asyncP.isAsync);
                Assert.AreEqual(false, asyncP.isDeSync);
            }
            #endregion Sync
        }

        public class UMI3DGroupAsyncPropertyTest
        {
            protected Mock<IUMI3DServer> serverServiceMock;

            protected float newFPSTracking = 18f;
            private List<UMI3DUser> users;

            //Init Mock Umi3dServer and users
            [SetUp]
            public void Setup()
            {
                serverServiceMock = new();

                var usersMock = new List<Mock<UMI3DUser>>()
            {
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
            };
                ulong i = 20000uL;
                foreach (var userMock in usersMock)
                {
                    userMock.Setup(x => x.Id()).Returns(i++);
                }
                users = usersMock.Select(x => x.Object).ToList();
                serverServiceMock.Setup(x => x.Users()).Returns(users);

                serverServiceMock.Setup(x => x.OnUserLeave).Returns(new UMI3DUserEvent());
            }


            [Test]
            public void GivenGroup_WhenAddUsersNull_ThenNull()
            {
                //GIVEN
                UMI3DGroupAsyncProperty group = new();
                Assert.AreEqual(0, group.Count());

                //WHEN
                List<SetEntityProperty> result = group.Add((UMI3DUser)null);

                //THEN
                Assert.AreEqual(null, result);
            }

            [Test]
            public void GivenGroup_WhenAddPropertyNull_ThenNothing()
            {
                //GIVEN
                UMI3DGroupAsyncProperty group = new();
                Assert.AreEqual(0, group.Count());

                //WHEN
                group.Add((UMI3DAsyncProperty)null);

            }

            [Test]
            public void GivenGroup_WhenRemoveUsersNull_ThenNull()
            {
                //GIVEN
                UMI3DGroupAsyncProperty group = new();
                Assert.AreEqual(0, group.Count());

                //WHEN
                List<SetEntityProperty> result = group.Remove((UMI3DUser)null);

                //THEN
                Assert.AreEqual(null, result);
            }

            [Test]
            public void GivenGroup_WhenRemovePropertyNull_ThenNull()
            {
                //GIVEN
                UMI3DGroupAsyncProperty group = new();
                Assert.AreEqual(0, group.Count());

                //WHEN
                var result = group.Remove((UMI3DAsyncProperty)null);

                //THEN
                Assert.AreEqual(null, result);
            }



            [Test]
            public void GivenGroup_WhenAddUsers_ThenUsersInGroup()
            {
                //GIVEN
                UMI3DGroupAsyncProperty group = new();
                Assert.AreEqual(0, group.Count());

                //WHEN
                foreach (var u in users)
                    group.Add(u);

                //THEN
                Assert.AreEqual(users.Count, group.Count());
                foreach (var u in users)
                    Assert.IsTrue(group.Contains(u));
            }

            [Test]
            public void GivenGroupContainingUsers_WhenAddUsers_ThenUsersInGroupOnlyOnce()
            {
                //GIVEN
                UMI3DGroupAsyncProperty group = new();
                foreach (var u in users)
                    group.Add(u);


                //WHEN
                foreach (var u in users)
                    group.Add(u);

                //THEN
                Assert.AreEqual(users.Count, group.Count());
                foreach (var u in users)
                    Assert.IsTrue(group.Contains(u));
            }

            [Test]
            public void GivenGroupContainingUsers_WhenRemoveUsers_ThenUsersNotInGroup()
            {
                //GIVEN
                UMI3DGroupAsyncProperty group = new();
                foreach (var u in users)
                    group.Add(u);

                //WHEN
                foreach (var u in users)
                    group.Remove(u);

                //THEN
                Assert.AreEqual(0, group.Count());
            }

            [Test]
            public void GivenGroup_WhenRemoveUsers_ThenNothing()
            {
                //GIVEN
                UMI3DGroupAsyncProperty group = new();

                //WHEN
                foreach (var u in users)
                    group.Remove(u);

                //THEN
                Assert.AreEqual(0, group.Count());
            }

            [Test]
            public void GivenPropertyAndGroup_WhenAddProperty_ThenPropertyInGroup()
            {
                //GIVEN
                ulong entity = 30uL;
                uint property = 40;
                int value = 42;
                var asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);

                UMI3DGroupAsyncProperty group = new();
                Assert.AreEqual(0, group.properties.Count());

                //WHEN
                group.Add(asyncP);

                //THEN
                Assert.IsTrue(asyncP.TEST_GroupValueMaps.ContainsKey(group));
                Assert.AreEqual(1, group.properties.Count());
            }

            [Test]
            public void GivenGroupContainingProperty_WhenRemoveProperty_ThenPropertyNotInGroup()
            {
                //GIVEN
                ulong entity = 30uL;
                uint property = 40;
                int value = 42;
                var asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);

                UMI3DGroupAsyncProperty group = new();

                group.Add(asyncP);

                Assert.AreEqual(1, group.properties.Count());

                //WHEN
                group.Remove(asyncP);

                //THEN
                Assert.AreEqual(0, group.properties.Count());
                Assert.IsFalse(asyncP.TEST_GroupValueMaps.ContainsKey(group));
            }

            [Test]
            public void GivenGroup_WhenRemoveProperty_ThenPropertyNotInGroup()
            {
                //GIVEN
                ulong entity = 30uL;
                uint property = 40;
                int value = 42;
                var asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);

                UMI3DGroupAsyncProperty group = new();

                //WHEN
                var result = group.Remove(asyncP);

                //THEN
                Assert.AreEqual(0, group.properties.Count());
                Assert.IsFalse(asyncP.TEST_GroupValueMaps.ContainsKey(group));
                Assert.AreEqual(null, result);
            }


            [Test]
            public void GivenGroupContainingProperty_WhenAddUser_ThenUserInPropertyGroupMap()
            {
                //GIVEN
                ulong entity = 30uL;
                uint property = 40;
                int value = 42;
                var asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);
                UMI3DGroupAsyncProperty group = new();
                group.Add(asyncP);

                //WHEN
                group.Add(users[0]);

                //THEN
                Assert.IsTrue(asyncP.TEST_GroupValueMaps.ContainsKey(group));
                Assert.IsTrue(asyncP.TEST_UserGroupMaps.ContainsKey(users[0]));
                Assert.AreEqual(group, asyncP.TEST_UserGroupMaps[users[0]]);
            }

            [Test]
            public void GivenGroupContainingUser_WhenAddProperty_ThenUserInPropertyGroupMap()
            {
                //GIVEN
                ulong entity = 30uL;
                uint property = 40;
                int value = 42;
                var asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);
                UMI3DGroupAsyncProperty group = new();
                group.Add(users[0]);

                //WHEN
                group.Add(asyncP);

                //THEN
                Assert.IsTrue(asyncP.TEST_GroupValueMaps.ContainsKey(group));
                Assert.IsTrue(asyncP.TEST_UserGroupMaps.ContainsKey(users[0]));
                Assert.AreEqual(group, asyncP.TEST_UserGroupMaps[users[0]]);
            }

            [Test]
            public void GivenGroupContainingUserAndContainingProperty_WhenRemoveUser_ThenUserNotInPropertyGroupMap()
            {
                //GIVEN
                ulong entity = 30uL;
                uint property = 40;
                int value = 42;
                var asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);
                UMI3DGroupAsyncProperty group = new();
                group.Add(users[0]);
                group.Add(asyncP);

                //WHEN
                group.Remove(users[0]);

                //THEN
                Assert.IsTrue(asyncP.TEST_GroupValueMaps.ContainsKey(group));
                Assert.IsFalse(asyncP.TEST_UserGroupMaps.ContainsKey(users[0]));
            }


            [Test]
            public void GivenGroupContainingUserAndContainingProperty_WhenRemoveProperty_ThenUserNotInPropertyGroupMap()
            {
                //GIVEN
                ulong entity = 30uL;
                uint property = 40;
                int value = 42;
                var asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);
                UMI3DGroupAsyncProperty group = new();
                group.Add(users[0]);
                group.Add(asyncP);

                //WHEN
                group.Remove(asyncP);

                //THEN
                Assert.IsFalse(asyncP.TEST_GroupValueMaps.ContainsKey(group));
                Assert.IsFalse(asyncP.TEST_UserGroupMaps.ContainsKey(users[0]));
            }
        }

        public class GetSetEntityOperationFor
        {
            protected Mock<IUMI3DServer> serverServiceMock;

            protected float newFPSTracking = 18f;
            private List<UMI3DUser> users;

            ulong entity = 30uL;
            uint property = 40;
            int value = 42;
            UMI3DAsyncProperty<int> asyncP;

            Dictionary<UMI3DUser, int> userExpectedValueMap = new();
            Dictionary<UMI3DUser, UMI3DGroupAsyncProperty> userGroupMap = new();
            Dictionary<UMI3DGroupAsyncProperty, int> groupExpectedValueMap = new();

            UMI3DGroupAsyncProperty group = new();

            //Init Mock Umi3dServer and users
            [SetUp]
            public void Setup()
            {
                serverServiceMock = new();

                var usersMock = new List<Mock<UMI3DUser>>()
            {
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
                new Mock<UMI3DUser>(),
            };
                ulong i = 20000uL;
                foreach (var userMock in usersMock)
                {
                    userMock.Setup(x => x.Id()).Returns(i++);
                }
                users = usersMock.Select(x => x.Object).ToList();
                serverServiceMock.Setup(x => x.Users()).Returns(users);

                serverServiceMock.Setup(x => x.OnUserLeave).Returns(new UMI3DUserEvent());

                entity = 30uL;
                property = 40;
                value = 42;
                asyncP = new UMI3DAsyncProperty<int>(serverServiceMock.Object, entity, property, value);
            }

            /// <summary>
            /// Fill property with a combination of user sync async in group not in group
            /// </summary>
            public void FillGroupAndProperty()
            {
                userExpectedValueMap = new();
                userGroupMap = new();
                groupExpectedValueMap = new();

                group = new();

                //group in property
                groupExpectedValueMap.Add(group, 123456789);

                //users async not in group
                userExpectedValueMap.Add(users[1], 8544555);

                //users async in group
                userExpectedValueMap.Add(users[2], 8544555);
                userGroupMap.Add(users[2], group);

                //users sync in group
                userGroupMap.Add(users[3], group);

                foreach (var item in userExpectedValueMap)
                    asyncP.SetValue(item.Key, item.Value);

                foreach (var item in userGroupMap)
                    (item.Value).Add(item.Key);

                foreach (var item in groupExpectedValueMap)
                    asyncP.SetValue(item.Value, item.Key);
            }

            #region SetEntity

            [Test]
            public void GivenNothing_WhenGetSetEntityOperationForAllUsers_ThenSetEntityForAllUser()
            {
                //GIVEN

                //WHEN
                var setEntity = asyncP.GetSetEntityOperationForAllUsers();

                //THEN
                Assert.AreEqual(property, setEntity.property);
                Assert.AreEqual(entity, setEntity.entityId);
                Assert.AreEqual(value, setEntity.value);
                Assert.AreEqual(users.Count, setEntity.users.Count);
                foreach (var user in users)
                    Assert.IsTrue(setEntity.users.Contains(user));
            }

            [Test]
            public void GivenSomeUserDeSync_WhenGetSetEntityOperationForAllUsers_ThenSetEntityForAllUserSync()
            {
                ///GIVEN
                HashSet<UMI3DUser> desyncUsers = new() { users[1], users[3] };
                foreach (var user in desyncUsers)
                    asyncP.DeSync(user, false);

                //WHEN
                var setEntity = asyncP.GetSetEntityOperationForAllUsers();

                //THEN
                Assert.AreEqual(property, setEntity.property);
                Assert.AreEqual(entity, setEntity.entityId);
                Assert.AreEqual(value, setEntity.value);
                Assert.AreEqual(users.Where(u => !desyncUsers.Contains(u)).Count(), setEntity.users.Count);
                foreach (var user in users.Where(u => !desyncUsers.Contains(u)))
                    Assert.IsTrue(setEntity.users.Contains(user));
            }

            [Test]
            public void GivenUsersInGroupSomeUserDeSync_WhenGetSetEntityOperationForAllUsersGroup_ThenSetEntityForAllUserSync()
            {
                //GIVEN
                int groupValue = 15464886;

                UMI3DGroupAsyncProperty group = new();
                foreach (var user in users)
                    group.Add(user);

                asyncP.SetValue(groupValue, group);

                HashSet<UMI3DUser> desyncUsers = new() { users[1], users[3] };
                foreach (var user in desyncUsers)
                    asyncP.DeSync(user, false);

                //WHEN
                var setEntity = asyncP.GetSetEntityOperationForAllUsers(group);

                //THEN
                Assert.AreEqual(property, setEntity.property);
                Assert.AreEqual(entity, setEntity.entityId);
                Assert.AreEqual(groupValue, setEntity.value);
                Assert.AreEqual(users.Where(u => !desyncUsers.Contains(u)).Count(), setEntity.users.Count);
                foreach (var user in users.Where(u => !desyncUsers.Contains(u)))
                    Assert.IsTrue(setEntity.users.Contains(user));
            }

            [Test]
            public void GivenUsersSyncGroupAsyncGroupSyncAsync_WhenGetSetEntityOperationForAllUsersAndGroups_ThenResultGroupP1NoUserOverlapAndAllSyncUsers()
            {

                FillGroupAndProperty();

                //Get List<SetEntity> for user sync
                var setEntitiesAllUsers = asyncP.GetSetEntityOperationForAllUsersAndGroups();

                Assert.AreEqual(groupExpectedValueMap.Count + 1, setEntitiesAllUsers.Count);

                HashSet<UMI3DUser> foundUser = new();
                foreach (var e in setEntitiesAllUsers)
                {
                    Assert.AreEqual(property, e.property);
                    Assert.AreEqual(entity, e.entityId);
                    //It can be tested but we would need to found which setEntity belong to which group or default
                    //Assert.AreEqual(value, e.value); 
                    foreach (var u in e.users)
                        Assert.IsTrue(foundUser.Add(u));
                }

                Assert.AreEqual(users.Where(u => !userExpectedValueMap.ContainsKey(u)).Count(), foundUser.Count);

                foreach (var user in users)
                    Assert.IsTrue(
                        userExpectedValueMap.ContainsKey(user)
                        ? !foundUser.Contains(user)
                        : foundUser.Contains(user)
                    );
            }

            #endregion SetEntity
        }

        public class PropertyList
        {
            public class GetSet
            {
                const int casesPerList = 2;
                protected Mock<IUMI3DServer> serverServiceMock;

                protected float newFPSTracking = 18f;
                private List<UMI3DUser> users;

                ulong entity = 30uL;
                uint property = 40;
                List<int> value = new() { 42 };
                UMI3DAsyncListProperty<int> asyncP;

                Dictionary<UMI3DUser, List<int>> userExpectedValueMap;
                Dictionary<UMI3DUser, UMI3DGroupAsyncProperty> userGroupMap;
                Dictionary<UMI3DGroupAsyncProperty, List<int>> groupExpectedValueMap;

                UMI3DGroupAsyncProperty group;

                HashSet<UMI3DUser> UserGroupSync;
                HashSet<UMI3DUser> UserGroupAsync;
                HashSet<UMI3DUser> UserNoGroupSync;
                HashSet<UMI3DUser> UserNoGroupAsync;
                HashSet<UMI3DUser> UserGroupSyncDesync;
                HashSet<UMI3DUser> UserGroupAsyncDesync;
                HashSet<UMI3DUser> UserNoGroupSyncDesync;
                HashSet<UMI3DUser> UserNoGroupAsyncDesync;

                List<HashSet<UMI3DUser>> userListList;

                //Init Mock Umi3dServer and users
                [SetUp]
                public void Setup()
                {
                    UserGroupSync = new();
                    UserGroupAsync = new();
                    UserNoGroupSync = new();
                    UserNoGroupAsync = new();
                    UserGroupSyncDesync = new();
                    UserGroupAsyncDesync = new();
                    UserNoGroupSyncDesync = new();
                    UserNoGroupAsyncDesync = new();

                    userListList = new()
                {
                    UserGroupSync,
                    UserGroupAsync,
                    UserNoGroupSync,
                    UserNoGroupAsync,
                    UserGroupSyncDesync,
                    UserGroupAsyncDesync,
                    UserNoGroupSyncDesync,
                    UserNoGroupAsyncDesync,
                };


                    serverServiceMock = new();

                    var usersMock = new List<Mock<UMI3DUser>>();

                    int cases = userListList.Count;

                    for (int ui = 0; ui < cases * casesPerList; ui++)
                        usersMock.Add(new Mock<UMI3DUser>());

                    ulong i = 20000uL;
                    foreach (var userMock in usersMock)
                    {
                        userMock.Setup(x => x.Id()).Returns(i++);
                    }
                    users = usersMock.Select(x => x.Object).ToList();
                    serverServiceMock.Setup(x => x.Users()).Returns(users);

                    serverServiceMock.Setup(x => x.OnUserLeave).Returns(new UMI3DUserEvent());

                    entity = 30uL;
                    property = 40;
                    value = new() { 42 };
                    asyncP = new UMI3DAsyncListProperty<int>(serverServiceMock.Object, entity, property, value);
                }

                /// <summary>
                /// Fill property with a combination of user sync async in group not in group
                /// </summary>
                public void FillGroupAndProperty()
                {
                    int i = 0;
                    userListList.Do(l => l = new()).ForEach(l =>
                    {
                        for (int j = 0; j < casesPerList; j++)
                            l.Add(users[i++]);
                    });

                    userExpectedValueMap = new();
                    userGroupMap = new();
                    groupExpectedValueMap = new();

                    group = new();

                    //group in property
                    groupExpectedValueMap.Add(group, new List<int>() { 123456789 });

                    #region initUserMap
                    //users sync not in group 
                    //NOTHING TO DO

                    //users sync in group
                    foreach (var user in UserGroupSync)
                    {
                        userGroupMap.Add(user, group);
                    }

                    //users async not in group
                    foreach (var user in UserNoGroupAsync)
                        userExpectedValueMap.Add(user, new List<int>() { 8544555 });

                    //users async in group
                    foreach (var user in UserGroupAsync)
                    {
                        userExpectedValueMap.Add(user, new List<int>() { 8544555 });
                        userGroupMap.Add(user, group);
                    }

                    //users sync not in group desync
                    foreach (var user in UserGroupSyncDesync)
                    {
                        asyncP.DeSync(user, true);
                    }

                    //users sync in group desync
                    foreach (var user in UserGroupSyncDesync)
                    {
                        userGroupMap.Add(user, group);
                        asyncP.DeSync(user, true);
                    }

                    //users async not in group desync
                    foreach (var user in UserNoGroupAsyncDesync)
                    {
                        userExpectedValueMap.Add(user, new List<int>() { 8544555 });
                        asyncP.DeSync(user, true);
                    }

                    //users async in group desync
                    foreach (var user in UserGroupAsyncDesync)
                    {
                        userExpectedValueMap.Add(user, new List<int>() { 8544555 });
                        userGroupMap.Add(user, group);
                        asyncP.DeSync(user, true);
                    }
                    #endregion

                    //Set user async
                    foreach (var item in userExpectedValueMap)
                        asyncP.SetValue(item.Key, item.Value);

                    //Set user in group
                    foreach (var item in userGroupMap)
                        (item.Value).Add(item.Key);

                    //Set group value
                    foreach (var item in groupExpectedValueMap)
                        asyncP.SetValue(item.Value, item.Key);
                }

                [Test]
                public void GivenNothing_WhenGetValueIndex_ThenDefaultValueIndex()
                {
                    //GIVEN

                    //WHEN
                    int index = 0;
                    var result = asyncP.GetValue(index);

                    //Then
                    Assert.AreEqual(value[index], result);
                    foreach (var user in users)
                        Assert.AreEqual(value[index], asyncP.GetValue(index, user));

                }

                [Test]
                public void GivenNothing_WhenGetValueIndexOutOfRange_ThenThrow()
                {
                    //GIVEN

                    //WHEN
                    int index1 = value.Count + 10;
                    int index2 = -1;

                    List<TestDelegate> tests = new()
                {
                    () => asyncP.GetValue(index1),
                    () => asyncP.GetValue(index2),
                };

                    foreach (var user in users)
                    {
                        tests.Add(() => asyncP.GetValue(index1, user));
                        tests.Add(() => asyncP.GetValue(index2, user));
                    }

                    //Then
                    foreach (var test in tests)
                        Assert.Throws<ArgumentOutOfRangeException>(test);
                }

                [Test]
                public void GivenFill_WhenGetValueIndexOutOfRange_ThenThrow()
                {
                    //GIVEN
                    FillGroupAndProperty();

                    //WHEN
                    int index1 = value.Count + 10;
                    int index2 = -1;

                    List<TestDelegate> tests = new()
                {
                    () => asyncP.GetValue(index1),
                    () => asyncP.GetValue(index2),
                };

                    foreach (var user in users)
                    {
                        tests.Add(() => asyncP.GetValue(index1, user));
                        tests.Add(() => asyncP.GetValue(index2, user));
                    }

                    //Then
                    foreach (var test in tests)
                        Assert.Throws<ArgumentOutOfRangeException>(test);
                }

                [Test]
                public void GivenNothing_WhenSetValueIndex_ThenNewValue()
                {
                    //Given

                    //When
                    var newValue = 336;
                    int index = 0;
                    asyncP.SetValue(index, newValue);

                    //Then
                    Assert.AreEqual(newValue, asyncP.GetValue(index));
                    foreach (var user in users)
                        Assert.AreEqual(newValue, asyncP.GetValue(index, user));

                }

                [Test]
                public void GivenNothing_WhenSetValueIndexUser_ThenNewValueUser()
                {
                    //Given

                    //When
                    var newValue = 336;
                    int index = 0;
                    var testUser = users.First();

                    asyncP.SetValue(testUser, index, newValue);

                    //Then
                    Assert.AreNotEqual(newValue, asyncP.GetValue(index));
                    Assert.AreEqual(newValue, asyncP.GetValue(index, testUser));
                    foreach (var user in users.Where(u => u != testUser))
                        Assert.AreNotEqual(newValue, asyncP.GetValue(index, user));
                }

                [Test]
                public void GivenFill_WhenSetValueIndex_ThenNewValue()
                {
                    //Given
                    FillGroupAndProperty();

                    //When
                    var newValue = 336;
                    int index = 0;
                    asyncP.SetValue(index, newValue);

                    //Then
                    Assert.AreEqual(newValue, asyncP.GetValue(index));
                    foreach (var user in UserNoGroupSync)
                        Assert.AreEqual(newValue, asyncP.GetValue(index, user));
                    foreach (var user in UserNoGroupSyncDesync)
                        Assert.AreEqual(newValue, asyncP.GetValue(index, user));
                    foreach (var user in users.Where(u => !UserNoGroupSync.Contains(u) && !UserNoGroupSyncDesync.Contains(u)))
                        Assert.AreNotEqual(newValue, asyncP.GetValue(index, user));
                }

                [Test]
                public void GivenFill_WhenSetValueIndexGroup_ThenNewValueGroup()
                {
                    //Given
                    FillGroupAndProperty();

                    //When
                    var newValue = 336;
                    int index = 0;
                    asyncP.SetValue(index, newValue, group);

                    //Then
                    Assert.AreEqual(newValue, asyncP.GetValue(index, group));
                    foreach (var user in UserGroupSync)
                        Assert.AreEqual(newValue, asyncP.GetValue(index, user));
                    foreach (var user in UserGroupSyncDesync)
                        Assert.AreEqual(newValue, asyncP.GetValue(index, user));
                    foreach (var user in users.Where(u => !UserGroupSync.Contains(u) && !UserGroupSyncDesync.Contains(u)))
                        Assert.AreNotEqual(newValue, asyncP.GetValue(index, user));
                }
            }

            public class AddRemove
            {
                const int casesPerList = 2;
                protected Mock<IUMI3DServer> serverServiceMock;

                protected float newFPSTracking = 18f;
                private List<UMI3DUser> users;

                ulong entity = 30uL;
                uint property = 40;
                List<int> value = new() { 42 };
                UMI3DAsyncListProperty<int> asyncP;

                Dictionary<UMI3DUser, List<int>> userExpectedValueMap;
                Dictionary<UMI3DUser, UMI3DGroupAsyncProperty> userGroupMap;
                Dictionary<UMI3DGroupAsyncProperty, List<int>> groupExpectedValueMap;

                UMI3DGroupAsyncProperty group;

                HashSet<UMI3DUser> UserGroupSync;
                HashSet<UMI3DUser> UserGroupAsync;
                HashSet<UMI3DUser> UserNoGroupSync;
                HashSet<UMI3DUser> UserNoGroupAsync;
                HashSet<UMI3DUser> UserGroupSyncDesync;
                HashSet<UMI3DUser> UserGroupAsyncDesync;
                HashSet<UMI3DUser> UserNoGroupSyncDesync;
                HashSet<UMI3DUser> UserNoGroupAsyncDesync;

                List<HashSet<UMI3DUser>> userListList;

                //Init Mock Umi3dServer and users
                [SetUp]
                public void Setup()
                {
                    UserGroupSync = new();
                    UserGroupAsync = new();
                    UserNoGroupSync = new();
                    UserNoGroupAsync = new();
                    UserGroupSyncDesync = new();
                    UserGroupAsyncDesync = new();
                    UserNoGroupSyncDesync = new();
                    UserNoGroupAsyncDesync = new();

                    userListList = new()
                {
                    UserGroupSync,
                    UserGroupAsync,
                    UserNoGroupSync,
                    UserNoGroupAsync,
                    UserGroupSyncDesync,
                    UserGroupAsyncDesync,
                    UserNoGroupSyncDesync,
                    UserNoGroupAsyncDesync,
                };


                    serverServiceMock = new();

                    var usersMock = new List<Mock<UMI3DUser>>();

                    int cases = userListList.Count;

                    for (int ui = 0; ui < cases * casesPerList; ui++)
                        usersMock.Add(new Mock<UMI3DUser>());

                    ulong i = 20000uL;
                    foreach (var userMock in usersMock)
                    {
                        userMock.Setup(x => x.Id()).Returns(i++);
                    }
                    users = usersMock.Select(x => x.Object).ToList();
                    serverServiceMock.Setup(x => x.Users()).Returns(users);

                    serverServiceMock.Setup(x => x.OnUserLeave).Returns(new UMI3DUserEvent());

                    entity = 30uL;
                    property = 40;
                    value = new() { 42 };
                    asyncP = new UMI3DAsyncListProperty<int>(serverServiceMock.Object, entity, property, value);
                }

                /// <summary>
                /// Fill property with a combination of user sync async in group not in group
                /// </summary>
                public void FillGroupAndProperty()
                {
                    int i = 0;
                    userListList.Do(l => l = new()).ForEach(l =>
                    {
                        for (int j = 0; j < casesPerList; j++)
                            l.Add(users[i++]);
                    });

                    userExpectedValueMap = new();
                    userGroupMap = new();
                    groupExpectedValueMap = new();

                    group = new();

                    //group in property
                    groupExpectedValueMap.Add(group, new List<int>() { 123456789 });

                    #region initUserMap
                    //users sync not in group 
                    //NOTHING TO DO

                    //users sync in group
                    foreach (var user in UserGroupSync)
                    {
                        userGroupMap.Add(user, group);
                    }

                    //users async not in group
                    foreach (var user in UserNoGroupAsync)
                        userExpectedValueMap.Add(user, new List<int>() { 8544555 });

                    //users async in group
                    foreach (var user in UserGroupAsync)
                    {
                        userExpectedValueMap.Add(user, new List<int>() { 8544555 });
                        userGroupMap.Add(user, group);
                    }

                    //users sync not in group desync
                    foreach (var user in UserGroupSyncDesync)
                    {
                        asyncP.DeSync(user, true);
                    }

                    //users sync in group desync
                    foreach (var user in UserGroupSyncDesync)
                    {
                        userGroupMap.Add(user, group);
                        asyncP.DeSync(user, true);
                    }

                    //users async not in group desync
                    foreach (var user in UserNoGroupAsyncDesync)
                    {
                        userExpectedValueMap.Add(user, new List<int>() { 8544555 });
                        asyncP.DeSync(user, true);
                    }

                    //users async in group desync
                    foreach (var user in UserGroupAsyncDesync)
                    {
                        userExpectedValueMap.Add(user, new List<int>() { 8544555 });
                        userGroupMap.Add(user, group);
                        asyncP.DeSync(user, true);
                    }
                    #endregion

                    //Set user async
                    foreach (var item in userExpectedValueMap)
                        asyncP.SetValue(item.Key, item.Value);

                    //Set user in group
                    foreach (var item in userGroupMap)
                        (item.Value).Add(item.Key);

                    //Set group value
                    foreach (var item in groupExpectedValueMap)
                        asyncP.SetValue(item.Value, item.Key);
                }

                [Test]
                public void GivenNothing_WhenAddValue_ThenDefaultValueLastIndex()
                {
                    //GIVEN

                    //WHEN
                    var newValue = 336;
                    var result = asyncP.Add(newValue) as SetEntityListAddProperty;

                    //Then
                    Assert.AreEqual(newValue, result.value);
                    Assert.AreEqual(asyncP.GetValue().Count - 1, result.index);
                    Assert.AreEqual(newValue, asyncP.GetValue()[result.index]);
                    foreach (var user in users)
                        Assert.AreEqual(value[result.index], asyncP.GetValue(result.index, user));

                }


                [Test]
                public void GivenNothing_WhenRemoveAtValue_ThenDefaultValueLastIndex()
                {
                    //GIVEN
                    var newValue = 336;
                    var add = asyncP.Add(newValue) as SetEntityListAddProperty;

                    //WHEN
                    var result = asyncP.RemoveAt(add.index) as SetEntityListRemoveProperty;

                    //Then
                    Assert.AreEqual(newValue, result.value);
                    Assert.AreEqual(add.index, result.index);
                    //Assert.AreEqual(newValue, asyncP.GetValue()[result.index]);
                    //foreach (var user in users)
                    //    Assert.AreEqual(value[result.index], asyncP.GetValue(result.index, user));

                }

                [Test]
                public void GivenNothing_WhenRemoveValue_ThenDefaultValueLastIndex()
                {
                    //GIVEN
                    var newValue = 336;
                    var add = asyncP.Add(newValue) as SetEntityListAddProperty;

                    //WHEN
                    var result = asyncP.Remove(newValue) as SetEntityListRemoveProperty;

                    //Then
                    Assert.AreEqual(newValue, result.value);
                    Assert.AreEqual(add.index, result.index);
                    //Assert.AreEqual(newValue, asyncP.GetValue()[result.index]);
                    //foreach (var user in users)
                    //    Assert.AreEqual(value[result.index], asyncP.GetValue(result.index, user));

                }

                [Test]
                public void GivenNothing_WhenRemoveValueNotInList_ThenNull()
                {
                    //GIVEN
                    var newValue = 336;

                    //WHEN
                    var result = asyncP.Remove(newValue) as SetEntityListRemoveProperty;

                    //Then
                    Assert.AreEqual(null, result);
                    //Assert.AreEqual(add.index, result.index);
                    //Assert.AreEqual(newValue, asyncP.GetValue()[result.index]);
                    //foreach (var user in users)
                    //    Assert.AreEqual(value[result.index], asyncP.GetValue(result.index, user));

                }
            }
        }
    }
}