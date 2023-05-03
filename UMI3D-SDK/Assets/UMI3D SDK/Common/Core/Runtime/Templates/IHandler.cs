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

using System.Threading.Tasks;

namespace umi3d.common
{
    /// <summary>
    /// Responsability chain pattern node. 
    /// The Handler should Handle the value if possible or ask its successor to handle it.
    /// </summary>
    /// <typeparam name="T">type to handle</typeparam>
    /// <typeparam name="L">type to return when handled</typeparam>
    public interface IHandler<T,L>
    {
        /// <summary>
        /// Handle the value if possible or ask its successor to handle it.
        /// </summary>
        /// <param name="value">value to handle</param>
        /// <returns>result</returns>
        Task<L> Handle(T value);

        /// <summary>
        /// Set current successor handler.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>current successor</returns>
        IHandler<T, L> SetNext(IHandler<T,L> value);

        /// <summary>
        /// Get current successor handler.
        /// </summary>
        /// <returns>current successor</returns>
        IHandler<T, L> GetNext();
    }
}