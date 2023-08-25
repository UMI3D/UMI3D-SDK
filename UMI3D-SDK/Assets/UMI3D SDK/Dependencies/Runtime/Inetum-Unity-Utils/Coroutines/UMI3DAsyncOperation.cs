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
using Unity.Jobs;
using UnityEngine;

namespace inetum.unityUtils
{
    /// <summary>
    /// A multi-threaded async operation based on <see cref="Coroutine"/> and <see cref="Task"/>.
    /// 
    /// <para>
    /// The operation is performed in another thread but the return events are performed in the main thread.
    /// </para>
    /// </summary>
    public class UMI3DAsyncOperation
    {
        /// <summary>
        /// The type of async operation.
        /// </summary>
        protected enum AsyncType
        {
            /// <summary>
            /// The type has not been defined.
            /// </summary>
            Unknown,
            /// <summary>
            /// The async operation has been started with a <see cref="System.Threading.Tasks"/>
            /// </summary>
            Task,
            /// <summary>
            /// The async operation has been started with a Job from <see cref="Unity.Jobs"/>.
            /// </summary>
            Job
        }

        /// <summary>
        /// Event raise when this <see cref="UMI3DAsyncOperation"/> has completed.
        /// </summary>
        public event Action<UMI3DAsyncOperation> completed;
        /// <summary>
        /// Event raise when this <see cref="UMI3DAsyncOperation"/> has been aborted.
        /// </summary>
        public event Action<UMI3DAsyncOperation> aborted;

        /// <summary>
        /// Whether or not this <see cref="UMI3DAsyncOperation"/> is done.
        /// </summary>
        public bool isDone { get; protected set; } = false;

        protected Coroutine coroutine;
        protected AsyncType type = AsyncType.Unknown;
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
        public UMI3DAsyncOperation Start(Action operation, CancellationToken? token = null)
        {
            if (coroutine != null)
            {
                return this;
            }

            type = AsyncType.Task;

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
        public UMI3DAsyncOperation Start(Func<Task> operation, CancellationToken? token = null)
        {
            if (coroutine != null)
            {
                return this;
            }

            type = AsyncType.Task;

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
        /// Start a job.
        /// 
        /// <para>
        /// Unlike task, a job cannot be cancelled.
        /// </para>
        /// </summary>
        /// <param name="operation">The operation to perform</param>
        /// <returns></returns>
        public UMI3DAsyncOperation Start<Job>(Job job)
            where Job: struct, IJob
        {
            if (coroutine != null)
            {
                return this;
            }

            type = AsyncType.Job;

            IEnumerator StartCoroutine()
            {
                var handle = job.Schedule();

                while (!handle.IsCompleted)
                {
                    yield return null;
                }

                handle.Complete();
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
            if (coroutine == null)
            {
                return;
            }

            if (type == AsyncType.Job)
            {
                // Job cannot be cancelled.
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
    public class UMI3DAsyncOperation<TResult> : UMI3DAsyncOperation
    {
        /// <summary>
        /// Event raised when this <see cref="UMI3DAsyncOperation"/> has completed.
        /// </summary>
        public event Action<UMI3DAsyncOperation, TResult> completedWithResult;

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
        public UMI3DAsyncOperation Start(Func<TResult> operation, CancellationToken? token = null)
        {
            if (coroutine != null)
            {
                return this;
            }

            type = AsyncType.Task;

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
        public UMI3DAsyncOperation Start(Func<Task<TResult>> operation, CancellationToken? token = null)
        {
            if (coroutine != null)
            {
                return this;
            }

            type = AsyncType.Task;

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
