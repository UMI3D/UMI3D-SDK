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
    public abstract class RangeDecoratorParameterInput<Value> : DecoratorParameterInput<Value>
    {
        public RangeDecoratorParameterInput(
            IParameterInputSystem<Value> decoratedInput,
            Value initialValue,
            Value min,
            Value max,
            Value increment
        )
        {
            this.decoratedInput = decoratedInput;
        }

        public Value initialValue {  get; private set; }
        public Value min {  get; private set; }
        public Value max {  get; private set; }
        public Value increment {  get; private set; }
    }

    public class FloatRangeDecoratorParameterInput : RangeDecoratorParameterInput<float>
    {
        public FloatRangeDecoratorParameterInput(
            IParameterInputSystem<float> decoratedInput, 
            float initialValue, 
            float min, 
            float max, 
            float increment
        ) : base(decoratedInput, initialValue, min, max, increment)
        {
        }

        public override void Perform(float value)
        {
            value = Mathf.Clamp(value, min, max);

            // TODO: check for increment.
            base.Perform(value);
        }
    }

    public class IntegerRangeDecoratorParameterInput : RangeDecoratorParameterInput<int>
    {
        public IntegerRangeDecoratorParameterInput(
            IParameterInputSystem<int> decoratedInput, 
            int initialValue, 
            int min, 
            int max, 
            int increment
        ) : base(decoratedInput, initialValue, min, max, increment)
        {
        }

        public override void Perform(int value)
        {
            value = Mathf.Clamp(value, min, max);
            // TODO: check for increment.
            base.Perform(value);
        }
    }
}