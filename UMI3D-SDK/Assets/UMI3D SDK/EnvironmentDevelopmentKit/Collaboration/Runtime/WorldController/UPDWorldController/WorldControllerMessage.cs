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

using umi3d.common;

namespace umi3d.worldController
{

    public static class UMI3DWorldControllerMessageKeys
    {
        public const uint UserJoin = 1;
        public const uint UserLeave = 1;
        public const uint RegisterUser = 1;
    }

    public class WorldControllerMessage
    {

        public virtual uint messageId => 0;

        protected virtual Bytable GetMessage()
        {
            return null;
        }

        public virtual Bytable ToBytable()
        {
            return UMI3DSerializer.Write(messageId) + GetMessage();
        }

        public Bytable ToBytableArray(params object[] parameters)
        {
            return ToBytable();
        }
    }
    public class WorldControllerUserJoinMessage : WorldControllerMessage
    {
        private readonly string uid;

        public WorldControllerUserJoinMessage(string uid)
        {
            this.uid = uid;
        }

        public override uint messageId => 1;

        protected override Bytable GetMessage()
        {
            return UMI3DSerializer.Write(uid);
        }
    }

    public class WorldControllerUserLeaveMessage : WorldControllerUserJoinMessage
    {
        public WorldControllerUserLeaveMessage(string uid) : base(uid)
        {

        }

        public override uint messageId => 2;
    }
}