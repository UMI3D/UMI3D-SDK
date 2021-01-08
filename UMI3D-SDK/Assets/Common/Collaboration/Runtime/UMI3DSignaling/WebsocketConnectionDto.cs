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

namespace umi3d.common.collaboration
{
    [System.Serializable]
    public class WebsocketConnectionDto : UMI3DDto
    {
        public string iP;
        public string postfix;
        public int port;
        public string websocketUrl;
        public string websocketReliableDataUrl;
        public string websocketUnreliableDataUrl;
        public string websocketReliableTrackingUrl;
        public string websocketUnreliableTrackingUrl;
        public string websocketAudio;
        public string websocketVideo;

        public IceServer[] iceServers;

        public WebsocketConnectionDto() : base() { }

    }
}
