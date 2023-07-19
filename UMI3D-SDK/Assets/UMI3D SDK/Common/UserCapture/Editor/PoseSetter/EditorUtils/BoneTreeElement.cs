/*
Copy 2019 - 2023 Inetum

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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.TreeViewExamples;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace umi3d.common.userCapture
{
    public class BoneTreeElement : TreeElement
    {
        public UnityEvent<BoolChangeData> onIsRootChanged = new();
        public UnityEvent<BoolChangeData> onIsSelectedChanged = new();


        public BoneTreeElement(bool isRoot, bool isSeledcted) 
        { 
            this.isRoot = isRoot; 
            this.isSelected = isSeledcted;  
        }

        public bool isRoot = false;
        public bool isSelected = false;
    }

    public struct BoolChangeData
    {
        public BoneTreeElement boneTreeEleements;
        public bool boolValue;
        public int itemID;
    }
}

#endif