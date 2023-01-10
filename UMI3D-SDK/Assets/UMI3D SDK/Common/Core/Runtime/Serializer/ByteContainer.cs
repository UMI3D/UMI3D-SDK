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

using BeardedManStudios.Forge.Networking.Frame;
using inetum.unityUtils;

namespace umi3d.common
{
    /// <summary>
    /// Bytes section in an array of bytes.
    /// </summary>
    public class ByteContainer
    {
        public ulong timeStep { get; private set; }

        /// <summary>
        /// The byte array that the container section belongs to.
        /// </summary>
        public byte[] bytes { get; private set; }

        /// <summary>
        /// Starting index of the section in the <see cref="bytes"/> array.
        /// </summary>
        public int position;

        /// <summary>
        /// Number of byte in the section.
        /// </summary>
        public int length;

        public ByteContainer(Binary frame) : this(frame.TimeStep, frame.StreamData.byteArr)
        {
        }

        public ByteContainer(ulong timeStep, byte[] bytes)
        {
            this.timeStep = timeStep;
            this.bytes = bytes;
            position = 0;
            length = bytes.Length;
        }

        public ByteContainer(ByteContainer container)
        {

            this.bytes = container.bytes;
            position = container.position;
            length = container.length;
            timeStep = container.timeStep;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{bytes.ToString<byte>()} [{position} : {length}]";
        }
    }
}