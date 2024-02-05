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
        public int size { get; private set; }
        /// <summary>
        /// Function that take an array of byte to fill up, an index of a cell to write, and an already been used size. 
        /// This function shoudl return a couple ((position+newly reserved size),(current total size + newly reserved size)).
        /// </summary>
        public Func<byte[], int, int, (int, int)> function { get; private set; }

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

        public static Bytable operator +(Bytable a, Bytable b)
        {
            if (a == null) return b;
            if (b == null) return a;

            Func<byte[], int, int, (int, int)> f = (by, i, bs) =>
            {
                (i, bs) = a.function(by, i, bs);
                return b.function(by, i, bs);
            };
            return new Bytable(a.size + b.size, f);
        }

        public static Bytable operator +(Bytable a, IEnumerable<Bytable> b)
        {
            if (b == null || b.Count() == 0) return a;
            if (a == null) return b.Aggregate((c, d) => c + d);


            Bytable b2 = b.Aggregate((c, d) => c + d);

            Func<byte[], int, int, (int, int)> f = (by, i, bs) =>
            {
                (i, bs) = a.function(by, i, bs);
                return b2.function(by, i, bs);
            };
            return new Bytable(a.size + b2.size, f);
        }

        public static Bytable operator +(IEnumerable<Bytable> a, Bytable b)
        {
            if (a == null || a.Count() == 0) return b;
            if (b == null) return a.Aggregate((c, d) => c + d);

            Bytable a2 = a.Aggregate((c, d) => c + d);

            Func<byte[], int, int, (int, int)> f = (by, i, bs) =>
            {
                (i, bs) = a2.function(by, i, bs);
                return b.function(by, i, bs);
            };
            return new Bytable(a2.size + b.size, f);
        }
    }
}