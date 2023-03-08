using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace umi3d.edk.userCapture
{
    public class UMI3DObjectField : ObjectField
    {
        public new class UxmlFactory : UxmlFactory<UMI3DObjectField, VisualElement.UxmlTraits> { }

        public void Init(Type type)
        {
            objectType= type;
            allowSceneObjects = true;
        }
    }
}

