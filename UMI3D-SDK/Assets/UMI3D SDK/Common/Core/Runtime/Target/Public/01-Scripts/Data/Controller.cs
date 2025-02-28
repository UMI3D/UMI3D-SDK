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
using UnityEngine;

namespace umi3d.common.core.target
{
    [Serializable]
    public struct Controller 
    {
        public readonly string name;
        public readonly int maxNumberOfItem;
        public readonly IReadOnlyList<SubController> subControllers;

        public Controller(string name, int maxNumberOfItem, params SubController[] subControllers)
        {
            this.name = name;
            this.maxNumberOfItem = maxNumberOfItem;
            this.subControllers = subControllers;
        }

        public static readonly Controller keyboardAndMouse = new("keyboard and mouse", 2, SubController.keyboard, SubController.mouse);
        public static readonly Controller screen = new("screen", 1);
        public static readonly Controller vrController = new("vr controller", 2, SubController.leftController, SubController.rightController);
        public static readonly Controller hand = new("hand", 2, SubController.leftHand, SubController.rightHand);
        public static readonly Controller eye = new("eye", 2, SubController.leftEye, SubController.rightEye);
    }

    [Serializable]
    public struct SubController
    {
        public readonly string name;

        public SubController(string name)
        {
            this.name = name;
        }

        public static readonly SubController keyboard = new SubController("keyboard");
        public static readonly SubController mouse = new SubController("mouse");
        public static readonly SubController leftController = new SubController("left controller");
        public static readonly SubController rightController = new SubController("right controller");
        public static readonly SubController leftHand = new SubController("left hand");
        public static readonly SubController rightHand = new SubController("right hand");
        public static readonly SubController leftEye = new SubController("left eye");
        public static readonly SubController rightEye = new SubController("right eye");
    }
}