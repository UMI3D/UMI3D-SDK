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

using System.Collections.Generic;

namespace umi3d.common.collaboration
{
    public static class WebRtcChannels
    {
        /// <summary>
        /// DataChannels that will be open between peers.
        /// </summary>
        public static List<DataChannel> defaultPeerToPeerChannels = new List<DataChannel>()
        {
            new DataChannel("Reliable",true,DataChannelTypes.Data),
            new DataChannel("Unreliable",false,DataChannelTypes.Data),
            //new DataChannel("Tracking",false,DataType.Tracking), // TO COMMENT
        };

        /// <summary>
        /// DataChannels that will be open between a peer and the server.
        /// Server should also open PeerToPeerChannels DataChannels.
        /// </summary>
        /// <seealso cref="defaultPeerToPeerChannels"/>
        public static List<DataChannel> defaultPeerToServerChannels = new List<DataChannel>();
    }
}