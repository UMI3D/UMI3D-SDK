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
using inetum.unityUtils.autoFixture;
using inetum.unityUtils.observation;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools;

public class NotificationFilterTests
{
    public class FilterByRefTests
    {
        public class ConstructorTest
        {
            [Test]
            public void GivenAFilterTypeAndNullParticipants_WhenInstantiate_ThenFieldAreSet()
            {
                FilterByRef filter1 = new FilterByRef(FilterType.AcceptOnly, null);
                FilterByRef filter2 = new FilterByRef(FilterType.AcceptAllExcept, null);

                Assert.AreEqual(filter1.filterType, FilterType.AcceptOnly);
                Assert.Null(filter1.participants);
                Assert.AreEqual(filter2.filterType, FilterType.AcceptAllExcept);
                Assert.Null(filter2.participants);
            }

            [Test]
            public void GivenAFilterTypeAndParticipants_WhenInstantiate_ThenFieldAreSet()
            {
                Fixture fixture = new();
                fixture.Customizations.Add(new RandomEnumGenerator());
                FilterType filterType = fixture.Create<FilterType>();

                FilterByRef filter1 = new FilterByRef(filterType, this);
                FilterByRef filter2 = new FilterByRef(filterType, this, 2);

                Assert.AreEqual(filter1.filterType, filterType);
                Assert.AreEqual(filter1.participants, new object[] { this });
                Assert.AreEqual(filter2.filterType, filterType);
                Assert.AreEqual(filter2.participants, new object[] { this, 2 });
            }
        }
        
        public class IsAcceptedTest
        {
            class FooClass { }

            [Test]
            public void GivenFilterAcceptOnlyWithNullParticipants_WhenCheckingIfParticipantIsAccepted_ThenTrue()
            {
                FilterByRef filter = new(FilterType.AcceptOnly, null);

                bool result1 = filter.IsAccepted(null);
                bool result2 = filter.IsAccepted(this);

                Assert.True(result1);
                Assert.True(result2);
            }

            [Test]
            public void GivenFilterAcceptOnly_WhenCheckingIfParticipantIsAcceptedWithNullOrAnotherParticipant_ThenFalse()
            {
                FilterByRef filter = new(FilterType.AcceptOnly, this);

                bool result1 = filter.IsAccepted(null);
                bool result2 = filter.IsAccepted(new FooClass());

                Assert.False(result1);
                Assert.False(result2);
            }

            [Test]
            public void GivenFilterAcceptOnly_WhenCheckingIfParticipantIsAccepted_ThenTrue()
            {
                FilterByRef filter = new(FilterType.AcceptOnly, this);

                bool result = filter.IsAccepted(this);

                Assert.True(result);
            }

            [Test]
            public void GivenFilterAcceptAllExceptWithNullParticipants_WhenCheckingIfParticipantIsAcceptedWithNullOrAnotherParticipant_ThenTrue()
            {
                FilterByRef filter = new(FilterType.AcceptAllExcept, null);

                bool result1 = filter.IsAccepted(null);
                bool result2 = filter.IsAccepted(this);

                Assert.True(result1);
                Assert.True(result2);
            }

            [Test]
            public void GivenFilterAcceptAllExcept_WhenCheckingIfParticipantIsAcceptedWithNullOrAnotherParticipant_ThenTrue()
            {
                FilterByRef filter = new(FilterType.AcceptAllExcept, this);

                bool result1 = filter.IsAccepted(null);
                bool result2 = filter.IsAccepted(new FooClass());

                Assert.True(result1);
                Assert.True(result2);
            }

            [Test]
            public void GivenFilterAcceptAllExcept_WhenCheckingIfParticipantIsAccepted_ThenFalse()
            {
                FilterByRef filter = new(FilterType.AcceptAllExcept, this);

                bool result = filter.IsAccepted(this);

                Assert.False(result);
            }
        }
    }

    public class FilterByConditionTests
    {
        public class ConstructorTest
        {
            [Test]
            public void GivenAFilterTypeAndNullFilter_WhenInstantiate_ThenFieldAreSet()
            {
                FilterByCondition filter1 = new FilterByCondition(FilterType.AcceptOnly, null);
                FilterByCondition filter2 = new FilterByCondition(FilterType.AcceptAllExcept, null);

                Assert.AreEqual(filter1.filterType, FilterType.AcceptOnly);
                Assert.Null(filter1.filter);
                Assert.AreEqual(filter2.filterType, FilterType.AcceptAllExcept);
                Assert.Null(filter2.filter);
            }

            [Test]
            public void GivenAFilterTypeAndFilter_WhenInstantiate_ThenFieldAreSet()
            {
                Fixture fixture = new();
                fixture.Customizations.Add(new RandomEnumGenerator());
                FilterType filterType = fixture.Create<FilterType>();
                Func<object, bool> _filter = participant => true;

                FilterByCondition filter = new FilterByCondition(filterType, _filter);

                Assert.AreEqual(filter.filterType, filterType);
                Assert.AreEqual(filter.filter, _filter);
            }
        }

        public class IsAcceptedTest
        {
            class FooClass { }

            [Test]
            public void GivenFilterAcceptOnlyWithNullFilter_WhenCheckingIfParticipantIsAccepted_ThenTrue()
            {
                FilterByCondition filter = new(FilterType.AcceptOnly, null);

                bool result1 = filter.IsAccepted(null);
                bool result2 = filter.IsAccepted(this);

                Assert.True(result1);
                Assert.True(result2);
            }

            [Test]
            public void GivenFilterThatThrow_WhenCheckingIfParticipantIsAccepted_ThenFalse()
            {
                FilterByCondition filter = new(FilterType.AcceptOnly, participant => throw new Exception());

                bool result1 = filter.IsAccepted(null);
                bool result2 = filter.IsAccepted(this);

                LogAssert.Expect(LogType.Error, "Error: a filter has thrown an exception.");
                LogAssert.Expect(LogType.Exception, "Exception: Exception of type 'System.Exception' was thrown.");
                LogAssert.Expect(LogType.Error, "Error: a filter has thrown an exception.");
                LogAssert.Expect(LogType.Exception, "Exception: Exception of type 'System.Exception' was thrown.");
                Assert.False(result1);
                Assert.False(result2);
            }

            [Test]
            public void GivenFilterAcceptOnly_WhenCheckingIfParticipantIsAcceptedWithNullOrAnotherParticipant_ThenFalse()
            {
                FilterByCondition filter = new(FilterType.AcceptOnly, participant => participant == this);

                bool result1 = filter.IsAccepted(null);
                bool result2 = filter.IsAccepted(new FooClass());

                Assert.False(result1);
                Assert.False(result2);
            }

            [Test]
            public void GivenFilterAcceptOnly_WhenCheckingIfParticipantIsAccepted_ThenTrue()
            {
                FilterByCondition filter = new(FilterType.AcceptOnly, participant => participant == this);

                bool result = filter.IsAccepted(this);

                Assert.True(result);
            }

            [Test]
            public void GivenFilterAcceptAllExceptWithNullFilter_WhenCheckingIfParticipantIsAccepted_ThenTrue()
            {
                FilterByCondition filter = new(FilterType.AcceptAllExcept, null);

                bool result1 = filter.IsAccepted(null);
                bool result2 = filter.IsAccepted(this);

                Assert.True(result1);
                Assert.True(result2);
            }

            [Test]
            public void GivenFilterAcceptAllExcept_WhenCheckingIfParticipantIsAcceptedWithNullOrAnotherParticipant_ThenTrue()
            {
                FilterByCondition filter = new(FilterType.AcceptAllExcept, participant => participant == this);

                bool result1 = filter.IsAccepted(null);
                bool result2 = filter.IsAccepted(new FooClass());

                Assert.True(result1);
                Assert.True(result2);
            }

            [Test]
            public void GivenFilterAcceptAllExcept_WhenCheckingIfParticipantIsAccepted_ThenFalse()
            {
                FilterByCondition filter = new(FilterType.AcceptAllExcept, participant => participant == this);

                bool result = filter.IsAccepted(this);

                Assert.False(result);
            }
        }
    }

    public class FilterGroupTests
    {
        public class ConstructorTest
        {
            [Test]
            public void GivenAFilterTypeAndNullFilter_WhenInstantiate_ThenFieldAreSet()
            {
                FilterGroup filter = new FilterGroup(null);

                Assert.Null(filter.filters);
            }

            [Test]
            public void GivenAFilterTypeAndFilter_WhenInstantiate_ThenFieldAreSet()
            {
                FilterByRef filter1 = new (FilterType.AcceptOnly, null);
                FilterByCondition filter2 = new (FilterType.AcceptOnly, null);

                FilterGroup filterGroup = new (filter1, filter2);

                Assert.AreEqual(filterGroup.filters.Length, 2);
                Assert.AreEqual(filterGroup.filters[0], filter1);
                Assert.AreEqual(filterGroup.filters[1], filter2);
            }
        }

        public class IsAcceptedTest
        {
            class FooClass { }

            [Test]
            public void GivenNoFilter_WhenCheckingIfParticipantIsAccepted_ThenTrue()
            {
                FilterGroup filterGroup = new(null);

                bool result1 = filterGroup.IsAccepted(null);
                bool result2 = filterGroup.IsAccepted(this);

                Assert.True(result1);
                Assert.True(result2);
            }

            [Test]
            public void GivenFilterAndNullFilter_WhenCheckingIfParticipantIsAccepted_ThenTrue()
            {
                FilterByRef filter1 = new(FilterType.AcceptOnly, this);
                FilterGroup filterGroup = new(filter1, null);

                bool result = filterGroup.IsAccepted(this);

                Assert.True(result);
            }

            [Test]
            public void GivenFilterAcceptOnly_WhenCheckingIfParticipantIsAcceptedWithNullOrAnotherParticipant_ThenFalse()
            {
                FilterByRef filter1 = new(FilterType.AcceptOnly, this);
                FilterByCondition filter2 = new(FilterType.AcceptOnly, participant => participant == this);
                FilterGroup filterGroup1 = new(filter1);
                FilterGroup filterGroup2 = new(filter2);

                bool result1 = filterGroup1.IsAccepted(null);
                bool result2 = filterGroup1.IsAccepted(new FooClass());

                bool result3 = filterGroup2.IsAccepted(null);
                bool result4 = filterGroup2.IsAccepted(new FooClass());

                Assert.False(result1);
                Assert.False(result2);
                Assert.False(result3);
                Assert.False(result4);
            }

            [Test]
            public void GivenFilterAcceptOnly_WhenCheckingIfParticipantIsAccepted_ThenTrue()
            {
                FilterByRef filter1 = new(FilterType.AcceptOnly, this);
                FilterByCondition filter2 = new(FilterType.AcceptOnly, participant => participant == this);
                FilterGroup filterGroup1 = new(filter1);
                FilterGroup filterGroup2 = new(filter2);

                bool result1 = filterGroup1.IsAccepted(this);
                bool result2 = filterGroup2.IsAccepted(this);

                Assert.True(result1);
                Assert.True(result2);
            }

            [Test]
            public void GivenFilterAcceptAllExcept_WhenCheckingIfParticipantIsAcceptedWithNullOrAnotherParticipant_ThenTrue()
            {
                FilterByRef filter1 = new(FilterType.AcceptAllExcept, this);
                FilterByCondition filter2 = new(FilterType.AcceptAllExcept, participant => participant == this);
                FilterGroup filterGroup1 = new(filter1);
                FilterGroup filterGroup2 = new(filter2);

                bool result1 = filterGroup1.IsAccepted(null);
                bool result2 = filterGroup1.IsAccepted(new FooClass());

                bool result3 = filterGroup2.IsAccepted(null);
                bool result4 = filterGroup2.IsAccepted(new FooClass());

                Assert.True(result1);
                Assert.True(result2);
                Assert.True(result3);
                Assert.True(result4);
            }

            [Test]
            public void GivenFilterAcceptAllExcept_WhenCheckingIfParticipantIsAccepted_ThenFalse()
            {
                FilterByRef filter1 = new(FilterType.AcceptAllExcept, this);
                FilterByCondition filter2 = new(FilterType.AcceptAllExcept, participant => participant == this);
                FilterGroup filterGroup1 = new(filter1);
                FilterGroup filterGroup2 = new(filter2);

                bool result1 = filterGroup1.IsAccepted(this);
                bool result2 = filterGroup2.IsAccepted(this);

                Assert.False(result1);
                Assert.False(result2);
            }

            [Test]
            public void GivenFiltersAcceptOnly_WhenCheckingIfParticipantIsAcceptedWithNullOrAnotherParticipant_ThenFalse()
            {
                FilterByRef filter1 = new(FilterType.AcceptOnly, this);
                FilterByCondition filter2 = new(FilterType.AcceptOnly, participant => participant == this);
                FilterGroup filterGroup = new(filter1, filter2);

                bool result1 = filterGroup.IsAccepted(null);
                bool result2 = filterGroup.IsAccepted(new FooClass());

                Assert.False(result1);
                Assert.False(result2);
            }

            [Test]
            public void GivenFiltersAcceptOnly_WhenCheckingIfParticipantIsAccepted_ThenTrue()
            {
                FilterByRef filter1 = new(FilterType.AcceptOnly, this);
                FilterByCondition filter2 = new(FilterType.AcceptOnly, participant => participant == this);
                FilterGroup filterGroup = new(filter1, filter2);

                bool result = filterGroup.IsAccepted(this);

                Assert.True(result);
            }

            [Test]
            public void GivenFiltersAcceptAllExcept_WhenCheckingIfParticipantIsAcceptedWithNullOrAnotherParticipant_ThenTrue()
            {
                FilterByRef filter1 = new(FilterType.AcceptAllExcept, this);
                FilterByCondition filter2 = new(FilterType.AcceptAllExcept, participant => participant == this);
                FilterGroup filterGroup = new(filter1, filter2);

                bool result1 = filterGroup.IsAccepted(null);
                bool result2 = filterGroup.IsAccepted(new FooClass());

                Assert.True(result1);
                Assert.True(result2);
            }

            [Test]
            public void GivenFiltersAcceptAllExcept_WhenCheckingIfParticipantIsAccepted_ThenFalse()
            {
                FilterByRef filter1 = new(FilterType.AcceptAllExcept, this);
                FilterByCondition filter2 = new(FilterType.AcceptAllExcept, participant => participant == this);
                FilterGroup filterGroup = new(filter1, filter2);

                bool result = filterGroup.IsAccepted(this);

                Assert.False(result);
            }

            [Test]
            public void GivenFiltersAcceptOnlyWithDifferentCondition_WhenCheckingIfParticipantIsAccepted_ThenFalse()
            {
                FilterByRef filter1 = new(FilterType.AcceptOnly, this);
                FilterByCondition filter2 = new(FilterType.AcceptOnly, participant => participant.GetType() == typeof(FooClass));
                FilterGroup filterGroup = new(filter1, filter2);

                bool result = filterGroup.IsAccepted(this);

                Assert.False(result);
            }

            [Test]
            public void GivenFiltersAcceptOnlyAndAcceptAllExceptWithDifferentCondition_WhenCheckingIfParticipantIsAccepted_ThenTrue()
            {
                FilterByRef filter1 = new(FilterType.AcceptOnly, this);
                FilterByCondition filter2 = new(FilterType.AcceptAllExcept, participant => participant.GetType() == typeof(FooClass));
                FilterGroup filterGroup = new(filter1, filter2);

                bool result = filterGroup.IsAccepted(this);

                Assert.True(result);
            }
        }
    }
}
