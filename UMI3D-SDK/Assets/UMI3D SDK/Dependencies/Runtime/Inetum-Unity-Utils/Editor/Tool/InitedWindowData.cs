/*
Copyright 2019 - 2021 Inetum

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

#if UNITY_EDITOR

namespace inetum.unityUtils.editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class InitedWindowData : ScriptableObject
    {
        [SerializeField]
        private List<data> dataList = new();

        public void Debug()
        {
            dataList.Debug();
        }

        public data FromNameOrNew(string name, bool canReload, bool showMessage, bool lastShowMessageValue, bool waitForRebuild)
        {
            var d = FromName(name);
            if(d == null)
            {
                d = new(name, canReload, showMessage, lastShowMessageValue, waitForRebuild);
                dataList.Add(d);
            }
            return d;
        }

        public data FromName(string name)
        {
            return dataList.FirstOrDefault(d => d?.name == name);
        }

        [Serializable]
        public class data
        {
            public string name;
            public bool canReload, showMessage, lastShowMessageValue, waitForInit;

            public data(string name, bool canReload, bool showMessage, bool lastShowMessageValue, bool waitForRebuild)
            {
                this.name = name;
                this.canReload = canReload;
                this.showMessage = showMessage;
                this.lastShowMessageValue = lastShowMessageValue;
                this.waitForInit = waitForRebuild;
            }

            public override string ToString()
            {
                return $"{name} : {canReload}, {showMessage}, {lastShowMessageValue}, {waitForInit}";
            }

            public void Set(data data)
            {
                name = data.name;
                canReload = data.canReload;
                showMessage = data.showMessage;
                lastShowMessageValue = data.lastShowMessageValue;
                waitForInit = data.waitForInit;
            }
        }



    }





}
#endif