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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace inetum.unityUtils
{
    public static class IEnumerableExtension
    {


        public static void Debug<A>(this IEnumerable<A> source, Func<A, string> action)
        {
            UnityEngine.Debug.Log(source.ToString<A>(action));
        }
        public static void Debug<A>(this IEnumerable<A> source)
        {
            UnityEngine.Debug.Log(source.ToString<A>());
        }

        /// <summary>
        /// return a string of element in an Ienumerable
        /// </summary>
        /// <param name="action">a method call to change a value to a string</param>
        /// <returns></returns>
        public static string ToString<A>(this IEnumerable<A> source, Func<A, string> action)
        {
            if (source.Count() == 0)
                return "[]";

            return ($"[{source.Select(v => action(v)).Aggregate((a, b) => $"{a};{b}")}]");
        }
        /// <summary>
        /// return a string of element in an Ienumerable
        /// </summary>
        /// <returns></returns>
        public static string ToString<A>(this IEnumerable<A> source)
        {
            if (source == null) return ($"[NULL]");
            if (source.Count() == 0) return ($"[]");
            return ($"[{source.Select(v => v.ToString()).Aggregate((a, b) => $"{a};{b}")}]");
        }

        /// <summary>
        /// An enumerable that skip the last element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source)
        {
            using (IEnumerator<T> e = source.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    for (T value = e.Current; e.MoveNext(); value = e.Current)
                    {
                        yield return value;
                    }
                }
            }
        }


        /// <summary>
        /// Do an action for each element when they are requested. 
        /// For an instant call of the action see <see cref="ForEach{A}(IEnumerable{A}, Action{A})"/>
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<A> Do<A>(this IEnumerable<A> source, Action<A> action)
        {
            if (action == null)
                throw new Exception("action should not be null");
            using (IEnumerator<A> it = source.GetEnumerator())
                while (it.MoveNext())
                {
                    action.Invoke(it.Current);
                    yield return it.Current;
                }
        }

        /// <summary>
        /// Do an action for each element. Could be use full for debug purpose.
        /// For an IEnimerable call see <see cref="Do{A}(IEnumerable{A}, Action{A})"/>
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEach<A>(this IEnumerable<A> source, Action<A> action)
        {
            if (action == null)
                throw new Exception("action should not be null");
            if(source != null)
                using (IEnumerator<A> it = source.GetEnumerator())
                    if(it != null)
                        while (it.MoveNext())
                        {
                            action.Invoke(it.Current);
                        }
        }

        /// <summary>
        /// Return the collection of DictionaryEntry in this IDictionary
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<DictionaryEntry> Entries(this IDictionary source)
        {
            IDictionaryEnumerator it = source.GetEnumerator();
            if (it != null)
                while (it.MoveNext())
                {
                    yield return (DictionaryEntry)it.Current;
                }
        }

        /// <summary>
        /// Set all value of an Array to a given value and return the Array.
        /// </summary>
        /// <param name="value">The given value</param>
        /// <returns>the source array.</returns>
        public static A[] Filled<A>(this A[] source, A value)
        {
            int length = source.Length;
            for (int i = 0; i < length; i++)
            {
                source[i] = value;
            }
            return source;
        }

        /// <summary>
        /// Set all value of an Array with a given function and return the Array.
        /// </summary>
        /// <param name="action">The given function</param>
        /// <returns>the source array.</returns>
        public static A[] Filled<A>(this A[] source, Func<int, A, A> action)
        {
            int length = source.Length;
            for (int i = 0; i < length; i++)
            {
                source[i] = action(i, source[i]); ;
            }
            return source;
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
        public static void CopyRangeTo<A>(this IEnumerable<A> source, A[] target, ref int at, int from, int to)
        {
            if (from >= to)
                throw new Exception($"'from' [{from}] should be inferior to 'to' [{to}]");
            if (at >= target.Length)
                throw new Exception($"'at' [{at}] should be inferior to target Length [{target.Length}]");
            if (target.Length - at <= to - from)
                throw new Exception($" target.Length - at [{target.Length} - {at}= {target.Length - at}] should be superior to to - from  [{to} - {from}= {to - from}]");
            using (IEnumerator<A> it = source.GetEnumerator())
            {
                int j = 0;
                while (j < from && it.MoveNext()) j++;
                while (j <= to && it.MoveNext())
                {
                    target[at] = it.Current;
                    at++;
                    j++;
                }
            }
        }

        /// <summary>
        /// Merge two ienumerable with a given Function. If one of the ienumerable is longuer the missing element of the other are set to the default value.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="to"></param>
        /// <param name="zipFunction"></param>
        /// <returns></returns>
        public static IEnumerable<C> ZipOverflow<A, B, C>(this IEnumerable<A> source, IEnumerable<B> to, Func<A, B, C> zipFunction)
        {
            using (IEnumerator<A> it = source.GetEnumerator())
            using (IEnumerator<B> toIt = to.GetEnumerator())
            {
                while (toIt.MoveNext() && it.MoveNext())
                {
                    yield return zipFunction(it.Current, toIt.Current);
                }
                while (toIt.MoveNext())
                {
                    yield return zipFunction(default(A), toIt.Current);
                }
                while (it.MoveNext())
                {
                    yield return zipFunction(it.Current, default(B));
                }
            }
        }

        /// <summary>
        /// Compare each element of two ienumerable.
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="source"></param>
        /// <param name="to"></param>
        /// <returns>a bool to state if all element are equal and a string displaying all element paired.</returns>
        public static (bool, string) Compare<A>(this IEnumerable<A> source, IEnumerable<A> to)
        {
            return source.ZipOverflow(to, (a, b) => (a, b)).Select(c => (c.a.Equals(c.b), $"({c.a}:{c.b})")).Aggregate((a, b) => (a.Item1 && b.Item1, $"{a.Item2};{b.Item2}"));
        }

        /// <summary>
        /// Convert a IEnumerable<KeyValuePair<T,K>> to a Dictionary<T,K>
        /// </summary>
        /// <returns></returns>
        public static Dictionary<T, K> ToDictionary<T, K>(this IEnumerable<KeyValuePair<T, K>> source)
        {
            return source.ToDictionary(k => k.Key, k => k.Value);
        }

    }
}