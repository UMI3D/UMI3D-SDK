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

namespace inetum.unityUtils
{
    /// <summary>
    /// A container that notifies observers when its items change.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class NotifyingList<T> : NotifyingVariable<List<T>>
    {
        public umi3d.debug.UMI3DLogger logger = new(mainTag: $"{nameof(NotifyingList<T>)}");

        /// <summary>
        /// Event raised when an item [secondArg] is inserted at [firstArg].
        /// </summary>
        public event Action<int, T> itemInserted;
        /// <summary>
        /// Event raised when an item [secondArg] is removed from [firstArg].
        /// </summary>
        public event Action<int, T> itemRemoved;
        /// <summary>
        /// Event raised when the list is cleared.
        /// </summary>
        public event Action cleared;

        public NotifyingList()
        {
            field = new();
        }

        public T this[int index]
        {
            get
            {
                return field[index];
            }
            set
            {
                Insert(index, value);
            }
        }

        /// <summary>
        /// Return the number of items in this list.
        /// </summary>
        public int Count
        {
            get
            {
                return field?.Count ?? 0;
            }
        }

        /// <summary>
        /// Add an item.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            Insert(Count, item);
        }
        /// <summary>
        /// Insert an item at <paramref name="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <exception cref="NotifyingListException"></exception>
        public void Insert(int index, T item)
        {
            if (field == null)
            {
                throw new NotifyingListException("List null");
            }
            else if (index < 0)
            {
                throw new NotifyingListException($"Index: <{index}> < 0.");
            }
            else if (index > Count)
            {
                throw new NotifyingListException($"Index: <{index}> > Count.");
            }

            field.Insert(index, item);
            itemInserted?.Invoke(index, item);
        }

        /// <summary>
        /// Remove the last item of the list.
        /// </summary>
        /// <returns></returns>
        public bool RemoveLast()
        {
            if (Count == 0)
            {
                return false;
            }

            try
            {
                RemoveAt(Count - 1);
            }
            catch (Exception e)
            {
                logger.Warning($"{nameof(Remove)}", $"{e.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Remove an item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            if (Count == 0)
            {
                return false;
            }

            var index = field.IndexOf(item);
            try
            {
                RemoveAt(index);
            }
            catch (Exception e)
            {
                logger.Warning($"{nameof(Remove)}", $"{e.Message}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Remove the item at <paramref name="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="NotifyingListException"></exception>
        public void RemoveAt(int index)
        {
            if (field == null)
            {
                throw new NotifyingListException("List null");
            }

            try
            {
                T item = field[index];
                field.RemoveAt(index);
                itemRemoved?.Invoke(index, item);
            }
            catch (Exception e)
            {
                throw new NotifyingListException("Remove item failed", e);
            }
        }

        /// <summary>
        /// Clear the list.
        /// </summary>
        public void Clear()
        {
            field?.Clear();
            cleared?.Invoke();
        }

        public class NotifyingListException: Exception
        {
            public NotifyingListException(string message) : base(message)
            {

            }

            public NotifyingListException(string message, Exception innerException) : base(message, innerException)
            {

            }
        }
    }
}
