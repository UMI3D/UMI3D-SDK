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

using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public sealed class Input 
    {
        internal Input(IInputSystem inputSystem)
        {
            this.inputSystem = inputSystem;
        }

        public bool isAvailable { get; internal set; } = true;

        public Controller controller { get; private set; }
        internal bool Associate(Controller controller)
        {
            if (this.controller != null) { return false; }

            this.controller = controller;
            return true;
        }
        internal void DissociateFromController()
        {
            this.controller = null;
        }

        public IInputSystem inputSystem { get; private set; }

        public IEventInputSystem eventInput
        {
            get
            {
                if (inputSystem is IEventInputSystem eventInput) { return eventInput; }
                else { return new NullObjectEventInput(); }
            }
        }

        public IParameterInputSystem<bool> booleanParameterInput
        {
            get
            {
                if (inputSystem is IParameterInputSystem<bool> @bool) { return @bool; }
                else { return new NullObjectParameterInput<bool>(); }
            }
        }

        public IParameterInputSystem<float> floatParameterInput
        {
            get
            {
                if (inputSystem is IParameterInputSystem<float> @float) { return @float; }
                else { return new NullObjectParameterInput<float>(); }
            }
        }

        public IParameterInputSystem<int> intParameterInput
        {
            get
            {
                if (inputSystem is IParameterInputSystem<int> @int) { return @int; }
                else { return new NullObjectParameterInput<int>(); }
            }
        }

        public IParameterInputSystem<Vector2> vector2ParameterInput
        {
            get
            {
                if (inputSystem is IParameterInputSystem<Vector2> vector2) { return vector2; }
                else { return new NullObjectParameterInput<Vector2>(); }
            }
        }

        public IParameterInputSystem<Vector3> vector3ParameterInput
        {
            get
            {
                if (inputSystem is IParameterInputSystem<Vector3> vector3) { return vector3; }
                else { return new NullObjectParameterInput<Vector3>(); }
            }
        }

        public IParameterInputSystem<Vector4> vector4ParameterInput
        {
            get
            {
                if (inputSystem is IParameterInputSystem<Vector4> vector4) { return vector4; }
                else { return new NullObjectParameterInput<Vector4>(); }
            }
        }

        public IParameterInputSystem<string> stringParameterInput
        {
            get
            {
                if (inputSystem is IParameterInputSystem<string> @string) { return @string; }
                else { return new NullObjectParameterInput<string>(); }
            }
        }

        public IParameterInputSystem<Color> colorParameterInput
        {
            get
            {
                if (inputSystem is IParameterInputSystem<Color> color) { return color; }
                else { return new NullObjectParameterInput<Color>(); }
            }
        }

        public IParameterInputSystem<LocalInfoRequestParameterValue> localInfoParameterInput
        {
            get
            {
                if (inputSystem is IParameterInputSystem<LocalInfoRequestParameterValue> localInfo) { return localInfo; }
                else { return new NullObjectParameterInput<LocalInfoRequestParameterValue>(); }
            }
        }

        public IUploadFileParameterInputSystem uploadFileParameterInput
        {
            get
            {
                if (inputSystem is IUploadFileParameterInputSystem uploadFile) { return uploadFile; }
                else { return new NullObjectUploadFileParameterInput(); }
            }
        }

        internal void Decorate(IInputSystem decorator)
        {
            inputSystem = decorator;
        }

        internal void UnDecorate<Value>()
        {
            if (inputSystem is DecoratorParameterInput<Value> decorator)
            {
                inputSystem = decorator.GetRootDecoratedInput();
            }
        }

        public string debugDescription
        {
            get
            {
                string description = "";

                description += $"---- Input ----\n";
                description += $"{controller?.id ?? "No controller"}, {isAvailable}, {inputSystem.id}\n";
                description += "\n";

                return description;
            }
        }
    }
}