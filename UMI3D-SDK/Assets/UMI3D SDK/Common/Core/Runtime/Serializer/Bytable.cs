﻿/*
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

using inetum.unityUtils;
using System;
using System.Collections.Generic;

namespace umi3d.common
{
    /// <summary>
    /// Intermediary class used for serialization of objects into an array of bytes.
    /// </summary>
    /// it is possible to sum 2 bytables to obtain a larger bytable.
    public class Bytable
    {
        private const DebugScope scope = DebugScope.Common | DebugScope.Core | DebugScope.Bytes;

        /// <summary>
        /// Size of the array of byte to reserve.
        /// </summary>
        public int size { get; protected set; }
        /// <summary>
        /// Function that take an array of byte to fill up, an index of a cell to write, and an already been used size. 
        /// This function shoudl return a couple ((position+newly reserved size),(current total size + newly reserved size)).
        /// </summary>
        public Func<byte[], int, int, (int index, int computedSize)> function { get; protected set; }

        public Bytable(int size, Func<byte[], int, int, (int index, int computedSize)> function)
        {
            this.size = size;
            this.function = function;
        }

        public Bytable()
        {
            this.size = 0;
            this.function = (bytes, index, bytesSize) => (index, bytesSize);
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[size];
            (int index, int computedSize) = function(bytes, 0, 0);
            if (computedSize != size) 
                UMI3DLogger.LogError($"Size requested [{size}] and size used [{computedSize}] have a different value. Last position is {index}. {bytes.ToString<byte>()}", scope);
            return bytes;
        }

        public byte[] ToBytes(byte[] bytes, int position = 0)
        {
            (int index, int computedSize) = function(bytes, position, 0);
            if (computedSize != size) UMI3DLogger.LogError($"Size requested [{size}] and size used [{computedSize}] have a different value. Last position is {index}. {bytes.ToString<byte>()}", scope);
            return bytes;
        }

        public static Bytable operator +(Bytable a, Bytable b)
        {
            if (a == null) return b;
            if (b == null) return a;

            BytableCollection collection;
            if (a is BytableCollection ac)
                collection = ac;
            else
            {
                collection = new();
                collection.Add(a);
            }

            if (b != null)
                collection.Add(b);

            return collection;
        }
    }

    public class BytableCollection : Bytable
    {
        public BytableCollection()
        {
            this.functions = new();
            this.function = Compute;
            this.size = 0;
        }

        (int index, int computedSize) Compute(byte[] bytes, int index, int bytesSize)
        {
            foreach (var f in functions)
            {
                (index, bytesSize) = f(bytes, index, bytesSize);
            }
            return (index, bytesSize);
        }

        public List<Func<byte[], int, int, (int index, int computedSize)>> functions { get; private set; }

        public void Add(Bytable a)
        {
            if (a == null) return;

            this.functions.Add(a.function);
            this.size += a.size;
        }
    }
}