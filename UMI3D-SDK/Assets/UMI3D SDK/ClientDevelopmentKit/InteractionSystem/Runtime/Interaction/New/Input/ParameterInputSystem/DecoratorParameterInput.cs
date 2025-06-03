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
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public abstract class DecoratorParameterInput<Value> : IParameterInputSystem<Value>
    {
        protected IParameterInputSystem<Value> decoratedInput;

        internal IParameterInputSystem<Value> GetRootDecoratedInput()
        {
            IParameterInputSystem<Value> rootInput = decoratedInput;

            while (rootInput is DecoratorParameterInput<Value> decorator)
            {
                rootInput = decorator.decoratedInput;
            }

            return rootInput;
        }

        public string id => decoratedInput.id;

        public event Action<Value> performed
        {
            add
            {
                decoratedInput.performed += value;
            }
            remove
            {
                decoratedInput.performed -= value;
            }
        }

        public virtual void Clear()
        {
            decoratedInput.Clear();
        }

        public virtual void Perform(Value value)
        {
            decoratedInput.Perform(value);
        }
    }
}