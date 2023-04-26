#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace inetum.unityUtils
{
    public class CustomObjectField : ObjectField
    {
        public new class UxmlFactory : UxmlFactory<CustomObjectField, VisualElement.UxmlTraits> { }

        public void Init(Type type)
        {
            objectType= type;
            allowSceneObjects = true;
        }
    }
}
#endif
