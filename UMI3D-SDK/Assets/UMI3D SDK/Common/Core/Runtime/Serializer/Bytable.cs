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
using System.Linq;

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
        public Func<byte[], int, int, (int, int)> function { get; protected set; }

        public Bytable(int size, Func<byte[], int, int, (int, int)> function)
        {
            this.size = size;
            this.function = function;
        }

        public Bytable()
        {
            this.size = 0;
            this.function = (by, i, bs) => (i, bs);
        }

        public byte[] ToBytes()
        {
            byte[] b = new byte[size];
            (int, int) c = function(b, 0, 0);
            if (c.Item2 != size) UMI3DLogger.LogError($"Size requested [{size}] and size used [{c.Item2}] have a different value. Last position is {c.Item1}. {b.ToString<byte>()}", scope);
            return b;
        }

        public byte[] ToBytes(byte[] bytes, int position = 0)
        {
            (int, int) c = function(bytes, position, 0);
            if (c.Item2 != size) UMI3DLogger.LogError($"Size requested [{size}] and size used [{c.Item2}] have a different value. Last position is {c.Item1}. {bytes.ToString<byte>()}", scope);
            return bytes;
        }

        public static BytableCollection operator +(Bytable a, Bytable b)
        {
            BytableCollection c;
            if (a == null)
            {
                if (b == null)
                    return null;
                if (b is BytableCollection bc)
                    return bc;
                c = new();
                c.Add(b);
                return c;
            }

            if (a is BytableCollection ac)
                c = ac;
            else
            {
                c = new();
                c.Add(a);
            }

            if (b != null)
                c.Add(b);

            return c;
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

        (int, int) Compute(byte[] by, int i, int bs)
        {
            foreach (var f in functions)
            {
                (i, bs) = f(by, i, bs);
            }
            return (i, bs);
        }

        public List<Func<byte[], int, int, (int, int)>> functions { get; private set; }

        public void Add(Bytable a)
        {
            if (a == null) return;

            this.functions.Add(a.function);
            this.size += a.size;
        }

        public static BytableCollection operator +(BytableCollection a, Bytable b)
        {
            if (a == null || b == null) return a;
            a.Add(b);
            return a;
        }
    }
}