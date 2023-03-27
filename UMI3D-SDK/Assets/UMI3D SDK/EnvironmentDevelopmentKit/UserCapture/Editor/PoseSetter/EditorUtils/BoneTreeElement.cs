using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.TreeViewExamples;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace inetum.unityUtils
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

