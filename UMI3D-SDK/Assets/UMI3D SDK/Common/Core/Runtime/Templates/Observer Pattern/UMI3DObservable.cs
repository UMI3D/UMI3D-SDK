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

namespace umi3d.common
{
    public class UMI3DObservable : IUMI3DObservable<(Type observer, string purpose)>
    {
        /// <summary>
        /// Structural equality for (<see cref="Type"/>, string).
        /// </summary>
        struct TypeAndPurposeEqualityComparer : IEqualityComparer<(Type observer, string purpose)>
        {
            public bool Equals((Type observer, string purpose) x, (Type observer, string purpose) y)
            {
                return x.observer.Equals(y.observer) && x.purpose == y.purpose;
            }

            public int GetHashCode((Type, string) obj)
            {
                return obj.GetHashCode();
            }
        }

        public IDictionary<(Type observer, string purpose), int> observersAndPurposeToPriorities
        {
            get
            {
                return baseObservable.observersAndPurposeToPriorities;
            }
        }
        public IDictionary<int, List<((Type observer, string purpose) key, Action action)>> prioritiesToActions
        {
            get
            {
                return baseObservable.prioritiesToActions;
            }
        }
        public IUMI3DBaseObservable<(Type observer, string purpose), Action> baseObservable
        {
            get; protected set;
        }

        /// <summary>
        /// Create an <see cref="UMI3DObservable"/> with an <see cref="UMI3DBaseObservable{Key, Act}"/>.
        /// </summary>
        public UMI3DObservable()
        {
            baseObservable = new UMI3DBaseObservable<(Type observer, string purpose), Action>(
                observersAndPurposeToPriorities: new Dictionary<(Type observer, string purpose), int>(new TypeAndPurposeEqualityComparer()),
                prioritiesToActions: new SortedList<int, List<((Type observer, string purpose) key, Action action)>>()
            );
        }

        /// <summary>
        /// Create an an <see cref="UMI3DObservable"/> with an <see cref="IUMI3DBaseObservable{Key, Act}{Key, Act}"/>.
        /// </summary>
        /// <param name="baseObservable"></param>
        public UMI3DObservable(IUMI3DBaseObservable<(Type observer, string purpose), Action> baseObservable)
        {
            this.baseObservable = baseObservable;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICollection<int> priorities
        {
            get
            {
                try
                {
                    return baseObservable.priorities;
                }
                catch (Exception e)
                {
                    ObservableException.LogException(
                        $"Exception raised when trying to GetPriorities.",
                        inner: e,
                        ObservableException.ExceptionTypeEnum.GetPriorities
                    );
                    return null;
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public bool TryGetPriority((Type observer, string purpose) key, out int priority)
        {
            try
            {
                return baseObservable.TryGetPriority(key, out priority);
            }
            catch (Exception e)
            {
                ObservableException.LogException(
                    $"Exception raised when trying to get priority.",
                    inner: e,
                    ObservableException.ExceptionTypeEnum.GetPriority
                );
                priority = 0;
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public bool Subscribe(out IDisposable unsubscriber, (Type observer, string purpose) key, Action action, int priority = 0)
        {
            try
            {
                return baseObservable.Subscribe(out unsubscriber, key, action, priority);
            }
            catch (Exception e)
            {
                ObservableException.LogException(
                    $"Exception raised when trying to Subscribe {key}.",
                    inner: e,
                    ObservableException.ExceptionTypeEnum.Subscribe
                );
                unsubscriber = null;
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Unsubscribe((Type observer, string purpose) key)
        {
            try
            {
                return baseObservable.Unsubscribe(key);
            }
            catch (Exception e)
            {
                ObservableException.LogException(
                    $"Exception raised when trying to Unsubscribe {key}.",
                    inner: e,
                    ObservableException.ExceptionTypeEnum.Unsubscribe
                );
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Notify()
        {
            foreach (var priority in priorities.Reverse())
            {
                foreach (var item in prioritiesToActions[priority])
                {
                    try
                    {
                        item.action();
                    }
                    catch (Exception e)
                    {
                        ObservableException.LogException(
                            $"Exception raised when trying to Notify {item.key}.",
                            inner: e,
                            ObservableException.ExceptionTypeEnum.Unsubscribe
                        );
                    }
                }
            }
        }

        /// <summary>
        /// An exception class to deal with <see cref="UMI3DObservable"/> issues.
        /// </summary>
        [Serializable]
        public class ObservableException : Exception
        {
            static debug.UMI3DLogger logger = new debug.UMI3DLogger(mainTag: $"{nameof(ObservableException)}");

            public enum ExceptionTypeEnum
            {
                Unknown,
                GetPriorities,
                GetPriority,
                Subscribe,
                Unsubscribe,
                Notify
            }

            public ExceptionTypeEnum exceptionType;

            public ObservableException(string message, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}")
            {
                this.exceptionType = exceptionType;
            }
            public ObservableException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown) : base($"{exceptionType}: {message}", inner)
            {
                this.exceptionType = exceptionType;
            }

            public static void LogException(string message, Exception inner, ExceptionTypeEnum exceptionType = ExceptionTypeEnum.Unknown)
            {
                logger.Exception(null, new ObservableException(message, inner, exceptionType));
            }
        }
    }
}
