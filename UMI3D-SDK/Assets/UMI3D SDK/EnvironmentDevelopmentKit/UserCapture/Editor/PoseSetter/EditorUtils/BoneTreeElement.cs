using System.Collections;
using System.Collections.Generic;
using UnityEditor.TreeViewExamples;
using UnityEngine;
using UnityEngine.UIElements;

namespace inetum.unityUtils
{
    public class BoneTreeElement : TreeElement
    {
        public BoneTreeElement(bool isRoot, bool isSeledcted) 
        { 
            this.isRoot = isRoot;
            this.isSelected = isSeledcted;  
        }

        public bool isRoot = false;
        public bool isSelected = false;
    }
}

