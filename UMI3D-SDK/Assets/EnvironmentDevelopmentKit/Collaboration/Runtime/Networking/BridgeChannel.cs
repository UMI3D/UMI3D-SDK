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
using umi3d.common.collaboration;

namespace umi3d.edk.collaboration
{
    public class BridgeChannel : DataChannel
    {
        public UMI3DCollaborationUser userA, userB;
        public BridgeChannel(UMI3DCollaborationUser userA, UMI3DCollaborationUser userB, string label, bool reliable, DataType type, Action onCreated = null, Action onOpen = null, Action onClose = null) : base(label, reliable, type, onCreated, onOpen, onClose)
        {
            this.userA = userA;
            this.userB = userB;
        }

        public bool Contain(UMI3DCollaborationUser userA, UMI3DCollaborationUser userB)
        {
            return (this.userA == userA && this.userB == userB) || (this.userA == userB && this.userB == userA);
        }

        public bool Equals(UMI3DCollaborationUser userA, UMI3DCollaborationUser userB,bool reliable,DataType type)
        {
            return Contain(userA,userB) && type == this.type && reliable == this.reliable;
        }

    }
}