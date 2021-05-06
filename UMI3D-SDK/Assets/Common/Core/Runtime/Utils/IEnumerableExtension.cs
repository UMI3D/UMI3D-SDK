/*
Copyright 2019 - 2021 Inetum

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

namespace umi3d.common
{
    public static class IEnumerableExtension
    {
        public static void ForEach<A>(this IEnumerable<A> source, Action<A> action)
        {
            if (action == null)
                throw new Exception("action should not be null");
            var it = source.GetEnumerator();
            while (it.MoveNext())
            {
                action.Invoke(it.Current);
            }
        }


        /// <summary>
        /// Copy a range of this IEnumerable to an other Ienumerable
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="target">The IEnumerable into which the data are copied</param>
        /// <param name="at">The starting position to copy the data in target</param>
        /// <param name="from">the index of the first value to copy from source into target</param>
        /// <param name="to">the index of the last value to copy from source into target</param>
        public static void CopyRangeTo<A>(this IEnumerable<A> source, A[] target, int at, int from, int to)
        {
            source.CopyRangeTo(target, ref at, from, to);
        }

        /// <summary>
        /// Copy a range of this IEnumerable to an other Ienumerable
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="target">The IEnumerable into which the data are copied</param>
        /// <param name="at">The starting position to copy the data in target</param>
        /// <param name="from">the index of the first value to copy from source into target</param>
        /// <param name="to">the index of the last value to copy from source into target</param>
        public static void CopyRangeTo<A>(this IEnumerable<A> source, A[] target,ref int at, int from, int to)
        {
            if (from >= to)
                throw new Exception($"'from' [{from}] should be inferior to 'to' [{to}]");
            if(at >= target.Length)
                throw new Exception($"'at' [{at}] should be inferior to target Length [{target.Length}]");
            if (target.Length - at <= to - from)
                throw new Exception($" target.Length - at [{target.Length} - {at}= {target.Length - at}] should be superior to to - from  [{to} - {from}= {to - from}]");
            var it = source.GetEnumerator();
            int j = 0;
            while (it.MoveNext() && j < from) j++;
            while (it.MoveNext() && j <= to)
            {
                target[at] = it.Current;
                at++;
                j++;
            }
        }

    }
}