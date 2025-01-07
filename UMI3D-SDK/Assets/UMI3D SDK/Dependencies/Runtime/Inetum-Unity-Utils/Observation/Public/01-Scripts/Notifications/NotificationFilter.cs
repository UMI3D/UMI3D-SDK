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
using System.Collections.Generic;
using System.Linq;

namespace inetum.unityUtils.observation
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

        /// <summary>
        /// This method checks if a participant is accepted based on the filter type and the list of participants.<br/>
        /// <br/>
        /// <example>
        /// Given a filter of type AcceptOnly when checking if a participant is accepted then true if the participants are the same.<br/>
        /// <code>
        /// FilterByRef filter = new(FilterType.AcceptOnly, this);
        ///
        /// filter.IsAccepted(this); // return true.
        /// filter.IsAccepted(null); // return false.
        /// </code>
        /// Given a filter of type AcceptAllExcept when checking if a participant is accepted then true if the participants are different.<br/>
        /// <code>
        /// FilterByRef filter = new(FilterType.AcceptAllExcept, this);
        ///
        /// filter.IsAccepted(null); // return true.
        /// filter.IsAccepted(this); // return false
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="participant">The participant to check.</param>
        /// <returns>True if the participant is accepted based on the filter type and participants list; otherwise, false.</returns>
        public bool IsAccepted(Object participant)
        {
            if (participants == null) { return true; }

            switch (filterType)
            {
                case FilterType.AcceptOnly:
                    return participants.Contains(participant);
                case FilterType.AcceptAllExcept:
                    return !participants.Contains(participant);
                default:
                    UnityEngine.Debug.LogError($"Error: Unhandled case.");
                    return true;
            }
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

        /// <summary>
        /// This method checks if a participant is accepted based on the filter type and a provided filter function.<br/>
        /// <br/>
        /// <example>
        /// Given a filter that throws an exception when checking if a participant is accepted then false.<br/>
        /// <code>
        /// FilterByCondition filter = new(FilterType.AcceptOnly, participant => throw new Exception());
        ///
        /// filter.IsAccepted(this); // return false and log an error.
        /// </code>
        /// Given a filter of type AcceptOnly when checking if a participant is accepted then true if the participants are the same.<br/>
        /// <code>
        /// FilterByCondition filter = new(FilterType.AcceptOnly, participant => participant == this);
        ///
        /// filter.IsAccepted(this); // return true;
        /// filter.IsAccepted(null); // return false;
        /// </code>
        /// Given a filter of type AcceptAllExcept when checking if a participant is accepted then true.<br/>
        /// <code>
        /// FilterByCondition filter = new(FilterType.AcceptAllExcept, participant => participant == this);
        ///
        /// filter.IsAccepted(null); // return true.
        /// filter.IsAccepted(this); // return false.
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="participant">The participant to check.</param>
        /// <returns>True if the participant is accepted based on the filter type and filter function; otherwise, false.</returns>
        public bool IsAccepted(Object participant)
        {
            if (filter == null) { return true; }

            try
            {
                switch (filterType)
                {
                    case FilterType.AcceptOnly:
                        return filter.Invoke(participant);
                    case FilterType.AcceptAllExcept:
                        return !filter.Invoke(participant);
                    default:
                        UnityEngine.Debug.LogError($"Error: Unhandled case.");
                        return true;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error: a filter has thrown an exception.");
                UnityEngine.Debug.LogException(e);
                return false;
            }
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

        /// <summary>
        /// This method checks if a participant is accepted based on an array of filters.<br/>
        /// <br/>
        /// <example>
        /// Given filters when checking if a participant is accepted then true if the condition for all the filters are true.<br/>
        /// <code>
        /// FilterByRef filter1 = new(FilterType.AcceptOnly, this);
        /// FilterByCondition filter2 = new(FilterType.AcceptAllExcept, participant => participant.GetType() == typeof(FooClass));
        /// FilterGroup filterGroup = new(filter1, filter2);
        ///
        /// filterGroup.IsAccepted(this); // return true.
        /// filterGroup.IsAccepted(new FooClass()); // return false.
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="participant">The participant to check.</param>
        /// <returns>True if the participant is accepted by all the filters; otherwise, false.</returns>
        public bool IsAccepted(Object participant)
        {
            if (filters == null) { return true; }

            bool result = true;
            for (int i = 0; i < filters.Length; i++)
            {
                result &= filters[i]?.IsAccepted(participant) ?? true;
            }

            return result;
        }
    }
}
