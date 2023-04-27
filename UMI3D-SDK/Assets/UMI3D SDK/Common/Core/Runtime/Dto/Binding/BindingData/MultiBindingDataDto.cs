/*
Copyright 2019 - 2023 Inetum

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

namespace umi3d.common
{
    [System.Serializable]
    public class MultiBindingDataDto : AbstractBindingDataDto
    {
        public MultiBindingDataDto()
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bindings">List Of all the simple bindings to apply</param>
        /// <param name="priority">level of priority of this binding [impact the order in which it is applied]</param>
        /// <param name="partialFit"> State if the binding can be applied partialy or not. A partial fit can happen in MultyBinding when it's not the binding with the highest priority.</param>
        public MultiBindingDataDto(AbstractSimpleBindingDataDto[] bindings, int priority, bool partialFit)
            : base(priority, partialFit)
        {
            this.Bindings = bindings;
        }

        /// <summary>
        /// List Of all the simple bindings to apply
        /// </summary>
        public AbstractSimpleBindingDataDto[] Bindings { get; private set; }
    }
}