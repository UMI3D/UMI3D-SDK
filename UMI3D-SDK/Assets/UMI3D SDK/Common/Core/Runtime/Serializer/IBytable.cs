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
    /// <summary>
    /// Object that could be converted to an array of bytes.
    /// </summary>
    public interface IBytable
    {
        /// <summary>
        /// Could the number of elements be deduced from a size parameter?
        /// </summary>
        /// <returns></returns>
        bool IsCountable();

        /// <summary>
        /// Convert the parameters to an array of <see cref="Bytable"/>.
        /// </summary>
        /// This method is used in the bytes networking system.
        /// <param name="parameters"></param>
        /// <returns></returns>
        Bytable ToBytableArray(params object[] parameters);
    }
}