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

using System;

namespace inetum.unityUtils
{
    public interface IRequestHub
    {
        #region Subscribe

        /// <summary>
        /// Bind <paramref name="supplier"/> to the association (<see cref="inetum.unityUtils.Request"/>, <paramref name="id"/>).<br/>
        /// <br/>
        /// If no such association exist then create a <see cref="inetum.unityUtils.Request"/> to bind with <paramref name="id"/>.<br/>
        /// Return the <see cref="inetum.unityUtils.Request"/> associated.
        /// </summary>
        /// <param name="supplier"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Request SubscribeAsSupplier(
            Object supplier,
            string id
        );

        /// <summary>
        /// Bind <paramref name="supplier"/> to the association (<see cref="inetum.unityUtils.Request"/>, id). Id is typeof(<typeparamref name="T"/>).FullName<br/>
        /// <br/>
        /// If no such association exist then create a <see cref="inetum.unityUtils.Request"/> to bind with id.<br/>
        /// Return the <see cref="inetum.unityUtils.Request"/> associated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public Request SubscribeAsSupplier<T>(Object supplier);

        /// <summary>
        /// Bind <paramref name="client"/> with the <see cref="Request"/> associated with <paramref name="id"/>.<br/>
        /// <br/>
        /// If no such association exist then create a <see cref="inetum.unityUtils.Request"/> to bind with <paramref name="id"/>.<br/>
        /// Return the <see cref="inetum.unityUtils.Request"/> associated.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Request SubscribeAsClient(
            Object client,
            string id
        );

        /// <summary>
        /// Bind <paramref name="client"/> with the <see cref="Request"/> associated with id. Id is typeof(<typeparamref name="T"/>).FullName<br/>
        /// <br/>
        /// If no such association exist then create a <see cref="inetum.unityUtils.Request"/> to bind with id.<br/>
        /// Return the <see cref="inetum.unityUtils.Request"/> associated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public Request SubscribeAsClient<T>(Object client);

        #endregion

        #region Unsubscibe

        /// <summary>
        /// Unbind <paramref name="supplier"/> with all its associated <see cref="Request"/>.<br/>
        /// <br/>
        /// Clear the (<see cref="Request"/>, id) association only if no (client, <see cref="Request"/>) association exist.
        /// </summary>
        /// <param name="supplier"></param>
        public void UnsubscribeAsSupplier(Object supplier);

        /// <summary>
        /// Unbind <paramref name="supplier"/> with its (<see cref="Request"/>, <paramref name="id"/>) association.<br/>
        /// <br/>
        /// Clear the (<see cref="Request"/>, <paramref name="id"/>) association only if no (client, <see cref="Request"/>) association exist.
        /// </summary>
        /// <param name="supplier"></param>
        /// <param name="id"></param>
        public void UnsubscribeAsSupplier(Object supplier, string id);

        /// <summary>
        /// Unbind <paramref name="supplier"/> with its (<see cref="Request"/>, id) association. Id is typeof(<typeparamref name="T"/>).FullName.<br/>
        /// <br/>
        /// Clear the (<see cref="Request"/>, id) association only if no (client, <see cref="Request"/>) association exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="supplier"></param>
        public void UnsubscribeAsSupplier<T>(Object supplier);

        #endregion
    }
}