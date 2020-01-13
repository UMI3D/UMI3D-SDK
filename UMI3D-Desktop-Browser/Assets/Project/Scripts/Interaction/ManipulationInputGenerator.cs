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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;
using umi3d.cdk;
using BrowserDesktop.Cursor;

namespace BrowserDesktop.Interaction
{
    public class ManipulationInputGenerator : umi3d.Singleton<ManipulationInputGenerator>
    {
        
        public float strength;
        public FrameIndicator frameIndicator;
        public Transform manipulationCursor;

        public Sprite X;
        public Sprite Y;
        public Sprite Z;
        public Sprite XY;
        public Sprite XZ;
        public Sprite YZ;
        public Sprite RX;
        public Sprite RY;
        public Sprite RZ;

        /// <summary>
        /// instanciate and init Manipulation according to dofGroups and Inputs
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="manipulationInputs"></param>
        static public ManipulationInput Instanciate(AbstractController controller, CursorKeyInput Input, DofGroupEnum dofGroup, ref List<ManipulationInput> manipulationInputs)
        {
            return (Exist) ? Instance._Instanciate(controller, Input, dofGroup, ref manipulationInputs) : null;
        }

        ManipulationInput _Instanciate(AbstractController controller, CursorKeyInput input, DofGroupEnum dofGroup, ref List<ManipulationInput> manipulationInputs)
        {
            var inputInstance = gameObject.AddComponent<ManipulationInput>();
            inputInstance.activationButton = input;
            inputInstance.DofGroup = dofGroup;
            manipulationInputs.Add(inputInstance);
            inputInstance.Init(controller);
            inputInstance.strength = strength;
            inputInstance.frameIndicator = frameIndicator;
            inputInstance.Icon = FindSprite(dofGroup);
            inputInstance.manipulationCursor = manipulationCursor;
            return inputInstance;
        }


        Sprite FindSprite(DofGroupEnum dof)
        {
            switch (dof)
            {
                case DofGroupEnum.X:
                    return X;
                case DofGroupEnum.Y:
                    return Y;
                case DofGroupEnum.Z:
                    return Z;
                case DofGroupEnum.XY:
                    return XY;
                case DofGroupEnum.XZ:
                    return XZ;
                case DofGroupEnum.YZ:
                    return YZ;
                case DofGroupEnum.RX:
                    return RX;
                case DofGroupEnum.RY:
                    return RY;
                case DofGroupEnum.RZ:
                    return RZ;
            }
            return null;
        }
    }
}