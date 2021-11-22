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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace inetum.unityUtils
{

    public class SizeSuffixer
    {
        string powerToSuffix(int power)
        {
            if (power <= 0) { return unit; }
            if (power <= Suffixes.Length)
            {
                return Suffixes[power - 1] + unitShort;
            }
            return "??" + unitShort;
        }

        int KBenBytes { get; }
        string unit { get; }
        string unitShort { get; }
        string[] Suffixes;

        public SizeSuffixer(string unit, string unitShort, int KiloToUnitRatio = 1000)
        {
            this.unit = unit;
            this.unitShort = unitShort;
            this.KBenBytes = KiloToUnitRatio;
            this.Suffixes = new string[] { "K", "M", "G", "T", "P", "E", "Z", "Y" };
        }

        public SizeSuffixer(string unit, string unitShort, string[] suffixes, int KiloToUnitRatio = 1000)
        {
            this.unit = unit;
            this.unitShort = unitShort;
            this.KBenBytes = KiloToUnitRatio;
            this.Suffixes = suffixes;
        }

        public string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, KBenBytes);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= KBenBytes;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                powerToSuffix(mag));
        }
    }
}