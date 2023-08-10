/*
Copyright 2019 - 2021 Inetum

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

namespace umi3d.common.interaction
{
    /// <summary>
    /// Abstract base template for parameters DTO.
    /// </summary>
    /// <typeparam name="V"></typeparam>
    [System.Serializable]
    public abstract class AbstractParameterDto<V> : AbstractParameterDto
    {
        /// <summary>
        /// Parameter value.
        /// </summary>
        public V value { get; set; }

        /// <inheritdoc/>
        public override object GetValue()
        {
            return value;
        }

        public AbstractParameterDto() : base() { }
    }

    /// <summary>
    /// Parameter dto.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractParameterDto : AbstractInteractionDto
    {
        /// <summary>
        /// State if a parameter is private or not. For password or other.  
        /// </summary>
        public bool privateParameter { get; set; }

        /// <summary>
        /// State if a parameter is a displayer.
        /// If set to true the value will not be editable.
        /// </summary>
        public bool isDisplayer { get; set; }

        /// <summary>
        /// Mark a field with a tag to enable autocompletion 
        /// </summary>
        public string tag { get; set; }

        /// <summary>
        /// Retrieve the parameter value.
        /// </summary>
        /// <returns>Parameter value to retrieve</returns>
        public abstract object GetValue();
    }
}