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

using System;

namespace umi3d.common
{
    /// <summary>
    /// Serializable implementation of a 4x4 float matrix.
    /// </summary>
    [Serializable]
    public class Matrix4x4Dto : UMI3DDto
    {
        /// <summary>
        /// 1st column.
        /// </summary>
        public Vector4Dto c0 { get; set; } = new Vector4Dto();
        /// <summary>
        /// 2nd column.
        /// </summary>
        public Vector4Dto c1 { get; set; } = new Vector4Dto();
        /// <summary>
        /// 3rd column.
        /// </summary>
        public Vector4Dto c2 { get; set; } = new Vector4Dto();
        /// <summary>
        /// 4th column.
        /// </summary>
        public Vector4Dto c3 { get; set; } = new Vector4Dto();

        public float this[int i]
        {
            get { if (i >= 0 && i < 16) return this[i / 4, i - (4 * (i / 4))]; else throw new ArgumentOutOfRangeException("index should be between 0 and 15"); }
            set { if (i >= 0 && i < 16) this[i / 4, i - (4 * (i / 4))] = value; else throw new ArgumentOutOfRangeException("index should be between 0 and 15"); }
        }

        public float this[int r, int c]
        {
            get
            {
                if (r >= 0 && r < 4 && c >= 0 && c < 4)
                {
                    if (c == 0) return c0[r];
                    if (c == 1) return c1[r];
                    if (c == 2) return c2[r];
                    return c3[r];
                }
                else
                {
                    throw new ArgumentOutOfRangeException("ensure that 0 < r:" + r.ToString() + " < 4 and 0 < c:" + c.ToString() + " < 4");
                }
            }
            set
            {
                if (r >= 0 && r < 4 && c >= 0 && c < 4)
                {
                    if (c == 0) c0[r] = value;
                    else if (c == 1) c1[r] = value;
                    else if (c == 2) c2[r] = value;
                    else c3[r] = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("ensure that 0 < r:" + r.ToString() + " < 4 and 0 < c:" + c.ToString() + " < 4");
                }
            }
        }
    }
}