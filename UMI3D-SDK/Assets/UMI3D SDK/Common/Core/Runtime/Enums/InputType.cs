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

namespace umi3d.common
{
    public class InputType : GenericEnumString
    {
        public InputType(string value) : base(value)
        {
        }

        public static InputType Checkbox => new InputType("checkbox");
        public static InputType Decimal => new InputType("decimal");
        public static InputType Integer => new InputType("integer");
        public static InputType Pin => new InputType("pin");
        public static InputType Pwd => new InputType("pwd");
        public static InputType Select => new InputType("select");
        public static InputType Text => new InputType("text");
    }
}