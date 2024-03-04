/*
Copyright 2019 - 2024 Inetum

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

namespace umi3d.common.interaction.form
{
    public enum StyleKeyword
    {
        //
        // Summary:
        //     Means that there's no keyword defined for that property.
        Undefined,
        //
        // Summary:
        //     Means that an inline style from IStyle has no value or keyword.
        Null,
        //
        // Summary:
        //     For style properties accepting auto.
        Auto,
        //
        // Summary:
        //     For style properties accepting none.
        None,
        //
        // Summary:
        //     The initial (or default) value of a style property.
        Initial
    }

}