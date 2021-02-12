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
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace umi3d.common
{
    public class ConstStringEnumAttribute : PropertyAttribute
    {
        public Type type;
        public string[] options;

        public ConstStringEnumAttribute(Type type)
        {
            this.type = type;
            options = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                            .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                            .Select(fi => fi.GetValue(null) as string).ToArray();
        }
    }
}