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

using inetum.unityUtils.observation;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace inetum.unityUtils.ui.canvas
{
    public class CollectionViewController<Layout, View>
        where Layout : HorizontalOrVerticalLayoutGroup
        where View : MonoBehaviour
    {
        public ICollectionViewController<Layout, View> dataDelegate;

        Delegates<ICollectionViewControllerDelegate<View>> _delegates = new();
        public Delegates<ICollectionViewControllerDelegate<View>> delegates => _delegates;

        /// <summary>
        /// Pool a view, activate it and set its sibling position to <paramref name="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public View ActiveViewAt(int index, Action<View, int> action = null)
        {
            View view = dataDelegate?.viewPooling.Get();

            if (dataDelegate.container.childCount < index || index < 0)
            {
                UnityEngine.Debug.LogError($"Error: try to activate view but index is out of bounds [{index}].");
                return null;
            }

            view.transform.SetSiblingIndex(index);

            action?.Invoke(view, index);

            delegates.ForEach(@delegate =>
            {
                @delegate?.OnViewActivatedAt(view, index);

                return Flow.Continue;
            });

            return view;
        }

        public View ActiveViewAtLastIndex(Action<View, int> action = null)
        {
            return ActiveViewAt(dataDelegate.container.childCount, action);
        }

        public void DeactivateViewAt(int index)
        {
            if (dataDelegate.container.childCount <= index || index < 0)
            {
                UnityEngine.Debug.LogError($"Error: try to deactivate view but index is out of bounds [{index}].");
                return;
            }

            View view = dataDelegate?.container.GetChild(index).GetComponent<View>();

            if (view == null)
            {
                UnityEngine.Debug.LogError($"Error: try to deactivate view but there is no [{typeof(View)}] at index [{index}].");
                return;
            }

            dataDelegate?.viewPooling.Release(view);

            delegates.ForEach(@delegate =>
            {
                @delegate?.OnViewDeactivatedAt(view, index);

                return Flow.Continue;
            });
        }

        public void DeactivateView(View view)
        {
            DeactivateViewAt(view.transform.GetSiblingIndex());
        }

        public void DeactivateAllView()
        {
            for (int i = dataDelegate?.viewPooling.activeViews.Count - 1 ?? -1; i >= 0; i--)
            {
                View view = dataDelegate?.viewPooling.activeViews[i];
                DeactivateView(view);
            }
        }

        //int ActiveIndexToGlobalIndex(int index)
        //{
        //    int globalIndex = 0;
        //    while (index >= 0)
        //    {
        //        if (dataDelegate?.container.GetChild(globalIndex).gameObject.activeSelf ?? true)
        //        {
        //            index--;
        //        }
        //        globalIndex++;
        //    }

        //    return globalIndex;
        //}
    }
}