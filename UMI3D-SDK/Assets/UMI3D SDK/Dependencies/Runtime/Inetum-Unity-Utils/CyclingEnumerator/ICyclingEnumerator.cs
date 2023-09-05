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
using System.Collections.Generic;

namespace inetum.unityUtils
{
    /// <summary>
    /// Interface for enumerator that can go backward and forward and keep going when it reach the end. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICyclingEnumerator<T> : IEnumerator<T>
    {
        /// <summary>
        /// Get the current element.
        /// </summary>
        new T Current { get; }

        /// <summary>
        /// Move the cursor to the previous value.
        /// 
        /// <para>
        /// Return false if the cursor stay at the same position.
        /// </para>
        /// </summary>
        /// <returns></returns>
        bool MovePrevious();

        /// <summary>
        /// Move the cursor to the next value.
        /// 
        /// <para>
        /// Return false if the cursor stay at the same position.
        /// </para>
        /// </summary>
        /// <returns></returns>
        new bool MoveNext();
    }
}
