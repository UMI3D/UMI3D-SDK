/*
Copyright 2019 Gfi Informatique

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
using BrowserDesktop.Controller;
using System.Collections.Generic;
using UnityEngine;

namespace BrowserDesktop
{
    [CreateAssetMenu(fileName = "ActionIcon", menuName = "UMI3D/Action Icon", order = 3)]
    public class ActionIconInstance : ScriptableObject
    {
        [System.Serializable]
        public class ActionIconCouple
        {
            public KeyCode KeyCode;
            public ActionIcon prefab;
        }
        [System.Serializable]
        public class LActionIconCouple
        {
            public List<KeyCode> KeyCodes;
            public LActionIcon prefab;
        }

        public List<ActionIconCouple> CoupleCollection;
        public List<LActionIconCouple> ListCollection;
        public LActionIcon defaultIcon;

        GameObject get(KeyCode Action,Transform parent)
        {
            foreach(var couple in CoupleCollection)
            {
                if(couple.KeyCode == Action)
                {
                    return Instantiate(couple.prefab,parent).gameObject;
                }
            }
            LActionIcon icon = defaultIcon;
            foreach (var couple in ListCollection)
            {
                if (couple.KeyCodes.Contains(Action))
                {
                    icon = couple.prefab;
                    break;
                }
            }
            LActionIcon actionIcon = Instantiate(icon, parent);
            actionIcon.Set(Action);
            return actionIcon.gameObject;
        }

    }
}