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
using UnityEngine;

namespace inetum.unityUtils
{
    /// <summary>
    /// Implementation of a BaseObservable.
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Act"></typeparam>
    public class UMI3DBaseObservable<Key, Act> : IBaseUMI3DObservable<Key, Act>
    {
        Dictionary<Key, int> observersAndPurposeToPriorities;
        SortedList<int, List<(Key key, Act action)>> prioritiesToActions;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        Dictionary<Key, int> IBaseUMI3DObservable<Key, Act>.observersAndPurposeToPriorities
        {
            get
            {
                return observersAndPurposeToPriorities;
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        SortedList<int, List<(Key key, Act action)>> IBaseUMI3DObservable<Key, Act>.prioritiesToActions
        {
            get
            {
                return prioritiesToActions;
            }
        }

        public UMI3DBaseObservable(Dictionary<Key, int> observersAndPurposeToPriorities, SortedList<int, List<(Key key, Act action)>> prioritiesToActions)
        {
            this.observersAndPurposeToPriorities = observersAndPurposeToPriorities;
            this.prioritiesToActions = prioritiesToActions;
        }
    }
}
