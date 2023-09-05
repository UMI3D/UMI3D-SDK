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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace inetum.unityUtils
{
    public class CyclingEnumerator<T> : ICyclingEnumerator<T>
    {
        List<T> items;
        int currentIndex;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public T Current
        {
            get
            {
                if (items.Count == 0)
                {
                    return default(T);
                }
                else
                {
                    return items[currentIndex];
                }
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public CyclingEnumerator(IEnumerable<T> collection)
        {
            items = new List<T>(collection);
            currentIndex = 0;
        }

        public CyclingEnumerator()
        {
            items = new List<T>();
            currentIndex = -1;
        }

        /// <summary>
        /// Add an item to the next position.
        /// </summary>
        /// <param name="item"></param>
        public void AddNext(T item)
        {
            if (currentIndex >= 0)
            {
                items.Insert(currentIndex + 1, item);
            }
            else
            {
                currentIndex = 0;
                items.Insert(0, item);
            }
        }

        /// <summary>
        /// Add an item to the current position.
        /// </summary>
        /// <param name="item"></param>
        public void AddCurrent(T item)
        {
            currentIndex = currentIndex >= 0 ? currentIndex : 0;
            items.Insert(currentIndex, item);
        }

        /// <summary>
        /// Add an item before the current position.
        /// </summary>
        /// <param name="item"></param>
        public void AddPrevious(T item)
        {
            if (currentIndex >= 0)
            {
                items.Insert(currentIndex, item);
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
                items.Insert(0, item);
            }
        }

        /// <summary>
        /// Clear the collection and set the current index to -1.
        /// </summary>
        public void Dispose()
        {
            items.Clear();
            currentIndex = -1;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            switch ((currentIndex, items.Count))
            {
                case (int index, int eltCount) when eltCount == 0 || eltCount == 1:
                    return false;
                case (int index, int eltCount) when index == eltCount - 1:
                    currentIndex = 0;
                    return true;
                default:
                    currentIndex++;
                    return true;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public bool MovePrevious()
        {
            switch ((currentIndex, items.Count))
            {
                case (int index, int eltCount) when eltCount == 0 || eltCount == 1:
                    return false;
                case (int index, int eltCount) when index == 0:
                    currentIndex = eltCount - 1;
                    return true;
                default:
                    currentIndex--;
                    return true;
            }
        }

        /// <summary>
        /// Set the current index to 0;
        /// 
        /// <para>
        /// Unlike <see cref="Dispose"/> the collection is not cleared.
        /// </para>
        /// </summary>
        public void Reset()
        {
            currentIndex = 0;
        }
    }
}
