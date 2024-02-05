/*
Copyright 2019 - 2022 Inetum

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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk
{
    //$"Load resources {(loadedResources / resourcesToLoad * 100).ToString("N2")} %"


    public class DebugProgress : MultiProgress
    {
        public DebugProgress(string state) : base(state)
        {
            OnCompleteUpdated += i => Debug();
            OnFailedUpdated += i => Debug();
            OnStatusUpdated += i => Debug();

        }
    }

    public class MultiProgress : Progress
    {
        /// <summary>
        /// Collection of processes progress.
        /// </summary>
        List<Progress> progressList;

        public MultiProgress(string state, IEnumerable<Progress> progresses) : this(state)
        {
            progressList.AddRange(progresses);
        }
        public MultiProgress(string state) : base(0, state)
        {
            progressList = new List<Progress>();
            OnCompleteUpdated += i => OnStatusUpdated?.Invoke(currentState);
            OnFailedUpdated += i => OnStatusUpdated?.Invoke(currentState);
        }

        bool ForcedFailed = false;
        bool ForcedCompleted = false;
        bool _started => ForcedCompleted || ForcedFailed;

        /// <inheritdoc/>
        public override bool started { get => progressList.Count > 0 ? progressList.Any(p => p.started) : _started; }

        /// <inheritdoc/>
        public override float completed
        {
            get
            {
                var c = progressList.Count;
                return c > 0 ? progressList.Aggregate(0f, (a, b) => a + (b.completedPercent / 100f)) : ForcedCompleted ? 1 : 0;
            }
        }

        /// <inheritdoc/>
        public override float failed
        {
            get
            {
                var c = progressList.Count;
                return c > 0 ? progressList.Aggregate(0f, (a, b) => a + (b.failedPercent / 100f)) : ForcedFailed ? 1 : 0;
            }
        }

        /// <inheritdoc/>
        public override float total
        {
            get
            {
                var c = progressList.Count;
                return c > 0 ? c : 1;
            }
        }

        /// <inheritdoc/>
        public override string currentState
        {
            get
            {
                if (progressList == null || progressList.Count < 0)
                    return base.currentState;

                string name = null;
                var p = progressList.LastOrDefault(pr => pr.started && pr.completedPercent < 100);

                if (p == null)
                    name = base.currentState;
                else if (p is MultiProgress)
                    name = p.currentState;
                else
                    name = p.currentState + " : " +( p.progressPercent).ToString("N2") + " %";
                return name;
            }
        }

        void NotifyUpdate(float completed, float failed, string state)
        {
            if (completed != this.completed)
                OnCompleteUpdated?.Invoke(this.completed);
            if (failed != this.failed)
                OnFailedUpdated?.Invoke(this.failed);
            if (state != this.currentState)
                OnStatusUpdated?.Invoke(this.currentState);
        }

        void GetStatus(out float completed, out float failed, out string state)
        {
            completed = this.completed;
            failed = this.failed;
            state = this.currentState;
        }

        /// <summary>
        /// Number of processes in this MultiProgress.
        /// </summary>
        public int Count => progressList.Count;

        /// <summary>
        /// Add a process progress.
        /// </summary>
        /// <param name="item"></param>
        public void Add(Progress item)
        {
            GetStatus(out float completed, out float failed, out string state);
            progressList.Add(item);
            item.OnCompleteUpdated += OnComplete;
            item.OnFailedUpdated += OnFailed;
            item.OnStatusUpdated += OnStatus;
            if(item.ResumeAfterFail == null)
                item.ResumeAfterFail = ResumeAfterFail;
            NotifyUpdate(completed, failed, state);
        }

        /// <summary>
        /// Add a range of process progresses.
        /// </summary>
        /// <param name="item"></param>
        public void AddRange(IEnumerable<Progress> items)
        {
            GetStatus(out float completed, out float failed, out string state);
            progressList.AddRange(items);
            foreach (Progress item in items)
            {
                item.OnCompleteUpdated += OnComplete;
                item.OnFailedUpdated += OnFailed;
                item.OnStatusUpdated += OnStatus;
                if (item.ResumeAfterFail == null)
                    item.ResumeAfterFail = ResumeAfterFail;
            }
            NotifyUpdate(completed, failed, state);
        }

        public void Clear()
        {
            GetStatus(out float completed, out float failed, out string state);
            foreach (Progress item in progressList)
            {
                item.OnCompleteUpdated -= OnComplete;
                item.OnFailedUpdated -= OnFailed;
                item.OnStatusUpdated -= OnStatus;
                if (item.ResumeAfterFail == ResumeAfterFail)
                    item.ResumeAfterFail = null;
            }
            progressList.Clear();
            NotifyUpdate(completed, failed, state);
        }


        public bool Contains(Progress item)
        {
            return progressList.Contains(item);
        }

        public void Insert(int index, Progress item)
        {
            GetStatus(out float completed, out float failed, out string state);
            progressList.Insert(index, item);
            item.OnCompleteUpdated += OnComplete;
            item.OnFailedUpdated += OnFailed;
            item.OnStatusUpdated += OnStatus;
            if (item.ResumeAfterFail == null)
                item.ResumeAfterFail = ResumeAfterFail;
            NotifyUpdate(completed, failed, state);
        }

        public bool Remove(Progress item)
        {
            GetStatus(out float completed, out float failed, out string state);
            var r = progressList.Remove(item);

            item.OnCompleteUpdated -= OnComplete;
            item.OnFailedUpdated -= OnFailed;
            item.OnStatusUpdated -= OnStatus;
            if (item.ResumeAfterFail == ResumeAfterFail)
                item.ResumeAfterFail = null;
            NotifyUpdate(completed, failed, state);
            return r;
        }

        public void RemoveRange(int index, int count)
        {
            GetStatus(out float completed, out float failed, out string state);
            foreach (Progress item in progressList)
            {
                item.OnCompleteUpdated -= OnComplete;
                item.OnFailedUpdated -= OnFailed;
                item.OnStatusUpdated -= OnStatus;
                if (item.ResumeAfterFail == ResumeAfterFail)
                    item.ResumeAfterFail = null;
            }
            progressList.RemoveRange(index, count);
            NotifyUpdate(completed, failed, state);
        }

        public void RemoveAt(int index)
        {
            GetStatus(out float completed, out float failed, out string state);
            var item = progressList[index];
            item.OnCompleteUpdated -= OnComplete;
            item.OnFailedUpdated -= OnFailed;
            item.OnStatusUpdated -= OnStatus;
            if (item.ResumeAfterFail == ResumeAfterFail)
                item.ResumeAfterFail = null;
            progressList.RemoveAt(index);
            NotifyUpdate(completed, failed, state);
        }

        void OnComplete(float c) { OnCompleteUpdated?.Invoke(completed); }
        void OnFailed(float f) { OnFailedUpdated?.Invoke(failed); }
        void OnStatus(string s) { OnStatusUpdated?.Invoke(currentState); }

        /// <summary>
        /// Indicate directly that all processes have failed.
        /// </summary>
        public override void SetAsFailed()
        {
            if (progressList.Count > 0)
                foreach (Progress item in progressList)
                {
                    item.SetAsFailed();
                }
            else
                ForcedFailed = true;
        }

        /// <summary>
        /// Indicate directly that all processes have been completed.
        /// </summary>
        public override void SetAsCompleted()
        {
            if (progressList.Count > 0)
                foreach (Progress item in progressList)
                {
                    item.SetAsCompleted();
                }
            else
                ForcedCompleted = true;
        }

        public override string ToString()
        {
            return CompactString();
        }

        /// <inheritdoc/>
        public override string CompactString()
        {
            if (progressList == null || progressList.Count <= 0)
                return base.ToString() + $"{_started} ";

            string sub = null;

            if (progressList != null && progressList.Count > 0)
                sub = ($"\n  SUB : {base.currentState} \n" + progressList.ToString<Progress>(progress => progress.CompactString() + "\n"));
            else
                sub = ($"\n  SUB {base.currentState} List Empty");
            return base.CompactString() +$"{_started} "+ sub;
        }
    }

    /// <summary>
    /// Progression representation of a process.
    /// </summary>
    public class Progress
    {
        public Progress(float total, string state = "")
        {
            this.total = total;
            completed = 0;
            failed = 0;
            currentState = state ?? "Null";
        }

        /// <summary>
        /// To connect to a popup to ask to resume or stop.
        /// </summary>
        public Func<Exception, Task<bool>> ResumeAfterFail = null;

        #region progressEvents

        /// <summary>
        /// Invoked when a task is completed.
        /// </summary>
        public Action<float> OnCompleteUpdated;

        /// <summary>
        /// Invoked when a task fails.
        /// </summary>
        public Action<float> OnFailedUpdated;

        /// <summary>
        /// Invoked when a task status change.
        /// </summary>
        public Action<string> OnStatusUpdated;

        #endregion progressEvents

        #region progressStats

        /// <summary>
        /// Has the process started?
        /// </summary>
        public virtual bool started { get; protected set; } = false;

        /// <summary>
        /// Fraction of the number of completed task over the total number of tasks.
        /// </summary>
        public virtual float completed { get; protected set; }

        /// <summary>
        /// Fraction of the number of failed task over the total number of tasks.
        /// </summary>
        public virtual float failed { get; protected set; }

        /// <summary>
        /// Total number of tasks.
        /// </summary>
        public virtual float total { get; protected set; }


        /// <summary>
        /// Name of the current state of the process.
        /// </summary>
        public virtual string currentState { get; protected set; }

        /// <summary>
        /// Progress of the process. Start: 0, End: 1.
        /// </summary>
        public virtual float progress { get => started ? completed + failed : 0; }


        /// <summary>
        /// Percentage of completed task over the total number of tasks.
        /// </summary>
        public virtual float completedPercent { get => !started || total == 0 ? 0 : (completed * 100f) / total; }

        /// <summary>
        /// Percentage of failed task over the total number of tasks.
        /// </summary>
        public virtual float failedPercent { get => !started || total == 0 ? 0 : (failed * 100f) / total; }

        /// <summary>
        /// Percentage of total processed tasks.
        /// </summary>
        /// Includes both completed and failed tasks.
        public virtual float progressPercent { get => !started || total == 0 ? 0 : (progress * 100f) / total; }
        
        #endregion progressStats

        /// <summary>
        /// Add a completed task.
        /// </summary>
        public virtual void AddComplete() { started = true; completed += 1; OnCompleteUpdated?.Invoke(completed); }

        /// <summary>
        /// Add a failed task.
        /// </summary>
        public virtual async Task<bool> AddFailed(Exception e) { started = true; failed += 1; OnFailedUpdated?.Invoke(failed); return await (ResumeAfterFail?.Invoke(e) ?? Task.FromResult( true)); }

        /// <summary>
        /// Set directly the current <paramref name="status"/> of the progress.
        /// </summary>
        /// <param name="status"></param>
        public virtual void SetStatus(string status) { currentState = status; OnStatusUpdated?.Invoke(currentState); }

        /// <summary>
        /// Indicate directly that the progress has failed.
        /// </summary>
        public virtual void SetAsFailed() { started = true; if (total == 0) total = 1; failed = total - completed; OnFailedUpdated?.Invoke(failed); }
        
        /// <summary>
        /// Indicate directly that the progress was completed.
        /// </summary>
        public virtual void SetAsCompleted() { started = true; if (total == 0) total = 1; completed = total - failed; OnCompleteUpdated?.Invoke(completed); }

        /// <summary>
        /// Set the number of task in this process.
        /// </summary>
        /// <param name="total"></param>
        public virtual void SetTotal(float total) { this.total = total; }

        /// <summary>
        /// Add one more task to be processed to the process.
        /// </summary>
        public virtual void AddTotal() { this.total += 1; }

        /// <summary>
        /// Add one more task to be processed to the process and set a specific <paramref name="status"/>.
        /// </summary>
        /// <param name="status"></param>
        public virtual void AddAndSetStatus(string status) { completed += 1; currentState = status; OnStatusUpdated?.Invoke(currentState); OnCompleteUpdated?.Invoke(completed); }


        public override string ToString()
        {
            return ($"Status : {(currentState)}  \n" +
            $"Progress : {(progressPercent).ToString("N2")} % | Complete {(completedPercent).ToString("N2")} % | Failed {(failedPercent).ToString("N2")} %\n" +
            $"Progress : {(progress)} / {total}               | Complete {(completed)} / {total}               | Failed {(failed)} / {total}   ");
        }

        /// <summary>
        /// Compact representation of the process progress.
        /// </summary>
        /// <returns></returns>
        public virtual string CompactString()
        {
            return ($"subState : {(currentState)} {(progressPercent).ToString("N2")} % {(progress)} / {total} | Complete {(completedPercent).ToString("N2")} % {(completed)} / {total} | Failed {(failedPercent).ToString("N2")} %  {(failed)} / {total} | {started}");
        }

        public void Debug()
        {
            UnityEngine.Debug.Log(ToString());
        }

    }
}