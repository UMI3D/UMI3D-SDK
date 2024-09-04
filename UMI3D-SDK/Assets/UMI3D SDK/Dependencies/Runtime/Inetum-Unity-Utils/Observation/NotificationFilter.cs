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

using System;
using System.Collections.Generic;
using System.Linq;

namespace inetum.unityUtils
{
    public enum FilterType
    {
        /// <summary>
        /// Only accept notification from or only send notification to those participants.
        /// </summary>
        AcceptOnly,
        /// <summary>
        /// Accept notification from or send notification to everyone but those participants.
        /// </summary>
        AcceptAllExcept
    }

    /// <summary>
    /// Filter a notificaiton.
    /// </summary>
    public interface INotificationFilter
    {
        /// <summary>
        /// Whether this participant can send or receive a notification from or to the target.
        /// </summary>
        /// <param name="participant"></param>
        /// <returns></returns>
        bool IsAccepted(Object participant);
    }

    /// <summary>
    /// Filter the notifications by comparing publishers or subscribers by reference.
    /// </summary>
    public class FilterByRef: INotificationFilter
    {
        public FilterType filterType;
        public Object[] participants;

        public FilterByRef(FilterType filterType, params Object[] participants)
        {
            this.filterType = filterType;
            this.participants = participants;
        }

        public bool IsAccepted(Object participant)
        {
            if (participants == null)
            {
                return true;
            }

            if (participants.Contains(participant))
            {
                return filterType == FilterType.AcceptOnly;
            }

            return filterType == FilterType.AcceptAllExcept;
        }
    }

    /// <summary>
    /// Filter the notifications using a condition on the publishers or subscribers.
    /// </summary>
    public class FilterByCondition : INotificationFilter
    {
        public FilterType filterType;
        public Func<Object, bool> filter;

        public FilterByCondition(FilterType filterType, Func<Object, bool> filter)
        {
            this.filterType = filterType;
            this.filter = filter;
        }

        public bool IsAccepted(Object participant)
        {
            return filter?.Invoke(participant) ?? true;
        }
    }

    /// <summary>
    /// Group some filters.
    /// </summary>
    public class FilterGroup : INotificationFilter
    {
        public INotificationFilter[] filters;

        public FilterGroup(params INotificationFilter[] filters)
        {
            this.filters = filters;
        }

        public bool IsAccepted(Object participant)
        {
            bool result = true;

            for (int i = 0; i < filters.Length; i++)
            {
                result &= filters[i].IsAccepted(participant);
            }

            return result;
        }
    }
}
