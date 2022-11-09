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

using BeardedManStudios.Forge.Networking;

namespace umi3d.common.collaboration
{
    /// <summary>
    /// Exclusive channel for each data type.
    /// </summary>
    public enum DataChannelTypes
    {
        /// <summary>
        /// Channel used for signals during the connection process.
        /// </summary>
        Signaling = MessageGroupIds.START_OF_GENERIC_IDS + 1,

        /// <summary>
        /// Channel for user body tracking data.
        /// </summary>
        /// The first message describing the avatar of the user
        /// is also sent on this channel.
        Tracking = MessageGroupIds.START_OF_GENERIC_IDS + 2,

        /// <summary>
        /// Channel for Transactions and DispatchableRequests.
        /// </summary>
        Data = MessageGroupIds.START_OF_GENERIC_IDS + 3,

        /// <summary>
        /// Channel for video streaming.
        /// </summary>
        Video = MessageGroupIds.START_OF_GENERIC_IDS + 4,

        /// <summary>
        /// Channel for audio streaming through Voice over IP.
        /// </summary>
        VoIP = MessageGroupIds.VOIP
    }
}