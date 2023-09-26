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
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace inetum.unityUtils
{
    /// <summary>
    /// A multi-threaded async operation.
    /// 
    /// <para>
    /// The operation is performed in another thread but the return events are performed in the main thread.
    /// </para>
    /// </summary>
    public class AsyncOperation
    {
        /// <summary>
        /// Event raise when this <see cref="AsyncOperation"/> has completed.
        /// </summary>
        public event Action<AsyncOperation> completed;
        /// <summary>
        /// Event raise when this <see cref="AsyncOperation"/> has been aborted.
        /// </summary>
        public event Action<AsyncOperation> aborted;

        /// <summary>
        /// Whether or not this <see cref="AsyncOperation"/> is done.
        /// </summary>
        public bool isDone { get; protected set; } = false;

        protected Coroutine coroutine;
        protected void Completed()
        {
            completed?.Invoke(this);
        }
        protected void Aborted()
        {
            aborted?.Invoke(this);
        }

        /// <summary>
        /// Start an operation on another thread.
        /// </summary>
        /// <param name="operation">The operation to perform</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public AsyncOperation Start(Action operation, CancellationToken? token = null)
        {
            if (coroutine != null)
            {
                return this;
            }

            IEnumerator StartCoroutine()
            {
                Task task;
                if (token.HasValue)
                {
                    task = Task.Factory.StartNew(
                        operation,
                        token.Value
                    );
                }
                else
                {
                    task = Task.Factory.StartNew(
                        operation
                    );
                }
                
                while (!task.IsCompleted)
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        yield break;
                    }

                    yield return null;
                }

                isDone = true;
                Completed();
            }

            coroutine = CoroutineManager.Instance.AttachCoroutine(StartCoroutine());

            return this;
        }

        /// <summary>
        /// Start an async operation on another thread.
        /// </summary>
        /// <param name="operation">The operation to perform</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public AsyncOperation Start(Func<Task> operation, CancellationToken? token = null)
        {
            if (coroutine != null)
            {
                return this;
            }

            IEnumerator StartCoroutine()
            {
                Task task;
                if (token.HasValue)
                {
                    task = Task.Factory.StartNew(
                        operation,
                        token.Value
                    ).Unwrap();
                }
                else
                {
                    task = Task.Factory.StartNew(
                        operation
                    ).Unwrap();
                }

                while (!task.IsCompleted)
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        yield break;
                    }

                    yield return null;
                }

                isDone = true;
                Completed();
            }

            coroutine = CoroutineManager.Instance.AttachCoroutine(StartCoroutine());

            return this;
        }

        /// <summary>
        /// Stop the operation.
        /// </summary>
        public void Stop(CancellationTokenSource tokenSource = null)
        {
            if (coroutine != null)
            {
                return;
            }

            tokenSource?.Cancel();
            CoroutineManager.Instance.DetachCoroutine(coroutine);
            Aborted();
        }
    }

    /// <summary>
    /// A multi-threaded async operation.
    /// 
    /// <para>
    /// The operation is performed in another thread but the return events are performed in the main thread.
    /// </para>
    /// </summary>
    public class AsyncOperation<TResult> : AsyncOperation
    {
        /// <summary>
        /// Event raised when this <see cref="AsyncOperation"/> has completed.
        /// </summary>
        public event Action<AsyncOperation, TResult> completedWithResult;

        protected void OnCompletedWithResult(TResult result)
        {
            completedWithResult?.Invoke(this, result);
        }

        /// <summary>
        /// Start an operation on another thread.
        /// </summary>
        /// <param name="operation">The operation to perform</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public AsyncOperation Start(Func<TResult> operation, CancellationToken? token = null)
        {
            if (coroutine != null)
            {
                return this;
            }

            IEnumerator StartCoroutine()
            {
                Task<TResult> task;
                if (token.HasValue)
                {
                    task = Task.Factory.StartNew(
                        operation,
                        token.Value
                    );
                }
                else
                {
                    task = Task.Factory.StartNew(
                        operation
                    );
                }

                while (!task.IsCompleted)
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        yield break;
                    }

                    yield return null;
                }

                isDone = true;
                Completed();
                OnCompletedWithResult(task.Result);
            }

            coroutine = CoroutineManager.Instance.AttachCoroutine(StartCoroutine());

            return this;
        }

        /// <summary>
        /// Start an async operation on another thread.
        /// </summary>
        /// <param name="operation">The operation to perform</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public AsyncOperation Start(Func<Task<TResult>> operation, CancellationToken? token = null)
        {
            if (coroutine != null)
            {
                return this;
            }

            IEnumerator StartCoroutine()
            {
                Task<TResult> task;
                if (token.HasValue)
                {
                    task = Task.Factory.StartNew(
                        operation,
                        token.Value
                    ).Unwrap();
                }
                else
                {
                    task = Task.Factory.StartNew(
                        operation
                    ).Unwrap();
                }

                while (!task.IsCompleted)
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        yield break;
                    }

                    yield return null;
                }

                isDone = true;
                Completed();
                OnCompletedWithResult(task.Result);
            }

            coroutine = CoroutineManager.Instance.AttachCoroutine(StartCoroutine());

            return this;
        }
    }
}
