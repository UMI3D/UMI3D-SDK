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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Pool;

namespace inetum.unityUtils.ui.canvas
{
    public class ViewPooling<View> where View : class
    {
        ObjectPool<View> pool;
        List<View> _activeViews = new();

        public ReadOnlyCollection<View> activeViews => _activeViews.AsReadOnly();

        public ViewPooling(
            Func<View> createFunc, 
            Action<View> actionOnGet = null, 
            Action<View> actionOnRelease = null, 
            Action<View> actionOnDestroy = null, 
            bool collectionCheck = true, 
            int defaultCapacity = 10, 
            int maxSize = 10000
        )
        {
            pool = new ObjectPool<View>(
                createFunc,
                actionOnGet: view =>
                {
                    actionOnGet(view);
                },
                actionOnRelease: view =>
                {
                    actionOnRelease(view);
                },
                actionOnDestroy,
                collectionCheck,
                defaultCapacity,
                maxSize
            );
        }

        public View Get()
        {
            View view = pool.Get();
            _activeViews.Add(view);
            return view;
        }

        public void Release(View view)
        {
            if (!_activeViews.Remove(view))
            {
                UnityEngine.Debug.LogError($"Error: try to remove a view that is not active.");
                return;
            }

            pool.Release(view);
        }

        public void ReleaseAt(int index)
        {
            if (_activeViews.Count <= index)
            {
                UnityEngine.Debug.LogError($"Error: Try to remove a view but index is out of bounds [{index}]");
                return;
            }

            View view = _activeViews[index];
            Release(view);
        }

        public void ReleaseAll()
        {
            for (int i = _activeViews.Count - 1; i >= 0; i--)
            {
                ReleaseAt(i);
            }
        }
    }
}