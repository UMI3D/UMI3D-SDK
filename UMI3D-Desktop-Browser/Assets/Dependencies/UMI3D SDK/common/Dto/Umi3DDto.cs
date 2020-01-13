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
using System;

namespace umi3d.common
{
    /// <summary>
    /// Base classe of all data tranfer object
    /// </summary>
    [Serializable]
    public class UMI3DDto
    {
        /// <summary>
        /// The Type as a string of the data
        /// </summary>
        public string Dtype;

        public UMI3DDto()
        {
            Dtype = this.GetType().ToString();
        }


        [System.AttributeUsage(System.AttributeTargets.Field)]
        public class CustomProperty : System.Attribute
        {
            public enum Type { DEFAULT, DATE }

            public string name = null;
            public Type type;

            public CustomProperty(string name)
            {
                this.name = name;
            }

            public CustomProperty(Type type)
            {
                this.type = type;
            }

            public CustomProperty(string name, Type type)
            {
                this.name = name;
                this.type = type;
            }
        }

    }

}