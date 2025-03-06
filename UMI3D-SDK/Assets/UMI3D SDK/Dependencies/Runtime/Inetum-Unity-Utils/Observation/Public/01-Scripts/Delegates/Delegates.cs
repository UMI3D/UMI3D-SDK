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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace inetum.unityUtils.observation
{
    public enum Flow
    {
        /// <summary>
        /// Continue the foreach loop with the next element.
        /// </summary>
        Continue,
        /// <summary>
        /// Stop the foreach loop.
        /// </summary>
        Break,
    }

    /// <summary>
    /// A thread-safe collection of weak references to delegates. This class allows adding, removing, and iterating over delegates while ensuring thread safety.<br/>
    /// It uses weak references to avoid preventing the garbage collector from collecting the delegates.<br/>
    /// </summary>
    /// <typeparam name="DelegateInterface">The type of the delegate interface.</typeparam>
    public class Delegates<DelegateInterface> : IEnumerable, IEnumerable<WeakReference<DelegateInterface>>, IReadOnlyList<WeakReference<DelegateInterface>>
        where DelegateInterface : class
    {
        readonly object _lockObject = new object();

        List<WeakReference<DelegateInterface>> _weakDelegates;

        const int _PoolDefaultCapacity = 5;
        ObjectPool<List<DelegateInterface>> _pool;

        public int Count
        {
            get
            {
                lock (_lockObject)
                {
                    return _weakDelegates.Count;
                }
            }
        }

        public WeakReference<DelegateInterface> this[int index]
        {
            get
            {
                lock ( _lockObject)
                {
                    return _weakDelegates[index];
                }
            }
        }

        #region Initialization

        public Delegates() 
        {
            _weakDelegates = new();
            _pool = new(
                () => new(Count), 
                actionOnRelease: list => list.Clear(),
                defaultCapacity: _PoolDefaultCapacity
            );
        }

        public Delegates(List<DelegateInterface> delegates)
        {
            _weakDelegates = delegates.Select(@delegate => new WeakReference<DelegateInterface>(@delegate)).ToList();
            _pool = new(
                () => new(Count),
                actionOnRelease: list => list.Clear(),
                defaultCapacity: _PoolDefaultCapacity
            );
        }

        public Delegates(Delegates<DelegateInterface> delegates)
        {
            lock (delegates._lockObject)
            {
                _weakDelegates = delegates._weakDelegates;
                _pool = delegates._pool;
            }
        }

        #endregion

        #region Data management

        bool Contains(DelegateInterface @delegate)
        {
            return FindIndex(@delegate) >= 0;
        }

        int FindIndex(DelegateInterface @delegate)
        {
            lock (_lockObject)
            {
                return _weakDelegates.FindIndex(weakDelegate =>
                {
                    if (weakDelegate.TryGetTarget(out DelegateInterface _delegate))
                    {
                        return _delegate == @delegate;
                    }
                    else if (@delegate == null) { return true; }
                    else { return false; }
                });
            }
        }

        /// <summary>
        /// Adds a delegate to the collection if it is not already present.<br/>
        /// Will replace the first null reference of a weakReference. If there is no null reference 
        /// then add a new weakReference at the end of the collection.<br/>
        /// </summary>
        /// <param name="delegate">The delegate to add.</param>
        public void Add(DelegateInterface @delegate)
        {
            if (@delegate == null) { return; }

            if (Contains(@delegate)) { return; }

            // Find the index of a null reference in the collection.
            int index = FindIndex(null);
            if (index >= 0)
            {
                lock (_lockObject)
                {
                    // If a null reference is found, replace it with the new delegate.
                    _weakDelegates[index].SetTarget(@delegate);
                }
            }
            else
            {
                lock (_lockObject)
                {
                    // If no null reference is found, add the new delegate to the collection.
                    _weakDelegates.Add(new(@delegate));
                }
            }
           
        }

        /// <summary>
        /// Removes a delegate from the collection if it is present.<br/>
        /// </summary>
        /// <param name="delegate">The delegate to remove.</param>
        /// <returns>True if the delegate was removed; otherwise, false.</returns>
        public bool Remove(DelegateInterface @delegate)
        {
            if (@delegate == null) { return false; }

            // Find the index of the delegate in the collection
            int index = FindIndex(@delegate);

            // If the delegate is not found, return false
            if (index < 0) { return false; }

            lock (_lockObject)
            {
                // Remove the delegate from the collection
                _weakDelegates.RemoveAt(index);
            }
            return true;
        }

        /// <summary>
        /// Inserts a delegate at the specified index in the collection.<br/>
        /// If the delegate already exists, it is moved to the specified index.<br/>
        /// When inserting a delegate with an invalid index, then an ArgumentOutOfRangeException is thrown.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When inserting a delegate with an invalid index</exception>
        /// <param name="index">The zero-based index at which the delegate should be inserted.</param>
        /// <param name="delegate">The delegate to insert.</param>
        public void Insert(int index, DelegateInterface @delegate)
        {
            if (@delegate == null) { return; }

            int oldIndex = FindIndex(@delegate);

            if (oldIndex >= 0)
            {
                lock (_lockObject)
                {
                    // If the delegate is already in the collection, move it to the specified index.
                    WeakReference<DelegateInterface> weakDelegate = _weakDelegates[oldIndex];
                    _weakDelegates.RemoveAt(oldIndex);
                    _weakDelegates.Insert(index, weakDelegate);
                }
            }
            else
            {
                // Find the index of a null reference in the collection.
                int nullIndex = FindIndex(null);
                if (nullIndex >= 0)
                {
                    lock (_lockObject)
                    {
                        // If a null reference is found, replace it with the new delegate and move it to the specified index.
                        WeakReference<DelegateInterface> weakDelegate = _weakDelegates[nullIndex];
                        weakDelegate.SetTarget(@delegate);
                        _weakDelegates.RemoveAt(nullIndex);
                        _weakDelegates.Insert(index, weakDelegate);
                    }
                }
                else
                {
                    lock (_lockObject)
                    {
                        // If no null reference is found, insert the new delegate at the specified index.
                        _weakDelegates.Insert(index, new(@delegate));
                    }
                }
            }
        }

        /// <summary>
        /// Removes the delegate at the specified index in the collection.<br/>
        /// When removing a delegate with an invalid index, then an ArgumentOutOfRangeException is thrown.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When removing a delegate with an invalid index</exception>
        /// <param name="index">The zero-based index of the delegate to remove.</param>
        public void RemoveAt(int index)
        {
            lock (_lockObject)
            {
                _weakDelegates.RemoveAt(index);
            }
        }

        #endregion

        /// <summary>
        /// Executes the specified action for each delegate in the collection.<br/> 
        /// The action can control the flow by returning Flow.Continue or Flow.Break.<br/>
        /// If return Flow.Continue then the loop continue, else stop execution.<br/>
        /// <br/>
        /// <example>
        /// Example:
        /// <code>
        /// _delegates.ForEach(d =>
        /// {
        ///     // process.
        ///     
        ///     if (/*Condition to continue*/) {
        ///         return Flow.Continue;
        ///     }
        ///     else { return Flow.Break; }
        /// });
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="action">The action to execute for each delegate.</param>
        public void ForEach(Func<DelegateInterface, Flow> action)
        {
            List<DelegateInterface> _delegates;
            lock (_lockObject)
            {
                _delegates = _pool.Get();
                int index = 0;
                for (int i = 0; i < _weakDelegates.Count; i++)
                {
                    if (_weakDelegates[i].TryGetTarget(out DelegateInterface @delegate))
                    {
                        _delegates.Insert(index, @delegate);
                        index++;
                    }
                }
            }

            // Execute the action for each delegate in the list.
            foreach (DelegateInterface @delegate in _delegates)
            {
                try
                {
                    // Invoke the action and get the flow control value.
                    Flow? flow = action?.Invoke(@delegate);

                    // If the flow control value is Break, exit the loop.
                    if (flow.HasValue && flow.Value == Flow.Break)
                    {
                        break;
                    }
                }
                catch (NotImplementedException)
                {
                    UnityEngine.Debug.LogWarning($"[Delegates<{typeof(DelegateInterface).Name}>] Warning: a delegate raise a NotImplementedException");
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[Delegates<{typeof(DelegateInterface).Name}>] Error: a delegate raise an exception");
                    UnityEngine.Debug.LogException(e);
                }
            }

            lock (_lockObject)
            {
                _pool.Release(_delegates);
            }
        }

        /// <summary>
        /// Executes the specified action for each delegate in the collection and collects the return values.<br/>
        /// </summary>
        /// <typeparam name="ReturnType">The type of the return value from the action.</typeparam>
        /// <param name="action">The action to execute for each delegate.</param>
        /// <returns>A list of return values from the action.</returns>
        public List<ReturnType> ForEach<ReturnType>(Func<DelegateInterface, ReturnType> action)
        {
            List<DelegateInterface> _delegates;
            lock (_lockObject)
            {
                _delegates = _pool.Get();
                int index = 0;
                for (int i = 0; i < _weakDelegates.Count; i++)
                {
                    if (_weakDelegates[i].TryGetTarget(out DelegateInterface @delegate))
                    {
                        _delegates.Insert(index, @delegate);
                        index++;
                    }
                }
            }

            // Create a list to store the results.
            List<ReturnType> result = new(_delegates.Count);

            // Execute the action for each delegate in the list.
            foreach (DelegateInterface @delegate in _delegates)
            {
                try
                {
                    ReturnType _result;
                    if (action != null)
                    {
                        // Invoke the action and get the result.
                        _result = action.Invoke(@delegate);
                    }
                    else { _result = default; }  // If the action is null, use the default value.
                    result.Add(_result);
                    continue;
                }
                catch (NotImplementedException)
                {
                    UnityEngine.Debug.LogWarning($"[Delegates<{typeof(DelegateInterface).Name}>] Warning: a delegate raise a NotImplementedException");
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[Delegates<{typeof(DelegateInterface).Name}>] Error: a delegate raise an exception");
                    UnityEngine.Debug.LogException(e);
                }
                // Add the default value to the list if an exception is thrown.
                result.Add(default);
            }

            lock (_lockObject)
            {
                _pool.Release(_delegates);
            }

            return result;
        }

        public IEnumerator<WeakReference<DelegateInterface>> GetEnumerator()
        {
            lock (_lockObject)
            {
                return _weakDelegates.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_lockObject)
            {
                return _weakDelegates.GetEnumerator();
            }
        }
    }
}